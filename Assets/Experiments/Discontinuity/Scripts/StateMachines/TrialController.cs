using UnityEngine;
using System.Collections;


/**
 * Events handles by the Trial statemachine
 */
public enum TrialEvents {
    WavingFinished,
    MeasureDone,
    ThreatDone,
};


/**
 * States of the Trial statemachine
 */
public enum TrialStates {
	AccomodationTime,           // Get used to the environment
    ExperimentWave,             // Reaching-like task
    Measure,                    // Implicit measure
    TrialFinished,              // End of the trial
};


public class TrialController : ICStateMachine<TrialStates, TrialEvents>
{
	// Reference to the experiment controller
	public ExperimentController experimentController;
    public WaveController waveController;
  //   public PropDriftController driftController;
    public Threat threatController;
    public ImplicitMeasure measureController;

	// Scripts to manipulate the hand and offset according to condition
	public HandSwitcher handSwitcher;
	public OffsetSwitcher offsetSwitcher;

    public GameObject testLights;
    public GameObject room;
    public GameObject table;

	// Parameters of the current trial
	public int hand;
	public float offset;
    public float noiseLevel;
    public float lNoise;
    public Vector3 knifeOffset;
    public bool changeGender;
    public bool genderChanged;

    // wave recording variables
    public int totWaves;
    public int correctWaves;
    public int incorrectWaves;
    public int lateWaves;


    public void Start() {
	}

    protected override void OnStart() {
        // Set trial parameters
        offsetSwitcher.initialOffset = offset;
        handSwitcher.selected = hand;
        handSwitcher.noiseLevelLeft = noiseLevel;
        handSwitcher.noiseLevelRight = noiseLevel;
        handSwitcher.lambdaLeft = lNoise;
        handSwitcher.lambdaRight = lNoise;
        threatController.threatOffset = knifeOffset;
        threatController.handOffset = new Vector3 (0, 0, offset);
        
        testLights.SetActive(true);
    }


    public void HandleEvent(TrialEvents ev) {
        Debug.Log("Event " + ev.ToString());

        if (!IsStarted())
			return;
	
		switch (GetState()) {
            case TrialStates.AccomodationTime:
                testLights.SetActive(false);
                break;

            case TrialStates.ExperimentWave:
                if (ev == TrialEvents.WavingFinished){
                    ChangeState(TrialStates.Measure);
                }
                break;

            case TrialStates.Measure:
                if (ev == TrialEvents.MeasureDone) {
                    ChangeState(TrialStates.TrialFinished);
                }
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
    				ChangeState(TrialStates.ExperimentWave);
    			break;

            case TrialStates.ExperimentWave:
                break;

            case TrialStates.Measure:
                break;

            case TrialStates.TrialFinished:
                break;
		}
	}
	

	protected override void OnEnter(TrialStates oldState){
		switch (GetState ()) {

            case TrialStates.AccomodationTime:
    			handSwitcher.showRightHand = true;

                if (genderChanged == false && changeGender == true)
                {
                    if (handSwitcher.useMale)
                    {
                        handSwitcher.useMale = false;
                        WriteLog("Gender changed to female");
                    }
                    else if (!handSwitcher.useMale)
                    {
                        handSwitcher.useMale = true;
                        WriteLog("Gender changed to male");
                    }
                    genderChanged = true;
                }
                else if (genderChanged == true && changeGender == false) {
                    if (handSwitcher.useMale)
                    {
                        handSwitcher.useMale = false;
                        WriteLog("Gender changed to female");
                    }
                    else if (!handSwitcher.useMale)
                    {
                        handSwitcher.useMale = true;
                        WriteLog("Gender changed to male");
                    }
                    genderChanged = false;
                }
                break;

            case TrialStates.ExperimentWave:
                waveController.StartMachine();
                break;

            case TrialStates.Measure:         
                measureController.measure.SetActive(true);
                measureController.StartMachine();
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

            case TrialStates.ExperimentWave:
                testLights.SetActive(false);
                totWaves = waveController.waveCounter;
                correctWaves = waveController.correctWaves;
                incorrectWaves = waveController.incorrectWaves;
                lateWaves = waveController.lateWaves;
                waveController.StopMachine();
                break;

            case TrialStates.Measure:
                break;

            case TrialStates.TrialFinished:
                break;
		}
    }
}
