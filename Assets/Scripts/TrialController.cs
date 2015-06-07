using UnityEngine;
using System.Collections;


/**
 * Events handles by the Trial statemachine
 * 
 * Initial must be last and wave_1 ... N must correspond to lights 1 .. N
 */
public enum TrialEvents {
	Wave_0 = 0,
	Wave_1 = 1,
	Wave_Initial,
	Delay,
};

/**
 * States of the Trial statemachine
 */
public enum TrialStates {
	AccomodationTime,
	WaitForInitial,
	WaitForWave,
	Waved,
	TooLate,
	WithoutFeedback,
	Final,
};


public class TrialController : StateMachine<TrialStates, TrialEvents>
{
	// Reference to the experiment controller
	public ExperimentController experimentController;

	// Initial and subsequent lights
	public MaterialChanger initialLight;
	public MaterialChanger[] lights;

	// Scripts to manipulate the hand and offset according to condition
	public HandSwitcher handSwitcher;
	public OffsetSwitcher offsetSwitcher;

	// Parameters of the current trial
	public int hand;
	public float offset;
	public int wavesRequired;

	// Keep track of required number of waves
	public int waveCounter;

	// Keep track of number of correct / incorrect waves
	public int lateWaves;
	public int correctWaves;
	public int incorrectWaves;

	// Number of the light that is currently on
	private int currentLight;

	// Turn on/off lights
	public bool greenLightOn;
	public bool initialLightOn;

	// Collider on/off
	public GameObject collisionLights;
	public GameObject collisionInitial;

	public void Start () {
		collisionLights = GameObject.Find ("CubeLight");
		collisionInitial = GameObject.Find ("CubeInitial");
	}

	public void HandleEvent (TrialEvents ev){
		Debug.Log ("Event " + ev.ToString ());
	
		if (!IsStarted ())
			return;
	
		switch (GetState ()) {
			case TrialStates.WaitForInitial:
			if (ev == TrialEvents.Delay && initialLightOn) {
				initialLight.activeMaterial = 1;
				collisionInitial.SetActive(true);
			} else if (ev == TrialEvents.Wave_Initial){
				ChangeState (TrialStates.WaitForWave);
			}
			break;

		case TrialStates.WaitForWave:
			if (ev == TrialEvents.Delay && greenLightOn) {
				// Turn on random target light
				currentLight = Random.Range (0, lights.Length);
				lights [currentLight].activeMaterial = 1;
				collisionLights.SetActive (true);
			} else if((int)ev == currentLight) {
				correctWaves++;
				ChangeState(TrialStates.Waved);
			} else if((int)ev != currentLight && ev != TrialEvents.Wave_Initial) {
				incorrectWaves++;
				ChangeState(TrialStates.Waved);
			}
			break;

		case TrialStates.Final:
			if (ev == TrialEvents.Wave_Initial){
				experimentController.HandleEvent (ExperimentEvents.TrialFinished);
				StopMachine();
			}
			break;
		}
	}

	protected override void OnStart(){

		// Set trial parameters
		offsetSwitcher.offset = offset;
		handSwitcher.selected = hand;

		// Clear counters
		waveCounter = 0;
		lateWaves = 0;
		correctWaves = 0;
		incorrectWaves = 0;

		initialLight.activeMaterial = 0;
		collisionInitial.SetActive (false);
		collisionLights.SetActive (false);
	}


	public void Update (){
		if (!IsStarted ())
			return;

		switch (GetState ()) {

		case TrialStates.AccomodationTime:				
			if (GetTimeInState() > 10.0f)
				ChangeState(TrialStates.WaitForInitial);
			break;

		case TrialStates.WaitForInitial:
			if (GetTimeInState ()> 1.0f && !initialLightOn) {
				initialLightOn = true;
				HandleEvent (TrialEvents.Delay);
			}
			break;

		
		case TrialStates.WaitForWave:

			// Wait between the lights turning on and off
			if (GetTimeInState () > 1.0f && !greenLightOn){
				greenLightOn = true;
				HandleEvent(TrialEvents.Delay);
			}

			// Move to next state if more than 5 seconds elapsed 
			// from the green light turning on
			if (GetTimeInState () > 6.0f) {
				lateWaves++;
				ChangeState (TrialStates.TooLate);
			}
			break;

		// Wait one second after the wave, and then start again
		case TrialStates.Waved:
			if (GetTimeInState () > 1.0f) {
				if (waveCounter < wavesRequired) {
					ChangeState (TrialStates.WaitForInitial);
				} else {
					ChangeState (TrialStates.WithoutFeedback);
				}
			}
			break;		


		case TrialStates.WithoutFeedback:
			if (GetTimeInState () > 1.0f)
				ChangeState (TrialStates.Final);
			break;
		}
	}
	
	protected override void OnEnter (TrialStates oldState){
		switch (GetState ()) {
		case TrialStates.AccomodationTime:

			handSwitcher.showRightHand = true;
			break;

		// Turn initial light on
		case TrialStates.WaitForInitial:
			break;

		case TrialStates.WaitForWave:
			// Increment wave counter
			waveCounter++;
			break;

		case TrialStates.Waved:
			break;
		
		case TrialStates.WithoutFeedback:
			break;

		case TrialStates.TooLate:
			ChangeState (TrialStates.Waved);
			break;

		case TrialStates.Final:
			// last initial light on to mark a central point for the drift measure
			initialLight.activeMaterial = 2;
			collisionInitial.SetActive(true);
			break;
		}
	}
	
	protected override void OnExit (TrialStates newState){
	
		switch (GetState ()) {
		case TrialStates.AccomodationTime:
			handSwitcher.showLeftHand = false;
			break;

		case TrialStates.WaitForInitial:
			collisionInitial.SetActive(false);
			initialLight.activeMaterial = 0;
			initialLightOn = false;
			break;

		case TrialStates.WaitForWave:
			collisionLights.SetActive (false);
			lights[currentLight].activeMaterial = 0;
			greenLightOn = false;
			break;
		
		case TrialStates.WithoutFeedback:
			break;		
		
		case TrialStates.TooLate:
			break;
		}
	}
}
