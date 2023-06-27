// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Configuration;
using GOV.NASA.GSFC.XR.MRET.Interfaces;
using GOV.NASA.GSFC.XR.MRET.SceneObjects;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Marker;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Note;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Part;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.PointCloud;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.Integrations.IoT;

namespace GOV.NASA.GSFC.XR.MRET.Project
{
    /// <remarks>
    /// History:
    /// 28 Sep 2021: Created (Jeffrey Hosler)
    /// </remarks>
    ///
    /// <summary>
    /// The MRET project content.<br>
    ///
    /// Author: Jeffrey Hosler
    /// </summary>
    /// 
	public class ProjectContent : XRObject<ContentType>, IProjectContent<ContentType>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(ProjectContent);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private ContentType serializedContent;

        #region IProjectContent
        /// <seealso cref="IProjectContent.SceneObjects"/>
        public GameObject SceneObjects { get => (_sceneObjects != null) ? _sceneObjects.gameObject : null; }
        private SceneObjectGroup _sceneObjects;

        /// <seealso cref="IProjectContent.Parts"/>
        public GameObject Parts { get => (_parts != null) ? _parts.gameObject : null; }
        private InteractablePartGroup _parts;

        /// <seealso cref="IProjectContent.Notes"/>
        public GameObject Notes { get => (_notes != null) ? _notes.gameObject : null; }
        private InteractableNoteGroup _notes;

        /// <seealso cref="IProjectContent.Drawings"/>
        public GameObject Drawings { get => _drawings; }
        private GameObject _drawings;

        /// <seealso cref="IProjectContent.Markers"/>
        public GameObject Markers { get => _markers; }
        private GameObject _markers;

        /// <seealso cref="IProjectContent.PointClouds"/>
        public GameObject PointClouds { get => (_pointClouds != null) ? _pointClouds.gameObject : null; }
        private PointCloudGroup _pointClouds;

        /// <seealso cref="IProjectContent.IoTThings"/>
        public GameObject IoTThings { get => _iotThings; }
        private GameObject _iotThings;

        /// <seealso cref="IProjectContent.GetRootContainer(GameObject)"/>
        public GameObject GetRootContainer(GameObject projectObject)
        {
            GameObject result = null;

            if (projectObject != null)
            {
                // Determine with of the project content containers contain the supplied project object
                if (projectObject.transform.IsChildOf(SceneObjects.transform))
                {
                    result = SceneObjects;
                }
                else if (projectObject.transform.IsChildOf(Parts.transform))
                {
                    result = Parts;
                }
                else if (projectObject.transform.IsChildOf(Notes.transform))
                {
                    result = Notes;
                }
                else if (projectObject.transform.IsChildOf(Drawings.transform))
                {
                    result = Drawings;
                }
                else if (projectObject.transform.IsChildOf(Markers.transform))
                {
                    result = Markers;
                }
                else if (projectObject.transform.IsChildOf(IoTThings.transform))
                {
                    result = IoTThings;
                }
            }
            else
            {
                LogWarning("Supplied project object is null", nameof(GetRootContainer));
            }

            return result;
        }
        #endregion IProjectContent

        #region MRETUpdateBehaviour
        /// <seealso cref="MRETBehaviour.IntegrityCheck"/>
        protected override IntegrityState IntegrityCheck()
        {
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            return (
                (state == IntegrityState.Failure) // TODO: || (MyRequiredRef == null)

                    ? IntegrityState.Failure   // Fail is base class fails or anything is null
                    : IntegrityState.Success); // Otherwise, our integrity is valid
        }

