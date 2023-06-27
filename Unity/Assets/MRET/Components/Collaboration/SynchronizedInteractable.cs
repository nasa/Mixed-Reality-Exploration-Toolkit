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
	/// SynchronizedInteractable
	///
	/// Performs synchronization on an interactable scene object
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public abstract class SynchronizedInteractable<T> : SynchronizedSceneObject<T>, ISynchronizedInteractable<T>
        where T : IInteractable
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(SynchronizedInteractable<T>);

        // Tracked properties
        private bool lastUsable;
        private bool lastGrabbable;

        #region ISynchronizedInteractable
        /// <seealso cref="ISynchronizedInteractable.SynchronizedObject"/>
        IInteractable ISynchronizedInteractable.SynchronizedObject => SynchronizedObject;

        /// <seealso cref="ISynchronizedInteractable.Initialize(IInteractable)"/>
        void ISynchronizedInteractable.Initialize(IInteractable synchronizedObject)
        {
            Initialize((T)synchronizedObject);
        }
        #endregion ISynchronizedInteractable

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

            // Interactable
            if (SynchronizedObject != null)
            {
                bool needsUpdate = false;

                // Usable
                if (SynchronizedObject.Usable != lastUsable)
                {
                    lastUsable = SynchronizedObject.Usable;
                    needsUpdate = true;
                }

                // Grabbable
                if (SynchronizedObject.Grabbable != lastGrabbable)
                {
                    lastGrabbable = SynchronizedObject.Grabbable;
                    needsUpdate = true;
                }

                // Notify XRC if we have anything to update
                if (needsUpdate)
                {
                    ReportUpdate();
                }
            }
        }

        /// <seealso cref="SynchronizedSceneObject{T}.RefreshState"/>
        protected override void RefreshState()
        {
            // Take the inherited behavior
            base.RefreshState();

            // Interactions
            lastUsable = (SynchronizedObject != null) ? SynchronizedObject.Usable : InteractableDefaults.USABLE;
            lastGrabbable = (SynchronizedObject != null) ? SynchronizedObject.Grabbable : InteractableDefaults.INTERACTABLE;
        }
    }

    /// <summary>
    /// Provides an implementation for the abstract synchronize interactable scene object component
    /// </summary>
    public class SynchronizedInteractable : SynchronizedInteractable<IInteractable>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(SynchronizedInteractable);
    }

}
