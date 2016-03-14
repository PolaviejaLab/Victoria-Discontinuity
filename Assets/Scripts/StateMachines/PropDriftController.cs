using UnityEngine;
using System.Collections;

public enum DriftEvents {
    Started, 
    Stopped, 
};

public enum DriftStates {
    Idle, 
    Moving, 
    Measured, 
};


public class PropDriftController : StateMachine <DriftStates, DriftEvents> {

    public ExperimentController experimentController;
    public TrialController trialController; 

    private GameObject marker;
    private GameObject pointer;

    public bool isStarted;

    public bool dirRight;
    private float speed;

    public float proprioceptiveDrift;

    public Transform handTransform;

    private float pointerx;
    private float pointery;
    private float pointerz;

    public Vector3 handPosition;


    void Start () {
        marker = GameObject.Find("Marker");
        pointer = GameObject.Find("Pointer");

        proprioceptiveDrift = 0;

        pointerx = pointer.transform.localPosition.x;
        pointery = pointer.transform.localPosition.y;
        pointerz = pointer.transform.localPosition.z;
    }


    public void HandleEvent(DriftEvents ev) {
        Debug.Log("Event " + ev.ToString());

        if (!IsStarted())
            return;

        switch (GetState()) {
            case DriftStates.Idle:
                if (ev == DriftEvents.Started)
                    ChangeState(DriftStates.Moving);
                break;

            case DriftStates.Moving:
                if (ev == DriftEvents.Stopped)
                    ChangeState(DriftStates.Measured);
                break;

            case DriftStates.Measured:
                if (GetTimeInState() > 1.0f) {
                    trialController.HandleEvent(TrialEvents.DriftMeasured);
                }
                break;
        }
    }



    void Update() {


        switch (GetState()) {
            case DriftStates.Idle:
                break;

            case DriftStates.Moving:
                if (Input.GetKeyDown(KeyCode.Space) && isStarted) {
                    MeasureProprioceptiveDrift();
                }
                break;

            case DriftStates.Measured:
                break;

        }

    }


    protected override void OnEnter(DriftStates oldState) {
        switch (GetState()) {
            case DriftStates.Idle:
                break;

            case DriftStates.Moving:
                marker.SetActive(true);
                speed = 0.04f;
                isStarted = true;
                StartMarker();
                break;

            case DriftStates.Measured:
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
                isStarted = false;
                marker.SetActive(false);

                // Restarts the pointer position to its original value (Start())
                pointer.transform.localPosition = new Vector3(pointerx, pointery, pointerz); // this should be a random vector3
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

        HandleEvent(DriftEvents.Stopped);

        return proprioceptiveDrift;
    }
}
