// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
using GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem;

namespace GSFC.ARVR.MRET.Infrastructure.Components.Locomotion
{
    /// <remarks>
    /// History:
    /// 2 February 2021: Created
    /// 17 March 2021: Added the motion constraint properties and multipliers to support fast, normal
    ///     and slow motion to centralize the logic for locomotion controllers. Added implementation of
    ///     IInputRigLocomotionControl to begin isolating the cross platform input system from the MRET
    ///     application. Added reference to the input head for subclasses, and wrapped debug messages in
    ///     the MRET_DEBUG compiler directive. (J. Hosler)
    /// </remarks>
	///
	/// <summary>
	/// LocomotionController
	///
	/// Abstract class providing common capabilities for locomotion controllers
	///
    /// Author: Jeffrey C. Hosler
	/// </summary>
	/// 
	public abstract class LocomotionController : MRETUpdateBehaviour, IInputRigLocomotionControl
    {
        /// <summary>
        /// Internal class used to track hand active state
        /// </summary>
        protected class InputHandActiveState
        {
            public InputHand hand = null;
            public bool active = false;
        }

        public const float DEFAULT_MOTION_MULTIPLIER_NORMAL = 1.0f;
        public const float DEFAULT_MOTION_MULTIPLIER_SLOW = 0.5f;
        public const float DEFAULT_MOTION_MULTIPLIER_FAST = 1.5f;

        /// <summary>
        /// Motion step multiplier to exagerate (values > 1) or limit movement (values < 1)
        /// </summary>
        [Tooltip("Normal motion step multiplier to exagerate (values > 1) or limit movement (values < 1)")]
        public float motionNormalMultiplier = DEFAULT_MOTION_MULTIPLIER_NORMAL;

        /// <summary>
        /// Slow motion step multiplier to exagerate (values > 1) or limit movement (values < 1)
        /// </summary>
        [Tooltip("Slow motion step multiplier to exagerate (values > 1) or limit movement (values < 1)")]
        public float motionSlowMultiplier = DEFAULT_MOTION_MULTIPLIER_SLOW;

        /// <summary>
        /// Fast motion step multiplier to exagerate (values > 1) or limit movement (values < 1)
        /// </summary>
        [Tooltip("Fast motion step multiplier to exagerate (values > 1) or limit movement (values < 1)")]
        public float motionFastMultiplier = DEFAULT_MOTION_MULTIPLIER_FAST;

        /// <summary>
        /// Indicates whether or not the active state of the hands are mutually exclusive.
        /// </summary>
        [Tooltip("Indicates whether or not the active state of the hands are mutually exclusive, i.e. True indicates only one hand can be active at a time.")]
        public bool handsMutuallyExclusive = false;

        [Tooltip("Specifies the contraint that gravity has on this control.")]
        public GravityConstraint gravityConstraint = GravityConstraint.Allowed;

        protected InputRig rig;

        protected InputHead head;

        protected InputHandActiveState[] handActiveStates;

        /// <summary>
        /// Indicates this locomotion control is active
        /// </summary>
        private bool _controlActive = false;
        public bool ControlActive
        {
            set
            {
                SetControlActive(value);
            }
            get
            {
                return GetControlActive();
            }
        }

        /// <summary>
        /// Available to subclasses to perform functionality before the control active state is changed
        /// </summary>
        /// 
        /// <param name="value">The new control active state</param>
        protected virtual void ControlStateChanging(bool value)
        {
#if MRET_DEBUG
            Debug.Log("[" + ClassName + "] Control state changing: " + value);
#endif
        }

        /// <summary>
        /// Available to subclasses to perform functionality after the control active state is changed
        /// </summary>
        protected virtual void ControlStateChanged()
        {
#if MRET_DEBUG
            Debug.Log("[" + ClassName + "] Control state changed");
#endif
        }

#region IInputRigLocomotionControl

        /// <seealso cref="IInputRigLocomotionControl.GetControlActive"/>
        public bool GetControlActive()
        {
            return _controlActive;
        }

        /// <seealso cref="IInputRigLocomotionControl.SetControlActive(bool)"/>
        public void SetControlActive(bool value)
        {
            if (value != _controlActive)
            {
                ControlStateChanging(value);
                _controlActive = value;
                ControlStateChanged();
            }
        }

