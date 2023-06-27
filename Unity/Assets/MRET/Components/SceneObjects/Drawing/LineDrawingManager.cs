// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;
using GOV.NASA.GSFC.XR.MRET.Action;
using GOV.NASA.GSFC.XR.MRET.Project;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.Drawing;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing
{
    /// <remarks>
    /// History:
    /// 13 December 2021: Created
    /// </remarks>
    /// <summary>
    /// LineDrawingManager is a class that manages line drawings.
    /// Author: Dylan Z. Baker
    /// </summary>
	public class LineDrawingManager : MRETSerializableManager<LineDrawingManager>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(LineDrawingManager);

        /// <summary>
        /// Data manager key for drawing mode.
        /// </summary>
        public static readonly string ISDRAWINGFLAGKEY = "MRET.INTERNAL.DRAWING.ACTIVE";

        /// <summary>
        /// Threshold for when a drawing should show its edit menu.
        /// </summary>
        [Tooltip("Threshold for when a drawing should show its edit menu.")]
        public float drawingTouchThreshold = 1f; // TODO DZB: Should probably go somewhere else.

        /// <summary>
        /// Prefab for the drawing edit menu.
        /// </summary>
        [Tooltip("Prefab for the drawing edit menu.")]
        public GameObject drawingEditPrefab;

        /// <summary>
        /// Prefab to use for drawing panels.
        /// </summary>
        [Tooltip("Prefab to use for drawing panels.")]
        public GameObject drawingPanelPrefab;

        /// <summary>
        /// Prefab for the drawing end visual.
        /// </summary>
        [Tooltip("Prefab for the drawing end visual.")]
        public GameObject drawingEndVisualPrefab;

        /// <summary>
        /// Prefab for the drawing point visual.
        /// </summary>
        [Tooltip("Prefab for the drawing point visual.")]
        public GameObject drawingPointVisualPrefab;

        /// <summary>
        /// Prefab for a volumetric drawing segment.
        /// </summary>
        [Tooltip("Prefab for a volumetric drawing segment.")]
        public GameObject volumetricDrawingSegmentPrefab;

        /// <summary>
        /// Prefab for a volumetric drawing corner.
        /// </summary>
        [Tooltip("Prefab for a volumetric drawing corner.")]
        public GameObject volumetricDrawingCornerPrefab;

        /// <summary>
        /// Prefab to use for editing drawings.
        /// </summary>
        public static GameObject DrawingEditPrefab => Instance.drawingEditPrefab;

        /// <summary>
        /// Prefab to use for drawing panels.
        /// </summary>
        public static GameObject DrawingPanelPrefab => Instance.drawingPanelPrefab;

        /// <summary>
        /// Prefab to use for drawing panels.
        /// </summary>
        public static GameObject DrawingEndVisualPrefab => Instance.drawingEndVisualPrefab;

        /// <summary>
        /// Prefab to use for drawing panels.
        /// </summary>
        public static GameObject DrawingPointVisualPrefab => Instance.drawingPointVisualPrefab;

        /// <summary>
        /// Prefab to use for drawing panels.
        /// </summary>
        public static GameObject VolumetricDrawingSegmentPrefab => Instance.volumetricDrawingSegmentPrefab;

        /// <summary>
        /// Prefab to use for drawing panels.
        /// </summary>
        public static GameObject VolumetricDrawingCornerPrefab => Instance.volumetricDrawingCornerPrefab;

        /// <summary>
        /// The line drawing controllers.
        /// </summary>
        public List<LineDrawingController> lineDrawingControllers { get; private set; }

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

        /// <seealso cref="MRETManager{M}.Initialize"/>
        public override void Initialize()
        {
            // Take the inherited behavior
            base.Initialize();

            // Required prefabs
            if (drawingEditPrefab == null)
            {
                LogError("Fatal Error. " + nameof(drawingEditPrefab) + " is not assigned. Aborting...", nameof(Initialize));
                MRET.Quit();
            }
            if (drawingPanelPrefab == null)
            {
                LogError("Fatal Error. " + nameof(drawingPanelPrefab) + " is not assigned. Aborting...", nameof(Initialize));
                MRET.Quit();
            }
            if (drawingEndVisualPrefab == null)
            {
                LogError("Fatal Error. " + nameof(drawingEndVisualPrefab) + " is not assigned. Aborting...", nameof(Initialize));
                MRET.Quit();
            }
            if (drawingPointVisualPrefab == null)
            {
                LogError("Fatal Error. " + nameof(drawingPointVisualPrefab) + " is not assigned. Aborting...", nameof(Initialize));
                MRET.Quit();
            }
            if (volumetricDrawingCornerPrefab == null)
            {
                LogError("Fatal Error. " + nameof(volumetricDrawingCornerPrefab) + " is not assigned. Aborting...", nameof(Initialize));
                MRET.Quit();
            }
            if (volumetricDrawingSegmentPrefab == null)
            {
                LogError("Fatal Error. " + nameof(volumetricDrawingSegmentPrefab) + " is not assigned. Aborting...", nameof(Initialize));
                MRET.Quit();
            }

            lineDrawingControllers = new List<LineDrawingController>();
            foreach (InputHand hand in MRET.InputRig.hands)
            {
                LineDrawingController ldc = hand.GetComponent<LineDrawingController>();
                if (ldc != null)
                {
                    lineDrawingControllers.Add(ldc);
                }
            }
        }

        #region Serializable Instantiation
        /// <seealso cref="MRETSerializableManager{M}.GetDefaultSerializableContainer{T}(T)"/>
        protected override Transform GetDefaultSerializableContainer<T>(T serialized)
        {
            return ProjectManager.DrawingsContainer.transform;
        }

        /// <summary>
        /// Creates an interactable 3D drawing.
        /// </summary>
        /// <param name="drawingName">Name of the LineDrawing.</param>
        /// <param name="parent">Parent for the LineDrawing.</param>
        /// <param name="localPosition">Local position of the LineDrawing.</param>
        /// <param name="localRotation">Local rotation of the LineDrawing.</param>
        /// <param name="localScale">Local scale of the LineDrawing.</param>
        /// <param name="type">Type of the LineDrawing.</param>
        /// <param name="width">Width or diameter of the LineDrawing.</param>
        /// <param name="color">Color of the LineDrawing.</param>
        /// <param name="positions">Positions of points in the LineDrawing.</param>
        /// <returns>A <code>IInteractable3dDrawing</code> instance.</returns>
        /// <see cref="IInteractable3dDrawing"/>
        public IInteractable3dDrawing CreateDrawing(string drawingName,
            GameObject parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale,
            DrawingRender3dType type, float width, Color32 color,
            Vector3[] positions, float cutoff = float.PositiveInfinity, bool showMeasurement = false)
        {
            IInteractable3dDrawing newLineDrawing = null;
            switch (type)
            {
                case DrawingRender3dType.Basic:
                    newLineDrawing = LineDrawing.Create(
                        drawingName, width, color, positions, cutoff, showMeasurement);
                    break;

                case DrawingRender3dType.Volumetric:
                    newLineDrawing = VolumetricDrawing.Create(drawingName,
                        width, color, positions, cutoff, showMeasurement);
                    break;

                default:
                    LogError("Invalid drawing type.", nameof(CreateDrawing));
                    newLineDrawing = null;
                    break;
            }

            // Additional settings if valid reference
            if (newLineDrawing != null)
            {
                // Parent
                if (parent == null)
                {
                    newLineDrawing.transform.SetParent(ProjectManager.DrawingsContainer.transform);
                }
                else
                {
                    newLineDrawing.transform.SetParent(parent.transform);
                }

                // Transform
                newLineDrawing.transform.localPosition = localPosition;
                newLineDrawing.transform.localRotation = localRotation;
                newLineDrawing.transform.localScale = localScale;

                // Set the grab behavior
                newLineDrawing.GrabBehavior = ProjectManager.SceneObjectManager.GrabBehavior;

                // Record the action
                var serializedDrawing = newLineDrawing.CreateSerializedType();
                newLineDrawing.Serialize(serializedDrawing);
                ProjectManager.UndoManager.AddAction(
                    new AddSceneObjectAction(serializedDrawing),
                    new DeleteIdentifiableObjectAction(newLineDrawing.id));
            }

            return newLineDrawing;
        }

        /// <summary>
        /// Instantiates the line drawing from the supplied serialized drawing.
        /// </summary>
        /// <param name="serializedDrawing">The <code>Drawing3dType</code> class instance
        ///     containing the serialized representation of the drawing to instantiate</param>
        /// <param name="go">The optional <code>GameObject</code> that will contain the instantiated
        ///     drawing. If not provided, one will be created.</param>
        /// <param name="container">The parent container <code>Transform</code> for the instantiated
        ///     drawing. If null, the default project drawings container will be used.</param>
        /// <param name="onLoaded">The optional <code>Action</code> to be asynchronously triggered on
        ///     completion</param>
        /// <param name="finishDrawingInstantiation">The optional
        ///     <code>FinishSerializableInstantiationDelegate</code> method to be called to finish the
        ///     drawing instantiation. Called before the onLoaded action is called. If not specified, a
        ///     default logging behavior will be used.</param>
        /// <param name="context">Optional context parameters to be supplied to the
        ///     finishDrawingInstantiation method to provide additional context</param>
        protected void InstantiateLineDrawing(Drawing3dType serializedDrawing, GameObject go = null,
            Transform container = null, Action<LineDrawing> onLoaded = null,
            FinishSerializableInstantiationDelegate<Drawing3dType, LineDrawing> finishDrawingInstantiation = null,
            params object[] context)
        {
            // Instantiate and deserialize the drawing
            InstantiateSerializable(serializedDrawing, go, container, onLoaded,
                finishDrawingInstantiation, context);
        }

        /// <summary>
        /// Instantiates the volumetric drawing from the supplied serialized drawing.
        /// </summary>
        /// <param name="serializedDrawing">The <code>Drawing3dType</code> class instance
        ///     containing the serialized representation of the drawing to instantiate</param>
        /// <param name="go">The optional <code>GameObject</code> that will contain the instantiated
        ///     drawing. If not provided, one will be created.</param>
        /// <param name="container">The parent container <code>Transform</code> for the instantiated
        ///     drawing. If null, the default project drawings container will be used.</param>
        /// <param name="onLoaded">The optional <code>Action</code> to be asynchronously triggered on
        ///     completion</param>
        /// <param name="finishDrawingInstantiation">The optional
        ///     <code>FinishSerializableInstantiationDelegate</code> method to be called to finish the
        ///     drawing instantiation. Called before the onLoaded action is called. If not specified, a
        ///     default logging behavior will be used.</param>
        /// <param name="context">Optional context parameters to be supplied to the
        ///     finishDrawingInstantiation method to provide additional context</param>
        protected void InstantiateVolumetricDrawing(Drawing3dType serializedDrawing, GameObject go = null,
            Transform container = null, Action<VolumetricDrawing> onLoaded = null,
            FinishSerializableInstantiationDelegate<Drawing3dType, VolumetricDrawing> finishDrawingInstantiation = null,
            params object[] context)
        {
            // Instantiate and deserialize the drawing
            InstantiateSerializable(serializedDrawing, go, container, onLoaded,
                finishDrawingInstantiation, context);
        }

        /// <summary>
        /// Instantiates the drawing from the supplied serialized drawing.
        /// </summary>
        /// <param name="serializedDrawing">The <code>Drawing3dType</code> class instance
        ///     containing the serialized representation of the drawing to instantiate</param>
        /// <param name="go">The optional <code>GameObject</code> that will contain the instantiated
        ///     drawing. If not provided, one will be created.</param>
        /// <param name="container">The parent container <code>Transform</code> for the instantiated
        ///     drawing. If null, the default project drawings container will be used.</param>
        /// <param name="onLoaded">The optional <code>Action</code> to be asynchronously triggered on
        ///     completion</param>
        /// <param name="finishDrawingInstantiation">The optional
        ///     <code>FinishSerializableInstantiationDelegate</code> method to be called to finish the
        ///     drawing instantiation. Called before the onLoaded action is called. If not specified, a
        ///     default logging behavior will be used.</param>
        /// <param name="context">Optional context parameters to be supplied to the
        ///     finishDrawingInstantiation method to provide additional context</param>
        public void InstantiateDrawing(Drawing3dType serializedDrawing, GameObject go = null,
            Transform container = null, Action<IInteractable3dDrawing> onLoaded = null,
            FinishSerializableInstantiationDelegate<Drawing3dType, IInteractable3dDrawing> finishDrawingInstantiation = null,
            params object[] context)
        {
            // Instantiate and deserialize the drawing
            switch (serializedDrawing.Type)
            {
                // FIXME: We can't pass the finish delegate because the generic types don't match
                case DrawingRender3dType.Basic:
                    InstantiateLineDrawing(serializedDrawing, go, container, onLoaded);
                    break;
                case DrawingRender3dType.Volumetric:
                    InstantiateVolumetricDrawing(serializedDrawing, go, container, onLoaded);
                    break;

                default:
                    LogError("Invalid drawing type", nameof(InstantiateDrawing));
                    return;
            }
        }

        /// <summary>
        /// Instantiates an array of drawings from the supplied serialized array of drawings.
        /// </summary>
        /// <param name="serializedDrawings">The array of <code>Drawing3dType</code> class instances
        ///     containing the serialized representations of the drawings to instantiate.</param>
        /// <param name="go">The optional <code>GameObject</code> that will contain the instantiated
        ///     drawings. If not provided, one will be created for each drawing.</param>
        /// <param name="container">The parent container <code>Transform</code> for the instantiated
        ///     drawings. If null, the project drawings container will be used.</param>
        /// <param name="onLoaded">The optional <code>Action</code> to be asynchronously triggered on
        ///     completion</param>
        /// <param name="finishDrawingInstantiation">The optional
        ///     <code>FinishSerializableInstantiationDelegate</code> method to be called to finish each
        ///     drawing instantiation. Called for each instantiated drawing. If not specified,
        ///     a default logging behavior will be used.</param>
        /// <param name="context">Optional context parameters to be supplied to the
        ///     finishDrawingInstantiation method to provide additional context.</param>
        public void InstantiateDrawings(Drawing3dType[] serializedDrawings, GameObject go = null,
            Transform container = null, Action<IInteractable3dDrawing[]> onLoaded = null)
        {
            // Instantiate and deserialize
            InstantiateSerializables(serializedDrawings, go, container, onLoaded,
                InstantiateDrawing);
        }
        #endregion Serializable Instantiation

    }
}