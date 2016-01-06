using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;


public enum ExperimentEvents {
	TrialFinished,
	HandThreatened, 
};


public enum ExperimentStates {
	Trial,
	Threat, 
	Finished,
};


public class ExperimentController: StateMachine<ExperimentStates, ExperimentEvents> {
	public HandController handController;
	public HandSwitcher handSwitcher;
	public TrialController trialController;
	public Threat threatController;

	public TableLights tableLights;
	
	private TrialList trialList;
	public string protocolFile;

	public string outputDirectory = "Results";
	public string participantName = "Anonymous";

	public int trialCounter;

	public void Start() {
		logger.OpenLog(GetLogFilename());

		trialList = new TrialList(protocolFile);
		WriteLog("Loaded " + trialList.Count () + " trials");

		this.StartMachine();
	}
	
	
	public void HandleEvent(ExperimentEvents ev) {
		WriteLog("Event " + ev.ToString());

		if(!IsStarted())
			return;
		
		switch(GetState()) {

		case ExperimentStates.Trial:
			if (ev == ExperimentEvents.TrialFinished) {
				ChangeState (ExperimentStates.Threat);
			}
			break;
		}
	}
	
	public void Update() {
		if(!IsStarted())
			return;
		
		switch (GetState ()) {
		case ExperimentStates.Threat:
//			if (GetTimeInState () > 4.0f)
//				threatController.isActive = true;
//			Debug.Log("miaw");
			break;
		}
	}
	
	protected override void OnEnter(ExperimentStates oldState) {

		switch(GetState()) {
		case ExperimentStates.Trial:
			if (!trialList.HasMore())
				ChangeState(ExperimentStates.Finished);
			
			trialCounter++;
			StartTrial();
			break;

		case ExperimentStates.Threat:
			threatController.isActive = true;
			Debug.Log ("This knife is almost falling");
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
		case ExperimentStates.Threat:
			break;

		}
	}


	/**
	 * Start the next trial
	 */
	private void StartTrial() {
		// Do not start if already running
		if(trialController.IsStarted())
			return;

		WriteLog("Starting trial");

		// Load next trial from list
		Dictionary<string, string> trial = trialList.Pop();
	
		handController.StartRecording(GetLEAPFilename(trialCounter));
		
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


	private string GetResultsFilename()
	{
		return outputDirectory + "\\" + DateTime.UtcNow.ToString("yyyy-MM-dd hh.mm ") + participantName + ".csv";
	}

	
	/**
	 * Appends the result of the previous trial to the datafile
	 */
	private void SaveTrialResult() {
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
