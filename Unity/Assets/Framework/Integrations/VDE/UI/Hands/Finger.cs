/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
#if PLATFORM_LUMIN
using UnityEngine;
using UnityEngine.XR.MagicLeap;

namespace Assets.VDE.UI.Hands
{
    internal class Finger : MonoBehaviour
    {
        internal string fingerName = "";
        internal Hand hand;
        internal new CapsuleCollider collider;
        internal new Renderer renderer;
        internal ColliderBehaviour behave;
        internal Function function;
        internal bool visible;

        internal enum Function
        {
            Index,
            Thumb,
            Wrist
        }

        internal void Init(
            string name
            )
        {
            fingerName = name;
            //gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //transform = gameObject.transform;
            //gameObject.SetActive(false);
            //gameObject.transform.SetParent(transform);
            gameObject.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            gameObject.name = fingerName;// Tip.ToString();
            renderer = gameObject.GetComponent<Renderer>();
            if (visible)
            {
                renderer.material.color = Color.grey;
            }
            else
            {
                renderer.material.color = new Color(1, 1, 1, 0);
                renderer.enabled = false;
            }
        }
        private void Update()
        {
            MLHandTracking.KeyPoint tip = Tip;
            if (tip is null || !hand.MagicHand.IsVisible)
            {
                collider.enabled = renderer.enabled = false;
            } 
            else
            {
                collider.enabled = renderer.enabled = visible;// true;
                transform.localPosition = tip.Position;
                transform.LookAt(Camera.main.transform.position);
                transform.Rotate(Vector3.up, 180);
            }
        }
        private MLHandTracking.KeyPoint Tip
        {
            get
            {
                try
                {
                    return function switch
                    {
                        Function.Index => hand.MagicHand.Index.Tip,
                        Function.Thumb => hand.MagicHand.Thumb.Tip,
                        Function.Wrist => hand.MagicHand.Wrist.Center,
                        _ => null,
                    };
                }
                catch (System.Exception)
                {
                    return null;
                }
            }
        }
    }
}
#elif PLATFORM_WINRT
using UnityEngine;

namespace Assets.VDE.UI.Hands
{
    internal class Finger : MonoBehaviour
    {
        internal string fingerName = "";
        internal Hand hand;
        private Log log;
        internal new CapsuleCollider collider;
        internal new Renderer renderer;
        internal ColliderBehaviour behave;
        internal Function function;
        internal bool visible;

        internal enum Function
        {
            Index,
            Thumb,
            Wrist,
            GrippingPoint
        }

