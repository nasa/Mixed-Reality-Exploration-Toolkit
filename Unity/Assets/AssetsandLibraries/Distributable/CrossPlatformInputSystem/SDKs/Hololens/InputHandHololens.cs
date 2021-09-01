// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem.SDK.Base;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;

namespace GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem.SDK.SteamVR
{
    /// <remarks>
    /// History:
    /// 11 May 2021: Created
    /// </remarks>
    /// <summary>
    /// Hololens wrapper for the input hand.
    /// Author: Dylan Z. Baker
    /// </summary>
    public class InputHandHololens : InputHandSDK //, IMixedRealityGestureHandler<Vector3>
    {
        /// <summary>
        /// Used to poll navigating.
        /// </summary>
        public override bool navigating
        {
            get
            {
                return _navigateValue != Vector2.zero;
            }
        }

        /// <summary>
        /// Used to poll navigate pressing.
        /// </summary>
        public override bool navigatePressing
        {
            get
            {
                return _navigatePressing;
            }
        }
        private bool _navigatePressing = false;

        /// <summary>
        /// Used to poll navigate value.
        /// </summary>
        public override Vector2 navigateValue
        {
            get
            {
                return _navigateValue;
            }
        }
        private Vector2 _navigateValue = Vector2.zero;

        public float fingerDownThreshold = 0.4f;

        public float pinchThreshold = 0.7f;

        /*public void OnGestureStarted(InputEventData eventData)
        {
            var action = eventData.MixedRealityInputAction.Description;
            if (action == "Tap Action")
            {
                inputHand.SelectBegin();
                ARDebugController.Log("Tap begin.");
            }
            else if (action == "Hold Action")
            {
                inputHand.GrabBegin();
                ARDebugController.Log("Hold begin.");
            }
            else if (action == "Manipulate Action")
            {
                ARDebugController.Log("Manipulate begin.");
            }
            else if (action == "Navigation Action")
            {
                inputHand.NavigatePressBegin();
                ARDebugController.Log("Navigate begin.");
            }
        }

        public void OnGestureUpdated(InputEventData eventData)
        {
            var action = eventData.MixedRealityInputAction.Description;
            if (action == "Tap Action")
            {
                ARDebugController.Log("Tap update.");
            }
            else if (action == "Hold Action")
            {
                ARDebugController.Log("Hold update.");
            }
            else if (action == "Manipulate Action")
            {
                ARDebugController.Log("Manipulate update.");
            }
            else if (action == "Navigation Action")
            {
                ARDebugController.Log("Navigate update.");
            }
        }

        public void OnGestureUpdated(InputEventData<Vector3> eventData)
        {
            var action = eventData.MixedRealityInputAction.Description;
            if (action == "Tap Action")
            {
                ARDebugController.Log("Tap update..");
            }
            else if (action == "Hold Action")
            {
                ARDebugController.Log("Hold update..");
            }
            else if (action == "Manipulate Action")
            {
                ARDebugController.Log("Manipulate update..");
            }
            else if (action == "Navigation Action")
            {
                ARDebugController.Log("Navigate update..");
            }
        }

        public void OnGestureCompleted(InputEventData eventData)
        {
            var action = eventData.MixedRealityInputAction.Description;
            if (action == "Tap Action")
            {
                inputHand.SelectComplete();
                ARDebugController.Log("Tap complete.");
            }
            else if (action == "Hold Action")
            {
                inputHand.GrabComplete();
                ARDebugController.Log("Hold complete.");
            }
            else if (action == "Manipulate Action")
            {
                ARDebugController.Log("Manipulate complete.");
            }
            else if (action == "Navigation Action")
            {
                inputHand.NavigatePressComplete();
                ARDebugController.Log("Navigate complete.");
            }
        }

        public void OnGestureCompleted(InputEventData<Vector3> eventData)
        {
            var action = eventData.MixedRealityInputAction.Description;
            if (action == "Tap Action")
            {
                inputHand.SelectComplete();
                ARDebugController.Log("Tap complete..");
            }
            else if (action == "Hold Action")
            {
                inputHand.GrabComplete();
                ARDebugController.Log("Hold complete..");
            }
            else if (action == "Manipulate Action")
            {
                ARDebugController.Log("Manipulate complete..");
            }
            else if (action == "Navigation Action")
            {
                inputHand.NavigatePressComplete();
                ARDebugController.Log("Navigate complete..");
            }
        }

        public void OnGestureCanceled(InputEventData eventData)
        {
            var action = eventData.MixedRealityInputAction.Description;
            if (action == "Tap Action")
            {
                inputHand.SelectComplete();
                ARDebugController.Log("Tap cancel.");
            }
            else if (action == "Hold Action")
            {
                inputHand.GrabComplete();
                ARDebugController.Log("Hold cancel.");
            }
            else if (action == "Manipulate Action")
            {
                ARDebugController.Log("Manipulate cancel.");
            }
            else if (action == "Navigation Action")
            {
                inputHand.NavigatePressComplete();
                ARDebugController.Log("Navigate cancel.");
            }
        }*/

