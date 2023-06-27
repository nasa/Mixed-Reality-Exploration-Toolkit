// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Part;

namespace GOV.NASA.GSFC.XR.MRET.Collaboration
{
    /// <remarks>
    /// History:
    /// 6 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// SynchronizedPart
	///
	/// Performs synchronization on a part in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public abstract class SynchronizedPart<T> : SynchronizedPhysicalSceneObject<T>, ISynchronizedPart<T>
        where T : InteractablePart
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(SynchronizedPart<T>);

        #region ISynchronizedPart
        /// <seealso cref="ISynchronizedPart.SynchronizedObject"/>
        InteractablePart ISynchronizedPart.SynchronizedObject => SynchronizedObject;

        /// <seealso cref="ISynchronizedPart.Initialize(InteractablePart)"/>
        void ISynchronizedPart.Initialize(InteractablePart synchronizedObject)
        {
            Initialize((T)synchronizedObject);
        }
        #endregion ISynchronizedPart

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

            // TODO: Part Settings. Can do this with polling here or with property listeners
        }
    }

    /// <summary>
    /// Provides an implementation for the abstract synchronize part component
    /// </summary>
    public class SynchronizedPart : SynchronizedPart<InteractablePart>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(SynchronizedPart);
    }

}
