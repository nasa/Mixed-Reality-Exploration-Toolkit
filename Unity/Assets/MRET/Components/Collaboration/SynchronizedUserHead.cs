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
    /// SynchronizedUserHead
    ///
    /// Performs synchronization on a user head
    ///
    /// Author: Jeffrey Hosler
    /// </summary>
    /// 
    public abstract class SynchronizedUserHead<T> : SynchronizedUserComponent<T>, ISynchronizedUserHead<T>
        where T : IUserHead
    {
        #region ISynchronizedUserHead
        /// <seealso cref="ISynchronizedUserHead.SynchronizedObject"/>
        IUserHead ISynchronizedUserHead.SynchronizedObject => SynchronizedObject;

        /// <seealso cref="ISynchronizedUserHead.Initialize(IInteractable)"/>
        void ISynchronizedUserHead.Initialize(IUserHead synchronizedObject)
        {
            Initialize((T)synchronizedObject);
        }
        #endregion ISynchronizedUserHead

        /// <seealso cref="Synchronized{T}.CheckForUpdate"/>
        protected override void CheckForUpdate()
        {
            // Take the inherited behavior
            base.CheckForUpdate();

            // TODO: Head Settings. Can do this with polling here or with property listeners
        }
    }

    /// <summary>
    /// Provides an implementation for the abstract synchronize user head component
    /// </summary>
    public class SynchronizedUserHead : SynchronizedUserHead<IUserHead>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(SynchronizedUserHead);
    }
}