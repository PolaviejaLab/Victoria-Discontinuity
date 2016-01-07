/**
 * Main state machine controlling the flow of the experiment.
 *  This state machine is responsible for loading trial descriptions,
 *  running the appropriate trials in order and saving the results
 *  back to the data file.
 *
 * Logic pertaining to the flow of a trial should be placed in the
 *  trial state machine.
 */
using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;


public enum ExperimentEvents {
	TrialFinished,
};


public enum ExperimentStates {
	Trial,
	Finished,
};


public class ExperimentController: StateMachine<ExperimentStates, ExperimentEvents> 
{
	public HandController handController;
	public HandSwitcher handSwitcher;
	public TrialController trialController;

	public TableLights tableLights;
	
	private TrialList trialList;
	public string protocolFile;

	public string outputDirectory = "Results";
	public string participantName = "Anonymous";

	public int trialCounter;


	public void Start() 
    {
		logger.OpenLog(GetLogFilename());

		// If the path is relative, add current directory
		if(!Path.IsPathRooted(protocolFile)) {
            // The default location is the current directory
            protocolFile = Path.GetFullPath(protocolFile);
		}

        // Load protocol
		trialList = new TrialList(protocolFile);
		WriteLog("Loaded " + trialList.Count () + " trials");

		this.StartMachine();
	}
	
	
	public void HandleEvent(ExperimentEvents ev) 
    {
		WriteLog("Event " + ev.ToString());

		if(!IsStarted())
			return;
		
		switch(GetState()) {

		case ExperimentStates.Trial:
			if (ev == ExperimentEvents.TrialFinished) {
				//ChangeState (ExperimentStates.Threat);
			}
			break;
		}
	}
	
    
	public void Update() 
    {
		if(!IsStarted())
			return;
	}
	
    
	protected override void OnEnter(ExperimentStates oldState)
    {
		switch(GetState()) {
    		case ExperimentStates.Trial:
    			if (!trialList.HasMore())
    				ChangeState(ExperimentStates.Finished);
    			
    			trialCounter++;
    			StartTrial();
    			break;
    
    		case ExperimentStates.Finished:
    			SaveTrialResult();
    			StopMachine();
    			WriteLog("No more trials, stopping machine");
    			break;
		}
	}


	protected override void OnExit(ExperimentStates newState) 
    {
		switch(GetState()) {
    		case ExperimentStates.Trial:
   				break;                
		}
	}


    /**
     * Parse trial details and pass them to the trial controller
     */
    private void PrepareTrial(Dictionary<string, string> trial, TrialController trialController)
    {
        // Determine which hand to use for given gapsize
        if(trial["GapStatus"] == "Inactive")
            trialController.hand = 1;
        else if(trial["GapStatus"] == "Active")
            trialController.hand = 0;
        else {
            WriteLog("Invalid GapSize in protocol");
            trialController.hand = -1;
        }
        
        WriteLog("Gap: " + trial["GapStatus"]);
        
        // Get offset
        float offset;
        float.TryParse(trial["Offset"], out offset);
        trialController.offset = offset / 100.0f;
        
        WriteLog("Offset: " + offset);
        
        // Determine the number of waves per each trial
        int wavesRequired;
        int.TryParse(trial["WavesRequired"], out wavesRequired);
        trialController.wavesRequired = wavesRequired;
    }    


	/**
	 * Start the next trial
	 */
	private void StartTrial() {
		// Do not start if already running
		if(trialController.IsStarted())
			return;

		WriteLog("Starting trial");

		// Get next trial from list and setup trialController
		Dictionary<string, string> trial = trialList.Pop();
        PrepareTrial(trial, trialController);
        
		handController.StartRecording(GetLEAPFilename(trialCounter));

		// Turn table lights on
		tableLights.isOn = true;

		trialController.initialState = TrialStates.AccomodationTime;
		trialController.StartMachine();
	}


	private string GetLEAPFilename(int trial)
    {
		return outputDirectory + "\\" + DateTime.UtcNow.ToString("yyyy-MM-dd hh.mm ") + participantName + " Trial " + trial + ".csv";
	}


	private string GetLogFilename()
    {
		return outputDirectory + "\\" + DateTime.UtcNow.ToString("yyyy-MM-dd hh.mm ") + participantName + ".log";
	}


	private string GetResultsFilename()
	{
		return outputDirectory + "\\" + DateTime.UtcNow.ToString("yyyy-MM-dd hh.mm ") + participantName + ".csv";
	}

	
	/**
	 * Appends the result of the previous trial to the datafile
	 */
	private void SaveTrialResult() 
    {
		handController.StopRecording();

		StreamWriter writer = new StreamWriter(GetResultsFilename(), true);
		
		// Append result of trial to data file
		writer.Write(trialCounter);
		writer.Write(", ");
		
		if(trialController.hand == 0)
			writer.Write("With gap, ");
		else if(trialController.hand == 1)
			writer.Write("Without gap, ");
		else
			writer.Write("Unknown, ");
				
		writer.Write(trialController.offset);
		writer.Write(", ");
		writer.Write(trialController.waveCounter);
		writer.Write(", ");
		writer.Write(trialController.correctWaves);
		writer.Write(", ");
		writer.Write(trialController.incorrectWaves);
		writer.Write(", ");
		writer.Write(trialController.lateWaves);
		writer.Write(", ");
		writer.WriteLine();
		
		writer.Close();
	}
}
