// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;
using GOV.NASA.GSFC.XR.XRUI;
using GOV.NASA.GSFC.XR.XRUI.Keyboard;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.Action;
using GOV.NASA.GSFC.XR.MRET.Configuration;
using GOV.NASA.GSFC.XR.MRET.Collaboration;
using GOV.NASA.GSFC.XR.MRET.Project;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Note
{
    public class InteractableNote : InteractableDisplay<NoteType>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(InteractableNote);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private NoteType serializedNote;

        /// <summary>
        /// The list of drawings in this note.
        /// </summary>
        protected List<Interactable2dDrawing> drawing2dObjectList = new List<Interactable2dDrawing>();
        public Interactable2dDrawing[] drawing2dObjects
        {
            get
            {
                return drawing2dObjectList.ToArray();
            }
        }

        public VR_InputField informationTextField;

        public string InformationText
        {
            get
            {
                return (informationTextField != null) ? informationTextField.text : "";
            }
            set
            {
                if (informationTextField != null)
                {
                    informationTextField.text = value;
                }
            }
        }
        public GameObject drawingArea;
        public GameObject drawingContainer;
        public Gradient lineColor;
        public Material drawingMaterial;
        public float drawingWidth;
        public bool informationTextBeingAutoChanged = false;
        public bool canDraw
        {
            get
            {
                return _canDraw;
            }
            private set
            {
                _canDraw = value;
            }
        }

        private bool needToDisable = false, needToEnable = false;
        private int enableDelay = 0;
        private bool drawingLeft = false, drawingRight = false, _canDraw = true;
        private int noteDrawingCount = 0;
        private Interactable2dDrawing currentLeftLine = null, currentRightLine = null;
        private GameObject leftUsingController, rightUsingController;
        private ControllerUIRaycastDetectionManager leftRaycaster, rightRaycaster;
        private string lastRecordedInformation = "";
        private BoxCollider drawingAreaCollider;

        #region MRETUpdateBehaviour
        /// <seealso cref="MRETBehaviour.IntegrityCheck"/>
        protected override IntegrityState IntegrityCheck()
        {
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            return (
                (state == IntegrityState.Failure) ||
                (drawingArea == null) ||
                (drawingContainer == null)
                    ? IntegrityState.Failure   // Fail is base class fails or anything is null
                    : IntegrityState.Success); // Otherwise, our integrity is valid
        }

        /// <seealso cref="MRETBehaviour.MRETAwake"/>
        protected override void MRETAwake()
        {
            base.MRETAwake();

            // Initialize before deserialization
            lineColor = InteractableDrawingDefaults.GRADIENT;
            drawingMaterial = InteractableDrawingDefaults.DRAWING_MATERIAL;
            drawingWidth = InteractableDrawingDefaults.WIDTH;
        }

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            base.MRETStart();

            // Make sure our drawing properites are valid
            if (drawingMaterial == null)
            {
                drawingMaterial = InteractableDrawingDefaults.DRAWING_MATERIAL;
            }
            if (lineColor == null)
            {
                lineColor = InteractableDrawingDefaults.GRADIENT;
            }
            if (drawingWidth <= 0.0f)
            {
                drawingWidth = InteractableDrawingDefaults.WIDTH;
            }

            // Setup information text change listener
            if (informationTextField != null)
            {
                informationTextField.onValueChanged.AddListener(delegate { CaptureInformationChange(); });
            }
        }

        /// <seealso cref="MRETUpdateBehaviour.MRETUpdate"/>
        protected override void MRETUpdate()
        {
            base.MRETUpdate();

            if (needToDisable)
            {
                gameObject.SetActive(false);
                needToDisable = false;
                needToEnable = true;
            }

            if (needToEnable)
            {
                if (enableDelay > 100)
                {
                    gameObject.SetActive(true);
                    needToEnable = false;
                    enableDelay = 0;
                }
                else
                {
                    enableDelay++;
                }
            }

            if (drawingArea != null)
            {
                if (drawingAreaCollider == null)
                {
                    drawingAreaCollider = drawingArea.GetComponent<BoxCollider>();
                }

                if (drawingAreaCollider != null)
                {
                    if (canDraw && drawingLeft && leftRaycaster.intersectionStatus)
                    {
                        Vector3 raycastPoint = leftRaycaster.raycastPoint;
                        if (drawingAreaCollider.bounds.Contains(raycastPoint))
                        {
                            if (!currentLeftLine)
                            {
                                GameObject drawing = new GameObject("NoteDrawing" + noteDrawingCount++);
                                drawing.transform.SetParent(drawingContainer.transform);
                                currentLeftLine = drawing.AddComponent<Interactable2dDrawing>();
                                currentLeftLine.width = drawingWidth;
                                currentLeftLine.Material = drawingMaterial;
                                currentLeftLine.Gradient = lineColor;
                            }

                            // Add the raycast point
                            List<Vector3> drawingPoints = new List<Vector3>(currentLeftLine.points);
                            drawingPoints.Add(raycastPoint);
                            currentLeftLine.points = drawingPoints.ToArray();
                        }
                    }

                    if (canDraw && drawingRight && rightRaycaster.intersectionStatus)
                    {
                        Vector3 raycastPoint = rightRaycaster.raycastPoint;
                        if (drawingAreaCollider.bounds.Contains(raycastPoint))
                        {
                            if (!currentRightLine)
                            {
                                GameObject drawing = new GameObject("NoteDrawing" + noteDrawingCount++);
                                drawing.transform.SetParent(drawingContainer.transform);
                                currentRightLine = drawing.AddComponent<Interactable2dDrawing>();
                                currentRightLine.width = drawingWidth;
                                currentRightLine.Material = drawingMaterial;
                                currentRightLine.Gradient = lineColor;
                            }

                            // Add the raycast point
                            List<Vector3> drawingPoints = new List<Vector3>(currentRightLine.points);
                            drawingPoints.Add(raycastPoint);
                            currentRightLine.points = drawingPoints.ToArray();
                        }
                    }
                }
            }
        }
        #endregion MRETUpdateBehaviour

        #region Serializable
        /// <seealso cref="InteractableDisplay{T}.Deserialize(T, SerializationState)"/>
        protected override IEnumerator Deserialize(NoteType serialized, SerializationState deserializationState)
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
            serializedNote = serialized;

            // Deserialize the note
            informationTextBeingAutoChanged = true;
            InformationText = serializedNote.Content;
            informationTextBeingAutoChanged = false;

            // Deserialize the drawings
            if (serializedNote.Drawings != null)
            {
                // Deserialize each drawing
                foreach (Drawing2dType drawing in serializedNote.Drawings)
                {
                    // Create and deserialize the drawing
                    GameObject drawingGameObject = new GameObject();
                    drawingGameObject.transform.parent = drawingContainer.transform;
                    Interactable2dDrawing drawingObject = drawingGameObject.AddComponent<Interactable2dDrawing>();

                    // Perform the drawing deserialization
                    SerializationState drawingDeserializationState = new SerializationState();
                    StartCoroutine(drawingObject.DeserializeWithLogging(drawing, drawingDeserializationState));

                    // Wait for the coroutine to complete
                    while (!drawingDeserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                    // Record the deserialization state
                    deserializationState.Update(drawingDeserializationState);

                    // Check for an error deserializing
                    if (!deserializationState.IsError)
                    {
                        // Add the drawing
                        drawing2dObjectList.Add(drawingObject);
                    }
                    else
                    {
                        // Destroy the drawing
                        Destroy(drawingObject);

                        // Abort the deserialization
                        yield break;
                    }
                }
            }

            // Record the deserialization state as complete
            deserializationState.complete = true;

            yield return null;
        }

        /// <seealso cref="InteractableDisplay{T}.Serialize(T, SerializationState)"/>
        protected override IEnumerator Serialize(NoteType serialized, SerializationState serializationState)
        {
            StartCoroutine(base.Serialize(serialized, serializationState));

            // Wait for the coroutine to complete
            while (!serializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) yield break;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Serialize the note
            serialized.Content = InformationText;

            // Serialize the drawings, but only assign a value if we have any
            serialized.Drawings = null;
            if (drawing2dObjectList.Count > 0)
            {
                // Serialize the drawings. The drawings can serialize themselves.
                List<Drawing2dType> serializedDrawings = new List<Drawing2dType>();
                foreach (Interactable2dDrawing drawingObject in drawing2dObjectList)
                {
                    // Perform the drawing serialization
                    SerializationState drawingSerializationState = new SerializationState();
                    Drawing2dType serializedDrawing = drawingObject.CreateSerializedType();
                    StartCoroutine(drawingObject.SerializeWithLogging(serializedDrawing, drawingSerializationState));

                    // Wait for the coroutine to complete
                    while (!drawingSerializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                    // Record the serialization state
                    serializationState.Update(drawingSerializationState);

                    // Check for an error deserializing
                    if (!serializationState.IsError)
                    {
                        // Add the drawing
                        serializedDrawings.Add(serializedDrawing);
                    }
                    else
                    {
                        // Destroy the drawing
                        Destroy(drawingObject);

                        // Abort the serialziation
                        yield break;
                    }
                }
                serialized.Drawings = serializedDrawings.ToArray();
            }

            // Save the final serialized reference
            serializedNote = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;

            yield return null;
        }
        #endregion Serializable

        /// <seealso cref="Identifiable{T}.CreateSynchronizedObject"/>
        protected override ISynchronized CreateSynchronizedObject()
        {
            return gameObject.AddComponent<SynchronizedNote>();
        }

        /// <seealso cref="InteractableSceneObject{T}.AfterBeginGrab"/>
        protected override void AfterBeginGrab(InputHand hand)
        {
            base.AfterBeginGrab(hand);

            DisableDrawing();
        }

        /// <seealso cref="InteractableSceneObject{T}.AfterEndGrab"/>
        protected override void AfterEndGrab(InputHand hand)
        {
            base.AfterEndGrab(hand);

            EnableDrawing();
        }

        public void EnableDrawing()
        {
            canDraw = true;
        }

        public void DisableDrawing()
        {
            canDraw = false;

            if (drawingLeft)
            {
                StopDrawing(leftUsingController);
            }

            if (drawingRight)
            {
                StopDrawing(rightUsingController);
            }
        }

        public void StopDrawing(GameObject controller)
        {
            InputHand hand = controller.GetComponent<InputHand>();
            if (hand)
            {
                if (hand == MRET.InputRig.rightHand && currentRightLine)
                {
                    // Right hand
                    var serializedDrawing = currentRightLine.CreateSerializedType();
                    currentRightLine.Serialize(serializedDrawing);

                    // Add the drawing
                    AddDrawing(serializedDrawing);

                    // Reset
                    currentRightLine = null;
                    drawingRight = false;
                    rightUsingController = null;
                    rightRaycaster = null;
                }
                else if (currentLeftLine)
                {
                    // Left hand
                    var serializedDrawing = currentLeftLine.CreateSerializedType();
                    currentLeftLine.Serialize(serializedDrawing);

                    // Add the drawing
                    AddDrawing(serializedDrawing);

                    // Reset
                    currentLeftLine = null;
                    drawingLeft = false;
                    leftUsingController = null;
                    leftRaycaster = null;
                }
            }
        }

        public void StartDrawing(GameObject controller)
        {
            InputHand hand = controller.GetComponent<InputHand>();
            if (hand)
            {
                if (hand == MRET.InputRig.rightHand)
                {   // Right.
                    rightRaycaster = hand.GetComponent<ControllerUIRaycastDetectionManager>();
                    drawingRight = true;
                    rightUsingController = controller;
                }
                else
                {   // Left.
                    leftRaycaster = hand.GetComponent<ControllerUIRaycastDetectionManager>();
                    drawingLeft = true;
                    leftUsingController = controller;
                }
            }
        }

        /// <summary>
        /// Create an interactable note.
        /// </summary>
        /// <param name="noteName">Name of the note</param>
        /// <param name="notePrefab">The note prefab to instantiate</param>
        /// <param name="container">The container (parent) for the note</param>
        /// <returns>The instantiated <code>InteractableNote</code></returns>
        public static InteractableNote Create(string noteName, GameObject notePrefab, Transform container = null)
        {
            // Make sure we have a valid container reference
            container = (container == null) ? ProjectManager.NotesContainer.transform : container;

            // Instantiate the prefab
            GameObject noteGO = Instantiate(notePrefab, container);

            // Get the interactable note reference
            InteractableNote note = noteGO.GetComponent<InteractableNote>();
            if (note == null)
            {
                note = noteGO.AddComponent<InteractableNote>();
                // TODO: If we have to add the interactable note, we need to set
                // the interactable note references to the correct note prefab
                // gameobjects to work.
            }

            // Generate a better ID for the note for serialization
            note.id = MRET.UuidRegistry.CreateUniqueIDFromName(noteName);

            // Rename the game object
            noteGO.name = noteName;

            return note;
        }

        public Interactable2dDrawing GetDrawing(string drawingId)
        {
            Interactable2dDrawing result = null;

            foreach (Interactable2dDrawing childDrawing in drawing2dObjects)
            {
                if (childDrawing.id == drawingId)
                {
                    result = childDrawing;
                    break;
                }
            }

            return result;
        }

        public bool ContainsDrawing(Drawing2dType serializedDrawing)
        {
            return (GetDrawing(serializedDrawing.ID) != null);
        }

        /// <summary>
        /// Adds the supplied serialized drawing to this note.
        /// </summary>
        /// <param name="serializedDrawing">The serialized drawing</param>
        /// <returns>A boolean indicating whether or not the drawing was added</returns>
        public bool AddDrawing(Drawing2dType serializedDrawing)
        {
            bool result = false;

            if (!ContainsDrawing(serializedDrawing))
            {
                // Create the drawing
                GameObject drawingGameObject = new GameObject();
                drawingGameObject.transform.parent = drawingContainer.transform;
                Interactable2dDrawing drawingObject = drawingGameObject.AddComponent<Interactable2dDrawing>();

                Action<bool, string> DeserializeAction = (bool successful, string message) =>
                {
                    if (successful)
                    {
                        // Add the drawing
                        drawing2dObjectList.Add(drawingObject);

                        // Record the action
                        ProjectManager.UndoManager.AddAction(
                            new AddDrawingToNoteAction(id, serializedDrawing),
                            new DeleteDrawingFromNoteAction(id, serializedDrawing));
                    }
                    else
                    {
                        LogError("Deserialization of the note drawing failed: " + message, nameof(AddDrawing));

                        // Destroy the drawing
                        Destroy(drawingObject);
                    }
                };

                // Perform the serialization
                drawingObject.Deserialize(serializedDrawing, DeserializeAction);
            }
            else
            {
                LogWarning("A drawing with the supplied ID is already contained within this note; ID: " +
                    serializedDrawing.ID, nameof(AddDrawing));
            }

            return result;
        }

        public bool DeleteDrawing(string drawingId)
        {
            bool result = false;

            // Obtain the drawing object
            Interactable2dDrawing drawingObject = GetDrawing(drawingId);
            if (drawingObject)
            {
                var serializedDrawing = drawingObject.CreateSerializedType();

                Action<bool, string> SerializeAction = (bool successful, string message) =>
                {
                    if (successful)
                    {
                        result = drawing2dObjectList.Remove(drawingObject);
                        if (result)
                        {
                            // Destroy the drawing
                            Destroy(drawingObject);

                            // Record the action
                            ProjectManager.UndoManager.AddAction(
                                new DeleteDrawingFromNoteAction(id, serializedDrawing),
                                new AddDrawingToNoteAction(id, serializedDrawing));
                        }
                    }
                    else
                    {
                        LogWarning("Serialization of the note drawing failed", nameof(DeleteDrawing));
                    }
                };

                // Perform the serialization
                drawingObject.Serialize(serializedDrawing, SerializeAction);
            }

            return result;
        }

        // Catpture the information changes as actions
        private void CaptureInformationChange()
        {
            if (!informationTextBeingAutoChanged)
            {
                ProjectManager.UndoManager.AddAction(
                    new IdentifiableObjectUpdateAction(
                        this,
                        new Dictionary<string, string>()
                        {
                            { nameof(InformationText), InformationText },
                        }),
                    new IdentifiableObjectUpdateAction(
                        this,
                        new Dictionary<string, string>()
                        {
                            { nameof(InformationText), lastRecordedInformation },
                        }));
                lastRecordedInformation = InformationText;
            }
        }

        #region Material Adjustment
        private Color initialInformationColor;

        protected override void SaveObjectMaterials(bool includeChildInteractables = false)
        {
            ;
            base.SaveObjectMaterials(includeChildInteractables);

            // Save the information color
            initialInformationColor = informationTextField.colors.normalColor;
        }

        protected override void RestoreObjectMaterials()
        {
            base.RestoreObjectMaterials();

            // Restore the information color
            ColorBlock newTCB = informationTextField.colors;
            newTCB.normalColor = initialInformationColor;
            informationTextField.colors = newTCB;
        }

        protected override void ReplaceObjectMaterials(Material matToUse, bool includeChildInteractables = false)
        {
            base.ReplaceObjectMaterials(matToUse, includeChildInteractables);

            // Replace the information color
            ColorBlock newTCB = informationTextField.colors;
            newTCB.normalColor = matToUse.color;
            informationTextField.colors = newTCB;
        }
        #endregion // Material Adjustment
    }
}