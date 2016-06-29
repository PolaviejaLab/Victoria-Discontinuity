/******************************************************************************\
* Copyright (C) Leap Motion, Inc. 2011-2014.                                   *
* Leap Motion proprietary. Licensed under Apache 2.0                           *
* Available at http://www.apache.org/licenses/LICENSE-2.0.html                 *
\******************************************************************************/

using UnityEngine;
using System.Collections;
using Leap;

namespace Leap.Unity
{
    public enum NoiseType
    {
        NoNoise,
        NormalAroundPalm,
        NormalRandomWalk
    }


    // Class to setup a rigged hand based on a model.
    public class RiggedHandEx : HandModel
    {
        public Transform arm;
        public bool partOfAvatar;

        public bool firstUpdate = true;
        public bool ignoreUpdates = false;

        public NoiseType noiseType;
        public float noiseLevel = 0.01f;

        public float lambda = 0.5f;

        private Vector3 prevActualPosition;
        private Vector3 prevVirtualPosition;

        public override ModelType HandModelType
        {
            get
            {
                return ModelType.Graphics;
            }
        }

        public Vector3 modelFingerPointing = Vector3.forward;
        public Vector3 modelPalmFacing = -Vector3.up;


        public override void InitHand()
        {
            UpdateHand();
        }


        public Quaternion Reorientation()
        {
            return Quaternion.Inverse(Quaternion.LookRotation(modelFingerPointing, -modelPalmFacing));
        }


        /**
         * Returns the position of the palm. Noise is added in this function.
         */
        public new virtual Vector3 GetPalmPosition()
        {
            Vector3 actualPosition = hand_.PalmPosition.ToVector3();
            Vector3 mean = new Vector3(0, 0, 0);
            Vector3 std = new Vector3(noiseLevel, noiseLevel, noiseLevel);

            if(firstUpdate) {
                prevActualPosition = actualPosition;
                prevVirtualPosition = actualPosition;
                firstUpdate = false;
            }

            // Compute difference in position
            Vector3 velocity = actualPosition - prevActualPosition;
            prevActualPosition = actualPosition;

            if (noiseType == NoiseType.NormalAroundPalm) {
                return actualPosition + NormalRandom.Random(mean, std);
            } else if (noiseType == NoiseType.NormalRandomWalk) {
                Vector3 update = prevVirtualPosition + velocity + NormalRandom.Random(mean, std);
                prevVirtualPosition = lambda * actualPosition + (1 - lambda) * update;

                return prevVirtualPosition;
            }

            return hand_.PalmPosition.ToVector3();
        }


        /**
         * Update location and orientation of arm, forearm, palm, and fingers
         */
        public override void UpdateHand()
        {
            if (ignoreUpdates)
                return;

            if(arm != null)
            {
                if(partOfAvatar)
                    arm.LookAt(this.GetElbowPosition(), Vector3.up);
                arm.rotation *= Reorientation();
            }

            if (palm != null)
            {
                if(!partOfAvatar)
                    palm.position = GetPalmPosition();
                palm.rotation = GetPalmRotation() * Reorientation();
            }

            if (forearm != null)
            {
                if (partOfAvatar) {
                    forearm.LookAt(GetWristPosition());
                    forearm.rotation *= Reorientation();
                } else {
                    /**
                     * Sometimes the LEAP does not correctly detect arm rotation. Because
                     * we use it as-it these incorrect arm rotations are present in the
                     * virtual arm.
                     *
                     * There are a few things that we can do:
                     *  - Use the reported arm rotation if confidence is high,
                     *     move arm to straight position if confidence is low.
                     *  - Restrict movement of arm to 'straighter' angles
                     * 
                     * If doing any of these, I would (using a property) keep the option
                     * of using the LEAP arm rotation (like we did with the noise).
                     * I'm not a mathematician, so I will chat with Mattia tomorrow, but
                     * perhaps we can convert to Euler angles...
                     */

                    // Here we obtain a quaternion, which is a way to represent a
                    // rotation.
                    Quaternion armRotation = GetArmRotation();

                    // Convert the quaternion to euler angles, a 3-element vector
                    // each element representing rotation around 1 axis.
                    //
                    // The elements are:
                    // X -> Pitch
                    // Y -> Yaw
                    // Z -> Roll
                    //
                    // Straight is 0, 270, 0
                    Vector3 armEulerRotation = armRotation.eulerAngles;

                    // Try this? It will restrict the range to 20 degrees
                    // If i want to rotate the arm, to to like if i fold the elbow poiting to the sides, it does not respond...
                    // I know
                    float pitchRange = 20.0f;

                    // The forearm is completely restricted :S I does not respond 

                    if (pitchRange < 360.0f) {
                        if (armEulerRotation.x > (0.5f * pitchRange) && armEulerRotation.x < 180.0f)
                            armEulerRotation.x = (0.5f * pitchRange);
                        if (armEulerRotation.x >= 180.0f && armEulerRotation.x < (360.0f - 0.5f * pitchRange))
                            armEulerRotation.x = (360.0f - 0.5f * pitchRange);
                    }

                    //armEulerRotation.x = 0.0f;

                    // Here we restrict "yaw" (side-ways) motion, now it's commented out -> no restriction on that
                    //armEulerRotation.y = 270.0f;

                    // But now the original problem is back hhehe
                    // Ho w is it back? I thought the "bad" movements were in the up/down direction?yes Now? now you can see what i do

                    // Looks fine to me? it does, let me check with the oculus! Yes it looks way way better!! Thanks so much!!!
                    // You should make this into a property or something? Just remove the floatPitchRange and make it into a property.
                    // Set it to 360.0f for full freedom
                    // Ok let me just commit this in case I mess it up! :)

                    armRotation.eulerAngles = armEulerRotation; // new Vector3(0.0f, 270.0f, armEulerRotation.z);

                    forearm.rotation = armRotation * Reorientation();

                    // TODO: Investigate this issue:
                    // What is the issue?
                    // When the arm seems to be in a certain position in respect to the table, it gets the weird angle/
                    // in the wrist
                    // Could it be distortion from the frame or reflections of some kind?
                    // Yes, maybe the frame of the table.
                }
            }

            for (int i = 0; i < fingers.Length; ++i)
            {
                if (fingers[i] != null)
                {
                    fingers[i].fingerType = (Finger.FingerType)i;
                    fingers[i].UpdateFinger();
                }
            }
        }
    }
}
