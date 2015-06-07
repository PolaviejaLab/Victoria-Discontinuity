using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public enum ExperimentEvents
{
	TrialFinished,
	MeasureProprioceptiveDrift,
	ProprioceptiveDriftMeasured,
};


public enum ExperimentStates
{
	Trial,
	ProprioceptiveDrift,
	Questionnaire,
	Finished,
};


public class ExperimentController: StateMachine<ExperimentStates, ExperimentEvents>
{
	public HandSwitcher handSwitcher;
	public TrialController trialController;
	public MarkerController markerController;
	public TableLights tableLights;
	
	private TrialList trialList;
	public string protocolFile;
	public string outputFile;

	public int trialCounter;

	private bool measurePD;

	public void Start() {

		trialList = new TrialList(protocolFile);
		Debug.Log("Loaded " + trialList.Count () + " trials");

		this.StartMachine();
	}
	
	
	public void HandleEvent(ExperimentEvents ev) {
		Debug.Log ("Event " + ev.ToString());


		if(!IsStarted())
			return;
		
		switch(GetState()) {


		case ExperimentStates.Trial:
			if(ev == ExperimentEvents.TrialFinished) {
				ChangeState(ExperimentStates.ProprioceptiveDrift);
			} 
			break;
		case ExperimentStates.ProprioceptiveDrift:
			if (ev == ExperimentEvents.MeasureProprioceptiveDrift){
				markerController.isStarted = true;
				markerController.dirRight = true;		

			} else if (ev == ExperimentEvents.ProprioceptiveDriftMeasured) {
				ChangeState(ExperimentStates.Questionnaire);
				markerController.isStarted = false;
				measurePD = false;
			}
			break;
		}
	}
	
	public void Update() {
		if(!IsStarted())
			return;
		
		switch (GetState ()) {
		case ExperimentStates.Questionnaire:
			if (Input.GetKeyDown (KeyCode.Return)) {
					ChangeState (ExperimentStates.Trial);
				}
			break;

		
		case ExperimentStates.ProprioceptiveDrift:
			if (GetTimeInState() > 2.0f && !measurePD) {
				HandleEvent(ExperimentEvents.MeasureProprioceptiveDrift);
				measurePD = true;
			}
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

		case ExperimentStates.ProprioceptiveDrift:
			handSwitcher.showRightHand = false;
			tableLights.isOn = false;
			break;

		case ExperimentStates.Questionnaire:
			break;

		case ExperimentStates.Finished:
			StopMachine();	
			Debug.Log("No more trials, stopping machine");
			break;
		}
	}


	protected override void OnExit(ExperimentStates newState) {
		switch(GetState()) {
			case ExperimentStates.Trial:
				break;

		 	case ExperimentStates.ProprioceptiveDrift:
				SaveTrialResult();
				break;

			case ExperimentStates.Questionnaire:
				break;
		}
	}


	
	/**
	 * Start the next trial
	 */
	private void StartTrial(){
		// Do not start if already running
		if(trialController.IsStarted())
			return;
		
		// Load next trial from list
		Dictionary<string, string> trial = trialList.Pop();
	
		
		// Determine which hand to use for given gapsize
		if(trial["GapStatus"] == "Inactive")
			trialController.hand = 1;
		else if(trial["GapStatus"] == "Active")
			trialController.hand = 0;
		else {
			Debug.Log ("Invalid GapSize in protocol");
			trialController.hand = -1;
		}
		
		Debug.Log("Gap: " + trial["GapStatus"]);
		
		// Get offset
		float offset;
		float.TryParse(trial["Offset"], out offset);
		trialController.offset = offset / 100.0f;

		Debug.Log("Offset: " + offset);

		// Determine the number of waves per each trial
		int wavesRequired;
		int.TryParse(trial["WavesRequired"], out wavesRequired);
		trialController.wavesRequired = wavesRequired;

		// Turn table lights on
		tableLights.isOn = true;

		// Turn proprioceptive drift marker off
		markerController.isStarted = false;
		
		trialController.initialState = TrialStates.AccomodationTime;
		trialController.StartMachine();
	}
	
	
	/**
	 * Appends the result of the previous trial to the datafile
	 */
	private void SaveTrialResult() {
		StreamWriter writer = new StreamWriter(outputFile, true);
		
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
		writer.Write(markerController.proprioceptiveDrift);
		writer.Write(", ");
		writer.Write(markerController.handPosition.x);
		writer.Write(", ");
		writer.Write(markerController.handPosition.y);
		writer.Write(", ");
		writer.Write(markerController.handPosition.z);
		writer.WriteLine();
		
		writer.Close();
	}
}