        internal void Init(
            string name
            )
        {
            fingerName = name;
            log = new Log("Finger [" + name + "]");
            //gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //transform = gameObject.transform;
            //gameObject.SetActive(false);
            //gameObject.transform.SetParent(transform);
            if (function == Function.GrippingPoint)
            {
                gameObject.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
            }
            else
            {
                gameObject.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            }
            gameObject.name = fingerName;// Tip.ToString();
            renderer = gameObject.GetComponent<Renderer>();
            renderer.material = hand.vde.visibleMaterial;
            if (visible)
            {
                renderer.material.color = Color.grey;
            }
            else
            {
                renderer.material.color = new Color(1, 1, 1, 0);
                renderer.enabled = false;
            }
        }
        internal void SetVisibility(bool setTo)
        {
            if (setTo)
            {
                renderer.material.color = new Color(0.5F, 0.5F, 0.5F, 0.3F);
            }
            renderer.enabled = setTo;
        }
        private void Update()
        {
            //if (this.hand.device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.handData, out UnityEngine.XR.Hand hand))
            {
                //log.Entry("InputDevices_deviceConnected: " + this.hand + ": CommonUsages.handData");
                //touchingDevices.Add(device);
                /*
                 * for debuging ML & HL
                 */

                //if (this.hand.device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.userPresence, out bool handInView) && handInView)
                {
                    switch (function)
                    {
                        case Function.Index:
                            if(
                            Microsoft.MixedReality.Toolkit.Input.HandJointUtils.TryGetJointPose(
                                Microsoft.MixedReality.Toolkit.Utilities.TrackedHandJoint.IndexTip, 
                                this.hand.handness, 
                                out Microsoft.MixedReality.Toolkit.Utilities.MixedRealityPose iPose)
                            )
                            {
                                transform.SetPositionAndRotation(iPose.Position, iPose.Rotation);
                            }
                            // as of 20220106 https://github.com/microsoft/MixedRealityToolkit-Unity/issues/10228 we need to use this workaround.
                            else
                            {
                                Transform iTrafo = hand.HandJointService.RequestJointTransform(Microsoft.MixedReality.Toolkit.Utilities.TrackedHandJoint.IndexTip, Microsoft.MixedReality.Toolkit.Utilities.Handedness.Right);
                                if (!(iTrafo is null))
                                {
                                    transform.SetPositionAndRotation(iTrafo.position, iTrafo.rotation);
                                }
                            }
                            break;
                        case Function.Thumb:
                            if(
                            Microsoft.MixedReality.Toolkit.Input.HandJointUtils.TryGetJointPose(
                                Microsoft.MixedReality.Toolkit.Utilities.TrackedHandJoint.ThumbTip,
                                this.hand.handness,
                                out Microsoft.MixedReality.Toolkit.Utilities.MixedRealityPose tPose)
                            )
                            {
                                transform.SetPositionAndRotation(tPose.Position, tPose.Rotation);
                            }
                            // as of 20220106 https://github.com/microsoft/MixedRealityToolkit-Unity/issues/10228 we need to use this workaround.
                            else
                            {
                                Transform iTrafo = hand.HandJointService.RequestJointTransform(Microsoft.MixedReality.Toolkit.Utilities.TrackedHandJoint.ThumbTip, Microsoft.MixedReality.Toolkit.Utilities.Handedness.Right);
                                if (!(iTrafo is null))
                                {
                                    transform.SetPositionAndRotation(iTrafo.position, iTrafo.rotation);
                                }
                            }
                            break;
                        case Function.Wrist:
                            if(
                            Microsoft.MixedReality.Toolkit.Input.HandJointUtils.TryGetJointPose(
                                Microsoft.MixedReality.Toolkit.Utilities.TrackedHandJoint.Wrist,
                                this.hand.handness,
                                out Microsoft.MixedReality.Toolkit.Utilities.MixedRealityPose wPose)
                            )
                            {
                                transform.SetPositionAndRotation(wPose.Position, wPose.Rotation);
                            }
                            // as of 20220106 https://github.com/microsoft/MixedRealityToolkit-Unity/issues/10228 we need to use this workaround.
                            else
                            {
                                Transform iTrafo = hand.HandJointService.RequestJointTransform(Microsoft.MixedReality.Toolkit.Utilities.TrackedHandJoint.Wrist, Microsoft.MixedReality.Toolkit.Utilities.Handedness.Right);
                                if (!(iTrafo is null))
                                {
                                    transform.SetPositionAndRotation(iTrafo.position, iTrafo.rotation);
                                }
                            }
                            break;
                        case Function.GrippingPoint:
                            if (
                                Microsoft.MixedReality.Toolkit.Input.HandJointUtils.TryGetJointPose(
                                    Microsoft.MixedReality.Toolkit.Utilities.TrackedHandJoint.IndexTip,
                                    this.hand.handness,
                                    out Microsoft.MixedReality.Toolkit.Utilities.MixedRealityPose igPose)
                                &&
                                Microsoft.MixedReality.Toolkit.Input.HandJointUtils.TryGetJointPose(
                                    Microsoft.MixedReality.Toolkit.Utilities.TrackedHandJoint.ThumbTip,
                                    this.hand.handness,
                                    out Microsoft.MixedReality.Toolkit.Utilities.MixedRealityPose tgPose)
                            )
                            {
                                transform.SetPositionAndRotation(igPose.Position + 0.5F * (tgPose.Position - igPose.Position), igPose.Rotation);
                            }
                            // as of 20220106 https://github.com/microsoft/MixedRealityToolkit-Unity/issues/10228 we need to use this workaround.
                            else
                            {
                                Transform iTrafo = hand.HandJointService.RequestJointTransform(Microsoft.MixedReality.Toolkit.Utilities.TrackedHandJoint.IndexTip, Microsoft.MixedReality.Toolkit.Utilities.Handedness.Right);
                                Transform tTrafo = hand.HandJointService.RequestJointTransform(Microsoft.MixedReality.Toolkit.Utilities.TrackedHandJoint.ThumbTip, Microsoft.MixedReality.Toolkit.Utilities.Handedness.Right);

                                if (!(iTrafo is null) && !(tTrafo is null))
                                {
                                    transform.SetPositionAndRotation(iTrafo.position + 0.5F * (tTrafo.position - iTrafo.position), iTrafo.rotation);
                                }
                            }
                            break;
                        default:
                            break;
                    }

                    //Debug.Log("FINGA " + function.ToString() +": " + transform.position.ToString() + " /" + transform.rotation.ToString());
                }
                /*
                System.Collections.Generic.List<UnityEngine.XR.Bone> bones = new System.Collections.Generic.List<UnityEngine.XR.Bone> { };
                switch (function)
                {
                    case Function.Index:
                        hand.TryGetFingerBones(UnityEngine.XR.HandFinger.Index, bones);
                        break;
                    case Function.Thumb:
                        hand.TryGetFingerBones(UnityEngine.XR.HandFinger.Thumb, bones);
                        break;
                    case Function.Wrist:
                        hand.TryGetFingerBones(UnityEngine.XR.HandFinger.Pinky, bones);
                        break;
                    default:
                        break;
                }
                foreach (UnityEngine.XR.Bone bone in bones)
                {
                    Vector3 vekktor = Vector3.zero;
                    Quaternion kvaterrnion = Quaternion.Euler(Vector3.zero);

                    bone.TryGetPosition(out vekktor);
                    bone.TryGetRotation(out kvaterrnion);
                    log.Entry("got index " + bones.IndexOf(bone) + " bone: " + bone.ToString() + " pos: " + vekktor.ToString() + " rott: " + kvaterrnion.ToString());
                }
                collider.enabled = renderer.enabled = visible;// true;
                if(bones[0].TryGetPosition(out Vector3 bonePos)) {
                    transform.localPosition = bonePos;
                    if (bones[0].TryGetRotation(out Quaternion boneRat))
                    {
                        transform.localRotation = boneRat;
                    }
                    //transform.LookAt(Camera.main.transform.position);
                    //transform.Rotate(Vector3.up, 180);
                }
                */
            }
            /*
            else
            {
                collider.enabled = renderer.enabled = false;
            }
            */
        }
    }
}
#elif MRET_2021_OR_LATER
using UnityEngine;

