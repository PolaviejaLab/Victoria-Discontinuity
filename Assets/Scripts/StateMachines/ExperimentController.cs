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
using System.Net;
using System.Collections;
using System.Collections.Generic;


public enum ExperimentEvents {
    ProtocolLoaded,
	TrialFinished,
    ExperimentFinished, 
};


public enum ExperimentStates {
    WaitingForFeedback,
    Start,
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

    private bool trialEmpty;


    public void Start() {
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
            case ExperimentStates.WaitingForFeedback:
                break;

            case ExperimentStates.Start:
                if (ev == ExperimentEvents.ProtocolLoaded)
                    ChangeState(ExperimentStates.Trial);
                break;

		
            case ExperimentStates.Trial:
                if (ev == ExperimentEvents.TrialFinished)
                    SaveTrialResult();
                    ChangeState(ExperimentStates.Trial);
                break;
		}
	}
	
    
	public void Update() {
		if(!IsStarted())
			return;

        // Change the gender of the hand
        if (Input.GetKeyDown(KeyCode.F)){
            handSwitcher.useMale = false;
            WriteLog("Gender changed to female");
        }
        else if (Input.GetKeyDown(KeyCode.M)){
            handSwitcher.useMale = true;
            WriteLog("Gender changed to male");
        }

        if (Input.GetKeyDown(KeyCode.Keypad0)){
            WriteLog("Tracking Failed");
        }

        switch (GetState()){
            case ExperimentStates.WaitingForFeedback:
                if (Input.GetKeyDown(KeyCode.Tab)){
                    ChangeState(ExperimentStates.Start);
                }
                else if (Input.GetKeyDown(KeyCode.Escape)){
                    ChangeState(ExperimentStates.Finished);
                }
                break;
        }
	}
	
    
	protected override void OnEnter(ExperimentStates oldState) {
		switch(GetState()) {
            case ExperimentStates.WaitingForFeedback:
                break;

            case ExperimentStates.Start:
                string[] dir = Directory.GetDirectories("Results");
                
                participantNumber = 1;
                participantName = "PC1-Participant" + participantNumber.ToString();
                
                trialCounter = 0;

                for (int i = 0; i < dir.Length; i++){
                    outputDirectory = "Results/ArEventTest" + participantNumber.ToString();
                    if (!Directory.Exists(outputDirectory)){
                        Directory.CreateDirectory(outputDirectory);
                        break;
                    } else {
                        participantNumber = participantNumber + 1;
                    }
                }
                
                logger.OpenLog(GetLogFilename());
                
                // Record participant number to log-file
                WriteLog("Participant" + participantNumber.ToString());
                if (!handSwitcher.useMale){
                    WriteLog("Hand model is female");
                } else if (handSwitcher.useMale){
                    WriteLog("Hand model is male");
                }

                
                string[] dirProtocol = Directory.GetFiles("Protocol/Parkinson");

                randomProtocol = UnityEngine.Random.Range(0, dirProtocol.Length);
                protocolFile = dirProtocol [randomProtocol]; 
                
                // Load protocol
                trialList = new TrialList(protocolFile);
                WriteLog("Loaded " + trialList.Count () + " trials");

                HandleEvent(ExperimentEvents.ProtocolLoaded);

                break;

    		case ExperimentStates.Trial:
    			if(trialList.HasMore()) {
                    trialCounter++;
                    StartTrial();
                } else {    			
                    ChangeState(ExperimentStates.WaitingForFeedback);
                }
    			break;
    
            case ExperimentStates.Finished:
    			StopMachine();
    			WriteLog("No more subjects, stopping machine");
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
            trialController.hand = 0;
        else if(trial["GapStatus"] == "Active")
            trialController.hand = 1;
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
        if (trial.ContainsKey("KnifePresent")){
            if (trial ["KnifePresent"].ToLower() == "true")
                trialController.knifePresent = true;
            else if (trial ["KnifePresent"].ToLower() == "false")
                trialController.knifePresent = false;
            else
                throw new Exception("Invalid value in trial list for field KnifePresent");
        }

        // Knife Offset
        if (trial.ContainsKey("KnifeOffset")){
            float knifeOffsetx; float knifeOffsety; float knifeOffsetz;
            float.TryParse(trial ["OffsetX"], out knifeOffsetx);
            float.TryParse(trial ["OffsetY"], out knifeOffsety);
            float.TryParse(trial ["OffsetZ"], out knifeOffsetz);

            Vector3 knifeVector = new Vector3(knifeOffsetx, knifeOffsety, knifeOffsetz);

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


	private string GetLEAPFilename(int trial){
		return outputDirectory + "\\" + DateTime.UtcNow.ToString("yyyy-MM-dd hh.mm ") + participantName + " Trial " + trial + ".csv";
	}


	private string GetLogFilename(){
		return outputDirectory + "\\" + DateTime.UtcNow.ToString("yyyy-MM-dd hh.mm ") + participantName + ".log";
	}


	private string GetResultsFilename() {
		return outputDirectory + "\\" + DateTime.UtcNow.ToString("yyyy-MM-dd hh.mm ") + participantName + ".csv";
	}

	
	/**
	 * Appends the result of the previous trial to the datafile
	 */
	private void SaveTrialResult() 
    {
		StreamWriter writer = new StreamWriter(GetResultsFilename(), true);
		
		// Append result of trial to data file
		writer.Write(trialCounter);
		writer.Write(", ");
		
		if(trialController.hand == 0)
			writer.Write("Continuous Limb, ");
		else if(trialController.hand == 1)
			writer.Write("Discontinous Limb, ");
		else
			writer.Write("Gap unknown, ");

        if (trialController.knifePresent == true)
            writer.Write("Threat active, ");
        else if (trialController.knifePresent == false)
            writer.Write("Threat inactive, ");
        else
            writer.Write("Threat unknown, ");

        if (trialController.noiseLevel == 0)
            writer.Write("Hand noise inactive, ");
        else if (trialController.noiseLevel == 1)
            writer.Write("Hand noise active, ");
        else 
            writer.Write("No information, ");

        writer.Write(trialController.knifeOffset);
        writer.Write(", ");		
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

    private void SaveExperimentResult(){
        handController.StopRecording();
    }
}
