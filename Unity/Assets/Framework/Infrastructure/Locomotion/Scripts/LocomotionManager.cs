// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;
using GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem;

namespace GSFC.ARVR.MRET.Infrastructure.Framework.Locomotion
{
    /// <remarks>
    /// History:
    /// 8 December 2020: Created
    /// 03 February 2021: Added ArmSwing locomotion (J. Hosler)
    /// 16 February 2021: Extended MRETBehavior, added logging and general cleanup (J. Hosler)
    /// 13 March 2021: Added gravity state machine implementation (J. Hosler)
    /// 17 March 2021: Replaced the speed references with the new motion constraint properties, multipliers
    ///     and associated data store keys to to support fast, normal and slow motion for each of the
    ///     locomotion controllers. Added logic to receive the navigation press events to control fast/normal
    ///     motion constraints. (J. Hosler)
    /// </remarks>
    /// <summary>
    /// LocomotionManager is a class that provides
    /// top-level control of locomotion in MRET.
    /// Author: Dylan Z. Baker
    /// </summary>
    public class LocomotionManager : MRETBehaviour
    {
        public enum Mode
        {
            Teleport,
            Armswing,
            Fly,
            Navigate
        }

        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName
        {
            get
            {
                return nameof(LocomotionManager);
            }
        }

        public static LocomotionManager instance;

        // TODO: Move constants to central class.

        // Prefix of all keys
        public const string KEY_PREFIX = "MRET.INTERNAL.LOCOMOTION";
        //TODO: ? public static readonly string KEY_PREFIX = typeof(LocomotionManager).Namespace.ToUpper();

        // DataManager keys
        public const string teleportKey = KEY_PREFIX + ".MODE.TELEPORT";
        //TODO: ? public static readonly string teleportKey = KEY_PREFIX + "." + nameof(Mode).ToUpper() + "." + Mode.Teleport.ToString().ToUpper();
        public const string teleportMaxDistanceKey = KEY_PREFIX + ".MODE.TELEPORT.MAXDISTANCE";
        public const string flyKey = KEY_PREFIX + ".MODE.FLY";
        public const string flyMotionConstraintMultiplerNormalKey = KEY_PREFIX + ".MODE.FLY.MOTIONCONSTRAINTMULTIPLIER.NORMAL";
        public const string flyMotionConstraintMultiplerSlowKey = KEY_PREFIX + ".MODE.FLY.MOTIONCONSTRAINTMULTIPLIER.SLOW";
        public const string flyMotionConstraintMultiplerFastKey = KEY_PREFIX + ".MODE.FLY.MOTIONCONSTRAINTMULTIPLIER.FAST";
        public const string flyMotionConstraintKey = KEY_PREFIX + ".MODE.FLY.MOTIONCONSTRAINT";
        public const string flyGravityConstraintKey = KEY_PREFIX + ".MODE.FLY.GRAVITYCONSTRAINT";
        public const string navigateKey = KEY_PREFIX + ".MODE.NAVIGATE";
        public const string navigateMotionConstraintMultiplerNormalKey = KEY_PREFIX + ".MODE.NAVIGATE.MOTIONCONSTRAINTMULTIPLIER.NORMAL";
        public const string navigateMotionConstraintMultiplerSlowKey = KEY_PREFIX + ".MODE.NAVIGATE.MOTIONCONSTRAINTMULTIPLIER.SLOW";
        public const string navigateMotionConstraintMultiplerFastKey = KEY_PREFIX + ".MODE.NAVIGATE.MOTIONCONSTRAINTMULTIPLIER.FAST";
        public const string navigateMotionConstraintKey = KEY_PREFIX + ".MODE.NAVIGATE.MOTIONCONSTRAINT";
        public const string navigateGravityConstraintKey = KEY_PREFIX + ".MODE.NAVIGATE.GRAVITYCONSTRAINT";
        public const string armswingKey = KEY_PREFIX + ".MODE.ARMSWING";
        public const string armswingMotionConstraintMultiplerNormalKey = KEY_PREFIX + ".MODE.ARMSWING.MOTIONCONSTRAINTMULTIPLIER.NORMAL";
        public const string armswingMotionConstraintMultiplerSlowKey = KEY_PREFIX + ".MODE.ARMSWING.MOTIONCONSTRAINTMULTIPLIER.SLOW";
        public const string armswingMotionConstraintMultiplerFastKey = KEY_PREFIX + ".MODE.ARMSWING.MOTIONCONSTRAINTMULTIPLIER.FAST";
        public const string armswingMotionConstraintKey = KEY_PREFIX + ".MODE.ARMSWING.MOTIONCONSTRAINT";
        public const string armswingGravityConstraintKey = KEY_PREFIX + ".MODE.ARMSWING.GRAVITYCONSTRAINT";
        public const string gravityKey = KEY_PREFIX + ".GRAVITY";
        public const string rotateXKey = KEY_PREFIX + ".ROTATEX";
        public const string rotateYKey = KEY_PREFIX + ".ROTATEY";
        public const string rotateZKey = KEY_PREFIX + ".ROTATEZ";
        public const string scaleKey = KEY_PREFIX + ".SCALE";

        /// <summary>
        /// Input Rig to be used with locomotion.
        /// </summary>
        public InputRig inputRig;

        /// <summary>
        /// The rotate object transforms.
        /// </summary>
        protected RotateObjectTransform[] rots;

        /// <summary>
        /// The scale object transforms.
        /// </summary>
        protected ScaleObjectTransform[] sots;

        /// <value>Locomotion pause state</value>
        private bool _paused = false;
        public bool Paused
        {
            set
            {
                SetPaused(value);
            }
            get
            {
                return _paused;
            }
        }

        /// <summary>
        /// Sets the locomotion paused state
        /// </summary>
        /// 
        /// <param name="value">The new locomotion paused state</param>
        protected void SetPaused(bool value)
        {
            // If pausing, disable all locomotion modes
            _paused = value;
            if (_paused)
            {
                // TODO: Disable without disabling the mode setting in the DataManager
                DisableAllLocomotionModes();
            }
        }

