// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using UnityEngine;
using GOV.NASA.GSFC.XR.Utilities.Renderer;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.Configuration;
using GOV.NASA.GSFC.XR.MRET.Project;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Marker
{
    /// <remarks>
    /// History:
    /// 1 January 9999: Created
    /// </remarks>
	///
	/// <summary>
	/// InteractableMarker
	///
	/// TODO: Describe this class here...
	///
    /// Author: TODO
	/// </summary>
	/// 
	public class InteractableMarker : PhysicalSceneObject<MarkerType>
	{
		/// <seealso cref="MRETBehaviour.ClassName"/>
		public override string ClassName => nameof(InteractableMarker);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private MarkerType serializedMarker;

        /// <summary>
        /// The marker <code>Material</code>
        /// </summary>
        public Material Material
        {
            get => _material;
            set
            {
                _material = value;
                if (!initializing)
                {
                    RendererUtil.ApplyMaterial(gameObject, _material);
                }
            }
        }
        private Material _material;

        /// <summary>
        /// The marker <code>Gradient</code>
        /// </summary>
        public Gradient Gradient
        {
            get => _gradient;
            set
            {
                _gradient = value;
                if (!initializing)
                {
                    LogWarning("Gradient not implemented", nameof(Gradient));
                }
            }
        }
        private Gradient _gradient;

        /// <summary>
        /// The marker <code>Color</code>
        /// </summary>
        public Color32 Color
        {
            get => _color;
            set
            {
                _color = value;
                if (!initializing)
                {
                    RendererUtil.ApplyColor(gameObject, _color);
                }
            }
        }
        private Color32 _color;

        // IoT things
        public static GameObject selectedMarker;

        private GameObject headsetObject;

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

            // TODO: Custom initialization (before deserialization)
            Material = new Material(InteractableMarkerDefaults.DRAWING_MATERIAL);
            Color = InteractableMarkerDefaults.COLOR;
            Gradient = InteractableMarkerDefaults.GRADIENT;
        }

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
		{
			// Take the inherited behavior
			base.MRETStart();

            // Mark initialization as complete
            initializing = false;

            // TODO: Custom initialization (after deserialization)
            headsetObject = MRET.InputRig.head.gameObject;

            // Override the configuration panel for markers
            configurationPanelPrefab = ProjectManager.MarkerManager.markerConfigurationPanelPrefab;

            // Force these properties to reinitialize with the current deserialized values
            Material = _material; // Set the material before the color!
            Color = _color;
            Gradient = _gradient;
        }

        /// <seealso cref="MRETBehaviour.MRETUpdate"/>
        protected override void MRETUpdate()
        {
            base.MRETUpdate();

            // If we are placing the marker, keep the marker upright
            if (IsPlacing)
            {
                transform.rotation = Quaternion.LookRotation(Vector3.forward);
            }
        }
        #endregion MRETUpdateBehaviour

        #region Serialization
        /// <seealso cref="PhysicalSceneObject{T}.Deserialize(T, SerializationState)"/>
        protected override IEnumerator Deserialize(MarkerType serialized, SerializationState deserializationState)
        {
            StartCoroutine(base.Deserialize(serialized, deserializationState));

            // Wait for the coroutine to complete
            while (!deserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (deserializationState.IsError) yield break;

            // Start with an incomplete indicator
            deserializationState.Clear();

            // Process the object specific configuration

            // Save the serialized reference
            serializedMarker = serialized;

            // Deserialize the material (optional)
            if (serializedMarker.Material != null)
            {
                // Perform the material deserialization
                ObjectSerializationState<Material> materialDeserializationState = new ObjectSerializationState<Material>();
                StartCoroutine(DeserializeMaterial(serializedMarker.Material, materialDeserializationState));

                // Wait for the coroutine to complete
                while (!materialDeserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // If the material failed, log a warning
                if (materialDeserializationState.IsError)
                {
                    LogWarning(materialDeserializationState.ErrorMessage);
                    materialDeserializationState.obj = null;
                }

                // Assign the material if set
                if (materialDeserializationState.obj != null)
                {
                    Material = materialDeserializationState.obj;
                }
            }

            // Deserialize the color (optional)
            if (serializedMarker.Item != null)
            {
                if (serializedMarker.Item is ColorGradientType)
                {
                    Gradient gradient = new Gradient();
                    SchemaUtil.DeserializeGradient(serializedMarker.Item as ColorGradientType, gradient);
                    Gradient = gradient;
                }
                else if (serializedMarker.Item is ColorComponentsType)
                {
                    Color32 color = InteractableMarkerDefaults.COLOR;
                    SchemaUtil.DeserializeColorComponents(serializedMarker.Item as ColorComponentsType, ref color);
                    Color = color;
                }
                else if (serializedMarker.Item is ColorPredefinedType)
                {
                    Color32 color = InteractableMarkerDefaults.COLOR;
                    SchemaUtil.DeserializeColorPredefined((ColorPredefinedType)serializedMarker.Item, ref color);
                    Color = color;
                }
            }

            // Record the deserialization state as complete
            deserializationState.complete = true;

            yield return null;
        }

        /// <seealso cref="PhysicalSceneObject{T}.Serialize(T, SerializationState)"/>
        protected override IEnumerator Serialize(MarkerType serialized, SerializationState serializationState)
        {
            StartCoroutine(base.Serialize(serialized, serializationState));

            // Wait for the coroutine to complete
            while (!serializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) yield break;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Serialize the object specific configuration

            // Serialize the interactable marker

            // Start with our internal serialized material to serialize out the material
            // using the original deserialized structure (if was provided during deserialization)
            MaterialType serializedMaterial = null;
            if (serializedMarker != null)
            {
                // Use this material structure
                serializedMaterial = serializedMarker.Material;

                // Only serialize if we have a valid serialized material reference
                if (serializedMaterial != null)
                {
                    // TODO: Serialize the material? Not sure what this means since we really just want
                    // to retain the structure defined in the original serialized form. We don't want to
                    // generate an entirely new serialized material without context to how it was defined.
                    //SchemaUtil.SerializeMaterial(ref serializedDrawingMaterial);
                }
            }
            serialized.Material = serializedMaterial;

            // Serialize the color (optional)
            object serializedColor = serialized.Item;
            if (serializedColor == null)
            {
                // Try to use this color structure from the deserialization process
                serializedColor = (serializedMarker != null) ? serializedMarker.Item : null;

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

            // Save the final serialized reference
            serializedMarker = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;

            yield return null;
        }
        #endregion Serialization

        /// <summary>
        /// Create an interactable marker.
        /// </summary>
        /// <param name="markerName">Name of the marker</param>
        /// <param name="serializedMarker">The serialized representation of the marker model</param>
        /// <param name="markerPrefab">The marker prefab to instantiate</param>
        /// <param name="container">The container (parent) for the marker</param>
        /// <returns>The instantiated <code>InteractableMarker</code></returns>
        public static InteractableMarker Create(string markerName, ModelType serializedMarker,
            GameObject markerPrefab, Transform container = null)
        {
            // Make sure we have a valid container reference
            container = (container == null) ? ProjectManager.MarkersContainer.transform : container;

            // Instantiate the prefab
            GameObject markerGO = Instantiate(markerPrefab, container);
            markerGO.transform.localPosition = Vector3.zero;
            markerGO.transform.localRotation = Quaternion.identity;
            markerGO.transform.localScale = Vector3.one;

            // Get the interactable marker reference
            InteractableMarker marker = markerGO.GetComponent<InteractableMarker>();
            if (marker == null)
            {
                marker = markerGO.AddComponent<InteractableMarker>();
                // TODO: If we have to add the interactable marker, we need to set
                // the interactable marker references to the correct marker prefab
                // gameobjects to work.
            }

            // Generate a better ID for the marker for serialization
            marker.id = MRET.UuidRegistry.CreateUniqueIDFromName(markerGO.name);

            // Rename the game object
            markerGO.name = markerName;

            // Assign the model
            marker.SetModel(markerGO, serializedMarker);

            return marker;
        }
    }

    public class InteractableMarkerDefaults : PhysicalSceneObjectDefaults
    {
        // TODO: We want to use the default values from the schema to keep in sync,
        // but width default not supported by MS XSD Schema Tool
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
        public static MarkerReferenceBasisType REFERENCE_BASIS = new MarkerType().ReferenceBasis;
    }
}