        /// <summary>
        /// The motion constraint being used for locomotion.
        /// </summary>
        public MotionConstraint MotionConstraint
        {
            get
            {
                return GetMotionConstraint();
            }
        }

        /// <seealso cref="IInputRigLocomotionControl.GetMotionConstraint"/>
        public MotionConstraint GetMotionConstraint()
        {
            return rig.MotionConstraint;
        }

        /// <seealso cref="IInputRigLocomotionControl.GetMotionMultiplier"/>
        public float GetMotionMultiplier()
        {
            float result;

            switch(MotionConstraint)
            {
                case MotionConstraint.Slow:
                    result = motionSlowMultiplier;
                    break;

                case MotionConstraint.Fast:
                    result = motionFastMultiplier;
                    break;

                case MotionConstraint.Normal:
                default:
                    result = motionNormalMultiplier;
                    break;

            }

            return result;
        }

        /// <seealso cref="IInputRigLocomotionControl.GetMotionConstraintMultiplier(MotionConstraint)"/>
        public float GetMotionConstraintMultiplier(MotionConstraint constraint)
        {
            float result;

            switch (constraint)
            {
                case MotionConstraint.Slow:
                    result = motionSlowMultiplier;
                    break;

                case MotionConstraint.Fast:
                    result = motionFastMultiplier;
                    break;

                case MotionConstraint.Normal:
                default:
                    result = motionNormalMultiplier;
                    break;

            }

            return result;
        }

        /// <seealso cref="IInputRigLocomotionControl.SetMotionConstraintMultiplier(MotionConstraint, float)"/>
        public void SetMotionConstraintMultiplier(MotionConstraint constraint, float value)
        {
            switch (constraint)
            {
                case MotionConstraint.Slow:
                    motionSlowMultiplier = value;
                    break;

                case MotionConstraint.Fast:
                    motionFastMultiplier = value;
                    break;

                case MotionConstraint.Normal:
                default:
                    motionNormalMultiplier = value;
                    break;

            }
        }

        /// <seealso cref="IInputRigLocomotionControl.GetGravityConstraint"/>
        public GravityConstraint GetGravityConstraint()
        {
            return gravityConstraint;
        }

        /// <seealso cref="IInputRigLocomotionControl.SetGravityConstraint"/>
        public void SetGravityConstraint(GravityConstraint value)
        {
            gravityConstraint = value;
        }

        /// <seealso cref="IInputRigLocomotionControl.SetHandActiveState"/>
        public void SetHandActiveState(InputHand hand, bool value)
        {
            // Obtain the active state object
            InputHandActiveState handActiveState = GetHandActiveState(hand);

            // Make sure we have a valid reference
            if (handActiveState == null) return;

            // We will change the state if:
            //   1) Value is actually changing, AND
            //   2) State change is allowed
            if (value != handActiveState.active)
            {
                // Indicator that we will allow the state change:
                //   1) False is allowed, OR
                //   2) Attempting to activate AND we can activate
                bool stateChangeAllowed = !value || (value && CanActivateHand(hand));

                // Make sure we are allowed to make the state change
                if (stateChangeAllowed)
                {
                    HandActiveStateChanging(hand, value);
                    handActiveState.active = value;
                    HandActiveStateChanged(hand);
                }
            }
        }

#endregion IInputRigLocomotionControl

        /// <summary>
        /// Obtains the active state object associatedd with the supplied hand
        /// </summary>
        /// <param name="hand"></param>
        /// <returns>A <code>InputHandActiveState</code> representing the supplied hand of NULL if no association</returns>
        protected InputHandActiveState GetHandActiveState(InputHand hand)
        {
            InputHandActiveState result = null;

            try
            {
                // Make sure the hand exists in the rig (should always be true)
                if (rig.hands.Contains(hand))
                {
                    // Locate the hand active state object that has this hand reference
                    result = Array.Find(handActiveStates, handActiveState => (handActiveState.hand == hand));
                }
            }
            catch (ArgumentNullException)
            {
                // Report the issue and continue
                Debug.LogWarning("[" + ClassName + "] Unexpected state encountered. Hand active state reference could not be located: " + hand.name);
            }

            return result;
        }

