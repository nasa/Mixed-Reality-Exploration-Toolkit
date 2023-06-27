// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.User;

namespace GOV.NASA.GSFC.XR.MRET.Collaboration
{
    /// <remarks>
    /// History:
    /// 6 December 2022: Created
    /// </remarks>
    ///
    /// <summary>
    /// SynchronizedInteractable
    ///
    /// Performs synchronization on an interactable scene object
    ///
    /// Author: Jeffrey Hosler
    /// </summary>
    /// 
    public class SynchronizedUser<T> : SynchronizedSceneObject<T>, ISynchronizedUser<T>
        where T : IUser
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(SynchronizedUser<T>);

        // Tracked properties
        private string lastAlias;
        private Color lastColor;

        public GameObject userObject;

        #region ISynchronizedInteractable
        /// <seealso cref="ISynchronizedUser.SynchronizedObject"/>
        IUser ISynchronizedUser.SynchronizedObject => SynchronizedObject;

        /// <seealso cref="ISynchronizedUser.Initialize(IInteractable)"/>
        void ISynchronizedUser.Initialize(IUser synchronizedObject)
        {
            Initialize((T)synchronizedObject);
        }
        #endregion ISynchronizedInteractable

        /// <seealso cref="Synchronized{T}.CheckForUpdate"/>
        protected override void CheckForUpdate()
        {
            // Take the inherited behavior
            base.CheckForUpdate();

            // TODO: Interactable
            if (SynchronizedObject != null)
            {
                bool needsUpdate = false;

                // Usable
                if (SynchronizedObject.Alias != lastAlias)
                {
                    lastAlias = SynchronizedObject.Alias;
                    needsUpdate = true;
                }

                // Grabbable
                if (SynchronizedObject.Color != lastColor)
                {
                    lastColor = SynchronizedObject.Color;
                    needsUpdate = true;
                }

                // Notify XRC if we have anything to update
                if (needsUpdate)
                {
                    ReportUpdate();
                }
            }
        }

        /// <seealso cref="SynchronizedSceneObject{T}.RefreshState"/>
        protected override void RefreshState()
        {
            // Take the inherited behavior
            base.RefreshState();

            // User
            lastAlias = (SynchronizedObject != null) ? SynchronizedObject.Alias : UserDefaults.ALIAS;
            lastColor = (SynchronizedObject != null) ? SynchronizedObject.Color : UserDefaults.COLOR;
        }
    }

    /// <summary>
    /// Provides an implementation for the abstract synchronize interactable scene object component
    /// </summary>
    public class SynchronizedUser : SynchronizedUser<IUser>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(SynchronizedUser);
    }

}