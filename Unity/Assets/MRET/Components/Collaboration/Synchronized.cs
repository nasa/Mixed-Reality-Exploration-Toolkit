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
	/// Synchronized
	///
	/// A synchronization component in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public abstract class Synchronized<T> : MRETUpdateBehaviour, ISynchronized<T>
        where T : IIdentifiable
	{
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(Synchronized<T>);

        // Tracked properties
        private string lastName;
        private string lastDescription;

        #region ISynchronized
        /// <seealso cref="ISynchronized.AutoSynchronize"/>
        public bool AutoSynchronize { get; set; }

        /// <seealso cref="ISynchronized.Lock"/>
        public Object Lock { get; private set; }  // TODO: Not sure we need this?

        /// <seealso cref="ISynchronized.SynchronizeEnabled"/>
        public bool SynchronizeEnabled { get; set; }

        /// <seealso cref="ISynchronized.Paused"/>
        public bool Paused { get; private set; }

        /// <seealso cref="ISynchronized.SynchronizedObject"/>
        IIdentifiable ISynchronized.SynchronizedObject => SynchronizedObject;

        /// <seealso cref="ISynchronized{T}.SynchronizedObject"/>
        public T SynchronizedObject { get; set; }

        /// <seealso cref="ISynchronized.Initialize(IIdentifiable)"/>
        void ISynchronized.Initialize(IIdentifiable synchronizedObject)
        {
            Initialize((T)synchronizedObject);
        }

        /// <seealso cref="ISynchronized{T}.Initialize(T)"/>
        public void Initialize(T synchronizedObject)
        {
            SynchronizedObject = synchronizedObject;
        }

        /// <seealso cref="ISynchronized.Pause"/>
        public void Pause() => Paused = true;

        /// <seealso cref="ISynchronized.Resume"/>
        public void Resume()
        {
            Paused = false;
            RefreshState();
        }
        #endregion ISynchronized

        #region MRETUpdateBehaviour
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

            // Set the update rate to some reasonable default
            updateRate = UpdateFrequency.Hz10;

            // Custom initialization (before deserialization)
            Lock = new Object();
        }

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
		{
			// Take the inherited behavior
			base.MRETStart();

            // Custom initialization (after deserialization)

            // Record tracked properties
            RefreshState();
        }

        /// <seealso cref="MRETUpdateBehaviour.MRETUpdate"/>
        protected override void MRETUpdate()
        {
            // Take the inherited behavior
            base.MRETUpdate();

            // Tell subclasses to check for a synchonization update
            lock (Lock)
            {
                // There are several conditions where we don't do any processing
                if ((SynchronizedObject != null) &&
                    SynchronizedObject.gameObject.activeInHierarchy &&
                    SynchronizedObject.SynchronizationEnabled &&
                    AutoSynchronize &&
                    !Paused)
                {
                    // Make sure the entity exists in the session
                    if (!MRET.CollaborationManager.SessionEntityExists(SynchronizedObject))
                    {
                        // See if the entity has a parent
                        IIdentifiable parent = null;
                        if (SynchronizedObject is ISceneObject)
                        {
                            parent = (SynchronizedObject as ISceneObject).parent;
                        }

                        // Add the entity
                        if (MRET.CollaborationManager.AddSessionEntity(SynchronizedObject, parent))
                        {
                            // Refresh the state
                            RefreshState();
                        }
                    }
                    else
                    {
                        // The synchronized object is in the session, so monitor for updates
                        CheckForUpdate();
                    }
                }
            }
        }

        /// <seealso cref="MRETBehaviour.MRETOnDestroy"/>
        protected override void MRETOnDestroy()
        {
            // Take the inherited behavior
            base.MRETOnDestroy();

            if ((SynchronizedObject != null) && SynchronizedObject.SynchronizationEnabled && AutoSynchronize)
            {
                // Remove the controlled object
                MRET.CollaborationManager.RemoveSessionEntity(SynchronizedObject);
            }
        }
        #endregion MRETUpdateBehaviour

        /// <summary>
        /// Called to refresh the state of the tracked properties
        /// </summary>
        protected virtual void RefreshState()
        {
            lastName = (SynchronizedObject != null) ? SynchronizedObject.name : "";
            lastDescription = (SynchronizedObject != null) ? SynchronizedObject.description : "";
        }

        /// <summary>
        /// Called to perform an update. Subclasses should override for custom behavior
        /// </summary>
        protected virtual void CheckForUpdate()
        {
            // TODO: Identifiable Settings. Can do this with polling here or with property listeners

            // See if we need an update
            bool needsUpdate = false;

            // Name
            if (SynchronizedObject.name != lastName)
            {
                lastName = SynchronizedObject.name;
                needsUpdate = true;
            }

            // Description
            if (SynchronizedObject.description != lastDescription)
            {
                lastDescription = SynchronizedObject.description;
                needsUpdate = true;
            }

            // Notify XRC if we have anything to update
            if (needsUpdate)
            {
                ReportUpdate();
            }
        }

        protected virtual void ReportUpdate()
        {
            // See if the entity has a parent
            IIdentifiable parent = null;
            if (SynchronizedObject is ISceneObject)
            {
                parent = (SynchronizedObject as ISceneObject).parent;
            }

            // Update the entity
            MRET.CollaborationManager.UpdateSessionEntity(SynchronizedObject, parent);
        }
    }

    /// <summary>
    /// Provides an implementation for the abstract synchronization component class
    /// </summary>
    public class Synchronized : Synchronized<IIdentifiable>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(Synchronized);
    }

}