        /// <summary>
        /// Performs the initialization of this manager
        /// </summary>
        public void Initialize()
        {
            instance = this;

            // Initialize DataManager
            MRET.DataManager.SaveValue(new DataManager.DataValue(teleportKey, false));
            MRET.DataManager.SaveValue(new DataManager.DataValue(flyKey, false));
            MRET.DataManager.SaveValue(new DataManager.DataValue(navigateKey, false));
            MRET.DataManager.SaveValue(new DataManager.DataValue(armswingKey, false));

            // Create the rig references that we need to perform rig motion
            List<RotateObjectTransform> rotsList = new List<RotateObjectTransform>();
            List<ScaleObjectTransform> sotsList = new List<ScaleObjectTransform>();
            foreach (InputHand inputHand in inputRig.hands)
            {
                // Obtain the rotation for the hand if it exists
                RotateObjectTransform rot = inputHand.GetComponentInChildren<RotateObjectTransform>();

                // We found it, so only add if it's not in the list already
                if (rot && !rotsList.Contains(rot))
                {
                    rotsList.Add(rot);
                }

                // Obtain the scale for the hand if it exists
                ScaleObjectTransform sot = inputHand.GetComponentInChildren<ScaleObjectTransform>();

                // We found it, so only add if it's not in the list already
                if (sot && !sotsList.Contains(sot))
                {
                    sotsList.Add(sot);
                }
            }

            // Store in our class arrays for quick access
            rots = rotsList.ToArray();
            sots = sotsList.ToArray();
        }

        /// <summary>
        /// Overridden to update settings based upon initial MRET settings now that everything should be initialized.
        /// </summary>
        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            // Take the inherited behavior
            base.MRETStart();

            // References are initialized now, so update data manager keys based upon initial MRET settings,
            // where appropriate
            MRET.DataManager.SaveValue(new DataManager.DataValue(gravityKey, inputRig.GravityEnabled));

            // Rotation
            bool rotXEnabled = false;
            bool rotYEnabled = false;
            bool rotZEnabled = false;
            foreach (RotateObjectTransform rot in rots)
            {
                // Rotation axes are enabled if any rot axes are enabled
                rotXEnabled |= rot.xEnabled;
                rotYEnabled |= rot.yEnabled;
                rotZEnabled |= rot.zEnabled;
            }
            MRET.DataManager.SaveValue(new DataManager.DataValue(rotateXKey, rotXEnabled));
            MRET.DataManager.SaveValue(new DataManager.DataValue(rotateYKey, rotYEnabled));
            MRET.DataManager.SaveValue(new DataManager.DataValue(rotateZKey, rotZEnabled));

            // Scale
            bool scaleEnabled = false;
            foreach (ScaleObjectTransform sot in sots)
            {
                // Scale is enabled if any sots are enabled
                scaleEnabled |= sot.enabled;
            }
            MRET.DataManager.SaveValue(new DataManager.DataValue(scaleKey, scaleEnabled));
        }

        /// <summary>
        /// Disables all locomotion mechanisms.
        /// </summary>
        public void DisableAllLocomotionModes()
        {
            DisableArmswing();
            DisableFlying();
            DisableNavigate();
            DisableTeleport();
        }

#region Event Handlers

        /// <summary>
        /// Called when the navigate press action begins.
        /// </summary>
        /// <param name="hand"><code>InputHand</code> that initiated the action.</param>
        public void OnNavigatePressBegin(InputHand hand)
        {
#if MRET_DEBUG
            Debug.Log("[" + ClassName + "->" + nameof(OnNavigatePressBegin) + "] Detected. Hand: " + hand.name);
#endif

            // If paused, do nothing
            if (Paused) return;

            // Set the motion constraint for the rig if one of the locomotion modes are active
            if (((bool)MRET.DataManager.FindPoint(armswingKey) == true) ||
                ((bool)MRET.DataManager.FindPoint(flyKey) == true) ||
                ((bool)MRET.DataManager.FindPoint(navigateKey) == true))
            {
                inputRig.MotionConstraint = MotionConstraint.Fast;
            }
        }

        /// <summary>
        /// Called when the navigate press action completes.
        /// </summary>
        /// <param name="hand"><code>InputHand</code> that completed the action.</param>
        public void OnNavigatePressComplete(InputHand hand)
        {
#if MRET_DEBUG
            Debug.Log("[" + ClassName + "->" + nameof(OnNavigatePressComplete) + "] Detected. Hand: " + hand.name);
#endif

            // If paused, do nothing
            if (Paused) return;

            // Set the motion constraint for the rig if one of the locomotion modes are active
            if (((bool)MRET.DataManager.FindPoint(armswingKey) == true) ||
                ((bool)MRET.DataManager.FindPoint(flyKey) == true) ||
                ((bool)MRET.DataManager.FindPoint(navigateKey) == true))
            {
                inputRig.MotionConstraint = MotionConstraint.Normal;
            }
        }

        /// <summary>
        /// Called when the grab action begins.
        /// </summary>
        /// <param name="hand"><code>InputHand</code> that initiated the action.</param>
        public void OnGrabBegin(InputHand hand)
        {
#if MRET_DEBUG
            Debug.Log("[" + ClassName + "->" + nameof(OnGrabBegin) + "] Detected. Hand: " + hand.name);
#endif

            // If paused, do nothing
            if (Paused) return;

            // Enable locomotion for the active mode
            if ((bool)MRET.DataManager.FindPoint(armswingKey) == true)
            {
                hand.EnableArmswing();
            }
            else if ((bool)MRET.DataManager.FindPoint(flyKey) == true)
            {
                hand.EnableFly();
            }

            // TODO other locomotion forms.
        }

        /// <summary>
        /// Called when the grab action completes.
        /// </summary>
        /// <param name="hand"><code>InputHand</code> that completed the action.</param>
        public void OnGrabComplete(InputHand hand)
        {
#if MRET_DEBUG
            Debug.Log("[" + ClassName + "->" + nameof(OnGrabComplete) + "] Detected. Hand: " + hand.name);
#endif

            // If paused, do nothing
            if (Paused) return;

            // Disable locomotion for the active mode
            if ((bool) MRET.DataManager.FindPoint(armswingKey) == true)
            {
                hand.DisableArmswing();
            }
            else if ((bool) MRET.DataManager.FindPoint(flyKey) == true)
            {
                hand.DisableFly();
            }

            // TODO other locomotion forms.
        }

