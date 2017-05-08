using UnityEngine;
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
    QuestionAnswered, 
    ThreatDone,
};

/**
* States of the Wave State Machine
*/
public enum WaveStates
{
    Initial,                    // Initial Light
    Target,                     // Target Light
    CorrectWave,                // Waved Correctly
    IncorrectWave,              // Waved Incorrectly
    TooLate,                    // Waved too late
    Waved,                      // Finished the wave
    Question,                   // Show the questionnaire and Pause the experiment
    //Threat,                     // Threat to the virtual hand
    //ToOrigin,                   // Go back to origin after the 
    EndWaving,                  // Task finished
};


public class WaveController : ICStateMachine<WaveStates, WaveEvents>
{

    // Reference to other classes
    public TrialController trialController;
    public Threat threatController;

    // Initial and subsequent lights
    public MaterialChanger initialLight;
    public MaterialChanger[] lights;
    public MaterialChanger display;

    // Parameters for the waving
    public int wavesRequired;
    public int waveThreat;

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

    // QUestionnaire canvas
    public GameObject Questionnaire;

    public void Start()
    {
        collisionLights = GameObject.Find("CubeLight");
        collisionInitial = GameObject.Find("CubeInitial");

        threatController.Stopped += (obj, ev) => HandleEvent(WaveEvents.ThreatDone);
    }


    protected override void OnStart()
    {
        // clear counters
        waveCounter = 0;
        lateWaves = 0;
        correctWaves = 0;
        incorrectWaves = 0;

        trialController.threatened = false;
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

                    //WriteLog("Waved incorrectly"); // uncomment for type of noise 2 that send directly to incorrecy wave
                    //incorrectWaves++;
                    //ChangeState(WaveStates.IncorrectWave);
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

        switch (GetState())
        {
            case WaveStates.Initial:
                if (GetTimeInState() > 0.5f && !initialLightOn)
                {
                    initialLightOn = true;
                    HandleEvent(WaveEvents.Delay);
                }
                break;

            case WaveStates.Target:
                // Wait between the lights turning on and off
                if (GetTimeInState() > 0.5f && !targetLightOn)
                {
                    targetLightOn = true;
                    HandleEvent(WaveEvents.Delay);
                }
                if (GetTimeInState() > 5.0f && targetLightOn)
                {
                    WriteLog("Waved Late");
                    lateWaves++;

                    ChangeState(WaveStates.TooLate);
                }
                break;

            case WaveStates.CorrectWave:
                if (GetTimeInState() > 0.75f)
                    ChangeState(WaveStates.Waved);
                break;

            case WaveStates.TooLate:
                break;

            case WaveStates.IncorrectWave:
                if (GetTimeInState() > 0.75f)
                    ChangeState(WaveStates.Waved);
                break;

            case WaveStates.Waved:
                if (GetTimeInState() > 1.5f)
                {
                    if (waveCounter < wavesRequired)
                    {
                        if (waveCounter % 4 == 0) {
                            ChangeState(WaveStates.Question);
                        }                   
                        else
                            ChangeState(WaveStates.Initial);
                    }
                    else {
                        if (GetTimeInState() > 0.5f)
                            ChangeState(WaveStates.EndWaving);
                    }
                }
                break;

            case WaveStates.Question:
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    Questionnaire.SetActive(false);
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
                TurnOffTarget();
                display.activeMaterial = 1; 
                break;

            case WaveStates.IncorrectWave:
                TurnOffTarget();
                display.activeMaterial = 2;
                break;

            case WaveStates.Waved:
                TurnOffTarget();
                display.activeMaterial = 0;
                if (waveThreat == waveCounter && trialController.knifePresent)
                {
                    threatController.StartMachine();
                    threatController.HandleEvent(ThreatEvent.ReleaseThreat);
                }
                break;

            case WaveStates.TooLate:
                ChangeState(WaveStates.IncorrectWave);
                break;


            case WaveStates.Question:
                Questionnaire.SetActive(true);
                break;

            case WaveStates.EndWaving:
                initialLight.activeMaterial = 2;
                collisionInitial.SetActive(true);
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

            case WaveStates.EndWaving:
                TurnOffInitial();
                break;
        }
    }


    public void TurnOffInitial()
    {
        initialLight.activeMaterial = 0;
        collisionInitial.SetActive(false);
        initialLightOn = false;
    }


    public void TurnOffTarget()
    {
        lights[currentLight].activeMaterial = 0;
        collisionLights.SetActive(false);
        targetLightOn = false;
    }
}
