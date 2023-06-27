// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.Configuration;
using GOV.NASA.GSFC.XR.MRET.Project;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Note
{
    /// <remarks>
    /// History:
    /// 13 Sep 2021: Created (Jeffrey Hosler)
    /// </remarks>
	///
	/// <summary>
    /// InteractableNoteGroup
    /// 
	/// Interactable Note Group in MRET<br>
	///
    /// Author: Jeffrey Hosler
	/// </summary>
    /// 
    /// <seealso cref="InteractableNote"/>
    /// 
    public class InteractableNoteGroup : Group<NotesType, InteractableNoteGroup, InteractableNoteGroup, NoteType, InteractableNote, InteractableNote>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(InteractableNoteGroup);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private NotesType serializedNoteGroup;

        #region Serializable
        /// <seealso cref="Group{GT, GC, GI, CT, CC, CI}.Deserialize(GT, SerializationState)"/>
        protected override IEnumerator Deserialize(NotesType serialized, SerializationState deserializationState)
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
            serializedNoteGroup = serialized;

            // Record the deserialization state as complete
            deserializationState.complete = true;

            yield return null;
        }

        /// <seealso cref="Group{GT, GC, GI, CT, CC, CI}.Serialize(GT, SerializationState)"/>
        protected override IEnumerator Serialize(NotesType serialized, SerializationState serializationState)
        {
            StartCoroutine(base.Serialize(serialized, serializationState));

            // Wait for the coroutine to complete
            while (!serializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) yield break;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Save the final serialized reference
            serializedNoteGroup = serialized;

            // Record the deserialization state as complete
            serializationState.complete = true;

            yield return null;
        }

        /// <seealso cref="Group{GT, GC, GI, CT, CC, CI}.InstantiateSerializable(CT, GameObject, Transform, Action{CC})"/>
        protected override void InstantiateSerializable(NoteType serializedChild, GameObject go, Transform parent, Action<InteractableNote> onFinished = null)
        {
            // The note gameobject will be created from a prefab
            ProjectManager.NoteManager.InstantiateNote(serializedChild, parent, onFinished);
        }

        /// <seealso cref="Group{GT, GC, GI, CT, CC, CI}.InstantiateSerializable(GT, GameObject, Transform, Action{GC})"/>
        protected override void InstantiateSerializable(NotesType serializedChildGroup, GameObject go, Transform parent, Action<InteractableNoteGroup> onFinished = null)
        {
            ProjectManager.NoteManager.InstantiateNotes(serializedChildGroup, go, parent, onFinished);
        }

        /// <seealso cref="Group{GT, GC, GI, CT, CC, CI}.ReadSerializedItems(GT)"/>
        protected override VersionedType[] ReadSerializedItems(NotesType serializedGroup) => serializedGroup.Items;

        /// <seealso cref="Group{GT, GC, GI, CT, CC, CI}.WriteSerializedItems(GT, VersionedType[])"/>
        protected override void WriteSerializedItems(NotesType serializedGroup, VersionedType[] serializedItems)
        {
            serializedGroup.Items = serializedItems;
        }
        #endregion Serializable
    }
}