        /// <summary>
        /// Called when the navigate action begins.
        /// </summary>
        /// <param name="hand"><code>InputHand</code> that initiated the action.</param>
        public void OnNavigateBegin(InputHand hand)
        {
#if MRET_DEBUG
            Debug.Log("[" + ClassName + "->" + nameof(OnNavigateBegin) + "] Detected");
#endif

            // If paused, do nothing
            if (Paused) return;

            // Enable locomotion for the active mode
            if ((bool) MRET.DataManager.FindPoint(navigateKey) == true)
            {
                hand.EnableNavigate();
            }

            // TODO other locomotion forms.
        }

        /// <summary>
        /// Called when the navigate action completes.
        /// </summary>
        /// <param name="hand"><code>InputHand</code> that completed the action.</param>
        public void OnNavigateComplete(InputHand hand)
        {
#if MRET_DEBUG
            Debug.Log("[" + ClassName + "->" + nameof(OnNavigateComplete) + "] Detected");
#endif

            // If paused, do nothing
            if (Paused) return;

            // Disable locomotion for the active mode
            if ((bool) MRET.DataManager.FindPoint(navigateKey) == true)
            {
                hand.DisableNavigate();
            }

            // TODO other locomotion forms.
        }

        /// <summary>
        /// Called when the select action begins.
        /// </summary>
        /// <param name="hand"><code>InputHand</code> that initiated the action.</param>
        public void OnSelectBegin(InputHand hand)
        {
#if MRET_DEBUG
            Debug.Log("[" + ClassName + "->" + nameof(OnSelectBegin) + "] Detected");
#endif

            // If paused, do nothing
            if (Paused) return;

            // Enable locomotion for the active mode
            if ((bool) MRET.DataManager.FindPoint(teleportKey) == true)
            {
                hand.ToggleTeleportOn();
            }

            // TODO other locomotion forms.
        }

        /// <summary>
        /// Called when the select action completes.
        /// </summary>
        /// <param name="hand"><code>InputHand</code> that completed the action.</param>
        public void OnSelectComplete(InputHand hand)
        {
#if MRET_DEBUG
            Debug.Log("[" + ClassName + "->" + nameof(OnSelectComplete) + "] Detected");
#endif

            // If paused, do nothing
            if (Paused) return;

            // Disable locomotion for the active mode
            if ((bool) MRET.DataManager.FindPoint(teleportKey) == true)
            {
                hand.CompleteTeleport();
                hand.ToggleTeleportOff();
            }

            // TODO other locomotion forms.
        }

#endregion // Event Handlers

#region Rig Settings

        /// <summary>
        /// Applies gravity to the based upon the supplied gravity constraints
        /// </summary>
        /// <param name="constraint">The constraint to apply</param>
        /// <returns>A boolean value indicating whether or not gravity constraints were applied</returns>
        protected bool ApplyGravityConstraints(GravityConstraint constraint)
        {
            bool applied = false;

            // Evaluate
            if (constraint == GravityConstraint.Required)
            {
                EnableGravity();
                applied = true;
            }
            else if (constraint == GravityConstraint.Prohibited)
            {
                DisableGravity();
                applied = true;
            }
            else
            {
                // Do nothing
            }

            return applied;
        }

        /// <summary>
        /// Applies gravity based upon the enabled locomotion gravity constraints
        /// </summary>
        /// <returns>A boolean value indicating whether or not gravity constraints were applied</returns>
        protected bool ApplyGravityFromEnabledLocomotion()
        {
            bool applied = false;

            // Check armswing
            if (!applied && inputRig.ArmswingEnabled)
            {
                applied = ApplyGravityConstraints(ArmswingGravityConstraint);
            }

            // Check flying
            if (!applied && inputRig.FlyingEnabled)
            {
                applied = ApplyGravityConstraints(FlyingGravityConstraint);
            }

            // Check navigation
            if (!applied && inputRig.NavigationEnabled)
            {
                applied = ApplyGravityConstraints(NavigationGravityConstraint);
            }

            return applied;
        }

        /// <summary>
        /// Toggles gravity.
        /// </summary>
        public void ToggleGravity(bool on)
        {
            if (on)
            {
                EnableGravity();
            }
            else
            {
                DisableGravity();
            }
        }

        /// <summary>
        /// Enables gravity for the rig.
        /// </summary>
        public void EnableGravity()
        {
            // Check that not already enabled
            if ((bool)MRET.DataManager.FindPoint(gravityKey) == true)
            {
                Debug.LogWarning("[" + ClassName + "->" + nameof(EnableGravity) + "] Gravity already enabled");
                return;
            }

            // Enable gravity
            inputRig.EnableGravity();

            // Save to DataManager
            MRET.DataManager.SaveValue(gravityKey, true);

            Debug.Log("[" + ClassName + "->" + nameof(EnableGravity) + "] Gravity enabled");
        }

        /// <summary>
        /// Disables gravity for the rig.
        /// </summary>
        public void DisableGravity()
        {
            // Check that not already disabled
            if ((bool)MRET.DataManager.FindPoint(gravityKey) == false)
            {
                Debug.LogWarning("[" + ClassName + "->" + nameof(DisableGravity) + "] Gravity already disabled");
                return;
            }

            // Disable gravity
            inputRig.DisableGravity();

            // Save to DataManager
            MRET.DataManager.SaveValue(gravityKey, false);

            Debug.Log("[" + ClassName + "->" + nameof(DisableGravity) + "] Gravity disabled");
        }

#endregion // Rig Settings

#region Modes

#region Teleport

        /// <summary>
        /// The multiplier to be applied to the flying motion.
        /// </summary>
        public float TeleportMaxDistance
        {
            set
            {
                SetTeleportMaxDistance(value);
            }
            get
            {
                // TODO: We should reevaluate where this value is contained because having the value defined at the hand level
                // implies they can be different max lengths. There should probably be a single max length.
                // Perhaps relocating the max value to the Rig and shared for all hands.
                // DZB: I'm not sure we want to preclude the possibility of making them different lengths. We definitely want to
                // have top-level control, but I can envision scenarios where there are different types of hands being used
                // simultaneously.
                float maxDistance = 0f;
                foreach (InputHand inputHand in inputRig.hands)
                {
                    maxDistance = (inputHand.TeleportMaxDistance > maxDistance) ? inputHand.TeleportMaxDistance : maxDistance;
                }

                return maxDistance;
            }
        }

