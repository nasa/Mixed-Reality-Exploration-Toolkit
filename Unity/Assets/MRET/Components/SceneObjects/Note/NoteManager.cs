// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Action;
using GOV.NASA.GSFC.XR.MRET.Data;
using GOV.NASA.GSFC.XR.MRET.Project;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Note
{
    public class NoteManager : MRETSerializableManager<NoteManager>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(NoteManager);

        public static readonly string notesKey = "MRET.INTERNAL.TOOLS.NOTES";

        public bool creatingNotes
        {
            get
            {
                return (bool)MRET.DataManager.FindPoint(notesKey);
            }
            set
            {
                MRET.DataManager.SaveValue(new DataManager.DataValue(notesKey, value));
            }
        }

        public GameObject notePrefab;

        /// <seealso cref="MRETManager{M}.Initialize"/>
        public override void Initialize()
        {
            // Take the inherited behavior
            base.Initialize();

            creatingNotes = false;
        }

        #region Serializable Instantiation
        /// <seealso cref="MRETSerializableManager{M}.GetDefaultSerializableContainer{T}(T)"/>
        protected override Transform GetDefaultSerializableContainer<T>(T serialized)
        {
            return ProjectManager.NotesContainer.transform;
        }

        /// <summary>
        /// Instantiates the note from the supplied serialized note.
        /// </summary>
        /// <param name="serializedNote">The <code>NoteType</code> class instance
        ///     containing the serialized representation of the note to instantiate</param>
        /// <param name="container">The parent container <code>Transform</code> for the instantiated
        ///     note. If null, the project notes container will be used.</param>
        /// <param name="onLoaded">The optional <code>Action</code> to be asynchronously triggered on
        ///     completion</param>
        /// <param name="finishNoteInstantiation">The optional
        ///     <code>FinishSerializableInstantiationDelegate</code> method to be called to finish the
        ///     note instantiation. Called before the onLoaded action is called. If not specified, a
        ///     default logging behavior will be used.</param>
        /// <param name="context">Optional context parameters to be supplied to the
        ///     finishNoteInstantiation method to provide additional context</param>
        public void InstantiateNote(NoteType serializedNote,
            Transform container = null, Action<InteractableNote> onLoaded = null,
            FinishSerializableInstantiationDelegate<NoteType, InteractableNote> finishNoteInstantiation = null,
            params object[] context)
        {
            if (!notePrefab)
            {
                LogError("Failed to instantiate note because the " + nameof(notePrefab) + " is null",
                    nameof(InstantiateNote));
                return;
            }

            // Create the note from the prefab
            GameObject noteGO = null;
            InteractableNote interactableNote = InteractableNote.Create("Note", notePrefab, container);
            if (interactableNote != null)
            {
                noteGO = interactableNote.gameObject;
            }

            // Instantiate and load the new note
            InstantiateSerializable(serializedNote, noteGO, container, onLoaded,
                finishNoteInstantiation, context);
        }

        /// <summary>
        /// Instantiates the note group from the supplied serialized note group.
        /// </summary>
        /// <param name="serializedNotes">The <code>NotesType</code> class instance
        ///     containing the serialized representation of the note group to instantiate.</param>
        /// <param name="go">The optional <code>GameObject</code> that will contain the
        ///     instantiated note group. If null, one will be created.</param>
        /// <param name="container">The parent container <code>Transform</code> for the instantiated
        ///     note. If null, the default project notes container will be used.</param>
        /// <param name="onLoaded">The optional <code>Action</code> to be asynchronously triggered on
        ///     completion.</param>
        public void InstantiateNotes(NotesType serializedNotes, GameObject go = null,
            Transform container = null, Action<InteractableNoteGroup> onLoaded = null,
            FinishSerializableInstantiationDelegate<NotesType, InteractableNoteGroup> finishNotesInstantiation = null,
            params object[] context)
        {
            // Instantiate and deserialize
            InstantiateSerializable(serializedNotes, go, container, onLoaded,
                finishNotesInstantiation, context);
        }

        /// <summary>
        /// Creates an interactable note.
        /// </summary>
        /// <param name="noteName">Name of the note</param>
        /// <param name="position">World space position of the note</param>
        /// <param name="rotation">World space rotation of the note</param>
        /// <returns>A <code>InteractableNote</code> instance.</returns>
        /// <see cref="InteractableNote"/>
        public InteractableNote CreateNote(string noteName,
            Vector3 position, Quaternion rotation)
        {
            InteractableNote newNote = null;

            if (creatingNotes)
            {
                newNote = InteractableNote.Create(noteName, notePrefab);

                // Additional settings if valid reference
                if (newNote != null)
                {
                    // Transform
                    newNote.transform.position = position;
                    newNote.transform.rotation = rotation;

                    // Set the grab behavior
                    newNote.GrabBehavior = ProjectManager.SceneObjectManager.GrabBehavior;

                    // Start maximized
                    newNote.Maximize();

                    // Record the action
                    var serializedNote = newNote.CreateSerializedType();
                    newNote.Serialize(serializedNote);
                    ProjectManager.UndoManager.AddAction(
                        new AddSceneObjectAction(serializedNote),
                        new DeleteIdentifiableObjectAction(newNote.id));
                }
            }

            return newNote;
        }
        #endregion Serializable Instantiation

    }
}