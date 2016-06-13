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
    // Class to setup a rigged hand based on a model.
    public class RiggedHandAndArm : HandModel
    {
        public Transform arm;
        public bool hasArm;
        public bool partOfAvatar;


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

        public override void UpdateHand()
        {
            if(arm != null)
            {
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
                if (!partOfAvatar) {
                    forearm.rotation = GetArmRotation() * Reorientation();
                } else {
                    forearm.LookAt(GetWristPosition());
                    forearm.rotation *= Reorientation();
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
