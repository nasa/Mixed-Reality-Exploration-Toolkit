/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
#define DISABLE_STEAMVR
using System;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.LowLevel;
#endif
using UnityEngine.XR;
#if !DISABLE_STEAMVR
using Unity.XR.OpenVR; //Steam.VR;
#endif
#if PLATFORM_LUMIN
using UnityEngine.XR.MagicLeap;
#endif

namespace Assets.VDE.UI.Input
{
    /// <summary>
    /// this class needs to be ripped to pieces, each supported input device in its own extension instead of #ifdefing.
    /// sometime in the future.
    /// </summary>
    internal class InputObserver: Observer
    {
        private VDE vde;
        private bool[] buttonsPressedInPreviousFrame = new bool[] { false, false, false, false, false, false, false, false, false, false, false, false, false };
        private List<InputDevice>
            devicesWithPrimaryButton = new List<InputDevice>(), 
            joyfulDevices = new List<InputDevice>(),
            triggerinDevices = new List<InputDevice>(),
            grippinginDevices = new List<InputDevice>(),
            touchingDevices = new List<InputDevice>(),
            pointingDevices = new List<InputDevice>();
        private bool
            scaleVDE,
            isGrabbing, 
            actionValidity, 
            edgesVisibile, 
            labelsVisibile;
        private Container grabbedContainer = null;
        private GameObject middleFinger;
        private Log log;
        private bool userIsPointing;
        private bool userIsPintching;

#if !DISABLE_STEAMVR && DO_STEAM
        internal SteamVR_Action_Vector2 thumbPositionOnTouch = SteamVR_Input.GetAction<SteamVR_Action_Vector2>("default", "ThumbOnTouch");
        internal SteamVR_Action_Single triggerFloat = SteamVR_Input.GetAction<SteamVR_Action_Single>("default", "Squeeze");
        internal SteamVR_Action_Boolean gripp = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("default", "GrabGrip");
        internal SteamVR_Action_Boolean trigger = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("default", "GrabPinch");
        internal SteamVR_Action_Boolean gamburger = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("default", "Gamburger");
#endif

        private void Awake()
        {
            log = new Log("InputObserver");
        }

