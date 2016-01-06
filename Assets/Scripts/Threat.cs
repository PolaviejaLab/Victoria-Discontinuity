using UnityEngine;
using System.Collections;


public enum ThreatState {
	Idle,
	Falling,
	Following 
};

public enum ThreatEvent { 
	HandReached , 
};

public class Threat: StateMachine<ThreatState, ThreatEvent> 
{
	// public Transform targetMovement;
	public MarkerController markerController;

	public GameObject threat;

	private Vector3 targetPosition;
	private Vector3 threatPosition;
	
	private float targetPositionx;
	private float targetPositiony;
	private float targetPositionz;

	public Transform handTransform;

	public Vector3 handPosition;

	private float threatSpeed;
	private float gravity = 9.81f;

	public bool isActive = false; 

	private ThreatState threatState;

	Quaternion initialThreatRotation;
	Quaternion savedRotation;

	// Use this for initialization
	void Start () {
		initialThreatRotation = threat.transform.rotation;
		threatState = ThreatState.Idle;
	}
	
	void Update () {

		handPosition = handTransform.position;
		Debug.Log (handTransform.position);

		if (!isActive) {
			threat.SetActive (false);
		}
		if (isActive) {
			Debug.Log("this is the miaw");
			threat.SetActive (true);

			// ChangeState(ThreatState.Falling);
		}

	
		if (Input.GetKeyDown ("space") && threatState == ThreatState.Idle) {
			Debug.Log ("Knife falling");
			threatState = ThreatState.Falling;
		}

		if (threatState == ThreatState.Falling) {

			if(Vector3.Distance(threat.transform.position, handTransform.position) < 0.001) {
				threatState = ThreatState.Following;
				savedRotation = handTransform.rotation;
			}

			FallOnTarget ();
		}

		if (threatState == ThreatState.Following) {
			threat.transform.position = handTransform.position;

			threat.transform.rotation = (handTransform.rotation * Quaternion.Inverse(savedRotation)) * initialThreatRotation;
		}
	}


	protected override void OnEnter (ThreatState oldState)
	{
		throw new System.NotImplementedException ();
	}

	protected override void OnExit (ThreatState newState)
	{

	}

	void FallOnTarget () {
		threatSpeed += gravity * Time.deltaTime;
		
		threat.transform.position = Vector3.MoveTowards (
			threat.transform.position, 
			handTransform.position, 
			threatSpeed * Time.deltaTime);
	}
}