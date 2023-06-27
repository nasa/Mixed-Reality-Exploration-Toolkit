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
    /// SynchronizedUserHand
    ///
    /// Performs synchronization on a user hand
    ///
    /// Author: Jeffrey Hosler
    /// </summary>
    /// 
    public abstract class SynchronizedUserHand<T> : SynchronizedUserComponent<T>, ISynchronizedUserHand<T>
        where T : IUserHand
    {
        #region ISynchronizedUserHand
        /// <seealso cref="ISynchronizedUserHand.SynchronizedObject"/>
        IUserHand ISynchronizedUserHand.SynchronizedObject => SynchronizedObject;

        /// <seealso cref="ISynchronizedUserHand.Initialize(IInteractable)"/>
        void ISynchronizedUserHand.Initialize(IUserHand synchronizedObject)
        {
            Initialize((T)synchronizedObject);
        }
        #endregion ISynchronizedUserHand

        /// <seealso cref="Synchronized{T}.CheckForUpdate"/>
        protected override void CheckForUpdate()
        {
            // Take the inherited behavior
            base.CheckForUpdate();

            // TODO: Hand Settings. Can do this with polling here or with property listeners
        }
    }

    /// <summary>
    /// Provides an implementation for the abstract synchronize user hand component
    /// </summary>
    public class SynchronizedUserHand : SynchronizedUserHand<IUserHand>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(SynchronizedUserHand);
    }
}