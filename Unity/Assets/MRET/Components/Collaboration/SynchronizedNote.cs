// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Note;

namespace GOV.NASA.GSFC.XR.MRET.Collaboration
{
    /// <remarks>
    /// History:
    /// 6 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// SynchronizedNote
	///
	/// Performs synchronization on an interactable note
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public abstract class SynchronizedNote<T> : SynchronizedDisplay<T>, ISynchronizedNote<T>
        where T : InteractableNote
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(SynchronizedNote<T>);

        // Tracked properities
        private string lastInformationText;

        #region ISynchronizedNote
        /// <seealso cref="ISynchronizedNote.SynchronizedObject"/>
        InteractableNote ISynchronizedNote.SynchronizedObject => SynchronizedObject;

        /// <seealso cref="ISynchronizedNote.Initialize(InteractableNote)"/>
        void ISynchronizedNote.Initialize(InteractableNote synchronizedObject)
        {
            Initialize((T)synchronizedObject);
        }
        #endregion ISynchronizedNote

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

            // Initialize this class
            lastInformationText = (SynchronizedObject != null) ? SynchronizedObject.InformationText : "";
        }

        /// <seealso cref="Synchronized{T}.CheckForUpdate"/>
        protected override void CheckForUpdate()
        {
            // Take the inherited behavior
            base.CheckForUpdate();

            // TODO: Note Settings. Can do this with polling here or with property listeners
            if (SynchronizedObject != null)
            {
                bool needsUpdate = false;

                // Information text
                if (SynchronizedObject.InformationText != lastInformationText)
                {
                    lastInformationText = SynchronizedObject.InformationText;
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

            // Text
            lastInformationText = (SynchronizedObject != null) ? SynchronizedObject.InformationText : "";
        }
    }

    /// <summary>
    /// Provides an implementation for the abstract synchronize interactable note component
    /// </summary>
    public class SynchronizedNote : SynchronizedNote<InteractableNote>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(SynchronizedNote);
    }
}