        /// <summary>
        /// Sets the maximum teleport distance (meters)
        /// </summary>
        protected virtual void SetTeleportMaxDistance(float value)
        {
            // Set the teleport laser distance speed
            // TODO: If relocated to the rig, this would need updating.
            // DZB: See above.
            float maxDistance = 0f;
            foreach (InputHand inputHand in inputRig.hands)
            {
                inputHand.TeleportMaxDistance = value;
                maxDistance = (inputHand.TeleportMaxDistance > maxDistance) ? inputHand.TeleportMaxDistance : maxDistance;
            }

            // Save to DataManager
            MRET.DataManager.SaveValue(teleportMaxDistanceKey, maxDistance);
        }

        /// <summary>
        /// Toggles teleportation locomotion.
        /// </summary>
        public void ToggleTeleport()
        {
            if ((bool)MRET.DataManager.FindPoint(teleportKey) == false)
            {
                EnableTeleport();
            }
            else
            {
                DisableTeleport();
            }
        }

        /// <summary>
        /// Toggles teleportation locomotion.
        /// </summary>
        public void ToggleTeleport(bool on)
        {
            if (on)
            {
                EnableTeleport();
            }
            else
            {
                DisableTeleport();
            }
        }

        /// <summary>
        /// Enables teleportation locomotion.
        /// </summary>
        public void EnableTeleport()
        {
            // Check that not already enabled.
            if ((bool) MRET.DataManager.FindPoint(teleportKey) == true)
            {
                Debug.LogWarning("[" + ClassName + "->" + nameof(EnableTeleport) + "] Teleport already enabled");
                return;
            }

            // Enable teleport
            foreach (InputHand inputHand in inputRig.hands)
            {
                inputHand.EnableTeleport();
            }

            // Save to DataManager
            MRET.DataManager.SaveValue(teleportKey, true);

            Debug.Log("[" + ClassName + "->" + nameof(EnableTeleport) + "] Teleport enabled");
        }

        /// <summary>
        /// Disables teleportation locomotion.
        /// </summary>
        public void DisableTeleport()
        {
            // Check that not already disabled
            if ((bool) MRET.DataManager.FindPoint(teleportKey) == false)
            {
                Debug.LogWarning("[" + ClassName + "->" + nameof(DisableTeleport) + "] Teleport already disabled");
                return;
            }

            // Disable teleport.
            foreach (InputHand inputHand in inputRig.hands)
            {
                inputHand.DisableTeleport();
            }

            // Save to DataManager, but only if we aren't paused
            if (!Paused)
            {
                MRET.DataManager.SaveValue(teleportKey, false);
            }

            Debug.Log("[" + ClassName + "->" + nameof(DisableTeleport) + "] Teleport disabled");
        }

#endregion // Teleport

#region Flying

        /// <summary>
        /// The multiplier to be applied to the normal flying motion.
        /// </summary>
        public float FlyingNormalMotionConstraintMultiplier
        {
            set
            {
                SetFlyingNormalMotionConstraintMultiplier(value);
            }
            get
            {
                return inputRig.FlyingNormalMotionConstraintMultiplier;
            }
        }

        /// <summary>
        /// Sets the normal flying motion constraint multiplier
        /// </summary>
        /// <param name="value">The new motion constraint multipler</param>
        protected virtual void SetFlyingNormalMotionConstraintMultiplier(float value)
        {
            // Set the flying motion multiplier
            inputRig.FlyingNormalMotionConstraintMultiplier = value;

            // Save to DataManager
            MRET.DataManager.SaveValue(flyMotionConstraintMultiplerNormalKey, inputRig.FlyingNormalMotionConstraintMultiplier);

            Debug.Log("[" + ClassName + "->" + nameof(SetFlyingNormalMotionConstraintMultiplier) +
                "] Flying normal motion constraint multiplier set to: " + inputRig.FlyingNormalMotionConstraintMultiplier);
        }

        /// <summary>
        /// The multiplier to be applied to the slow flying motion.
        /// </summary>
        public float FlyingSlowMotionConstraintMultiplier
        {
            set
            {
                SetFlyingSlowMotionConstraintMultiplier(value);
            }
            get
            {
                return inputRig.FlyingSlowMotionConstraintMultiplier;
            }
        }

        /// <summary>
        /// Sets the slow flying motion constraint multiplier
        /// </summary>
        /// <param name="value">The new motion constraint multipler</param>
        protected virtual void SetFlyingSlowMotionConstraintMultiplier(float value)
        {
            // Set the flying motion multiplier
            inputRig.FlyingSlowMotionConstraintMultiplier = value;

            // Save to DataManager
            MRET.DataManager.SaveValue(flyMotionConstraintMultiplerSlowKey, inputRig.FlyingSlowMotionConstraintMultiplier);

            Debug.Log("[" + ClassName + "->" + nameof(SetFlyingSlowMotionConstraintMultiplier) +
                "] Flying slow motion constraint multiplier set to: " + inputRig.FlyingSlowMotionConstraintMultiplier);
        }

        /// <summary>
        /// The multiplier to be applied to the fast flying motion.
        /// </summary>
        public float FlyingFastMotionConstraintMultiplier
        {
            set
            {
                SetFlyingFastMotionConstraintMultiplier(value);
            }
            get
            {
                return inputRig.FlyingFastMotionConstraintMultiplier;
            }
        }

        /// <summary>
        /// Sets the fast flying motion constraint multiplier
        /// </summary>
        /// <param name="value">The new motion constraint multipler</param>
        protected virtual void SetFlyingFastMotionConstraintMultiplier(float value)
        {
            // Set the flying motion multiplier
            inputRig.FlyingFastMotionConstraintMultiplier = value;

            // Save to DataManager
            MRET.DataManager.SaveValue(flyMotionConstraintMultiplerFastKey, inputRig.FlyingFastMotionConstraintMultiplier);

            Debug.Log("[" + ClassName + "->" + nameof(SetFlyingFastMotionConstraintMultiplier) +
                "] Flying fast motion constraint multiplier set to: " + inputRig.FlyingFastMotionConstraintMultiplier);
        }

