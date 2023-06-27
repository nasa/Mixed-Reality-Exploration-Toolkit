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
	/// SynchronizedUserComponent
	///
	/// Performs synchronization on a user component
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
    public abstract class SynchronizedUserComponent<T> : SynchronizedSceneObject<T>, ISynchronizedUserComponent<T>
        where T : IUserComponent
    {
        #region ISynchronizedUserComponent
        /// <seealso cref="ISynchronizedUserComponent.SynchronizedObject"/>
        IUserComponent ISynchronizedUserComponent.SynchronizedObject => SynchronizedObject;

        /// <seealso cref="ISynchronizedUserComponent.Initialize(IInteractable)"/>
        void ISynchronizedUserComponent.Initialize(IUserComponent synchronizedObject)
        {
            Initialize((T)synchronizedObject);
        }
        #endregion ISynchronizedUserComponent

        /// <seealso cref="Synchronized{T}.CheckForUpdate"/>
        protected override void CheckForUpdate()
        {
            // Take the inherited behavior
            base.CheckForUpdate();

            // TODO: User component Settings. Can do this with polling here or with property listeners
        }
    }
}