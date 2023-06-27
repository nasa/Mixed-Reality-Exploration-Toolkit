// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET;

namespace GOV.NASA.GSFC.XR.MRET.Project
{
    /// <remarks>
    /// History:
    /// 23 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// IProjectContent
	///
	/// The project content in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IProjectContent : IXRObject
    {
        /// <summary>
        /// The scene objects container for the project
        /// </summary>
        public GameObject SceneObjects { get; }

        /// <summary>
        /// The parts container for the project
        /// </summary>
        /// <seealso cref="InteractablePartGroup"/>
        public GameObject Parts { get; }

        /// <summary>
        /// The notes container for the project
        /// </summary>
        /// <seealso cref="InteractableNoteGroup"/>
        public GameObject Notes { get; }

        /// <summary>
        /// The 3D drawings container for the project (excludes note/2d drawings)
        /// </summary>
        public GameObject Drawings { get; }

        /// <summary>
        /// The markers container for the project
        /// </summary>
        public GameObject Markers { get; }

        /// <summary>
        /// The point cloud container for the project
        /// </summary>
        public GameObject PointClouds { get; }

        /// <summary>
        /// The IoTThings container for the project
        /// </summary>
        public GameObject IoTThings { get; }

        /// <summary>
        /// Obtains the root container of the supplied project game object.
        /// </summary>
        /// <param name="projectObject">The <code>GameObject</code> to query</param>
        /// <returns>The root <code>GameObject</code> containing the supplied project game
        ///     object, or null if not found</returns>
        public GameObject GetRootContainer(GameObject projectObject);

        /// <seealso cref="IVersioned.CreateSerializedType"/>
        new public ContentType CreateSerializedType();

        /// <seealso cref="IVersioned.Deserialize(VersionedType, Action{bool, string})"/>
        public void Deserialize(ContentType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="IVersioned.Serialize(VersionedType, Action{bool, string})"/>
        public void Serialize(ContentType serialized, Action<bool, string> onFinished = null);

    }

    /// <remarks>
    /// History:
    /// 23 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// IProjectContent
	///
	/// The <generic> project content in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IProjectContent<T> : IXRObject<T>, IProjectContent
        where T : ContentType
    {
    }
}