        /// <summary>
        /// The gravity constraint to be applied to the flying motion.
        /// </summary>
        public GravityConstraint FlyingGravityConstraint
        {
            set
            {
                SetFlyingGravityConstraint(value);
            }
            get
            {
                return inputRig.FlyingGravityConstraint;
            }
        }

        /// <summary>
        /// Sets the gravity constraint to be applied to the flying motion.
        /// </summary>
        /// <param name="value">The gravity constraint for flying</param>
        /// <seealso cref="GravityConstraint"/>
        protected virtual void SetFlyingGravityConstraint(GravityConstraint value)
        {
            // Set the flying gravity constraint
            inputRig.FlyingGravityConstraint = value;

            // Save to DataManager
            MRET.DataManager.SaveValue(flyGravityConstraintKey, inputRig.FlyingGravityConstraint);

            Debug.Log("[" + ClassName + "->" + nameof(SetFlyingGravityConstraint) + "] Flying gravity constraint set to: " + inputRig.FlyingGravityConstraint);
        }

        /// <summary>
        /// Toggles flying locomotion.
        /// </summary>
        public void ToggleFlying()
        {
            if ((bool) MRET.DataManager.FindPoint(flyKey) == false)
            {
                EnableFlying();
            }
            else
            {
                DisableFlying();
            }
        }

        /// <summary>
        /// Toggles flying locomotion.
        /// </summary>
        public void ToggleFlying(bool on)
        {
            if (on)
            {
                EnableFlying();
            }
            else
            {
                DisableFlying();
            }
        }

        /// <summary>
        /// Enables flying locomotion.
        /// </summary>
        public void EnableFlying()
        {
            // Check that not already enabled
            if ((bool) MRET.DataManager.FindPoint(flyKey) == true)
            {
                Debug.LogWarning("[" + ClassName + "->" + nameof(EnableFlying) + "] Fly already enabled");
                return;
            }

            // Enable flying
            inputRig.EnableFlying();

            // Gravity
            ApplyGravityFromEnabledLocomotion();

            // Save to DataManager
            MRET.DataManager.SaveValue(flyKey, true);

            Debug.Log("[" + ClassName + "->" + nameof(EnableFlying) + "] Fly enabled");
        }

        /// <summary>
        /// Disables flying locomotion.
        /// </summary>
        public void DisableFlying()
        {
            // Check that not already disabled
            if ((bool) MRET.DataManager.FindPoint(flyKey) == false)
            {
                Debug.LogWarning("[" + ClassName + "->" + nameof(DisableFlying) + "] Fly already disabled");
                return;
            }

            // Disable flying
            inputRig.DisableFlying();

            // Gravity
            ApplyGravityFromEnabledLocomotion();

            // Save to DataManager, but only if we aren't paused
            if (!Paused)
            {
                MRET.DataManager.SaveValue(flyKey, false);
            }

            Debug.Log("[" + ClassName + "->" + nameof(DisableFlying) + "] Fly disabled");
        }

#endregion // Flying

#region Navigation

        /// <summary>
        /// The multiplier to be applied to the normal navigation motion.
        /// </summary>
        public float NavigationNormalMotionConstraintMultiplier
        {
            set
            {
                SetNavigationNormalMotionConstraintMultiplier(value);
            }
            get
            {
                return inputRig.NavigationNormalMotionConstraintMultiplier;
            }
        }

        /// <summary>
        /// Sets the normal navigation motion constraint multiplier
        /// </summary>
        /// <param name="value">The new motion constraint multipler</param>
        protected virtual void SetNavigationNormalMotionConstraintMultiplier(float value)
        {
            // Set the navigation motion multiplier
            inputRig.NavigationNormalMotionConstraintMultiplier = value;

            // Save to DataManager
            MRET.DataManager.SaveValue(navigateMotionConstraintMultiplerNormalKey, inputRig.NavigationNormalMotionConstraintMultiplier);

            Debug.Log("[" + ClassName + "->" + nameof(SetNavigationNormalMotionConstraintMultiplier) +
                "] Navigation normal motion constraint multiplier set to: " + inputRig.NavigationNormalMotionConstraintMultiplier);
        }

        /// <summary>
        /// The multiplier to be applied to the slow navigation motion.
        /// </summary>
        public float NavigationSlowMotionConstraintMultiplier
        {
            set
            {
                SetNavigationSlowMotionConstraintMultiplier(value);
            }
            get
            {
                return inputRig.NavigationSlowMotionConstraintMultiplier;
            }
        }

        /// <summary>
        /// Sets the slow navigation motion constraint multiplier
        /// </summary>
        /// <param name="value">The new motion constraint multipler</param>
        protected virtual void SetNavigationSlowMotionConstraintMultiplier(float value)
        {
            // Set the navigation motion multiplier
            inputRig.NavigationSlowMotionConstraintMultiplier = value;

            // Save to DataManager
            MRET.DataManager.SaveValue(navigateMotionConstraintMultiplerSlowKey, inputRig.NavigationSlowMotionConstraintMultiplier);

            Debug.Log("[" + ClassName + "->" + nameof(SetNavigationSlowMotionConstraintMultiplier) +
                "] Navigation slow motion constraint multiplier set to: " + inputRig.NavigationSlowMotionConstraintMultiplier);
        }

        /// <summary>
        /// The multiplier to be applied to the fast navigation motion.
        /// </summary>
        public float NavigationFastMotionConstraintMultiplier
        {
            set
            {
                SetNavigationFastMotionConstraintMultiplier(value);
            }
            get
            {
                return inputRig.NavigationFastMotionConstraintMultiplier;
            }
        }

        /// <summary>
        /// Sets the fast navigation motion constraint multiplier
        /// </summary>
        /// <param name="value">The new motion constraint multipler</param>
        protected virtual void SetNavigationFastMotionConstraintMultiplier(float value)
        {
            // Set the navigation motion multiplier
            inputRig.NavigationFastMotionConstraintMultiplier = value;

            // Save to DataManager
            MRET.DataManager.SaveValue(navigateMotionConstraintMultiplerFastKey, inputRig.NavigationFastMotionConstraintMultiplier);

            Debug.Log("[" + ClassName + "->" + nameof(SetNavigationFastMotionConstraintMultiplier) +
                "] Navigation fast motion constraint multiplier set to: " + inputRig.NavigationFastMotionConstraintMultiplier);
        }

