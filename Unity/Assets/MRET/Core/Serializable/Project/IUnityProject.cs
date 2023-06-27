// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Time;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.User;

namespace GOV.NASA.GSFC.XR.MRET.Project
{
    /// <remarks>
    /// History:
    /// 1 Oct 2021: Created
    /// </remarks>
	///
	/// <summary>
	/// IUnityProject
	///
	/// A Unity project description in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IUnityProject : IVersioned
	{
        /// <summary>
        /// The project description
        /// </summary>
        public string description { get; set; }

        /// <summary>
        /// Whether or not changes have been made to the project since last cleared.
        /// </summary>
        public bool Changed { get; }

        /// <summary>
        /// The scale multiplier for the contents of the project
        /// </summary>
        public float ScaleMultiplier { get; set; }

        /// <summary>
        /// The project environment
        /// </summary>
        /// <seealso cref="IEnvironment"/>
        public IEnvironment Environment { get; }

        /// <summary>
        /// The user.
        /// </summary>
        /// <seealso cref="IUser"/>
        public IUser User { get; }

        /// <summary>
        /// The time simulation for the project
        /// </summary>
        /// <seealso cref="IEnvironment"/>
        public ITimeSimulation TimeSimulation { get; }

        /// <summary>
        /// The project content
        /// </summary>
        /// <seealso cref="IProjectContent"/>
        public IProjectContent Content { get; }

        /// <summary>
        /// The 3rd party interfaces for this project
        /// </summary>
        public GameObject Interfaces { get; }

        /// <seealso cref="IVersioned.CreateSerializedType"/>
        new public ProjectType CreateSerializedType();

        /// <seealso cref="IVersioned.Deserialize(VersionedType, Action{bool, string})"/>
        public void Deserialize(ProjectType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="IVersioned.Serialize(VersionedType, Action{bool, string})"/>
        public void Serialize(ProjectType serialized, Action<bool, string> onFinished = null);
    }

    /// <remarks>
    /// History:
    /// 1 Oct 2021: Created
    /// </remarks>
	///
	/// <summary>
	/// IUnityProject
	///
	/// A <generic> Unity project description in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IUnityProject<T> : IVersioned<T>, IUnityProject
        where T : ProjectType
    {
    }
}
