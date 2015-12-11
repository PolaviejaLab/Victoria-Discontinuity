using UnityEngine;
using System.Collections;

public class Threat : MonoBehaviour 
{
	private enum ThreatState { Idle, Falling, Following };



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


	private ThreatState threatState;
	Quaternion initialThreatRotation;
	Quaternion savedRotation;

	// Use this for initialization
	void Start () {
		initialThreatRotation = threat.transform.rotation;
		threatState = ThreatState.Idle;
	}
	
	// Update is called once per frame
	void Update () {
		// targetPositionx = targetMovement.position.x;
		// targetPositiony = targetMovement.position.y;
		// targetPositionz = targetMovement.position.z;
		// Debug.Log (targetMovement.position);
		handPosition = handTransform.position;
		Debug.Log (handTransform.position);

		if (Input.GetKeyDown ("space") && threatState == ThreatState.Idle) {
			Debug.Log ("Knife falling");
			threatState = ThreatState.Falling;
			threatSpeed = 0;
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

	void FallOnTarget () {
		threatSpeed += gravity * Time.deltaTime;

			threat.transform.position = Vector3.MoveTowards (
				threat.transform.position, 
				handTransform.position, 
				threatSpeed * Time.deltaTime);
	}
}