        /// <summary>
        /// The gravity constraint to be applied to the navigation motion.
        /// </summary>
        public GravityConstraint NavigationGravityConstraint
        {
            set
            {
                SetNavigationGravityConstraint(value);
            }
            get
            {
                return inputRig.NavigationGravityConstraint;
            }
        }

        /// <summary>
        /// Sets the gravity constraint to be applied to the navigation motion.
        /// </summary>
        /// <param name="value">The gravity constraint for navigation</param>
        /// <seealso cref="GravityConstraint"/>
        protected virtual void SetNavigationGravityConstraint(GravityConstraint value)
        {
            // Set the flying gravity constraint
            inputRig.NavigationGravityConstraint = value;

            // Save to DataManager
            MRET.DataManager.SaveValue(navigateGravityConstraintKey, inputRig.NavigationGravityConstraint);

            Debug.Log("[" + ClassName + "->" + nameof(SetNavigationGravityConstraint) + "] Navigation gravity constraint set to: " + inputRig.NavigationGravityConstraint);
        }

        /// <summary>
        /// Toggles navigation locomotion.
        /// </summary>
        public void ToggleNavigation()
        {
            if ((bool) MRET.DataManager.FindPoint(navigateKey) == false)
            {
                EnableNavigate();
            }
            else
            {
                DisableNavigate();
            }
        }

        /// <summary>
        /// Toggles navigation locomotion.
        /// </summary>
        public void ToggleNavigation(bool on)
        {
            if (on)
            {
                EnableNavigate();
            }
            else
            {
                DisableNavigate();
            }
        }

        /// <summary>
        /// Enables navigation locomotion.
        /// </summary>
        public void EnableNavigate()
        {
            // Check that not already enabled
            if ((bool) MRET.DataManager.FindPoint(navigateKey) == true)
            {
                Debug.LogWarning("[" + ClassName + "->" + nameof(EnableNavigate) + "] Navigate already enabled");
                return;
            }

            // Enable navigation
            inputRig.EnableNavigation();

            // Gravity
            ApplyGravityFromEnabledLocomotion();

            // Save to DataManager
            MRET.DataManager.SaveValue(navigateKey, true);

            Debug.Log("[" + ClassName + "->" + nameof(EnableNavigate) + "] Navigate enabled");
        }

        /// <summary>
        /// Disables navigation locomotion.
        /// </summary>
        public void DisableNavigate()
        {
            // Check that not already disabled
            if ((bool) MRET.DataManager.FindPoint(navigateKey) == false)
            {
                Debug.LogWarning("[" + ClassName + "->" + nameof(DisableNavigate) + "] Navigate already disabled");
                return;
            }

            // Disable navigation
            inputRig.DisableNavigation();

            // Gravity
            ApplyGravityFromEnabledLocomotion();

            // Save to DataManager, but only if we aren't paused
            if (!Paused)
            {
                MRET.DataManager.SaveValue(navigateKey, false);
            }

            Debug.Log("[" + ClassName + "->" + nameof(DisableNavigate) + "] Navigate disabled");
        }

#endregion // Navigation

#region Armswing

        /// <summary>
        /// The multiplier to be applied to the normal arm swing motion.
        /// </summary>
        public float ArmswingNormalMotionConstraintMultiplier
        {
            set
            {
                SetArmswingNormalMotionConstraintMultiplier(value);
            }
            get
            {
                return inputRig.ArmswingNormalMotionConstraintMultiplier;
            }
        }

        /// <summary>
        /// Sets the normal arm swing motion constraint multiplier
        /// </summary>
        /// <param name="value">The new motion constraint multipler</param>
        protected virtual void SetArmswingNormalMotionConstraintMultiplier(float value)
        {
            // Set the arm swing motion multiplier
            inputRig.ArmswingNormalMotionConstraintMultiplier = value;

            // Save to DataManager
            MRET.DataManager.SaveValue(armswingMotionConstraintMultiplerNormalKey, inputRig.ArmswingNormalMotionConstraintMultiplier);

            Debug.Log("[" + ClassName + "->" + nameof(SetArmswingNormalMotionConstraintMultiplier) +
                "] Armswing normal motion constraint multiplier set to: " + inputRig.ArmswingNormalMotionConstraintMultiplier);
        }

        /// <summary>
        /// The multiplier to be applied to the slow arm swing motion.
        /// </summary>
        public float ArmswingSlowMotionConstraintMultiplier
        {
            set
            {
                SetArmswingSlowMotionConstraintMultiplier(value);
            }
            get
            {
                return inputRig.ArmswingSlowMotionConstraintMultiplier;
            }
        }

        /// <summary>
        /// Sets the slow arm swing motion constraint multiplier
        /// </summary>
        /// <param name="value">The new motion constraint multipler</param>
        protected virtual void SetArmswingSlowMotionConstraintMultiplier(float value)
        {
            // Set the arm swing motion multiplier
            inputRig.ArmswingSlowMotionConstraintMultiplier = value;

            // Save to DataManager
            MRET.DataManager.SaveValue(armswingMotionConstraintMultiplerSlowKey, inputRig.ArmswingSlowMotionConstraintMultiplier);

            Debug.Log("[" + ClassName + "->" + nameof(SetArmswingSlowMotionConstraintMultiplier) +
                "] Armswing slow motion constraint multiplier set to: " + inputRig.ArmswingSlowMotionConstraintMultiplier);
        }

        /// <summary>
        /// The multiplier to be applied to the fast arm swing motion.
        /// </summary>
        public float ArmswingFastMotionConstraintMultiplier
        {
            set
            {
                SetArmswingFastMotionConstraintMultiplier(value);
            }
            get
            {
                return inputRig.ArmswingFastMotionConstraintMultiplier;
            }
        }

