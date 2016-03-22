using UnityEngine;
using System.Collections;


/**
 * Events handles by the Trial statemachine
 */
public enum TrialEvents {
    WavingFinished,
    DriftMeasured,
    ThreatDone,
};


/**
 * States of the Trial statemachine
 */
public enum TrialStates {
	AccomodationTime,           // Get used to the environment
    Wave,                       // Reaching-like task
    ProprioceptiveDrift,        // Measure proprioceptive drift
    Threat,                     // Threat to the virtual hand
    Finished,                   // End of the trial
};


public class TrialController : StateMachine<TrialStates, TrialEvents>
{
	// Reference to the experiment controller
	public ExperimentController experimentController;
    public WaveController waveController;
    public PropDriftController driftController;
    public Threat threatController;

	// Scripts to manipulate the hand and offset according to condition
	public HandSwitcher handSwitcher;
	public OffsetSwitcher offsetSwitcher;

    public GameObject testLights;

	// Parameters of the current trial
	public int hand;
	public float offset;
    public float noiseLevel;
    public bool knifePresent;
    public Vector3 knifeOffset;


    public void Start() {
        threatController.Stopped += (obj, ev) => HandleEvent(TrialEvents.ThreatDone);
	}


	public void HandleEvent(TrialEvents ev) {
        Debug.Log("Event " + ev.ToString());

        if (!IsStarted())
			return;
	
		switch (GetState()) {
            case TrialStates.AccomodationTime:
                break;

            case TrialStates.Wave:
                if (ev == TrialEvents.WavingFinished){
                    ChangeState(TrialStates.ProprioceptiveDrift);
                }
                break;

            case TrialStates.ProprioceptiveDrift:
                if (ev == TrialEvents.DriftMeasured)
                    ChangeState(TrialStates.Threat);
                break;

            case TrialStates.Threat:
                if (ev == TrialEvents.ThreatDone)
                    ChangeState(TrialStates.Finished);
                break;

            case TrialStates.Finished:
                break;
			
        }
    }
    
    
    protected override void OnStart() {
		// Set trial parameters
		offsetSwitcher.offset = offset;
		handSwitcher.selected = hand;
        handSwitcher.noiseLevelLeft = noiseLevel;
        handSwitcher.noiseLevelRight = noiseLevel;
        threatController.knifeOffset = knifeOffset;
	}


	public void Update(){
		if (!IsStarted ())
			return;

		switch (GetState ()) {  
    		case TrialStates.AccomodationTime:				
    			if (Input.GetKey(KeyCode.Q))
    				ChangeState(TrialStates.Wave);
    			break;

            case TrialStates.Wave:
                break;

            case TrialStates.ProprioceptiveDrift:
                if (GetTimeInState() > 3.0f)
                    driftController.StartMachine();
                break;

    		case TrialStates.Threat:
    			break;

            case TrialStates.Finished:
                break;
		}
	}
	

	protected override void OnEnter(TrialStates oldState){
		switch (GetState ()) {

            case TrialStates.AccomodationTime:
    			handSwitcher.showRightHand = true;
    			break;

            case TrialStates.Wave:
                waveController.StartMachine();
                break;

            case TrialStates.ProprioceptiveDrift:
                handSwitcher.showRightHand = false; 
                break;

            case TrialStates.Threat:
                if (knifePresent) {
                    threatController.StartMachine();
                    threatController.HandleEvent(ThreatEvent.ReleaseThreat);
                }
                break;

            case TrialStates.Finished:
                break;

        }
    }
    

    protected override void OnExit(TrialStates newState) {
		switch (GetState ()) {
    		case TrialStates.AccomodationTime:
    			handSwitcher.showLeftHand = false;
    			break;

            case TrialStates.Wave:
                testLights.SetActive(false);
                waveController.StopMachine();
                break;

            case TrialStates.ProprioceptiveDrift:
                break;

            case TrialStates.Threat:
                break;

            case TrialStates.Finished:
                break;

		}
    }
}
