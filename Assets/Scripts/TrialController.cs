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
				Debug.Log ("Num of Correct Waves: " + correctWaves);
				Debug.Log ("Total num of waves: " + waveCounter);

			}
			if(ev == TrialEvents.Wave_2 && currentLight == 1){
				correctWaves++;
				ChangeState (TrialStates.Waved);
				Debug.Log ("Num of Correct Waves: " + correctWaves);
				Debug.Log ("Total num of waves: " + waveCounter);


			}
			if(ev == TrialEvents.Wave_3 && currentLight == 2){
				correctWaves++;
				ChangeState(TrialStates.Waved);
				Debug.Log ("Num of Correct Waves: " + correctWaves);
				Debug.Log ("Total num of waves: " + waveCounter);

	
			}
			// incorrect waves
			if (ev == TrialEvents.Wave_1 && currentLight == 1){
				incorrectWaves++;
				ChangeState(TrialStates.Waved);
				Debug.Log ("Num of Incorrect Waves: " + incorrectWaves);
				Debug.Log ("Total num of waves: " + waveCounter);

		
			}
			if (ev == TrialEvents.Wave_1 && currentLight == 2){
				incorrectWaves++;
				ChangeState(TrialStates.Waved);
				Debug.Log ("Total num of waves: " + waveCounter);
				Debug.Log ("Num of Incorrect Waves: " + incorrectWaves);

			}
			if (ev == TrialEvents.Wave_2 && currentLight == 0){
				incorrectWaves++;
				ChangeState(TrialStates.Waved);
				Debug.Log ("Num of Incorrect Waves: " + incorrectWaves);
				Debug.Log ("Total num of waves: " + waveCounter);


			}
			if (ev == TrialEvents.Wave_2 && currentLight == 2){
				incorrectWaves++;
				ChangeState(TrialStates.Waved);
				Debug.Log ("Num of Incorrect Waves: " + incorrectWaves);
				Debug.Log ("Total num of waves: " + waveCounter);


			}
			if (ev == TrialEvents.Wave_3 && currentLight == 0){
				incorrectWaves++;
				ChangeState(TrialStates.Waved);
				Debug.Log ("Num of Incorrect Waves: " + incorrectWaves);
				Debug.Log ("Total num of waves: " + waveCounter);


			}
			if (ev == TrialEvents.Wave_3 && currentLight == 1){
				incorrectWaves++;
				ChangeState(TrialStates.Waved);
				Debug.Log ("Num of Incorrect Waves: " + incorrectWaves);
				Debug.Log ("Total num of waves: " + waveCounter);


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
//			if(GetTimeInState() > 2.0f)
//				ChangeState(TrialStates.WaitForWave);
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
			offsetSwitcher.offset = offset;
			// set the hand
			handSwitcher.selected = hand;
			// turn on random light
			currentLight = Random.Range(0, lights.Length);
			lights[currentLight].activeMaterial = 1;	
			break;

		case TrialStates.WithoutFeedback:
			lights[currentLight].activeMaterial = 0;
			handSwitcher.selected = 3;
//			handSwitcher.selected = 2;
//			currentLight = Random.Range(0, lights.Length);				
//			lights[currentLight].activeMaterial = 1;		
			break;
						
		case TrialStates.TooLate:
			ChangeState(TrialStates.Waved);
			break;

		case TrialStates.Waved:
			waveCounter++;
			Debug.Log ("miaw+");
			if (waveCounter <= wavesRequired){
				ChangeState (TrialStates.WaitForWave);
			} 
			else {
				ChangeState (TrialStates.WithoutFeedback);
			}
			break;

		case TrialStates.Final:
			Debug.Log (correctWaves);
			Debug.Log (incorrectWaves);
			Debug.Log (waveCounter);
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
//			handSwitcher.selected = 3;
//			lights[currentLight].activeMaterial = 0;
			break;		
		
		case TrialStates.TooLate:
			break;
		}
	}
}
