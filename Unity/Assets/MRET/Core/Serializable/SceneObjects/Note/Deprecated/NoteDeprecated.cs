// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;
using GOV.NASA.GSFC.XR.XRUI;
using GOV.NASA.GSFC.XR.XRUI.Keyboard;
using GOV.NASA.GSFC.XR.MRET.Action;
using GOV.NASA.GSFC.XR.MRET.Collaboration;
using GOV.NASA.GSFC.XR.MRET.Project;
using GOV.NASA.GSFC.XR.MRET.Tools.UndoRedo;
using GOV.NASA.GSFC.XR.MRET.Tools.Selection;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_1;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Note
{
    [Obsolete("Refer to the " + nameof(GOV.NASA.GSFC.XR.MRET.SceneObjects.Note.InteractableNote) + " class")]
    public class NoteDeprecated : InteractableSceneObjectDeprecated, ISelectable
    {
        public VR_InputField titleText, informationText;
        public GameObject fullNote, minimizedNote, drawingContainer;
        public Gradient lineColor;
        public Material drawingMaterial;
        public Color highlightColor = new Color();
        public bool textBeingAutoChanged = false;
        public int id = -1;
        public Guid guid;
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

        private bool drawingLeft = false, drawingRight = false, _canDraw = true;
        private int noteDrawingCount = 0;
        private LineRenderer currentLeftLine = null, currentRightLine = null;
        private GameObject leftUsingController, rightUsingController;
        private ControllerUIRaycastDetectionManager leftRaycaster, rightRaycaster;
        private SynchronizedNoteDeprecated synchronizedNote;
        private string lastRecordedTitle = "", lastRecordedInformation = "";
        private UndoManagerDeprecated undoManager;
        private BoxCollider noteCollider;

        public Color color
        {
            get
            {
                return minimizedNote.GetComponent<Image>().color;
            }
            set
            {
                minimizedNote.GetComponent<Image>().color = value;
                titleText.transform.parent.GetComponent<Image>().color = value;
                informationText.transform.parent.GetComponent<Image>().color = value;
            }
        }

        public NoteType ToNoteType()
        {
            Vector3[][] serializedDrawings = SerializeDrawings();

            NoteDrawingType[] noteDrawings = new NoteDrawingType[serializedDrawings.Length];
            for (int i = 0; i < serializedDrawings.Length; i++)
            {
                noteDrawings[i] = new NoteDrawingType();
                noteDrawings[i].Points = new Vector3Type[serializedDrawings[i].Length];
                for (int j = 0; j < serializedDrawings[i].Length; j++)
                {
                    noteDrawings[i].Points[j] = new Vector3Type();
                    noteDrawings[i].Points[j].X = serializedDrawings[i][j].x;
                    noteDrawings[i].Points[j].Y = serializedDrawings[i][j].y;
                    noteDrawings[i].Points[j].Z = serializedDrawings[i][j].z;
                }
            }

            return new NoteType()
            {
                GUID = guid.ToString(),
                Title = titleText.text,
                Details = informationText.text,
                Drawings = new NoteDrawingsType()
                {
                    NoteDrawings = noteDrawings
                },
                State = fullNote.activeSelf ? NoteTypeState.Maximized : NoteTypeState.Minimized,
                Transform = new UnityTransformType()
                {
                    Position = new Vector3Type()
                    {
                        X = transform.position.x,
                        Y = transform.position.y,
                        Z = transform.position.z
                    },
                    Rotation = new QuaternionType()
                    {
                        X = transform.rotation.x,
                        Y = transform.rotation.y,
                        Z = transform.rotation.z
                    },
                    Scale = new NonNegativeFloat3Type()
                    {
                        X = transform.localScale.x,
                        Y = transform.localScale.y,
                        Z = transform.localScale.z
                    }
                }
            };
        }

        protected override void MRETStart()
        {
            base.MRETStart();

            unityProject = GameObject.Find("LoadedProject").GetComponent<UnityProjectDeprecated>();

            titleText.onValueChanged.AddListener(delegate { CaptureTitleChange(); });
            //informationText.onValueChanged.AddListener(delegate { CaptureInformationChange(); });

            if (unityProject.collaborationEnabled)
            {
                synchronizedNote = gameObject.AddComponent<SynchronizedNoteDeprecated>();
            }

            undoManager = FindObjectOfType<UndoManagerDeprecated>();

            noteCollider = fullNote.GetComponent<BoxCollider>();
        }

        protected override void MRETUpdate()
        {
            base.MRETUpdate();

            HandleScaleManagement();

            if (canDraw && drawingLeft && leftRaycaster.intersectionStatus)
            {
                Vector3 raycastPoint = leftRaycaster.raycastPoint;
                if (noteCollider.bounds.Contains(raycastPoint))
                {
                    if (!currentLeftLine)
                    {
                        GameObject drawing = new GameObject("NoteDrawing" + noteDrawingCount++);
                        drawing.transform.SetParent(drawingContainer.transform);
                        currentLeftLine = drawing.AddComponent<LineRenderer>();
                        currentLeftLine.widthMultiplier = 0.0025f;
                        currentLeftLine.colorGradient = lineColor;
                        currentLeftLine.useWorldSpace = false;
                        currentLeftLine.SetPosition(0, raycastPoint);
                        currentLeftLine.positionCount = 0;
                        currentLeftLine.material = drawingMaterial;
                    }

                    currentLeftLine.positionCount++;
                    currentLeftLine.SetPosition(currentLeftLine.positionCount - 1, raycastPoint);
                }
            }

            if (canDraw && drawingRight && rightRaycaster.intersectionStatus)
            {
                Vector3 raycastPoint = rightRaycaster.raycastPoint;
                if (noteCollider.bounds.Contains(raycastPoint))
                {
                    if (!currentRightLine)
                    {
                        GameObject drawing = new GameObject("NoteDrawing" + noteDrawingCount++);
                        drawing.transform.SetParent(drawingContainer.transform);
                        currentRightLine = drawing.AddComponent<LineRenderer>();
                        currentRightLine.widthMultiplier = 0.0025f;
                        currentRightLine.colorGradient = lineColor;
                        currentRightLine.useWorldSpace = false;
                        currentRightLine.SetPosition(0, raycastPoint);
                        currentRightLine.positionCount = 0;
                        currentRightLine.material = drawingMaterial;
                    }

                    currentRightLine.positionCount++;
                    currentRightLine.SetPosition(currentRightLine.positionCount - 1, raycastPoint);
                }
            }
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
                {   // Right.
                    Vector3[] points = new Vector3[currentRightLine.positionCount];
                    currentRightLine.GetPositions(points);
                    undoManager.AddAction(ProjectActionDeprecated.AddNoteDrawingAction(transform.name, currentRightLine.name, points),
                        ProjectActionDeprecated.DeleteNoteDrawingAction(transform.name, currentRightLine.name));
                    currentRightLine = null;
                    drawingRight = false;
                    rightUsingController = null;
                    rightRaycaster = null;
                }
                else if (currentLeftLine)
                {   // Left.
                    Vector3[] points = new Vector3[currentLeftLine.positionCount];
                    currentLeftLine.GetPositions(points);
                    undoManager.AddAction(ProjectActionDeprecated.AddNoteDrawingAction(transform.name, currentLeftLine.name, points),
                        ProjectActionDeprecated.DeleteNoteDrawingAction(transform.name, currentLeftLine.name));
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

        public static NoteDeprecated MakeNote(GameObject notePrefab, Vector3 pos, Quaternion rot, int id)
        {
            GameObject noteObj = Instantiate(notePrefab, pos, rot, GameObject.Find("Notes").transform);
            noteObj.name = "NoteDeprecated" + id;
            return noteObj.GetComponent<NoteDeprecated>();
        }

        public static NoteDeprecated MakeNote(GameObject notePrefab, Vector3 pos, Quaternion rot, string noteName)
        {
            GameObject noteObj = Instantiate(notePrefab, pos, rot, GameObject.Find("Notes").transform);
            noteObj.name = noteName;
            return noteObj.GetComponent<NoteDeprecated>();
        }

        public void Minimize()
        {
            fullNote.SetActive(false);
            minimizedNote.SetActive(true);
        }

        public void Maximize()
        {
            minimizedNote.SetActive(false);
            fullNote.SetActive(true);
        }

        public static NoteData toSerializable(NoteDeprecated note)
        {
            return new NoteData()
            {
                title = note.titleText.text,
                information = note.informationText.text,
                pos = note.gameObject.transform.position,
                rot = note.gameObject.transform.rotation,
            };
        }

        public static NoteDeprecated fromSerializable(NoteData data, NoteDrawingType[] drawings, int index, Guid guid)
        {
            NoteDeprecated n = MakeNote(NoteManagerDeprecated.notePrefab, data.pos, data.rot, index);
            n.guid = guid;
            n.titleText.text = data.title;
            n.informationText.text = data.information;
            n.DeserializeDrawings(drawings);
            n.Maximize();
            return n;
        }

        public static NoteDeprecated fromSerializable(NoteData data, NoteDrawingType[] drawings, string noteName, Guid guid)
        {
            NoteDeprecated n = MakeNote(NoteManagerDeprecated.notePrefab, data.pos, data.rot, noteName);
            n.guid = guid;
            n.titleText.text = data.title;
            n.informationText.text = data.information;
            n.DeserializeDrawings(drawings);
            n.Maximize();
            return n;
        }

        public Vector3[][] SerializeDrawings()
        {
            List<Vector3[]> returnList = new List<Vector3[]>();

            foreach (Transform drawObj in drawingContainer.transform)
            {
                LineRenderer rend = drawObj.GetComponent<LineRenderer>();
                if (rend)
                {
                    List<Vector3> points = new List<Vector3>();
                    for (int i = 0; i < rend.positionCount; i++)
                    {
                        points.Add(rend.GetPosition(i));
                    }
                    returnList.Add(points.ToArray());
                }
            }

            return returnList.ToArray();
        }

        private void CaptureTitleChange()
        {
            if (synchronizedNote)
            {
                synchronizedNote.UpdateTitleText(titleText.text);
            }

            if (!textBeingAutoChanged)
            {
                undoManager.AddAction(ProjectActionDeprecated.ChangeNoteTextAction(transform.name, titleText.text, informationText.text),
                    ProjectActionDeprecated.ChangeNoteTextAction(transform.name, lastRecordedTitle, lastRecordedInformation));
                lastRecordedTitle = titleText.text;
                lastRecordedInformation = informationText.text;
            }
        }

        private void CaptureInformationChange()
        {
            if (synchronizedNote)
            {
                synchronizedNote.UpdateInformationText(informationText.text);
            }

            if (!textBeingAutoChanged)
            {
                undoManager.AddAction(ProjectActionDeprecated.ChangeNoteTextAction(transform.name, titleText.text, informationText.text),
                    ProjectActionDeprecated.ChangeNoteTextAction(transform.name, lastRecordedTitle, lastRecordedInformation));
                lastRecordedTitle = titleText.text;
                lastRecordedInformation = informationText.text;
            }
        }

        private void DeserializeDrawings(NoteDrawingType[] drawings)
        {
            if (drawings != null)
            {
                foreach (NoteDrawingType drawing in drawings)
                {
                    GameObject draw = new GameObject();
                    draw.transform.SetParent(drawingContainer.transform);
                    LineRenderer lineRend = draw.AddComponent<LineRenderer>();
                    lineRend.widthMultiplier = 0.0025f;
                    lineRend.colorGradient = lineColor;
                    lineRend.useWorldSpace = false;
                    lineRend.positionCount = 0;
                    lineRend.material = drawingMaterial;

                    foreach (Vector3Type point in drawing.Points)
                    {
                        lineRend.positionCount++;
                        lineRend.SetPosition(lineRend.positionCount - 1, new Vector3(point.X, point.Y, point.Z));
                    }
                }
            }
        }

        #region ScaleManagement
        public float minScale = 0.1f, maxScale = 1;
        private UnityProjectDeprecated unityProject = null;
        private int updateDivider = 0;
        private bool minimizedRescaled = false;

        void HandleScaleManagement()
        {
            if (updateDivider++ > 16 && unityProject)
            {
                if (fullNote)
                {
                    fullNote.transform.localScale = new Vector3(1 / unityProject.scaleMultiplier, 1 / unityProject.scaleMultiplier, 1 / unityProject.scaleMultiplier);
                }

                if (minimizedNote)
                {
                    if (unityProject.scaleMultiplier < minScale)
                    {
                        minimizedNote.transform.localScale = new Vector3(minScale / unityProject.scaleMultiplier, minScale / unityProject.scaleMultiplier, minScale / unityProject.scaleMultiplier);
                        minimizedRescaled = true;
                    }
                    else if (minimizedRescaled)
                    {
                        minimizedNote.transform.localScale = new Vector3(1, 1, 1);
                        minimizedRescaled = false;
                    }

                    if (unityProject.scaleMultiplier > maxScale)
                    {
                        minimizedNote.transform.localScale = new Vector3(maxScale / unityProject.scaleMultiplier, maxScale / unityProject.scaleMultiplier, maxScale / unityProject.scaleMultiplier);
                    }
                }

                updateDivider = 0;
            }
        }
        #endregion

        [System.Serializable]
        public class NoteData
        {
            public string title;
            public string information;
            public Vector3 pos;
            public Quaternion rot;
        }

        #region Selection
        private bool isSelected = false;

        public void Select(bool hierarchical = true)
        {
            if (isSelected)
            {
                return;
            }

            isSelected = true;

            // Highlight the entire note.
            Highlight();
        }

        public void Deselect(bool hierarchical = true)
        {
            if (!isSelected)
            {
                return;
            }

            isSelected = false;

            // Unhighlight the entire note.
            Unhiglight();

        }
        #endregion

        #region Highlighting
        private Color initialTitleColor, initialInformationColor;

        private void SaveNormalColors()
        {
            initialTitleColor = titleText.colors.normalColor;
            initialInformationColor = informationText.colors.normalColor;
        }

        private void RestoreNormalColors()
        {
            ColorBlock newTCB = titleText.colors;
            newTCB.normalColor = initialTitleColor;
            titleText.colors = newTCB;

            ColorBlock newICB = informationText.colors;
            newICB.normalColor = initialInformationColor;
            informationText.colors = newICB;
        }

        private void ApplyHighlightedColors()
        {
            ColorBlock newTCB = titleText.colors;
            newTCB.normalColor = highlightColor;
            titleText.colors = newTCB;

            ColorBlock newICB = informationText.colors;
            newICB.normalColor = highlightColor;
            informationText.colors = newICB;
        }

        private void Highlight()
        {
            SaveNormalColors();
            ApplyHighlightedColors();
        }

        private void Unhiglight()
        {
            RestoreNormalColors();
        }
        #endregion
    }
}