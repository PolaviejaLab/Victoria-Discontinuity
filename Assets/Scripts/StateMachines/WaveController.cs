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
    WaitForInitial,
    WaitForWave, 
    Waved,
    ToOrigin,
    TooLate,
    EndWaving, 
    WavesFinished, 
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


    protected override void OnStart()
    {
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
            case WaveStates.WaitForInitial:
                if (ev == WaveEvents.Delay && initialLightOn) {
                    initialLight.activeMaterial = 1;
                    collisionInitial.SetActive(true);
                } else if (ev == WaveEvents.Wave_Initial){
                    ChangeState(WaveStates.WaitForWave);
                }
                break;

            case WaveStates.WaitForWave:
                if (ev == WaveEvents.Delay && targetLightOn)
                {
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

            case WaveStates.EndWaving:
                // if (ev == WaveEvents.Wave_Initial)
                // send an event to trial event that waving has finished   
                break;

            case WaveStates.Waved:
                break;

            case WaveStates.TooLate:
                break;

            case WaveStates.WavesFinished:
                break;
        }
    }


    public void Update () {
        if (!IsStarted())
            return;

        switch (GetState()) {
            case WaveStates.WaitForInitial:
                if (GetTimeInState() > 0.5f && !initialLightOn) {
                    initialLightOn = true;
                    HandleEvent(WaveEvents.Delay);
                }
                break;

            case WaveStates.WaitForWave:
                // Wait between the lights turning on and off
                if (GetTimeInState() > 0.5f && !targetLightOn) {
                    targetLightOn = true;
                    HandleEvent(WaveEvents.Delay);
                }
                break;

            case WaveStates.Waved:
                if (GetTimeInState() > 1.0f) {
                    if (waveCounter < wavesRequired) {
                        ChangeState(WaveStates.WaitForInitial);
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

            case WaveStates.WavesFinished:
                break;
        }
    }


    protected override void OnEnter(WaveStates oldState) {
        switch (GetState()) {
            case WaveStates.WaitForInitial:
                break;

            case WaveStates.WaitForWave:
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

            case WaveStates.WavesFinished:
                trialController.HandleEvent(TrialEvents.WavingFinished);
                break;
        }
    }


    protected override void OnExit(WaveStates newState) {
        switch (GetState()) {
            case WaveStates.WaitForInitial:
                TurnOffInitial();
                break;

            case WaveStates.WaitForWave: // also transform into a method.
                collisionLights.SetActive(false);
                lights[currentLight].activeMaterial = 0;
                targetLightOn = false;
                break;

            case WaveStates.Waved:
                break;

            case WaveStates.TooLate:
                break;

            case WaveStates.ToOrigin:
                break;

            case WaveStates.WavesFinished:
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
        // use what is used to turn of Initial
    }
}
