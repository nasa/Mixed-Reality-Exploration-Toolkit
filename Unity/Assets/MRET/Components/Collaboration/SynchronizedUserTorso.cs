// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using GOV.NASA.GSFC.XR.MRET.SceneObjects.User;

namespace GOV.NASA.GSFC.XR.MRET.Collaboration
{
    /// <remarks>
    /// History:
    /// 13 January 2023: Created
    /// </remarks>
    ///
    /// <summary>
    /// SynchronizedUserTorso
    ///
    /// Performs synchronization on a user torso
    ///
    /// Author: Jeffrey Hosler
    /// </summary>
    /// 
    public abstract class SynchronizedUserTorso<T> : SynchronizedUserComponent<T>, ISynchronizedUserTorso<T>
        where T : IUserTorso
    {
        #region ISynchronizedUserTorso
        /// <seealso cref="ISynchronizedUserTorso.SynchronizedObject"/>
        IUserTorso ISynchronizedUserTorso.SynchronizedObject => SynchronizedObject;

        /// <seealso cref="ISynchronizedUserTorso.Initialize(IInteractable)"/>
        void ISynchronizedUserTorso.Initialize(IUserTorso synchronizedObject)
        {
            Initialize((T)synchronizedObject);
        }
        #endregion ISynchronizedUserTorso

        /// <seealso cref="Synchronized{T}.CheckForUpdate"/>
        protected override void CheckForUpdate()
        {
            // Take the inherited behavior
            base.CheckForUpdate();

            // TODO: Torso Settings. Can do this with polling here or with property listeners
        }
    }

    /// <summary>
    /// Provides an implementation for the abstract synchronize user torso component
    /// </summary>
    public class SynchronizedUserTorso : SynchronizedUserTorso<IUserTorso>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(SynchronizedUserTorso);
    }
}