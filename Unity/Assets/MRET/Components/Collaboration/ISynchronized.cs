// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;

namespace GOV.NASA.GSFC.XR.MRET.Collaboration
{
    /// <remarks>
    /// History:
    /// 6 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// ISynchronized
	///
	/// An synchronized object in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface ISynchronized
	{
        /// <summary>
        /// Indicates if auto-synchronizing is enabled
        /// </summary>
        public bool AutoSynchronize { get; set; }

        /// <summary>
        /// Semaphore for the synchronizing the updates to the controlled object
        /// </summary>
        public Object Lock { get; }

        /// <summary>
        /// Indicates if synchronization of the synchronized object is enabled
        /// </summary>
        public bool SynchronizeEnabled { get; set; }

        /// <summary>
        /// The synchronized object associated with this synchronize instance
        /// </summary>
        public IIdentifiable SynchronizedObject { get; }

        /// <summary>
        /// Idicates if the synchronization is paused.
        /// </summary>
        public bool Paused { get; }

        /// <summary>
        /// Called to initialize this component with the synchronized object
        /// </summary>
        /// <param name="synchronizedObject"></param>
        public void Initialize(IIdentifiable synchronizedObject);

        /// <summary>
        /// Pauses the synchronization.
        /// </summary>
        public void Pause();

        /// <summary>
        /// Resumes the synchronization.
        /// </summary>
        public void Resume();
    }

    /// <remarks>
    /// History:
    /// 6 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// ISynchronized
	///
	/// A <generic> synchronized object in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
    public interface ISynchronized<T> : ISynchronized
        where T : IIdentifiable
    {
        /// <seealso cref="ISynchronized.SynchronizedObject"/>
        new public T SynchronizedObject { get; }

        /// <seealso cref="ISynchronized.Initialize(IIdentifiable)"/>
        public void Initialize(T synchronizedObject);
    }
}