        private bool ThumbUp(Handedness hand)
        {
            return HandPoseUtils.ThumbFingerCurl(hand) <= fingerDownThreshold;
        }

        private bool ThumbDown(Handedness hand)
        {
            return HandPoseUtils.ThumbFingerCurl(hand) > fingerDownThreshold;
        }

        private bool PointerUp(Handedness hand)
        {
            return HandPoseUtils.IndexFingerCurl(hand) <= fingerDownThreshold;
        }

        private bool PointerDown(Handedness hand)
        {
            return HandPoseUtils.IndexFingerCurl(hand) > fingerDownThreshold;
        }

        private bool MiddleUp(Handedness hand)
        {
            return HandPoseUtils.MiddleFingerCurl(hand) <= fingerDownThreshold;
        }

        private bool MiddleDown(Handedness hand)
        {
            return HandPoseUtils.MiddleFingerCurl(hand) > fingerDownThreshold;
        }

        private bool RingUp(Handedness hand)
        {
            return HandPoseUtils.RingFingerCurl(hand) <= fingerDownThreshold;
        }

        private bool RingDown(Handedness hand)
        {
            return HandPoseUtils.RingFingerCurl(hand) > fingerDownThreshold;
        }

        private bool PinkyUp(Handedness hand)
        {
            return HandPoseUtils.PinkyFingerCurl(hand) <= fingerDownThreshold;
        }

        private bool PinkyDown(Handedness hand)
        {
            return HandPoseUtils.PinkyFingerCurl(hand) > fingerDownThreshold;
        }

        private bool Pinching(Handedness hand)
        {
            return HandPoseUtils.CalculateIndexPinch(hand) > pinchThreshold;
        }

        private bool AllFingersUp(Handedness hand)
        {
            return ThumbUp(hand) && PointerUp(hand) && MiddleUp(hand) && RingUp(hand) && PinkyUp(hand);
        }

        private bool AllFingersDown(Handedness hand)
        {
            return ThumbDown(hand) && PointerDown(hand) && MiddleDown(hand) && RingDown(hand) && PinkyDown(hand);
        }

        private bool GivingTheFinger(Handedness hand)
        {
            return ThumbDown(hand) && PointerDown(hand) && MiddleUp(hand) && RingDown(hand) && PinkyDown(hand);
        }

        /// <summary>
        /// The menu gesture starts with an open hand, followed by a closed hand, followed by another open hand.
        /// </summary>
        private enum MenuGesturePhase { None, Start, HandClose, Finish }

        private MenuGesturePhase menuGesturePhase = MenuGesturePhase.None;

        private void CheckMenuGesture(Handedness hand)
        {
            switch (menuGesturePhase)
            {
                case MenuGesturePhase.None:
                    if (AllFingersUp(hand))
                    {
                        menuGesturePhase = MenuGesturePhase.Start;
                    }
                    break;

                case MenuGesturePhase.Start:
                    if (AllFingersDown(hand))
                    {
                        menuGesturePhase = MenuGesturePhase.HandClose;
                    }
                    break;

                case MenuGesturePhase.HandClose:
                    if (AllFingersUp(hand))
                    {
                        menuGesturePhase = MenuGesturePhase.Finish;

                        // Call menu press event.
                        inputHand.MenuPressed(transform);
                    }
                    break;

                case MenuGesturePhase.Finish:
                    menuGesturePhase = MenuGesturePhase.None;

                    // Call menu release event.
                    inputHand.MenuReleased(transform);
                    break;

                default:
                    Debug.LogError("[InputHandHololens] Menu Gesture Phase state error.");
                    menuGesturePhase = MenuGesturePhase.None;
                    break;
            }
        }

        /// <summary>
        /// Whether or not the select gesture is being performed.
        /// </summary>
        private bool isSelecting = false;

        private void CheckSelectGesture(Handedness hand)
        {
            if (isSelecting)
            {
                if (PointerUp(hand))
                {
                    isSelecting = false;

                    // Call select begin event.
                    inputHand.SelectBegin();
                }
            }
            else
            {
                if (PointerDown(hand))
                {
                    isSelecting = true;

                    // Call select complete event.
                    inputHand.SelectComplete();
                }
            }
        }

        /// <summary>
        /// Whether or not the navigate press gesture is being performed.
        /// </summary>
        private bool isNavigatePressing = false;

