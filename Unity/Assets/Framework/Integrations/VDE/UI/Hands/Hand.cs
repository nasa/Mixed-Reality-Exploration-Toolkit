/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.VDE.UI.Hands
{
    public class Hand : MonoBehaviour
    {
        VDE vde;
        Vector3 previous;
        float velocity;
        Rigidbody rigidity;
        internal bool hasNotification { get; private set; }
        internal Dictionary<string, Finger> fingers = new Dictionary<string, Finger> { };
        internal Container grabbedContainer;
        internal GameObject wristRoot, gripPoint;
        Notification notification;
        internal Which which;
        internal bool grabbing;
        private GameObject notificationObject;

        public enum Which
        {
            Right,
            Left
        }
        internal void Init(Which which, GameObject wristRoot, VDE vde)
        {
            this.which = which;
            this.wristRoot = wristRoot;
            this.vde = vde;
        }
        private void Start()
        {
            if (!gameObject.TryGetComponent(out rigidity))
            {
                rigidity = gameObject.AddComponent<Rigidbody>();
            }
        }
        private void Update()
        {
            if (!(rigidity is null) && rigidity.useGravity)
            {
                rigidity.isKinematic = true;
                rigidity.useGravity = false;
                rigidity.mass = 0;
            }
            velocity = ((transform.position - previous).magnitude) / Time.deltaTime;
            previous = transform.position;
        }
        internal void Grabbing(bool setTo)
        {
            grabbing = setTo;
            if (fingers.ContainsKey("middleFingerTip"))
            {
                fingers["middleFingerTip"].collider.enabled = setTo;
            }
            if (fingers.ContainsKey("indexFingerTip"))
            {
                fingers["indexFingerTip"].collider.enabled = !setTo;
            }
            if (!setTo)
            {
                if (!(grabbedContainer is null))
                {
                    grabbedContainer.Released(this);
                    grabbedContainer = null;
                }
            }
        }
        internal void InitializeFinger(string gameObjectName, string tag, ColliderBehaviour.Mode mode, float size = 0.002F)
        {
            GameObject finger = GameObject.Find(gameObjectName);
            if (!(finger is null))
            {                
                if (!fingers.ContainsKey(tag))
                {
                    fingers.Add(tag, new Finger(tag));
                }
                finger.TryGetComponent(out fingers[tag].collider);
                if (fingers[tag].collider is null)
                {
                    fingers[tag].collider = finger.AddComponent<CapsuleCollider>();
                    fingers[tag].collider.radius = size;
                    fingers[tag].collider.height = size;
                }
                fingers[tag].collider.isTrigger = true;
                fingers[tag].collider.enabled = true;
                fingers[tag].collider.material = vde.physicMaterial;
                fingers[tag].collider.tag = tag;
                ColliderBehaviour behaviour = finger.AddComponent<ColliderBehaviour>();
                behaviour.hand = this;
                behaviour.mode = mode;
                fingers[tag].behave = behaviour;
            }
        }
        internal void SetNotificationText(string text)
        {
            if (notification is null)
            {
                hasNotification = true;
                notification = wristRoot.AddComponent<Notification>();
                notification.font = vde.font;
                notification.text = notificationObject.GetComponent<TMPro.TextMeshPro>();
            }
            notification.SetText(text);
        }
        /// <summary>
        /// this is currently specific to oculus, as unity doestn (yet?) provide an easy way to get the controller / hand velocities from non-prop sources.
        /// </summary>
        /// <returns></returns>
        internal Vector3[] GetVelos()
        {
            
#if !DISABLE_STEAMVR
            return new Vector3[] {
                rigidity.velocity,
                rigidity.angularVelocity
            };
#elif OVR
            return new Vector3[] {
                (which == Which.Right) ? OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch) : OVRInput.GetLocalControllerVelocity(OVRInput.Controller.LTouch),
                (which == Which.Right) ? OVRInput.GetLocalControllerAngularVelocity(OVRInput.Controller.RTouch) : OVRInput.GetLocalControllerAngularVelocity(OVRInput.Controller.LTouch)
            };
#else
            return new Vector3[] { Vector3.zero, Vector3.zero };
#endif
        }

        internal void InitializeNotification(string handNotificationName)
        {
            notificationObject = GameObject.Find(handNotificationName);
            if (notificationObject is null && !(vde.VDEHandText is null))
            {
                notificationObject = vde.VDEHandText;
                notificationObject.SetActive(true);
                notificationObject.transform.SetParent(transform.parent);
                notificationObject.transform.localPosition = new Vector3(0, 0.02F, -0.2F);
                notificationObject.transform.localEulerAngles = new Vector3(2.6F, 90F, -3F);
                notificationObject.transform.localScale = Vector3.one * 0.01F;
            }
        }
    }
}
