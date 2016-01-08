/******************************************************************************\
* Copyright (C) Leap Motion, Inc. 2011-2014.                                   *
* Leap Motion proprietary. Licensed under Apache 2.0                           *
* Available at http://www.apache.org/licenses/LICENSE-2.0.html                 *
\******************************************************************************/

using UnityEngine;
using System.Collections.Generic;
using Leap;
using System;
using System.IO;


/**
 * Overall Controller object that will instantiate hands and tools when they appear.
 */
public class HandController: MonoBehaviour 
{
	protected const float MM_TO_M = 0.001f;

	public bool separateLeftRight = false;

	// Allow multiple hand models
	public HandModel[] leftGraphicsModels;
	public HandModel[] rightGraphicsModels;
	public HandModel[] leftPhysicsModels;
	public HandModel[] rightPhysicsModels;

	public bool isHeadMounted = false;
	public bool mirrorZAxis = false;

	public Vector3 handMovementScale = Vector3.one;

	protected Controller leap_controller_;
	private StreamWriter streamWriter;

	/**
	 * Initialize LEAP when script instance is being loaded.
	 */
	void Awake()
	{
		leap_controller_ = new Controller();

		if(leap_controller_ == null) 
		{
			Debug.LogWarning("Cannot connect to controller. Make sure you have Leap Motion v2.0+ installed");
			return;
		}

		// Optimize for top-down tracking if on head mounted display.
		Controller.PolicyFlag policy_flags = leap_controller_.PolicyFlags;
		if (isHeadMounted)
			policy_flags |= Controller.PolicyFlag.POLICY_OPTIMIZE_HMD;
		else
			policy_flags &= ~Controller.PolicyFlag.POLICY_OPTIMIZE_HMD;

		leap_controller_.SetPolicyFlags(policy_flags);
		
		// Find number of devices
		Debug.Log("Number of LEAP devices found: " + leap_controller_.Devices.Count);
	}


	public void IgnoreCollisionsWithHands(GameObject to_ignore, bool ignore = true)
	{
		foreach (HandModel hand in leftPhysicsModels)
			Leap.Utils.IgnoreCollisions(hand.gameObject, to_ignore, ignore);
		foreach (HandModel hand in rightPhysicsModels)
			Leap.Utils.IgnoreCollisions(hand.gameObject, to_ignore, ignore);
	}


	protected void UpdateHandModels(HandList leap_hands, HandModel[] left, HandModel[] right) 
	{
		// Go through all the active hands and update them.
		int num_hands = leap_hands.Count;
					
		for(int h = 0; h < num_hands; h++) 
		{
			Hand leap_hand = leap_hands[h];      
			HandModel[] models = (mirrorZAxis != leap_hand.IsLeft)?left:right;

			foreach(HandModel hand in models)
			{
				hand.SetController(this);
				hand.SetLeapHand(leap_hand);
				hand.MirrorZAxis(mirrorZAxis);
					
				float hand_scale = MM_TO_M * leap_hand.PalmWidth / hand.handModelPalmWidth;
				hand.transform.localScale = hand_scale * transform.lossyScale;				
	    		hand.UpdateHand();
			}      
		}
	}


	public Controller GetLeapController() 
	{
		return leap_controller_;
	}


	public Frame GetFrame() 
	{
		return leap_controller_.Frame();
	}


	/**
	 * Update GraphicsModels every frame.
	 */
	void Update() 
	{
		if(leap_controller_ == null)
			return;
    
    	UpdateRecorder();
    
		Frame frame = GetFrame();
		UpdateHandModels(frame.Hands, leftGraphicsModels, rightGraphicsModels);
	}


	/**
	 * Update PhysicsModels every fixed frame.
	 */
	void FixedUpdate() 
	{
		if (leap_controller_ == null)
			return;

		Frame frame = GetFrame();
		UpdateHandModels(frame.Hands, leftPhysicsModels, rightPhysicsModels);
	}


	/**
	 * Returns true if LEAP is connected.
	 */
	public bool IsConnected() 
	{
		return leap_controller_.IsConnected;
	}


	public HandModel[] GetAllGraphicsHands() 
	{
		return new HandModel[0];
	}


	public HandModel[] GetAllPhysicsHands()
	{
		return new HandModel[0];
	}


	public void DestroyAllHands()
	{
	}


	public void StartRecording(string filename)
	{
		streamWriter = new StreamWriter(filename, true);
	}


	public void StopRecording()
	{
		streamWriter.Close();
        streamWriter = null;
	}


	/**
	 * Writes a 3d vector to file
	 */
	private void WritePosition(StreamWriter writer, Vector vector)
	{
		writer.Write(vector.x);
		writer.Write(", ");
		writer.Write(vector.y);
		writer.Write(", ");
		writer.Write(vector.z);
	}


	protected void UpdateRecorder() 
	{
		if(streamWriter == null)
			return;

		Frame frame = leap_controller_.Frame();
		bool gotHand = false;

		streamWriter.Write(DateTime.UtcNow.ToString("o") + ", ");

		int num_hands = frame.Hands.Count;
		for (int h = 0; h < num_hands; ++h) {
			if(frame.Hands[h].IsRight) {
				gotHand = true;

				streamWriter.Write(frame.Hands[h].IsValid);
				streamWriter.Write(", ");

				WritePosition(streamWriter, frame.Hands[h].PalmPosition);
				break;
			}
		}

		if(!gotHand) {
			streamWriter.Write("False, 0, 0, 0");
		}

		streamWriter.WriteLine();
		streamWriter.Flush();
	}
}