        private void Start()
        {
            log.messenger = vde.data.messenger;
#if !ENABLE_LEGACY_INPUT_MANAGER
            EnhancedTouchSupport.Enable();
#endif
#if PLATFORM_LUMIN
            try
            {
                List<MLHandTracking.HandKeyPose> handPoses = new List<MLHandTracking.HandKeyPose>();
                handPoses.Add(MLHandTracking.HandKeyPose.Finger);
                handPoses.Add(MLHandTracking.HandKeyPose.Pinch);
                handPoses.Add(MLHandTracking.HandKeyPose.Fist);
                handPoses.Add(MLHandTracking.HandKeyPose.Thumb);
                handPoses.Add(MLHandTracking.HandKeyPose.L);
                handPoses.Add(MLHandTracking.HandKeyPose.OpenHand);
                handPoses.Add(MLHandTracking.HandKeyPose.Ok);
                handPoses.Add(MLHandTracking.HandKeyPose.C);
                handPoses.Add(MLHandTracking.HandKeyPose.NoPose);
                MLHandTracking.KeyPoseManager.EnableKeyPoses(handPoses.ToArray(), true, false);
                MLInput.OnControllerButtonDown += MLInput_OnControllerButtonDown;
                MLInput.OnControllerButtonUp += MLInput_OnControllerButtonUp;
                MLInput.OnTriggerDown += MLInput_OnTriggerDown;
                MLInput.OnTriggerUp += MLInput_OnTriggerUp;
                MLHandTracking.KeyPoseManager.OnKeyPoseBegin += KeyPoseManager_OnKeyPoseBegin;
                MLHandTracking.KeyPoseManager.OnKeyPoseEnd += KeyPoseManager_OnKeyPoseEnd;
            }
            catch (Exception exe)
            {
                log.Entry("Unable to start the magic: " + exe.StackTrace);
            }

#elif DOTNETWINRT_PRESENT
            FindFinger();
#endif
        }
        private void FindFinger()
        {
            if (!(vde.controller is null) && !(vde.controller.hands is null) && !vde.controller.hands.ContainsKey(vde.primaryHand))
            {
                string 
                    thumbTipName = "",
                    indexFingerTipName = "",
                    middleFingerTipName = "",
                    wristRootName = "",
                    handNotificationName = "",
                    grippingPointName = "",
                    searchForGrippingPointFrom = "";

                switch (vde.inputSource)
                {
                    case VDE.InputSource.QuestHands:
                        indexFingerTipName = "Hand_IndexTip";
                        wristRootName = "Hand_WristRoot";
                        break;
                    case VDE.InputSource.RiftPseudoHands:
                        grippingPointName = "Offset";
                        if (vde.primaryHand == Hands.Hand.Which.Right)
                        {
                            indexFingerTipName = "hands:b_r_index_ignore";
                            middleFingerTipName = "hands_coll:b_r_middle3";
                            searchForGrippingPointFrom = wristRootName = "CustomHandRight";
                            handNotificationName = "VDEHandTextRight";

                            GameObject toDisable = GameObject.Find("CustomHandLeft");
                            if (!(toDisable is null)) { toDisable.SetActive(false); }
                            toDisable = GameObject.Find("VDEHandTextLeft");
                            if (!(toDisable is null)) { toDisable.SetActive(false); }
                        } 
                        else
                        {
                            indexFingerTipName = "hands:b_l_index_ignore";
                            // indeed coll_hands instead of hands_coll, as corresponding object is named for the right hand.
                            // that's facebook for you in 2020.
                            middleFingerTipName = "coll_hands:b_l_middle3";
                            searchForGrippingPointFrom = wristRootName = "CustomHandLeft";
                            handNotificationName = "VDEHandTextLeft";

                            GameObject toDisable = GameObject.Find("CustomHandRight");
                            if (!(toDisable is null)) { toDisable.SetActive(false); }
                            toDisable = GameObject.Find("VDEHandTextRight");
                            if (!(toDisable is null)) { toDisable.SetActive(false); }
                        }
                        break;
                    case VDE.InputSource.MagicalLeapingHands:
                        {
                            thumbTipName = "magicThumb";
                            indexFingerTipName = "magicIndex";
                            middleFingerTipName = "magicFinger";
                            wristRootName = "MagicWrist";
                            handNotificationName = "__noname__";// "VDEHandTextRight";
                            grippingPointName = thumbTipName;// "magicThumb";
                            searchForGrippingPointFrom = "MagicWrist";
                        }
                        break;
                    case VDE.InputSource.SteamingPseudoHands:
                        {
                            indexFingerTipName = "finger_index_r_end";
                            middleFingerTipName = "finger_middle_r_end";
                            wristRootName = "wrist_r";
                            handNotificationName = "VDEHandTextRight";
                            grippingPointName = "ObjectAttachmentPoint";
                            searchForGrippingPointFrom = "RightHand";
                        }
                        break;
                    case VDE.InputSource.MRET:
                        {
                            indexFingerTipName = "VDEIndexAntenna";
                            middleFingerTipName = "VDEMiddleAntenna";
                            // until MRET.21.2
                            //wristRootName = "RightHand Controller";
                            // since MRET.21.3
                            wristRootName = "RightHand";
                            handNotificationName = "VDEHandTextRight";
                            grippingPointName = middleFingerTipName;
                            searchForGrippingPointFrom = wristRootName;
                        }
                        break;
                    case VDE.InputSource.HoloLens:
                        {
                            /*
                            thumbTipName = "ThumbTip Proxy Transform";
                            indexFingerTipName = "IndexTip Proxy Transform";
                            middleFingerTipName = "MiddleTip Proxy Transform";
                            wristRootName = "Wrist Proxy Transform";
                            handNotificationName = "__noname__";// "VDEHandTextRight";
                            grippingPointName = thumbTipName;// "magicThumb";
                            searchForGrippingPointFrom = "Right_HandSkeleton(Clone)";
                            */

                            /*
                            thumbTipName = vde.primaryHand.ToString() + " ThumbTip";
                            indexFingerTipName = vde.primaryHand.ToString() + " IndexTip";
                            middleFingerTipName = vde.primaryHand.ToString() + " MiddleTip";
                            wristRootName = "Hand Physics Service";
                            handNotificationName = "VDEHandText"; // "VDEHandTextRight";
                            grippingPointName = "VDEGrippingPoint"; // "magicThumb";
                            searchForGrippingPointFrom = wristRootName;
                            */

                            thumbTipName = vde.primaryHand.ToString() + " ThumbTip";
                            indexFingerTipName = vde.primaryHand.ToString() + " IndexTip";
                            middleFingerTipName = vde.primaryHand.ToString() + " MiddleTip";
                            wristRootName = "Right";
                            handNotificationName = "VDEHandText"; // "VDEHandTextRight";
                            grippingPointName = "VDEGrippingPoint"; // "magicThumb";
                            searchForGrippingPointFrom = wristRootName;
                        }
                        break;
                    default:
                        break;
                }
#if DOTNETWINRT_PRESENT
                GameObject wrist = vde.RightHand;

#else
                GameObject wrist = GameObject.Find(wristRootName);
#endif
                if (!(wrist is null) && !vde.controller.hands.ContainsKey(vde.primaryHand))
                {
#if DOTNETWINRT_PRESENT
                    GameObject grippingPointGO = wrist;
#else
                    GameObject grippingPointGO = GameObject.Find(searchForGrippingPointFrom);
#endif
                    if (!(grippingPointGO is null))
                    {
                        Transform pregripper = grippingPointGO.transform;
                        Hands.Hand hand;
#if MRET_2021_OR_LATER
                        Transform antenna = new GameObject("Hanz").transform;
                        antenna.localPosition = Vector3.zero;
                        antenna.localScale = Vector3.one;// * 0.02F;
                        antenna.gameObject.AddComponent<RidTheBody>().position = new Vector3(0, -0.035F, 0.01F);
                        antenna.SetParent(pregripper);
                        hand = antenna.gameObject.AddComponent<Hands.Hand>();
                        vde.data.burnOnDestruction.Add(antenna.gameObject);
                        /*
                        antenna = new GameObject(middleFingerTipName).transform;
                        antenna.localPosition = Vector3.zero;
                        antenna.localScale = Vector3.one * 0.025F;
                        antenna.gameObject.AddComponent<RidTheBody>().position = new Vector3(0, -0.035F, 0.01F);
                        antenna.SetParent(pregripper);
                        hand = antenna.gameObject.AddComponent<Hands.Hand>();
                        vde.data.burnOnDestruction.Add(antenna.gameObject);
                        */
#else
                        hand = pregripper.gameObject.AddComponent<Hands.Hand>();
#endif
                        hand.Init(vde.primaryHand, wrist, vde);
                        vde.controller.hands.Add(vde.primaryHand, hand);
#if MRET_2021_OR_LATER
                        hand.InitializeFinger(indexFingerTipName, "indexFingerTip", Hands.ColliderBehaviour.Mode.Select, 0.03F);
                        hand.InitializeFinger(middleFingerTipName, "middleFingerTip", Hands.ColliderBehaviour.Mode.Grab, 0.02F);
                        hand.InitializeNotification(handNotificationName, pregripper);
#elif PLATFORM_LUMIN
                        hand.InitializeFinger(middleFingerTipName, "wrist", Hands.ColliderBehaviour.Mode.None);
                        hand.InitializeFinger(indexFingerTipName, "indexFingerTip", Hands.ColliderBehaviour.Mode.Select);
                        hand.InitializeFinger(thumbTipName, "middleFingerTip", Hands.ColliderBehaviour.Mode.Grab);
                        hand.InitializeNotification(handNotificationName, hand.fingers["wrist"].transform, false);
#elif PLATFORM_WINRT
                        // thumb and middle finger swap places here indeed.
                        hand.InitializeFinger(middleFingerTipName, "wrist", Hands.ColliderBehaviour.Mode.None);
                        hand.InitializeFinger(indexFingerTipName, "indexFingerTip", Hands.ColliderBehaviour.Mode.Select);
                        hand.InitializeFinger(thumbTipName, "middleFingerTip", Hands.ColliderBehaviour.Mode.Grab);
                        hand.InitializeFinger(grippingPointName, "grippingPoint", Hands.ColliderBehaviour.Mode.GrippingPoint);
                        hand.InitializeNotification(handNotificationName, hand.fingers["wrist"].transform, false);
#else
                        hand.InitializeFinger(indexFingerTipName, "indexFingerTip", Hands.ColliderBehaviour.Mode.Select);
                        hand.InitializeFinger(middleFingerTipName, "middleFingerTip", Hands.ColliderBehaviour.Mode.Grab);
                        hand.InitializeNotification(handNotificationName, transform.parent);
#endif
                        hand.SetNotificationText("touch a node!");
                        try
                        {
                            // works with oculus
                            //Transform gripper = wrist.transform.Find(grippingPointName);
#if MRET_2021_OR_LATER
                            Transform gripper = hand.fingers["middleFingerTip"].transform;
#elif PLATFORM_LUMIN
                            Transform gripper = hand.fingers["middleFingerTip"].transform;                            
#elif PLATFORM_WINRT
                            Transform gripper = hand.fingers["grippingPoint"].transform;
#else
                            Transform gripper = pregripper.Find(grippingPointName);
#endif
                            if (!(gripper is null))
                            {
                                hand.gripPoint = gripper.gameObject;
                            }
                        }
                        catch (Exception exe)
                        {
                            log.Entry("Unable to find hands: " + exe.StackTrace, Log.Event.ToServer);
                        }
                        log.Entry("FindFinger: found " + wrist.name);
                    }
                    else
                    {
                        log.Entry("Handle " + searchForGrippingPointFrom + " not found");
                    }
                }
                else if(wrist is null)
                {
                    log.Entry(wristRootName + " unfound.");
                }
            }
            else
            {
                log.Entry("FindFinger was unsuccessful: " + !(vde.controller is null) + !(vde.controller.hands is null) + !vde.controller.hands.ContainsKey(vde.primaryHand));
            }
        }

