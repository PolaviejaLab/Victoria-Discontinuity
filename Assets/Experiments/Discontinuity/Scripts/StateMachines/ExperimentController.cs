﻿/**
 * Main state machine controlling the flow of the experiment.
 *  This state machine is responsible for loading trial descriptions,
 *  running the appropriate trials in order and saving the results
 *  back to the data file.
 *
 * Logic pertaining to the flow of a trial should be placed in the
 *  trial state machine.
 */
using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.Collections;
using System.Collections.Generic;

using Leap;
using Leap.Unity;

public enum ExperimentEvents
{
    ProtocolLoaded,
    TrialFinished,
    QuestionsFinished,
    ExperimentFinished,
};


public enum ExperimentStates
{
    WaitingForFeedback,
    Start,
    Trial,
    Questionnaires,
    Finished,
};


public class ExperimentController : ICStateMachine<ExperimentStates, ExperimentEvents>
{
    /**
     * Link to the Trial state machine which is responsible for the flow of a single trial.
     */
    public TrialController trialController;
    public InactiveTrialController inactiveTrialController;

    /**
     * Links to state machines used by the Trial state machine (TrialController)
     * we need those here in order to load variables from the protocol file into those
     * state machines.
     *
     * FIXME: This is undesirable behaviour as it breaks the hierarchical organization.
     * Instead, we should tell the Trial state machine which should set variables on the
     * child machines.
     */
    public WaveController waveController;
    public PropDriftController driftController;
    public Threat threatController;

    public HandController handController;
    public HandSwitcher handSwitcher;

    public TableLights tableLights;

    private ICTrialList trialList;
    public string protocolFile;

    private string outputDirectory;

    private int participantNumber;
    private string participantName;

    public int trialCounter;
    public int randomProtocol;

    private bool trialEmpty;


    public void Start()
    {
        // When the trial controller is stopped, invoke an event
        trialController.Stopped +=
            (sender, e) =>
                { HandleEvent(ExperimentEvents.TrialFinished); };

        this.StartMachine();
    }


    public void HandleEvent(ExperimentEvents ev)
    {
        WriteLog("Event " + ev.ToString());

        if (!IsStarted())
            return;

        switch (GetState())
        {
            case ExperimentStates.WaitingForFeedback:
                break;

            case ExperimentStates.Start:
                if (ev == ExperimentEvents.ProtocolLoaded)
                    ChangeState(ExperimentStates.Trial);
                break;


            case ExperimentStates.Trial:
                if (ev == ExperimentEvents.TrialFinished)
                {
                    SaveTrialResult();
                    ChangeState(ExperimentStates.Questionnaires);
                }
                break;

            case ExperimentStates.Questionnaires:
                if (ev == ExperimentEvents.QuestionsFinished)
                    ChangeState(ExperimentStates.Trial);
                break;


        }
    }


