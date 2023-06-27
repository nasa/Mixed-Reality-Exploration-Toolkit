// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing;

namespace GOV.NASA.GSFC.XR.MRET.Collaboration
{
    /// <remarks>
    /// History:
    /// 6 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// SynchronizedDrawing
	///
	/// Performs synchronization on an interactable drawing
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public abstract class SynchronizedDrawing<T> : SynchronizedInteractable<T>, ISynchronizedDrawing<T>
        where T : IInteractableDrawing
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(SynchronizedDrawing<T>);

        #region ISynchronizedDrawing
        /// <seealso cref="ISynchronizedDrawing.SynchronizedObject"/>
        IInteractableDrawing ISynchronizedDrawing.SynchronizedObject => SynchronizedObject;

        /// <seealso cref="ISynchronizedDrawing.Initialize(IInteractableDrawing)"/>
        void ISynchronizedDrawing.Initialize(IInteractableDrawing synchronizedObject)
        {
            Initialize((T)synchronizedObject);
        }
        #endregion ISynchronizedDrawing

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

            // TODO: Drawing Settings. Can do this with polling here or with property listeners
        }
    }

    /// <summary>
    /// Provides an implementation for the abstract synchronize interactable drawing component
    /// </summary>
    public class SynchronizedDrawing : SynchronizedDrawing<IInteractableDrawing>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(SynchronizedDrawing);
    }

}
