/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
#if (MSFT_OPENXR || DOTNETWINRT_PRESENT) && (UNITY_STANDALONE_WIN || UNITY_WSA)
using Microsoft.MixedReality.Input;
using Microsoft.MixedReality.OpenXR;
//using Handedness = Microsoft.MixedReality.Toolkit.Utilities.Handedness;
using Microsoft.MixedReality.Toolkit.Utilities;
#endif
namespace Assets.VDE.UI.Hands
{
    public class Hand : MonoBehaviour
    {
        internal VDE vde;
        Vector3 previous;
        float velocity;
        Rigidbody rigidity;
        float[] presence = new float[42];
        internal bool hasNotification { get; private set; }
        internal Dictionary<string, Finger> fingers = new Dictionary<string, Finger> { };
        internal Container grabbedContainer;
        internal GameObject wristRoot, gripPoint;
        Notification notification;
        internal Which which;
        internal bool grabbing;
        private GameObject notificationObject;
        internal InputDevice device;
        private MeshRenderer notificationObjectMesh;
#if (MSFT_OPENXR || DOTNETWINRT_PRESENT) && (UNITY_STANDALONE_WIN || UNITY_WSA)
        internal Microsoft.MixedReality.Toolkit.Utilities.Handedness handness = Microsoft.MixedReality.Toolkit.Utilities.Handedness.Right;
        internal Microsoft.MixedReality.Toolkit.Input.IMixedRealityHandJointService handJointService = null;

