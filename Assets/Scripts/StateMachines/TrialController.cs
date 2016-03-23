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
    TrialFinished,              // End of the trial
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

    protected override void OnStart() {
        // Set trial parameters
        offsetSwitcher.offset = offset;
        handSwitcher.selected = hand;
        handSwitcher.noiseLevelLeft = noiseLevel;
        handSwitcher.noiseLevelRight = noiseLevel;
        threatController.knifeOffset = knifeOffset;

        testLights.SetActive(true);
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
                    ChangeState(TrialStates.TrialFinished);
                break;

            case TrialStates.TrialFinished:
                break;
			
        }
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

                break;

    		case TrialStates.Threat:
    			break;

            case TrialStates.TrialFinished:
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
                driftController.StartMachine();
                driftController.markerOn = true;
                break;

            case TrialStates.Threat:
                if (knifePresent) {
                    handSwitcher.showRightHand = true;
                    threatController.StartMachine();
                    threatController.HandleEvent(ThreatEvent.ReleaseThreat);
                }
                break;

            case TrialStates.TrialFinished:
                experimentController.HandleEvent(ExperimentEvents.TrialFinished);
                this.StopMachine();
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
                handSwitcher.showRightHand = false;
                waveController.StopMachine();
                break;

            case TrialStates.ProprioceptiveDrift:
                driftController.markerOn = false;
                driftController.marker.SetActive(false);
                driftController.StopMachine();
                break;

            case TrialStates.Threat:
                threatController.StopMachine();
                break;

            case TrialStates.TrialFinished:
                break;

		}
    }
}
