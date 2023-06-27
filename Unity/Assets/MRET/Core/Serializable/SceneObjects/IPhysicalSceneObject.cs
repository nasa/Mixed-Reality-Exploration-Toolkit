// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.Configuration;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects
{
    /// <remarks>
    /// History:
    /// 1 Oct 2021: Created
    /// </remarks>
	/// 
	/// <summary>
	/// IPhysicalSceneObject
	///
	/// Represents a physical scene object in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IPhysicalSceneObject : IInteractable
	{
        /// <seealso cref="IInteractable.CreateSerializedType"/>
        new public PhysicalSceneObjectType CreateSerializedType();

        /// <summary>
        /// The <code>Rigidbody</code> associated with the physical scene object
        /// </summary>
        public Rigidbody Rigidbody { get; }

        /// <summary>
        /// The mass of the physical scene object
        /// </summary>
        public float Mass { get; set; }

        /// <summary>
        /// The angular drag of the physical scene object
        /// </summary>
        public float AngularDrag { get; set; }

        /// <summary>
        /// The drag of the physical scene object
        /// </summary>
        public float Drag { get; set; }

        /// <summary>
        /// Whether or not collisions are enabled for the physical scene object
        /// </summary>
        public bool EnableCollisions { get; set; }

        /// <summary>
        /// Whether or not gravity is enabled for the physical scene object. If
        /// <code>EnableCollisions</code> is disabled (kinematic), this setting may
        /// be ignored.
        /// </summary>
        public bool EnableGravity { get; set; }

        /// <summary>
        /// The physical specifications for this physical scene object
        /// </summary>
        /// <see cref="PhysicalSpecificationsType"/>
        public PhysicalSpecifications Specifications { get; }

        /// <summary>
        /// Indicates whether randomized textures are applied to the physical scene object
        /// </summary>
        public bool RandomizeTextures { get; set; }

        /// <summary>
        /// Indicates whether the physical scene object is exploding
        /// </summary>
        public bool Exploding { get; }

        /// <summary>
        /// Indicates exploding mode.
        /// </summary>
        public Preferences.ExplodeMode ExplodeMode { get; set; }

        /// <summary>
        /// The scale multiplier when exploding.
        /// </summary>
        public float ExplodeScale { get; set; }

        /// <summary>
        /// The current explode factor.
        /// </summary>
        public float ExplodeFactor { get; }

        /// <summary>
        /// Called to initialize the exploding. This must be called before <code>Explode</code>
        /// or <code>Unexplode</code>.
        /// </summary>
        /// <see cref="Explode"/>
        /// <see cref="Unexplode"/>
        /// <see cref="StopExplode"/>
        public void StartExplode();

        /// <summary>
        /// Called to explode the children in the expanding direction
        /// </summary>
        public void Explode();

        /// <summary>
        /// Called to explode the children in the collapsing direction
        /// </summary>
        public void Unexplode();

        /// <summary>
        /// Called to stop the exploding.
        /// </summary>
        /// <see cref="StartExplode"/>
        public void StopExplode();

        /// <seealso cref="IInteractable.Synchronize(InteractableSceneObjectType, Action{bool, string})"/>
        public void Synchronize(PhysicalSceneObjectType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="IInteractable.Deserialize(InteractableSceneObjectType, Action{bool, string})"/>
        public void Deserialize(PhysicalSceneObjectType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="IInteractable.Serialize(InteractableSceneObjectType, Action{bool, string})"/>
        public void Serialize(PhysicalSceneObjectType serialized, Action<bool, string> onFinished = null);
    }

    /// <remarks>
    /// History:
    /// 1 Oct 2021: Created
    /// </remarks>
	/// 
	/// <summary>
	/// IPhysicalSceneObject
	///
	/// Represents a physical scene object in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IPhysicalSceneObject<T> : IInteractable<T>, IPhysicalSceneObject
        where T : PhysicalSceneObjectType
    {
    }
}
