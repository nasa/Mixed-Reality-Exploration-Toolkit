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
	/// SynchronizedDisplay
	///
	/// Performs synchronization on an interactable display
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public abstract class SynchronizedDisplay<T> : SynchronizedInteractable<T>, ISynchronizedDisplay<T>
        where T : IInteractableDisplay
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(SynchronizedDisplay<T>);

        // Tracked properities
        private string lastTitle;

        #region ISynchronizedDisplay
        /// <seealso cref="ISynchronizedDisplay.SynchronizedObject"/>
        IInteractableDisplay ISynchronizedDisplay.SynchronizedObject => SynchronizedObject;

        /// <seealso cref="ISynchronizedDisplay.Initialize(IInteractableDisplay)"/>
        void ISynchronizedDisplay.Initialize(IInteractableDisplay synchronizedObject)
        {
            Initialize((T)synchronizedObject);
        }
        #endregion ISynchronizedDisplay

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

            // Record tracked properties
            lastTitle = (SynchronizedObject != null) ? SynchronizedObject.Title : "";
        }

        /// <seealso cref="Synchronized{T}.CheckForUpdate"/>
        protected override void CheckForUpdate()
        {
            // Take the inherited behavior
            base.CheckForUpdate();

            // TODO: Display Settings. Can do this with polling here or with property listeners
            if (SynchronizedObject != null)
            {
                // Information text
                if (SynchronizedObject.Title != lastTitle)
                {
                    lastTitle = SynchronizedObject.Title;
                    ReportUpdate();
                }
            }

            if (SynchronizedObject != null)
            {
                bool needsUpdate = false;

                // Information text
                if (SynchronizedObject.Title != lastTitle)
                {
                    lastTitle = SynchronizedObject.Title;
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

            // Title
            lastTitle = (SynchronizedObject != null) ? SynchronizedObject.Title : "";
        }

    }

    /// <summary>
    /// Provides an implementation for the abstract synchronize interactable display component
    /// </summary>
    public class SynchronizedDisplay : SynchronizedDisplay<IInteractableDisplay>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(SynchronizedDisplay);
    }

}