        /// <seealso cref="MRETBehaviour.IntegrityCheck"/>
        protected override void MRETAwake()
        {
            base.MRETAwake();

            // Create our internal game objects

            // Scene Objects
            GameObject sceneObjectsGO = new GameObject("SceneObjects");
            sceneObjectsGO.transform.parent = gameObject.transform;
            sceneObjectsGO.transform.localPosition = Vector3.zero;
            sceneObjectsGO.transform.localRotation = Quaternion.identity;
            sceneObjectsGO.transform.localScale = Vector3.one;
            _sceneObjects = sceneObjectsGO.AddComponent<SceneObjectGroup>();

            // Parts
            GameObject partsGO = new GameObject("Parts");
            partsGO.transform.parent = gameObject.transform;
            partsGO.transform.localPosition = Vector3.zero;
            partsGO.transform.localRotation = Quaternion.identity;
            partsGO.transform.localScale = Vector3.one;
            _parts = partsGO.AddComponent<InteractablePartGroup>();

            // Notes
            GameObject notesGO = new GameObject("Notes");
            notesGO.transform.parent = gameObject.transform;
            notesGO.transform.localPosition = Vector3.zero;
            notesGO.transform.localRotation = Quaternion.identity;
            notesGO.transform.localScale = Vector3.one;
            _notes = notesGO.AddComponent<InteractableNoteGroup>();

            // Drawings
            _drawings = new GameObject("Drawings");
            _drawings.transform.parent = gameObject.transform;
            _drawings.transform.localPosition = Vector3.zero;
            _drawings.transform.localRotation = Quaternion.identity;
            _drawings.transform.localScale = Vector3.one;

            // Markers
            _markers = new GameObject("Markers");
            _markers.transform.parent = gameObject.transform;
            _markers.transform.localPosition = Vector3.zero;
            _markers.transform.localRotation = Quaternion.identity;
            _markers.transform.localScale = Vector3.one;

            // PointClouds
            GameObject pointCloudsGO = new GameObject("PointClouds");
            pointCloudsGO.transform.parent = gameObject.transform;
            pointCloudsGO.transform.localPosition = Vector3.zero;
            pointCloudsGO.transform.localRotation = Quaternion.identity;
            pointCloudsGO.transform.localScale = Vector3.one;
            _pointClouds = pointCloudsGO.AddComponent<PointCloudGroup>();

            // IoTThings
            _iotThings = new GameObject("IoTThings");
            _iotThings.transform.parent = gameObject.transform;
            _iotThings.transform.localPosition = Vector3.zero;
            _iotThings.transform.localRotation = Quaternion.identity;
            _iotThings.transform.localScale = Vector3.one;
        }

        /// <seealso cref="MRETBehaviour.MRETOnDestroy"/>
        protected override void MRETOnDestroy()
        {
            // Take the inherited behavior
            base.MRETOnDestroy();

            Destroy(_sceneObjects);
            Destroy(_parts);
            Destroy(_notes);
            Destroy(_drawings);
            Destroy(_markers);
            Destroy(_pointClouds);
            Destroy(_iotThings);
        }
        #endregion MRETUpdateBehaviour

