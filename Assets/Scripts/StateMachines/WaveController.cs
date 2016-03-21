using UnityEngine;
using System.Collections;
using System;

/**
* Events handled by the Wave State Machine
*/

public enum WaveEvents {
    Wave_0 = 0,
    Wave_1 = 1,
    Wave_Initial, 
    Delay,
};

/**
* States of the Wave State Machine
*/
public enum WaveStates {
    Initial,
    Target, 
    Waved,
    TooLate,
    ToOrigin,
    EndWaving,  
};


public class WaveController : StateMachine<WaveStates, WaveEvents>
{
    // Reference to other classes
    public TrialController trialController;

    // Initial and subsequent lights
    public MaterialChanger initialLight;
    public MaterialChanger[] lights;

    // Parameters for the waving
    public int wavesRequired;

    // Keep track of the number of waves
    public int waveCounter;

    // Keep track of the outcome of the waves
    public int lateWaves;
    public int correctWaves;
    public int incorrectWaves;

    // Number of current light
    private int currentLight;

    // State of the lights
    public bool initialLightOn;
    public bool targetLightOn;

    // Colliders on/off
    public GameObject collisionLights;
    public GameObject collisionInitial;


    public void Start () {
        collisionLights = GameObject.Find("CubeLight");
        collisionInitial = GameObject.Find("CubeInitial");
    }


    protected override void OnStart() {
        // clear counters
        waveCounter = 0;
        lateWaves = 0;
        correctWaves = 0;
        incorrectWaves = 0;
    }


    public void HandleEvent(WaveEvents ev) {
        Debug.Log ("Event " + ev.ToString());

        if (!IsStarted())
            return;

        switch (GetState()) {
            case WaveStates.Initial:
                if (ev == WaveEvents.Delay && initialLightOn) {
                    initialLight.activeMaterial = 1;
                    collisionInitial.SetActive(true);
                } else if (ev == WaveEvents.Wave_Initial){
                    WriteLog("Initial Waved");

                    ChangeState(WaveStates.Target);
                }
                break;

            case WaveStates.Target:
                if (ev == WaveEvents.Delay && targetLightOn) {
                    // Turn on random target light
                    currentLight = UnityEngine.Random.Range(0, lights.Length);

                    WriteLog("Light: " + currentLight);

                    lights[currentLight].activeMaterial = 1;
                    collisionLights.SetActive(true);
                }
                else if ((int)ev == currentLight)
                {
                    WriteLog("Waved correctly");

                    correctWaves++;
                    ChangeState(WaveStates.Waved);
                }
                else if ((int)ev != currentLight && ev != WaveEvents.Wave_Initial) {
                    WriteLog("Waved incorrectly");

                    incorrectWaves++;
                    ChangeState(WaveStates.Waved);
                }
                break;

            case WaveStates.Waved:
                break;

            case WaveStates.ToOrigin:
                break;

            case WaveStates.TooLate:
                ChangeState(WaveStates.Waved);
                break;

            case WaveStates.EndWaving:
                if (ev == WaveEvents.Wave_Initial)
                    trialController.HandleEvent(TrialEvents.WavingFinished);
                break;

        }
    }


    public void Update () {
        if (!IsStarted())
            return;

        switch (GetState()) {
            case WaveStates.Initial:
                if (GetTimeInState() > 0.5f && !initialLightOn) {
                    initialLightOn = true;
                    HandleEvent(WaveEvents.Delay);
                }
                break;

            case WaveStates.Target:
                // Wait between the lights turning on and off
                if (GetTimeInState() > 0.5f && !targetLightOn) {
                    targetLightOn = true;
                    HandleEvent(WaveEvents.Delay);
                }
                if (GetTimeInState () > 6.0f && targetLightOn) {
                    WriteLog("Waved Late");
                    lateWaves++;
                    
                    ChangeState(WaveStates.TooLate);

                }
                break;

            case WaveStates.Waved:
                if (GetTimeInState() > 1.0f) {
                    if (waveCounter < wavesRequired) {
                        ChangeState(WaveStates.Initial);
                    } else {
                        ChangeState(WaveStates.ToOrigin);
                    }
                }
                break;

            case WaveStates.ToOrigin:
                if (GetTimeInState() > 0.5f)
                    ChangeState(WaveStates.EndWaving);
                break;

            case WaveStates.TooLate:
                break;

        }
    }


    protected override void OnEnter(WaveStates oldState) {

        switch (GetState()) {
            case WaveStates.Initial:
                break;

            case WaveStates.Target:
                waveCounter++;
                break;

            case WaveStates.Waved:
                break;

            case WaveStates.ToOrigin:

                break;

            case WaveStates.TooLate:
                ChangeState(WaveStates.Waved);
                break;

            case WaveStates.EndWaving:
                initialLight.activeMaterial = 2;
                collisionInitial.SetActive(true);
                break;
        }
    }


    protected override void OnExit(WaveStates newState) {
        switch (GetState()) {
            case WaveStates.Initial:
                TurnOffInitial();
                break;

            case WaveStates.Target:
                TurnOffTarget();
                break;

            case WaveStates.Waved:
                break;

            case WaveStates.TooLate:
                break;

            case WaveStates.ToOrigin:
                break;

            case WaveStates.EndWaving:
                TurnOffInitial();
                break;
        }
    }


    public void TurnOffInitial() {
        initialLight.activeMaterial = 0;
        collisionInitial.SetActive(false);
        initialLightOn = false;
    }


    public void TurnOffTarget() {
        collisionLights.SetActive(false);
        lights[currentLight].activeMaterial = 0;
        targetLightOn = false;
    }
}
