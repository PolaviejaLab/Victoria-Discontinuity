/**
 * State machine for a threat (e.g. knife) that
 * falls on a target (e.g. hand) and then follows it.
 */
 
using UnityEngine;
using System.Collections;
using Leap;


/**
 * States the threat can be in
 */
public enum ThreatState {
	Initial,    // Idle state
	Falling,    // Falling on / towards target
	Following   // Stick to target
};


/**
 * Events handled by the threat state machine.
 */
public enum ThreatEvent {
    ReleaseThreat,  // Drop the threat
	TargetReached   // Target reached
};


public class Threat: StateMachine<ThreatState, ThreatEvent> 
{
    public HandController handController;

    protected Controller leap_controller_;

    public GameObject threat;
	public Transform targetTransform;

	private float threatSpeed;
	private float gravity = 9.81f;

    public Vector3 knifeOffset;
    public bool knifeOnReal;

    public float followingTimeout;
    
    public bool hideOnStopped = false;
    public bool isHeadMounted = false;

    Vector3 initialThreatPosition;
	Quaternion initialThreatRotation;
	Quaternion savedRotation;

    void Start() {
        // Store the initial transformation of the threat
        // this way we can reset it later
        initialThreatPosition = threat.transform.position;
		initialThreatRotation = threat.transform.rotation;

        if(hideOnStopped)
            threat.SetActive(false);

        followingTimeout = 2.0f;

        leap_controller_ = handController.GetLeapController();
	}
	
    
    protected override void OnStart() {
        if(hideOnStopped)
            threat.SetActive(true);
    }
    
    
    protected override void OnStop() {
        if(hideOnStopped)
            threat.SetActive(false);
    }
    
    
	void Update () 
    {
        if(!IsStarted())
            return;

        Frame frame = leap_controller_.Frame();

        Vector3 handPosition = new Vector3(frame.Hands[3].PalmPosition.x, frame.Hands[3].PalmPosition.y, frame.Hands[3].PalmPosition.z);

        switch (GetState()) {        
            case ThreatState.Falling:
                if (!knifeOnReal) {
                    FallOnTarget();
                }
                else if (knifeOnReal) {
                    FallOnReal(handPosition);
                }
                break;

            case ThreatState.Following:
                if (!knifeOnReal) {
                    threat.transform.position = targetTransform.position + knifeOffset / 30;
                    threat.transform.rotation = (targetTransform.rotation * Quaternion.Inverse(savedRotation)) * initialThreatRotation;
                }

                if (knifeOnReal) {

                    threat.transform.position = handPosition;

//                    threat.transform.rotation = (handController.rightPhysicsModels[0].transform.rotation * Quaternion.Inverse(savedRotation)) * initialThreatRotation;
                }

                if (GetTimeInState() > followingTimeout) {
                    StopMachine();
                }
                break;            
        }

        // If threat is close to target, emit TargetReached event
        if (Vector3.Distance(threat.transform.position, targetTransform.position + knifeOffset / 30) < 0.001 && !knifeOnReal) {
            HandleEvent(ThreatEvent.TargetReached);
            Debug.Log("miaw target reached");
        }

        if (Vector3.Distance(threat.transform.position, handPosition) < 0.001 && knifeOnReal) {
            HandleEvent(ThreatEvent.TargetReached);
            Debug.Log("miaw real reached");
        }

        


    }


    public void HandleEvent(ThreatEvent ev) {
        switch(GetState()) {
            case ThreatState.Initial:
                if(ev == ThreatEvent.ReleaseThreat)
                    ChangeState(ThreatState.Falling);
                break;
                
            case ThreatState.Falling:
                if(ev == ThreatEvent.TargetReached)
                    ChangeState(ThreatState.Following);
                break;

        }
    }


	protected override void OnEnter(ThreatState oldState){
        switch(GetState()) {
            case ThreatState.Falling:
                threat.transform.position += knifeOffset/30;
                threatSpeed = 0.0f;
                break;

            case ThreatState.Following:
                savedRotation = targetTransform.rotation;
                break;
        }
	}    


	protected override void OnExit(ThreatState newState) {
        switch(GetState()) {
            // Reset initial position and rotation when following is complete
            case ThreatState.Following:
                threat.transform.position = initialThreatPosition;
                threat.transform.rotation = initialThreatRotation;
                break;
        }        
	}


    /**
     * Advances the threat position such that is falls on the target
     */
	private void FallOnTarget() {
		threatSpeed += gravity * Time.deltaTime;
		        
		threat.transform.position = Vector3.MoveTowards(
            threat.transform.position, 
            targetTransform.position + knifeOffset/30, // find the right proportion
			threatSpeed * Time.deltaTime);
	}

    private void FallOnReal(Vector3 handPosition) {
        threatSpeed += gravity * Time.deltaTime;

        threat.transform.position = Vector3.MoveTowards(
            threat.transform.position,
            handPosition,
            threatSpeed * Time.deltaTime);

    }
}
