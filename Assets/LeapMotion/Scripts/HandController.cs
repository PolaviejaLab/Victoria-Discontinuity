/******************************************************************************\
* Copyright (C) Leap Motion, Inc. 2011-2014.                                   *
* Leap Motion proprietary. Licensed under Apache 2.0                           *
* Available at http://www.apache.org/licenses/LICENSE-2.0.html                 *
\******************************************************************************/

using UnityEngine;
using System.Collections.Generic;
using Leap;

// Overall Controller object that will instantiate hands and tools when they appear.
public class HandController : MonoBehaviour {

  // Reference distance from thumb base to pinky base in mm.
  protected const float GIZMO_SCALE = 5.0f;
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

    
  void OnDrawGizmos() {
    // Draws the little Leap Motion Controller in the Editor view.
    Gizmos.matrix = Matrix4x4.Scale(GIZMO_SCALE * Vector3.one);
    Gizmos.DrawIcon(transform.position, "leap_motion.png");
  }

  void Awake() {
    leap_controller_ = new Controller();

    // Optimize for top-down tracking if on head mounted display.
    Controller.PolicyFlag policy_flags = leap_controller_.PolicyFlags;
    if (isHeadMounted)
      policy_flags |= Controller.PolicyFlag.POLICY_OPTIMIZE_HMD;
    else
      policy_flags &= ~Controller.PolicyFlag.POLICY_OPTIMIZE_HMD;

    leap_controller_.SetPolicyFlags(policy_flags);
  }

  void Start() {
    if (leap_controller_ == null) {
      Debug.LogWarning(
          "Cannot connect to controller. Make sure you have Leap Motion v2.0+ installed");
    }
  }


  public void IgnoreCollisionsWithHands(GameObject to_ignore, bool ignore = true) {
    foreach (HandModel hand in leftPhysicsModels)
      Leap.Utils.IgnoreCollisions(hand.gameObject, to_ignore, ignore);
	foreach (HandModel hand in rightPhysicsModels)
      Leap.Utils.IgnoreCollisions(hand.gameObject, to_ignore, ignore);
  }


  protected void UpdateHandModels(HandList leap_hands, HandModel[] left, HandModel[] right) 
  {
    // Go through all the active hands and update them.
    int num_hands = leap_hands.Count;
    for (int h = 0; h < num_hands; ++h) {
      Hand leap_hand = leap_hands[h];      
      HandModel[] models = (mirrorZAxis != leap_hand.IsLeft) ? left:right;
      
	  foreach (HandModel hand in models) {
	  	hand.SetController(this);
	    hand.SetLeapHand(leap_hand);	  
	    hand.MirrorZAxis(mirrorZAxis);
					
	    float hand_scale = MM_TO_M * leap_hand.PalmWidth / hand.handModelPalmWidth;
	    hand.transform.localScale = hand_scale * transform.lossyScale;
	    hand.UpdateHand();		  
	  }      
	}
  }


  public Controller GetLeapController() {
    return leap_controller_;
  }


  public Frame GetFrame() {
    return leap_controller_.Frame();
  }


  void Update() {
    if (leap_controller_ == null)
      return;
    
    Frame frame = GetFrame();
    UpdateHandModels(frame.Hands, leftGraphicsModels, rightGraphicsModels);
  }


  void FixedUpdate() {
    if (leap_controller_ == null)
      return;

    Frame frame = GetFrame();
    UpdateHandModels(frame.Hands, leftPhysicsModels, rightPhysicsModels);
  }


  public bool IsConnected() {
    return leap_controller_.IsConnected;
  }

  public bool IsEmbedded() {
    DeviceList devices = leap_controller_.Devices;
    if (devices.Count == 0)
      return false;
    return devices[0].IsEmbedded;
  }

  public HandModel[] GetAllGraphicsHands() {
    return new HandModel[0];
  }

  public HandModel[] GetAllPhysicsHands() {
    return new HandModel[0];
  }

  public void DestroyAllHands() {
  }



  public void Record() { }  
  public string FinishAndSaveRecording() { return ""; }
  public void ResetRecording() { }
}
