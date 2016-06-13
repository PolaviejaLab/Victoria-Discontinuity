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
        
        return Vector3.Normalize(first.localPosition);
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


    static RiggedFinger FindChildFinger(GameObject handObject, Finger.FingerType type)
    {
        foreach (Transform child in handObject.transform)
        {
            RiggedFinger finger = child.GetComponent<RiggedFinger>();
            if (finger != null && finger.fingerType == type)
                return finger;
        }

        return null;
    }

    

    /**
     * Adds a RiggedHandAndArm to the specified object and
     *  initializes it.
     */
    static T AddHandModelToObject<T>(GameObject handObject) where T : HandModel
    {
        T hand = handObject.GetComponent<T>();

        // Create RiggedHand if it doesn't already exist
        if (hand == null) {
            hand = handObject.AddComponent<T>();
        } else {
            Debug.LogWarning("Hand already exists");
        }

        // Assign palm, forearm, and arm if not already assigned
        if (hand.palm == null)
            hand.palm = handObject.transform;

        if (hand.forearm == null)
            hand.forearm = hand.palm.parent;

        // Add fingers to hand object
        int index = 0;
        foreach (Transform child in handObject.transform) {
            AddRiggedFingerToObject(child.gameObject, index);
        }

        hand.fingers[0] = FindChildFinger(handObject, Finger.FingerType.TYPE_THUMB);
        hand.fingers[1] = FindChildFinger(handObject, Finger.FingerType.TYPE_INDEX);
        hand.fingers[2] = FindChildFinger(handObject, Finger.FingerType.TYPE_MIDDLE);
        hand.fingers[3] = FindChildFinger(handObject, Finger.FingerType.TYPE_RING);
        hand.fingers[4] = FindChildFinger(handObject, Finger.FingerType.TYPE_PINKY);

        return hand;
    }    


    static void AddRiggedHandToObject(GameObject handModel)
    {
        RiggedHand hand = AddHandModelToObject<RiggedHand>(handModel);

        // Find index finger and update hand pointing direction
        foreach (RiggedFinger finger in hand.fingers)
        {
            if (finger.fingerType == Finger.FingerType.TYPE_INDEX)
                hand.modelFingerPointing = finger.modelFingerPointing;
        }
    }


    static void AddRiggedHandAndArmToObject(GameObject handModel)
    {
        RiggedHandAndArm hand = AddHandModelToObject<RiggedHandAndArm>(handModel);

        // Find index finger and update hand pointing direction
        foreach (RiggedFinger finger in hand.fingers)
        {
            if (finger.fingerType == Finger.FingerType.TYPE_INDEX)
                hand.modelFingerPointing = finger.modelFingerPointing;
        }

        if (hand.arm == null)
        {
            hand.arm = hand.forearm.parent;
            hand.hasArm = true;
            hand.partOfAvatar = true;
        }
    }




    [MenuItem("GameObject/LEAP/Add rigged hand", false, 0)]
    static void AddRiggedHand()
    {
        AddRiggedHandToObject(Selection.activeGameObject);
    }


    [MenuItem("GameObject/LEAP/Add rigged hand (with arm)", false, 0)]
    static void AddRiggedHandAndArm()
    {
        AddRiggedHandAndArmToObject(Selection.activeGameObject);
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