// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using GOV.NASA.GSFC.XR.MRET.SceneObjects;

namespace GOV.NASA.GSFC.XR.MRET.Collaboration
{
    /// <remarks>
    /// History:
    /// 6 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// SynchronizedPhysicalSceneObject
	///
	/// Performs synchronization on a physical scene object in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public abstract class SynchronizedPhysicalSceneObject<T> : SynchronizedInteractable<T>, ISynchronizedPhysicalSceneObject<T>
        where T : IPhysicalSceneObject
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(SynchronizedPhysicalSceneObject<T>);

        // Tracked properties
        private bool lastVisible;
        private float lastOpacity;
        private bool lastRandomizeTextures;
        private bool lastEnableCollisions;
        private bool lastEnableGravity;

        #region ISynchronizedPhysicalSceneObject
        /// <seealso cref="ISynchronizedPhysicalSceneObject.SynchronizedObject"/>
        IPhysicalSceneObject ISynchronizedPhysicalSceneObject.SynchronizedObject => SynchronizedObject;

        /// <seealso cref="ISynchronizedPhysicalSceneObject.Initialize(IPhysicalSceneObject)"/>
        void ISynchronizedPhysicalSceneObject.Initialize(IPhysicalSceneObject synchronizedObject)
        {
            Initialize((T)synchronizedObject);
        }
        #endregion ISynchronizedPhysicalSceneObject

        /// <seealso cref="MRETBehaviour.IntegrityCheck"/>
        protected override IntegrityState IntegrityCheck()
        {
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            return (
                (state == IntegrityState.Failure) // TODO: || (MyRequiredRef == null)

                    ? IntegrityState.Failure   // Fail is base class fails or anything is null
                    : IntegrityState.Success); // Otherwise, our integrity is valid
        }

        /// <seealso cref="MRETBehaviour.MRETAwake"/>
        protected override void MRETAwake()
        {
            // Take the inherited behavior
            base.MRETAwake();

            // TODO: Custom initialization (before deserialization)
        }

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            // Take the inherited behavior
            base.MRETStart();
        }

        /// <seealso cref="Synchronized{T}.CheckForUpdate"/>
        protected override void CheckForUpdate()
        {
            // Take the inherited behavior
            base.CheckForUpdate();

            // TODO: PhysicalSceneObject Settings. Can do this with polling here or with property listeners
            if (SynchronizedObject != null)
            {
                bool needsUpdate = false;

                // Visible
                if (SynchronizedObject.Visible != lastVisible)
                {
                    lastVisible = SynchronizedObject.Visible;
                    needsUpdate = true;
                }

                // Opacity
                if (SynchronizedObject.Opacity != lastOpacity)
                {
                    lastOpacity = SynchronizedObject.Opacity;
                    needsUpdate = true;
                }

                // Randomize Textures
                if (SynchronizedObject.RandomizeTextures != lastRandomizeTextures)
                {
                    lastRandomizeTextures = SynchronizedObject.RandomizeTextures;
                    needsUpdate = true;
                }

                // Collisions
                if (SynchronizedObject.EnableCollisions != lastEnableCollisions)
                {
                    lastEnableCollisions = SynchronizedObject.EnableCollisions;
                    needsUpdate = true;
                }

                // Collisions
                if (SynchronizedObject.EnableGravity != lastEnableGravity)
                {
                    lastEnableGravity = SynchronizedObject.EnableGravity;
                    needsUpdate = true;
                }

                // Notify XRC if we have anything to update
                if (needsUpdate)
                {
                    ReportUpdate();
                }
            }
        }

        /// <seealso cref="SynchronizedInteractable{T}.RefreshState"/>
        protected override void RefreshState()
        {
            // Take the inherited behavior
            base.RefreshState();

            // Interactions
            lastVisible = (SynchronizedObject != null) ? SynchronizedObject.Visible : PhysicalSceneObjectDefaults.VISIBLE;
            lastOpacity = (SynchronizedObject != null) ? SynchronizedObject.Opacity : PhysicalSceneObjectDefaults.OPACITY;
            lastRandomizeTextures = (SynchronizedObject != null) ? SynchronizedObject.RandomizeTextures : PhysicalSceneObjectDefaults.RANDOMIZE_TEXTURES;
            lastEnableCollisions = (SynchronizedObject != null) ? SynchronizedObject.EnableCollisions: PhysicalSceneObjectDefaults.COLLISIONS_ENABLED;
            lastEnableGravity = (SynchronizedObject != null) ? SynchronizedObject.EnableGravity : PhysicalSceneObjectDefaults.GRAVITY_ENABLED;
        }
    }

    /// <summary>
    /// Provides an implementation for the abstract synchronize physical scene object component
    /// </summary>
    public class SynchronizedPhysicalSceneObject : SynchronizedPhysicalSceneObject<IPhysicalSceneObject>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(SynchronizedPhysicalSceneObject);
    }

}
