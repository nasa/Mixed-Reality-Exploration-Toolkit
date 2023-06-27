// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.Configuration;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects
{
    /// <remarks>
    /// History:
    /// 13 Sep 2021: Created (Jeffrey Hosler)
    /// </remarks>
	///
	/// <summary>
    /// SceneObjectGroup
    /// 
	/// Scene Object Group in MRET<br>
	///
    /// Author: Jeffrey Hosler
	/// </summary>
    /// 
    /// <seealso cref="ISceneObjectGroup"/>
    /// 
    public abstract class SceneObjectGroup<GT, GC, GI, CT, CC, CI> : Group<GT, GC, GI, CT, CC, CI>, ISceneObjectGroup<GT, GI, CT, CI>
        where GT : SceneObjectsType, new()
        where GC : SceneObjectGroup<GT, GC, GI, CT, CC, CI>, new()
        where GI : ISceneObjectGroup
        where CT : SceneObjectType, new()
        where CC : SceneObject<CT>, new()
        where CI : ISceneObject
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(SceneObjectGroup<GT, GC, GI, CT, CC, CI>);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private GT serializedSceneObjectsGroup;

        #region ISceneObjectGroup
        /// <seealso cref="ISceneObjectGroup.CreateSerializedType"/>
        SceneObjectsType ISceneObjectGroup.CreateSerializedType() => CreateSerializedType();

        /// <seealso cref="ISceneObjectGroup.Deserialize(SceneObjectsType, Action{bool, string})"/>
        void ISceneObjectGroup.Deserialize(SceneObjectsType serialized, Action<bool, string> onFinished)
        {
            Deserialize(serialized as GT, onFinished);
        }

        /// <seealso cref="ISceneObjectGroup.Serialize(SceneObjectsType, Action{bool, string})"/>
        void ISceneObjectGroup.Serialize(SceneObjectsType serialized, Action<bool, string> onFinished)
        {
            Serialize(serialized as GT, onFinished);
        }
        #endregion ISceneObjectGroup

        #region Serialization
        /// <seealso cref="Group{GT, GC, GI, CT, CC, CI}.ReadSerializedItems(GT)"/>
        protected override VersionedType[] ReadSerializedItems(GT serializedGroup) => serializedGroup.Items;

        /// <seealso cref="Group{GT, GC, GI, CT, CC, CI}.WriteSerializedItems(GT, VersionedType[])"/>
        protected override void WriteSerializedItems(GT serializedGroup, VersionedType[] serializedItems)
        {
            serializedGroup.Items = serializedItems;
        }

        /// <seealso cref="Group{GT, GC, GI, CT, CC, CI}.Deserialize(GT, SerializationState)"/>
        protected override IEnumerator Deserialize(GT serialized, SerializationState deserializationState)
        {
            StartCoroutine(base.Deserialize(serialized, deserializationState));

            // Wait for the coroutine to complete
            while (!deserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (deserializationState.IsError) yield break;

            // Start with an incomplete indicator
            deserializationState.Clear();

            // Process this object specific deserialization

            // Save the serialized reference
            serializedSceneObjectsGroup = serialized;

            // Record the deserialization state as complete
            deserializationState.complete = true;

            yield return null;
        }

        /// <seealso cref="Group{GT, GI, CT, CI}.Serialize(GT, SerializationState)"/>
        protected override IEnumerator Serialize(GT serialized, SerializationState serializationState)
        {
            StartCoroutine(base.Deserialize(serialized, serializationState));

            // Wait for the coroutine to complete
            while (!serializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) yield break;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Save the final serialized reference
            serializedSceneObjectsGroup = serialized;

            // Record the deserialization state as complete
            serializationState.complete = true;

            yield return null;
        }
        #endregion Serialization
    }

    /// <summary>
    /// Provides an implementation for the abstract SceneObject class
    /// </summary>
    public class SceneObjectGroup : SceneObjectGroup<SceneObjectsType, SceneObjectGroup, ISceneObjectGroup, SceneObjectType, SceneObject, ISceneObject>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(SceneObjectGroup);
    }

}
