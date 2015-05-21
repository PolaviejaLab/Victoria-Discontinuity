using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public enum ExperimentEvents
{
	TrialFinished,
	ProprioceptiveDriftMeasured,
};


public enum ExperimentStates
{
	AccomodationTime,
	Trial,
	ProprioceptiveDrift,
	Finished,
};


public class ExperimentController: StateMachine<ExperimentStates, ExperimentEvents>
{
	public HandSwitcher handSwitcher;
	public TrialController trialController;
	public MarkerController markerController;
	
	private TrialList trialList;
	public string protocolFile;
	public string outputFile;
	
			
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
				if(GetTimeInState() > 1.0f) {
					Debug.Log("measuring proprioceptive drift");
					ChangeState(ExperimentStates.ProprioceptiveDrift);
				} 
//				ChangeState(ExperimentStates.AccomodationTime);
			}
			break;
		case ExperimentStates.ProprioceptiveDrift:
			if (ev == ExperimentEvents.ProprioceptiveDriftMeasured) {
				ChangeState(ExperimentStates.AccomodationTime);
				Debug.Log ("PD measured");
			}
			break;
		}
	}
	
	
	public void Update()
	{
		if(!IsStarted())
			return;
		
		switch(GetState()) {
		case ExperimentStates.AccomodationTime:
			if (!trialList.HasMore()){
				ChangeState(ExperimentStates.Finished);
			}
			else if (GetTimeInState() > 1.0f){
				ChangeState(ExperimentStates.Trial);
			}
			break;
		}
	}
	
	protected override void OnEnter(ExperimentStates oldState)
	{
		switch(GetState()) {
		case ExperimentStates.Trial:	
			// Load next trial from list
			Dictionary<string, string> trial = trialList.Pop ();

			Debug.Log("Starting new trial");

			// Determine which hand to use for given gapsize
			if(trial["GapSize"] == "Small")
				trialController.hand = 0;
			else if(trial["GapSize"] == "Large")
				trialController.hand = 1;
			else {
				Debug.Log ("Invalid GapSize in protocol");
				trialController.hand = -1;
			}
				
			Debug.Log("Gap: " + trial["GapSize"]);
			
			// Get offset
			float offset;
			float.TryParse(trial["Offset"], out offset);
			trialController.offset = offset;
				
			Debug.Log("Offset: " + offset);
			
			// Start trial
			trialController.waveCounter = 0;
			trialController.StartMachine();
			trialController.ChangeState(TrialStates.WaitForWave);
			break;

		case ExperimentStates.ProprioceptiveDrift:
			markerController.isStarted = true;
			break;

		case ExperimentStates.Finished:
			Debug.Log("No more trials, stopping machine");
//			markerController.isStarted = false;
			StopMachine();	
			break;
		}
	}
	
	
	protected override void OnExit(ExperimentStates newState){

		StreamWriter writer = new StreamWriter(outputFile, true);

		switch(GetState()) {
		case ExperimentStates.Trial:
			// Append result of trial to data file
			if(trialController.hand == 0)
				writer.Write("Without gap, ");
			else if(trialController.hand == 1)
				writer.Write("With gap, ");
			else
				writer.Write("Unknown, ");

			writer.Write(trialController.offset);
			writer.Write(", ");
			writer.Write(trialController.response);
			writer.WriteLine();

			break;

		 case ExperimentStates.ProprioceptiveDrift:
			writer.Write(markerController.proprioceptiveDrift);
			break;
		}
		writer.Close();
	}
}