        internal Microsoft.MixedReality.Toolkit.Input.IMixedRealityHandJointService HandJointService => handJointService ?? Microsoft.MixedReality.Toolkit.CoreServices.GetInputSystemDataProvider<Microsoft.MixedReality.Toolkit.Input.IMixedRealityHandJointService>();
#endif
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
#if (MSFT_OPENXR || DOTNETWINRT_PRESENT) && (UNITY_STANDALONE_WIN || UNITY_WSA)
            if(which == Which.Left)
            {
                handness = Microsoft.MixedReality.Toolkit.Utilities.Handedness.Left;
            }
#endif
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
#if PLATFORM_LUMIN
            //transform.localPosition = MagicHand.Wrist.Center.Position;
#endif
#if DOTNETWINRT_PRESENT
            //if (device.TryGetFeatureValue(CommonUsages.userPresence, out bool handInView) && handInView)
            if(Microsoft.MixedReality.Toolkit.Input.HandJointUtils.TryGetJointPose(TrackedHandJoint.Wrist, handness, out MixedRealityPose wristPose))
            {
                //Microsoft.MixedReality.Toolkit.Input.HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexTip, handness, out var indexPose);
                //Microsoft.MixedReality.Toolkit.Input.HandJointUtils.TryGetJointPose(TrackedHandJoint.Wrist, handness, out MixedRealityPose wristPose);
                transform.SetPositionAndRotation(wristPose.Position, wristPose.Rotation);
                //Debug.Log("WRIST: " + wristPose.Position.ToString() + "/" + wristPose.Rotation.ToString());
            }
            if (!(notificationObjectMesh is null))
            {
                if (notificationObjectMesh.enabled && !CheckPresence())// notificationObject.transform.parent.transform.position.magnitude == 0F)
                {
                    notificationObjectMesh.enabled = false;
                }
                else if (!notificationObjectMesh.enabled && CheckPresence())// notificationObject.transform.parent.transform.position.magnitude > 0.1F)
                {
                    notificationObjectMesh.enabled = true;
                }
            }
            else
            {
                SetNotificationText("");
            }
            // NOT NOW!
            /*
            else if (3>3)
            {
                switch (device.characteristics)
                {
                    case InputDeviceCharacteristics.Left:
                        vde.controller.hands.Remove(Which.Left);
                        break;
                    case InputDeviceCharacteristics.Right:
                        vde.controller.hands.Remove(Which.Right);
                        break;
                    default:
                        break;
                }
                Destroy(this);
            }
            */
#endif
        }

#if DOTNETWINRT_PRESENT
        internal void Grabbing(bool setTo)
        {
            grabbing = setTo;
            if (fingers.ContainsKey("grippingPoint"))
            {
                fingers["grippingPoint"].collider.enabled = setTo;
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
#else
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
#endif
        internal void InitializeFinger(string gameObjectName, string tag, ColliderBehaviour.Mode mode, float size = 0.002F)
        {
#if PLATFORM_LUMIN
            Debug.Log("Initializing Magical Finger: " + tag);
            GameObject finger = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            finger.name = gameObjectName;
            finger.transform.SetParent(this.transform);
            Finger magicFinger = finger.AddComponent<Finger>();
            magicFinger.function = mode switch
            {
                ColliderBehaviour.Mode.Select => Finger.Function.Index,
                ColliderBehaviour.Mode.Grab => Finger.Function.Thumb,
                ColliderBehaviour.Mode.None => Finger.Function.Wrist,
                _ => Finger.Function.Wrist,
            };
            //magicFinger.function = (mode == ColliderBehaviour.Mode.Select) ? Finger.Function.Index : Finger.Function.Thumb;
            magicFinger.hand = this;
            magicFinger.visible = mode != ColliderBehaviour.Mode.None;
            magicFinger.Init(tag);
#elif PLATFORM_WINRT
            Debug.Log("Initializing Holographical Finger: " + tag);
            GameObject finger = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            if(finger.TryGetComponent(out SphereCollider deplorable))
            {
                deplorable.enabled = false;
            }
            finger.name = gameObjectName;
            finger.transform.SetParent(GameObject.Find("Hanz").transform);
            Finger fFinger = finger.AddComponent<Finger>();
            fFinger.function = mode switch
            {
                ColliderBehaviour.Mode.Select => Finger.Function.Index,
                ColliderBehaviour.Mode.Grab => Finger.Function.Thumb,
                ColliderBehaviour.Mode.None => Finger.Function.Wrist,
                ColliderBehaviour.Mode.GrippingPoint => Finger.Function.GrippingPoint,
                _ => Finger.Function.Wrist,
            };
            fFinger.hand = this;
            fFinger.visible = false;
            fFinger.Init(tag);
#elif MRET_2021_OR_LATER
            Debug.Log("Initializing MRET Finger: " + tag);
            GameObject finger = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            if (finger.TryGetComponent(out SphereCollider deplorable))
            {
                deplorable.enabled = false;
            }
            finger.name = gameObjectName;
            finger.transform.SetParent(transform);
            finger.transform.localScale = Vector3.one * size;
            // setting size here to 1 avoids another ifdef below for collider scale.
            size = 1;
            Finger fFinger = finger.AddComponent<Finger>();
            fFinger.function = mode switch
            {
                ColliderBehaviour.Mode.Select => Finger.Function.Index,
                ColliderBehaviour.Mode.Grab => Finger.Function.Thumb,
                ColliderBehaviour.Mode.None => Finger.Function.Wrist,
                ColliderBehaviour.Mode.GrippingPoint => Finger.Function.GrippingPoint,
                _ => Finger.Function.Wrist,
            };
            fFinger.hand = this;
            fFinger.visible = false;
            fFinger.Init(tag);
#else
            GameObject finger = GameObject.Find(gameObjectName);
#endif
            if (!(finger is null))
            {                
                if (!fingers.ContainsKey(tag))
                {
#if PLATFORM_LUMIN
                    fingers.Add(tag, magicFinger);
#elif PLATFORM_WINRT
                    fingers.Add(tag, fFinger);
#else
                    //fingers.Add(tag, new Finger(tag));
                    fingers.Add(tag, fFinger);
#endif
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

#if PLATFORM_WINRT
                behaviour.mode = (mode == ColliderBehaviour.Mode.GrippingPoint) ? ColliderBehaviour.Mode.Grab : mode;
#else
                behaviour.mode = mode;
#endif
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

        internal void InitializeNotification(string handNotificationName, Transform setParentTo, bool adjust = true)
        {
            notificationObject = GameObject.Find(handNotificationName);
            if (notificationObject is null && !(vde.VDEHandText is null))
            {
                notificationObject = vde.VDEHandText;
                notificationObject.SetActive(true);
                notificationObject.transform.SetParent(setParentTo);

                if (adjust)
                {
                    notificationObject.transform.localPosition = new Vector3(0, 0.02F, -0.2F);
                    notificationObject.transform.localEulerAngles = new Vector3(2.6F, 90F, -3F);
                    notificationObject.transform.localScale = Vector3.one * 0.01F;
                }
            }
            if (notificationObject.TryGetComponent(out MeshRenderer mesh))
            {
                notificationObjectMesh = mesh;
            }
        }
        private void OnDestroy()
        {
            try
            {
                foreach (KeyValuePair<string, Finger> finger in fingers)
                {
                    Destroy(finger.Value.gameObject);
                    fingers.Remove(finger.Key);
                }
            }
            catch (Exception)
            {
                // in case the death of VDE is too sudden, this demolisher may reach "InvalidOperationException: Collection was modified; enumeration operation may not execute."
            }
        }
#if DOTNETWINRT_PRESENT
        private bool CheckPresence()
        {
            string bam = "";
            Transform trafo = HandJointService.RequestJointTransform(Microsoft.MixedReality.Toolkit.Utilities.TrackedHandJoint.IndexTip, Microsoft.MixedReality.Toolkit.Utilities.Handedness.Right);
            for (int hist = 1; hist < presence.Count(); hist++)
            {
                presence[hist] = presence[hist - 1];
                bam += presence[hist].ToString() + ",";
            }
            presence[0] = (trafo.position.x + trafo.position.y + trafo.position.z + trafo.rotation.x + trafo.rotation.y + trafo.rotation.z + trafo.rotation.w) * 10000;
            bam += presence[0].ToString() + ";";
            if (presence[0] != 0 && presence.Where(prese => prese != presence[0]).Any())
            {
                //Debug.Log("PRESENT!");
                return true;
            }

            //Debug.Log("GONE! " + bam);
            return false;
        }
#endif
#if PLATFORM_LUMIN
        internal UnityEngine.XR.MagicLeap.MLHandTracking.Hand MagicHand
        {
            get
            {
                if (which == Which.Left)
                {
                    return UnityEngine.XR.MagicLeap.MLHandTracking.Left;
                }
                else
                {
                    return UnityEngine.XR.MagicLeap.MLHandTracking.Right;
                }
            }
        }
#endif
    }
}
