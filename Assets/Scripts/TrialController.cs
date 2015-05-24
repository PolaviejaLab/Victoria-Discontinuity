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
	Waved,
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

	public OffsetSwitcher offsetSwitcher;
	public float offset;

	public int response;
	
	public int wavesRequired;
	
	public int waveCounter;
	public int correctWaves;
	public int incorrectWaves;

	public void HandleEvent(TrialEvents ev)
	{
		Debug.Log ("Event " + ev.ToString());
	
		if(!IsStarted())
			return;
	
		switch(GetState()) {
		case TrialStates.WaitForWave:
			// correct waves
			if(ev == TrialEvents.Wave_1 && currentLight == 0) {
				correctWaves++;
				ChangeState (TrialStates.Waved);
			}
			if(ev == TrialEvents.Wave_2 && currentLight == 1){
				correctWaves++;
				ChangeState (TrialStates.Waved);
			}
			if(ev == TrialEvents.Wave_3 && currentLight == 2){
				correctWaves++;
				ChangeState(TrialStates.Waved);
			}
			// incorrect waves
			if (ev == TrialEvents.Wave_1 && currentLight == 1){
				incorrectWaves++;
				ChangeState(TrialStates.Waved);
			}
			if (ev == TrialEvents.Wave_1 && currentLight == 2){
				incorrectWaves++;
				ChangeState(TrialStates.Waved);
			}
			if (ev == TrialEvents.Wave_2 && currentLight == 0){
				incorrectWaves++;
				ChangeState(TrialStates.Waved);
			}
			if (ev == TrialEvents.Wave_2 && currentLight == 2){
				incorrectWaves++;
				ChangeState(TrialStates.Waved);
			}
			if (ev == TrialEvents.Wave_3 && currentLight == 0){
				incorrectWaves++;
				ChangeState(TrialStates.Waved);
			}
			if (ev == TrialEvents.Wave_3 && currentLight == 1){
				incorrectWaves++;
				ChangeState(TrialStates.Waved);
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
			
		case TrialStates.Waved:
			if (GetTimeInState() > 1.0f) {
				if (waveCounter < wavesRequired){
					ChangeState (TrialStates.WaitForWave);
				} 
				else {
					ChangeState (TrialStates.WithoutFeedback);
				}
			}
			break;		
				
		case TrialStates.WithoutFeedback:
			if(GetTimeInState() > 1.0f)
				ChangeState(TrialStates.Final);
			break;	
		}
	}
	
	protected override void OnEnter(TrialStates oldState) {
		switch(GetState()) {
		case TrialStates.WaitForWave:
			waveCounter++;
			// set the offset
			offsetSwitcher.offset = offset;
			// set the hand
			handSwitcher.selected = hand;
			// turn on random light
			currentLight = Random.Range(0, lights.Length);
			lights[currentLight].activeMaterial = 1;	
			break;

		case TrialStates.Waved:
			break;
		
		case TrialStates.WithoutFeedback:
			break;

		case TrialStates.TooLate:
			ChangeState(TrialStates.Waved);
			break;

		case TrialStates.Final:
			experimentController.HandleEvent(ExperimentEvents.TrialFinished);
			StopMachine();
			break;
		}
	}
	
	protected override void OnExit(TrialStates newState){
		switch(GetState()) {
		case TrialStates.WaitForWave:
			lights[currentLight].activeMaterial = 0;
			break;
		
		case TrialStates.WithoutFeedback:
			break;		
		
		case TrialStates.TooLate:
			break;
		}
	}
}
