// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.User;

namespace GOV.NASA.GSFC.XR.MRET.Collaboration
{
    /// <remarks>
    /// History:
    /// 13 January 2023: Created
    /// </remarks>
	///
	/// <summary>
	/// SynchronizedUserHandComponent
	///
	/// Performs synchronization on a user hand component
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
    public abstract class SynchronizedUserHandComponent<T> : SynchronizedUserComponent<T>, ISynchronizedUserHandComponent<T>
        where T : IUserHandComponent
    {
        #region ISynchronizedUserHandComponent
        /// <seealso cref="ISynchronizedUserHandComponent.SynchronizedObject"/>
        IUserHandComponent ISynchronizedUserHandComponent.SynchronizedObject => SynchronizedObject;

        /// <seealso cref="ISynchronizedUserHandComponent.Initialize(IInteractable)"/>
        void ISynchronizedUserHandComponent.Initialize(IUserHandComponent synchronizedObject)
        {
            Initialize((T)synchronizedObject);
        }
        #endregion ISynchronizedUserHandComponent

        /// <seealso cref="Synchronized{T}.CheckForUpdate"/>
        protected override void CheckForUpdate()
        {
            // Take the inherited behavior
            base.CheckForUpdate();

            // TODO: User component Settings. Can do this with polling here or with property listeners
        }
    }
}