        private void FindFinger(InputDevice device)
        {
            FindFinger();

            if (vde.controller.hands.ContainsKey(vde.primaryHand))
            {
                switch (vde.primaryHand)
                {
                    case Hands.Hand.Which.Right:
                        if (device.characteristics == InputDeviceCharacteristics.Right)
                        {
                            vde.controller.hands[vde.primaryHand].device = device;
                        }
                        break;
                    case Hands.Hand.Which.Left:
                        if (device.characteristics == InputDeviceCharacteristics.Left)
                        {
                            vde.controller.hands[vde.primaryHand].device = device;
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        void OnEnable()
        {
            vde = gameObject.GetComponent<VDE>();
            List<InputDevice> allDevices = new List<InputDevice>();

            InputDevices.GetDevicesWithCharacteristics(vde.primaryHand == Hands.Hand.Which.Right ? InputDeviceCharacteristics.Right : InputDeviceCharacteristics.Left, allDevices);
            foreach (InputDevice device in allDevices) InputDevices_deviceConnected(device);

            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HandTracking, allDevices);
            foreach (InputDevice device in allDevices) InputDevices_deviceConnected(device);

            InputDevices.deviceConnected += InputDevices_deviceConnected;
            InputDevices.deviceDisconnected += InputDevices_deviceDisconnected;
#if !DOTNETWINRT_PRESENT
            FindFinger();
#endif

#if ENABLE_INPUT_SYSTEM
            InputState.onChange += InputState_onChange;
            var action = new UnityEngine.InputSystem.InputAction(binding: "/<XRController>{rightHand}/position");

            UnityEngine.InputSystem.InputSystem.onDeviceChange +=
                (device, change) =>
                {
                    switch (change)
                    {
                        case UnityEngine.InputSystem.InputDeviceChange.Added:
                            // New Device.
                            InputDevices_deviceConnected(device);
                            break;
                        case UnityEngine.InputSystem.InputDeviceChange.Disconnected:
                            // Device got unplugged.
                            break;
                        case UnityEngine.InputSystem.InputDeviceChange.Reconnected:
                            // Plugged back in.
                            InputDevices_deviceConnected(device);
                            break;
                        case UnityEngine.InputSystem.InputDeviceChange.Removed:
                            // Remove from Input System entirely; by default, Devices stay in the system once discovered.
                            break;
                        default:
                            // See InputDeviceChange reference for other event types.
                            break;
                    }
                };
#endif
        }

#if PLATFORM_LUMIN

        private void MLInput_OnTriggerDown(byte controllerId, float triggerValue)
        {
            if (triggerValue > 0.8F && !actionValidity)
            {
                actionValidity = true;
                vde.data.messenger.Post(new Communication.Message()
                {
                    HUDEvent = HUD.HUD.Event.Progress,
                    number = 2,
                    floats = new List<float> { 0F, 1F },
                    message = "Release trigger to set the center of the metashape.",
                    from = vde.data.layouts.current.GetGroot()
                });
            }
        }

        private void MLInput_OnTriggerUp(byte controllerId, float triggerValue)
        {
            if (triggerValue < 0.1F && actionValidity)
            {
                log.Entry("Trying to position at: " + MLInput.GetController(controllerId).Position.ToString());
                inputEvent.Invoke(new Assets.VDE.UI.Input.Event
                {
                    Vector3 = MLInput.GetController(controllerId).Position,
                    function =  Input.Event.Function.PositionVDE
                });
                
                actionValidity = false;
            }
        }
        private void MLInput_OnControllerButtonDown(byte controllerId, MLInput.Controller.Button button)
        {
            //log.Entry("MLInput_OnControllerButtonDown: " + button.ToString());
            if (button == MLInput.Controller.Button.Bumper && !actionValidity)
            {
                inputEvent.Invoke(new Assets.VDE.UI.Input.Event
                {
                    function = Assets.VDE.UI.Input.Event.Function.ToggleNotifications,
                    //Bool = edgesVisibile
                });
                //StartCoroutine(Action(Input.Event.Function.ToggleNotifications, "Hold to toggle notifications"));
            }
            else if (button == MLInput.Controller.Button.HomeTap)
            {
                inputEvent.Invoke(new Assets.VDE.UI.Input.Event
                {
                    function = Assets.VDE.UI.Input.Event.Function.ToggleEdges,
                    Bool = edgesVisibile
                });
                edgesVisibile = !edgesVisibile;
            }
        }
        private void MLInput_OnControllerButtonUp(byte controllerId, MLInput.Controller.Button button)
        {
            //log.Entry("MLInput_OnControllerButtonUp: " + button.ToString());
            if (button == MLInput.Controller.Button.Bumper && actionValidity)
            {
                actionValidity = false;
            }
        }
        private System.Collections.IEnumerator Action(Input.Event.Function function, string massage, bool setu = true, float floats = 0)
        {
            actionValidity = true;
            float timeout = 5;
            float whenToDisable = Time.realtimeSinceStartup + timeout;
            while (whenToDisable > Time.realtimeSinceStartup && actionValidity)
            {
                vde.data.messenger.Post(new Communication.Message()
                {
                    HUDEvent = HUD.HUD.Event.Progress,
                    number = 0,
                    floats = new List<float> { (whenToDisable - Time.realtimeSinceStartup) / timeout, 1F },
                    message = massage,
                    from = vde.data.layouts.current.GetGroot()
                });
                yield return new WaitForSeconds(0.33F);
            }
            if (actionValidity)
            {
                if (function == Input.Event.Function.ScaleVDE)
                {
                    float newRelativeSize = Vector3.Distance(MLHandTracking.Left.Thumb.Tip.Position, MLHandTracking.Right.Thumb.Tip.Position);
                    // floats on 100%
                    // scaleto on 
                    float desiredChange = newRelativeSize / floats;
                    Vector3 targetScale = desiredChange * vde.transform.localScale;
                    log.Entry("Scaled: " + (newRelativeSize * 100).ToString() + "/" + (floats * 100).ToString() + "=" + (desiredChange * 100).ToString() + "*" + (vde.transform.localScale * 100).ToString() + " = " + (targetScale * 100).ToString());

                    inputEvent.Invoke(new Assets.VDE.UI.Input.Event
                    {
                        Bool = setu,
                        Vector3 = targetScale,
                        function = function
                    });
                    scaleVDE = false;
                }
                else
                {
                    inputEvent.Invoke(new Assets.VDE.UI.Input.Event
                    {
                        Bool = setu,
                        function = function// Assets.VDE.UI.Input.Event.Function.ToggleNotifications
                    });
                }

                log.Entry("it has been done!");
            }
            vde.data.messenger.Post(new Communication.Message()
            {
                HUDEvent = HUD.HUD.Event.Progress,
                number = 0,
                floats = new List<float> { 0F, 1F },
                message = "",
                from = vde.data.layouts.current.GetGroot()
            });
            vde.hud.StartCoroutine(vde.hud.FadeAwayAndDefault());
            actionValidity = false;
        }
        private void KeyPoseManager_OnKeyPoseBegin(MLHandTracking.HandKeyPose pose, MLHandTracking.HandType type)
        {
            //log.Entry("KeyPoseManager_OnKeyPoseBegin: " + pose.ToString());
            if (
                type == MLHandTracking.HandType.Left &&
                !actionValidity)
            {
                if (pose == MLHandTracking.HandKeyPose.OpenHand)
                {
                    StartCoroutine(Action(Input.Event.Function.ToggleNotifications, "Hold to toggle notifications"));
                }
                else if (pose == MLHandTracking.HandKeyPose.Fist)
                {
                    StartCoroutine(Action(Input.Event.Function.ToggleEdges, "Hold for edges' visibility", edgesVisibile));
                    edgesVisibile = !edgesVisibile;
                }
                else if (pose == MLHandTracking.HandKeyPose.Thumb)
                {
                    StartCoroutine(Action(Input.Event.Function.ToggleLabels, "Hold for labels' visibility", labelsVisibile));
                    labelsVisibile = !labelsVisibile;
                }
                else if (pose == MLHandTracking.HandKeyPose.Pinch && !isGrabbing && !scaleVDE)
                {
                    //StartCoroutine(Action(Input.Event.Function.ToggleLabels, "Adjust size while 'pinching' with both hands", scaleVDE));
                    scaleVDE = true;
                }
            }
            else if (
                type == MLHandTracking.HandType.Right &&
                !actionValidity && 
                pose == MLHandTracking.HandKeyPose.Pinch && 
                !isGrabbing && 
                scaleVDE)
            {
                float newScale = Vector3.Distance(MLHandTracking.Left.Thumb.Tip.Position, MLHandTracking.Right.Thumb.Tip.Position);
                log.Entry("scale from: " + newScale);
                StartCoroutine(Action(Input.Event.Function.ScaleVDE, "Adjust size while 'pinching' with both hands", scaleVDE, newScale));
            }
            else if (pose is MLHandTracking.HandKeyPose.Ok || pose is MLHandTracking.HandKeyPose.Pinch)
            {
                userIsPintching = true;
            }
            else if (pose is MLHandTracking.HandKeyPose.Finger || pose is MLHandTracking.HandKeyPose.L)
            {
                userIsPointing = true;
            }
            else if (pose == MLHandTracking.HandKeyPose.NoHand || pose == MLHandTracking.HandKeyPose.NoPose)
            {
                userIsPintching = userIsPointing = actionValidity = false;
            }
        }
        private void KeyPoseManager_OnKeyPoseEnd(MLHandTracking.HandKeyPose pose, MLHandTracking.HandType type)
        {
            //log.Entry("KeyPoseManager_OnKeyPoseEnd: " + pose.ToString());
            userIsPintching = userIsPointing = actionValidity = false;
        }
        /*
        private System.Collections.IEnumerator SetEdgesVisibility()
        {
            goingToDisableEdges = true;
            yield return new WaitForSeconds(2);
            inputEvent.Invoke(new Assets.VDE.UI.Input.Event
            {
                function = Assets.VDE.UI.Input.Event.Function.ToggleEdges,
                Bool = edgesVisibile
            });
            edgesVisibile = !edgesVisibile;
            goingToDisableEdges = false;
        }
        */
#endif
#if ENABLE_INPUT_SYSTEM
        private void InputDevices_deviceConnected(UnityEngine.InputSystem.InputDevice device)
        {
        }

        private void InputState_onChange(UnityEngine.InputSystem.InputDevice arg1, InputEventPtr arg2)
        {
        }
#endif

        private void OnDisable()
        {
            InputDevices.deviceConnected -= InputDevices_deviceConnected;
            InputDevices.deviceDisconnected -= InputDevices_deviceDisconnected;
            devicesWithPrimaryButton.Clear();
        }

        private void InputDevices_deviceConnected(InputDevice device)
        {
            if (vde.primaryHand == Hands.Hand.Which.Left ?
                    device.characteristics == (InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Left) :
                    device.characteristics == (InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Right)
                )
            {
                log.Entry("InputDevices_deviceConnected: " + device.name + ": device.characteristics");
                devicesWithPrimaryButton.Add(device);
                joyfulDevices.Add(device);
                triggerinDevices.Add(device);
                grippinginDevices.Add(device);
                pointingDevices.Add(device);
            }

            if (device.TryGetFeatureValue(CommonUsages.primaryButton, out _))
            {
                log.Entry("InputDevices_deviceConnected: " + device.name + ": CommonUsages.primaryButton");
                devicesWithPrimaryButton.Add(device);
            }
            if (device.TryGetFeatureValue(CommonUsages.primary2DAxis, out _))
            {
                log.Entry("InputDevices_deviceConnected: " + device.name + ": CommonUsages.primary2DAxis");
                joyfulDevices.Add(device);
            }
            if (device.TryGetFeatureValue(CommonUsages.trigger, out _))
            {
                log.Entry("InputDevices_deviceConnected: " + device.name + ": CommonUsages.trigger");
                triggerinDevices.Add(device);
            }
            if (device.TryGetFeatureValue(CommonUsages.grip, out _))
            {
                log.Entry("InputDevices_deviceConnected: " + device.name + ": CommonUsages.grip");
                grippinginDevices.Add(device);
            }
            if (device.TryGetFeatureValue(CommonUsages.deviceRotation, out _))
            {
                log.Entry("InputDevices_deviceConnected: " + device.name + ": CommonUsages.deviceRotation");
                pointingDevices.Add(device);
            }
            if (device.TryGetFeatureValue(CommonUsages.handData, out UnityEngine.XR.Hand hand))
            {
                log.Entry("InputDevices_deviceConnected: " + device.name + ": CommonUsages.handData");
                touchingDevices.Add(device);
                /*
                 * for debuging ML & HL
                 *
                List<Bone> bones = new List<Bone> { };
                hand.TryGetFingerBones(HandFinger.Index, bones);
                foreach (UnityEngine.XR.Bone bone in bones)
                {
                    Vector3 vekktor = Vector3.zero;
                    Quaternion kvaterrnion = Quaternion.Euler(Vector3.zero);

                    bone.TryGetPosition(out vekktor);
                    bone.TryGetRotation(out kvaterrnion);
                    log.Entry("got index " + bones.IndexOf(bone) + " bone: " + bone.ToString() + " pos: " + vekktor.ToString() + " rott: " + kvaterrnion.ToString());
                }
                */
                FindFinger(device);
            }
        }

        private void InputDevices_deviceDisconnected(InputDevice device)
        {
            if (devicesWithPrimaryButton.Contains(device))
                devicesWithPrimaryButton.Remove(device);
            if (joyfulDevices.Contains(device))
                joyfulDevices.Remove(device);
            if (touchingDevices.Contains(device))
            {
                switch (device.characteristics)
                {
                    case InputDeviceCharacteristics.Left:
                        Destroy(vde.controller.hands[Hands.Hand.Which.Left]);
                        vde.controller.hands.Remove(Hands.Hand.Which.Left);
                        break;
                    case InputDeviceCharacteristics.Right:
                        Destroy(vde.controller.hands[Hands.Hand.Which.Right]);
                        vde.controller.hands.Remove(Hands.Hand.Which.Right);
                        break;
                    default:
                        break;
                }
                touchingDevices.Remove(device);
            }
            log.Entry("InputDevices_deviceDisconnected: " + device.name);
        }
        void Update()
        {
            bool[] buttonsNow = new bool[] { false, false, false, false, false, false, false, false, false, false, false, false, false };
            float[] floatsNow = new float[] { 0, 0, 0, 0, 0, 0 };
            Vector2[] vectorsNow = new Vector2[] { Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero};
#if !DOTNETWINRT_PRESENT
            if (!vde.controller.hands.ContainsKey(vde.primaryHand))
            {
                FindFinger();
            }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            if (UnityEngine.Input.anyKey)
            {
                if (UnityEngine.Input.GetKey(KeyCode.R))
                {
                    buttonsNow[8] = true;
                }
                if (UnityEngine.Input.GetKey(KeyCode.J))
                {
                    buttonsNow[10] = true;
                }
                if (UnityEngine.Input.GetKey(KeyCode.S))
                {
                    buttonsNow[9] = true;
                }
                if (UnityEngine.Input.GetKey(KeyCode.C))
                {
                    buttonsNow[6] = true;
                }
                if (UnityEngine.Input.GetKey(KeyCode.L))
                {
                    buttonsNow[7] = true;
                }
                if (UnityEngine.Input.GetKey(KeyCode.Escape))
                {
                    inputEvent.Invoke(new Input.Event
                    {
                        function = Input.Event.Function.Exit,
                        type = Input.Event.Type.Bool,
                        Bool = true
                    });
                }
                if (UnityEngine.Input.GetKey(KeyCode.UpArrow))
                {
                    inputEvent.Invoke(new Assets.VDE.UI.Input.Event
                    {
                        function = Assets.VDE.UI.Input.Event.Function.Move,
                        type = Assets.VDE.UI.Input.Event.Type.Vector2,
                        Vector2 = Vector2.up
                    });
                }
                if (UnityEngine.Input.GetKey(KeyCode.DownArrow))
                {
                    inputEvent.Invoke(new Assets.VDE.UI.Input.Event
                    {
                        function = Assets.VDE.UI.Input.Event.Function.Move,
                        type = Assets.VDE.UI.Input.Event.Type.Vector2,
                        Vector2 = Vector2.down
                    });
                }
            }
#elif ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Keyboard.current.anyKey.isPressed)
            {
                if (UnityEngine.InputSystem.Keyboard.current[UnityEngine.InputSystem.Key.UpArrow].isPressed)
                {
                    inputEvent.Invoke(new Input.Event
                    {
                        function = Input.Event.Function.Move,
                        type = Input.Event.Type.Vector2,
                        Vector2 = Vector2.up
                    });
                }
                if (UnityEngine.InputSystem.Keyboard.current[UnityEngine.InputSystem.Key.DownArrow].isPressed)
                {
                    inputEvent.Invoke(new Input.Event
                    {
                        function = Input.Event.Function.Move,
                        type = Input.Event.Type.Vector2,
                        Vector2 = Vector2.up
                    });
                }
            }
#endif
#if DOTNETWINRT_PRESENT
            if (Microsoft.MixedReality.Toolkit.Utilities.HandPoseUtils.IndexFingerCurl(Microsoft.MixedReality.Toolkit.Utilities.Handedness.Right) > 0.5)
            {
                buttonsNow[0] = true;
            }
#endif
            foreach (var device in devicesWithPrimaryButton)
            {
#if !DOTNETWINRT_PRESENT
                // oculus: [X/A] - Press; vive: Primary (sandwich button)(1)
                device.TryGetFeatureValue(CommonUsages.primaryButton, out buttonsNow[0]);
                // oculus: [Y/B] - Press
                device.TryGetFeatureValue(CommonUsages.secondaryButton, out buttonsNow[1]);
                // oculus & vive: Grip - Press
                device.TryGetFeatureValue(CommonUsages.gripButton, out buttonsNow[2]);
                // oculus & vive: Trigger - Press
                device.TryGetFeatureValue(CommonUsages.triggerButton, out buttonsNow[3]);

                // oculus: [X/A] - Touch
                device.TryGetFeatureValue(CommonUsages.primaryTouch, out buttonsNow[4]);
                // oculus: [Y/B] - Touch
                device.TryGetFeatureValue(CommonUsages.secondaryTouch, out buttonsNow[5]);
                // vive: Trackpad - Press
                device.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out buttonsNow[11]);

                device.TryGetFeatureValue(CommonUsages.grip, out floatsNow[0]);
                device.TryGetFeatureValue(CommonUsages.trigger, out floatsNow[1]);

                // CommonUsages.indexTouch is deprecated while OculusUsages.indexTouch doesnt work yet (as of 20200727)
                // https://forum.unity.com/threads/oculususages-not-working.860482/
                // device.TryGetFeatureValue(OculusUsages.indexTouch, out buttonsNow[XX]);
                // hence we have to live with this deprecated thing until.. better times come around.
#pragma warning disable CS0618 // Type or member is obsolete
                device.TryGetFeatureValue(CommonUsages.indexTouch, out floatsNow[2]);
#pragma warning restore CS0618 // Type or member is obsolete
#endif
#if STEAMVR
                vectorsNow[0] = thumbPositionOnTouch.GetAxis(SteamVR_Input_Sources.RightHand);
                floatsNow[2] = triggerFloat.GetAxis(SteamVR_Input_Sources.RightHand);
                buttonsNow[2] = gripp.GetState(SteamVR_Input_Sources.RightHand);
                buttonsNow[1] = buttonsNow[2] = buttonsNow[5] = trigger.GetState(SteamVR_Input_Sources.RightHand);
                buttonsNow[0] = gamburger.GetState(SteamVR_Input_Sources.RightHand);
                if (buttonsNow[2])
                {
                    floatsNow[0] = floatsNow[1] = 1;
                }
#endif
            }
            // user is pointing towards something
#if !DISABLE_STEAMVR && !MRET_2021_OR_LATER
            if (!(vde is null) && !(vde.lazer is null) && vde.lazer.laserBeamTransform.gameObject.activeSelf && buttonsNow[1])
#elif PLATFORM_LUMIN
            if (userIsPointing)
#elif DOTNETWINRT_PRESENT
            if(Microsoft.MixedReality.Toolkit.Utilities.HandPoseUtils.IndexFingerCurl(Microsoft.MixedReality.Toolkit.Utilities.Handedness.Right) < 0.2)
#else
            if (buttonsNow[2] && buttonsNow[5] && !buttonsNow[3])
#endif
            {
                // user has clicked the secondary (y/b) button to select something.
#if !PLATFORM_LUMIN && !DOTNETWINRT_PRESENT
                if (buttonsNow[1] && !buttonsPressedInPreviousFrame[1])
#endif
                {
                    inputEvent.Invoke(new Assets.VDE.UI.Input.Event
                    {
                        function = Assets.VDE.UI.Input.Event.Function.Select,
                        type = Assets.VDE.UI.Input.Event.Type.Bool
                    });
                }
            }
            // index finger is resting on the trigger
            // disable the lazer
#if !MRET_2021_OR_LATER && !VDE_ML
            if (!(vde.lazer is null) && vde.lazer.laserBeamTransform.gameObject.activeSelf && floatsNow[2] < 0.05 && !buttonsNow[1])
            //if (floatsNow[2] > 0.001 && vde.lazer.laserBeamTransform.gameObject.activeSelf)
            {
                //vde.lazer.laserBeamTransform.gameObject.SetActive(false);
                inputEvent.Invoke(new Input.Event
                {
                    function = Input.Event.Function.LazerState,
                    type = Input.Event.Type.Bool,
                    Bool = false
                });
            }
            // user hasnt managed to grab anything, index finger isnt touching the trigger, grip is well pressed and lazer is not active.
            // activate the lazar.
            if (!isGrabbing && !(vde.lazer is null) && !vde.lazer.laserBeamTransform.gameObject.activeSelf && (floatsNow[2] > 0.05 || buttonsNow[1]))
            //if (!isGrabbing && floatsNow[2] < 0.001 && floatsNow[0] > 0.6 && !vde.lazer.laserBeamTransform.gameObject.activeSelf)
            {
                //vde.lazer.laserBeamTransform.gameObject.SetActive(true);
                inputEvent.Invoke(new Input.Event
                {
                    function = Input.Event.Function.LazerState,
                    type = Input.Event.Type.Bool,
                    Bool = true
                });
            }
            // user hasnt managed to grab anything yet, but trigger and grip are well pressed
            // grab something
#endif
#if MRET_2021_OR_LATER
            if (!isGrabbing && floatsNow[0] > 0.8)
#elif PLATFORM_LUMIN
            if (!isGrabbing && userIsPintching)
#elif DOTNETWINRT_PRESENT
            if (!isGrabbing && Microsoft.MixedReality.Toolkit.Utilities.HandPoseUtils.CalculateIndexPinch(Microsoft.MixedReality.Toolkit.Utilities.Handedness.Right) > 0.7)
#else
            if (!isGrabbing && floatsNow[0] > 0.8 && floatsNow[1] > 0.8)
#endif
            {
                if (vde.controller.hands.ContainsKey(vde.primaryHand))
                {
                    isGrabbing = true;
                    //vde.lazer.laserBeamTransform.gameObject.SetActive(false);
                    inputEvent.Invoke(new Assets.VDE.UI.Input.Event
                    {
                        function = Assets.VDE.UI.Input.Event.Function.Grabbing,
                        type = Assets.VDE.UI.Input.Event.Type.GameObject,
                        GameObject = vde.controller.hands[vde.primaryHand].gripPoint,// middleFinger,
                        Bool = true
                    });
                }
            }
            // user tried to grab something but flexed its fingers
            // stop grabbing
#if PLATFORM_LUMIN
            else if (isGrabbing && !userIsPintching)
#elif DOTNETWINRT_PRESENT
            else if (isGrabbing && Microsoft.MixedReality.Toolkit.Utilities.HandPoseUtils.CalculateIndexPinch(Microsoft.MixedReality.Toolkit.Utilities.Handedness.Right) < 0.6)
#else
            else if (isGrabbing && floatsNow[0] < 0.9F && floatsNow[1] < 0.9F)
#endif
            {
                isGrabbing = false;
                // inputEvent.Invoke blocks execution, hence nulling grabbedContainer AFTER that wouldnt work. 
                // hence the tmpc
                Container tmpc = grabbedContainer;
                grabbedContainer = null;

                inputEvent.Invoke(new Input.Event
                {
                    function = Input.Event.Function.Grabbing,
                    type = Input.Event.Type.Bool,
                    obj = tmpc,
                    Bool = false
                });
            }
            // set the history of button presses from this frame so, that held down buttons wouldnt retrigger in the next frame.
            for (int buttonNr = 0; buttonNr < buttonsNow.Length; buttonNr++)
            {
                if (buttonsNow[buttonNr] != buttonsPressedInPreviousFrame[buttonNr])
                {
                    buttonsPressedInPreviousFrame[buttonNr] = buttonsNow[buttonNr];
                    if (buttonsNow[buttonNr])
                    {
                        switch (buttonNr)
                        {
                            case 0:
#if DOTNETWINRT_PRESENT
                                // in case of HL, it would otherwise re-enable / disable all edges per finger twerks.
                                if(buttonsNow[buttonNr])
#endif
                                inputEvent.Invoke(new Input.Event
                                {
                                    function = Input.Event.Function.ToggleEdges,
                                    type = Input.Event.Type.Bool,
                                    Bool = buttonsNow[buttonNr]
                                });
                                break;
                            case 10:
                                inputEvent.Invoke(new Input.Event
                                {
                                    function = Input.Event.Function.ExportObjectWithCoordinates,
                                    type = Input.Event.Type.Bool,
                                    Bool = buttonsNow[buttonNr]
                                });
                                break;
                            case 9:
                                inputEvent.Invoke(new Input.Event
                                {
                                    function = Input.Event.Function.SwitchSki,
                                    type = Input.Event.Type.Bool,
                                    Bool = buttonsNow[buttonNr]
                                });
                                break;
                            case 6:
                                inputEvent.Invoke(new Input.Event
                                {
                                    function = Input.Event.Function.DestroyTheMirror,
                                    type = Input.Event.Type.Bool,
                                    Bool = buttonsNow[buttonNr]
                                });
                                break;
                            case 7:
                                inputEvent.Invoke(new Input.Event
                                {
                                    function = Input.Event.Function.UpdateLinks,
                                    type = Input.Event.Type.Bool,
                                    Bool = buttonsNow[buttonNr]
                                });
                                break;
                            case 8:
                                inputEvent.Invoke(new Input.Event
                                {
                                    function = Input.Event.Function.UpdateShapes,
                                    type = Input.Event.Type.Bool,
                                    Bool = true
                                });
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            foreach (InputDevice input in joyfulDevices)
            {
                if (input.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 measure) && (measure.x != 0 || measure.y != 0))
                {
                    inputEvent.Invoke(new Assets.VDE.UI.Input.Event
                    {
                        function = Assets.VDE.UI.Input.Event.Function.Move,
                        type = Assets.VDE.UI.Input.Event.Type.Vector2,
                        Vector2 = measure
                    });
                }
            }

            if (vectorsNow[0].y != 0)
            {
                inputEvent.Invoke(new Input.Event
                {
                    function = Input.Event.Function.Move,
                    type = Input.Event.Type.Vector2,
                    Vector2 = Vector2.up * vectorsNow[0]
                });
            }
        }
    }
}

