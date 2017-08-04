using UnityEngine;
using System.Collections;


/**
* Events Handles by the Inactive Trial statemachine
*/
public enum InactiveTrialEvents {
    Timeout,                // Time is finished
}

public enum InactiveTrialStates {
    AccomodationTime,       // Get used to the environment
    HandNotMoving,          // Hand will be kept still in the same position
    TrialFinished,          // End of trial
}

public class InactiveTrialController : ICStateMachine<InactiveTrialStates, InactiveTrialEvents> {

    // Reference to other state machines
    public ExperimentController experimentController;
    public Threat threatController;

    // Reference to other scripts
    public HandSwitcher handSwitcher;

    // Parameters of the current inactive trial
    public int hand;
    public bool knifePresent;

    public GameObject testLights;

    public void Start () {
	}

    protected override void OnStart() {
        handSwitcher.selected = hand;

    }

    public void HandleEvent(InactiveTrialEvents ev) {
        Debug.Log("Event " + ev.ToString());

        if (!IsStarted())
            return;

        switch (GetState()) {
            case InactiveTrialStates.AccomodationTime:
                testLights.SetActive(false);
                break;

            case InactiveTrialStates.HandNotMoving:
                break;

            case InactiveTrialStates.TrialFinished:
                break;
        }
    }

    public void Update () {
        if (!IsStarted())
            return;

        switch (GetState()) {
            case InactiveTrialStates.AccomodationTime:
                if (Input.GetKey(KeyCode.Q))
                    ChangeState(InactiveTrialStates.HandNotMoving);
                break;

            case InactiveTrialStates.HandNotMoving:
                if (GetTimeInState() > 60.0f)
                    ChangeState(InactiveTrialStates.TrialFinished);
                break;

            case InactiveTrialStates.TrialFinished:
                break;
        }

    }

    protected override void OnEnter(InactiveTrialStates oldstate) {
        switch (GetState()) {
            case InactiveTrialStates.AccomodationTime:
                handSwitcher.showRightHand = true;
                testLights.SetActive(false);
                break;

            case InactiveTrialStates.HandNotMoving:
                handSwitcher.ignoreUpdatesRight = true;
                break;

            case InactiveTrialStates.TrialFinished:
                experimentController.HandleEvent(ExperimentEvents.TrialFinished);
                testLights.SetActive(false);
                this.StopMachine();
                break;
        }
    }

    protected override void OnExit(InactiveTrialStates newState) {
        switch (GetState()) {
            case InactiveTrialStates.AccomodationTime:
                handSwitcher.showLeftHand = false;
                break;

            case InactiveTrialStates.HandNotMoving:
                break;

            case InactiveTrialStates.TrialFinished:
                break;
        }

    }
}