        /// <summary>
        /// Sets the fast arm swing motion constraint multiplier
        /// </summary>
        /// <param name="value">The new motion constraint multipler</param>
        protected virtual void SetArmswingFastMotionConstraintMultiplier(float value)
        {
            // Set the arm swing motion multiplier
            inputRig.ArmswingFastMotionConstraintMultiplier = value;

            // Save to DataManager
            MRET.DataManager.SaveValue(armswingMotionConstraintMultiplerFastKey, inputRig.ArmswingFastMotionConstraintMultiplier);

            Debug.Log("[" + ClassName + "->" + nameof(SetArmswingFastMotionConstraintMultiplier) +
                "] Armswing fast motion constraint multiplier set to: " + inputRig.ArmswingFastMotionConstraintMultiplier);
        }

        /// <summary>
        /// The gravity constraint for armswing locomotion.
        /// </summary>
        public GravityConstraint ArmswingGravityConstraint
        {
            set
            {
                SetArmswingGravityConstraint(value);
            }
            get
            {
                return inputRig.ArmswingGravityConstraint;
            }
        }

        /// <summary>
        /// Sets the gravity constraint for armswing locomotion.
        /// </summary>
        /// <param name="value">The gravity constraint for armswing</param>
        /// <seealso cref="GravityConstraint"/>
        protected virtual void SetArmswingGravityConstraint(GravityConstraint value)
        {
            // Set the armswing gravity constraint
            inputRig.ArmswingGravityConstraint = value;

            // Save to DataManager
            MRET.DataManager.SaveValue(armswingGravityConstraintKey, inputRig.ArmswingGravityConstraint);

            Debug.Log("[" + ClassName + "->" + nameof(SetArmswingGravityConstraint) + "] Armswing gravity constraint set to: " + inputRig.ArmswingGravityConstraint);
        }

        /// <summary>
        /// Toggles armswing locomotion.
        /// </summary>
        public void ToggleArmswing()
        {
            if ((bool) MRET.DataManager.FindPoint(armswingKey) == false)
            {
                EnableArmswing();
            }
            else
            {
                DisableArmswing();
            }
        }

        /// <summary>
        /// Toggles armswing locomotion.
        /// </summary>
        public void ToggleArmswing(bool on)
        {
            if (on)
            {
                EnableArmswing();
            }
            else
            {
                DisableArmswing();
            }
        }

        /// <summary>
        /// Enables armswing locomotion.
        /// </summary>
        public void EnableArmswing()
        {
            // Check that not already enabled
            if ((bool) MRET.DataManager.FindPoint(armswingKey) == true)
            {
                Debug.LogWarning("[" + ClassName + "->" + nameof(EnableArmswing) + "] Armswing already enabled");
                return;
            }

            // Enable armswing
            inputRig.EnableArmswing();

            // Gravity
            ApplyGravityFromEnabledLocomotion();

            // Save to DataManager
            MRET.DataManager.SaveValue(armswingKey, true);

            Debug.Log("[" + ClassName + "->" + nameof(EnableArmswing) + "] Armswing enabled");
        }

        /// <summary>
        /// Disables armswing locomotion.
        /// </summary>
        public void DisableArmswing()
        {
            // Check that not already disabled
            if ((bool) MRET.DataManager.FindPoint(armswingKey) == false)
            {
                Debug.LogWarning("[" + ClassName + "->" + nameof(DisableArmswing) + "] Armswing already disabled");
                return;
            }

            // Disable armswing
            inputRig.DisableArmswing();

            // Gravity
            ApplyGravityFromEnabledLocomotion();

            // Save to DataManager, but only if we aren't paused
            if (!Paused)
            {
                MRET.DataManager.SaveValue(armswingKey, false);
            }

            Debug.Log("[" + ClassName + "->" + nameof(DisableArmswing) + "] Armswing disabled");
        }

#endregion // Armswing

#endregion // Modes

 #region Rotation/Scaling

        /// <summary>
        /// Toggles X axis rotation.
        /// </summary>
        public void ToggleXRotation()
        {
            if ((bool) MRET.DataManager.FindPoint(rotateXKey) == false)
            {
                EnableXRotation();
            }
            else
            {
                DisableXRotation();
            }
        }

        /// <summary>
        /// Toggles Y axis rotation.
        /// </summary>
        public void ToggleYRotation()
        {
            if ((bool) MRET.DataManager.FindPoint(rotateYKey) == false)
            {
                EnableYRotation();
            }
            else
            {
                DisableYRotation();
            }
        }

        /// <summary>
        /// Toggles Z axis rotation.
        /// </summary>
        public void ToggleZRotation()
        {
            if ((bool) MRET.DataManager.FindPoint(rotateZKey) == false)
            {
                EnableZRotation();
            }
            else
            {
                DisableZRotation();
            }
        }

        /// <summary>
        /// Toggles scaling.
        /// </summary>
        public void ToggleScaling()
        {
            if ((bool) MRET.DataManager.FindPoint(scaleKey) == false)
            {
                EnableScaling();
            }
            else
            {
                DisableScaling();
            }
        }

        /// <summary>
        /// Toggles X axis rotation.
        /// </summary>
        /// <param name="on">Whether or not to turn it on.</param>
        public void ToggleXRotation(bool on)
        {
            if (on)
            {
                EnableXRotation();
            }
            else
            {
                DisableXRotation();
            }
        }

        /// <summary>
        /// Toggles Y axis rotation.
        /// </summary>
        /// <param name="on">Whether or not to turn it on.</param>
        public void ToggleYRotation(bool on)
        {
            if (on)
            {
                EnableYRotation();
            }
            else
            {
                DisableYRotation();
            }
        }

        /// <summary>
        /// Toggles Z axis rotation.
        /// </summary>
        /// <param name="on">Whether or not to turn it on.</param>
        public void ToggleZRotation(bool on)
        {
            if (on)
            {
                EnableZRotation();
            }
            else
            {
                DisableZRotation();
            }
        }

        /// <summary>
        /// Toggles scaling.
        /// </summary>
        /// <param name="on"></param>
        public void ToggleScaling(bool on)
        {
            if (on)
            {
                EnableScaling();
            }
            else
            {
                DisableScaling();
            }
        }

