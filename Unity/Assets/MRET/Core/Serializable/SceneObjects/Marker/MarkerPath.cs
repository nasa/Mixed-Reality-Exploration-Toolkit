// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GOV.NASA.GSFC.XR.Utilities.Math;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.Configuration;
using GOV.NASA.GSFC.XR.MRET.Project;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Marker
{
    /// <remarks>
    /// History:
    /// 1 January 9999: Created
    /// </remarks>
	///
	/// <summary>
	/// MarkerPath
	///
	/// TODO: Describe this class here...
	///
    /// Author: TODO
	/// </summary>
	/// 
	public class MarkerPath : InteractableSceneObject<MarkerPathType>
    {
		/// <seealso cref="MRETBehaviour.ClassName"/>
		public override string ClassName => nameof(MarkerPath);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private MarkerPathType serializedMarkerPath;

        /// <seealso cref="IInteractable.Grabbable"/>
        public new bool Grabbable
        {
            get => _grabbable;
            set
            {
                _grabbable = value;
                if (!initializing)
                {
                    _path.Grabbable = _grabbable;
                }
            }
        }
        private bool _grabbable;

        /// <seealso cref="IInteractable.Usable"/>
        public new bool Usable
        {
            get => _usable;
            set
            {
                _usable = value;
                if (!initializing)
                {
                    _path.Usable = _usable;
                }
            }
        }
        private bool _usable;

        /// <summary>
        /// The width of the path
        /// </summary>
        public float width
        {
            get => _width;
            set
            {
                _width = value;
                if (!initializing)
                {
                    _path.width = _width;
                }
            }
        }
        private float _width;

        /// <summary>
        /// Markers in the path
        /// </summary>
        public InteractableMarker[] PathMarkers
        {
            get => _pathMarkers.ToArray();
            set
            {
                _pathMarkers = new List<InteractableMarker>(value);
                _path.points = GetPoints(value);
            }
        }
        private List<InteractableMarker> _pathMarkers;

        /// <summary>
        /// The path <code>Material</code>
        /// </summary>
        public Material Material
        {
            get => _material;
            set
            {
                _material = value;
                if (!initializing)
                {
                    _path.Material = _material;
                }
            }
        }
        private Material _material;

        /// <summary>
        /// The path <code>Gradient</code>
        /// </summary>
        public Gradient Gradient
        {
            get => _gradient;
            set
            {
                _gradient = value;
                if (!initializing)
                {
                    _path.Gradient = _gradient;
                }
            }
        }
        private Gradient _gradient;

        /// <summary>
        /// The path <code>Color32</code>
        /// </summary>
        public Color32 Color
        {
            get => _color;
            set
            {
                _color = value;
                if (!initializing)
                {
                    _path.Color = _color;
                }
            }
        }
        private Color32 _color;

        /// <summary>
        /// Indicates the type of 3D drawing being rendered for the path
        /// </summary>
        public DrawingRender3dType RenderType
        {
            get => _renderType;
            set
            {
                if (value != _renderType)
                {
                    _renderType = value;
                    if (!initializing)
                    {
                        _path = ProjectManager.DrawingManager.CreateDrawing("Path", gameObject,
                            Vector3.zero, Quaternion.identity, Vector3.one,
                            RenderType, width, Color, GetPoints(PathMarkers),
                            float.PositiveInfinity, DisplayMeasurement);
                        _path.EditingAllowed = false;
                        _path.Material = Material;
                        _path.Gradient = Gradient;
                        _path.DesiredUnits = DesiredUnits;
                        _path.DisplaySegmentMeasurements = DisplaySegmentMeasurements;
                    }
                }
            }
        }
        private DrawingRender3dType _renderType;

        /// <summary>
        /// Indicates whether or not to display the total measurement of the path
        /// </summary>
        public bool DisplayMeasurement
        {
            get => _displayMeasurement;
            set
            {
                _displayMeasurement = value;
                if (!initializing)
                {
                    _path.DisplayMeasurement = _displayMeasurement;
                }
            }
        }
        private bool _displayMeasurement;

        /// <summary>
        /// Indicates whether or not to display the segment measurements of the path
        /// </summary>
        public bool DisplaySegmentMeasurements
        {
            get => _displaySegmentMeasurements;
            set
            {
                _displaySegmentMeasurements = value;
                if (!initializing)
                {
                    _path.DisplaySegmentMeasurements = _displaySegmentMeasurements;
                }
            }
        }
        private bool _displaySegmentMeasurements;

        /// <summary>
        /// The units to display if measurement displaying is enabled
        /// </summary>
        public LengthUnitType DesiredUnits
        {
            get => _desiredUnits;
            set
            {
                _desiredUnits = value;
                if (!initializing)
                {
                    _path.DesiredUnits = _desiredUnits;
                }
            }
        }
        private LengthUnitType _desiredUnits;

        /// <summary>
        /// The internal drawing for the path
        /// </summary>
        /// <seealso cref="IInteractable3dDrawing"/>
        private IInteractable3dDrawing _path;

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
			// Take the inherited behavior
			base.MRETAwake();

            // Initialize the internal components
            _pathMarkers = new List<InteractableMarker>();
            _path = ProjectManager.DrawingManager.CreateDrawing("MarkerPath", gameObject,
                Vector3.zero, Quaternion.identity, Vector3.one,
                RenderType, width, Color, new Vector3[0]);
            _path.EditingAllowed = false;

            // Initialize defaults (before serialization)
            RenderType = MarkerPathDefaults.RENDER_TYPE;
            width = MarkerPathDefaults.WIDTH;
            Material = new Material(MarkerPathDefaults.DRAWING_MATERIAL);
            Color = MarkerPathDefaults.COLOR;
            Gradient = MarkerPathDefaults.GRADIENT;
            DesiredUnits = MarkerPathDefaults.DESIRED_UNITS;
            DisplayMeasurement = MarkerPathDefaults.DISPLAY_MEASUREMENT;
            DisplaySegmentMeasurements = MarkerPathDefaults.DISPLAY_SEGMENT_MEASUREMENTS;
            Grabbable = MarkerPathDefaults.INTERACTABLE;
            Usable = MarkerPathDefaults.USABLE;
        }

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
		{
			// Take the inherited behavior
			base.MRETStart();

            // Mark initialization as complete
            initializing = false;

            // Custom initialization (after deserialization)

            // Force these properties to reinitialize with the current deserialized values
            RenderType = _renderType;
            width = _width;
            Material = _material; // Set the material before the color!
            Color = _color;
            Gradient = _gradient;
            DesiredUnits = _desiredUnits;
            DisplayMeasurement = _displayMeasurement;
            DisplaySegmentMeasurements = _displaySegmentMeasurements;
            Grabbable = _grabbable;
            Usable = _usable;
        }

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETUpdate()
        {
            base.MRETUpdate();

            // Make sure the marker positions haven't changed
            InteractableMarker[] currentMarkers = PathMarkers;
            Vector3[] pathPoints = _path.points;
            if (currentMarkers.Length == pathPoints.Length)
            {
                for (int i = 0; i < currentMarkers.Length; i++)
                {
                    Vector3 markerPosition = GetPoint(currentMarkers[i]);
                    if (!MathUtil.ApproximatelyEquals(pathPoints[i], markerPosition))
                    {
                        _path.ReplacePoint(i, markerPosition);
                    }
                }
            }
            else
            {
                // Internal error
                LogWarning("Internal error. Marker array length doesn't equal the drawing points length. Resetting...");
                PathMarkers = currentMarkers;
            }
        }
        #endregion MRETUpdateBehaviour

        /// <summary>
        /// Create a volumetric drawing.
        /// </summary>
        /// <param name="pathName">Name of the path.</param>
        /// <param name="renderType">The <code>DrawingRender3dType</code> of the path.</param>
        /// <param name="width">Width of the path.</param>
        /// <param name="markers">The array of <code>InteractableMarker</code> markers that make
        ///     up the points of the path.</param>
        /// <param name="showTotalMeasurement">Whether or not to show the total path measurement.</param>
        /// <param name="showSegmentMeasurements">Whether or not to show the measurements between markers.</param>
        /// <param name="color">Color of the drawing.</param>
        /// <returns>A <code>IInteractable3dDrawing</code> instance.</returns>
        public static MarkerPath Create(string pathName, DrawingRender3dType renderType, float width,
            InteractableMarker[] markers, bool showTotalMeasurement, bool showSegmentMeasurements,
            Color32 color)
        {
            GameObject pathGO = new GameObject(pathName);
            MarkerPath markerPath = pathGO.AddComponent<MarkerPath>();
            markerPath.RenderType = renderType;
            markerPath.PathMarkers = markers;
            markerPath.width = width;
            markerPath.Color = color;
            markerPath.DisplayMeasurement = showTotalMeasurement;
            markerPath.DisplaySegmentMeasurements = showSegmentMeasurements;

            return markerPath;
        }

        #region Serializable
        /// <seealso cref="InteractableSceneObject{T}.Deserialize(T, SerializationState)"/>
        protected override IEnumerator Deserialize(MarkerPathType serialized, SerializationState deserializationState)
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
            serializedMarkerPath = serialized;

            // Deserialize the path settings
            DesiredUnits = serializedMarkerPath.Units;
            DisplayMeasurement = serializedMarkerPath.DisplayMeasurement;
            DisplaySegmentMeasurements = serializedMarkerPath.DisplaySegmentMeasurements;

            // Deserialize the path width
            float deserializedWidth = MarkerPathDefaults.WIDTH;
            if (serializedMarkerPath.Width != null)
            {
                SchemaUtil.DeserializeLength(serializedMarkerPath.Width, ref deserializedWidth);
                if (float.IsInfinity(deserializedWidth) || float.IsNaN(deserializedWidth) || (deserializedWidth < 0))
                {
                    deserializedWidth = MarkerPathDefaults.WIDTH;
                }
            }
            width = deserializedWidth;

            // Deserialize the material (optional)
            if (serializedMarkerPath.Material != null)
            {
                // Perform the path material deserialization
                ObjectSerializationState<Material> materialDeserializationState = new ObjectSerializationState<Material>();
                StartCoroutine(DeserializeMaterial(serializedMarkerPath.Material, materialDeserializationState));

                // Wait for the coroutine to complete
                while (!materialDeserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // If the material failed, log a warning
                if (materialDeserializationState.IsError)
                {
                    LogWarning(materialDeserializationState.ErrorMessage);
                    materialDeserializationState.obj = null;
                }

                // Assign the path material if set
                if (materialDeserializationState.obj != null)
                {
                    Material = materialDeserializationState.obj;
                }
            }

            // Deserialize the color (optional)
            if (serializedMarkerPath.Item != null)
            {
                if (serializedMarkerPath.Item is ColorGradientType)
                {
                    Gradient gradient = new Gradient();
                    SchemaUtil.DeserializeGradient(serializedMarkerPath.Item as ColorGradientType, gradient);
                    Gradient = gradient;
                }
                else if (serializedMarkerPath.Item is ColorComponentsType)
                {
                    Color32 color = MarkerPathDefaults.COLOR;
                    SchemaUtil.DeserializeColorComponents(serializedMarkerPath.Item as ColorComponentsType, ref color);
                    Color = color;
                }
                else if (serializedMarkerPath.Item is ColorPredefinedType)
                {
                    Color32 color = MarkerPathDefaults.COLOR;
                    SchemaUtil.DeserializeColorPredefined((ColorPredefinedType)serializedMarkerPath.Item, ref color);
                    Color = color;
                }
            }

            // Clear the path
            ClearPath();

            // Markers

            // Build the paths
            if ((serializedMarkerPath.PathMarkers != null) && (serializedMarkerPath.PathMarkers.Length > 0))
            {
                // Need at least two markers for a path
                if (serializedMarkerPath.PathMarkers.Length < 2)
                {
                    deserializationState.Error("Invalid marker path defined. Two markers minimum for a path. Path length: " +
                        serializedMarkerPath.PathMarkers.Length);
                    yield break;
                }

                foreach (string markerId in serializedMarkerPath.PathMarkers)
                {
                    // Get the marker reference
                    IIdentifiable identifiable = MRET.UuidRegistry.GetByID(markerId);
                    if (!(identifiable is InteractableMarker))
                    {
                        deserializationState.Error("Invalid path marker defined. Marker ID does not exist: " + markerId);
                        yield break;
                    }

                    // Add the marker to the path
                    AddMarker(identifiable as InteractableMarker);
                }
            }

            // Record the deserialization state as complete
            deserializationState.complete = true;

            yield return null;
        }

        /// <seealso cref="InteractableSceneObject{T}.Serialize(T, SerializationState)"/>
        protected override IEnumerator Serialize(MarkerPathType serialized, SerializationState serializationState)
        {
            StartCoroutine(base.Serialize(serialized, serializationState));

            // Wait for the coroutine to complete
            while (!serializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) yield break;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Serialize the path

            // Width
            LengthUnitType units = (serializedMarkerPath != null) && (serializedMarkerPath.Width != null) ?
                serializedMarkerPath.Width.units :
                LengthUnitType.Meter;
            serialized.Width = new LengthType()
            {
                Value = SchemaUtil.UnityUnitsToLength(width, units),
                units = units
            };

            // Start with our internal serialized material to serialize out the material
            // using the original deserialized structure (if was provided during deserialization)
            MaterialType serializedPathMaterial = null;
            if (serializedMarkerPath != null)
            {
                // Use this material structure
                serializedPathMaterial = serializedMarkerPath.Material;

                // Only serialize if we have a valid serialized material reference
                if (serializedPathMaterial != null)
                {
                    // TODO: Serialize the material? Not sure what this means since we really just want
                    // to retain the structure defined in the original serialized form. We don't want to
                    // generate an entirely new serialized material without context to how it was defined.
                    //SchemaUtil.SerializeMaterial(ref serializedDrawingMaterial);
                }
            }
            serialized.Material = serializedPathMaterial;

            // Serialize the color (optional)
            object serializedColor = serialized.Item;
            if (serializedColor == null)
            {
                // Try to use this color structure from the deserialization process
                serializedColor = (serializedMarkerPath != null) ? serializedMarkerPath.Item : null;

                // Try the gradient first
                if (serializedColor is ColorGradientType)
                {
                    Gradient gradient = Gradient;
                    SchemaUtil.SerializeGradient(gradient, serializedColor as ColorGradientType);
                }
                else
                {
                    // Look for a color match or use the components
                    Color color = Color;
                    ColorPredefinedType serializedPredefinedColor = ColorPredefinedType.Black;
                    if (SchemaUtil.SerializeColorPredefined(color, ref serializedPredefinedColor))
                    {
                        serializedColor = serializedPredefinedColor;
                    }
                    else
                    {
                        ColorComponentsType serializedColorComponents = new ColorComponentsType();
                        SchemaUtil.SerializeColorComponents(color, serializedColorComponents);
                        serializedColor = serializedColorComponents;
                    }
                }
            }
            serialized.Item = serializedColor;

            // Serialize the path attributes
            serialized.Type = RenderType;
            serialized.Units = DesiredUnits;
            serialized.DisplayMeasurement = DisplayMeasurement;
            serialized.DisplaySegmentMeasurements = DisplaySegmentMeasurements;

            // Get all the valid marker IDs that make up the path
            List<string> markerIds = new List<string>();
            foreach (InteractableMarker marker in _pathMarkers)
            {
                markerIds.Add(marker.id);
            }

            // Serialize the markers
            if (markerIds.Count > 1)
            {
                serialized.PathMarkers = markerIds.ToArray();
            }
            else
            {
                serializationState.Error("Invalid number of markers for a path. Must be at least two: " +
                    markerIds.Count);
                yield break;
            }

            // Save the final serialized reference
            serializedMarkerPath = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;

            yield return null;
        }
        #endregion Serializable

        /// <summary>
        /// Gets a point from a marker
        /// </summary>
        /// <param name="marker">The <code>InteractableMarker</code> to convert</param>
        /// <returns>A <code>Vector3</code></returns>
        private Vector3 GetPoint(InteractableMarker marker)
        {
            return marker.transform.position;
        }

        /// <summary>
        /// Gets an array of points from an array of markers
        /// </summary>
        /// <param name="markers">The array of <code>InteractableMarker</code> to convert</param>
        /// <returns>An array of <code>Vector3</code></returns>
        private Vector3[] GetPoints(InteractableMarker[] markers)
        {
            List<Vector3> points = new List<Vector3>();
            foreach (InteractableMarker marker in markers)
            {
                points.Add(GetPoint(marker));
            }
            return points.ToArray();
        }

        /// <summary>
        /// Clears the path
        /// </summary>
        public void ClearPath()
        {
            _pathMarkers.Clear();
            _path.ClearDrawing();
        }

        /// <summary>
        /// Adds a marker to the end of the path
        /// </summary>
        /// <param name="marker">The <code>InteractableMarker</code> to add to the path</param>
        public void AddMarker(InteractableMarker marker)
        {
            InsertMarker(_pathMarkers.Count, marker);
        }

        /// <summary>
        /// Inserts a marker into the path
        /// </summary>
        /// <param name="index">Index to insert the marker</param>
        /// <param name="marker">The <code>InteractableMarker</code> to insert into to the path</param>
        public void InsertMarker(int index, InteractableMarker marker)
        {
            try
            {
                _pathMarkers.Insert(index, marker);
                _path.InsertPoint(index, GetPoint(marker));
            }
            catch (ArgumentOutOfRangeException e)
            {
                LogError("Invalid index: " + e, nameof(InsertMarker));
            }
        }

        /// <summary>
        /// Inserts markers into the path
        /// </summary>
        /// <param name="index">Starting index to insert the markers</param>
        /// <param name="markers">Array of <code>InteractableMarker</code> markers to insert</param>
        public void InsertMarkers(int index, InteractableMarker[] markers)
        {
            try
            {
                _pathMarkers.InsertRange(index, markers);
                _path.InsertPoints(index, GetPoints(markers));
            }
            catch (ArgumentOutOfRangeException e)
            {
                LogError("Invalid index: " + e, nameof(InsertMarkers));
            }
        }

        /// <summary>
        /// Removes a marker from the path
        /// </summary>
        /// <param name="marker">The <code>InteractableMarker</code> to remove from the path</param>
        /// <returns>A boolean indicating successful removal</returns>
        public bool RemoveMarker(InteractableMarker marker)
        {
            bool removed = false;

            int index = _pathMarkers.IndexOf(marker);
            if (index >= 0)
            {
                // Remove the marker
                removed = RemoveMarker(index);
            }
            else
            {
                LogError("Invalid marker", nameof(RemoveMarker));
            }

            return removed;
        }

        /// <summary>
        /// Removes a marker at the supplied index from the path
        /// </summary>
        /// <param name="index">The marker index to remove from the path</param>
        /// <returns>A boolean indicating successful removal</returns>
        public bool RemoveMarker(int index)
        {
            if (index >= _pathMarkers.Count || index < 0)
            {
                LogError("Invalid index", nameof(RemoveMarker));
                return false;
            }

            bool removed = false;
            try
            {
                // Remove the marker
                _pathMarkers.RemoveAt(index);
                _path.RemovePoint(index);

                removed = true;
            }
            catch (ArgumentOutOfRangeException e)
            {
                LogError("Invalid index: " + e, nameof(RemoveMarker));
            }

            return removed;
        }

        /// <summary>
        /// Replace a marker in the path
        /// </summary>
        /// <param name="index">Index of the marker to replace</param>
        /// <param name="marker">The <code>InteractableMarker</code> representing the new marker</param>
        public void ReplaceMarker(int index, InteractableMarker marker)
        {
            if (index >= _pathMarkers.Count || index < 0)
            {
                LogError("Invalid index.", nameof(ReplaceMarker));
                return;
            }

            // Replace the marker
            _pathMarkers[index] = marker;
            _path.ReplacePoint(index, GetPoint(marker));
        }

        /// <summary>
        /// Append a path to this path
        /// </summary>
        /// <param name="path">The <code>MarkerPath</code> to append.</param>
        /// <param name="appendBackwards">Whether or not to append points in reverse order.</param>
        /// <param name="appendToFront">Whether or not to append points to front</param>
        public void AppendPath(MarkerPath path, bool appendBackwards, bool appendToFront)
        {
            if (path == null)
            {
                LogError("Supplied path is null", nameof(AppendPath));
                return;
            }

            InteractableMarker[] appendMarkers = path.PathMarkers;
            if (appendToFront)
            {
                InteractableMarker[] originalMarkers = PathMarkers;
                for (int i = 0; i < originalMarkers.Length; i++)
                {
                    RemoveMarker(0);
                }

                if (appendBackwards)
                {
                    // Add last marker, don't add first marker
                    for (int i = appendMarkers.Length - 1; i > 0; i--)
                    {
                        AddMarker(appendMarkers[i]);
                    }
                }
                else
                {
                    // Add first marker, don't add last marker
                    for (int i = 0; i < appendMarkers.Length - 1; i++)
                    {
                        AddMarker(appendMarkers[i]);
                    }
                }

                foreach (InteractableMarker marker in originalMarkers)
                {
                    AddMarker(marker);
                }
            }
            else
            {
                InteractableMarker originalEndMarker = PathMarkers[PathMarkers.Length - 1];
                if (appendBackwards)
                {
                    // Add first marker, don't add last marker
                    for (int i = appendMarkers.Length - 2; i > -1; i--)
                    {
                        AddMarker(appendMarkers[i]);
                    }
                }
                else
                {
                    // Add last marker, don't add first marker
                    for (int i = 1; i < appendMarkers.Length; i++)
                    {
                        AddMarker(appendMarkers[i]);
                    }
                }
            }
        }

    }

    public class MarkerPathDefaults : InteractableDefaults
    {
        // TODO: We want to use the default values from the schema to keep in sync,
        // but width default not supported by MS XSD Schema Tool
        public static readonly DrawingRender3dType RENDER_TYPE = new MarkerPathType().Type;
        public static readonly float WIDTH = 0.01f;
        public static readonly Material DRAWING_MATERIAL = MRET.LineDrawingMaterial;
        public static readonly Color32 COLOR = MRET.LineDrawingMaterial.color;
        public static readonly string SHADER_NAME = "Universal Render Pipeline/Lit";
        public new static readonly bool INTERACTABLE = false;

        // Gradient
        private static readonly GradientColorKey[] COLOR_KEYS = new GradientColorKey[2]
        {
            new GradientColorKey()
            {
                color = COLOR,
                time = 0.0f
            },
            new GradientColorKey()
            {
                color = COLOR,
                time = 1.0f
            },
        };
        private static readonly GradientAlphaKey[] ALPHA_KEYS = new GradientAlphaKey[2]
        {
            new GradientAlphaKey()
            {
                alpha = COLOR.a,
                time = 0.0f
            },
            new GradientAlphaKey()
            {
                alpha = COLOR.a,
                time = 1.0f
            },
        };
        public static readonly Gradient GRADIENT = new Gradient()
        {
            colorKeys = COLOR_KEYS,
            alphaKeys = ALPHA_KEYS
        };
        public static readonly LengthUnitType DESIRED_UNITS = new MarkerPathType().Units;
        public static bool DISPLAY_MEASUREMENT = new MarkerPathType().DisplayMeasurement;
        public static bool DISPLAY_SEGMENT_MEASUREMENTS = new MarkerPathType().DisplaySegmentMeasurements;
    }
}