    public void Update()
    {
        if (!IsStarted())
            return;

        // Change the gender of the hand
        if (Input.GetKeyDown(KeyCode.F))
        {
            handSwitcher.useMale = false;
            WriteLog("Gender changed to female");
        }
        else if (Input.GetKeyDown(KeyCode.M))
        {
            handSwitcher.useMale = true;
            WriteLog("Gender changed to male");
        }

        switch (GetState()) {
            case ExperimentStates.WaitingForFeedback:
                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    ChangeState(ExperimentStates.Start);
                }
                else if (Input.GetKeyDown(KeyCode.Escape))
                {
                    ChangeState(ExperimentStates.Finished);
                }
                break;

            case ExperimentStates.Questionnaires:
                if (Input.GetKeyDown(KeyCode.W))
                    HandleEvent(ExperimentEvents.QuestionsFinished);
                break;
        }
    }


    protected override void OnEnter(ExperimentStates oldState)
    {
        switch (GetState())
        {
            case ExperimentStates.WaitingForFeedback:
                break;

            case ExperimentStates.Start:
                string[] dir = Directory.GetDirectories("Results");

                participantNumber = 1;
                participantName = "Participant" + participantNumber.ToString();

                trialCounter = 0;

                for (int i = 0; i < dir.Length; i++)
                {
                    outputDirectory = "Results/CCUSubjects" + participantNumber.ToString();
                    if (!Directory.Exists(outputDirectory))
                    {
                        Directory.CreateDirectory(outputDirectory);
                        break;
                    }
                    else {
                        participantNumber = participantNumber + 1;
                    }
                }

                logger.OpenLog(GetLogFilename());

                // Record participant number to log-file
                WriteLog("Participant" + participantNumber.ToString());
                if (!handSwitcher.useMale)
                {
                    WriteLog("Hand model is female");
                }
                else if (handSwitcher.useMale)
                {
                    WriteLog("Hand model is male");
                }

                string[] dirProtocol = Directory.GetFiles("Protocol/Exp1_Frontiers");

                randomProtocol = UnityEngine.Random.Range(0, dirProtocol.Length);
                protocolFile = dirProtocol[randomProtocol];

                // Load protocol
                Debug.Log("Loading protocol: " + protocolFile);
                trialList = new ICTrialList(protocolFile);
                WriteLog("Loaded " + trialList.Count() + " trials");

                HandleEvent(ExperimentEvents.ProtocolLoaded);

                break;

            case ExperimentStates.Trial:
                if (trialList.HasMore()) {
                    trialCounter++;
                    StartTrial();
                }
                else {
                    ChangeState(ExperimentStates.WaitingForFeedback);
                }
                break;

            case ExperimentStates.Questionnaires:
                handSwitcher.showRightHand = false;
                break;

            case ExperimentStates.Finished:
                StopMachine();
                WriteLog("No more subjects, stopping machine");
                break;
        }
    }


    protected override void OnExit(ExperimentStates newState)
    {
        switch (GetState())
        {
            case ExperimentStates.Trial:
                break;
        }
    }


    /**
     * Parse trial details and pass them to the trial controller
     */
    private void PrepareTrial(Dictionary<string, string> trial, TrialController trialController)
    {
        // Determine which hand to use for given gapsize
        if (trial["GapStatus"] == "Inactive") {
            trialController.hand = 0;
            inactiveTrialController.hand = 0;
        }
        else if (trial["GapStatus"] == "Active") {
            trialController.hand = 1;
            inactiveTrialController.hand = 1;
        }
        
        else {
            WriteLog("Invalid GapSize in protocol");
            trialController.hand = -1;
            inactiveTrialController.hand = -1;
        }

        WriteLog("Gap: " + trial["GapStatus"]);

        // Get offset
        float offset;
        try {
            float.TryParse(trial["HandOffset"], out offset);
            trialController.offset = offset / 100.0f;
        } catch(Exception e) {
            throw new Exception("Could not parse HandOffset in ProtocolFile");
        }

        WriteLog("HandOffset: " + offset);

        // Determine the number of waves per each trial
        int wavesRequired;
        int.TryParse(trial["WavesRequired"], out wavesRequired);
        waveController.wavesRequired = wavesRequired;


        // Noise level
        if (trial.ContainsKey("NoiseLevel"))
        {
            float noiseLevel;
            float.TryParse(trial["NoiseLevel"], out noiseLevel);
            trialController.noiseLevel = noiseLevel;
            WriteLog("NoiseLevel: " + noiseLevel);
        }
        else {
            trialController.noiseLevel = 0.0f;
        }

        if (trial.ContainsKey("LNoise")) {
            float lNoise;
            float.TryParse(trial["LNoise"], out lNoise);
            trialController.lNoise = lNoise;
            WriteLog("Lambda: " + lNoise);
        }
        else {
            trialController.lNoise = 0.0f;
        }

        // Knife
        if (trial.ContainsKey("KnifePresent"))
        {
            if (trial["KnifePresent"].ToLower() == "true")
                trialController.knifePresent = true;
            else if (trial["KnifePresent"].ToLower() == "false")
                trialController.knifePresent = false;
            else
                throw new Exception("Invalid value in trial list for field KnifePresent");

            WriteLog("Knife Present" + trialController.knifePresent);
        }

        // Knife Offset
        if (trial.ContainsKey("KnifeOffset"))
        {
            float knifeOffsetx; float knifeOffsety; float knifeOffsetz;
            float.TryParse(trial["OffsetX"], out knifeOffsetx);
            float.TryParse(trial["OffsetY"], out knifeOffsety);
            float.TryParse(trial["OffsetZ"], out knifeOffsetz);

            Vector3 knifeVector = new Vector3(knifeOffsetx, knifeOffsety, knifeOffsetz);

            trialController.knifeOffset = knifeVector;

            WriteLog("Knife Offset: " + knifeVector);
        }

        // Knife
        if (trial.ContainsKey("KnifeOnReal")) {
            if (trial["KnifeOnReal"].ToLower() == "true") {
                threatController.knifeOnReal = true; 
            } else if (trial["KnifeOnReal"].ToLower() == "false") {
                threatController.knifeOnReal = false; 
            }

            else
                throw new Exception("Invalid value in trial list for field KnifeOnReal");
            
            WriteLog("Knife on Real" + threatController.knifeOnReal);
        }      
    }


    /**
	 * Start the next trial
	 */
    private void StartTrial()
    {
        // Do not start if already running
        if (trialController.IsStarted())
            return;

        WriteLog("Starting trial");

        // Get next trial from list and setup trialController
        Dictionary<string, string> trial = trialList.Pop();
        PrepareTrial(trial, trialController);

        handController.StartRecording(GetLEAPFilename(trialCounter));

        // Turn table lights on
        tableLights.isOn = true;
        handSwitcher.ignoreUpdatesRight = false;

        if (trial.ContainsKey("IgnoreUpdate")) {
            if (trial["IgnoreUpdate"].ToLower() == "true") {
                inactiveTrialController.initialState = InactiveTrialStates.AccomodationTime;
                handSwitcher.ignoreUpdatesRight = true;
                inactiveTrialController.StartMachine();               
            }
            else if (trial["IgnoreUpdate"].ToLower() == "false") {
                trialController.initialState = TrialStates.AccomodationTime;
                //handSwitcher.ignoreUpdatesRight = false;
                trialController.StartMachine();
            } else {
                throw new Exception("Invalid value for IgnoreUpdate");
            }

            WriteLog("Right hand still " + handSwitcher.ignoreUpdatesRight);
        }

        
    }


    private string GetLEAPFilename(int trial)
    {
        return outputDirectory + "\\" + DateTime.UtcNow.ToString("yyyy-MM-dd hh.mm ") + participantName + " Trial " + trial + ".csv";
    }


    private string GetLogFilename()
    {
        return outputDirectory + "\\" + DateTime.UtcNow.ToString("yyyy-MM-dd hh.mm ") + participantName + ".log";
    }


    private string GetResultsFilename()
    {
        return outputDirectory + "\\" + DateTime.UtcNow.ToString("yyyy-MM-dd hh.mm ") + participantName + ".csv";
    }


    /**
	 * Appends the result of the previous trial to the datafile
	 */
    private void SaveTrialResult()
    {
        StreamWriter writer = new StreamWriter(GetResultsFilename(), true);

        // Append result of trial to data file
        writer.Write(trialCounter);
        writer.Write(", ");

        if (trialController.hand == 0)
            writer.Write("Continuous Limb, ");
        else if (trialController.hand == 1)
            writer.Write("Discontinous Limb, ");
        else
            writer.Write("Gap unknown, ");

        if (trialController.knifePresent == true)
            writer.Write("Threat active, ");
        else if (trialController.knifePresent == false)
            writer.Write("Threat inactive, ");
        else
            writer.Write("Threat unknown, ");

        if (trialController.noiseLevel == 0)
            writer.Write("Hand noise inactive, ");
        else
            writer.Write("Hand noise active, ");

        if (threatController.knifeOnReal)
            writer.Write("Knife on the real hand, ");
        else if (!threatController.knifeOnReal)
            writer.Write("Knife on the virtual hand, ");


        writer.Write(trialController.offset);
        writer.Write(", ");
        writer.Write(waveController.waveCounter);
        writer.Write(", ");
        writer.Write(waveController.correctWaves);
        writer.Write(", ");
        writer.Write(waveController.incorrectWaves);
        writer.Write(", ");
        writer.Write(waveController.lateWaves);
        writer.Write(", ");
        writer.Write(driftController.proprioceptiveDrift);
        writer.Write(", ");
        writer.WriteLine();

        writer.Close();
    }

    private void SaveExperimentResult()
    {
        handController.StopRecording();
    }
}
