/******************************************************************************\
* Copyright (C) Leap Motion, Inc. 2011-2014.                                   *
* Leap Motion proprietary. Licensed under Apache 2.0                           *
* Available at http://www.apache.org/licenses/LICENSE-2.0.html                 *
\******************************************************************************/

using UnityEngine;
using System.Collections;
using Leap;

namespace Leap.Unity {
    // Class to setup a rigged hand based on a model.
    public class RiggedHand : HandModel {

        public bool enableNoise = false;

        public override ModelType HandModelType {
            get {
                return ModelType.Graphics;
            }
        }
        public Vector3 modelFingerPointing = Vector3.forward;
        public Vector3 modelPalmFacing = -Vector3.up;


        private System.Random random = new System.Random();
        private Vector3 oldPosition;
        private Quaternion oldRotation;

        public override void InitHand() {
            UpdateHand();
        }

        public Quaternion Reorientation() {
            return Quaternion.Inverse(Quaternion.LookRotation(modelFingerPointing, -modelPalmFacing));
        }
       

        public void Start()
        {
            if(wristJoint != null)
            {
                oldRotation = wristJoint.transform.localRotation;
            }
        }


        public override void UpdateHand() {
            float d = 0.0f;

            if(enableNoise)
            {
                float distance = Vector3.Distance(palm.position, oldPosition);
                oldPosition = palm.position;

                d = 0.0025f * 1.0f / distance;
            }

            if (palm != null) {
                palm.position = GetPalmPosition();
                palm.rotation = GetPalmRotation() * Reorientation();
            }

            if (forearm != null)
                forearm.rotation = GetArmRotation() * Reorientation();

            // Update fingers
            for (int i = 0; i < fingers.Length; ++i) {
                if (fingers[i] != null) {
                    fingers[i].fingerType = (Finger.FingerType)i;
                    fingers[i].UpdateFinger();

                    if(enableNoise)
                    {
                        RiggedFinger riggedFinger = (RiggedFinger)fingers[i];
                        riggedFinger.AddNoise(Mathf.Min(d, 1.0f));
                    }
                }
            }

            // Add noise
            if (wristJoint != null)
            {
                // Add some noise to the position of the hand
                if (enableNoise)
                {
                    float amplitude = Mathf.Min(d, 1.0f);

                    Quaternion noise = Quaternion.Euler(
                        amplitude * (NormalRandom.NextGaussianFloat(random) / 1.25f),
                        amplitude * (NormalRandom.NextGaussianFloat(random) / 1.25f),
                        amplitude * (NormalRandom.NextGaussianFloat(random) / 1.25f)
                        );

                    wristJoint.transform.localRotation = oldRotation * noise;

                }
            }
        }


    }
}
