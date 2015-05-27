/******************************************************************************\
* Copyright (C) Leap Motion, Inc. 2011-2014.                                   *
* Leap Motion proprietary. Licensed under Apache 2.0                           *
* Available at http://www.apache.org/licenses/LICENSE-2.0.html                 *
\******************************************************************************/

using UnityEngine;
using System.Collections;
using Leap;

/**
 * Class to setup a rigged hand based on a model.
 */
public class RiggedHand :HandModel 
{
	// Links to components of the arm
	public Transform arm;
	public Transform foreArm;
	public Transform palm;

	public bool detectDirections = false;

	// Directions of finger and palm
	public Vector3 modelFingerPointing = Vector3.forward;
	public Vector3 modelPalmFacing = -Vector3.up;

	// Debug mode
	public bool debugMode;
	protected bool partOfAvatar = false;

	public void Awake()
	{
		// If no fingers have been assigned, do it automatically
		if(fingers.Length == 0)
		{
			fingers = new FingerModel[transform.childCount];
		
			for(int i = 0; i < transform.childCount; i++) {
				GameObject obj = transform.GetChild(i).gameObject;		
				fingers[i] = obj.GetComponent<FingerModel>();			
			}
		}
		
		// Assign arm, forearm, and palm if not assigned
		if(palm == null)
			palm = transform;
		
		if(foreArm == null)
			foreArm = palm.parent;
		
		if(arm == null)
			arm = foreArm.parent;
	}


	public void Start()
	{
		if(detectDirections) {
			foreach(FingerModel finger in fingers) {
				if(!finger is RiggedFinger)
					continue;
					
				RiggedFinger riggedFinger = finger as RiggedFinger;
			
				if(riggedFinger.fingerType != Finger.FingerType.TYPE_INDEX)
					continue;
			
				modelFingerPointing = riggedFinger.modelFingerPointing;
			}
		}
	}


	public override void InitHand() 
	{
		UpdateHand();
	}


	public Quaternion Reorientation() 
	{
		return Quaternion.Inverse(Quaternion.LookRotation(modelFingerPointing, -modelPalmFacing));
	}


	public override void UpdateHand() 
	{
		if(arm != null) {
			arm.LookAt(this.GetElbowPosition(), Vector3.up);
			arm.rotation *= Reorientation();
		}
		
		if(foreArm != null) {
			foreArm.LookAt(this.GetWristPosition());		
			foreArm.rotation *= Reorientation();
		}
		
		if(debugMode) {
			Debug.DrawLine(this.GetElbowPosition(), arm.position, Color.red);
			Debug.DrawLine(this.GetWristPosition(), arm.position, Color.yellow);
		}

		if (palm != null) {
			if(!partOfAvatar)
				palm.position = GetPalmPosition();

			palm.rotation = GetPalmRotation() * Reorientation();						
		}

		for (int i = 0; i < fingers.Length; ++i) {
			if (fingers[i] != null)
				fingers[i].UpdateFinger();
		}
	}
}
