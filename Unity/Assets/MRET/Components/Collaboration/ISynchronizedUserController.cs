// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using GOV.NASA.GSFC.XR.MRET.SceneObjects.User;

namespace GOV.NASA.GSFC.XR.MRET.Collaboration
{
    /// <remarks>
    /// History:
    /// 8 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// ISynchronizedUserController
	///
	/// An synchronization component of a user hand controller in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface ISynchronizedUserController : ISynchronizedUserHandComponent
    {
        /// <seealso cref="ISynchronizedUserHandComponent.SynchronizedObject"/>
        new public IUserController SynchronizedObject { get; }

        /// <seealso cref="ISynchronizedUserHandComponent.Initialize(ISceneObject)"/>
        public void Initialize(IUserController synchronizedObject);
    }

    /// <remarks>
    /// History:
    /// 8 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// ISynchronizedUserController
	///
	/// A <generic> synchronization component of a user hand controller in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
    public interface ISynchronizedUserController<T> : ISynchronizedUserHandComponent<T>, ISynchronizedUserController
        where T : IUserController
    {
    }
}