        /// <summary>
        /// Indicates if the supplied hand is active
        /// </summary>
        /// <param name="hand"></param>
        /// <returns>A boolean value indicating whether or not the hand is active</returns>
        protected bool IsHandActive(InputHand hand)
        {
            // Obtain the active state object
            InputHandActiveState handActiveState = GetHandActiveState(hand);

            // Make sure the active state reference is valid
            return (handActiveState != null) && handActiveState.active;
        }

        /// <summary>
        /// Indicates if the supplied hand can be activated based upon mutual exclusivity settings.
        /// </summary>
        /// <param name="hand"></param>
        /// <returns>A boolean value indicating whether or not the hand can be activated</returns>
        protected bool CanActivateHand(InputHand hand)
        {
            // Obtain the active state object
            InputHandActiveState handActiveState = GetHandActiveState(hand);

            // Indicator that we will allow the state change
            bool result = (handActiveState != null);

            // Make sure this hand will be mutually exlusively active
            if (handsMutuallyExclusive)
            {
                // Make sure the other hands are not already active
                foreach (InputHand rigHand in rig.hands)
                {
                    // Ignore this hand
                    if (hand != rigHand)
                    {
                        result &= !IsHandActive(rigHand);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the array of active hands
        /// </summary>
        /// <returns>An array of <code>InputHand</code> references for all active hands</returns>
        protected InputHand[] GetActiveHands()
        {
            List<InputHand> activeHands = new List<InputHand>();

            // Check all available hands
            foreach (InputHand rigHand in rig.hands)
            {
                // If the hand is active, add it to the list
                if (IsHandActive(rigHand))
                {
                    activeHands.Add(rigHand);
                }
            }

            return activeHands.ToArray();
        }

        /// <summary>
        /// Available to subclasses to perform functionality before the supplied hand active state is changed
        /// </summary>
        /// 
        /// <param name="hand">The hand that is changing state</param>
        /// <param name="value">The new active state</param>
        protected virtual void HandActiveStateChanging(InputHand hand, bool value)
        {
#if MRET_DEBUG
            Debug.Log("[" + ClassName + "] Hand active state changing: " + hand.name + "; Active: " + value);
#endif
        }

        /// <summary>
        /// Available to subclasses to perform functionality after the supplied hand active state is changed
        /// </summary>
        /// <param name="hand">The hand that changed state</param>
        protected virtual void HandActiveStateChanged(InputHand hand)
        {
#if MRET_DEBUG
            Debug.Log("[" + ClassName + "] Hand active state changed: " + hand.name);
#endif
        }

        /// <seealso cref="MRETBehaviour.IntegrityCheck"/>
        protected override IntegrityState IntegrityCheck()
		{
			// Take the inherited behavior
			IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            return (
                (state == IntegrityState.Failure) ||
                (rig == null) ||
                (head == null) ||
                (handActiveStates.Length == 0)
                    ? IntegrityState.Failure   // Fail if base class fails, OR rig, head or hands are null
                    : IntegrityState.Success); // Otherwise, our integrity is valid
		}

        /// <summary>
        /// DoLocomotion
        /// 
        /// Performs the locomotion by moving the rig to a new position. Only called if the control is active.
        /// </summary>
        /// <see cref="ControlActive"/>
        protected abstract void DoLocomotion();

        /// <seealso cref="MRETUpdateBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            // Take the inherited behavior
            base.MRETStart();

            // Get the internal references needed to perform locomotion
            rig = Framework.MRET.InputRig;
            head = Framework.MRET.InputRig.head;

            // Build the state tracking for the hands
            List<InputHandActiveState> handStateList = new List<InputHandActiveState>();
            foreach (InputHand inputHand in rig.hands)
            {
                // Create a hand state tracker for this hand
                InputHandActiveState handState = new InputHandActiveState
                {
                    hand = inputHand,
                    active = false
                };
                handStateList.Add(handState);
            }

            // Store the hand state array
            handActiveStates = handStateList.ToArray();
        }

        /// <seealso cref="MRETUpdateBehaviour.MRETUpdate"/>
        protected override void MRETUpdate()
        {
            // Take the inherited behavior
            base.MRETUpdate();

            // Perform the locomotion if we are active
            if (ControlActive) DoLocomotion();
        }

    }
}