        private void CheckNavigatePressGesture(Handedness hand)
        {
            if (isNavigatePressing)
            {
                if (PointerUp(hand) && MiddleUp(hand))
                {
                    isNavigatePressing = false;

                    // Call navigate press begin event.
                    inputHand.NavigatePressBegin();
                }    
            }
            else
            {
                if (PointerDown(hand) && MiddleDown(hand))
                {
                    isNavigatePressing = true;

                    // Call navigate press complete event.
                    inputHand.NavigatePressComplete();
                }
            }
        }

        /// <summary>
        /// Whether or not the grab gesture is being performed.
        /// </summary>
        private bool isGrabbing = false;

        private void CheckGrabGesture(Handedness hand)
        {
            if (isGrabbing)
            {
                if (!Pinching(hand))
                {
                    isGrabbing = false;

                    // Call grab begin event.
                    inputHand.GrabBegin();
                }
            }
            else
            {
                if (Pinching(hand))
                {
                    isGrabbing = true;

                    // Call grab complete event.
                    inputHand.GrabComplete();
                }
            }
        }

        private void CheckAllGestures(Handedness hand)
        {
            CheckMenuGesture(hand);
            CheckSelectGesture(hand);
            CheckNavigatePressGesture(hand);
            CheckGrabGesture(hand);
        }

        /*private void OnEnable()
        {
            //CoreServices.InputSystem?.RegisterHandler<IMixedRealityGestureHandler<Vector3>>(this);
        }

        private void OnDisable()
        {
            //CoreServices.InputSystem?.UnregisterHandler<IMixedRealityGestureHandler<Vector3>>(this);
        }*/

        private void Update()
        {
            IMixedRealityHandJointService jointService = CoreServices.GetInputSystemDataProvider<IMixedRealityHandJointService>();
            if (jointService != null)
            {
                Transform jointTransform = jointService.RequestJointTransform(TrackedHandJoint.Palm,
                    inputHand.handedness == InputHand.Handedness.left ? Handedness.Left : Handedness.Right);
                if (jointTransform != null)
                {
                    transform.position = jointTransform.position;
                    transform.rotation = jointTransform.rotation;
                }
            }

            CheckAllGestures(inputHand.handedness == InputHand.Handedness.left ? Handedness.Left : Handedness.Right);
        }

#region Locomotion

#region Locomotion [Teleport]

        public override void EnableTeleportation()
        {

        }

        public override void DisableTeleportation()
        {

        }

        public override void ToggleTeleportOn()
        {

        }

        public override void ToggleTeleportOff()
        {

        }

        public override void CompleteTeleport()
        {

        }

        public override void BlockTeleport()
        {

        }

        public override void UnblockTeleport()
        {

        }

#endregion // Locomotion [Teleport]

#region Locomotion [Armswing]

        /// <seealso cref="InputHandSDK.EnableArmswing"/>
        public override void EnableArmswing()
        {

        }

        /// <seealso cref="InputHandSDK.DisableArmswing"/>
        public override void DisableArmswing()
        {

        }

#endregion // Locomotion [Armswing]

#region Locomotion [Fly]

        /// <seealso cref="InputHandSDK.EnableFly"/>
        public override void EnableFly()
        {

        }

        /// <seealso cref="InputHandSDK.DisableFly"/>
        public override void DisableFly()
        {

        }

#endregion // Locomotion [Fly]

#region Locomotion [Navigate]

        /// <seealso cref="InputHandSDK.EnableNavigate"/>
        public override void EnableNavigate()
        {
            // Notify the locomotion controller that we are enabled
            if (_navigationController != null) _navigationController.SetHandActiveState(this.inputHand, true);
        }

        /// <seealso cref="InputHandSDK.DisableNavigate"/>
        public override void DisableNavigate()
        {
            // Notify the locomotion controller that we are disabled
            if (_navigationController != null) _navigationController.SetHandActiveState(this.inputHand, false);
        }

#endregion // Locomotion [Navigate]

#endregion // Locomotion

#region UI Handling

        public override void ToggleUIPointerOn(bool soft, bool showWhenInvalid)
        {
            if (uiPointerController == null)
            {
                return;
            }

            uiPointerController.EnterMode();
            uiPointerController.ToggleUIPointingOn(soft);
            uiPointerController.showInvalid = showWhenInvalid;
        }

        public override void ToggleUIPointerOff(bool soft)
        {
            if (uiPointerController == null)
            {
                return;
            }

            uiPointerController.ToggleUIPointingOff(soft);
            uiPointerController.ExitMode();
        }

        public override void UIPointerSelect()
        {
            if (uiPointerController == null)
            {
                return;
            }

            uiPointerController.Select();
        }

#endregion

    }
}