// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.Collaboration;
using GOV.NASA.GSFC.XR.MRET.Configuration;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing
{
    /// <remarks>
    /// History:
    /// 10 Sep 2021: Created
    /// </remarks>
    ///
    /// <summary>
    /// Interactable drawing object
    ///
    /// Author: Jeffrey Hosler
    /// </summary>
    /// 
    public abstract class InteractableDrawing<T> : InteractableSceneObject<T>, IInteractableDrawing<T>
        where T : DrawingType, new()
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(InteractableDrawing<T>);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private T serializedDrawing;

        #region IInteractableDrawing
        /// <seealso cref="IInteractableDrawing.CreateSerializedType"/>
        DrawingType IInteractableDrawing.CreateSerializedType() => CreateSerializedType();

        /// <seealso cref="IInteractableDrawing.width"/>
        public float width
        {
            get => !initializing ? GetWidth() : _width;
            set
            {
                _width = value;
                if (!initializing)
                {
                    SetWidth(_width);
                }
            }
        }
        private float _width;

        /// <seealso cref="IInteractableDrawing.length"/>
        public float length
        {
            get
            {
                Vector3[] pointsCopy = points;
                float length = 0;
                for (int i = 1; i < pointsCopy.Length; i++)
                {
                    length += Math.Abs(Vector3.Distance(pointsCopy[i], pointsCopy[i - 1]));
                }
                return length;
            }
        }

        /// <seealso cref="IInteractableDrawing.center"/>
        public Vector3 center
        {
            get
            {
                Vector3[] pointsCopy = points;
                if (pointsCopy.Length < 1)
                {
                    return transform.position;
                }

                float minX = pointsCopy[0].x, minY = pointsCopy[0].y, minZ = pointsCopy[0].z,
                      maxX = pointsCopy[0].x, maxY = pointsCopy[0].y, maxZ = pointsCopy[0].z;
                foreach (Vector3 point in pointsCopy)
                {
                    if (point.x < minX)
                    {
                        minX = point.x;
                    }
                    else if (point.x > maxX)
                    {
                        maxX = point.x;
                    }

                    if (point.y < minY)
                    {
                        minY = point.y;
                    }
                    else if (point.y > maxY)
                    {
                        maxY = point.y;
                    }

                    if (point.z < minZ)
                    {
                        minZ = point.z;
                    }
                    else if (point.z > maxZ)
                    {
                        maxZ = point.z;
                    }
                }
                return Vector3.Lerp(new Vector3(minX, minY, minZ), new Vector3(maxX, maxY, maxZ), 0.5f);
            }
        }

        /// <summary>
        /// Points in the drawing.
        /// </summary>
        private List<Vector3> _points;

        /// <seealso cref="IInteractableDrawing.points"/>
        public Vector3[] points
        {
            get => _points.ToArray();
            set
            {
                _points = new List<Vector3>(value);
                RenderDrawing();
            }
        }

        /// <seealso cref="IInteractableDrawing.Material"/>
        public Material Material
        {
            get => !initializing ?  GetMaterial() : _material;
            set
            {
                _material = value;
                if (!initializing)
                {
                    SetMaterial(_material);
                }
            }
        }
        private Material _material;

        /// <seealso cref="IInteractableDrawing.Gradient"/>
        public Gradient Gradient
        {
            get => !initializing ? GetGradient() : _gradient;
            set
            {
                _gradient = value;
                if (!initializing)
                {
                    SetGradient(_gradient);
                }
            }
        }
        private Gradient _gradient;

        /// <seealso cref="IInteractableDrawing.ClearDrawing"/>
        public void ClearDrawing()
        {
            _points.Clear();
            RenderDrawing();
        }

        /// <seealso cref="IInteractableDrawing.AddPoint(Vector3)"/>
        public void AddPoint(Vector3 point)
        {
            InsertPoint(_points.Count, point);
        }

        /// <seealso cref="IInteractableDrawing.InsertPoint(int, Vector3)"/>
        public void InsertPoint(int index, Vector3 point)
        {
            try
            {
                _points.Insert(index, point);
                if (CanProcessSegments)
                {
                    if (index > 0)
                    {
                        RenderSegment(points[index - 1], points[index]);
                    }

                    if (index < points.Length - 1)
                    {
                        RenderSegment(points[index], points[index + 1]);
                    }
                }
                else
                {
                    RenderDrawing();
                }
            }
            catch (ArgumentOutOfRangeException e)
            {
                LogError("Invalid index: " + e, nameof(InsertPoint));
            }
        }

        /// <seealso cref="IInteractableDrawing.InsertPoint(int, Vector3)"/>
        public void InsertPoints(int index, Vector3[] points)
        {
            try
            {
                _points.InsertRange(index, points);
                RenderDrawing();
            }
            catch (ArgumentOutOfRangeException e)
            {
                LogError("Invalid index: " + e, nameof(InsertPoints));
            }
        }

        /// <seealso cref="IInteractableDrawing.RemovePoint(Vector3)"/>
        public bool RemovePoint(Vector3 point)
        {
            bool removed = false;

            int index = _points.IndexOf(point);
            if (index >= 0)
            {
                // Remove the point
                removed = RemovePoint(index);
            }
            else
            {
                LogError("Invalid point", nameof(RemovePoint));
            }

            return removed;
        }

        /// <seealso cref="IInteractableDrawing.RemovePoint(int)"/>
        public bool RemovePoint(int index)
        {
            if (index >= _points.Count || index < 0)
            {
                LogError("Invalid index", nameof(RemovePoint));
                return false;
            }

            bool removed = false;
            try
            {
                // Process segments if we can
                if (CanProcessSegments)
                {
                    if (index < _points.Count - 1)
                    {
                        // Point exists after this one.
                        if (index > 0)
                        {
                            // Point exists before this one.
                            RemoveSegment(_points[index - 1], _points[index]);
                            RemoveSegment(_points[index], _points[index + 1]);
                            RenderSegment(_points[index - 1], _points[index]);
                        }
                        else
                        {
                            // First point
                            RemoveSegment(_points[index], _points[index + 1]);
                        }
                    }
                    else
                    {
                        // Last point.
                        if (index > 0)
                        {
                            // Point exists before this one.
                            RemoveSegment(_points[index - 1], _points[index]);
                        }
                        else
                        {
                            // Only point.
                            // No segment to remove.
                        }
                    }
                }

                // Remove the point
                _points.RemoveAt(index);

                // Can't process segments so render the entire drawing
                if (!CanProcessSegments)
                {
                    RenderDrawing();
                }

                removed = true;
            }
            catch (ArgumentOutOfRangeException e)
            {
                LogError("Invalid index: " + e, nameof(RemovePoint));
            }

            return removed;
        }

        /// <seealso cref="IInteractableDrawing.ReplacePoint(int, Vector3)"/>
        public void ReplacePoint(int index, Vector3 value)
        {
            if (index >= _points.Count || index < 0)
            {
                LogError("Invalid index.", nameof(ReplacePoint));
                return;
            }

            // Process segments if we can
            if (CanProcessSegments)
            {
                if (index < _points.Count - 1)
                {
                    // Point exists after this one.
                    if (index > 0)
                    {
                        // Point exists before this one.
                        RemoveSegment(_points[index - 1], _points[index]);
                        RemoveSegment(_points[index], _points[index + 1]);
                        RenderSegment(_points[index - 1], value);
                        RenderSegment(value, points[index + 1]);
                    }
                    else
                    {
                        // First point.
                        RemoveSegment(_points[index], _points[index + 1]);
                        RenderSegment(value, _points[index + 1]);
                    }
                }
                else
                {
                    // Last point.
                    if (index > 0)
                    {
                        // Point exists before this one.
                        RemoveSegment(_points[index - 1], _points[index]);
                        RenderSegment(_points[index - 1], value);
                    }
                    else
                    {
                        // Only point.
                        // No segment to replace.
                    }
                }
            }

            // Replace the point
            _points[index] = value;

            // Can't process segments so render the entire drawing
            if (!CanProcessSegments)
            {
                RenderDrawing();
            }
        }

        /// <seealso cref="IInteractableDrawing.AppendDrawing(IInteractableDrawing, bool, bool)"/>
        public void AppendDrawing(IInteractableDrawing drawing, bool appendBackwards, bool appendToFront)
        {
            if (drawing == null)
            {
                LogError("Supplied drawing is null", nameof(AppendDrawing));
                return;
            }

            Vector3[] appendPoints = drawing.points;
            if (appendToFront)
            {
                Vector3[] originalDrawingPoints = points;
                for (int i = 0; i < originalDrawingPoints.Length; i++)
                {
                    RemovePoint(0);
                }

                if (appendBackwards)
                {
                    // Add last point, don't add first point
                    for (int i = appendPoints.Length - 1; i > 0; i--)
                    {
                        Vector3 newPoint = transform.InverseTransformPoint(
                            drawing.transform.TransformPoint(appendPoints[i]));
                        AddPoint(newPoint);
                    }
                }
                else
                {
                    // Add first point, don't add last point
                    for (int i = 0; i < appendPoints.Length - 1; i++)
                    {
                        Vector3 newPoint = transform.InverseTransformPoint(
                            drawing.transform.TransformPoint(appendPoints[i]));
                        AddPoint(newPoint);
                    }
                }

                foreach (Vector3 point in originalDrawingPoints)
                {
                    AddPoint(point);
                }
            }
            else
            {
                Vector3 originalEndPoint = points[points.Length - 1];
                if (appendBackwards)
                {
                    // Add first point, don't add last point.
                    for (int i = appendPoints.Length - 2; i > -1; i--)
                    {
                        Vector3 newPoint = transform.InverseTransformPoint(
                            drawing.transform.TransformPoint(appendPoints[i]));
                        AddPoint(newPoint);
                    }
                }
                else
                {
                    // Add last point, don't add first point.
                    for (int i = 1; i < appendPoints.Length; i++)
                    {
                        Vector3 newPoint = transform.InverseTransformPoint(
                            drawing.transform.TransformPoint(appendPoints[i]));
                        AddPoint(newPoint);
                    }
                }
            }
        }

        /// <seealso cref="IInteractableDrawing.Color"/>
        public Color32 Color
        {
            get => !initializing ? GetColor() : _color;
            set
            {
                _color = value;
                if (!initializing)
                {
                    SetColor(_color);
                }
            }
        }
        private Color32 _color;

        /// <summary>
        /// Gets the color of the drawing. Available for subclasses to derive behavior.
        /// </summary>
        /// <returns>The drawing <code>Color32</code></returns>
        protected virtual Color32 GetColor()
        {
            if (initializing) return _color;

            if (Material == null)
            {
                LogError("Material not initialized.", nameof(GetColor));
                return InteractableDrawingDefaults.COLOR;
            }

            if (Material.shader == null)
            {
                LogError("Shader not initialized.", nameof(GetColor));
                return InteractableDrawingDefaults.COLOR;
            }

            if (Material.shader.name != InteractableDrawingDefaults.SHADER_NAME)
            {
                LogError("Incorrect shader. Must be \'" + InteractableDrawingDefaults.SHADER_NAME + "\'.", nameof(GetColor));
                return InteractableDrawingDefaults.COLOR;
            }

            return Material.GetColor("_BaseColor");
        }

        /// <summary>
        /// Sets the color of the drawing. Available for subclasses to derive behavior.
        /// </summary>
        /// <param name="color">The drawing <code>Color32</code></param>
        protected virtual void SetColor(Color32 color)
        {
            if (initializing) return;

            if (Material == null)
            {
                LogError("Material not initialized.", nameof(SetColor));
                return;
            }

            if (Material.shader == null)
            {
                LogError("Shader not initialized.", nameof(SetColor));
                return;
            }

            if (Material.shader.name != InteractableDrawingDefaults.SHADER_NAME)
            {
                LogError("Incorrect shader. Must be \'" + InteractableDrawingDefaults.SHADER_NAME + "\'.", nameof(SetColor));
                return;
            }

            Material.SetColor("_BaseColor", color);
        }

        /// <seealso cref="IInteractableDrawing.Deserialize(DrawingType, Action{bool, string})"/>
        void IInteractableDrawing.Deserialize(DrawingType serialized, Action<bool, string> onFinished)
        {
            Deserialize(serialized as T, onFinished);
        }

        /// <seealso cref="IInteractableDrawing.Serialize(DrawingType, Action{bool, string})"/>
        void IInteractableDrawing.Serialize(DrawingType serialized, Action<bool, string> onFinished)
        {
            Serialize(serialized as T, onFinished);
        }
        #endregion IInteractableDrawing

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
            _points = new List<Vector3>();

            // Initialize defaults (before serialization)
            width = InteractableDrawingDefaults.WIDTH;
            Material = new Material(InteractableDrawingDefaults.DRAWING_MATERIAL);
            Color = InteractableDrawingDefaults.COLOR;
            Gradient = InteractableDrawingDefaults.GRADIENT;
        }

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            base.MRETStart();

            // Mark initialization as complete
            initializing = false;

            // Initialize defaults (after serialization)

            // Force these properties to reinitialize with the corrent deserialized values
            width = _width;
            Material = _material; // Set the material before the color!
            Color = _color;
            Gradient = _gradient;
        }
        #endregion MRETUpdateBehaviour

        #region Serializable
        /// <seealso cref="InteractableSceneObject{T}.Deserialize(T, SerializationState)"/>
        protected override IEnumerator Deserialize(T serialized, SerializationState deserializationState)
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

            // Deserialize the drawing width
            float deserializedWidth = InteractableDrawingDefaults.WIDTH;
            if (serializedDrawing.Width != null)
            {
                SchemaUtil.DeserializeLength(serializedDrawing.Width, ref deserializedWidth);
                if (float.IsInfinity(deserializedWidth) || float.IsNaN(deserializedWidth) || (deserializedWidth < 0))
                {
                    deserializedWidth = InteractableDrawingDefaults.WIDTH;
                }
            }
            width = deserializedWidth;

            // Deserialize the material (optional)
            if (serializedDrawing.Material != null)
            {
                // Perform the drawing material deserialization
                ObjectSerializationState<Material> materialDeserializationState = new ObjectSerializationState<Material>();
                StartCoroutine(DeserializeMaterial(serializedDrawing.Material, materialDeserializationState));

                // Wait for the coroutine to complete
                while (!materialDeserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // If the material failed, log a warning
                if (materialDeserializationState.IsError)
                {
                    LogWarning(materialDeserializationState.ErrorMessage);
                    materialDeserializationState.obj = null;
                }

                // Assign the drawing material if set
                if (materialDeserializationState.obj != null)
                {
                    Material = materialDeserializationState.obj;
                }
            }

            // Deserialize the color (optional)
            if (serializedDrawing.Item != null)
            {
                if (serializedDrawing.Item is ColorGradientType)
                {
                    Gradient gradient = new Gradient();
                    SchemaUtil.DeserializeGradient(serializedDrawing.Item as ColorGradientType, gradient);
                    Gradient = gradient;
                }
                else if (serializedDrawing.Item is ColorComponentsType)
                {
                    Color32 color = InteractableDrawingDefaults.COLOR;
                    SchemaUtil.DeserializeColorComponents(serializedDrawing.Item as ColorComponentsType, ref color);
                    Color = color;
                }
                else if (serializedDrawing.Item is ColorPredefinedType)
                {
                    Color32 color = InteractableDrawingDefaults.COLOR;
                    SchemaUtil.DeserializeColorPredefined((ColorPredefinedType)serializedDrawing.Item, ref color);
                    Color = color;
                }
            }

            // Clear the drawing
            ClearDrawing();

            // Add the drawing points
            foreach (PointType point in serializedDrawing.Points.Items)
            {
                AddPoint(new Vector3(point.X, point.Y, point.Z));
            }

            // Record the deserialization state as complete
            deserializationState.complete = true;

            yield return null;
        }

        /// <seealso cref="InteractableSceneObject{T}.Serialize(T, SerializationState)"/>
        protected override IEnumerator Serialize(T serialized, SerializationState serializationState)
        {
            StartCoroutine(base.Serialize(serialized, serializationState));

            // Wait for the coroutine to complete
            while (!serializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) yield break;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Width
            LengthUnitType units = (serializedDrawing != null) && (serializedDrawing.Width != null) ?
                serializedDrawing.Width.units :
                LengthUnitType.Meter;
            serialized.Width = new LengthType()
            {
                Value = SchemaUtil.UnityUnitsToLength(width, units),
                units = units
            };

            // Start with our internal serialized material to serialize out the material
            // using the original deserialized structure (if was provided during deserialization)
            MaterialType serializedDrawingMaterial = null;
            if (serializedDrawing != null)
            {
                // Use this material structure
                serializedDrawingMaterial = serializedDrawing.Material;

                // Only serialize if we have a valid serialized material reference
                if (serializedDrawingMaterial != null)
                {
                    // TODO: Serialize the material? Not sure what this means since we really just want
                    // to retain the structure defined in the original serialized form. We don't want to
                    // generate an entirely new serialized material without context to how it was defined.
                    //SchemaUtil.SerializeMaterial(ref serializedDrawingMaterial);
                }
            }
            serialized.Material = serializedDrawingMaterial;

            // Serialize the color (optional)
            object serializedColor = serialized.Item;
            if (serializedColor == null)
            {
                // Try to use this color structure from the deserialization process
                serializedColor = (serializedDrawing != null) ? serializedDrawing.Item : null;

                // Try the gradient first
                if (serializedColor is ColorGradientType)
                {
                    Gradient gradient = Gradient;
                    SchemaUtil.SerializeGradient(gradient, serializedColor as ColorGradientType);
                }
                else
                {
                    // Look for a color match or use the components
                    Color32 color = Color;
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

            // Serialize the points
            PointsType serializedPoints = new PointsType();
            Vector3[] drawingPoints = points;
            if ((drawingPoints != null) && (drawingPoints.Length > 0))
            {
                // Build a serialized list of points
                List<PointType> serializedPointList = new List<PointType>();
                foreach (Vector3 drawingPoint in drawingPoints)
                {
                    PointType serializedPoint = new PointType();
                    serializedPoint.X = drawingPoint.x;
                    serializedPoint.Y = drawingPoint.y;
                    serializedPoint.Z = drawingPoint.z;
                    serializedPointList.Add(serializedPoint);
                }
                // Assign the serialized array of points
                serializedPoints.Items = serializedPointList.ToArray();
            }
            serialized.Points = serializedPoints;

            // Save the final serialized reference
            serializedDrawing = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;

            yield return null;
        }
        #endregion Serializable

        /// <seealso cref="Identifiable{T}.CreateSynchronizedObject"/>
        protected override ISynchronized CreateSynchronizedObject()
        {
            return gameObject.AddComponent<SynchronizedDrawing>();
        }

        /// <summary>
        /// Called to get the width of the drawing
        /// </summary>
        /// <returns>The current width of the drawing</returns>
        public abstract float GetWidth();

        /// <summary>
        /// Called to set the width of the drawing
        /// </summary>
        /// <param name="width">The drawing width</param>
        public abstract void SetWidth(float width);

        /// <summary>
        /// Called to set the material to used for the drawing
        /// </summary>
        /// <param name="material">The <code>Material</code> to use for the drawing</param>
        public abstract void SetMaterial(Material material);

        /// <summary>
        /// Called to get the material being used for the drawing
        /// </summary>
        /// <returns>The <code>Material</code> being used for the drawing</returns>
        public abstract Material GetMaterial();

        /// <summary>
        /// Called to set the gradient to used for the drawing
        /// </summary>
        /// <param name="material">The <code>Gradient</code> to use for the drawing</param>
        public abstract void SetGradient(Gradient gradient);

        /// <summary>
        /// Called to get the gradient being used for the drawing
        /// </summary>
        /// <returns>The <code>Gradient</code> being used for the drawing</returns>
        public abstract Gradient GetGradient();

        /// <summary>
        /// Called to render the drawing
        /// </summary>
        protected abstract void RenderDrawing();

        /// <summary>
        /// Indicates whether or not the subclass is able to process segments of a drawing
        /// </summary>
        protected virtual bool CanProcessSegments => false;

        /// <summary>
        /// Remove a segment from the drawing between two points
        /// </summary>
        /// <param name="pointA">A <code>Vector3</code> representing the starting point of the segment</param>
        /// <param name="pointB">A <code>Vector3</code> representing the ending point of the segment</param>
        protected virtual void RemoveSegment(Vector3 pointA, Vector3 pointB)
        {

        }

        /// <summary>
        /// Renders a segment of the drawing between two points
        /// </summary>
        /// <param name="pointA">A <code>Vector3</code> representing the starting point of the segment</param>
        /// <param name="pointB">A <code>Vector3</code> representing the ending point of the segment</param>
        protected virtual void RenderSegment(Vector3 pointA, Vector3 pointB)
        {

        }
    }

    public class InteractableDrawingDefaults
    {
        // TODO: We want to use the default values from the schema to keep in sync,
        // but width default not supported by MS XSD Schema Tool
        public static readonly float WIDTH = 0.0025f;
        public static readonly Material DRAWING_MATERIAL = MRET.LineDrawingMaterial;
        public static readonly Color32 COLOR = MRET.LineDrawingMaterial.color;
        public static readonly string SHADER_NAME = "Universal Render Pipeline/Lit";

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
    }
}
