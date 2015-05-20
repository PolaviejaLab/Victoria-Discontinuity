using UnityEngine;
using System.Collections;


public enum TrialEvents
{
	Wave_1,
	Wave_2,
	Wave_3,
};


public enum TrialStates
{
	WaitingForFirstTrial,
	WaitForWave,
	TooLate,
	WithoutFeedback,
	Final,
};


public class TrialController: StateMachine<TrialStates, TrialEvents>
{
	public ExperimentController experimentController;

	public int currentLight;
	public MaterialChanger[] lights;
	
	public HandSwitcher handSwitcher;
	public int hand;
	
	public int offset;
	public int response;
	
	public int wavesRequired;
	
	public int waveCounter;
	
	public void HandleEvent(TrialEvents ev)
	{
		Debug.Log ("Event " + ev.ToString());
	
		if(!IsStarted())
			return;
	
		switch(GetState()) {

		case TrialStates.WaitForWave:
			if(ev == TrialEvents.Wave_1 && currentLight == 0) {
				waveCounter++;

				if(waveCounter < wavesRequired){
					ChangeState (TrialStates.WaitForWave);
				}
				else {
					ChangeState(TrialStates.WithoutFeedback);
				}
			}
				
			if(ev == TrialEvents.Wave_2 && currentLight == 1){
				waveCounter++;

				if(waveCounter < wavesRequired){
					ChangeState (TrialStates.WaitForWave);
				}
				else {
					ChangeState(TrialStates.WithoutFeedback);
				}
			}
				
			if(ev == TrialEvents.Wave_3 && currentLight == 2){
				waveCounter++;
				
				if(waveCounter < wavesRequired){
					ChangeState (TrialStates.WaitForWave);
				}
				else {
					ChangeState(TrialStates.WithoutFeedback);
				}
			}
			break;
		}
	}
	
	public void Update()
	{
		if(!IsStarted())
			return;

		switch(GetState()) {


		case TrialStates.WaitForWave:
			if(GetTimeInState() > 5.0f)
				ChangeState(TrialStates.TooLate);
			break;
			
		case TrialStates.TooLate:
			if(GetTimeInState() > 2.0f)
				ChangeState(TrialStates.Final);
			break;		
				
		case TrialStates.WithoutFeedback:
			if(GetTimeInState() > 1.0f)
				ChangeState(TrialStates.Final);
			break;	
		}
	}
	
	protected override void OnEnter(TrialStates oldState)
	{
		switch(GetState()) {
		case TrialStates.WaitForWave:

			// set the offset

			handSwitcher.selected = hand;
			currentLight = Random.Range(0, lights.Length);
			lights[currentLight].activeMaterial = 1;	
			break;

		case TrialStates.WithoutFeedback:
			handSwitcher.selected = 2;
			currentLight = Random.Range(0, lights.Length);				
			lights[currentLight].activeMaterial = 1;		
			break;
						
		case TrialStates.TooLate:
			break;
				
		case TrialStates.Final:
			StopMachine();
			experimentController.HandleEvent(ExperimentEvents.TrialFinished);
			break;
		}
	}
	
	protected override void OnExit(TrialStates newState)
	{
		switch(GetState()) {
		case TrialStates.WaitForWave:
			lights[currentLight].activeMaterial = 0;
			break;
		
		case TrialStates.WithoutFeedback:
			handSwitcher.selected = 3;
			lights[currentLight].activeMaterial = 0;
			break;		
		
		case TrialStates.TooLate:
			break;
		}
	}
}