namespace Assets.VDE.UI.Hands
{
    internal class Finger : MonoBehaviour
    {
        internal string fingerName = "";
        internal Hand hand;
        private Log log;
        internal new CapsuleCollider collider;
        internal new Renderer renderer;
        internal ColliderBehaviour behave;
        internal Function function;
        internal bool visible;

        internal enum Function
        {
            Index,
            Thumb,
            Wrist,
            GrippingPoint
        }

        internal void Init(
            string name
            )
        {
            fingerName = name;
            log = new Log("Finger [" + name + "]");
            //gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //transform = gameObject.transform;
            //gameObject.SetActive(false);
            //gameObject.transform.SetParent(transform);
            /*
            if (function == Function.GrippingPoint)
            {
                gameObject.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
            }
            else
            {
                gameObject.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            }*/
            gameObject.name = fingerName;// Tip.ToString();
            renderer = gameObject.GetComponent<Renderer>();
            renderer.material = hand.vde.visibleMaterial;
            if (visible)
            {
                renderer.material.color = Color.grey;
            }
            else
            {
                renderer.material.color = new Color(1, 1, 1, 0);
                renderer.enabled = false;
            }
        }
        internal void SetVisibility(bool setTo)
        {
            if (setTo)
            {
                renderer.material.color = new Color(0.5F, 0.5F, 0.5F, 0.3F);
            }
            renderer.enabled = setTo;
        }
    }
}
#else
using UnityEngine;

namespace Assets.VDE.UI.Hands
{
    internal class Finger
    {
        internal string fingerName = "";
        internal GameObject gameObject;
        internal CapsuleCollider collider;
        internal ColliderBehaviour behave;
        Renderer renderer;
        bool visible;

        public Finger(string name)
        {
            fingerName = name;
            renderer = gameObject.GetComponent<Renderer>();
            if (visible)
            {
                renderer.material.color = Color.grey;
            }
            else
            {
                renderer.material.color = new Color(1, 1, 1, 0);
                renderer.enabled = false;
            }
        }
        internal void SetVisibility(bool setTo)
        {
            if (setTo)
            {
                renderer.material.color = new Color(0.5F, 0.5F, 0.5F, 0.3F);
            }
            renderer.enabled = setTo;
        }
    }
}
#endif