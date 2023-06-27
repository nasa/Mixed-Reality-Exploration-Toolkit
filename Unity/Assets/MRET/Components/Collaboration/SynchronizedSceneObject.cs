// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.SceneObjects;

namespace GOV.NASA.GSFC.XR.MRET.Collaboration
{
    /// <remarks>
    /// History:
    /// 6 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// SynchronizedSceneObject
	///
	/// Performs synchronization on a scene object
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public abstract class SynchronizedSceneObject<T> : Synchronized<T>, ISynchronizedSceneObject<T>
        where T : ISceneObject
	{
		/// <seealso cref="MRETBehaviour.ClassName"/>
		public override string ClassName => nameof(SynchronizedSceneObject<T>);

        // Tracked properties
        private ISceneObject lastParent;
        private Vector3 lastPosition;
        private Vector3 lastScale;
        private Quaternion lastRotation;

        #region ISynchronizedSceneObject
        /// <seealso cref="ISynchronizedSceneObject.SynchronizedObject"/>
        ISceneObject ISynchronizedSceneObject.SynchronizedObject => SynchronizedObject;

        /// <seealso cref="ISynchronizedSceneObject.Initialize(ISceneObject)"/>
        void ISynchronizedSceneObject.Initialize(ISceneObject synchronizedObject)
        {
            Initialize((T)synchronizedObject);
        }
        #endregion ISynchronizedSceneObject

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

            // Custom initialization (after deserialization)
        }

        /// <seealso cref="Synchronized{T}.CheckForUpdate"/>
        protected override void CheckForUpdate()
        {
            // Take the inherited behavior
            base.CheckForUpdate();

            // TODO: Identifiable Settings. Can do this with polling here or with property listeners
            if (SynchronizedObject != null)
            {
                // TODO: Not sure if we need this
                /*
                Rigidbody rBody = GetComponent<Rigidbody>();
                if (rBody)
                {
                    if (!rBody.IsSleeping())
                    {
                        return;
                    }
                }
                */

                // Parent
                if (SynchronizedObject.parent != lastParent)
                {
                    lastParent = SynchronizedObject.parent;
                    ReportParentUpdate();
                }

                // Transform
                if (transform.hasChanged)
                {
                    bool updateTransform = false;

                    // Position
                    if (transform.position != lastPosition)
                    {
                        lastPosition = transform.position;
                        updateTransform = true;
                    }

                    // Rotation
                    if (transform.rotation != lastRotation)
                    {
                        lastRotation = transform.rotation;
                        updateTransform = true;
                    }

                    // Scale
                    if (transform.localScale != lastScale)
                    {
                        lastScale = transform.localScale;
                        updateTransform = true;
                    }

                    // Notify XRC if we have anything in the transform to update
                    if (updateTransform)
                    {
                        ReportTransformUpdate();
                    }

                    transform.hasChanged = false;
                }
            }
        }

        /// <seealso cref="Synchronized{T}.RefreshState"/>
        protected override void RefreshState()
        {
            // Take the inherited behavior
            base.RefreshState();

            // Parent
            lastParent = (SynchronizedObject != null) ? SynchronizedObject.parent : null;

            // Transform
            lastPosition = (SynchronizedObject != null) ? SynchronizedObject.transform.position : Vector3.zero;
            lastScale = (SynchronizedObject != null) ? SynchronizedObject.transform.localScale : Vector3.one;
            lastRotation = (SynchronizedObject != null) ? SynchronizedObject.transform.rotation : Quaternion.identity;
        }

        /// <summary>
        /// Called to update the synchronized object parent. Available for subclasses to override.
        /// </summary>
        /// <returns></returns>
        protected virtual bool ReportParentUpdate()
        {
            return MRET.CollaborationManager.UpdateSessionEntityParent(SynchronizedObject, SynchronizedObject.parent);
        }

        /// <summary>
        /// Called to update the synchronized object transform. Available for subclasses to override.
        /// </summary>
        /// <returns></returns>
        protected virtual bool ReportTransformUpdate()
        {
            return MRET.CollaborationManager.UpdateSessionEntityTransform(SynchronizedObject);
        }
    }

    /// <summary>
    /// Provides an implementation for the abstract synchronize scene object component
    /// </summary>
    public class SynchronizedSceneObject : Synchronized<ISceneObject>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(SynchronizedSceneObject);
    }

}
