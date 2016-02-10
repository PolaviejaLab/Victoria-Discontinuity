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
public class RiggedHand: HandModel 
{
	// Noise source
	private System.Random random = new System.Random();

	// Links to components of the arm
	public Transform arm;
	public Transform foreArm;
	public Transform palm;
    public Transform wrist;

	public bool hasArm = true;

	public bool detectDirections = false;

	// Directions of finger and palm
	public Vector3 modelFingerPointing = Vector3.forward;
	public Vector3 modelPalmFacing = -Vector3.up;

	// Debug mode
	public bool debugMode;
	public bool partOfAvatar = false;

	public bool enableNoise = false;
	private Vector3 noise;

	private Vector3 oldPosition;
    private Quaternion oldRotation;

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
			palm = this.transform;
		
		if(foreArm == null)
			foreArm = palm.parent;
		
		if(arm == null && hasArm)
			arm = foreArm.parent;
	}


	public void Start()
	{		
        if (wrist != null)
        {
            oldRotation = wrist.transform.localRotation;
        }

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
		// Compute inverse of velocity
		float d = 0.0f;
		
		if(enableNoise) {
			float distance = Vector3.Distance(palm.position, oldPosition);
			oldPosition = palm.position;
        
			d = 0.0025f * 1.0f / distance;
		}
		
		// Update arm/hand/palm position/rotation	
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
            palm.rotation = GetPalmRotation() * Reorientation();

			if(!partOfAvatar) {
				Vector3 position = GetPalmPosition();
			

                palm.position = position;
			}
		}

		// Update fingers
		for (int i = 0; i < fingers.Length; ++i) {
			if (fingers[i] == null)
				continue;
				
			fingers[i].UpdateFinger();
			
			// Add some noise to rotations in the fingers.
			if(enableNoise) {
				RiggedFinger riggedFinger = (RiggedFinger) fingers[i];								
				riggedFinger.AddNoise(Mathf.Min(d, 1.0f));
			}
		}

        if (wrist != null)
        {
            // Add some noise to the position of the hand
            if(enableNoise)
            {
                float amplitude = Mathf.Min(d, 1.0f);
                
                Quaternion noise = Quaternion.Euler(
                    amplitude * (NormalRandom.NextGaussianFloat(random)/1.25f),
                    amplitude * (NormalRandom.NextGaussianFloat(random)/1.25f),
                    amplitude * (NormalRandom.NextGaussianFloat(random)/1.25f)
                    );                       
                
                wrist.transform.localRotation = oldRotation * noise;
                
            }
        }
	}
}
