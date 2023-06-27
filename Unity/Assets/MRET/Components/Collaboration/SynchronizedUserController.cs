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
    /// SynchronizedUserController
    ///
    /// Performs synchronization on a user controller
    ///
    /// Author: Jeffrey Hosler
    /// </summary>
    /// 
    public abstract class SynchronizedUserController<T> : SynchronizedUserHandComponent<T>, ISynchronizedUserController<T>
        where T : IUserController
    {
        #region ISynchronizedUserController
        /// <seealso cref="ISynchronizedUserController.SynchronizedObject"/>
        IUserController ISynchronizedUserController.SynchronizedObject => SynchronizedObject;

        /// <seealso cref="ISynchronizedUserController.Initialize(IInteractable)"/>
        void ISynchronizedUserController.Initialize(IUserController synchronizedObject)
        {
            Initialize((T)synchronizedObject);
        }
        #endregion ISynchronizedUserController

        /// <seealso cref="Synchronized{T}.CheckForUpdate"/>
        protected override void CheckForUpdate()
        {
            // Take the inherited behavior
            base.CheckForUpdate();

            // TODO: Controller Settings. Can do this with polling here or with property listeners
        }
    }

    /// <summary>
    /// Provides an implementation for the abstract synchronize user controller component
    /// </summary>
    public class SynchronizedController : SynchronizedUserController<IUserController>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(SynchronizedController);
    }
}