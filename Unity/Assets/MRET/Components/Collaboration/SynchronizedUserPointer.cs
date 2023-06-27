// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.User;

namespace GOV.NASA.GSFC.XR.MRET.Collaboration
{
    /// <remarks>
    /// History:
    /// 8 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// SynchronizedUserPointer
	///
	/// Performs synchronization on a user pointer
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
    public abstract class SynchronizedUserPointer<T> : SynchronizedUserHandComponent<T>, ISynchronizedUserPointer<T>
        where T : IUserPointer
    {
        #region ISynchronizedUserPointer
        /// <seealso cref="ISynchronizedUserPointer.SynchronizedObject"/>
        IUserPointer ISynchronizedUserPointer.SynchronizedObject => SynchronizedObject;

        /// <seealso cref="ISynchronizedUserPointer.Initialize(IInteractable)"/>
        void ISynchronizedUserPointer.Initialize(IUserPointer synchronizedObject)
        {
            Initialize((T)synchronizedObject);
        }
        #endregion ISynchronizedUserPointer

        /// <seealso cref="Synchronized{T}.CheckForUpdate"/>
        protected override void CheckForUpdate()
        {
            // Take the inherited behavior
            base.CheckForUpdate();

            // TODO: Pointer Settings. Can do this with polling here or with property listeners
        }
    }

    /// <summary>
    /// Provides an implementation for the abstract synchronize user pointer component
    /// </summary>
    public class SynchronizedPointer : SynchronizedUserPointer<IUserPointer>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(SynchronizedPointer);
    }
}