        #region Serializable
        protected virtual IEnumerator DeserializeDrawings(Drawing3dType[] serializedDrawings, SerializationState deserializationState)
        {
            Action<IInteractable3dDrawing[]> OnDrawingsLoadedAction = (IInteractable3dDrawing[] loadedDrawings) =>
            {
                if ((loadedDrawings == null) || (loadedDrawings.Length == 0))
                {
                    // Record the error
                    deserializationState.Error("A problem was encountered instantiating the drawings");
                }

                // Mark as complete
                deserializationState.complete = true;
            };

            // Instantiate and deserialize the drawings
            ProjectManager.DrawingManager.InstantiateDrawings(serializedDrawings,
                null, _drawings.transform, OnDrawingsLoadedAction);

            // Wait for completion
            while (!deserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the deserialization failed, there's no point in continuing
            if (deserializationState.IsError) yield break;

            yield return null;
        }

        protected virtual IEnumerator DeserializeMarkers(MarkersType serializedMarkers, SerializationState deserializationState)
        {
            Action<InteractableMarker[]> OnMarkersLoadedAction = (InteractableMarker[] loadedMarkers) =>
            {
                if ((loadedMarkers == null) || (loadedMarkers.Length == 0))
                {
                    // Record the error
                    deserializationState.Error("A problem was encountered instantiating the markers");
                }

                // Mark as complete
                deserializationState.complete = true;
            };

            // Instantiate and deserialize the markers and paths
            ProjectManager.MarkerManager.InstantiateMarkersAndPaths(serializedMarkers,
                null, _markers.transform, OnMarkersLoadedAction);

            // Wait for completion
            while (!deserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the deserialization failed, there's no point in continuing
            if (deserializationState.IsError) yield break;

            yield return null;
        }

        /// <seealso cref="VersionedMRETBehaviour{T}.Deserialize(T, SerializationState))"/>
        protected override IEnumerator Deserialize(ContentType serialized, SerializationState deserializationState)
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
            serializedContent = serialized;

            // Perform the deserialization

            // Destroy the old scene object group contents
            _sceneObjects.DestroyGroup();

            // Scene Objects (optional)
            if (serializedContent.SceneObjects != null)
            {
                // Deserialize the scene objects
                VersionedSerializationState<SceneObjectGroup> sceneObjectsDeserializationState = new VersionedSerializationState<SceneObjectGroup>();
                StartCoroutine(DeserializeVersioned(serializedContent.SceneObjects,
                    SceneObjects, gameObject.transform, sceneObjectsDeserializationState));

                // Wait for the coroutine to complete
                while (!sceneObjectsDeserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // Record the deserialization state
                deserializationState.Update(sceneObjectsDeserializationState);

                // Make sure the resultant scene objects type is not null
                if (sceneObjectsDeserializationState.versioned is null)
                {
                    deserializationState.Error("Deserialized scene object group cannot be null, denoting a possible internal issue.");
                }

                // If the scene objects deserialization failed, there's no point in continuing. Something is wrong
                if (deserializationState.IsError) yield break;

                // Assign the scene objects
                _sceneObjects = sceneObjectsDeserializationState.versioned;

                // Clear the state
                deserializationState.Clear();
            }

            // Destroy the old part group contents
            if (_parts != null) _parts.DestroyGroup();

            // Parts (optional)
            if (serializedContent.Parts != null)
            {
                SerializationState partsDeserializationState = new SerializationState();
                Action<InteractablePartGroup> OnPartsLoadedAction = (InteractablePartGroup loadedGroup) =>
                {
                    if (loadedGroup != null)
                    {
                        // Assign the parts
                        _parts = loadedGroup;

                        // Mark as complete
                        partsDeserializationState.complete = true;
                    }
                    else
                    {
                        // Record the error
                        partsDeserializationState.Error("A problem was encountered deserializing the parts");
                    }
                };

                // Load the parts
                ProjectManager.PartManager.InstantiateParts(serializedContent.Parts,
                    Parts, gameObject.transform, OnPartsLoadedAction);

                // Wait for the coroutine to complete
                while (!partsDeserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // Record the deserialization state
                deserializationState.Update(partsDeserializationState);

                // If the deserialization failed, there's no point in continuing. Something is wrong
                if (deserializationState.IsError) yield break;

                // Clear the state
                deserializationState.Clear();
            }

            // Destroy the old displays
            if (_notes != null) _notes.DestroyGroup();

            // Displays (optional)
            if (serializedContent.Displays != null)
            {
                if (serializedContent.Displays.Notes != null)
                {
                    SerializationState notesDeserializationState = new SerializationState();
                    Action<InteractableNoteGroup> OnNotesLoadedAction = (InteractableNoteGroup loadedGroup) =>
                    {
                        if (loadedGroup != null)
                        {
                            // Assign the notes
                            _notes = loadedGroup;

                            // Mark as complete
                            notesDeserializationState.complete = true;
                        }
                        else
                        {
                            // Record the error
                            notesDeserializationState.Error("A problem was encountered deserializing the notes");
                        }
                    };

                    // Load the notes
                    ProjectManager.NoteManager.InstantiateNotes(serializedContent.Displays.Notes,
                        Notes, gameObject.transform, OnNotesLoadedAction);

                    // Wait for the coroutine to complete
                    while (!notesDeserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                    // Record the deserialization state
                    deserializationState.Update(notesDeserializationState);

                    // If the deserialization failed, there's no point in continuing. Something is wrong
                    if (deserializationState.IsError) yield break;

                    // Clear the state
                    deserializationState.Clear();
                }

                // TODO: HUD

                // TODO: Panels
            }

            // Destroy the old drawings
            if (_drawings != null)
            {
                IInteractable3dDrawing[] drawings = _drawings.GetComponentsInChildren<IInteractable3dDrawing>();
                foreach (IInteractable3dDrawing drawing in drawings)
                {
                    Destroy(drawing.gameObject);
                }
            }

            // Drawings (optional)
            if ((serializedContent.Drawings != null) && (serializedContent.Drawings.Length > 0))
            {
                // Deserialize the drawings
                SerializationState drawingsDeserializationState = new SerializationState();
                StartCoroutine(DeserializeDrawings(serializedContent.Drawings, drawingsDeserializationState));

                // Wait for the coroutine to complete
                while (!drawingsDeserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // Record the deserialization state
                deserializationState.Update(drawingsDeserializationState);

                // If the deserialization failed, there's no point in continuing. Something is wrong
                if (deserializationState.IsError) yield break;

                // Clear the state
                deserializationState.Clear();
            }

            // Destroy the old markers
            if (_markers != null)
            {
                InteractableMarker[] markers = _markers.GetComponentsInChildren<InteractableMarker>();
                foreach (InteractableMarker marker in markers)
                {
                    Destroy(marker.gameObject);
                }
            }

            // Markers (optional)
            if (serializedContent.Markers != null)
            {
                // Deserialize the markers
                SerializationState markersDeserializationState = new SerializationState();
                StartCoroutine(DeserializeMarkers(serializedContent.Markers, markersDeserializationState));

                // Wait for the coroutine to complete
                while (!markersDeserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // Record the deserialization state
                deserializationState.Update(markersDeserializationState);

                // If the deserialization failed, there's no point in continuing. Something is wrong
                if (deserializationState.IsError) yield break;

                // Clear the state
                deserializationState.Clear();
            }

            // Feeds (optional)
            if (serializedContent.Feeds != null)
            {
                // TODO: ...
            }

            // ActionSequences/Animations (optional)
            if (serializedContent.ActionSequences != null)
            {
                // TODO: ...
                /*
                AnimationPanelsType panels = (AnimationPanelsType)serializedProject.Items[i];

                if (panels != null)
                {
                    if (panels.AnimationPanels != null)
                    {
                        foreach (AnimationPanelType panel in panels.AnimationPanels)
                        {
                            GameObject instantiatedPanel = Instantiate(animationPanelPrefab);
                            instantiatedPanel.transform.position = new Vector3(panel.Position.X, panel.Position.Y, panel.Position.Z);
                            instantiatedPanel.transform.rotation = new Quaternion(panel.Rotation.X, panel.Rotation.Y, panel.Rotation.Z, panel.Rotation.W);

                            AnimationMenuController panelController = instantiatedPanel.GetComponentInChildren<AnimationMenuController>(true);
                            if (panelController)
                            {
                                //MRETAnimationGroup anim = MRETAnimationGroup.Deserialize(panel.Animation);
                                //panelController.loopToggle.isOn = anim.LoopCount > 1;
                                //panelController.autoplayToggle.isOn = anim.Autostart;
                                //panelController.SetAnimation(anim);
                            }
                        }
                    }
                }
                 */
            }

            // Destroy the old point cloud group contents
            if (_pointClouds != null) _pointClouds.DestroyGroup();

            // Static PointClouds (optional)
            if (serializedContent.PointClouds != null)
            {
                SerializationState pointCLoudsDeserializationState = new SerializationState();
                Action<PointCloudGroup> OnPointCloudsLoadedAction = (PointCloudGroup loadedGroup) =>
                {
                    if (loadedGroup != null)
                    {
                        // Assign the point cloud
                        _pointClouds = loadedGroup;

                        // Mark as complete
                        pointCLoudsDeserializationState.complete = true;
                    }
                    else
                    {
                        // Record the error
                        pointCLoudsDeserializationState.Error("A problem was encountered deserializing the point clouds");
                    }
                };

                // Load the point clouds
                ProjectManager.PointCloudManager.InstantiatePointClouds(serializedContent.PointClouds,
                    PointClouds, gameObject.transform, OnPointCloudsLoadedAction);

                // Wait for the coroutine to complete
                while (!pointCLoudsDeserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // Record the deserialization state
                deserializationState.Update(pointCLoudsDeserializationState);

                // If the deserialization failed, there's no point in continuing. Something is wrong
                if (deserializationState.IsError) yield break;

                // Clear the state
                deserializationState.Clear();
            }

            // Scene Object Generators (optional)
            if (serializedContent.SceneObjectGenerators != null)
            {
                // Load each object generator into the environment.
                foreach (SceneObjectGeneratorType ogt in serializedContent.SceneObjectGenerators)
                {
                    // TODO: Add an action so we can track completion of each generator
                    StartCoroutine(ProjectManager.ProceduralObjectGeneratorManager.AddGenerator(ogt));
                }
            }

            // Terrains (optional)
            if (serializedContent.Terrains != null)
            {
                // TODO:
            }

            // Destroy the old IoTThings
            if (_iotThings != null)
            {
                IIoTThing[] iotThings = _iotThings.GetComponentsInChildren<IIoTThing>();
                foreach (IIoTThing iotThing in iotThings)
                {
                    Destroy(iotThing.gameObject);
                }
            }

            // IoT Things (optional)
            if ((serializedContent.IoTThings != null) && (serializedContent.IoTThings.Length > 0))
            {
                SerializationState iotThingsDeserializationState = new SerializationState();
                Action<IIoTThing[]> OnIoTThingsLoadedAction = (IIoTThing[] loadedIoTThings) =>
                {
                    if (loadedIoTThings == null)
                    {
                        // Record the error
                        deserializationState.Error("A problem was encountered deserializing the IoTThings");
                    }

                    // Mark as complete
                    iotThingsDeserializationState.complete = true;
                };

                // Load the IoTThings
                ThirdPartyInterfaceManager.IoTManager.InstantiateIoTThings(serializedContent.IoTThings, null,
                    IoTThings.transform, OnIoTThingsLoadedAction);

                // Wait for the loading to complete
                while (!iotThingsDeserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // Record the deserialization state
                deserializationState.Update(iotThingsDeserializationState);

                // If the deserialization failed, there's no point in continuing. Something is wrong
                if (deserializationState.IsError) yield break;

                // Clear the state
                deserializationState.Clear();
            }

            // Record the deserialization state as complete
            deserializationState.complete = true;

            yield return null;
        }

        /* Old serialization
        public ProjectType Serialize()
        {
            List<object> serializationItems = new List<object>();
            List<ItemsChoiceType3> serializationItemsName = new List<ItemsChoiceType3>();

            // Save Current View.
            if (loadedProject_currentView != null)
            {
                serializationItems.Add(loadedProject_currentView);
                serializationItemsName.Add(ItemsChoiceType3.CurrentView);
            }

            // Save Description.
            if (loadedProject_description != null)
            {
                serializationItems.Add(loadedProject_description);
                serializationItemsName.Add(ItemsChoiceType3.Description);
            }

            // Save Environment.
            if (loadedProject_environment != null)
            {
                serializationItems.Add(loadedProject_environment);
                serializationItemsName.Add(ItemsChoiceType3.Environment);
            }

            // Save Parts.
            if (projectObjectContainer != null)
            {
                List<PartType> partList = new List<PartType>();
                List<int> idList = new List<int>();
                int i = 0;
                Transform topLevelContainer = projectObjectContainer.transform;
                foreach (InteractablePart part in projectObjectContainer.GetComponentsInChildren<InteractablePart>())
                {
                    if (part.transform.parent == topLevelContainer)
                    {
                        partList.Add(part.Serialize());
                        idList.Add(++i);
                    }
                }
                PartsType parts = new PartsType();
                parts.Parts = partList.ToArray();
                parts.ID = idList.ToArray();
                serializationItems.Add(parts);
                serializationItemsName.Add(ItemsChoiceType3.Parts);
            }

            // Save Drawings.
            if (projectDrawingContainer != null)
            {
                List<DrawingType> drawingList = new List<DrawingType>();
                List<int> idList = new List<int>();
                int i = 0;
                foreach (MeshLineRenderer projectDrawing in projectDrawingContainer.GetComponentsInChildren<MeshLineRenderer>())
                {
                    drawingList.Add(projectDrawing.drawingScript.Serialize());
                    idList.Add(++i);
                }
                DrawingsType drawings = new DrawingsType();
                drawings.Drawings = drawingList.ToArray();
                drawings.ID = idList.ToArray();
                serializationItems.Add(drawings);
                serializationItemsName.Add(ItemsChoiceType3.Drawings);
            }

            // Save GMSEC Sources.
            if (loadedProject_gmsecSources != null)
            {
                List<GMSECSourceType> gmsecSourceList = new List<GMSECSourceType>();
                List<int> idList = new List<int>();
                int i = 0;
                foreach (GMSECSourceType gmsecSource in loadedProject_gmsecSources)
                {
                    gmsecSourceList.Add(gmsecSource);
                    idList.Add(++i);
                }
                GMSECSourcesType gmsecSources = new GMSECSourcesType();
                gmsecSources.GMSECSources = gmsecSourceList.ToArray();
                gmsecSources.ID = idList.ToArray();
                serializationItems.Add(gmsecSources);
                serializationItemsName.Add(ItemsChoiceType3.GMSECSources);
            }

            // Save Notes.
            if (projectNoteContainer != null)
            {
                List<NoteType> noteList = new List<NoteType>();
                List<int> idList = new List<int>();
                foreach (NoteDeprecated note in projectNoteContainer.GetComponentsInChildren<NoteDeprecated>())
                {
                    NoteType serializedNote = new NoteType();
                    serializedNote.Transform = new UnityTransformType();
                    serializedNote.Transform.Position = new Vector3Type();
                    serializedNote.Transform.Position.X = note.transform.position.x;
                    serializedNote.Transform.Position.Y = note.transform.position.y;
                    serializedNote.Transform.Position.Z = note.transform.position.z;
                    serializedNote.Transform.Rotation = new QuaternionType();
                    serializedNote.Transform.Rotation.X = note.transform.rotation.x;
                    serializedNote.Transform.Rotation.Y = note.transform.rotation.y;
                    serializedNote.Transform.Rotation.Z = note.transform.rotation.z;
                    serializedNote.Transform.Rotation.W = note.transform.rotation.w;
                    serializedNote.Title = note.titleText.text;
                    serializedNote.Details = note.informationText.text;
                    serializedNote.GUID = note.guid.ToString();

                    Vector3[][] serializedDrawings = note.SerializeDrawings();
                    serializedNote.Drawings = new NoteDrawingsType();
                    serializedNote.Drawings.NoteDrawings = new NoteDrawingType[serializedDrawings.Length];
                    for (int i = 0; i < serializedDrawings.Length; i++)
                    {
                        serializedNote.Drawings.NoteDrawings[i] = new NoteDrawingType();
                        serializedNote.Drawings.NoteDrawings[i].Points = new Vector3Type[serializedDrawings[i].Length];
                        for (int j = 0; j < serializedDrawings[i].Length; j++)
                        {
                            serializedNote.Drawings.NoteDrawings[i].Points[j] = new Vector3Type();
                            serializedNote.Drawings.NoteDrawings[i].Points[j].X = serializedDrawings[i][j].x;
                            serializedNote.Drawings.NoteDrawings[i].Points[j].Y = serializedDrawings[i][j].y;
                            serializedNote.Drawings.NoteDrawings[i].Points[j].Z = serializedDrawings[i][j].z;
                        }
                    }
                    noteList.Add(serializedNote);
                }
                NotesType notes = new NotesType();
                notes.Notes = noteList.ToArray();
                notes.ID = idList.ToArray();
                serializationItems.Add(notes);
                serializationItemsName.Add(ItemsChoiceType3.Notes);
            }

            // Save Animation Panels.
            if (animationPanelsContainer != null)
            {
                List<AnimationPanelType> serializedPanels = new List<AnimationPanelType>();
                List<int> idList = new List<int>();
                int i = 0;
                foreach (AnimationPanelsMenuController panel in animationPanelsContainer.GetComponentsInChildren<AnimationPanelsMenuController>())
                {
                    AnimationPanelType serializedPanel = new AnimationPanelType();

                    if (panel.animationPanel)
                    {
                        AnimationMenuController menuContr = panel.animationPanel.GetComponent<AnimationMenuController>();
                        if (menuContr)
                        {
                            //serializedPanel.Animation =
                            //    menuContr.activeAnimation.Serialize(menuContr.loopToggle.isOn, menuContr.autoplayToggle.isOn);
                            serializedPanel.Position = new Vector3Type()
                            {
                                X = panel.transform.position.x,
                                Y = panel.transform.position.y,
                                Z = panel.transform.position.z
                            };
                            serializedPanel.Rotation = new QuaternionType()
                            {
                                X = panel.transform.rotation.x,
                                Y = panel.transform.rotation.y,
                                Z = panel.transform.rotation.z,
                                W = panel.transform.rotation.w
                            };
                        }
                    }

                    serializedPanels.Add(serializedPanel);
                    idList.Add(++i);
                }
                AnimationPanelsType panelsType = new AnimationPanelsType();
                panelsType.AnimationPanels = serializedPanels.ToArray();
                panelsType.index = idList.ToArray();
                serializationItems.Add(panelsType);
                serializationItemsName.Add(ItemsChoiceType3.AnimationPanels);
            }

            // Store in Serialization Destination.
            ProjectType serializationDestination = new ProjectType();
            serializationDestination.Items = serializationItems.ToArray();
            serializationDestination.ItemsElementName = serializationItemsName.ToArray();
            return serializationDestination;
        }
         */

        /// <seealso cref="VersionedMRETBehaviour{T}.Serialize(T, SerializationState)"/>
        protected override IEnumerator Serialize(ContentType serialized, SerializationState serializationState)
        {
            StartCoroutine(base.Serialize(serialized, serializationState));

            // Wait for the coroutine to complete
            while (!serializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) yield break;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Serialize the scene objects (optional)
            serialized.SceneObjects = null;
            if ((_sceneObjects != null) && ((_sceneObjects.Children.Length > 0) || (_sceneObjects.ChildGroups.Length > 0)))
            {
                SceneObjectsType serializedSceneObjects = _sceneObjects.CreateSerializedType();

                // Serialize the scene objects
                SerializationState sceneObjectsState = new SerializationState();
                StartCoroutine(_sceneObjects.SerializeWithLogging(serializedSceneObjects, sceneObjectsState));

                // Wait for the coroutine to complete
                while (!sceneObjectsState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // Record the serialization state
                serializationState.Update(sceneObjectsState);

                // If the content serialization failed, there's no point in continuing. Something is wrong
                if (serializationState.IsError) yield break;

                // Assign the scene objects
                serialized.SceneObjects = serializedSceneObjects;

                // Clear the state
                serializationState.Clear();
            }

            // Serialize the parts (optional)
            serialized.Parts = null;
            if ((_parts != null) && ((_parts.Children.Length > 0) || (_parts.ChildGroups.Length > 0)))
            {
                PartsType serializedParts = _parts.CreateSerializedType();

                // Serialize the parts
                SerializationState partsState = new SerializationState();
                StartCoroutine(_parts.SerializeWithLogging(serializedParts, partsState));

                // Wait for the coroutine to complete
                while (!partsState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // Record the serialization state
                serializationState.Update(partsState);

                // If the content serialization failed, there's no point in continuing. Something is wrong
                if (serializationState.IsError) yield break;

                // Assign the parts
                serialized.Parts = serializedParts;

                // Clear the state
                serializationState.Clear();
            }

            // TODO: displays (optional)
            serialized.Displays = null;
            {
                // TODO: HUD

                // TODO: Panels

                // Serialize the notes (optional)
                if ((_notes != null) && ((_notes.Children.Length > 0) || (_notes.ChildGroups.Length > 0)))
                {
                    // Make sure we have a serialized displays reference
                    serialized.Displays = (serialized.Displays == null) ? new DisplaysType() : null;

                    // Create the serialized notes to load
                    serialized.Displays.Notes = null;
                    NotesType serializedNotes = _notes.CreateSerializedType();

                    // Serialize the notes
                    SerializationState notesState = new SerializationState();
                    StartCoroutine(_notes.SerializeWithLogging(serializedNotes, notesState));

                    // Wait for the coroutine to complete
                    while (!notesState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                    // Record the serialization state
                    serializationState.Update(notesState);

                    // If the content serialization failed, there's no point in continuing. Something is wrong
                    if (serializationState.IsError) yield break;

                    // Assign the notes
                    serialized.Displays.Notes = serializedNotes;

                    // Clear the state
                    serializationState.Clear();
                }
            }

            // Serialize the drawings (optional)
            serialized.Drawings = null;
            if (_drawings != null)
            {
                List<Drawing3dType> serializedDrawings = new List<Drawing3dType>();

                // Serialize the drawings
                IInteractable3dDrawing[] drawings = _drawings.GetComponentsInChildren<IInteractable3dDrawing>();
                foreach (IInteractable3dDrawing drawing in drawings)
                {
                    // Create the empty serialized type to load
                    Drawing3dType serializedDrawing = drawing.CreateSerializedType();

                    SerializationState drawingSerializationState = new SerializationState();
                    Action<bool, string> OnDrawingSerializedAction = (bool loaded, string message) =>
                    {
                        if (loaded)
                        {
                            serializedDrawings.Add(serializedDrawing);

                            // Mark as complete
                            drawingSerializationState.complete = true;
                        }
                        else
                        {
                            // Record the error
                            drawingSerializationState.Error("A problem was encountered " +
                                "serializing the drawing: " + message);
                        }
                    };

                    // Serialize the drawing
                    drawing.Serialize(serializedDrawing, OnDrawingSerializedAction);

                    // Wait for the serialization to complete
                    while (!drawingSerializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                    // Record the serialization state
                    serializationState.Update(drawingSerializationState);

                    // If the serialization failed, there's no point in continuing. Something is wrong
                    if (serializationState.IsError) yield break;
                }

                // Assign the drawings
                serialized.Drawings = serializedDrawings.Count > 0 ? serializedDrawings.ToArray() : null;

                // Clear the state
                serializationState.Clear();
            }

            // Serialize the markers (optional)
            serialized.Markers = null;
            if (_markers != null)
            {
                List<MarkerType> serializedMarkers = new List<MarkerType>();
                List<MarkerPathType> serializedMarkerPaths = new List<MarkerPathType>();

                // Serialize the markers
                InteractableMarker[] markers = _markers.GetComponentsInChildren<InteractableMarker>();
                foreach (InteractableMarker marker in markers)
                {
                    // Create the empty serialized type to load
                    MarkerType serializedMarker = marker.CreateSerializedType();

                    SerializationState markerSerializationState = new SerializationState();
                    Action<bool, string> OnMarkerSerializedAction = (bool loaded, string message) =>
                    {
                        if (loaded)
                        {
                            serializedMarkers.Add(serializedMarker);

                            // Mark as complete
                            markerSerializationState.complete = true;
                        }
                        else
                        {
                            // Record the error
                            markerSerializationState.Error("A problem was encountered " +
                                "serializing the marker: " + message);
                        }
                    };

                    // Serialize the marker
                    marker.Serialize(serializedMarker, OnMarkerSerializedAction);

                    // Wait for the serialization to complete
                    while (!markerSerializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                    // Record the serialization state
                    serializationState.Update(markerSerializationState);

                    // If the serialization failed, there's no point in continuing. Something is wrong
                    if (serializationState.IsError) yield break;
                }

                // Serialize the paths
                MarkerPath[] markerPaths = _markers.GetComponentsInChildren<MarkerPath>();
                foreach (MarkerPath markerPath in markerPaths)
                {
                    // Create the empty serialized type to load
                    MarkerPathType serializedMarkerPath = markerPath.CreateSerializedType();

                    SerializationState markerPathSerializationState = new SerializationState();
                    Action<bool, string> OnMarkerPathSerializedAction = (bool loaded, string message) =>
                    {
                        if (loaded)
                        {
                            serializedMarkerPaths.Add(serializedMarkerPath);

                            // Mark as complete
                            markerPathSerializationState.complete = true;
                        }
                        else
                        {
                            // Record the error
                            markerPathSerializationState.Error("A problem was encountered " +
                                "serializing the marker path: " + message);
                        }
                    };

                    // Serialize the marker path
                    markerPath.Serialize(serializedMarkerPath, OnMarkerPathSerializedAction);

                    // Wait for the serialization to complete
                    while (!markerPathSerializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                    // Record the serialization state
                    serializationState.Update(markerPathSerializationState);

                    // If the serialization failed, there's no point in continuing. Something is wrong
                    if (serializationState.IsError) yield break;
                }

                // Assign the markers
                if (serializedMarkers.Count > 0)
                {
                    serialized.Markers = new MarkersType()
                    {
                        Marker = serializedMarkers.Count > 0 ? serializedMarkers.ToArray() : null,
                        Paths = serializedMarkerPaths.Count > 0 ? serializedMarkerPaths.ToArray() : null
                    };
                }

                // Clear the state
                serializationState.Clear();
            }

            // TODO: Feeds (optional)

            // TODO: ActionSequences/Animations (optional)

            // PointClouds (optional)
            serialized.PointClouds = null;
            if ((_pointClouds != null) && ((_pointClouds.Children.Length > 0) || (_pointClouds.ChildGroups.Length > 0)))
            {
                PointCloudsType serializedPointClouds = _pointClouds.CreateSerializedType();

                // Serialize the point clouds
                SerializationState pointCloudsState = new SerializationState();
                StartCoroutine(_pointClouds.SerializeWithLogging(serializedPointClouds, pointCloudsState));

                // Wait for the coroutine to complete
                while (!pointCloudsState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // Record the serialization state
                serializationState.Update(pointCloudsState);

                // If the content serialization failed, there's no point in continuing. Something is wrong
                if (serializationState.IsError) yield break;

                // Assign the point clouds
                serialized.PointClouds = serializedPointClouds;

                // Clear the state
                serializationState.Clear();
            }

            // TODO: Scene Object Generators (optional)

            // TODO: Terrains (optional)

            // IoT Things (optional)
            serialized.IoTThings = null;
            if (_iotThings != null)
            {
                List<IoTThingType> serializedThings = new List<IoTThingType>();

                // Serialize the IoT things
                IIoTThing[] things = _iotThings.GetComponentsInChildren<IIoTThing>();
                foreach (IIoTThing thing in things)
                {
                    // Create the empty serialized type to load
                    IoTThingType serializedThing = thing.CreateSerializedType();

                    SerializationState thingSerializationState = new SerializationState();
                    Action<bool, string> OnThingSerializedAction = (bool loaded, string message) =>
                    {
                        if (loaded)
                        {
                            serializedThings.Add(serializedThing);

                            // Mark as complete
                            thingSerializationState.complete = true;
                        }
                        else
                        {
                            // Record the error
                            thingSerializationState.Error("A problem was encountered " +
                                "serializing the IoTThing: " + message);
                        }
                    };

                    // Serialize the IoTThing
                    thing.Serialize(serializedThing, OnThingSerializedAction);

                    // Wait for the serialization to complete
                    while (!thingSerializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                    // Record the serialization state
                    serializationState.Update(thingSerializationState);

                    // If the content serialization failed, there's no point in continuing. Something is wrong
                    if (serializationState.IsError) yield break;
                }

                // Assign the IoTThings
                serialized.IoTThings = serializedThings.Count > 0 ? serializedThings.ToArray() : null;

                // Clear the state
                serializationState.Clear();
            }

            // Save the final serialized reference
            serializedContent = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;

            yield return null;
        }
        #endregion Serializable

    }
}
