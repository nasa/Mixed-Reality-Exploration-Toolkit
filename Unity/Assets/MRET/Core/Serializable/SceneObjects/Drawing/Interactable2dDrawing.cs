// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.Configuration;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing
{
    /// <remarks>
    /// History:
    /// 10 Sep 2021: Created
    /// </remarks>
    ///
    /// <summary>
    /// 2D drawing object
    ///
    /// Author: Jeffrey Hosler
    /// </summary>
    /// 
    public class Interactable2dDrawing : InteractableDrawing<Drawing2dType>, IInteractable2dDrawing<Drawing2dType>
    {
		/// <seealso cref="MRETBehaviour.ClassName"/>
		public override string ClassName => nameof(Interactable2dDrawing);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private Drawing2dType serializedDrawing;

        protected LineRenderer lineRenderer { get; private set; }

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

            // Make sure we have a line renderer
            lineRenderer = gameObject.GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
            }

            // Set the line renderer defaults
            lineRenderer.useWorldSpace = false;
        }
        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            base.MRETStart();

            // Set the defaults (after serialization)
        }
        #endregion MRETUpdateBehaviour

        #region Serializable
        /// <seealso cref="InteractableDrawing{T}.Deserialize(T, SerializationState)"/>
        protected override IEnumerator Deserialize(Drawing2dType serialized, SerializationState deserializationState)
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

            // Deserialize the 2D drawing (nothing unique yet from base)

            // Record the deserialization state as complete
            deserializationState.complete = true;

            yield return null;
        }

        /// <seealso cref="InteractableDrawing{T}.Serialize(T, SerializationState)"/>
        protected override IEnumerator Serialize(Drawing2dType serialized, SerializationState serializationState)
        {
            StartCoroutine(base.Serialize(serialized, serializationState));

            // Wait for the coroutine to complete
            while (!serializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) yield break;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Serialize the 2D drawing (nothing unique yet from base)

            // Save the final serialized reference
            serializedDrawing = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;

            yield return null;
        }
        #endregion Serializable

        #region InteractableDrawing
        /// <seealso cref="InteractableDrawing{T}.SetWidth"/>
        public override void SetWidth(float width)
        {
            lineRenderer.widthMultiplier = width;
        }

        /// <seealso cref="InteractableDrawing{T}.GetWidth"/>
        public override float GetWidth()
        {
            return lineRenderer.widthMultiplier;
        }

        /// <seealso cref="InteractableDrawing{T}.SetMaterial"/>
        public override void SetMaterial(Material material)
        {
            lineRenderer.material = material;
        }

        /// <seealso cref="InteractableDrawing{T}.GetMaterial"/>
        public override Material GetMaterial()
        {
            return lineRenderer.material;
        }

        /// <seealso cref="InteractableDrawing{T}.SetGradient"/>
        public override void SetGradient(Gradient gradient)
        {
            lineRenderer.colorGradient = gradient;
        }

        /// <seealso cref="InteractableDrawing{T}.GetGradient"/>
        public override Gradient GetGradient()
        {
            return lineRenderer.colorGradient;
        }

        /// <seealso cref="InteractableDrawing{T}.RenderDrawing"/>
        protected override void RenderDrawing()
        {
            Vector3[] pointsArray = points;
            lineRenderer.positionCount = pointsArray.Length;
            if (lineRenderer.positionCount > 0)
            {
                lineRenderer.SetPositions(pointsArray);
            }
        }
        #endregion InteractableDrawing
    }
}
