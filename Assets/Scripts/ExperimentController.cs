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
	Delay1,
	Trial,
	ProprioceptiveDrift,
};


public class ExperimentController: StateMachine<ExperimentStates, ExperimentEvents>
{
	public HandSwitcher handSwitcher;
	public TrialController trialController;
	public MarkerController markerController;
	
	private TrialList trialList;
	public string protocolFile;
	public string outputFile;
	
			
	public void Start()
	{
		trialList = new TrialList(protocolFile);
		Debug.Log("Loaded " + trialList.Count () + " trials");

		this.StartMachine();
	}
	
	
	public void HandleEvent(ExperimentEvents ev)
	{
		Debug.Log ("Event " + ev.ToString());
		
		if(!IsStarted())
			return;
		
		switch(GetState()) {
		case ExperimentStates.Trial:
			if(ev == ExperimentEvents.TrialFinished) {
				ChangeState(ExperimentStates.Delay1);
			}
			break;
//		case ExperimentStates.ProprioceptiveDrift:
//			if(ev == ExperimentEvents.ProprioceptiveDriftMeasured)
//			{
//				ChangeState(ExperimentStates.Delay1);
//			}
//			break;	
		}
	}
	
	
	public void Update()
	{
		if(!IsStarted())
			return;
		
		switch(GetState()) {
//		case ExperimentStates.ProprioceptiveDrift:
//			if (!trialList.HasMore ())
//			{
//				Debug.Log("No more trials, measuring proprioceptive drift");
//				markerController.isStarted = true;
//				Debug.Log(markerController.proprioceptiveDrift);
//			}
//			break;
		case ExperimentStates.Delay1:
			// Stop the experiment if all trials have been done
			if(!trialList.HasMore()) {
				Debug.Log("No more trials, stopping machine");
				StopMachine();					
			} 
			else if(GetTimeInState() > 1.5f) {
				ChangeState(ExperimentStates.Trial);
				Debug.Log ("Changing ExperimentStates to Trial");
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
			else if(trial["GapSize"] == "Active")
				trialController.hand = 1;

			else {
				Debug.Log ("Invalid GapSize in protocol");
				trialController.hand = -1;
			}
				
			Debug.Log("Gap: " + trial["GapSize"]);
			

			// Get offset
			int offset;
			int.TryParse(trial["Offset"], out offset);
			trialController.offset = offset;
				
			Debug.Log("Offset: " + offset);
			
			// Start trial
			trialController.waveCounter = 0;
			trialController.StartMachine();
			break;

		case ExperimentStates.Delay1:
			break;	
		}
	}
	
	
	protected override void OnExit(ExperimentStates newState)
	{
		StreamWriter writer = new StreamWriter(outputFile, true);
		switch(GetState()) 
		{
		case ExperimentStates.Trial:
			// Append result of trial to data file
			if(trialController.hand == 0)
				writer.Write("Short, ");
			else if(trialController.hand == 1)
				writer.Write("Long, ");
			else
				writer.Write("Unknown, ");

			writer.Write(trialController.offset);
			writer.Write(", ");
			writer.Write(trialController.response);
			writer.WriteLine();

			writer.Close();

			break;

		// case ExperimentStates.ProprioceptiveDrift:
//			writer.Write(markerController.proprioceptiveDrift);
//			break;
		}
	}
}