        /// <summary>
        /// Enables X axis rotation.
        /// </summary>
        public void EnableXRotation()
        {
            // Check that not already enabled
            if ((bool) MRET.DataManager.FindPoint(rotateXKey) == true)
            {
                Debug.LogWarning("[" + ClassName + "->" + nameof(EnableXRotation) + "] X rotation already enabled");
                return;
            }

            // Enable X rotation
            foreach (RotateObjectTransform rot in rots)
            {
                // Enable rotation
                rot.enabled = true;

                // Enable the X axis
                rot.xEnabled = true;
            }

            // Save to DataManager
            MRET.DataManager.SaveValue(rotateXKey, true);

            Debug.Log("[" + ClassName + "->" + nameof(EnableXRotation) + "] X rotation enabled");
        }

        /// <summary>
        /// Disables X axis rotation.
        /// </summary>
        public void DisableXRotation()
        {
            // Check that not already disabled
            if ((bool) MRET.DataManager.FindPoint(rotateXKey) == false)
            {
                Debug.LogWarning("[" + ClassName + "->" + nameof(DisableXRotation) + "] X rotation already disabled");
                return;
            }

            // Disable X rotation
            foreach (RotateObjectTransform rot in rots)
            {
                // Disable the X axis
                rot.xEnabled = false;

                // Disable rotation if all axes are turned off
                rot.enabled = rot.xEnabled || rot.yEnabled || rot.zEnabled;
            }

            // Save to DataManager
            MRET.DataManager.SaveValue(rotateXKey, false);

            Debug.Log("[" + ClassName + "->" + nameof(DisableXRotation) + "] X rotation disabled");
        }

        /// <summary>
        /// Enables Y axis rotation.
        /// </summary>
        public void EnableYRotation()
        {
            // Check that not already enabled.
            if ((bool) MRET.DataManager.FindPoint(rotateYKey) == true)
            {
                Debug.LogWarning("[" + ClassName + "->" + nameof(EnableYRotation) + "] Y rotation already enabled");
                return;
            }

            // Enable Y rotation
            foreach (RotateObjectTransform rot in rots)
            {
                // Enable rotation
                rot.enabled = true;

                // Enable the Y axis
                rot.yEnabled = true;
            }

            // Save to DataManager
            MRET.DataManager.SaveValue(rotateYKey, true);

            Debug.Log("[" + ClassName + "->" + nameof(EnableYRotation) + "] Y rotation enabled");
        }

        /// <summary>
        /// Disables Y axis rotation.
        /// </summary>
        public void DisableYRotation()
        {
            // Check that not already disabled
            if ((bool) MRET.DataManager.FindPoint(rotateYKey) == false)
            {
                Debug.LogWarning("[" + ClassName + "->" + nameof(DisableYRotation) + "] Y rotation already disabled");
                return;
            }

            // Disable Y rotation
            foreach (RotateObjectTransform rot in rots)
            {
                // Disable the Y axis
                rot.yEnabled = false;

                // Disable rotation if all axes are turned off
                rot.enabled = rot.xEnabled || rot.yEnabled || rot.zEnabled;
            }

            // Save to DataManager
            MRET.DataManager.SaveValue(rotateYKey, false);

            Debug.Log("[" + ClassName + "->" + nameof(DisableYRotation) + "] Y rotation disabled");
        }

        /// <summary>
        /// Enables Z axis rotation.
        /// </summary>
        public void EnableZRotation()
        {
            // Check that not already enabled
            if ((bool) MRET.DataManager.FindPoint(rotateZKey) == true)
            {
                Debug.LogWarning("[" + ClassName + "->" + nameof(EnableZRotation) + "] Z rotation already enabled");
                return;
            }

            // Enable Z rotation
            foreach (RotateObjectTransform rot in rots)
            {
                // Enable rotation
                rot.enabled = true;

                // Enable the Z axis
                rot.zEnabled = true;
            }

            // Save to DataManager
            MRET.DataManager.SaveValue(rotateZKey, true);

            Debug.Log("[" + ClassName + "->" + nameof(EnableZRotation) + "] Z rotation enabled");
        }

        /// <summary>
        /// Disables Z axis rotation.
        /// </summary>
        public void DisableZRotation()
        {
            // Check that not already disabled
            if ((bool) MRET.DataManager.FindPoint(rotateZKey) == false)
            {
                Debug.LogWarning("[" + ClassName + "->" + nameof(DisableZRotation) + "] Z rotation already disabled");
                return;
            }

            // Disable Z rotation
            foreach (RotateObjectTransform rot in rots)
            {
                // Disable the Z axis
                rot.zEnabled = false;

                // Disable rotation if all axes are turned off
                rot.enabled = rot.xEnabled || rot.yEnabled || rot.zEnabled;
            }

            // Save to DataManager
            MRET.DataManager.SaveValue(rotateZKey, false);

            Debug.Log("[" + ClassName + "->" + nameof(DisableZRotation) + "] Z rotation disabled");
        }

        /// <summary>
        /// Enables scaling.
        /// </summary>
        public void EnableScaling()
        {
            // Check that not already enabled
            if ((bool) MRET.DataManager.FindPoint(scaleKey) == true)
            {
                Debug.LogWarning("[" + ClassName + "->" + nameof(EnableScaling) + "] Scaling already enabled");
                return;
            }

            // Enable scaling
            foreach (ScaleObjectTransform sot in sots)
            {
                sot.enabled = true;
            }

            // Save to DataManager
            MRET.DataManager.SaveValue(scaleKey, true);

            Debug.Log("[" + ClassName + "->" + nameof(EnableScaling) + "] Scaling enabled");
        }

        /// <summary>
        /// Disables scaling.
        /// </summary>
        public void DisableScaling()
        {
            // Check that not already disabled
            if ((bool) MRET.DataManager.FindPoint(scaleKey) == false)
            {
                Debug.LogWarning("[" + ClassName + "->" + nameof(DisableScaling) + "] Scaling already disabled");
                return;
            }

            // Disable scaling
            foreach (ScaleObjectTransform sot in sots)
            {
                sot.enabled = false;
            }

            // Save to DataManager
            MRET.DataManager.SaveValue(scaleKey, false);

            Debug.Log("[" + ClassName + "->" + nameof(DisableScaling) + "] Scaling disabled");
        }

#endregion // Rotation/Scaling

    }
}