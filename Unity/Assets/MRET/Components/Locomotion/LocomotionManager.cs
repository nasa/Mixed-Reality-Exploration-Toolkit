// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;
using GOV.NASA.GSFC.XR.MRET.Data;
using GOV.NASA.GSFC.XR.MRET.SceneObjects;
using GOV.NASA.GSFC.XR.MRET.Integrations.CPIS.Transforms;

namespace GOV.NASA.GSFC.XR.MRET.Locomotion
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
    /// 30 April 2021: Replaced the Pause mechanism with a reference counting system, akin to a mutex lock/unlock.
    ///     Pausing is now performed with calls to PauseRequest and PauseRelease. If the reference counter
    ///     is greater than 0 (calls to PauseRequest), the locomotion system is paused. Once the reference
    ///     counter reaches 0 (calls to PauseRelease), the locomotion system is unpaused. (J. Hosler)
    /// 14 May 2021: Removed InputRig public property and replaced references with MRET.InputRig (D. Baker)
    /// 22 July 2021: Repaired accidental merge with an old version that ended up breaking the pause
	///     functionality of Locomotion: commit hash 4d9a097b. Restored the version at commit hash ce9e2fc8/
    ///     [30 April 2021] and reapplied the changes performed in commit hash 942cf513/[14 May 2021].  (J. Hosler)
    /// 24 July 2021: Added Climbing locomotion (C. Lian)
    /// 17 November 2021: Added navigate based flying (DZB)
    /// 27 June 2022: Added explode object controller (SL)
    /// 22 Sept 2022: Moved the Rotate, Scale and Explode transform components out of the hands and placed them
    ///     under the LocomotionManager so that there is only one instance of the scripts at runtime (J. Hosler)
    /// </remarks>
    /// 
    /// <summary>
    /// LocomotionManager is a class that provides
    /// top-level control of locomotion in MRET.
    /// Author: Dylan Z. Baker
    /// </summary>
    public class LocomotionManager : MRETManager<LocomotionManager>
    {
        public enum Mode
        {
            Teleport,
            Armswing,
            Fly,
            Navigate,
            Climb
        }

        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(LocomotionManager);

        // TODO: Move constants to central class.

        // Prefix of all keys
        public const string KEY_PREFIX = "MRET.INTERNAL.LOCOMOTION";
        //TODO: ? public static readonly string KEY_PREFIX = typeof(LocomotionManager).Namespace.ToUpper();

        // DataManager keys
        #region Data Keys
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
        public const string climbKey = KEY_PREFIX + ".MODE.CLIMB";
        public const string climbMotionConstraintMultiplerNormalKey = KEY_PREFIX + ".MODE.CLIMB.MOTIONCONSTRAINTMULTIPLIER.NORMAL";
        public const string climbMotionConstraintMultiplerSlowKey = KEY_PREFIX + ".MODE.CLIMB.MOTIONCONSTRAINTMULTIPLIER.SLOW";
        public const string climbMotionConstraintMultiplerFastKey = KEY_PREFIX + ".MODE.CLIMB.MOTIONCONSTRAINTMULTIPLIER.FAST";
        public const string climbMotionConstraintKey = KEY_PREFIX + ".MODE.CLIMB.MOTIONCONSTRAINT";
        public const string climbGravityConstraintKey = KEY_PREFIX + ".MODE.CLIMB.GRAVITYCONSTRAINT";
        public const string gravityKey = KEY_PREFIX + ".GRAVITY";
        public const string rotateKey = KEY_PREFIX + ".ROTATE";
        public const string rotateXKey = KEY_PREFIX + ".ROTATE.X";
        public const string rotateYKey = KEY_PREFIX + ".ROTATE.Y";
        public const string rotateZKey = KEY_PREFIX + ".ROTATE.Z";
        public const string scaleKey = KEY_PREFIX + ".SCALE";
        public const string explodeKey = KEY_PREFIX + ".EXPLODE";
        public const string explodeObjectKey = KEY_PREFIX + ".EXPLODE.OBJECT";
        public const string pausedKey = KEY_PREFIX + ".PAUSED";
        public const string pauseReferenceCountKey = KEY_PREFIX + ".PAUSED.REFERENCECOUNT";
        #endregion Data Keys

        /// <summary>
        /// The rotate object transforms.
        /// </summary>
        protected RotateObjectTransform[] rots;

        /// <summary>
        /// The scale object transforms.
        /// </summary>
        protected ScaleObjectTransform[] sots;

        /// <summary>
        /// The exploded objects' transforms. 
        /// </summary>
        protected ExplodeObjectTransform[] eots;

        /// <value>Locomotion pause state</value>
        private int _pauseReferenceCount = 0;
        public bool Paused
        {
            get
            {
                return (_pauseReferenceCount > 0);
            }
        }

        /// <summary>
        /// Called to request that the locomotion be paused. This works like a reference counter, so
        /// for every time this method is called (increasing the reference count), the
        /// <code>PauseRelease</code> must be called (decrementing the reference counter) to undo the
        /// pause request. Once the pause reference counter reaches 0, the locomotion state will no longer
        /// be paused.
        /// </summary>
        /// 
        /// <see cref="PauseRelease"/>
        public void PauseRequest()
        {
            // Increment the reference counter
            _pauseReferenceCount++;

            // Save the new state to the DataManager
            MRET.DataManager.SaveValue(pauseReferenceCountKey, _pauseReferenceCount);
            MRET.DataManager.SaveValue(pausedKey, Paused);

            // Update the internal locomotion state
            UpdateLocomotionState();
        }

        /// <summary>
        /// Called to release a previously called pause request. The mechanism works like a reference
        /// counter, so for every time <code>PauseRequest</code> is called (increasing the reference count),
        /// this method must be called (decrementing the reference counter) to undo the pause request.
        /// Once the pause reference counter reaches 0, the locomotion state will no longer be paused.
        /// </summary>
        /// 
        /// <see cref="PauseRequest"/>
        public void PauseRelease()
        {
            // If paused, decrement the reference counter (prevents reference counter from going negative)
            if (Paused)
            {
                // Decrement the reference counter
                _pauseReferenceCount--;

                // Save the new state to the DataManager
                MRET.DataManager.SaveValue(pauseReferenceCountKey, _pauseReferenceCount);
                MRET.DataManager.SaveValue(pausedKey, Paused);

                // Update the internal locomotion state
                UpdateLocomotionState();
            }
        }

        /// <seealso cref="MRETBehaviour.IntegrityCheck"/>
        protected override IntegrityState IntegrityCheck()
        {
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            return (
                (state == IntegrityState.Failure) // TODO: || (MyRequiredRef == null)
                    ? IntegrityState.Failure      // Fail is base class fails or anything is null
                    : IntegrityState.Success);    // Otherwise, our integrity is valid
        }

        /// <seealso cref="MRETManager{M}.Initialize"/>
        public override void Initialize()
        {
            // Take the inherited behavior
            base.Initialize();

            // Initialize DataManager
            MRET.DataManager.SaveValue(new DataManager.DataValue(pausedKey, false));
            MRET.DataManager.SaveValue(new DataManager.DataValue(pauseReferenceCountKey, _pauseReferenceCount));
            MRET.DataManager.SaveValue(new DataManager.DataValue(teleportKey, false));
            MRET.DataManager.SaveValue(new DataManager.DataValue(flyKey, false));
            MRET.DataManager.SaveValue(new DataManager.DataValue(navigateKey, false));
            MRET.DataManager.SaveValue(new DataManager.DataValue(armswingKey, false));
            MRET.DataManager.SaveValue(new DataManager.DataValue(climbKey, false));

            // Create the transform references that we need to perform transforms
            List<RotateObjectTransform> rotsList = new List<RotateObjectTransform>();
            List<ScaleObjectTransform> sotsList = new List<ScaleObjectTransform>();
            List<ExplodeObjectTransform> eotsList = new List<ExplodeObjectTransform>();

            // Obtain the rotation for the hand if it exists
            {
                RotateObjectTransform rot = GetComponentInChildren<RotateObjectTransform>();

                // We found it, so only add if it's not in the list already
                if (rot && !rotsList.Contains(rot))
                {
                    rotsList.Add(rot);
                }
            }

            // Obtain the scale for the hand if it exists
            {
                ScaleObjectTransform sot = GetComponentInChildren<ScaleObjectTransform>();

                // We found it, so only add if it's not in the list already
                if (sot && !sotsList.Contains(sot))
                {
                    sotsList.Add(sot);
                }
            }

            // Obtain the position for the hand if it exists
            {
                ExplodeObjectTransform eot = GetComponentInChildren<ExplodeObjectTransform>();

                // We found it, so only add if it's not in the list already
                if (eot && !eotsList.Contains(eot))
                {
                    eotsList.Add(eot);
                }
            }

            // Store in our class arrays for quick access
            rots = rotsList.ToArray();
            sots = sotsList.ToArray();
            eots = eotsList.ToArray();

            // References are initialized now, so update data manager keys based upon initial MRET settings,
            // where appropriate
            MRET.DataManager.SaveValue(new DataManager.DataValue(gravityKey, MRET.InputRig.GravityEnabled));

            // Rotation
            bool rotEnabled = false;
            bool rotXEnabled = false;
            bool rotYEnabled = false;
            bool rotZEnabled = false;
            foreach (RotateObjectTransform rot in rots)
            {
                // Rotation as a whole
                rotEnabled |= rot.rotationEnabled;

                // Rotation axes are enabled if any rot axes are enabled
                rotXEnabled |= rot.xEnabled;
                rotYEnabled |= rot.yEnabled;
                rotZEnabled |= rot.zEnabled;
            }
            MRET.DataManager.SaveValue(new DataManager.DataValue(rotateKey, rotEnabled));
            MRET.DataManager.SaveValue(new DataManager.DataValue(rotateXKey, rotXEnabled));
            MRET.DataManager.SaveValue(new DataManager.DataValue(rotateYKey, rotYEnabled));
            MRET.DataManager.SaveValue(new DataManager.DataValue(rotateZKey, rotZEnabled));

            // Scale
            bool scaleEnabled = false;
            foreach (ScaleObjectTransform sot in sots)
            {
                // Scale is enabled if any sots are enabled
                scaleEnabled |= sot.scalingEnabled;
            }
            MRET.DataManager.SaveValue(new DataManager.DataValue(scaleKey, scaleEnabled));

            // Explode
            bool explodeEnabled = false;
            foreach (ExplodeObjectTransform eot in eots)
            {
                // Explode is enabled if any eots are enabled
                explodeEnabled |= eot.explodingEnabled;
            }
            MRET.DataManager.SaveValue(new DataManager.DataValue(explodeKey, explodeEnabled));
            MRET.DataManager.SaveValue(new DataManager.DataValue(explodeObjectKey, null));
        }

        /// <summary>
        /// Overridden to update settings based upon initial MRET settings now that everything should be initialized.
        /// </summary>
        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            // Take the inherited behavior
            base.MRETStart();
        }

        /// <summary>
        /// Disables all locomotion mechanisms.
        /// </summary>
        protected virtual void UpdateLocomotionState()
        {
            // Locomotion controllers
            UpdateArmswingState();
            UpdateFlyingState();
            UpdateNavigationState();
            UpdateTeleportState();
            UpdateClimbingState();

            // Rotation and scaling
            UpdateRotationState();
            UpdateScalingState();
            UpdateExplodingState();
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
            DisableClimbing();
        }

        #region Event Handlers

        /// <summary>
        /// Called when the navigate press action begins.
        /// </summary>
        /// <param name="hand"><code>InputHand</code> that initiated the action.</param>
        public void OnNavigatePressBegin(InputHand hand)
        {
#if MRET_DEBUG
            Log("Event Detected. Hand: " + hand.name, nameof(OnNavigatePressBegin));
#endif

            // If paused, do nothing
            if (Paused) return;

            // Set the motion constraint for the rig if one of the locomotion modes are active
            if (((bool)MRET.DataManager.FindPoint(armswingKey) == true) ||
                ((bool)MRET.DataManager.FindPoint(flyKey) == true) ||
                ((bool)MRET.DataManager.FindPoint(navigateKey) == true) ||
                ((bool)MRET.DataManager.FindPoint(climbKey) == true))
            {
                MRET.InputRig.MotionConstraint = MotionConstraint.Fast;
            }
        }

        /// <summary>
        /// Called when the navigate press action completes.
        /// </summary>
        /// <param name="hand"><code>InputHand</code> that completed the action.</param>
        public void OnNavigatePressComplete(InputHand hand)
        {
#if MRET_DEBUG
            Log("Event Detected. Hand: " + hand.name, nameof(OnNavigatePressComplete));
#endif

            // If paused, do nothing
            if (Paused) return;

            // Set the motion constraint for the rig if one of the locomotion modes are active
            if (((bool)MRET.DataManager.FindPoint(armswingKey) == true) ||
                ((bool)MRET.DataManager.FindPoint(flyKey) == true) ||
                ((bool)MRET.DataManager.FindPoint(navigateKey) == true) ||
                ((bool)MRET.DataManager.FindPoint(climbKey) == true))
            {
                MRET.InputRig.MotionConstraint = MotionConstraint.Normal;
            }
        }

        /// <summary>
        /// Called when the grab action begins.
        /// </summary>
        /// <param name="hand"><code>InputHand</code> that initiated the action.</param>
        public void OnGrabBegin(InputHand hand)
        {
#if MRET_DEBUG
            Log("Event Detected. Hand: " + hand.name, nameof(OnGrabBegin));
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
            else if ((bool)MRET.DataManager.FindPoint(climbKey) == true)
            {
                hand.EnableClimb();
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
            Log("Event Detected. Hand: " + hand.name, nameof(OnGrabComplete));
#endif

            // If paused, do nothing
            if (Paused) return;

            // Disable locomotion for the active mode
            if ((bool)MRET.DataManager.FindPoint(armswingKey) == true)
            {
                hand.DisableArmswing();
            }
            else if ((bool)MRET.DataManager.FindPoint(flyKey) == true)
            {
                hand.DisableFly();
            }
            else if ((bool)MRET.DataManager.FindPoint(climbKey) == true)
            {
                hand.DisableClimb();
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
            Log("Event Detected. Hand: " + hand.name, nameof(OnNavigateBegin));
#endif

            // If paused, do nothing
            if (Paused) return;

            // Enable locomotion for the active mode
            if ((bool)MRET.DataManager.FindPoint(navigateKey) == true)
            {
                hand.EnableNavigate();
            }
            else if ((bool)MRET.DataManager.FindPoint(flyKey) == true)
            {
                hand.EnableFly();
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
            Log("Event Detected. Hand: " + hand.name, nameof(OnNavigateComplete));
#endif

            // If paused, do nothing
            if (Paused) return;

            // Disable locomotion for the active mode
            if ((bool)MRET.DataManager.FindPoint(navigateKey) == true)
            {
                hand.DisableNavigate();
            }
            else if ((bool)MRET.DataManager.FindPoint(flyKey) == true)
            {
                hand.DisableFly();
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
            Log("Event Detected. Hand: " + hand.name, nameof(OnSelectBegin));
#endif

            // If paused, do nothing
            if (Paused) return;

            // Enable locomotion for the active mode
            if ((bool)MRET.DataManager.FindPoint(teleportKey) == true)
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
            Log("Event Detected. Hand: " + hand.name, nameof(OnSelectComplete));
#endif

            // If paused, do nothing
            if (Paused) return;

            // Disable locomotion for the active mode
            if ((bool)MRET.DataManager.FindPoint(teleportKey) == true)
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
            if (!applied && MRET.InputRig.ArmswingEnabled)
            {
                applied = ApplyGravityConstraints(ArmswingGravityConstraint);
            }

            // Check flying
            if (!applied && MRET.InputRig.FlyingEnabled)
            {
                applied = ApplyGravityConstraints(FlyingGravityConstraint);
            }

            // Check navigation
            if (!applied && MRET.InputRig.NavigationEnabled)
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
#if MRET_DEBUG
                LogWarning("Gravity already enabled", nameof(EnableGravity));
#endif
                return;
            }

            // Enable gravity
            MRET.InputRig.EnableGravity();

            // Save to DataManager
            MRET.DataManager.SaveValue(gravityKey, true);

            Log("Gravity enabled", nameof(EnableGravity));
        }

        /// <summary>
        /// Disables gravity for the rig.
        /// </summary>
        public void DisableGravity()
        {
            // Check that not already disabled
            if ((bool)MRET.DataManager.FindPoint(gravityKey) == false)
            {
#if MRET_DEBUG
                LogWarning("Gravity already disabled", nameof(DisableGravity));
#endif
                return;
            }

            // Disable gravity
            MRET.InputRig.DisableGravity();

            // Save to DataManager
            MRET.DataManager.SaveValue(gravityKey, false);

            Log("Gravity disabled", nameof(DisableGravity));
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
                foreach (InputHand inputHand in MRET.InputRig.hands)
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
            foreach (InputHand inputHand in MRET.InputRig.hands)
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
        /// Updates teleport locomotion to reflect the current state. Available for subclasses to override.
        /// </summary>
        protected virtual void UpdateTeleportState()
        {
            // Obtain the current locomotion state
            bool teleporting = (bool)MRET.DataManager.FindPoint(teleportKey);

            // Only enable if the state is on AND we are not paused. Disable otherwise.
            if (!Paused && teleporting)
            {
                // Enable teleport
                foreach (InputHand inputHand in MRET.InputRig.hands)
                {
                    inputHand.EnableTeleport();
                }
            }
            else
            {
                // Disable teleport
                foreach (InputHand inputHand in MRET.InputRig.hands)
                {
                    inputHand.DisableTeleport();
                }
            }
        }

        /// <summary>
        /// Enables teleportation locomotion.
        /// </summary>
        public void EnableTeleport()
        {
            // Obtain the current locomotion state
            bool teleporting = (bool)MRET.DataManager.FindPoint(teleportKey);

            // Check that not already enabled
            if (teleporting)
            {
#if MRET_DEBUG
                LogWarning("Teleport already enabled", nameof(EnableTeleport));
#endif
                return;
            }

            // Save the new state to the DataManager
            MRET.DataManager.SaveValue(teleportKey, true);

            // Update the teleport state
            UpdateTeleportState();

            Log("Teleport enabled", nameof(EnableTeleport));
        }

        /// <summary>
        /// Disables teleportation locomotion.
        /// </summary>
        public void DisableTeleport()
        {
            // Obtain the current locomotion state
            bool teleporting = (bool)MRET.DataManager.FindPoint(teleportKey);

            // Check that not already disabled
            if (!teleporting)
            {
#if MRET_DEBUG
                LogWarning("Teleport already disabled", nameof(DisableTeleport));
#endif
                return;
            }

            // Save the new state to the DataManager
            MRET.DataManager.SaveValue(teleportKey, false);

            // Update the teleport state
            UpdateTeleportState();

            Log("Teleport disabled", nameof(DisableTeleport));
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
                return MRET.InputRig.FlyingNormalMotionConstraintMultiplier;
            }
        }

        /// <summary>
        /// Sets the normal flying motion constraint multiplier
        /// </summary>
        /// <param name="value">The new motion constraint multipler</param>
        protected virtual void SetFlyingNormalMotionConstraintMultiplier(float value)
        {
            // Set the flying motion multiplier
            MRET.InputRig.FlyingNormalMotionConstraintMultiplier = value;

            // Save to DataManager
            MRET.DataManager.SaveValue(flyMotionConstraintMultiplerNormalKey, MRET.InputRig.FlyingNormalMotionConstraintMultiplier);

            Log("Flying normal motion constraint multiplier set to: " + MRET.InputRig.FlyingNormalMotionConstraintMultiplier,
                nameof(SetFlyingNormalMotionConstraintMultiplier));
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
                return MRET.InputRig.FlyingSlowMotionConstraintMultiplier;
            }
        }

        /// <summary>
        /// Sets the slow flying motion constraint multiplier
        /// </summary>
        /// <param name="value">The new motion constraint multipler</param>
        protected virtual void SetFlyingSlowMotionConstraintMultiplier(float value)
        {
            // Set the flying motion multiplier
            MRET.InputRig.FlyingSlowMotionConstraintMultiplier = value;

            // Save to DataManager
            MRET.DataManager.SaveValue(flyMotionConstraintMultiplerSlowKey, MRET.InputRig.FlyingSlowMotionConstraintMultiplier);

            Log("Flying slow motion constraint multiplier set to: " + MRET.InputRig.FlyingSlowMotionConstraintMultiplier,
                nameof(SetFlyingSlowMotionConstraintMultiplier));
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
                return MRET.InputRig.FlyingFastMotionConstraintMultiplier;
            }
        }

        /// <summary>
        /// Sets the fast flying motion constraint multiplier
        /// </summary>
        /// <param name="value">The new motion constraint multipler</param>
        protected virtual void SetFlyingFastMotionConstraintMultiplier(float value)
        {
            // Set the flying motion multiplier
            MRET.InputRig.FlyingFastMotionConstraintMultiplier = value;

            // Save to DataManager
            MRET.DataManager.SaveValue(flyMotionConstraintMultiplerFastKey, MRET.InputRig.FlyingFastMotionConstraintMultiplier);

            Log("Flying fast motion constraint multiplier set to: " + MRET.InputRig.FlyingFastMotionConstraintMultiplier,
                nameof(SetFlyingFastMotionConstraintMultiplier));
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
                return MRET.InputRig.FlyingGravityConstraint;
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
            MRET.InputRig.FlyingGravityConstraint = value;

            // Save to DataManager
            MRET.DataManager.SaveValue(flyGravityConstraintKey, MRET.InputRig.FlyingGravityConstraint);

            Log("Flying gravity constraint multiplier set to: " + MRET.InputRig.FlyingGravityConstraint,
                nameof(SetFlyingGravityConstraint));
        }

        /// <summary>
        /// Toggles flying locomotion.
        /// </summary>
        public void ToggleFlying()
        {
            if ((bool)MRET.DataManager.FindPoint(flyKey) == false)
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
        /// Updates flying locomotion to reflect the current state. Available for subclasses to override.
        /// </summary>
        protected virtual void UpdateFlyingState()
        {
            // Obtain the current locomotion state
            bool flying = (bool)MRET.DataManager.FindPoint(flyKey);

            // Only enable if the state is on AND we are not paused. Disable otherwise.
            if (!Paused && flying)
            {
                // Enable flying
                MRET.InputRig.EnableFlying();
            }
            else
            {
                // Disable flying
                MRET.InputRig.DisableFlying();
            }

            // Gravity
            ApplyGravityFromEnabledLocomotion();
        }

        /// <summary>
        /// Enables flying locomotion.
        /// </summary>
        public void EnableFlying()
        {
            // Obtain the current locomotion state
            bool flying = (bool)MRET.DataManager.FindPoint(flyKey);

            // Check that not already enabled
            if (flying)
            {
#if MRET_DEBUG
                LogWarning("Fly already enabled", nameof(EnableFlying));
#endif
                return;
            }

            // Save the new state to the DataManager
            MRET.DataManager.SaveValue(flyKey, true);

            // Update the flying state
            UpdateFlyingState();

            Log("Fly enabled", nameof(EnableFlying));
        }

        /// <summary>
        /// Disables flying locomotion.
        /// </summary>
        public void DisableFlying()
        {
            // Obtain the current locomotion state
            bool flying = (bool)MRET.DataManager.FindPoint(flyKey);

            // Check that not already disabled
            if (!flying)
            {
#if MRET_DEBUG
                LogWarning("Fly already disabled", nameof(DisableFlying));
#endif
                return;
            }

            // Save the new state to the DataManager
            MRET.DataManager.SaveValue(flyKey, false);

            // Update the flying state
            UpdateFlyingState();

            Log("Fly disabled", nameof(DisableFlying));
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
                return MRET.InputRig.NavigationNormalMotionConstraintMultiplier;
            }
        }

        /// <summary>
        /// Sets the normal navigation motion constraint multiplier
        /// </summary>
        /// <param name="value">The new motion constraint multipler</param>
        protected virtual void SetNavigationNormalMotionConstraintMultiplier(float value)
        {
            // Set the navigation motion multiplier
            MRET.InputRig.NavigationNormalMotionConstraintMultiplier = value;

            // Save to DataManager
            MRET.DataManager.SaveValue(navigateMotionConstraintMultiplerNormalKey, MRET.InputRig.NavigationNormalMotionConstraintMultiplier);

            Log("Navigation normal motion constraint multiplier set to: " + MRET.InputRig.NavigationNormalMotionConstraintMultiplier,
                nameof(SetNavigationNormalMotionConstraintMultiplier));
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
                return MRET.InputRig.NavigationSlowMotionConstraintMultiplier;
            }
        }

        /// <summary>
        /// Sets the slow navigation motion constraint multiplier
        /// </summary>
        /// <param name="value">The new motion constraint multipler</param>
        protected virtual void SetNavigationSlowMotionConstraintMultiplier(float value)
        {
            // Set the navigation motion multiplier
            MRET.InputRig.NavigationSlowMotionConstraintMultiplier = value;

            // Save to DataManager
            MRET.DataManager.SaveValue(navigateMotionConstraintMultiplerSlowKey, MRET.InputRig.NavigationSlowMotionConstraintMultiplier);

            Log("Navigation slow motion constraint multiplier set to: " + MRET.InputRig.NavigationSlowMotionConstraintMultiplier,
                nameof(SetNavigationSlowMotionConstraintMultiplier));
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
                return MRET.InputRig.NavigationFastMotionConstraintMultiplier;
            }
        }

        /// <summary>
        /// Sets the fast navigation motion constraint multiplier
        /// </summary>
        /// <param name="value">The new motion constraint multipler</param>
        protected virtual void SetNavigationFastMotionConstraintMultiplier(float value)
        {
            // Set the navigation motion multiplier
            MRET.InputRig.NavigationFastMotionConstraintMultiplier = value;

            // Save to DataManager
            MRET.DataManager.SaveValue(navigateMotionConstraintMultiplerFastKey, MRET.InputRig.NavigationFastMotionConstraintMultiplier);

            Log("Navigation fast motion constraint multiplier set to: " + MRET.InputRig.NavigationFastMotionConstraintMultiplier,
                nameof(SetNavigationFastMotionConstraintMultiplier));
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
                return MRET.InputRig.NavigationGravityConstraint;
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
            MRET.InputRig.NavigationGravityConstraint = value;

            // Save to DataManager
            MRET.DataManager.SaveValue(navigateGravityConstraintKey, MRET.InputRig.NavigationGravityConstraint);

            Log("Navigation gravity constraint set to: " + MRET.InputRig.NavigationGravityConstraint,
                nameof(SetNavigationGravityConstraint));
        }

        /// <summary>
        /// Toggles navigation locomotion.
        /// </summary>
        public void ToggleNavigation()
        {
            if ((bool)MRET.DataManager.FindPoint(navigateKey) == false)
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
        /// Updates navigation locomotion to reflect the current state. Available for subclasses to override.
        /// </summary>
        protected virtual void UpdateNavigationState()
        {
            // Obtain the current locomotion state
            bool navigating = (bool)MRET.DataManager.FindPoint(navigateKey);

            // Only enable if the state is on AND we are not paused. Disable otherwise.
            if (!Paused && navigating)
            {
                // Enable navigation
                MRET.InputRig.EnableNavigation();
            }
            else
            {
                // Disable navigation
                MRET.InputRig.DisableNavigation();
            }

            // Gravity
            ApplyGravityFromEnabledLocomotion();
        }

        /// <summary>
        /// Enables navigation locomotion.
        /// </summary>
        public void EnableNavigate()
        {
            // Obtain the current locomotion state
            bool navigating = (bool)MRET.DataManager.FindPoint(navigateKey);

            // Check that not already enabled
            if (navigating)
            {
#if MRET_DEBUG
                LogWarning("Navigate already enabled", nameof(EnableNavigate));
#endif
                return;
            }

            // Save the new state to the DataManager
            MRET.DataManager.SaveValue(navigateKey, true);

            // Update the navigation state
            UpdateNavigationState();

            Log("Navigate enabled", nameof(EnableNavigate));
        }

        /// <summary>
        /// Disables navigation locomotion.
        /// </summary>
        public void DisableNavigate()
        {
            // Obtain the current locomotion state
            bool navigating = (bool)MRET.DataManager.FindPoint(navigateKey);

            // Check that not already disabled
            if (!navigating)
            {
#if MRET_DEBUG
                LogWarning("Navigate already disabled", nameof(DisableNavigate));
#endif
                return;
            }

            // Save the new state to the DataManager
            MRET.DataManager.SaveValue(navigateKey, false);

            // Update the navigation state
            UpdateNavigationState();

            Log("Navigate disabled", nameof(DisableNavigate));
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
                return MRET.InputRig.ArmswingNormalMotionConstraintMultiplier;
            }
        }

        /// <summary>
        /// Sets the normal arm swing motion constraint multiplier
        /// </summary>
        /// <param name="value">The new motion constraint multipler</param>
        protected virtual void SetArmswingNormalMotionConstraintMultiplier(float value)
        {
            // Set the arm swing motion multiplier
            MRET.InputRig.ArmswingNormalMotionConstraintMultiplier = value;

            // Save to DataManager
            MRET.DataManager.SaveValue(armswingMotionConstraintMultiplerNormalKey, MRET.InputRig.ArmswingNormalMotionConstraintMultiplier);

            Log("Armswing normal motion constraint multiplier set to: " + MRET.InputRig.ArmswingNormalMotionConstraintMultiplier,
                nameof(SetArmswingNormalMotionConstraintMultiplier));
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
                return MRET.InputRig.ArmswingSlowMotionConstraintMultiplier;
            }
        }

        /// <summary>
        /// Sets the slow arm swing motion constraint multiplier
        /// </summary>
        /// <param name="value">The new motion constraint multipler</param>
        protected virtual void SetArmswingSlowMotionConstraintMultiplier(float value)
        {
            // Set the arm swing motion multiplier
            MRET.InputRig.ArmswingSlowMotionConstraintMultiplier = value;

            // Save to DataManager
            MRET.DataManager.SaveValue(armswingMotionConstraintMultiplerSlowKey, MRET.InputRig.ArmswingSlowMotionConstraintMultiplier);

            Log("Armswing slow motion constraint multiplier set to: " + MRET.InputRig.ArmswingSlowMotionConstraintMultiplier,
                nameof(SetArmswingSlowMotionConstraintMultiplier));
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
                return MRET.InputRig.ArmswingFastMotionConstraintMultiplier;
            }
        }

        /// <summary>
        /// Sets the fast arm swing motion constraint multiplier
        /// </summary>
        /// <param name="value">The new motion constraint multipler</param>
        protected virtual void SetArmswingFastMotionConstraintMultiplier(float value)
        {
            // Set the arm swing motion multiplier
            MRET.InputRig.ArmswingFastMotionConstraintMultiplier = value;

            // Save to DataManager
            MRET.DataManager.SaveValue(armswingMotionConstraintMultiplerFastKey, MRET.InputRig.ArmswingFastMotionConstraintMultiplier);

            Log("Armswing fast motion constraint multiplier set to: " + MRET.InputRig.ArmswingFastMotionConstraintMultiplier,
                nameof(SetArmswingFastMotionConstraintMultiplier));
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
                return MRET.InputRig.ArmswingGravityConstraint;
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
            MRET.InputRig.ArmswingGravityConstraint = value;

            // Save to DataManager
            MRET.DataManager.SaveValue(armswingGravityConstraintKey, MRET.InputRig.ArmswingGravityConstraint);

            Log("Armswing gravity constraint set to: " + MRET.InputRig.ArmswingGravityConstraint,
                nameof(SetArmswingGravityConstraint));
        }

        /// <summary>
        /// Toggles armswing locomotion.
        /// </summary>
        public void ToggleArmswing()
        {
            if ((bool)MRET.DataManager.FindPoint(armswingKey) == false)
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
        /// Updates armswing locomotion to reflect the current state. Available for subclasses to override.
        /// </summary>
        protected virtual void UpdateArmswingState()
        {
            // Obtain the current locomotion state
            bool armswinging = (bool)MRET.DataManager.FindPoint(armswingKey);

            // Only enable if the state is on AND we are not paused. Disable otherwise.
            if (!Paused && armswinging)
            {
                // Enable armswing
                MRET.InputRig.EnableArmswing();
            }
            else
            {
                // Disable armswing
                MRET.InputRig.DisableArmswing();
            }

            // Gravity
            ApplyGravityFromEnabledLocomotion();
        }

        /// <summary>
        /// Enables armswing locomotion.
        /// </summary>
        public void EnableArmswing()
        {
            // Obtain the current locomotion state
            bool armswinging = (bool)MRET.DataManager.FindPoint(armswingKey);

            // Check that not already enabled
            if (armswinging)
            {
#if MRET_DEBUG
                LogWarning("Armswing already enabled", nameof(EnableArmswing));
#endif
                return;
            }

            // Save the new state to the DataManager
            MRET.DataManager.SaveValue(armswingKey, true);

            // Update the armswing state
            UpdateArmswingState();

            Log("Armswing enabled", nameof(EnableArmswing));
        }

        /// <summary>
        /// Disables armswing locomotion.
        /// </summary>
        public void DisableArmswing()
        {
            bool armswinging = (bool)MRET.DataManager.FindPoint(armswingKey);

            // Check that not already disabled
            if (!armswinging)
            {
#if MRET_DEBUG
                LogWarning("Armswing already disabled", nameof(DisableArmswing));
#endif
                return;
            }

            // Save the new state to the DataManager
            MRET.DataManager.SaveValue(armswingKey, false);

            // Update the armswing state
            UpdateArmswingState();

            Log("Armswing disabled", nameof(DisableArmswing));
        }

        #endregion // Armswing

        #region Climbing

        /// <summary>
        /// The multiplier to be applied to the normal climbing motion.
        /// </summary>
        public float ClimbingNormalMotionConstraintMultiplier
        {
            set
            {
                SetClimbingNormalMotionConstraintMultiplier(value);
            }
            get
            {
                return MRET.InputRig.ClimbingNormalMotionConstraintMultiplier;
            }
        }

        /// <summary>
        /// Sets the normal climbing motion constraint multiplier
        /// </summary>
        /// <param name="value">The new motion constraint multipler</param>
        protected virtual void SetClimbingNormalMotionConstraintMultiplier(float value)
        {
            // Set the climbing motion multiplier
            MRET.InputRig.ClimbingNormalMotionConstraintMultiplier = value;

            // Save to DataManager
            MRET.DataManager.SaveValue(climbMotionConstraintMultiplerNormalKey, MRET.InputRig.ClimbingNormalMotionConstraintMultiplier);

            Log("Climbing normal motion constraint multiplier set to: " + MRET.InputRig.ClimbingNormalMotionConstraintMultiplier,
                nameof(SetClimbingNormalMotionConstraintMultiplier));
        }

        /// <summary>
        /// The multiplier to be applied to the slow climbing motion.
        /// </summary>
        public float ClimbingSlowMotionConstraintMultiplier
        {
            set
            {
                SetClimbingSlowMotionConstraintMultiplier(value);
            }
            get
            {
                return MRET.InputRig.ClimbingSlowMotionConstraintMultiplier;
            }
        }

        /// <summary>
        /// Sets the slow climbing motion constraint multiplier
        /// </summary>
        /// <param name="value">The new motion constraint multipler</param>
        protected virtual void SetClimbingSlowMotionConstraintMultiplier(float value)
        {
            // Set the climbing motion multiplier
            MRET.InputRig.ClimbingSlowMotionConstraintMultiplier = value;

            // Save to DataManager
            MRET.DataManager.SaveValue(climbMotionConstraintMultiplerSlowKey, MRET.InputRig.ClimbingSlowMotionConstraintMultiplier);

            Log("Climbing slow motion constraint multiplier set to: " + MRET.InputRig.ClimbingSlowMotionConstraintMultiplier,
                nameof(SetClimbingSlowMotionConstraintMultiplier));
        }

        /// <summary>
        /// The multiplier to be applied to the fast climbing motion.
        /// </summary>
        public float ClimbingFastMotionConstraintMultiplier
        {
            set
            {
                SetClimbingFastMotionConstraintMultiplier(value);
            }
            get
            {
                return MRET.InputRig.ClimbingFastMotionConstraintMultiplier;
            }
        }

        /// <summary>
        /// Sets the fast climbing motion constraint multiplier
        /// </summary>
        /// <param name="value">The new motion constraint multipler</param>
        protected virtual void SetClimbingFastMotionConstraintMultiplier(float value)
        {
            // Set the climbing motion multiplier
            MRET.InputRig.ClimbingFastMotionConstraintMultiplier = value;

            // Save to DataManager
            MRET.DataManager.SaveValue(climbMotionConstraintMultiplerFastKey, MRET.InputRig.ClimbingFastMotionConstraintMultiplier);

            Log("Climbing fast motion constraint multiplier set to: " + MRET.InputRig.ClimbingFastMotionConstraintMultiplier,
                nameof(SetClimbingFastMotionConstraintMultiplier));
        }

        /// <summary>
        /// The gravity constraint to be applied to the climbing motion.
        /// </summary>
        public GravityConstraint ClimbingGravityConstraint
        {
            set
            {
                SetClimbingGravityConstraint(value);
            }
            get
            {
                return MRET.InputRig.ClimbingGravityConstraint;
            }
        }

        /// <summary>
        /// Sets the gravity constraint to be applied to the climbing motion.
        /// </summary>
        /// <param name="value">The gravity constraint for climbing</param>
        /// <seealso cref="GravityConstraint"/>
        protected virtual void SetClimbingGravityConstraint(GravityConstraint value)
        {
            // Set the climbing gravity constraint
            MRET.InputRig.ClimbingGravityConstraint = value;

            // Save to DataManager
            MRET.DataManager.SaveValue(climbGravityConstraintKey, MRET.InputRig.ClimbingGravityConstraint);

            Log("Climbing gravity constraint set to: " + MRET.InputRig.ClimbingGravityConstraint,
                nameof(SetClimbingGravityConstraint));
        }

        /// <summary>
        /// Toggles climbing locomotion.
        /// </summary>
        public void ToggleClimbing()
        {
            if ((bool)MRET.DataManager.FindPoint(climbKey) == false)
            {
                EnableClimbing();
            }
            else
            {
                DisableClimbing();
            }
        }

        /// <summary>
        /// Toggles climbing locomotion.
        /// </summary>
        public void ToggleClimbing(bool on)
        {
            if (on)
            {
                EnableClimbing();
            }
            else
            {
                DisableClimbing();
            }
        }

        /// <summary>
        /// Updates climbing locomotion to reflect the current state. Available for subclasses to override.
        /// </summary>
        protected virtual void UpdateClimbingState()
        {
            // Obtain the current locomotion state
            bool climbing = (bool)MRET.DataManager.FindPoint(climbKey);

            // Only enable if the state is on AND we are not paused. Disable otherwise.
            if (!Paused && climbing)
            {
                // Enable climbing
                MRET.InputRig.EnableClimbing();
            }
            else
            {
                // Disable climbing
                MRET.InputRig.DisableClimbing();
            }

            // Gravity
            ApplyGravityFromEnabledLocomotion();
        }

        /// <summary>
        /// Enables climbing locomotion.
        /// </summary>
        public void EnableClimbing()
        {
            // Obtain the current locomotion state
            bool climbing = (bool)MRET.DataManager.FindPoint(climbKey);

            // Check that not already enabled
            if (climbing)
            {
#if MRET_DEBUG
                LogWarning("Climb already enabled", nameof(EnableClimbing));
#endif
                return;
            }

            // Save the new state to the DataManager
            MRET.DataManager.SaveValue(climbKey, true);

            // Update the climbing state
            UpdateClimbingState();

            Log("Climbing enabled", nameof(EnableClimbing));
        }

        /// <summary>
        /// Disables climbing locomotion.
        /// </summary>
        public void DisableClimbing()
        {
            // Obtain the current locomotion state
            bool climbing = (bool)MRET.DataManager.FindPoint(climbKey);

            // Check that not already disabled
            if (!climbing)
            {
#if MRET_DEBUG
                LogWarning("Climb already disabled", nameof(DisableClimbing));
#endif
                return;
            }

            // Save the new state to the DataManager
            MRET.DataManager.SaveValue(climbKey, false);

            // Update the climbing state
            UpdateClimbingState();

            Log("Climbing disabled", nameof(DisableClimbing));
        }

        #endregion // Climbing

        #endregion // Modes

        #region Rotation/Scaling/Exploding

        /// <summary>
        /// Toggles X axis rotation.
        /// </summary>
        public void ToggleXRotation()
        {
            if ((bool)MRET.DataManager.FindPoint(rotateXKey) == false)
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
            if ((bool)MRET.DataManager.FindPoint(rotateYKey) == false)
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
            if ((bool)MRET.DataManager.FindPoint(rotateZKey) == false)
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
            if ((bool)MRET.DataManager.FindPoint(scaleKey) == false)
            {
                EnableScaling();
            }
            else
            {
                DisableScaling();
            }
        }

        /// <summary>
        /// Toggles exploding.
        /// </summary>
        public void ToggleExploding()
        {
            if ((bool)MRET.DataManager.FindPoint(explodeKey) == false)
            {
                EnableExploding();
            }
            else
            {
                DisableExploding();
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
        /// Toggles Exploding. 
        /// </summary>
        /// <param name="on"></param>
        public void ToggleExploding(bool on)
        {
            if (on)
            {
                EnableExploding();
            }

            else
            {
                DisableExploding();
            }
        }

        /// <summary>
        /// Updates rotation to reflect the current state. Available for subclasses to override.
        /// </summary>
        protected virtual void UpdateRotationState()
        {
            // Obtain the current rotation states
            bool rotatingX = (bool)MRET.DataManager.FindPoint(rotateXKey);
            bool rotatingY = (bool)MRET.DataManager.FindPoint(rotateYKey);
            bool rotatingZ = (bool)MRET.DataManager.FindPoint(rotateZKey);
            bool rotating = rotatingX || rotatingY || rotatingZ;

            // Only enable if the state is on AND we are not paused. Disable otherwise.
            if (!Paused && rotating)
            {
                // Enable rotation
                foreach (RotateObjectTransform rot in rots)
                {
                    // Enable rotation
                    rot.rotationEnabled = rotating;

                    // Save the new state to the DataManager
                    MRET.DataManager.SaveValue(rotateKey, rotating);

                    // Enable each axis based upon the state
                    rot.xEnabled = rotatingX;
                    rot.yEnabled = rotatingY;
                    rot.zEnabled = rotatingZ;
                }
            }
            else
            {
                // Disable rotation
                foreach (RotateObjectTransform rot in rots)
                {
                    // Disable rotation
                    rot.rotationEnabled = false;

                    // Save the new state to the DataManager
                    MRET.DataManager.SaveValue(rotateKey, false);

                    // Disable each axis
                    rot.xEnabled = false;
                    rot.yEnabled = false;
                    rot.zEnabled = false;
                }
            }
        }

        /// <summary>
        /// Enables X axis rotation.
        /// </summary>
        public void EnableXRotation()
        {
            // Obtain the current rotating X state
            bool rotatingX = (bool)MRET.DataManager.FindPoint(rotateXKey);

            // Check that not already enabled
            if (rotatingX)
            {
#if MRET_DEBUG
                LogWarning("X rotation already enabled", nameof(EnableXRotation));
#endif
                return;
            }

            // Save the new state to the DataManager
            MRET.DataManager.SaveValue(rotateXKey, true);

            // Update the rotation state
            UpdateRotationState();

            Log("X rotation enabled", nameof(EnableXRotation));
        }

        /// <summary>
        /// Disables X axis rotation.
        /// </summary>
        public void DisableXRotation()
        {
            // Obtain the current rotating X state
            bool rotatingX = (bool)MRET.DataManager.FindPoint(rotateXKey);

            // Check that not already disabled
            if (!rotatingX)
            {
#if MRET_DEBUG
                LogWarning("X rotation already disabled", nameof(DisableXRotation));
#endif
                return;
            }

            // Save the new state to the DataManager
            MRET.DataManager.SaveValue(rotateXKey, false);

            // Update the rotation state
            UpdateRotationState();

            Log("X rotation disabled", nameof(DisableXRotation));
        }

        /// <summary>
        /// Enables Y axis rotation.
        /// </summary>
        public void EnableYRotation()
        {
            // Obtain the current rotating Y state
            bool rotatingY = (bool)MRET.DataManager.FindPoint(rotateYKey);

            // Check that not already enabled.
            if (rotatingY)
            {
#if MRET_DEBUG
                LogWarning("Y rotation already enabled", nameof(EnableYRotation));
#endif
                return;
            }

            // Save the new state to the DataManager
            MRET.DataManager.SaveValue(rotateYKey, true);

            // Update the rotation state
            UpdateRotationState();

            Log("Y rotation enabled", nameof(EnableYRotation));
        }

        /// <summary>
        /// Disables Y axis rotation.
        /// </summary>
        public void DisableYRotation()
        {
            // Obtain the current rotating Y state
            bool rotatingY = (bool)MRET.DataManager.FindPoint(rotateYKey);

            // Check that not already disabled
            if (!rotatingY)
            {
#if MRET_DEBUG
                LogWarning("Y rotation already disabled", nameof(DisableYRotation));
#endif
                return;
            }

            // Save the new state to the DataManager
            MRET.DataManager.SaveValue(rotateYKey, false);

            // Update the rotation state
            UpdateRotationState();

            Log("Y rotation disabled", nameof(DisableYRotation));
        }

        /// <summary>
        /// Enables Z axis rotation.
        /// </summary>
        public void EnableZRotation()
        {
            // Obtain the current rotating Z state
            bool rotatingZ = (bool)MRET.DataManager.FindPoint(rotateZKey);

            // Check that not already enabled
            if (rotatingZ)
            {
#if MRET_DEBUG
                LogWarning("Z rotation already enabled", nameof(EnableZRotation));
#endif
                return;
            }

            // Save the new state to the DataManager
            MRET.DataManager.SaveValue(rotateZKey, true);

            // Update the rotation state
            UpdateRotationState();

            Log("Z rotation enabled", nameof(EnableZRotation));
        }

        /// <summary>
        /// Disables Z axis rotation.
        /// </summary>
        public void DisableZRotation()
        {
            // Obtain the current rotating Z state
            bool rotatingZ = (bool)MRET.DataManager.FindPoint(rotateZKey);

            // Check that not already disabled
            if (!rotatingZ)
            {
#if MRET_DEBUG
                LogWarning("Z rotation already disabled", nameof(DisableZRotation));
#endif
                return;
            }

            // Save the new state to the DataManager
            MRET.DataManager.SaveValue(rotateZKey, false);

            // Update the rotation state
            UpdateRotationState();

            Log("Z rotation disabled", nameof(DisableZRotation));
        }

        /// <summary>
        /// Updates scaling to reflect the current state. Available for subclasses to override.
        /// </summary>
        protected virtual void UpdateScalingState()
        {
            // Obtain the current scaling state
            bool scaling = (bool)MRET.DataManager.FindPoint(scaleKey);

            // Only enable if the state is on AND we are not paused. Disable otherwise.
            if (!Paused && scaling)
            {
                // Enable scaling
                foreach (ScaleObjectTransform sot in sots)
                {
                    sot.scalingEnabled = true;
                }
            }
            else
            {
                // Disable scaling
                foreach (ScaleObjectTransform sot in sots)
                {
                    sot.scalingEnabled = false;
                }
            }
        }

        /// <summary>
        /// Updates exploding for the current state. Available for subclasses to override. 
        /// </summary>

        protected virtual void UpdateExplodingState()
        {
            // Obtain the current exploding state
            bool exploding = (bool)MRET.DataManager.FindPoint(explodeKey);
            object explodingObject = MRET.DataManager.FindPoint(explodeObjectKey);

            // Only enable if the state is on AND we are not paused. Disable otherwise.
            if (!Paused && exploding)
            {
                // Enable exploding
                foreach (ExplodeObjectTransform eot in eots)
                {
                    eot.explodingEnabled = true;
                    if (explodingObject is IPhysicalSceneObject)
                    {
                        eot.explodingObject = explodingObject as IPhysicalSceneObject;
                    }
                }
            }
            else
            {
                // Disable exploding
                foreach (ExplodeObjectTransform eot in eots)
                {
                    eot.explodingEnabled = false;
                    if (explodingObject is IPhysicalSceneObject)
                    {
                        eot.explodingObject = explodingObject as IPhysicalSceneObject;
                    }
                }
            }
        }

        /// <summary>
        /// Enables scaling.
        /// </summary>
        public void EnableScaling()
        {
            // Obtain the current scaling state
            bool scaling = (bool)MRET.DataManager.FindPoint(scaleKey);

            // Check that not already enabled
            if (scaling)
            {
#if MRET_DEBUG
                LogWarning("Scaling already enabled", nameof(EnableScaling));
#endif
                return;
            }

            // Save the new state to the DataManager
            MRET.DataManager.SaveValue(scaleKey, true);

            // Update the scaling state
            UpdateScalingState();

            Log("Scaling enabled", nameof(EnableScaling));
        }

        /// <summary>
        /// Disables scaling.
        /// </summary>
        public void DisableScaling()
        {
            // Obtain the current scaling state
            bool scaling = (bool)MRET.DataManager.FindPoint(scaleKey);

            // Check that not already disabled
            if (!scaling)
            {
#if MRET_DEBUG
                LogWarning("Scaling already disabled", nameof(DisableScaling));
#endif
                return;
            }

            // Save the new state to the DataManager
            MRET.DataManager.SaveValue(scaleKey, false);

            // Update the scaling state
            UpdateScalingState();

            Log("Scaling disabled", nameof(DisableScaling));
        }

        /// <summary>
        /// Enables exploding. 
        /// </summary>
        public void EnableExploding()
        {
            // Obtain the current exploding state
            bool exploding = (bool)MRET.DataManager.FindPoint(explodeKey);

            // Check that not already enabled
            if (exploding)
            {
#if MRET_DEBUG
                LogWarning("Exploding already enabled", nameof(EnableExploding));
#endif
                return;
            }

            // Save the new state to the DataManager
            MRET.DataManager.SaveValue(explodeKey, true);

            // Update the exploding state
            UpdateExplodingState();

            Log("Exploding enabled", nameof(EnableExploding));
        }

        /// <summary>
        /// Disables exploding. 
        /// </summary>
        public void DisableExploding()
        {
            // Obtain the current exploding state
            bool exploding = (bool)MRET.DataManager.FindPoint(explodeKey);

            // Check that not already disabled
            if (!exploding)
            {
#if MRET_DEBUG
                LogWarning("Exploding already disabled", nameof(DisableExploding));
#endif
                return;
            }

            // Save the new state to the DataManager
            MRET.DataManager.SaveValue(explodeKey, false);
            MRET.DataManager.SaveValue(explodeObjectKey, null);

            // Update the exploding state
            UpdateExplodingState();

            Log("Exploding disabled", nameof(DisableExploding));
        }

        #endregion // Rotation/Scaling/Exploding

    }
}