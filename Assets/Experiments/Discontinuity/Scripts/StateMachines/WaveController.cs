﻿using UnityEngine;
using System.Collections;
using System;

/**
* Events handled by the Wave State Machine
*/

public enum WaveEvents
{
    Wave_0 = 0,
    Wave_1 = 1,
    Wave_Initial,
    Delay,

    ThreatDone,
};

/**
* States of the Wave State Machine
*/
public enum WaveStates
{
    Initial,
    Target,
    CorrectWave,
    IncorrectWave,
    Waved,
    Feedback,
    TooLate,
    Question, 
    Threat,
    EndWaving,
};


public class WaveController : ICStateMachine<WaveStates, WaveEvents>
{
    // Reference to other classes
    public TrialController trialController;
    public Threat threatController;
    public HandSwitcher handSwitcher;

    // Initial and subsequent lights
    public MaterialChanger initialLight;
    public MaterialChanger[] lights;
    public MaterialChanger feedbackScreen;

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
    public float collisionProbability;
    public float randomProbability;

    // threat
    public int waveThreat;
    public bool knifePresent;
    public GameObject threat;

    // Questionnaire
    public GameObject Questionnaire;

    public void Start()
    {
        collisionLights = GameObject.Find("CubeLight");
        collisionInitial = GameObject.Find("CubeInitial");
    }


    protected override void OnStart()
    {
        threatController.Stopped += (obj, ev) => HandleEvent(WaveEvents.ThreatDone);

        // clear counters
        waveCounter = 0;
        lateWaves = 0;
        correctWaves = 0;
        incorrectWaves = 0;

        waveThreat = UnityEngine.Random.Range(2, wavesRequired - 1);
        WriteLog("Threat wave is: " + waveThreat);
    }


    public void HandleEvent(WaveEvents ev)
    {
        Debug.Log("Event " + ev.ToString());

        if (!IsStarted())
            return;

        switch (GetState())
        {
            case WaveStates.Initial:
                if (ev == WaveEvents.Delay && initialLightOn)
                {
                    initialLight.activeMaterial = 1;
                    collisionInitial.SetActive(true);
                }
                else if (ev == WaveEvents.Wave_Initial)
                {
                    WriteLog("Initial Waved");

                    ChangeState(WaveStates.Target);
                }
                break;

            case WaveStates.Target:
                if (ev == WaveEvents.Delay && targetLightOn)
                {
                    // Turn on random target light
                    currentLight = UnityEngine.Random.Range(0, lights.Length);

                    WriteLog("Light: " + currentLight);

                    lights[currentLight].activeMaterial = 1;
                    collisionLights.SetActive(true);
                }
                else if ((int)ev == currentLight && randomProbability <= collisionProbability)
                {
                    WriteLog("Probability for wave" + waveCounter + " is " + randomProbability);
                    WriteLog("Waved correctly");

                    correctWaves++;
                    ChangeState(WaveStates.CorrectWave);
                }
                else if ((int)ev == currentLight && randomProbability > collisionProbability) {
                    WriteLog("Probability for wave" + waveCounter + " is " + randomProbability);

                    WriteLog("Not waved"); // Uncomment for noise-type 2.2, that waits until timeout
                }
                else if ((int)ev != currentLight && ev != WaveEvents.Wave_Initial)
                {
                    WriteLog("Waved incorrectly");
                    incorrectWaves++;
                    ChangeState(WaveStates.IncorrectWave);
                }
                break;

            case WaveStates.Waved:
                break;

            case WaveStates.TooLate:
                ChangeState(WaveStates.Waved);
                break;

            case WaveStates.Question:
                break;

            case WaveStates.Threat:
                if (ev == WaveEvents.ThreatDone)
                    ChangeState(WaveStates.Initial);
                break;

            case WaveStates.EndWaving:
                if (ev == WaveEvents.Wave_Initial)
                    trialController.HandleEvent(TrialEvents.WavingFinished);
                break;
        }
    }


    public void Update()
    {
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
                if (GetTimeInState() > 3.0f && targetLightOn)
                {
                    WriteLog("Waved Late");
                    lateWaves++;

                    ChangeState(WaveStates.TooLate);
                }
                break;

            case WaveStates.CorrectWave:
                if (GetTimeInState() > 0.5f)
                    ChangeState(WaveStates.Waved);
                break;

            case WaveStates.IncorrectWave:
                if (GetTimeInState() > 0.5f)
                    ChangeState(WaveStates.Waved);
                break;

            case WaveStates.Waved:
                if (GetTimeInState() > 1.5f) {
                    if (waveCounter < wavesRequired) {
                        if (waveCounter == waveThreat && wavesRequired % waveCounter != 0) {
                            ChangeState(WaveStates.Threat);
                        }
                        else if (wavesRequired % waveCounter == 0) {
                            ChangeState(WaveStates.Question);
                        }
                        else {
                            ChangeState(WaveStates.Initial);
                        }
                    }
                    else {
                        if (GetTimeInState() > 0.5f)
                            ChangeState(WaveStates.EndWaving);
                    }
                }
                break;

            case WaveStates.TooLate:
                break;

            case WaveStates.Question:
                if (Input.GetKeyDown(KeyCode.Return)) {
                    ChangeState(WaveStates.Initial);
                }
                break;

        }
    }


    protected override void OnEnter(WaveStates oldState)
    {
        switch (GetState())
        {
            case WaveStates.Initial:
                break;

            case WaveStates.Target:
                waveCounter++;
                break;

            case WaveStates.CorrectWave:
                feedbackScreen.activeMaterial = 1;
                break;

            case WaveStates.IncorrectWave:
                feedbackScreen.activeMaterial = 2;
                break;

            case WaveStates.Waved:
                feedbackScreen.activeMaterial = 0;
                TurnOffTarget();
                break;

            case WaveStates.TooLate:
                ChangeState(WaveStates.IncorrectWave);
                break;

            case WaveStates.Threat:
                if (knifePresent == true)
                {
                    threatController.StartMachine();
                    threatController.HandleEvent(ThreatEvent.ReleaseThreat);
                }
                else {
                    ChangeState(WaveStates.Initial);
                }
                break;

            case WaveStates.EndWaving:
                TurnOnInitial();
                break;

            case WaveStates.Question:
                Questionnaire.SetActive(true);
                break;
        }
    }


    protected override void OnExit(WaveStates newState)
    {
        switch (GetState())
        {
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

            case WaveStates.Threat:
                threatController.StopMachine();
                break;

            case WaveStates.Question:
                Questionnaire.SetActive(false);
                break;

            case WaveStates.EndWaving:
                TurnOffInitial();
                break;
        }
    }

    public void TurnOnInitial() {
        initialLight.activeMaterial = 1;
        collisionInitial.SetActive(true);
        initialLightOn = true;
    }
    
    public void TurnOffInitial() {
        initialLight.activeMaterial = 0;
        collisionInitial.SetActive(false);
        initialLightOn = false;
    }

    public void TurnOnTarget() {
        collisionLights.SetActive(true);
        lights[currentLight].activeMaterial = 1;
        targetLightOn = true;
    }

    public void TurnOffTarget() {
        collisionLights.SetActive(false);
        lights[currentLight].activeMaterial = 0;
        targetLightOn = false;
    }
}
