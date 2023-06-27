// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if MRET_EXTENSION_MRTK
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
#endif

namespace GOV.NASA.GSFC.XR.CrossPlatformInputSystem.SDK.Hololens
{
    public class FingerTracker : MonoBehaviour
    {
        public InputHand hand;

        private List<ITouchBehavior> collisionListeners = new List<ITouchBehavior>();

#if MRET_EXTENSION_MRTK
        public TrackedHandJoint finger;

        public Handedness currentHand;
#endif
        public float touchCooldown = 1f;

        #region MonoBehaviour
        private void Update()
        {
#if MRET_EXTENSION_MRTK
            MixedRealityPose pose;

            if (HandJointUtils.TryGetJointPose(finger, currentHand, out pose))
            {
                this.transform.position = pose.Position;
            }
#endif
        }
        #endregion MonoBehaviour

        public void addCollisionListener(ITouchBehavior behavior)
        {
            collisionListeners.Add(behavior);
        }

        public void removeCollisionListener(ITouchBehavior behavior)
        {
            collisionListeners.Remove(behavior);
        }

        public bool hasCollisionListener(ITouchBehavior behavior)
        {
            return collisionListeners.Contains(behavior);
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("TriggerEnter: " + collisionListeners.Count);
            //go through all objects currently listening for a collision
            foreach (ITouchBehavior listener in collisionListeners)
            {
                if (listener.OnTouch(other))
                {
                    Collider collider = this.gameObject.GetComponent<Collider>();
                    Debug.Log("TriggerEnter2: " + other.gameObject.name + ": " + collider.enabled);
                    collider.enabled = false;
                    StartCoroutine(HandTouchCooldown());
                }
            }
        }

        private IEnumerator HandTouchCooldown()
        {
            Collider collider = this.gameObject.GetComponent<Collider>();
            yield return new WaitForSeconds(touchCooldown);
            collider.enabled = true;
        }
    }
}
