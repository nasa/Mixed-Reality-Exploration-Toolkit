// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using GOV.NASA.GSFC.XR.MRET.Annotation;

namespace GOV.NASA.GSFC.XR.MRET.Collaboration
{
    /// <remarks>
    /// History:
    /// 28 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// ISynchronizedAnnotation
	///
	/// A synchronization component of an annotation in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface ISynchronizedAnnotation : ISynchronized
    {
        /// <seealso cref="ISynchronized.SynchronizedObject"/>
        new public IAnnotation SynchronizedObject { get; }

        /// <seealso cref="ISynchronized.Initialize(IIdentifiable)"/>
        public void Initialize(IAnnotation synchronizedObject);
    }

    /// <remarks>
    /// History:
    /// 28 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// ISynchronizedAnnotation
	///
	/// A <generic> synchronization component of an annotation in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
    public interface ISynchronizedAnnotation<T> : ISynchronized<T>, ISynchronizedAnnotation
        where T : IAnnotation
    {
    }
}
