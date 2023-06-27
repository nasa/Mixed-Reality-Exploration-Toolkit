// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;
using GOV.NASA.GSFC.XR.MRET.Configuration;
using GOV.NASA.GSFC.XR.MRET.Project;
using GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.Drawing;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing
{
    /// <remarks>
    /// History:
    /// 11 Nov 2022: Created
    /// </remarks>
    ///
    /// <summary>
    /// 3D drawing object
    ///
    /// Author: Jeffrey Hosler
    /// </summary>
    /// 
	public abstract class Interactable3dDrawing : InteractableDrawing<Drawing3dType>, IInteractable3dDrawing<Drawing3dType>
    {
		/// <seealso cref="MRETBehaviour.ClassName"/>
		public override string ClassName => nameof(Interactable3dDrawing);

        /// <summary>
        /// Scale multiplier for endpoint visuals.
        /// </summary>
        private const float DEFAULT_ENDPOINTVISUALSCALEMULTIPLIER = 1.5f;

        private const float DEFAULT_MIDPOINTVISUALSCALEMULTIPLIER = 1.75f;

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private Drawing3dType serializedDrawing;

        /// <summary>
        /// The total measurement text.
        /// </summary>
        protected MeasurementText measurementText { get => _measurementText; }
        private MeasurementText _measurementText;

        /// <summary>
        /// The segment measurement texts.
        /// </summary>
        protected MeasurementText[] measurementSegmentTexts { get => _measurementSegmentTexts.ToArray(); }
        private List<MeasurementText> _measurementSegmentTexts;

        /// <summary>
        /// The drawing edit controller.
        /// </summary>
        protected DrawingEditController drawingEditController { get => _drawingEditController; }
        private DrawingEditController _drawingEditController;

        /// <summary>
        /// Container for point visuals.
        /// </summary>
        protected GameObject pointVisualContainer;

        /// <summary>
        /// Visualizations for the endpoints.
        /// </summary>
        private Dictionary<Vector3, GameObject> endpointVisuals;

        /// <summary>
        /// Visualizations for the points in between the endpoints.
        /// </summary>
        private Dictionary<Vector3, GameObject> midpointVisuals;

        /// <summary>
        /// Remaining length for drawing, -1 if no limit.
        /// </summary>
        public virtual float remainingLength
        {
            get
            {
                if (float.IsInfinity(LengthLimit) || float.IsNaN(LengthLimit) || (LengthLimit < 0))
                {
                    return -1;
                }

                return Math.Max(LengthLimit - length, 0);
            }
        }

        private bool initializing = true;

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

        /// <seealso cref="MRETBehaviour.MRETAwake"/>
        protected override void MRETAwake()
        {
            base.MRETAwake();

            // Initialize the internal components
            _measurementSegmentTexts = new List<MeasurementText>();
            _measurementText = CreateMeasurementText();
            CreateEditController();

            // Setup the visual point container
            pointVisualContainer = new GameObject("PointVisuals");
            pointVisualContainer.transform.SetParent(transform);
            pointVisualContainer.transform.localPosition = Vector3.zero;
            pointVisualContainer.transform.localRotation = Quaternion.identity;
            pointVisualContainer.transform.localScale = Vector3.one;

            // Initialize defaults
            DesiredUnits = Interactable3dDrawingDefaults.DESIRED_UNITS;
            DisplayMeasurement = Interactable3dDrawingDefaults.DISPLAY_MEASUREMENT;
            DisplaySegmentMeasurements = Interactable3dDrawingDefaults.DISPLAY_SEGMENT_MEASUREMENTS;
            LengthLimit = Interactable3dDrawingDefaults.LIMIT_LENGTH;
            EditingActive = (_drawingEditController != null) ? _drawingEditController.menuEnabled : false;

            // Override the default touch behavior
            TouchBehavior = IInteractable.TouchBehaviors.Hold;
        }

        protected override void MRETStart()
        {
            base.MRETStart();

            // Initialize the internal components here because the initialization may rely on the creation
            // of the internal components within the entire class hierarchy during the awake process since
            // there are abstract methods that need an implementation.
            InitializeEditController();
            initializing = false;

            // Force these properties to reinitialize with the current deserialized values
            DisplayMeasurement = _displayMeasurement;
            DisplaySegmentMeasurements = _displaySegmentMeasurements;
            DesiredUnits = _desiredUnits;
            LengthLimit = _lengthLimit;
            EditingAllowed = _editingAllowed;
            EditingActive = _editingActive;

            // Render the drawing
            RenderDrawing();
        }

        private void DestroySegmentMeasurementTexts()
        {
            MeasurementText[] segmentTexts = measurementSegmentTexts;
            foreach (MeasurementText segmentText in segmentTexts)
            {
                segmentText.gameObject.SetActive(false);
                Destroy(segmentText);
            }
            _measurementSegmentTexts.Clear();
        }

        protected override void MRETOnDestroy()
        {
            base.MRETOnDestroy();

            Destroy(_drawingEditController);
            Destroy(_measurementText);
            DestroySegmentMeasurementTexts();
            Destroy(pointVisualContainer);
        }
        #endregion MRETUpdateBehaviour

        #region IInteractable3dDrawing
        /// <seealso cref="IInteractable3dDrawing.RenderType"/>
        public abstract DrawingRender3dType RenderType { get; }

        /// <seealso cref="IInteractable3dDrawing.DesiredUnits"/>
        public LengthUnitType DesiredUnits
        {
            get => _desiredUnits;
            set
            {
                _desiredUnits = value;
                if (!initializing)
                {
                    RenderDrawing();
                }
            }
        }
        private LengthUnitType _desiredUnits;

        /// <seealso cref="IInteractable3dDrawing.DisplayMeasurement"/>
        public bool DisplayMeasurement
        {
            get => _displayMeasurement;
            set
            {
                _displayMeasurement = value;
                if (!initializing)
                {
                    RenderDrawing();
                }
            }
        }
        private bool _displayMeasurement;

        /// <seealso cref="IInteractable3dDrawing.DisplayMeasurement"/>
        public bool DisplaySegmentMeasurements
        {
            get => _displaySegmentMeasurements;
            set
            {
                _displaySegmentMeasurements = value;
                if (!initializing)
                {
                    RenderDrawing();
                }
            }
        }
        private bool _displaySegmentMeasurements;

        /// <seealso cref="IInteractable3dDrawing.EditingActive"/>
        public bool EditingActive
        {
            get
            {
                return !initializing
                    ? (_drawingEditController != null) && _drawingEditController.menuEnabled
                    : _editingActive;
            }
            set
            {
                _editingActive = value && EditingAllowed;
                if (!initializing)
                {
                    RefreshState();
                }
            }
        }
        [SerializeField]
        private bool _editingActive = false;

        /// <seealso cref="IInteractable3dDrawing.EditingAllowed"/>
        public bool EditingAllowed
        {
            get => _editingAllowed;
            set
            {
                _editingAllowed = value;
                if (!initializing)
                {
                    RefreshState();
                }
            }
        }
        [SerializeField]
        private bool _editingAllowed = true;

        /// <seealso cref="IInteractable3dDrawing.LengthLimit"/>
        public virtual float LengthLimit
        {
            get => _lengthLimit;
            set
            {
                if (value != _lengthLimit)
                {
                    _lengthLimit = value;
                    RenderDrawing();
                }
            }
        }
        public float _lengthLimit;
        #endregion IInteractable3dDrawing

        #region Serializable
        /// <seealso cref="InteractableDrawing{T}.Deserialize(T, SerializationState)"/>
        protected override IEnumerator Deserialize(Drawing3dType serialized, SerializationState deserializationState)
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
            serializedDrawing = serialized;

            // Deserialize the 3D drawing
            DesiredUnits = serializedDrawing.Units;
            DisplayMeasurement = serializedDrawing.DisplayMeasurement;
            DisplaySegmentMeasurements = serializedDrawing.DisplaySegmentMeasurements;
            float deserializedLimitLength = Interactable3dDrawingDefaults.LIMIT_LENGTH;
            if (serializedDrawing.LengthLimit != null)
            {
                SchemaUtil.DeserializeLength(serializedDrawing.LengthLimit, ref deserializedLimitLength);
                if (float.IsInfinity(deserializedLimitLength) || float.IsNaN(deserializedLimitLength) || (deserializedLimitLength < 0))
                {
                    deserializedLimitLength = Interactable3dDrawingDefaults.LIMIT_LENGTH;
                }
            }
            LengthLimit = deserializedLimitLength;

            // Force a rendering of the drawing
            RenderDrawing();

            // Record the deserialization state as complete
            deserializationState.complete = true;

            yield return null;
        }

        /// <seealso cref="InteractableDrawing{T}.Serialize(T, SerializationState)"/>
        protected override IEnumerator Serialize(Drawing3dType serialized, SerializationState serializationState)
        {
            StartCoroutine(base.Serialize(serialized, serializationState));

            // Wait for the coroutine to complete
            while (!serializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) yield break;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Serialize the 3D drawing
            serialized.Type = RenderType;
            serialized.Units = DesiredUnits;
            serialized.DisplayMeasurement = DisplayMeasurement;
            serialized.DisplaySegmentMeasurements = DisplaySegmentMeasurements;

            // Serialize the length limit
            LengthType serializedLengthLimit = null;
            if (!float.IsInfinity(LengthLimit) && !float.IsNaN(LengthLimit) && (LengthLimit >= 0))
            {
                // Use the original serialized length limit if it exists
                if (serializedDrawing != null)
                {
                    serializedLengthLimit = serializedDrawing.LengthLimit;
                }

                // Make sure we have a valid serialized field
                if (serializedLengthLimit == null)
                {
                    /// Use the default serialized type
                    serializedLengthLimit = new LengthType();
                }
                SchemaUtil.SerializeLength(LengthLimit, serializedLengthLimit);
            }
            serialized.LengthLimit = serializedLengthLimit;

            // Save the final serialized reference
            serializedDrawing = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;

            yield return null;
        }
        #endregion Serializable

        #region InteractableDrawing
        /// <summary>
        /// Available to subclasses to perform an action just before redering the drawing
        /// </summary>
        protected virtual void BeforeRendering()
        {
        }

        /// <summary>
        /// Available to subclasses to perform an action after rendering the drawing
        /// </summary>
        protected virtual void AfterRendering()
        {
        }

        /// <seealso cref="InteractableDrawing{T}.RenderDrawing"/>
        protected sealed override void RenderDrawing()
        {
            if (initializing) return;

            // Refresh the state
            RefreshState();

            // Notify subclasses we are about to render
            BeforeRendering();

            // Perform rendering

            // Total measurement text
            if (DisplayMeasurement)
            {
                measurementText.SetValue(length, remainingLength);
            }

            // Segment measurement texts
            if (DisplaySegmentMeasurements)
            {
                Vector3[] pointsCopy = points;
                for (int i = 1; i < pointsCopy.Length; i++)
                {
                    MeasurementText segmentText = _measurementSegmentTexts[i - 1];

                    segmentText.SetValue(Math.Abs(Vector3.Distance(points[i], points[i - 1])), -1);
                }
            }

            // Notify subclasses we are finished rendering
            AfterRendering();

            // Update the drawing colliders
            UpdateDrawingColliders();
        }

        /// <summary>
        /// Sets the color of the drawing. Available for subclasses to derive behavior.
        /// </summary>
        /// <param name="color">The drawing <code>Color32</code></param>
        protected override void SetColor(Color32 color)
        {
            base.SetColor(color);

            RefreshState();
        }
        #endregion InteractableDrawing

        /// <summary>
        /// Updates the drawing colliders.
        /// </summary>
        protected abstract void UpdateDrawingColliders();

        /// <summary>
        /// Refreshes the internal state of this drawing
        /// </summary>
        public virtual void RefreshState()
        {
            if (initializing) return;

            if (drawingEditController != null)
            {
                if (EditingActive)
                {
                    drawingEditController.RefreshState();
                    drawingEditController.EnableMenu();
                }
                else
                {
                    drawingEditController.DisableMenu();
                }

                if (EditingAllowed)
                {
                    drawingEditController.ShowButton();
                }
                else
                {
                    drawingEditController.HideButton();
                }
            }

            if (measurementText != null)
            {
                measurementText.gameObject.SetActive(DisplayMeasurement);
                measurementText.transform.localPosition = center + new Vector3(0f, 0.1f, 0f);
                measurementText.units = DesiredUnits;
            }

            // Make sure we have the correct number of segment measurement texts
            Vector3[] pointsCopy = points;
            int numSegments = ((pointsCopy.Length - 1) >= 0) ? (pointsCopy.Length - 1) : 0;
            if (_measurementSegmentTexts.Count > numSegments)
            {
                // Remove the extra
                _measurementSegmentTexts.RemoveRange(numSegments, _measurementSegmentTexts.Count - numSegments);
            }
            else if (_measurementSegmentTexts.Count < numSegments)
            {
                // Add the missing segments
                do
                {
                    _measurementSegmentTexts.Add(CreateMeasurementText());
                } while (_measurementSegmentTexts.Count < numSegments);
            }

            // Update the state of each segment text
            for (int i = 1; i < pointsCopy.Length; i++)
            {
                MeasurementText segmentText = _measurementSegmentTexts[i - 1];

                segmentText.gameObject.SetActive(DisplaySegmentMeasurements);
                segmentText.transform.localPosition = Vector3.Lerp(pointsCopy[i], pointsCopy[i - 1], 0.5f) + new Vector3(0f, 0.1f, 0f);
                segmentText.units = DesiredUnits;
            }
        }

        /// <summary>
        /// Set up the measurement text
        /// </summary>
        protected virtual MeasurementText CreateMeasurementText()
        {
            GameObject measurementObj = new GameObject("Measurement");
            measurementObj.transform.SetParent(transform);
            measurementObj.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
            measurementObj.AddComponent<Canvas>();
            MeasurementText resultMeasurementText = measurementObj.AddComponent<MeasurementText>();
            resultMeasurementText.text = measurementObj.AddComponent<TMPro.TextMeshProUGUI>();
            resultMeasurementText.text.enableAutoSizing = true;
            return resultMeasurementText;
        }

        private void CreateEditController()
        {
            GameObject drawingEditObject = Instantiate(ProjectManager.DrawingManager.drawingEditPrefab);
            drawingEditObject.transform.SetParent(transform);
            drawingEditObject.transform.localPosition = Vector3.zero;
            drawingEditObject.transform.localRotation = Quaternion.identity;
            _drawingEditController = drawingEditObject.GetComponent<DrawingEditController>();
            if (_drawingEditController == null)
            {
                LogWarning("Unable to set up drawing edit controller.", nameof(CreateEditController));
                return;
            }
        }

        /// <summary>
        /// Initializes the drawing editing to the default state
        /// </summary>
        public void InitializeEditController()
        {
            if (drawingEditController != null)
            {
                drawingEditController.currentDrawing = this;
                drawingEditController.Initialize(_editingActive);
            }
        }

        /// <summary>
        /// Enables the endpoint visuals.
        /// </summary>
        public void EnableEndpointVisuals()
        {
            if (endpointVisuals != null)
            {
                foreach (KeyValuePair<Vector3, GameObject> endpointVisual in endpointVisuals)
                {
                    Destroy(endpointVisual.Value);
                }
            }
            endpointVisuals = new Dictionary<Vector3, GameObject>();
            Vector3[] linePoints = points.Distinct().ToArray();
            if (linePoints == null || linePoints.Length < 1)
            {
                LogError("No points.", nameof(EnableEndpointVisuals));
                return;
            }
            if (linePoints.Length > 0)
            {
                float pointDiameter = width * DEFAULT_ENDPOINTVISUALSCALEMULTIPLIER;
                // Create first point.
                endpointVisuals.Add(linePoints[0], SetUpEndPoint(0, linePoints[0], pointDiameter));
                if (linePoints.Length > 1)
                {
                    // Create last point.
                    endpointVisuals.Add(linePoints[linePoints.Length - 1], SetUpEndPoint(
                        linePoints.Length - 1, linePoints[linePoints.Length - 1], pointDiameter));
                }
            }
        }

        /// <summary>
        /// Disables the endpoint visuals.
        /// </summary>
        public void DisableEndpointVisuals()
        {
            if (endpointVisuals != null)
            {
                foreach (KeyValuePair<Vector3, GameObject> endpointVisual in endpointVisuals)
                {
                    Destroy(endpointVisual.Value);
                }
            }
            endpointVisuals = null;
        }

        /// <summary>
        /// Enables the mid-point visuals.
        /// </summary>
        /// <param name="numberOfPoints">Number of points to visualize.</param>
        public void EnableMidpointVisuals(int numberOfPoints)
        {
            Vector3[] drawingPoints = points.Distinct().ToArray();
            if (drawingPoints.Length < 3)
            {
                LogWarning("No mid-points", nameof(EnableMidpointVisuals));
                return;
            }
            if (numberOfPoints > drawingPoints.Length - 2)
            {
                LogWarning("Desired number of points exceeds actual", nameof(EnableMidpointVisuals));
                numberOfPoints = drawingPoints.Length - 2;
            }
            if (midpointVisuals != null)
            {
                foreach (KeyValuePair<Vector3, GameObject> midpointVisual in midpointVisuals)
                {
                    Destroy(midpointVisual.Value);
                }
            }
            midpointVisuals = new Dictionary<Vector3, GameObject>();
            float pointDiameter = width * DEFAULT_MIDPOINTVISUALSCALEMULTIPLIER;
            float pointsPerVisual = drawingPoints.Length / numberOfPoints;
            for (float i = 1; i < drawingPoints.Length - 2; i += pointsPerVisual)
            {
                int roundI = Mathf.RoundToInt(i);
                if (roundI <= 0 || roundI >= drawingPoints.Length - 1)
                {
                    continue;
                }
                // Tyler Kitts: I had to change this from a .add to [key] = value since adding a point with
                // the same midpoint would cause an error
                midpointVisuals[points[roundI]] = SetUpMidPoint(roundI, drawingPoints[roundI], pointDiameter);
            }
        }

        /// <summary>
        /// Disables the mid-point visuals.
        /// </summary>
        public void DisableMidpointVisuals()
        {
            if (midpointVisuals != null)
            {
                foreach (KeyValuePair<Vector3, GameObject> midpointVisual in midpointVisuals)
                {
                    Destroy(midpointVisual.Value);
                }
            }
            midpointVisuals = null;
        }

        /// <summary>
        /// Sets up an endpoint.
        /// </summary>
        /// <param name="index">Index of the endpoint.</param>
        /// <param name="position">Position to give endpoint.</param>
        /// <param name="diameter">Diameter to give endpoint.</param>
        /// <returns>Endpoint game object.</returns>
        private GameObject SetUpEndPoint(int index, Vector3 position, float diameter)
        {
            GameObject visual = Instantiate(ProjectManager.DrawingManager.drawingEndVisualPrefab, pointVisualContainer.transform);
            visual.name = "EndPoint";
            visual.transform.localPosition = position;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = new Vector3(diameter, diameter, diameter);
            DrawingPointVisual drawingPointVisual = visual.GetComponent<DrawingPointVisual>();
            if (drawingPointVisual == null)
            {
                LogError("Error loading point visual", nameof(SetUpEndPoint));
                return null;
            }
            Action<Vector3, InputHand> pointChangeAction = (Vector3 newPoint, InputHand hand) => {
                ReplacePoint(index, transform.InverseTransformPoint(newPoint));
                visual.transform.position = newPoint;
            };
            Action<Vector3, InputHand> pointChangeEndAction = (Vector3 newPoint, InputHand hand) => {

            };
            Action<Vector3, InputHand> pointAppendAction = (Vector3 newPoint, InputHand hand) => {
                LineDrawingController ldc = hand.GetComponent<LineDrawingController>();
                if (ldc == null)
                {
                    LogError("No drawing edit controller", nameof(SetUpEndPoint));
                }
                else if (ldc.touchingVisual.drawing == this)
                {
                    if (index == 0)
                    {
                        ldc.AddToLine(drawingPointVisual.drawing, true);
                    }
                    else
                    {
                        ldc.AddToLine(drawingPointVisual.drawing, false);
                    }
                    visual.transform.position = newPoint;
                }
            };
            Action<Vector3, InputHand, DrawingPointVisual> pointAppendEndAction =
                (Vector3 newPoint, InputHand hand, DrawingPointVisual touchingVisual) => {
                    LineDrawingController ldc = hand.GetComponent<LineDrawingController>();
                    if (ldc == null)
                    {
                        LogError("No drawing edit controller", nameof(SetUpEndPoint));
                    }
                    else
                    {
                        ldc.FinishLine();
                        if ((touchingVisual != null) && ((UnityEngine.Object)touchingVisual.drawing != this))
                        {
                            AppendDrawing(touchingVisual.drawing, !touchingVisual.isFirstPoint, index == 0);
                            Destroy(touchingVisual.drawing.gameObject);
                        }
                        DisableEndpointVisuals();
                        EnableEndpointVisuals();
                    }
                };
            drawingPointVisual.Initialize(pointChangeAction, pointChangeEndAction,
                pointAppendAction, pointAppendEndAction, true, index == 0, this);
            return visual;
        }

        /// <summary>
        /// Sets up a midpoint.
        /// </summary>
        /// <param name="index">Index of the midpoint.</param>
        /// <param name="position">Position to give midpoint.</param>
        /// <param name="diameter">Diameter to give midpoint.</param>
        /// <returns>Midpoint game object.</returns>
        private GameObject SetUpMidPoint(int index, Vector3 position, float diameter)
        {
            GameObject visual = Instantiate(ProjectManager.DrawingManager.drawingPointVisualPrefab);
            visual.name = "Midpoint";
            visual.transform.SetParent(pointVisualContainer.transform);
            visual.transform.localPosition = position;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = new Vector3(diameter, diameter, diameter);
            DrawingPointVisual drawingPointVisual = visual.GetComponent<DrawingPointVisual>();
            if (drawingPointVisual == null)
            {
                LogError("Error loading point visual", nameof(SetUpMidPoint));
                return null;
            }
            Action<Vector3, InputHand> pointChangeAction = (Vector3 newPoint, InputHand hand) => {
                /*int i = index - 1;
                bool restructuredLine = false;
                while (i >= 0 && points[i] != null)
                {
                    if (midpointVisuals.ContainsKey(points[i]))
                    {
                        // Key exists, finish finding previous point.
                        break;
                    }
                    else
                    {
                        midpointVisuals.Remove(points[i]);
                        RemovePoint(i);
                        restructuredLine = true;
                    }
                    i--;
                }
                i = index + 1;
                while (i < points.Length && points[i] != null)
                {
                    if (midpointVisuals.ContainsKey(points[i]))
                    {
                        // Key exists, finish finding next point.
                        break;
                    }
                    else
                    {
                        midpointVisuals.Remove(points[i]);
                        RemovePoint(i);
                        restructuredLine = true;
                    }
                    i++;
                }
                if (restructuredLine)
                {
                    int numPointVisuals = midpointVisuals.Count;
                    DisableMiddlePointVisuals();
                    EnableMiddlePointVisuals(numPointVisuals);
                }
                else
                {*/
                ReplacePoint(index, transform.InverseTransformPoint(newPoint));
                drawingPointVisual.transform.position = newPoint;
                //}
            };
            Action<Vector3, InputHand> pointChangeEndAction = (Vector3 newPoint, InputHand hand) => {

            };
            Action<Vector3, InputHand> pointAppendAction = (Vector3 newPoint, InputHand hand) => {

            };
            Action<Vector3, InputHand, DrawingPointVisual> pointAppendEndAction =
                (Vector3 newPoint, InputHand hand, DrawingPointVisual touchingVisual) => {

                };
            drawingPointVisual.Initialize(pointChangeAction, pointChangeEndAction,
                pointAppendAction, pointAppendEndAction, false, false, this);
            return visual;
        }

        #region InteractableSceneObject
        /// <seealso cref="InteractableSceneObject{T}.BeginTouchHold(InputHand)"/>
        protected override void BeginTouchHold(InputHand hand)
        {
            base.BeginTouchHold(hand);
            if ((drawingEditController != null) && EditingAllowed)
            {
                drawingEditController.ShowButton();
            }
        }

        /// <seealso cref="InteractableSceneObject{T}.Unuse(InputHand)"/>
        public override void Unuse(InputHand hand)
        {
            base.Unuse(hand);

            if (drawingEditController != null)
            {
                drawingEditController.HideButton();
                drawingEditController.DisableMenu();
            }
        }
        #endregion InteractableSceneObject

    }

    public class Interactable3dDrawingDefaults : InteractableDrawingDefaults
    {
        // TODO: We want to use the default values from the schema to keep in sync,
        // but width default not supported by MS XSD Schema Tool
        public static readonly float LIMIT_LENGTH = float.PositiveInfinity;
        public static readonly LengthUnitType DESIRED_UNITS = new Drawing3dType().Units;
        public static bool DISPLAY_MEASUREMENT = new Drawing3dType().DisplayMeasurement;
        public static bool DISPLAY_SEGMENT_MEASUREMENTS = new Drawing3dType().DisplaySegmentMeasurements;
    }
}
