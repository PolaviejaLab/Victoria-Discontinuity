using UnityEngine;
using System.Collections;

public enum DriftEvents {
    Start, 
    ButtonPressed,
    Stopped, 
};

public enum DriftStates {
    Idle, 
    Moving, 
    Measured, 
    Finished,
};


public class PropDriftController : ICStateMachine<DriftStates, DriftEvents> {
    public ExperimentController experimentController;
    public TrialController trialController;

    public GameObject marker;
    public GameObject pointer;

    public bool markerOn;

    public bool pointerMove;
    public bool dirRight;
    public bool isMeasured;

    public float speed;

    public float proprioceptiveDrift;

    public Transform handTransform;

    private Vector3 pointerPosition;

    public Vector3 handPosition;


    new public void Start() {
        marker = GameObject.Find("Marker");
        pointer = GameObject.Find("Pointer");

        marker.SetActive(false);

        pointerPosition = new Vector3(pointer.transform.localPosition.x,
            pointer.transform.localPosition.y,
            pointer.transform.localPosition.z);
    }


    protected override void OnStart() {
        proprioceptiveDrift = 0;
        isMeasured = false;
    }


    public void HandleEvent(DriftEvents ev) {
        Debug.Log("Event " + ev.ToString());

        switch (GetState()) {
            case DriftStates.Idle:
                if (ev == DriftEvents.Start && markerOn) {
                    ChangeState(DriftStates.Moving);
                }
                break;

            case DriftStates.Moving:
                if (ev == DriftEvents.ButtonPressed) {
                    MeasureProprioceptiveDrift();
                    ChangeState(DriftStates.Measured);
                }
                break;

            case DriftStates.Measured:
                break;

        }
    }


    public void Update() {
        if (!IsStarted())
            return;

        switch (GetState()) {
            case DriftStates.Idle:
                if (GetTimeInState() > 1.5f) {
                    marker.SetActive(true);
                    HandleEvent(DriftEvents.Start);
                }
                break;

            case DriftStates.Moving:
                if (pointerMove) {
                    StartMarker();
                }
                
                break;

            case DriftStates.Measured:
                if (GetTimeInState() > 1.0f && !isMeasured) {
                    pointer.transform.localPosition = pointerPosition;
                    trialController.HandleEvent(TrialEvents.DriftMeasured);
                    isMeasured = true;
                }
                    
                break;

        }
    }


    protected override void OnEnter(DriftStates oldState) {

        switch (GetState()) {
            case DriftStates.Idle:
                
                break;

            case DriftStates.Moving:
                speed = 0.04f;
                dirRight = true;
                pointerMove = true;
                break;

            case DriftStates.Measured:
                marker.SetActive(false);
                break;

        }
    }


    protected override void OnExit(DriftStates newState) {
        switch (GetState()) {
            case DriftStates.Idle:
                break;

            case DriftStates.Moving:
                break;

            case DriftStates.Measured:
                break;
        }
    }


    public void StartMarker() {
        Vector3 movement = new Vector3(0, 0, 1);

        // marker moving from left to right in the x axis
        if (dirRight) {
            pointer.transform.Translate(movement * speed * Time.deltaTime);
            if (pointer.transform.localPosition.z >= 0.28f)
                dirRight = false;
        } else {
            // change to the opposite direction along the axis. 
            pointer.transform.Translate(-movement * speed * Time.deltaTime);
            if (pointer.transform.localPosition.z <= -0.28f)
                dirRight = true;
        }
    }

    // Method that will be called when proprioceptive drift needs to be measured
    public float MeasureProprioceptiveDrift() {
        speed = 0.0f;
        proprioceptiveDrift += pointer.transform.localPosition.z;
        handPosition = handTransform.position;

        return proprioceptiveDrift;
    }
}