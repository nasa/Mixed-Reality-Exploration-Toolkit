// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem.SDK.Base;
#if MRET_EXTENSION_MRTK
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
#endif

namespace GOV.NASA.GSFC.XR.CrossPlatformInputSystem.SDK.Hololens
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
        protected override void Start()
        {
            base.Start();

#if !MRET_EXTENSION_MRTK
            Debug.LogWarning("MRTK is required for the Hololens but is not installed.");
#endif
        }

#if MRET_EXTENSION_MRTK
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
        /// Whether or not the pointer is currently on.
        /// </summary>
        public override bool pointerOn
        {
            get
            {
                //I am unsure if these if statements for ui and drawing are necessary since they no longer use raycasts (but some related methods still access pointerOn)
                if (uiPointerController.raycastLaser != null)
                {
                    if (uiPointerController.raycastLaser.active)
                    {
                        return true;
                    }
                }

                if (teleportController.raycastLaser != null)
                {
                    if (teleportController.raycastLaser.active)
                    {
                        return true;
                    }
                }

                if (drawingPointerController.raycastLaser != null)
                {
                    if (drawingPointerController.raycastLaser.active)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// TODO: The current endpoint of the pointer.
        /// </summary>
        public override Vector3 pointerEnd
        {
            get
            {
                if (drawingPointerController.raycastLaser != null)
                {
                    if (drawingPointerController.raycastLaser.active)
                    {
                        return drawingPointerController.raycastLaser.hitPos;
                    }
                }

                if (uiPointerController.raycastLaser != null)
                {
                    return GetComponentInChildren<FingerTracker>().transform.position;
                }

                if (teleportController.raycastLaser != null)
                {
                    if (teleportController.raycastLaser.active)
                    {
                        return teleportController.raycastLaser.hitPos;
                    }
                }

                return Vector3.zero;
            }
        }

        //Added by Rod for updated CheckMenuGesture method functionality
        /// <summary>
        /// Returns whether the opposite hand's "index tip" is touching the input hand's outer "wrist" (Usually where a wrist watch is worn)
        /// </summary>
        private bool OppositeHandTouchingWatch(Handedness hand)
        {
            MixedRealityPose oppHandIndexTip, currHandWatch;

            if (HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexTip, hand.GetOppositeHandedness(), out oppHandIndexTip) &&
                HandJointUtils.TryGetJointPose(TrackedHandJoint.Wrist, hand, out currHandWatch))
            {
                float error_radius = 0.035f;

                        //we need to be within a sphere of error with radius "error_radius"
                return (Vector3.Distance(oppHandIndexTip.Position, currHandWatch.Position) < error_radius &&
                    
                        //only if your wrist is pointing up
                        (Vector3.Dot(currHandWatch.Up, Vector3.up) > 0));
            }
            return false;
        }

        /// <summary>
        /// The menu appears once the user taps their "index knuckle" with their opposite "index tip" all while keeping an open hand
        /// </summary>
        private enum MenuGesturePhase { None, Start, InProgress, Finish }

        private MenuGesturePhase menuGesturePhase = MenuGesturePhase.None;

        private void CheckMenuGesture(Handedness hand)
        {

            switch (menuGesturePhase)
            {
                case MenuGesturePhase.None:
                    if ( !OppositeHandTouchingWatch(hand))
                    {
                        menuGesturePhase = MenuGesturePhase.Start;
                    }
                    break;

                case MenuGesturePhase.Start:
                    if (OppositeHandTouchingWatch(hand))
                    {
                        menuGesturePhase = MenuGesturePhase.InProgress;
                    }
                    break;

                case MenuGesturePhase.InProgress:
                    if ( !OppositeHandTouchingWatch(hand))
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
        //         /// The Old/Simulator menu gesture starts with an open hand, followed by a closed hand, followed by another open hand.
        //         /// </summary>
        // private enum MenuGesturePhase { None, Start, HandClose, Finish }

        // private MenuGesturePhase menuGesturePhase = MenuGesturePhase.None;

        // private void CheckMenuGesture(Handedness hand)
        // {
        //     switch (menuGesturePhase)
        //     {
        //         case MenuGesturePhase.None:
        //             if (AllFingersUp(hand))
        //             {
        //                 menuGesturePhase = MenuGesturePhase.Start;
        //             }
        //             break;

        //         case MenuGesturePhase.Start:
        //             if (AllFingersDown(hand))
        //             {
        //                 menuGesturePhase = MenuGesturePhase.HandClose;
        //             }
        //             break;

        //         case MenuGesturePhase.HandClose:
        //             if (AllFingersUp(hand))
        //             {
        //                 menuGesturePhase = MenuGesturePhase.Finish;

        //                 // Call menu press event.
        //                 inputHand.MenuPressed(transform);
        //             }
        //             break;

        //         case MenuGesturePhase.Finish:
        //             menuGesturePhase = MenuGesturePhase.None;

        //             // Call menu release event.
        //             inputHand.MenuReleased(transform);
        //             break;

        //         default:
        //             Debug.LogError("[InputHandHololens] Menu Gesture Phase state error.");
        //             menuGesturePhase = MenuGesturePhase.None;
        //             break;
        //     }
        // }
        /// <summary>
        /// Whether or not the select gesture is being performed.
        /// </summary>
        /// 
        private bool GetTriFingerPinch(Handedness hand, float error_radius)
        {

            MixedRealityPose indexTip, middleTip, thumbTip;


            if (HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexTip, hand, out indexTip) &&
                HandJointUtils.TryGetJointPose(TrackedHandJoint.MiddleTip, hand, out middleTip) &&
                HandJointUtils.TryGetJointPose(TrackedHandJoint.ThumbTip, hand, out thumbTip))
            {

                //we need to be within a sphere of error with radius "error_radius"
                return (Vector3.Distance(thumbTip.Position, indexTip.Position) < error_radius &&
                        Vector3.Distance(thumbTip.Position, middleTip.Position) < error_radius &&
                        Vector3.Distance(indexTip.Position, middleTip.Position) < error_radius);

            }
            return false;
        }

        private bool isSelecting = false;

        private void CheckSelectGesture(Handedness hand)
        {
            if (!isSelecting)
            {

                if (GetTriFingerPinch(hand, 0.035f))
                {
                    isSelecting = true;
                    inputHand.SelectBegin();
                }

            }
            else
            {
                if (!GetTriFingerPinch(hand, 0.035f))
                {
                    isSelecting = false;
                    inputHand.SelectComplete();
                }

            }
        }

        private bool isDrawing = false;
        /// <summary>
        /// INCOMPLETE METHOD: currently redundant with navigate
        /// </summary>
        /// 
        private void CheckDrawGesture(Handedness hand)
        {
            //TODO: make it context sensitive to only do the navigate press begin when drawing
            if (!_navigatePressing)
            {
                if (PointerUp(hand) && MiddleUp(hand))
                {
                    _navigatePressing = true;

                    // Call navigate press begin event.
                    //inputHand.NavigatePressBegin();
                }    
            }
            else
            {
                if (PointerDown(hand) && MiddleDown(hand))
                {
                    _navigatePressing = false;

                    // Call navigate press complete event.
                    //inputHand.NavigatePressComplete();
                }
            }
            // if (!isDrawing)
            // {
            //     if (PinkyDown(hand) && RingDown(hand) && MiddleDown(hand) && Pinching(hand))
            //     {
            //         isDrawing = true;
            //         inputHand.ToggleDrawingPointerOn();
            //     }
            // }
            // else
            // {

            //     if (!(PinkyDown(hand) && RingDown(hand) && MiddleDown(hand) && Pinching(hand)))
            //     {
            //         isDrawing = false;
            //         inputHand.ToggleDrawingPointerOff();
            //     }
            // }

        }


        private bool isScaling = false;
        /// <summary>
        /// INCOMPLETE METHOD: need to enable and disable scaling where indicated
        /// </summary>
        /// 
        private void CheckScaleGesture(Handedness hand)
        {
            Handedness oppHand = hand.GetOppositeHandedness();

            if (!isScaling)
            {
                if (PinkyUp(hand) && RingUp(hand) && MiddleUp(hand) && Pinching(hand) &&
                    PinkyUp(oppHand) && RingUp(oppHand) && MiddleUp(oppHand) && Pinching(oppHand))
                {
                    isScaling = true;
                    
                }
            }
            else
            {

                if (!(PinkyUp(hand) && RingUp(hand) && MiddleUp(hand) && Pinching(hand) &&
                    PinkyUp(oppHand) && RingUp(oppHand) && MiddleUp(oppHand) && Pinching(oppHand)))
                {
                    isScaling = false;
                    //disable scaling here
                }
            }

        }

        /// <summary>
        /// Whether or not the navigate press gesture is being performed.
        /// </summary>

        private void CheckNavigatePressGesture(Handedness hand)
        {
            if (!_navigatePressing)
            {
                if (PointerUp(hand) && MiddleUp(hand))
                {
                    _navigatePressing = true;

                    // Call navigate press begin event.
                    inputHand.NavigatePressBegin();
                }    
            }
            else
            {
                if (PointerDown(hand) && MiddleDown(hand))
                {
                    _navigatePressing = false;

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

            if (AllFingersDown(hand))
            {
                //if doing grab action

                //and grab hasn't started yet
                if (isGrabbing != true)
                {
                    isGrabbing = true;
                    inputHand.GrabBegin();
                }

            }
            else
            {
                //if not doing grab action

                //but user was previously grabbing
                if (isGrabbing)
                {
                    inputHand.GrabComplete();
                    isGrabbing = false;
                }
            }
        }

        private void CheckAllGestures(Handedness hand)
        {
            CheckMenuGesture(hand);
            CheckSelectGesture(hand);
            CheckNavigatePressGesture(hand);
            CheckGrabGesture(hand);
            CheckScaleGesture(hand);
            //TODO: CheckTeleportGesture(hand);
            CheckDrawGesture(hand);
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
            teleportController.EnterMode();
        }

        public override void DisableTeleportation()
        {
            teleportController.ExitMode();
        }

        public override void ToggleTeleportOn()
        {
            teleportController.ToggleTeleportOn();
        }

        public override void ToggleTeleportOff()
        {
            teleportController.ToggleTeleportOff();
        }

        public override void CompleteTeleport()
        {
            teleportController.CompleteTeleport(inputHand.inputRig.gameObject);
        }

        public override void BlockTeleport()
        {
            teleportController.TeleportBlocked = true;
        }

        public override void UnblockTeleport()
        {
            teleportController.TeleportBlocked = false;
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
#region Drawing
        public override void ToggleDrawingPointerOn()
        {
            drawingPointerController.EnterMode();
            drawingPointerController.TogglePointingOn();
        }

        public override void ToggleDrawingPointerOff()
        {
            drawingPointerController.TogglePointingOff();
            drawingPointerController.ExitMode();
        }
#endregion
#endif
    }

}