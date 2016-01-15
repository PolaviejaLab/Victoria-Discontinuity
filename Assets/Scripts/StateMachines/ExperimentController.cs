﻿/**
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

    private string outputDirectory;

    private int participantNumber;
    private string participantName;

	public int trialCounter;

    public int randomProtocol;


	public void Start() {
        string[] dir = Directory.GetDirectories("Results");

        participantNumber = 1;
        participantName = "Participant";

        for (int i = 0; i < dir.Length; i++){
            outputDirectory = "Results/RandomProtocolTesting" + participantNumber.ToString();
            if (!Directory.Exists(outputDirectory)){
                Directory.CreateDirectory(outputDirectory);
                break;
            } else {
                participantNumber = participantNumber + 1;
            }
        }


		logger.OpenLog(GetLogFilename());

        string[] dirProtocol = Directory.GetFiles("Protocol/Parkinson");

        randomProtocol = UnityEngine.Random.Range(0, dirProtocol.Length);
        protocolFile = dirProtocol [randomProtocol]; 

//		// If the path is relative, add current directory
//		if(!Path.IsPathRooted(protocolFile)) {
//            // The default location is the current directory
//            protocolFile = Path.GetFullPath(protocolFile);
//		}

        // Load protocol
		trialList = new TrialList(protocolFile);
		WriteLog("Loaded " + trialList.Count () + " trials");

        // When the trial controller is stopped, invoke an event
        trialController.Stopped += 
            (sender, e) => 
                { HandleEvent(ExperimentEvents.TrialFinished); };


		this.StartMachine();
	}
	
	
	public void HandleEvent(ExperimentEvents ev) {
		WriteLog("Event " + ev.ToString());

		if(!IsStarted())
			return;
		
		switch(GetState()) {

		case ExperimentStates.Trial:
			if (ev == ExperimentEvents.TrialFinished)
				ChangeState(ExperimentStates.Trial);
			break;
		}
	}
	
    
	public void Update() {
		if(!IsStarted())
			return;
	}
	
    
	protected override void OnEnter(ExperimentStates oldState) {
		switch(GetState()) {
    		case ExperimentStates.Trial:
    			if(trialList.HasMore()) {
                    trialCounter++;
                    StartTrial();
                } else {    			
                    ChangeState(ExperimentStates.Finished);
                }
    			break;
    
    		case ExperimentStates.Finished:
    			SaveTrialResult();
    			StopMachine();
    			WriteLog("No more trials, stopping machine");
    			break;
		}
	}


	protected override void OnExit(ExperimentStates newState) {
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
        
        // Noise level
        if(trial.ContainsKey("NoiseLevel")) {
            float noiseLevel;
            float.TryParse(trial["NoiseLevel"], out noiseLevel);
            trialController.noiseLevel = noiseLevel;
            WriteLog("NoiseLevel: " + noiseLevel);
        } else {
            trialController.noiseLevel = 0;
        }
        
        // Knife
        if (trial.ContainsKey("KnifePresent"))
        {
            if (trial ["KnifePresent"].ToLower() == "true")
                trialController.knifePresent = true;
            else if (trial ["KnifePresent"].ToLower() == "false")
                trialController.knifePresent = false;
            else
                throw new Exception("Invalid value in trial list for field KnifePresent");
        }

        // Knife Offset
        if(trial.ContainsKey("KnifeOffset")){
            float knifeOffset;
            float.TryParse(trial["KnifeOffset"], out knifeOffset);
            Vector3 knifeVector = new Vector3(0, 0, knifeOffset);
            trialController.knifeOffset = knifeVector;

            WriteLog("Knife Offset: " + knifeVector);
        }
    }    


	/**
	 * Start the next trial
	 */
	private void StartTrial() 
    {
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
