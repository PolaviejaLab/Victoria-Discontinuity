/**
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
    public GetInfo expInfo;
    public getExperimentNumber expNumber;
    public WaveController waveController;

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

    public int noiseType;


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
                // participantName = "Participant" + participantNumber.ToString();

                trialCounter = 0;

                for (int i = 0; i < dir.Length; i++)
                {
                    // outputDirectory = "Results/PreparingExp2" + participantNumber.ToString();
                    outputDirectory = "E:/Data/Experiment_Rep/Unity_Files/" + expInfo.subjectCode;
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

                // WriteLog("Participant" + participantNumber.ToString());
                WriteLog("Participant: " + expInfo.subjectCode);

                if (!handSwitcher.useMale) {
                    WriteLog("Hand model is female");
                }
                else if (handSwitcher.useMale)
                {
                    WriteLog("Hand model is male");
                }

                // string[] dirProtocol = Directory.GetFiles("Protocol/Exp2_Trial");
                string[] dirProtocol = Directory.GetFiles("Protocol/" + expNumber.experimentName.ToString() + "/");


                randomProtocol = UnityEngine.Random.Range(0, dirProtocol.Length);
                protocolFile = dirProtocol[randomProtocol];


                // Load protocol
                Debug.Log("Loading protocol: " + protocolFile);
                WriteLog("Protocol file " + protocolFile);
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


        // Determine noise type
        if (trial.ContainsKey("NoiseType")) {
            int.TryParse(trial["NoiseType"], out noiseType);
            WriteLog("Noise type " + noiseType);
        }

        // Noise level
        if (trial.ContainsKey("NoiseLevel")) {
            float noiseLevel;
            float.TryParse(trial["NoiseLevel"], out noiseLevel);
            trialController.noiseLevel = noiseLevel;
            WriteLog("NoiseLevel: " + noiseLevel);
        }
        else {
            trialController.noiseLevel = 0.0f;
        }

        if (trial.ContainsKey("NoiseLambda")) {
            float noiseLambda;
            float.TryParse(trial["NoiseLambda"], out noiseLambda);
            trialController.lNoise = noiseLambda;
            WriteLog("Lambda: " + noiseLambda);
        }
        else {
            trialController.lNoise = 0.0f;
        }


        // Determine collision probability
        if (trial.ContainsKey("CollisionProbability"))
        {
            float collisionProbability;
            float.TryParse(trial["CollisionProbability"], out collisionProbability);
            waveController.collisionProbability = collisionProbability;
            WriteLog("Collision probability: " + collisionProbability);
        }
        else {
            waveController.collisionProbability = 1.0f;
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
        if (trial.ContainsKey("KnifeOffset")) {
            float knifeOffsetx; float knifeOffsety; float knifeOffsetz;
            float.TryParse(trial["OffsetX"], out knifeOffsetx);
            float.TryParse(trial["OffsetY"], out knifeOffsety);
            float.TryParse(trial["OffsetZ"], out knifeOffsetz);

            Vector3 knifeVector = new Vector3(knifeOffsetx, knifeOffsety, knifeOffsetz);

            trialController.knifeOffset = knifeVector;

            WriteLog("Knife Offset: " + knifeVector);
        }

        // Gender Change
        if (trial.ContainsKey("ChangeGender")) {
            if (trial["ChangeGender"].ToLower() == "true") {
                trialController.changeGender = true;
            } else if (trial["ChangeGender"].ToLower() == "false"){
                trialController.changeGender = false;
            }

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
            if (trial["IgnoreUpdate"].ToLower() == "false") {
                trialController.initialState = TrialStates.AccomodationTime;
                trialController.StartMachine();
            } else if (trial["IgnoreUpdate"].ToLower() == "true") {
                inactiveTrialController.initialState = InactiveTrialStates.AccomodationTime;
                inactiveTrialController.StartMachine(); 
            } else {
                throw new Exception("Invalid value for IgnoreUpdate");
            }

            WriteLog("Right hand still " + handSwitcher.ignoreUpdatesRight);
        }

        
    }


    private string GetLEAPFilename(int trial)
    {
        return outputDirectory + "\\" + DateTime.UtcNow.ToString("yyyy-MM-dd hh.mm ") + expInfo.subjectCode + " Trial " + trial + ".csv";
    }


    private string GetLogFilename()
    {
        return outputDirectory + "\\" + DateTime.UtcNow.ToString("yyyy-MM-dd hh.mm ") + expInfo.subjectCode + ".log";
    }


    private string GetResultsFilename()
    {
        return outputDirectory + "\\" + DateTime.UtcNow.ToString("yyyy-MM-dd hh.mm ") + expInfo.subjectCode + ".csv";
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

        
        if (noiseType == 0)
            writer.Write("Hand noise inactive, ");
        else if (noiseType == 1)
            writer.Write("Bodiliy noise active, ");
        else if (noiseType == 2)
            writer.Write("Outcome noise active, ");
        else if (noiseType == 3)
            writer.Write("Bodily and outcome noise active, ");
        else
            writer.Write("no noise information available, ");


        if (handSwitcher.useMale)
        {
            writer.Write("Male model, ");
        }
        else if (!handSwitcher.useMale) {
            writer.Write("Female model, ");
        }

       
        writer.Write(trialController.offset);
        writer.Write(", ");
        writer.Write(waveController.collisionProbability);
        writer.Write(", ");
        writer.Write(trialController.totWaves);
        writer.Write(", ");
        writer.Write(trialController.correctWaves);
        writer.Write(", ");
        writer.Write(trialController.incorrectWaves);
        writer.Write(", ");
        writer.Write(trialController.lateWaves);
        writer.Write(", ");
        writer.WriteLine();

        writer.Close();
    }

    private void SaveExperimentResult()
    {
        handController.StopRecording();
    }
}
