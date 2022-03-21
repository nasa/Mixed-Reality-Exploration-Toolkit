using Microsoft.MixedReality.Toolkit.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Assets.VDE
{
    public class Menu : MonoBehaviour
    {
        public VDE vde;
        public GameObject PrimaryMenu, SecondaryMenu, Target, Peegel;
        bool primaryMenuUsedUp = false, handInFocus = false;
        float[] presence = new float[42];
        float min = 290, max = 310;
        Vector3 velocity = Vector3.zero;
#if (MSFT_OPENXR || DOTNETWINRT_PRESENT || MRET_2021_OR_LATER) && (UNITY_STANDALONE_WIN || UNITY_WSA)
        internal Microsoft.MixedReality.Toolkit.Utilities.Handedness handness = Microsoft.MixedReality.Toolkit.Utilities.Handedness.Right;
        internal Microsoft.MixedReality.Toolkit.Input.IMixedRealityHandJointService handJointService = null;
        internal Microsoft.MixedReality.Toolkit.Input.IMixedRealityHandJointService HandJointService => handJointService ?? Microsoft.MixedReality.Toolkit.CoreServices.GetInputSystemDataProvider<Microsoft.MixedReality.Toolkit.Input.IMixedRealityHandJointService>();
#endif
        private void Update()
        {
            if (SecondaryMenu.activeSelf || PrimaryMenu.activeSelf)
            {
                SetMenuPosition();
                if (
                    Microsoft.MixedReality.Toolkit.Utilities.HandPoseUtils.IndexFingerCurl(Microsoft.MixedReality.Toolkit.Utilities.Handedness.Left) > 0.6 ||
                    Microsoft.MixedReality.Toolkit.Utilities.HandPoseUtils.MiddleFingerCurl(Microsoft.MixedReality.Toolkit.Utilities.Handedness.Left) > 0.6 ||
                    Microsoft.MixedReality.Toolkit.Utilities.HandPoseUtils.ThumbFingerCurl(Microsoft.MixedReality.Toolkit.Utilities.Handedness.Left) > 0.6 ||
                    !CheckPresence() &&
                    !handInFocus
                    )
                {
                    //HideMenu();
                }
            }
            else if (!SecondaryMenu.activeSelf && !PrimaryMenu.activeSelf)
            {
                if (
                    Microsoft.MixedReality.Toolkit.Utilities.HandPoseUtils.IndexFingerCurl(Microsoft.MixedReality.Toolkit.Utilities.Handedness.Left) < 0.4 &&
                    Microsoft.MixedReality.Toolkit.Utilities.HandPoseUtils.MiddleFingerCurl(Microsoft.MixedReality.Toolkit.Utilities.Handedness.Left) < 0.4 &&
                    Microsoft.MixedReality.Toolkit.Utilities.HandPoseUtils.ThumbFingerCurl(Microsoft.MixedReality.Toolkit.Utilities.Handedness.Left) < 0.4 &&
                    CheckPresence() &&
                    handInFocus
                )
                {
                    ShowMenu();
                }
            }
        }

        private bool CheckPresence()
        {
            string bam = "";
            if (!(handJointService is null))
            {
                Transform trafo = handJointService.RequestJointTransform(Microsoft.MixedReality.Toolkit.Utilities.TrackedHandJoint.IndexTip, Microsoft.MixedReality.Toolkit.Utilities.Handedness.Left);
                for (int hist = 1; hist < presence.Count(); hist++)
                {
                    presence[hist] = presence[hist - 1];
                    bam += presence[hist].ToString() + ",";
                }
                presence[0] = trafo.position.x + trafo.position.y + trafo.position.z + trafo.rotation.x + trafo.rotation.y + trafo.rotation.z + trafo.rotation.w;
                bam += presence[0].ToString() + ";";
                if (presence.Where(prese => prese != presence[0]).Any())
                {
                    return true;
                }
            }
            return false;
        }

        private void SetMenuPosition()
        {
            if (vde.usableCamera)
            {
                transform.SetPositionAndRotation(
                    Vector3.SmoothDamp(
                        transform.position,
#if DOTNETWINRT_PRESENT || MRET_2021_OR_LATER
                        Vector3.MoveTowards(
                            transform.position,
                            (Target.transform.parent.position.y > Target.transform.position.y + 0.22) ?
                                new Vector3(
                                    Target.transform.position.x,
                                    Target.transform.parent.position.y - 0.22F,
                                    Target.transform.position.z
                                    ) :
                                (Target.transform.parent.position.y < Target.transform.position.y + 0.1) ?
                                    new Vector3(
                                        Target.transform.position.x,
                                        Target.transform.parent.position.y - 0.1F,
                                        Target.transform.position.z
                                        ) :
                                    Target.transform.position
                                ,
                            2F
                        ),
                        ref velocity,
                        0.6F, 3F
#else
                        Vector3.MoveTowards(
                            transform.position,
                            (activeCamera.transform.forward * HUDPosition.z) + activeCamera.transform.position,
                            HUDPosition.z
                        ),
                        ref velocity,
                        0.6F
#endif
                    ),
                    Quaternion.Slerp(
                        transform.rotation,
                        new Quaternion(
                            0,
                            vde.usableCamera.transform.rotation.y,
                            0,
                            vde.usableCamera.transform.rotation.w
                            ),
                        0.6F
                    )
                );
            }
        }
        public void HandInFocus()
        {
            handInFocus = true;
        }
        public void HandNotInFocus()
        {
            handInFocus = false;
        }
        public void ShowMenu()
        {
            {
                if (primaryMenuUsedUp)
                {
                    if (!SecondaryMenu.activeSelf)
                    {
                        SecondaryMenu.SetActive(true);
                    }
                    else
                    {
                        SetMenuPosition();
                    }
                }
                else if (!PrimaryMenu.activeSelf)
                {
                    PrimaryMenu.SetActive(true);
                }
            }
            /*
            else
            {
                Debug.Log("mamba: " + Microsoft.MixedReality.Toolkit.Utilities.HandPoseUtils.IndexFingerCurl(Microsoft.MixedReality.Toolkit.Utilities.Handedness.Left).ToString() + " : " + Microsoft.MixedReality.Toolkit.Utilities.HandPoseUtils.MiddleFingerCurl(Microsoft.MixedReality.Toolkit.Utilities.Handedness.Left).ToString() + " :" + Microsoft.MixedReality.Toolkit.Utilities.HandPoseUtils.ThumbFingerCurl(Microsoft.MixedReality.Toolkit.Utilities.Handedness.Left).ToString());
            }*/
        }
        public void HideMenu()
        {
            if (SecondaryMenu.activeSelf || PrimaryMenu.activeSelf)
            {
                SecondaryMenu.SetActive(false);
                PrimaryMenu.SetActive(false);
            }
        }
        public void ShowPrimaryMenu()
        {
            PrimaryMenu.SetActive(true);
        }
        public void HidePrimaryMenu()
        {
            primaryMenuUsedUp = true;
            PrimaryMenu.SetActive(false);

            Peegel.transform.Find("Aja kiri").gameObject.SetActive(false);
            Peegel.transform.Find("Legend").gameObject.SetActive(true);
        }
        public void ShowSecondaryMenu()
        {
            SecondaryMenu.SetActive(true);
        }
        public void FingersOn()
        {
            SetFingersVisibility(true);
        }
        public void FingersOff()
        {
            SetFingersVisibility(false);
            //InvokeFunction(UI.Input.Event.Function.ExportObjectWithCoordinates, false);
        }
        private void SetFingersVisibility(bool setTo)
        {
            foreach (var fingers in vde.controller.hands.Values.Select(hand => hand.fingers))
            {
                foreach (Assets.VDE.UI.Hands.Finger finger in fingers.Values)
                {
                    finger.SetVisibility(setTo);
                }
            }
        }
        public void EdgesOff()
        {
            InvokeFunction(UI.Input.Event.Function.ToggleEdges, false);
        }
        public void EdgesOn()
        {
            InvokeFunction(UI.Input.Event.Function.ToggleEdges, true);
        }
        public void LabelsOff()
        {
            InvokeFunction(UI.Input.Event.Function.ToggleLabels, false);
        }
        public void LabelsOn()
        {
            InvokeFunction(UI.Input.Event.Function.ToggleLabels, true);
        }
        public void NotificationsOff()
        {
            InvokeFunction(UI.Input.Event.Function.ToggleNotifications, false);
        }
        public void NotificationsOn()
        {
            InvokeFunction(UI.Input.Event.Function.ToggleNotifications, true);
        }
        public void ShowJuhan()
        {
            Peegel.SetActive(true);
        }
        public void HideJuhan()
        {
            Peegel.SetActive(false);
        }
        private void InvokeFunction(UI.Input.Event.Function fun, bool setu)
        {
            vde.controller.inputObserver.inputEvent.Invoke(new UI.Input.Event
            {
                function = fun,
                type = UI.Input.Event.Type.Bool,
                Bool = setu
            });
        }
        public void SetPosition()
        {
            vde.controller.inputObserver.inputEvent.Invoke(new UI.Input.Event
            {
                Vector3 = transform.position,
                function = UI.Input.Event.Function.PositionVDE
            });
        }
    }
}
