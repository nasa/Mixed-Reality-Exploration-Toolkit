// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.Project;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Marker
{
    /// <remarks>
    /// History:
    /// 1 January 2023: Created
    /// </remarks>
	///
	/// <summary>
	/// MarkerManager
	///
	/// Manages the markers in the project
	///
    /// Author: Sean Letavish/Jeff Hosler
	/// </summary>
	/// 
	public class MarkerManager : MRETSerializableManager<MarkerManager>
    {
		/// <seealso cref="MRETBehaviour.ClassName"/>
		public override string ClassName => nameof(MarkerManager);

        [Tooltip("Marker configuration panel prefab")]
        public GameObject markerConfigurationPanelPrefab;

        [Tooltip("List of all the available marker prefabs")]
        public List<MarkerPrefab> markerPrefabs = new List<MarkerPrefab>();

        [Tooltip("Data display prefab")]
        public GameObject dataDisplayPrefab;

        /// <summary>
        /// The active marker prefab used when creating new markers
        /// </summary>
        public MarkerPrefab ActiveMarkerPrefab
        {
            get => ((ActiveMarkerPrefabIndex >= 0) && (ActiveMarkerPrefabIndex < markerPrefabs.Count))
                ? markerPrefabs[ActiveMarkerPrefabIndex]
                : null;
        }

        /// <summary>
        /// The index of the active marker prefab to use when creating new markers
        /// </summary>
        public int ActiveMarkerPrefabIndex
        {
            get => _activeMarkerPrefabIndex;
            set
            {
                if ((value >=0) && (value < markerPrefabs.Count))
                {
                    _activeMarkerPrefabIndex = value;
                }
            }
        }
        private int _activeMarkerPrefabIndex;

        /// <summary>
        /// The color to assign new markers
        /// </summary>
        public Color32 ActiveMarkerColor
        {
            get => _activeMarkerColor;
            set
            {
                _activeMarkerColor = value;
                if (MarkerPathActive)
                {
                    ActiveMarkerPath.Color = value;
                }
            }
        }
        private Color32 _activeMarkerColor;

        /// <summary>
        /// The render type to use for paths when paths are enabled
        /// </summary>
        public DrawingRender3dType ActiveMarkerPathRenderType
        {
            get => _activeMarkerPathRenderType;
            set
            {
                _activeMarkerPathRenderType = value;
                if (MarkerPathActive)
                {
                    ActiveMarkerPath.RenderType = value;
                }
            }
        }
        private DrawingRender3dType _activeMarkerPathRenderType;

        /// <summary>
        /// The width of the lines to use for paths when paths are enabled
        /// </summary>
        public float ActiveMarkerPathWidth
        {
            get => _activeMarkerPathWidth;
            set
            {
                _activeMarkerPathWidth = value;
                if (MarkerPathActive)
                {
                    ActiveMarkerPath.width = value;
                }
            }
        }
        private float _activeMarkerPathWidth;

        /// <summary>
        /// Display the total measurement for paths when paths are enabled
        /// </summary>
        public bool ActiveMarkerPathDisplayTotalMeasurement
        {
            get => _activeMarkerPathDisplayTotalMeasurement;
            set
            {
                _activeMarkerPathDisplayTotalMeasurement = value;
                if (MarkerPathActive)
                {
                    ActiveMarkerPath.DisplayMeasurement = value;
                }
            }
        }
        private bool _activeMarkerPathDisplayTotalMeasurement;

        /// <summary>
        /// Display the segment measurements for paths when paths are enabled
        /// </summary>
        public bool ActiveMarkerPathDisplaySegmentMeasurements
        {
            get => _activeMarkerPathDisplaySegmentMeasurements;
            set
            {
                _activeMarkerPathDisplaySegmentMeasurements = value;
                if (MarkerPathActive)
                {
                    ActiveMarkerPath.DisplaySegmentMeasurements = value;
                }
            }
        }
        private bool _activeMarkerPathDisplaySegmentMeasurements;

        /// <summary>
        /// Indicates if pathing is enabled
        /// </summary>
        public bool MarkerPathActive
        {
            get => _activeMarkerPath != null;
        }

        /// <summary>
        /// Obtains a reference to the active <code>MarkerPath</code> when
        /// <code>MarkerPathActive</code> is enabled
        /// </summary>
        public MarkerPath ActiveMarkerPath
        {
            get => _activeMarkerPath;
        }
        private MarkerPath _activeMarkerPath;

        #region MRETUpdateManager
        /// <seealso cref="MRETBehaviour.IntegrityCheck"/>
        protected override IntegrityState IntegrityCheck()
		{
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            return (
                (state == IntegrityState.Failure) || // TODO: || (MyRequiredRef == null)
				(markerConfigurationPanelPrefab == null) ||
                (dataDisplayPrefab == null) ||
                (markerPrefabs.Count <= 0)
                    ? IntegrityState.Failure   // Fail is base class fails or anything is null
                    : IntegrityState.Success); // Otherwise, our integrity is valid
		}

        /// <seealso cref="MRETUpdateSingleton{T}.Initialize"/>
        public override void Initialize()
        {
            // Take the inherited behavior
            base.Initialize();

            if ((markerPrefabs == null) || (markerPrefabs.Count <= 0))
            {
                LogError("Fatal Error. Marker prefabs are not assigned. Aborting...", nameof(Initialize));
                MRET.Quit();
            }
            else
            {
                // Make sure the prefab references are valid
                foreach (MarkerPrefab markerPrefab in markerPrefabs)
                {
                    if (markerPrefab.prefab == null)
                    {
                        LogError("Fatal Error. Marker prefab is not assigned for marker '" +
                            markerPrefab.name + "'. Aborting...", nameof(Initialize));
                        MRET.Quit();
                    }
                }
            }

            // TODO: Custom initialization (before deserialization)
            _activeMarkerPrefabIndex = -1;
            _activeMarkerColor = InteractableMarkerDefaults.COLOR;
            _activeMarkerPath = null;
            _activeMarkerPathRenderType = MarkerPathDefaults.RENDER_TYPE;
            _activeMarkerPathWidth = MarkerPathDefaults.WIDTH;
            _activeMarkerPathDisplayTotalMeasurement = MarkerPathDefaults.DISPLAY_MEASUREMENT;
            _activeMarkerPathDisplaySegmentMeasurements = MarkerPathDefaults.DISPLAY_SEGMENT_MEASUREMENTS;
        }

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            // Take the inherited behavior
            base.MRETStart();

            // TODO: Custom initialization (after deserialization)
            if ((_activeMarkerPrefabIndex < 0) && (markerPrefabs.Count > 1))
            {
                _activeMarkerPrefabIndex = 0;
            }
        }

        /// <seealso cref="MRETBehaviour.MRETOnDestroy"/>
        protected override void MRETOnDestroy()
        {
            base.MRETOnDestroy();

            if (ActiveMarkerPath)
            {
                EndPath();
            }
        }
        #endregion MRETUpdateManager

        #region Serializable Instantiation
        /// <seealso cref="MRETSerializableManager{M}.GetDefaultSerializableContainer{T}(T)"/>
        protected override Transform GetDefaultSerializableContainer<T>(T serialized)
        {
            return ProjectManager.MarkersContainer.transform;
        }

        /// <summary>
        /// Instantiates the marker from the supplied serialized marker.
        /// </summary>
        /// <param name="serializedMarker">The <code>MarkerType</code> class instance
        ///     containing the serialized representation of the marker to instantiate</param>
        /// <param name="go">The optional <code>GameObject</code> that will contain the instantiated
        ///     marker. If not provided, one will be created.</param>
        /// <param name="container">The parent container <code>Transform</code> for the instantiated
        ///     marker. If null, the default project markers container will be used.</param>
        /// <param name="onLoaded">The optional <code>Action</code> to be asynchronously triggered on
        ///     completion</param>
        /// <param name="finishMarkerInstantiation">The optional
        ///     <code>FinishSerializableInstantiationDelegate</code> method to be called to finish the
        ///     marker instantiation. Called before the onLoaded action is called. If not specified, a
        ///     default logging behavior will be used.</param>
        /// <param name="context">Optional context parameters to be supplied to the
        ///     finishMarkerInstantiation method to provide additional context</param>
        protected void InstantiateMarker(MarkerType serializedMarker, GameObject go = null,
            Transform container = null, Action<InteractableMarker> onLoaded = null,
            FinishSerializableInstantiationDelegate<MarkerType, InteractableMarker> finishMarkerInstantiation = null,
            params object[] context)
        {
            // Check the context for hand placement
            bool placingByHand = (context.Length > 0) && (context[0] is bool value) && value;

            // Instantiate and deserialize
            InstantiateSerializable(serializedMarker, go, container, onLoaded,
                finishMarkerInstantiation, placingByHand);
        }

        /// <summary>
        /// Instantiates the marker from the supplied serialized marker.
        /// </summary>
        /// <param name="serializedMarker">The <code>MarkerType</code> class instance
        ///     containing the serialized representation of the marker to instantiate</param>
        /// <param name="placingByHand">Indicates if the user will be placing the marker by hand.
        ///     Default is false.</param>
        /// <param name="container">The parent container <code>Transform</code> for the instantiated
        ///     marker. If null, the default project markers container will be used.</param>
        /// <param name="onLoaded">The optional <code>Action</code> to be asynchronously triggered on
        ///     completion</param>
        public void InstantiateMarker(MarkerType serializedMarker, bool placingByHand = false,
            Transform container = null, Action<InteractableMarker> onLoaded = null)
        {
            // Instantiate and deserialize
            InstantiateMarker(serializedMarker, null, container, onLoaded,
                FinishMarkerInstantiation, placingByHand);
        }

        /// <summary>
        /// Instantiates an array of markers from the supplied serialized array of markers.
        /// </summary>
        /// <param name="serializedMarkers">The array of <code>MarkerType</code> class instances
        ///     containing the serialized representations of the markers to instantiate.</param>
        /// <param name="go">The optional <code>GameObject</code> that will contain the instantiated
        ///     markers. If not provided, one will be created for each marker</param>
        /// <param name="container">The parent container <code>Transform</code> for the instantiated
        ///     markers. If null, the project markers container will be used.</param>
        /// <param name="onLoaded">The optional <code>Action</code> to be asynchronously triggered on
        ///     completion</param>
        public void InstantiateMarkers(MarkerType[] serializedMarkers, GameObject go = null,
            Transform container = null, Action<InteractableMarker[]> onLoaded = null)
        {
            // Instantiate and deserialize
            InstantiateSerializables(serializedMarkers, go, container, onLoaded,
                InstantiateMarker, FinishMarkerInstantiation);
        }

        /// <summary>
        /// Instantiates a marker path from the supplied serialized marker path.
        /// </summary>
        /// <param name="serializedMarkerPath">The <code>MarkerPathType</code> class instance
        ///     containing the serialized representation of the marker path to instantiate</param>
        /// <param name="go">The optional <code>GameObject</code> that will contain the instantiated
        ///     marker path. If not provided, one will be created.</param>
        /// <param name="container">The parent container <code>Transform</code> for the instantiated
        ///     marker path. If null, the project markers container will be used.</param>
        /// <param name="onLoaded">The optional <code>Action</code> to be asynchronously triggered on
        ///     completion</param>
        /// <param name="finishMarkerPathInstantiation">The optional
        ///     <code>FinishSerializableInstantiationDelegate</code> method to be called to finish the
        ///     marker path instantiation. Called before the onLoaded action is called. If not specified,
        ///     a default logging behavior will be used.</param>
        /// <param name="context">Optional context parameters to be supplied to the
        ///     finishMarkerPathInstantiation method to provide additional context</param>
        public void InstantiateMarkerPath(MarkerPathType serializedMarkerPath, GameObject go = null,
            Transform container = null, Action<MarkerPath> onLoaded = null,
            FinishSerializableInstantiationDelegate<MarkerPathType, MarkerPath> finishMarkerPathInstantiation = null,
            params object[] context)
        {
            // Instantiate and deserialize
            InstantiateSerializable(serializedMarkerPath, go, container, onLoaded,
                finishMarkerPathInstantiation, context);
        }

        /// <summary>
        /// Instantiates an array of marker paths from the supplied serialized array of marker paths.
        /// </summary>
        /// <param name="serializedMarkerPaths">The array of <code>MarkerPathType</code> class instances
        ///     containing the serialized representations of the marker paths to instantiate.</param>
        /// <param name="go">The optional <code>GameObject</code> that will contain the instantiated
        ///     marker paths. If not provided, one will be created for each marker path</param>
        /// <param name="container">The parent container <code>Transform</code> for the instantiated
        ///     marker paths. If null, the project markers container will be used.</param>
        /// <param name="onLoaded">The optional <code>Action</code> to be asynchronously triggered on
        ///     completion</param>
        /// <param name="finishMarkerInstantiation">The optional
        ///     <code>FinishSerializableInstantiationDelegate</code> method to be called to finish each
        ///     marker path instantiation. Called for each instantiated marker path. If not specified,
        ///     a default logging behavior will be used.</param>
        /// <param name="context">Optional context parameters to be supplied to the
        ///     finishMarkerInstantiation method to provide additional context.</param>
        public void InstantiateMarkerPaths(MarkerPathType[] serializedMarkerPaths, GameObject go = null,
            Transform container = null, Action<MarkerPath[]> onLoaded = null,
            FinishSerializableInstantiationDelegate<MarkerPathType, MarkerPath> finishMarkerInstantiation = null,
            params object[] context)
        {
            // Instantiate and deserialize
            InstantiateSerializables(serializedMarkerPaths, go, container, onLoaded,
                InstantiateMarkerPath, finishMarkerInstantiation, context);
        }

        /// <summary>
        /// Instantiates a markers and marker paths from the supplied serialized markers.
        /// </summary>
        /// <param name="serializedMarkers">The <code>MarkersType</code> class instance
        ///     containing the serialized representation of the markers to instantiate</param>
        /// <param name="go">The optional <code>GameObject</code> that will contain the instantiated
        ///     markers and marker paths. If not provided, one will be created.</param>
        /// <param name="container">The parent container <code>Transform</code> for the instantiated
        ///     markers and marker paths. If null, the project markers container will be used.</param>
        /// <param name="onLoaded">The optional <code>Action</code> to be asynchronously triggered on
        ///     completion</param>
        public void InstantiateMarkersAndPaths(MarkersType serializedMarkers, GameObject go = null,
            Transform container = null, Action<InteractableMarker[]> onLoaded = null)
        {
            // Delegate the instantiation action for markers
            Action<InteractableMarker[]> InstantiatedMarkersAction = (InteractableMarker[] instantiatedMarkers) =>
            {
                // Make sure we have a valid array of markers
                if ((instantiatedMarkers != null) || (instantiatedMarkers.Length > 0))
                {
                    // Delegate the instantiation action for the marker paths
                    Action<MarkerPath[]> InstantiatedMarkerPathsAction = (MarkerPath[] instantiatedMarkerPaths) =>
                    {
                        // Make sure we have a valid array of marker paths
                        if ((instantiatedMarkerPaths == null) ||
                            (instantiatedMarkerPaths.Length != serializedMarkers.Paths.Length))
                        {
                            LogWarning("A problem occurred instantiating the marker paths");
                        }
                    };

                    // Instantiate the marker paths
                    if ((serializedMarkers.Paths != null) && (serializedMarkers.Paths.Length > 0))
                    {
                        InstantiateMarkerPaths(serializedMarkers.Paths, go, container, InstantiatedMarkerPathsAction);
                    }
                }

                // Trigger the event
                OnMarkersLoaded(instantiatedMarkers, onLoaded);
            };

            // Make sure we have a valid container reference
            container = (container == null) ? ProjectManager.MarkersContainer.transform : container;

            // Instantiate and deserialize the Markers
            if ((serializedMarkers == null) || (serializedMarkers.Marker == null) || (serializedMarkers.Marker.Length == 0))
            {
                LogWarning("No markers defined");

                // Trigger the event
                OnMarkersLoaded(null, onLoaded);
            }
            else
            {
                // Instantiate the markers
                InstantiateMarkers(serializedMarkers.Marker, go, container, InstantiatedMarkersAction);
            }
        }

        /// <summary>
        /// Creates a new marker from the current active marker prefab.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="onLoaded"></param>
        public void CreateActiveMarker(Transform container = null,
            Action<InteractableMarker> onLoaded = null)
        {
            // Delegate the instantiation action for markers
            Action<InteractableMarker> InstantiatedMarkerAction = (InteractableMarker instantiatedMarker) =>
            {
                // Complete the instantiation
                FinishMarkerInstantiation(null, instantiatedMarker, true);

                // Trigger the event
                OnMarkerLoaded(instantiatedMarker, onLoaded);
            };

            // Create the active marker
            StartCoroutine(CreateActiveMarkerAsync(container, InstantiatedMarkerAction));
        }

        protected IEnumerator CreateActiveMarkerAsync(Transform container = null,
            Action<InteractableMarker> onLoaded = null)
        {
            if (ActiveMarkerPrefab == null)
            {
                LogError("Active marker prefab is not set", nameof(CreateActiveMarkerAsync));
                onLoaded?.Invoke(default);
                yield break;
            }

            // Yield to the next call or interactive placement fails because this method will be run
            // sequentially (instead of asychronously), causing the onloaded event to be triggered
            // before the handinteractor completes the end placement calls of all interactable scene
            // objects currently being placed.
            yield return null;

            // Make sure we have a valid container reference
            container = (container == null) ? ProjectManager.MarkersContainer.transform : container;

            // Create the new marker
            ModelType serializedModel = new ModelType
            {
                Item = new AssetType
                {
                    // Define the information for serialization
                    AssetName = (ActiveMarkerPrefab.prefab != null) ? ActiveMarkerPrefab.prefab.name : ActiveMarkerPrefab.name,
                    AssetBundle = ActiveMarkerPrefab.assetBundle
                }
            };
            InteractableMarker marker = InteractableMarker.Create(ActiveMarkerPrefab.name, serializedModel,
                ActiveMarkerPrefab.prefab, container);

            // Increase the update rate for a better user experience
            marker.updateRate = UpdateFrequency.Hz10;

            // Set the color
            marker.Color = ActiveMarkerColor;

            // Set the grab behavior
            marker.GrabBehavior = ProjectManager.SceneObjectManager.GrabBehavior;
            marker.configurationPanelPrefab = markerConfigurationPanelPrefab;

            // Add it to the active path
            if (MarkerPathActive)
            {
                ActiveMarkerPath.AddMarker(marker);
            }

            // Surface the event
            onLoaded?.Invoke(marker);

            yield return null;
        }

        /// <seealso cref="MRETSerializableManager{M}.FinishSerializableInstantiation{T, I}"/>
        private void FinishMarkerInstantiation(MarkerType serializedMarker, InteractableMarker interactableMarker,
            params object[] context)
        {
            // Grab the name if available
            string markerName = !string.IsNullOrEmpty(serializedMarker?.Name)
                ? "\"" + serializedMarker.Name + "\" "
                : "";

            // Check for success or failure
            if (interactableMarker != default)
            {
                Log("Marker " + markerName + "instantiation complete", nameof(FinishMarkerInstantiation));

                // Check if we are placing by hand. The indicator is supplied as the first argument
                // of the optional context params during the part instantiation.
                bool placingByHand = (context.Length > 0) && (context[0] is bool value) && value;
                if (placingByHand)
                {
                    // Start placing
                    interactableMarker.BeginPlacing(MRET.InputRig.placingHand.gameObject);
                }
            }
            else
            {
                // Log the error
                LogError("Marker " + markerName + "instantiation failed", nameof(FinishMarkerInstantiation));
            }
        }

        public void OnMarkerLoaded(InteractableMarker interactableMarker,
            Action<InteractableMarker> onLoaded = null)
        {
            StartCoroutine(OnMarkerLoadedAsync(interactableMarker, onLoaded));
        }

        public IEnumerator OnMarkerLoadedAsync(InteractableMarker interactableMarker,
            Action<InteractableMarker> onLoaded = null)
        {
            // Notify caller the marker loading is complete
            onLoaded?.Invoke(interactableMarker);

            yield return null;
        }

        public void OnMarkersLoaded(InteractableMarker[] interactableMarkers,
            Action<InteractableMarker[]> onLoaded = null)
        {
            StartCoroutine(OnMarkersLoadedAsync(interactableMarkers, onLoaded));
        }

        public IEnumerator OnMarkersLoadedAsync(InteractableMarker[] interactableMarkers,
            Action<InteractableMarker[]> onLoaded = null)
        {
            // Notify caller the marker loading is complete
            onLoaded?.Invoke(interactableMarkers);

            yield return null;
        }
        #endregion Serializable Instantiation

        #region Path Management
        /// <summary>
        /// Starts a new marker path
        /// </summary>
        public void StartPath()
        {
            if (MarkerPathActive)
            {
                LogWarning("There is already an active marker path. Ending current active path and starting a new path.");
                EndPath();
            }

            // Create the marker path with no markers
            _activeMarkerPath = MarkerPath.Create("MarkerPath",
                ActiveMarkerPathRenderType, ActiveMarkerPathWidth, new InteractableMarker[0],
                ActiveMarkerPathDisplayTotalMeasurement, ActiveMarkerPathDisplaySegmentMeasurements,
                ActiveMarkerColor);

            // While creating the path, maximize the update rate for a better user experience
            _activeMarkerPath.updateRate = UpdateFrequency.HzMaximum;

            // Disable all interaction. Interaction should only be done with the markers
            _activeMarkerPath.Grabbable = false;
            _activeMarkerPath.Usable = false;

            // Don't add to the project until the path is completed
            _activeMarkerPath.transform.SetParent(transform);
            _activeMarkerPath.transform.localPosition = Vector3.zero;
            _activeMarkerPath.transform.localRotation = Quaternion.identity;
            _activeMarkerPath.transform.localScale = Vector3.one;
        }

        /// <summary>
        /// Ends the current active path
        /// </summary>
        public void EndPath()
        {
            if (!MarkerPathActive)
            {
                LogError("No active marker path", nameof(EndPath));
                return;
            }

            // if there is a project loaded and the active path is valid, add it to the project
            if (ProjectManager.Loaded && (_activeMarkerPath.PathMarkers.Length > 1))
            {
                Log("Adding active marker path to the project: " + _activeMarkerPath.id);
                _activeMarkerPath.transform.SetParent(ProjectManager.MarkersContainer.transform);
                _activeMarkerPath.transform.localPosition = Vector3.zero;
                _activeMarkerPath.transform.localRotation = Quaternion.identity;
                _activeMarkerPath.transform.localScale = Vector3.one;

                // Drop the update rate now that the interaction is less likely
                _activeMarkerPath.updateRate = UpdateFrequency.Hz10;
            }
            else
            {
                // Destroy the marker path
                LogWarning("Active marker path being released because it is no longer valid: " + _activeMarkerPath.id);
                Destroy(_activeMarkerPath.gameObject);
            }

            _activeMarkerPath = null;
        }
        #endregion Path Management
    }

    /// <summary>
    /// Container for an avatar
    /// </summary>
    [System.Serializable]
    public class MarkerPrefab
    {
        [Tooltip("The marker name")]
        public string name = "";

        [Tooltip("The marker asset bundle")]
        public string assetBundle = "";

        [Tooltip("The thumbnail image")]
        public Sprite thumbnail = null;

        [Tooltip("The marker prefab")]
        public GameObject prefab = null;
    }

}
