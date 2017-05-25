using UnityEngine;
using System.Collections;

public enum MeasureEvents
{
    Wave_Started, 
    Wave_Finished,
};

public enum MeasureStates
{
    Idle,
    WaitingToStart,
    Measuring, 
    Finished,
};


public class ImplicitMeasure : ICStateMachine<MeasureStates, MeasureEvents>
{
    public TrialController trialController;
    public OffsetSwitcher offsetSwitcher;

    public GameObject measure;

    public MaterialChanger startLight;
    public MaterialChanger finishLight;

    public GameObject startCollision;
    public GameObject finishCollision;

    public bool startLightOn;
    public bool finishLightOn;

    public float updatedDisplacement = 0;


    new public void Start()
    {
    }


    protected override void OnStart()
    {
        Debug.Log("Measure Machine Started");
    }


    public void HandleEvent(MeasureEvents ev)
    {
        if (!IsStarted())
            return;

        Debug.Log("Event " + ev.ToString());

        switch (GetState())
        {
            case MeasureStates.Idle:
                break;

            case MeasureStates.WaitingToStart:
                if (ev == MeasureEvents.Wave_Started && startLightOn) {
                    ChangeState(MeasureStates.Measuring);
                }
                break;

            case MeasureStates.Measuring:
                if (ev == MeasureEvents.Wave_Finished && finishLightOn) {
                    ChangeState(MeasureStates.Finished);
                }
                break;
        }
    }


    public void Update()
    {
        if (!IsStarted())
            return;

        switch (GetState())
        {
            case MeasureStates.Idle:
                if (GetTimeInState() > 1.5f)
                    ChangeState(MeasureStates.WaitingToStart);
                break;

            case MeasureStates.Measuring:
                offsetSwitcher.displaceHand(updatedDisplacement);
                updatedDisplacement += 0.00001f;
                break;

            case MeasureStates.Finished:
                if (GetTimeInState() > 0.5f) {
                    trialController.HandleEvent(TrialEvents.MeasureDone);
                    measure.SetActive(false);
                    offsetSwitcher.noDisplacement();
                    this.StopMachine();


                }           
                break;
        }
    }


    protected override void OnEnter(MeasureStates oldState)
    {

        switch (GetState())
        {
            case MeasureStates.Idle:
                break;

            case MeasureStates.WaitingToStart:
                TurnOnStartLight();
                break;

            case MeasureStates.Measuring:
                TurnOnFinishLight();
                WriteLog("Implicit Measure Started");
                // Code here that is going to do the changes in the Virtual hand
                // Be sure that it records both the real and the virtual hand positions. 
                
                break;

            case MeasureStates.Finished:
                WriteLog("Implicit Measure Finished"); 
                break;
        }
    }


    protected override void OnExit(MeasureStates newState)
    {
        switch (GetState())
        {
            case MeasureStates.Idle:
                measure.SetActive(true);
                break;

            case MeasureStates.WaitingToStart:
                TurnOffStartLight();
                break;

            case MeasureStates.Measuring:
                TurnOffFinishLight();
                break;
        }
    }

    public void TurnOnStartLight() {
        startLight.activeMaterial = 1;      
        startLightOn = true;        
    }

    public void TurnOffStartLight() {
        startLight.activeMaterial = 0;
        startLightOn = false;
    }

    public void TurnOnFinishLight() {
        finishLight.activeMaterial = 1;
        finishLightOn = true;

    }

    public void TurnOffFinishLight() {
        finishLight.activeMaterial = 0;
        finishLightOn = false;
    }
}