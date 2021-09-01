/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
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

namespace Assets.VDE.UI.Input
{
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
        private bool isGrabbing;
        private Container grabbedContainer = null;
        private GameObject middleFinger;
        private Log log;

#if !DISABLE_STEAMVR && DUH
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
            log.Entry("start");
        }
        private void FindFinger()
        {
            if (!(vde.controller is null) && !(vde.controller.hands is null) && !vde.controller.hands.ContainsKey(vde.primaryHand))
            {
                string 
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
                            wristRootName = "RightHand Controller";
                            handNotificationName = "VDEHandTextRight";
                            grippingPointName = middleFingerTipName;
                            searchForGrippingPointFrom = wristRootName;
                        }
                        break;
                    default:
                        break;
                }

                GameObject wrist = GameObject.Find(wristRootName);
                if (!(wrist is null) && !vde.controller.hands.ContainsKey(vde.primaryHand))
                {
                    Transform pregripper = GameObject.Find(searchForGrippingPointFrom).transform;
                    Hands.Hand hand;
#if MRET_2021_OR_LATER
                    Transform antenna = new GameObject(indexFingerTipName).transform;
                    antenna.localPosition = Vector3.zero;
                    antenna.localScale = Vector3.one * 0.02F;
                    antenna.gameObject.AddComponent<RidTheBody>().position = new Vector3(0, -0.035F, 0.01F);
                    antenna.SetParent(pregripper);
                    hand = antenna.gameObject.AddComponent<Hands.Hand>();
                    vde.data.burnOnDestruction.Add(antenna.gameObject);

                    antenna = new GameObject(middleFingerTipName).transform;
                    antenna.localPosition = Vector3.zero;
                    antenna.localScale = Vector3.one * 0.025F;
                    antenna.gameObject.AddComponent<RidTheBody>().position = new Vector3(0, -0.035F, 0.01F);
                    antenna.SetParent(pregripper);
                    hand = antenna.gameObject.AddComponent<Hands.Hand>();
                    vde.data.burnOnDestruction.Add(antenna.gameObject);
#else
                    hand = pregripper.gameObject.AddComponent<Hands.Hand>();
#endif
                    hand.Init(
                        vde.primaryHand, 
                        wrist, 
                        vde);
                    vde.controller.hands.Add(vde.primaryHand, hand);
                    hand.InitializeNotification(handNotificationName);
#if MRET_2021_OR_LATER
                    hand.InitializeFinger(indexFingerTipName, "indexFingerTip", Hands.ColliderBehaviour.Mode.Select, 0.5F);
                    hand.InitializeFinger(middleFingerTipName, "middleFingerTip", Hands.ColliderBehaviour.Mode.Grab, 0.4F);
#else
                    hand.InitializeFinger(indexFingerTipName, "indexFingerTip", Hands.ColliderBehaviour.Mode.Select);
                    hand.InitializeFinger(middleFingerTipName, "middleFingerTip", Hands.ColliderBehaviour.Mode.Grab);
#endif
                    hand.SetNotificationText("touch a node!");
                    try
                    {
                        // works with oculus
                        //Transform gripper = wrist.transform.Find(grippingPointName);
                        Transform gripper = pregripper.Find(grippingPointName);
                        if (!(gripper is null))
                        {
                            hand.gripPoint = gripper.gameObject;
                        }
                    }
                    catch (Exception exe)
                    {
                        log.Entry("Unable to find hands: " + exe.StackTrace, Log.Event.ToServer);
                    }
                } else
                {
                    //log.Entry("Unable to find wrist for primary hand: " + wristRootName);
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
            FindFinger();

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
        }

        private void InputDevices_deviceConnected(UnityEngine.InputSystem.InputDevice device)
        {
        }

        private void InputState_onChange(UnityEngine.InputSystem.InputDevice arg1, InputEventPtr arg2)
        {
        }

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
                devicesWithPrimaryButton.Add(device);
                joyfulDevices.Add(device);
                triggerinDevices.Add(device);
                grippinginDevices.Add(device);
                pointingDevices.Add(device);
            }

            if (device.TryGetFeatureValue(CommonUsages.primaryButton, out _))
            {
                devicesWithPrimaryButton.Add(device);
            }
            if (device.TryGetFeatureValue(CommonUsages.primary2DAxis, out _))
            {
                joyfulDevices.Add(device);
            }
            if (device.TryGetFeatureValue(CommonUsages.trigger, out _))
            {
                triggerinDevices.Add(device);
            }
            if (device.TryGetFeatureValue(CommonUsages.grip, out _))
            {
                grippinginDevices.Add(device);
            }
            if (device.TryGetFeatureValue(CommonUsages.deviceRotation, out _))
            {
                pointingDevices.Add(device);
            }
            if (device.TryGetFeatureValue(CommonUsages.handData, out UnityEngine.XR.Hand hand))
            {
                touchingDevices.Add(device);
                /*
                 * for debuging ML1
                List<Bone> bones = new List<Bone> { };
                hand.TryGetFingerBones(HandFinger.Index, bones);
                foreach (var item in bones)
                {
                    // left here for future debuging exercises.
                }
                */
            }
        }

        private void InputDevices_deviceDisconnected(InputDevice device)
        {
            if (devicesWithPrimaryButton.Contains(device))
                devicesWithPrimaryButton.Remove(device);
            if (joyfulDevices.Contains(device))
                joyfulDevices.Remove(device);
        }
        void Update()
        {
            bool[] buttonsNow = new bool[] { false, false, false, false, false, false, false, false, false, false, false, false, false };
            float[] floatsNow = new float[] { 0, 0, 0, 0, 0, 0 };
            Vector2[] vectorsNow = new Vector2[] { Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero};
            if (!vde.controller.hands.ContainsKey(vde.primaryHand))
            {
                FindFinger();
            }

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
            foreach (var device in devicesWithPrimaryButton)
            {

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
#else
            if (buttonsNow[2] && buttonsNow[5] && !buttonsNow[3])
#endif
            {
                // user has clicked the secondary (y/b) button to select something.
                if (buttonsNow[1] && !buttonsPressedInPreviousFrame[1])
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
#if !MRET_2021_OR_LATER
            if (vde.lazer.laserBeamTransform.gameObject.activeSelf && floatsNow[2] < 0.05 && !buttonsNow[1])
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
            if (!isGrabbing && !vde.lazer.laserBeamTransform.gameObject.activeSelf && (floatsNow[2] > 0.05 || buttonsNow[1]))
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
#else
            if (!isGrabbing && floatsNow[0] > 0.8 && floatsNow[1] > 0.8)
#endif
            {
                isGrabbing = true;
                //vde.lazer.laserBeamTransform.gameObject.SetActive(false);
                inputEvent.Invoke(new Assets.VDE.UI.Input.Event
                {
                    function = Assets.VDE.UI.Input.Event.Function.Grabbing,
                    type = Assets.VDE.UI.Input.Event.Type.GameObject,
                    GameObject = middleFinger,
                    Bool = true
                });                
            }
            // user tried to grab something but flexed its fingers
            // stop grabbing
            else if (isGrabbing && floatsNow[0] < 0.9F && floatsNow[1] < 0.9F)
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

