/******************************************************************************\
* Copyright (C) Leap Motion, Inc. 2011-2014.                                   *
* Leap Motion proprietary. Licensed under Apache 2.0                           *
* Available at http://www.apache.org/licenses/LICENSE-2.0.html                 *
\******************************************************************************/

using UnityEngine;
using System.Collections;
using Leap;


public class RiggedFinger :FingerModel 
{
	public bool detectType = true;

	public Transform[] bones = new Transform[NUM_BONES];

	public bool detectDirections = false;

	public Vector3 modelFingerPointing = Vector3.forward;
	public Vector3 modelPalmFacing = -Vector3.up;

	
	
	private void GuessDirectionFromBones()
	{
		Transform first = bones[0];
		Transform last = bones[bones.Length - 1];
		
		modelFingerPointing = Vector3.Normalize(last.localPosition - first.localPosition);
		
		Debug.Log(name + " points in direction " + modelFingerPointing);
	}


	private Finger.FingerType GuessType(string name)
	{
		if(name.ToLower().Contains("index"))
			return Finger.FingerType.TYPE_INDEX;
		if(name.ToLower().Contains("thumb"))
			return Finger.FingerType.TYPE_THUMB;
		if(name.ToLower().Contains("ring"))
			return Finger.FingerType.TYPE_RING;
		if(name.ToLower().Contains("pinky"))
			return Finger.FingerType.TYPE_PINKY;
		if(name.ToLower().Contains("middle"))
			return Finger.FingerType.TYPE_MIDDLE;
			
		Debug.Log("Could not identify type of finger " + name);

		return fingerType;
	}


	/**
	 * This will attempt to set the finger type and
	 * assign all currently unassigned bones.
	 */
	public void Awake()
	{
		// Figure out finger type
		if(detectType)
			fingerType = GuessType(name);
			
		// Assign bones
		Transform current = this.transform;
	
		for(int i = 1; i < bones.Length; i++) 
		{
			if(bones[i] != null)
				continue;

			Debug.Log("Assigning " + current.gameObject.name + " to bone " + i);
			bones[i] = current;

			if(current.childCount == 0)
				continue;

			current = current.GetChild(0);
		}
		
		// Use bones to detect finger direction
		if(detectDirections) 
			GuessDirectionFromBones();
	}

    
	public Quaternion Reorientation() 
	{
		return Quaternion.Inverse(Quaternion.LookRotation(modelFingerPointing, -modelPalmFacing));
	}


	public override void InitFinger() 
	{
		UpdateFinger();
	}


	public override void UpdateFinger() 
	{
		for (int i = 0; i < bones.Length; ++i) {
			if (bones[i] != null)
				bones[i].rotation = GetBoneRotation(i) * Reorientation();		
		}
	}
}
