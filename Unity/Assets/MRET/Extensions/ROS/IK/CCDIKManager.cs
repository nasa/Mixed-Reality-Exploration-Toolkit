// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;
#if MRET_EXTENSION_ROSSHARP
using RosSharp;
using RosSharp.Urdf;
#endif
#if MRET_EXTENSION_FINALIK
using RootMotion.FinalIK;
#endif

namespace GOV.NASA.GSFC.XR.MRET.Extensions.Ros.IK
{
    public class CCDIKManager : MonoBehaviour
    {
#if MRET_EXTENSION_ROSSHARP
        // Initialize the inverse kinematics for the robot arm using FinalIK's CCD IK Component
        public static void InitializeCCDIK(UrdfRobot urdfRobot, Transform target = null)
        {
#if MRET_EXTENSION_FINALIK
            // Add the CCDIK component to the game object with the UrdfRobot component
            CCDIK ccdik = urdfRobot.transform.AddComponentIfNotExists<CCDIK>();
            // Initialize a new list of bones to use for the CCD IK component
            List<IKSolver.Bone> bones = new List<IKSolver.Bone>();
            
            // Iterate through each joint in the robot
            foreach (UrdfJoint urdfJoint in urdfRobot.GetComponentsInChildren<UrdfJoint>())
            {
                // If the joint is not a fixed joint, add a new RotationLimitHinge (used for the IK) to each joint
                if(urdfJoint.JointType != UrdfJoint.JointTypes.Fixed)
                {
                    RotationLimitHinge rotationLimitHinge = urdfJoint.transform.AddComponentIfNotExists<RotationLimitHinge>();
                    HingeJoint hingeJoint =  urdfJoint.transform.gameObject.GetComponent<HingeJoint>();
                    rotationLimitHinge.axis = hingeJoint.axis; // set the joint's axis to the rotation axis
                    
                    HingeJointLimitsManager hingeJointLimitsManager = urdfJoint.transform.gameObject.GetComponent<HingeJointLimitsManager>();

                    // If there is a joint limits manager for this joint, set the rotation limits to the joint limits
                    // Otherwise, do not use limits for the RotationHingeLimit component
                    if(hingeJointLimitsManager)
                    {
                        rotationLimitHinge.min = hingeJointLimitsManager.LargeAngleLimitMin;
                        rotationLimitHinge.max = hingeJointLimitsManager.LargeAngleLimitMax;
                    }
                    else
                    {
                        rotationLimitHinge.useLimits = false;
                    }
                    
                    // Create a new bone for the CCD IK component from the joint's transform and add it to the list of bones
                    IKSolver.Bone bone = new IKSolver.Bone();
                    bone.transform = urdfJoint.transform;
                    bones.Add(bone);
                }
            }
            // Set the CCD IK component's bone array in the solver to the list of bones created from the joints
            ccdik.solver.bones = bones.ToArray();

            // if a target transform was passed into the function, set the CCD IK component's target in the solver to the target transform parameter
            if(target)
            {
                ccdik.solver.target = target;
            }
#else
            Debug.LogWarning("FinalIK is unavailable");
#endif
        }
#endif
    }
}