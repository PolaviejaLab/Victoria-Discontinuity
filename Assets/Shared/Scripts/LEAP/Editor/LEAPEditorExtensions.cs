using UnityEngine;
using System.Collections;
using UnityEditor;
using Leap;
using Leap.Unity;

/**
 * Adds two items to the context menu:
 *
 *  Add rigged hand
 *  ---------------
 *  Adds a RiggedHand component to the selected object and adds RiggedFinger
 *  components to all child objects. It also sets up links between these
 *  objects and tries to guess the finger type and bone structure.
 *
 *  Remove all LEAP objects
 *  -----------------------
 *  Recursively removes all RiggedHand and RiggedFinger components from 
 *  all objects that are descendents of the selected object.
 */
public static class LEAPEditorExtensions
{

    /**
     * Guess the direction of the fingers from the (local) bone positions
     */
    private static Vector3 GuessDirectionFromBones(RiggedFinger finger)
    {
        Transform first = finger.bones[1];
        Transform last = finger.bones[finger.bones.Length - 1];

        return Vector3.Normalize(last.localPosition - first.localPosition);
    }


    /**
     * Guesses the type of the finger based on the name and index (order in which it
     *  appears in the scene graph).
     */
    private static Finger.FingerType GuessFingerType(string name, int index)
    {
        name = name.ToLower();

        if (name.Contains("index"))
            return Finger.FingerType.TYPE_INDEX;
        if (name.Contains("thumb"))
            return Finger.FingerType.TYPE_THUMB;
        if (name.Contains("ring"))
            return Finger.FingerType.TYPE_RING;
        if (name.Contains("pinky"))
            return Finger.FingerType.TYPE_PINKY;
        if (name.Contains("middle"))
            return Finger.FingerType.TYPE_MIDDLE;

        switch (index)
        {
            case 0: return Finger.FingerType.TYPE_THUMB;
            case 1: return Finger.FingerType.TYPE_INDEX;
            case 2: return Finger.FingerType.TYPE_MIDDLE;
            case 3: return Finger.FingerType.TYPE_RING;
            case 4: return Finger.FingerType.TYPE_PINKY;
        }

        return Finger.FingerType.TYPE_INDEX;
    }


    /**
     * Add a RiggedRinger to the object specified and intializes it.
     */
    static void AddRiggedFingerToObject(GameObject fingerObject, int index = 0)
    {
        // Create RiggedFinger is it doens't already exist
        RiggedFinger finger = fingerObject.GetComponent<RiggedFinger>();

        if (finger == null)
        {
            finger = fingerObject.AddComponent<RiggedFinger>();
        }
        else {
            Debug.LogError("Finger already exists");
            return;
        }


        // Assign the bones to the finger
        Transform current = fingerObject.transform;

        for (int i = 1; i < finger.bones.Length; i++)
        {
            finger.bones[i] = current;

            if (current.childCount == 0)
                continue;

            current = current.GetChild(0);
        }


        // Guess type and finger direction
        finger.fingerType =
            GuessFingerType(fingerObject.name, index);

        finger.modelFingerPointing =
            GuessDirectionFromBones(finger);
    }


    /**
     * Adds a RiggedHand to the specified object and
     *  initializes it.
     */
    static void AddRiggedHandToObject(GameObject handObject)
    {
        RiggedHand hand = handObject.GetComponent<RiggedHand>();

        // Create RiggedHand if it doesn't already exist
        if (hand == null)
        {
            hand = handObject.AddComponent<RiggedHand>();
        }
        else {
            Debug.LogError("Hand already exists");
            return;
        }


        // Assign palm, forearm, and arm if not already assigned
        if (hand.palm == null)
            hand.palm = handObject.transform;

        /*if (hand.foreArm == null)
            hand.foreArm = hand.palm.parent;

        if (hand.arm == null)
            hand.arm = hand.foreArm.parent;*/


        // Add fingers to hand object
        int index = 0;
        foreach (Transform child in handObject.transform)
        {
            AddRiggedFingerToObject(child.gameObject, index);

            if (hand.fingers[index] == null)
                hand.fingers[index] = child.gameObject.GetComponent<RiggedFinger>();

            index++;
        }


        // Find index finger
        foreach (RiggedFinger finger in hand.fingers)
        {
            if (finger.fingerType == Finger.FingerType.TYPE_INDEX)
                hand.modelFingerPointing = finger.modelFingerPointing;
        }
    }


    [MenuItem("GameObject/LEAP/Add rigged hand", false, 0)]
    static void AddRiggedHand()
    {
        AddRiggedHandToObject(Selection.activeGameObject);
    }


    /**
     * Recursively remove RiggedHand and RiggedFinger objects
     *  from objects in the scene graph. 
     *
     * It would be better to first build a list of all objects and then 
     *  ask the user for confirmation before deleting the objects.
     */
    private static void RecursiveRemove(GameObject obj)
    {
        RiggedHand hand = obj.GetComponent<RiggedHand>();

        if (hand)
            Object.DestroyImmediate(hand);

        RiggedFinger finger = obj.GetComponent<RiggedFinger>();

        if (finger)
            Object.DestroyImmediate(finger);

        foreach (Transform child in obj.transform)
            RecursiveRemove(child.gameObject);
    }


    [MenuItem("GameObject/LEAP/Remove all LEAP objects", false, 0)]
    static void RemoveAll()
    {
        GameObject obj = Selection.activeGameObject;
        RecursiveRemove(obj);
    }
}