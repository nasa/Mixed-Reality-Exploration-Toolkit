// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;
using GOV.NASA.GSFC.XR.MRET.Tools.Orientation;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects
{
    /// <remarks>
    /// History:
    /// 17 November 2020: Created
    /// 18 November 2020: Inherit from SceneObject
    /// 9 August 2021: Added Motion Constraints (D. Chheda)
    /// 4 January 2022: Fixed bug with parent get (DZB)
    /// 11 October 2022: Renamed to SceneObject in preparation for the new schema hierarchy
    /// </remarks>
    /// 
    /// <summary>
    /// Base class for all MRET Scene Objects.
    /// 
    /// Author: Dylan Z. Baker
    /// Author: Jeffrey Hosler (refactored)
    /// </summary>
    [System.Obsolete("Refer to " + nameof(GOV.NASA.GSFC.XR.MRET.SceneObjects.InteractableSceneObject))]
    public class InteractableSceneObjectDeprecated : SceneObjectDeprecated
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName
        {
            get
            {
                return nameof(InteractableSceneObjectDeprecated);
            }
        }

        /// <summary>
        /// The parent InteractableSceneObjectDeprecated.
        /// </summary>
        public InteractableSceneObjectDeprecated parent
        {
            get
            {
                Transform p = transform.parent;
                while (p != null)
                {
                    InteractableSceneObjectDeprecated so = p.GetComponent<InteractableSceneObjectDeprecated>();
                    if (so != null)
                    {
                        return so;
                    }
                    p = p.parent;
                }
                return null;
            }
        }

        /// <summary>
        /// The children SceneObjects.
        /// </summary>
        public InteractableSceneObjectDeprecated[] children
        {
            get
            {
                List<InteractableSceneObjectDeprecated> childObjects = new List<InteractableSceneObjectDeprecated>();
                foreach (InteractableSceneObjectDeprecated sceneObject in GetComponentsInChildren<InteractableSceneObjectDeprecated>())
                {
                    if (sceneObject != this)
                    {
                        childObjects.Add(sceneObject);
                    }
                }
                return childObjects.ToArray();
            }
        }

        public Guid uuid;

        protected bool selected = false;

        protected bool isTouchedByGrabber = false; // is this object still being touched the hand grabbing it?

        protected override void MRETUpdate()
        {
            base.MRETUpdate();
            if (grabBehavior == GrabBehavior.Constrained)
            {
                ConstrainMotion();
            }
        }

        public static InteractableSceneObjectDeprecated Create()
        {
            GameObject sceneObjectGameObject = new GameObject();
            return sceneObjectGameObject.AddComponent<InteractableSceneObjectDeprecated>();
        }

        /// <summary>
        /// Finds the InteractableSceneObjectDeprecated that the provided GameObject is part of.
        /// </summary>
        /// <param name="go">GameObject to perform the search for.</param>
        /// <returns>If InteractableSceneObjectDeprecated found, the InteractableSceneObjectDeprecated that the GameObject is part of. Otherwise, null.</returns>
        public static InteractableSceneObjectDeprecated GetSceneObjectForGameObject(GameObject go)
        {
            // Start with the current object and search each of the parents. When a InteractableSceneObjectDeprecated
            // is found on the parent, return that InteractableSceneObjectDeprecated. If no parent is found, return null.
            Transform currentTransform = go.transform;
            while (currentTransform != null)
            {
                InteractableSceneObjectDeprecated foundSceneObject = currentTransform.GetComponent<InteractableSceneObjectDeprecated>();
                if (foundSceneObject != null)
                {
                    return foundSceneObject;
                }
                currentTransform = currentTransform.parent;
            }
            return null;
        }

        public override void Use(InputHand hand)
        {
            LogWarning("Not implemented for InteractableSceneObjectDeprecated.", nameof(Use));
        }

        /// <summary>
        /// Gets all the GameObjects that are part of this InteractableSceneObjectDeprecated.
        /// </summary>
        /// <param name="includeChildSceneObjects"></param>
        /// <returns>Array of GameObjects that are part of this InteractableSceneObjectDeprecated.</returns>
        protected override GameObject[] GetAllGameObjects(bool includeChildSceneObjects)
        {
            List<GameObject> rtnObjects = new List<GameObject>();

            // Go through each child.
            foreach (Transform t in transform)
            {
                // If child InteractableSceneObjectDeprecated are included, automatically add.
                if (includeChildSceneObjects)
                {
                    rtnObjects.Add(t.gameObject);
                }
                // If not, check if it belongs to this InteractableSceneObjectDeprecated.
                else if (GetSceneObjectForGameObject(t.gameObject) == this)
                {
                    rtnObjects.Add(t.gameObject);
                }
            }
            return rtnObjects.ToArray();
        }

        /// <summary>
        /// Gets all the MeshRenderers that are part of this InteractableSceneObjectDeprecated.
        /// </summary>
        /// <param name="includeChildSceneObjects"></param>
        /// <returns>Array of MeshRenderers that are part of this InteractableSceneObjectDeprecated.</returns>
        protected override MeshRenderer[] GetAllMeshRenderers(bool includeChildSceneObjects)
        {
            List<MeshRenderer> rtnObjects = new List<MeshRenderer>();

            // Go through each child.
            foreach (MeshRenderer m in GetComponentsInChildren<MeshRenderer>())
            {
                // If child InteractableSceneObjectDeprecated are included, automatically add.
                if (includeChildSceneObjects)
                {
                    rtnObjects.Add(m);
                }
                // If not, check if it belongs to this InteractableSceneObjectDeprecated.
                else if (GetSceneObjectForGameObject(m.gameObject) == this)
                {
                    rtnObjects.Add(m);
                }
            }
            return rtnObjects.ToArray();
        }

        /// <summary>
        /// Begin to touch the provided hand. Behavior will
        /// be dictated by the configured touchBehavior.
        /// </summary>
        /// <param name="hand">Hand to touch.</param>
        protected override void BeginTouch(InputHand hand)
        {
            if (!useable)
            {
                return;
            }
            
            // Don't change touch if being grabbed.
            if (touchingHand != null)
            {
                if (IsGrabbedBy(touchingHand))
                {
                    return;
                }
            }

            switch (touchBehavior)
            {
                case TouchBehavior.Hold:
                    holdCount = 0;
                    goto case TouchBehavior.Highlight;

                case TouchBehavior.Highlight:
                    if (!selected)
                    {
                        //only highlight if unlocked
                        if (!locked)
                        {
                            // Alter this object's materials.
                            if (savedMaterialInfo != null)
                            {
                                RestoreObjectMaterials();
                            }
                            SaveObjectMaterials(true);
                            ReplaceObjectMaterials(MRET.HighlightMaterial, true);
                        }

                        // Save the touching hand.
                        touchingHand = hand;
                    }
                    break;

                case TouchBehavior.Custom:
                default:
                    LogWarning("Not implemented for InteractableSceneObjectDeprecated.", nameof(BeginTouch));
                    break;
            }
        }

        /// <summary>
        /// Stop touching the current touching hand. Behavior will
        /// be dictated by the configured touchBehavior.
        /// </summary>
        protected override void EndTouch()
        {
            if (!useable)
            {
                return;
            }

            // Don't stop touching if still being grabbed.
            if (touchingHand != null)
            {
                if (IsGrabbedBy(touchingHand))
                {
                    return;
                }
            }

            switch (touchBehavior)
            {
                case TouchBehavior.Hold:
                    holdCount = -1;
                    touchHolding = false;
                    goto case TouchBehavior.Highlight;

                case TouchBehavior.Highlight:
                    if (!selected)
                    {
                        // Restore this object's materials.
                        RestoreObjectMaterials();
                    }
                    break;

                case TouchBehavior.Custom:
                default:
                    LogWarning("Not implemented for InteractableSceneObjectDeprecated.", nameof(EndTouch));
                    break;
            }
        }

        public override void BeginGrab(InputHand hand)
        {
            if (!locked)
            {
                switch (grabBehavior)
                {
                    case GrabBehavior.Constrained:
                        if (grabbable)
                        {
                            StartConstrainedMotion(hand.transform);
                        }
                        break;

                    case GrabBehavior.Attach:
                    case GrabBehavior.Custom:
                    default:
                        base.BeginGrab(hand);
                        break;
                }
            }
        }

        public override void EndGrab(InputHand hand)
        {
            // TODO: Can there be a race condition if the grab mode
            // changes mid-grab?
            switch (grabBehavior)
            {
                case GrabBehavior.Constrained:
                    if (grabbable)
                    {
                        EndConstrainedMotion();
                    }
                    break;

                case GrabBehavior.Attach:
                case GrabBehavior.Custom:
                default:
                    base.EndGrab(hand);
                    break;
            }
        }

#region Attach Grabbing

        /// <summary>
        /// Target to follow in constrained motion mode.
        /// </summary>
        protected Transform constrainedMotionTarget;

        /// <summary>
        /// The offset between this object and the constrainedMotionTarget.
        /// </summary>
        protected Vector3 constrainedMotionOffset;

        /// <summary>
        /// Determines whether or not the current object is grabbed by the provided hand.
        /// Compatible with regular and constrained attach grabbing.
        /// </summary>
        /// <param name="hand">Hand to check.</param>
        /// <returns>True if is being grabbed, false if not.</returns>
        public override bool IsGrabbedBy(InputHand hand)
        {
            if (hand == null || hand.transform == null) return false;
            if(base.IsGrabbedBy(hand) || constrainedMotionTarget == hand.transform)
            {
                return true;
            }
            return false;
        }
#endregion

#region Motion Constraints

        /// <summary>
        /// Data Manager key for motion constraints.
        /// </summary>
        public static readonly string motionConstraintsKey = "MRET.INTERNAL.TOOLS.MOTION_CONSTRAINTS";

        public void StartConstrainedMotion(Transform attachToTransform)
        {
            constrainedMotionTarget = attachToTransform;
            constrainedMotionOffset = transform.position - attachToTransform.position;
            InitializeMotionConstraints();
        }

        public void EndConstrainedMotion()
        {
            constrainedMotionTarget = null;
            constrainedMotionOffset = Vector3.zero; // not really necessary
            DeinitializeMotionConstraints();
        }

        /// <summary>
        /// Stores the data obtained from a ray cast searching for floors.
        /// </summary>
        protected struct FloorHitData
        {
            public Vector3 floorNormal, point;
            public float floorDistance;
            public GameObject floor;
        }

        private static int floorLayer = 24;
        /// <summary>
        /// The layer used for floor ray casts. 
        /// Only accepts layers from 0-30.
        /// Uses layer 24 by default.
        /// </summary>
        public static int FloorLayer
        {
            get
            {
                return floorLayer;
            }
            set
            {
                if (value >= 0 && value <= 30)
                {
                    floorLayer = value;
                }
            }
        }

        /// <summary>
        /// Tag used for specifying floor objects.
        /// </summary>
        protected const string FLOOR_TAG = "Floor";

        // Methods for managing the height of the object, to prevent clipping into the floor (see CMM.1).
        // None - no height management. This can lead to clipping into the floor after repeated floor transitions. Not recommended if advanced 
        // transition mode is disabled
        // Maintain - maintain a constant height from the floor. Can lead to undesired bheavior if there are intentional vertical gaps between floors.
        // Additionally, can look a bit weird on ramps.
        // Clamp - clamps the height to an acceptable range. Can be customized with the heightMinThreshold and heightMaxThreshold. 
        // If the object's height falls below the heightMinThreshold, the height is increased to heightMinThreshold, and vice versa 
        // if the height increases beyond heightMaxThreshold.
        /// <summary>
        /// Method for managing the height of the object above the ground (i.e. to prevent clipping into the floor).
        /// </summary>
        public enum HeightManagementMethod { None, Maintain, Clamp };

        /// <summary>
        /// Method used to manage the height of the object during motion constraints (relative to floors).
        /// </summary>
        [Tooltip("Method used to manage the height of the object during motion constraints (relative to floors).")]
        public HeightManagementMethod heightManagementMethod = HeightManagementMethod.None;

        /// <summary>
        /// The minimum allowable height (from the current floor) for this object.
        /// Only used during motion constraints and when height management method is set to Clamp.
        /// </summary>
        [Tooltip("The minimum allowable height (from the current floor) for this object. Only used during motion constraints and when height management method is set to Clamp.")]
        public float heightMinThreshold = .1f;

        /// <summary>
        /// The maximum allowable height (from the current floor) for this object.
        /// Only used during motion constraints and when height management method is set to Clamp.
        /// </summary>
        [Tooltip("The maximum allowable height (from the current floor) for this object. Only used during motion constraints and when height management method is set to Clamp.")]
        public float heightMaxThreshold = .5f;

        /// <summary>
        /// Whether this object should rotate to align with the current floor during motion constraints.
        /// If disabled, the object will maintain its initial orientation.
        /// </summary>
        [Tooltip("Whether this object should rotate to align with the current floor during motion constraints. If disabled, the object will maintain its initial orientation.")]
        public bool alignWithFloor = true;

        /// <summary> 
        /// Whether to use advanced transitions between floors during motion constraints. This has little effect if floor alignment is disabled.
        /// </summary>
        [Tooltip("Whether to use advanced transitions between floors during motion constraints. This has little effect if floor alignment is disabled.")]
        public bool advancedTransitionFeatures = true;

        // It SHOULD be okay to modify the object-aligned bounding box during execution, if needed.
        /// <summary>
        /// The object aligned bounding box used for all transition computation.
        /// If this is not specified, a bounding box will automaticaly be computed when motion constraints are initialized.
        /// </summary>
        [Tooltip("The object aligned bounding box used for all transition computation. If this is not specified, a bounding box will automaticaly be computed when motion constraints are initialized.")]
        public ObjectAlignedBoundingBox objectAlignedBoundingBox;


        private Vector3 downDirection = Vector3.down;
        /// <summary>
        /// The down direction used for floor detection. Should only be set at the beginning or end of the frame to prevent strange behavior.
        /// </summary>
        public Vector3 DownDirection
        {
            get
            {
                return downDirection.normalized;
            }
            set
            {
                downDirection = value.normalized;
            }
        }

        /// <summary>
        /// The maximum vertical distance allowed between floors. This is used to prevent objects from "falling" between floors
        /// </summary>
        public float maxFloorGap = 1f;

        /// <summary>
        /// Points of a square in two dimensions, ordered clockwise. Used for BoundingBoxRayCast.
        /// </summary>
        private static readonly Vector2[] squarePoints = { new Vector2(1, 1), new Vector2(-1, 1), new Vector2(-1, -1), new Vector2(1, -1) };

        /// <summary>
        /// The floor surface this object is currently constrained to.
        /// </summary>
        private GameObject currentFloor;

        /// <summary>
        /// Distance to the current floor. Initially, this is 0, but it gets set immediately in InitializeMotionConstraints()
        /// </summary>
        private float currentFloorDistance = 0f;

        private bool initializedMotionConstraints = false;
        /// <summary>
        /// Initialize motion constraints by computing an object aligned bounding box (if one does not exist),
        /// and by computing an initial ray cast for the current floor and floor distance.
        /// </summary>
        protected void InitializeMotionConstraints()
        {
            if (initializedMotionConstraints) return;

            // Compute a bounding box if none exists 
            if (!ObjectAlignedBoundingBox.IsValid(objectAlignedBoundingBox))
            {
                Log("Automatically computing an object-aligned bounding box.", nameof(InitializeMotionConstraints));
                objectAlignedBoundingBox = ObjectAlignedBoundingBox.Encapsulate(transform);
            }

            FloorHitData hitData = CenterRayCast(Vector3.zero, true);
            currentFloor = hitData.floor;
            currentFloorDistance = hitData.floorDistance;

            initializedMotionConstraints = true;
        }

        /// <summary>
        /// Deinitialize motion constraints.
        /// This ensures that floor data is computed correctly when toggling between attach modes.
        /// </summary> 
        protected void DeinitializeMotionConstraints()
        {
            initializedMotionConstraints = false;
        }

        /// <summary>
        /// All motion constraints computations for a single frame.
        /// </summary>
        protected void ConstrainMotion()
        {
            InitializeMotionConstraints(); // just in case
            
            FloorHitData hitData = CenterRayCast();
            currentFloor = hitData.floor;
            currentFloorDistance = hitData.floorDistance;
            Vector3 floorNormal = hitData.floorNormal;

            if (!currentFloor) 
            {
                Log("No floors", nameof(ConstrainMotion));
                return;
            }

            float heightStart = FindHeight();

            if(advancedTransitionFeatures) floorNormal = AdvancedTransition(floorNormal);

            if(alignWithFloor) AlignWithSurface(floorNormal);

            // Project the delta for this frame onto the current floor, and ensure that the object remains on a floor with edge detection.
            if (constrainedMotionTarget != null)
            {
                Vector3 baseDelta = (constrainedMotionTarget.position + constrainedMotionOffset - transform.position);
                Vector3 finalDelta = EdgeDetection(Vector3.ProjectOnPlane(baseDelta, floorNormal));
                transform.position += finalDelta;


                // Recompute the floor distance to use for height management.
                float heightEnd = FindHeight();
                // Fixes height variation issues (i.e. height changes on transitions due to numerical error)
                switch (heightManagementMethod)
                {
                    case HeightManagementMethod.Maintain:
                        transform.position += (heightStart - heightEnd) * -DownDirection;
                        break;
                    case HeightManagementMethod.Clamp:
                        float desiredHeight = Mathf.Clamp(heightEnd, heightMinThreshold, heightMaxThreshold);

                        if (desiredHeight != heightEnd)
                        {
                            transform.position += (heightEnd - desiredHeight) * DownDirection;
                        }
                        break;
                    case HeightManagementMethod.None:
                    default:
                        break;
                }

                // Recompute the floor distance at the end of the frame
                currentFloorDistance = CenterRayCast().floorDistance;
            }
        }

        /// <summary>
        /// Finds the minimum height from this object to the floor (i.e. minimum height from the bounding box edges and center).
        /// </summary>
        protected float FindHeight()
        {
            float minFloorDistance = CenterRayCast().floorDistance;

            foreach (FloorHitData hit in BoundingBoxRayCast())
            {
                minFloorDistance = Mathf.Min(minFloorDistance, hit.floorDistance);
            }

            return minFloorDistance;
        }

        // Works by detecting upcoming changes in the current Floor by ray 
        // casting from the bottom vertices of an object aligned bounding box. The results of these 4 ray casts are then processed to 
        // determine an "intermediate" rotation of the object which is between the rotation of the current floor surface and the upcoming 
        // floor. The method assumes planar floors (or at least locally planar floors) for now.
        // Returns the computed orientation of the object
        //
        // TODO: Add detection for down-ramps vs up-ramps, and different behavior
        // TODO: Add advanced behavior for more than 2 different floor surfaces
        /// <summary>
        /// Allows for a smoother transition between different floor surfaces.
        /// </summary>
        /// <param name="currentFloorNormal">The current floor normal, which is often the object's current up direction.</param>
        /// <returns>A computed orientation for the object's up direction, which can be used to align/rotate the object.</returns>
        private Vector3 AdvancedTransition(Vector3 currentFloorNormal)
        {

            // Compute ray casts (for Floor surfaces) at the bottom vertices of the object aligned bounding box.
            FloorHitData[] boundingBoxRayCasts = BoundingBoxRayCast();

            // Check if all bounding box ray casts return the same floor. If so, no advanced transition is necessary.
            bool allSameFloor = (boundingBoxRayCasts[0].floor != null);
            foreach (FloorHitData boundingBoxRayCast in boundingBoxRayCasts)
            {
                allSameFloor &= (boundingBoxRayCast.floor == boundingBoxRayCasts[0].floor);
            }

            if (allSameFloor) return currentFloorNormal;


            // NOTE: Unity uses the left-hand system when calculating cross products.
            // This seems to work well for most cases
            return Vector3.Cross(
                boundingBoxRayCasts[0].point - boundingBoxRayCasts[2].point,
                boundingBoxRayCasts[3].point - boundingBoxRayCasts[1].point
            ).normalized;
        }


        /// <summary>
        /// Detects whether a give position delta would cause this object to move off the edge of the floor. 
        /// If the given delta would cause the object to have no floor, returns the 0 vector, preventing the object from falling off the edge.
        /// </summary>
        /// <param name="delta">The proposed position delta for this object.</param> 
        /// <returns>A new position delta which is either the original delta or the zero vector, depending on whether the passed delta is valid or not.</returns>
        private Vector3 EdgeDetection(Vector3 delta)
        {
            FloorHitData centerCast = CenterRayCast(delta);

            bool allSafe = (centerCast.floor != null);
            if (advancedTransitionFeatures)
            {
                FloorHitData[] edgeCasts = BoundingBoxRayCast(delta);
                foreach (FloorHitData edgeCast in edgeCasts)
                {
                    allSafe &= (edgeCast.floor != null);
                }
            }

            return allSafe ? delta : Vector3.zero;
        }

        // TODO - fix known issue: rotation is suddenly changed when toggling constrained motion mode.
        /// <summary>
        /// Align this object's 'up' direction with a given Vector3. If the rotation is not safe as defined by SafeRotation, this does nothing.
        /// </summary>
        /// <param name="surfaceNormal">The vector with which to align this object. This is often the normal of a floor surface.</param>
        private void AlignWithSurface(Vector3 surfaceNormal)
        {
            // If the object is already aligned with the surface, no rotation is required.
            if (transform.up == surfaceNormal) return;

            // The rotation from the current 'up' to the desired 'up' (surface normal)
            Quaternion rot = Quaternion.FromToRotation(transform.up, surfaceNormal);
            SafeRotation(rot);
        }

        /// <summary>
        /// Check if the rotation is valid - i.e., all ray casts are still valid, the rotation doesn't cause oscillating behavior, etc.
        /// If it is safe, rotates by the given rotation, else remains at original rotation.
        /// </summary>
        /// <param name="rot">The rotation to rotate the object by if it is safe to do so.</param>
        /// <returns>A boolean indicating whether the rotation was successful or not.</returns>
        private bool SafeRotation(Quaternion rot)
        {
            if (!advancedTransitionFeatures)
            {
                return true;
            }

            Quaternion reverseRot = Quaternion.Inverse(rot);
            FloorHitData[] originalCasts = BoundingBoxRayCast();

            // Rotate the object to align with the surface
            transform.rotation = rot * transform.rotation;

            FloorHitData[] rotationCasts = BoundingBoxRayCast();

            for (int i = 0; i < rotationCasts.Length; i++)
            {
                if (rotationCasts[i].floor != originalCasts[i].floor)
                {
                    // If the rotation is not safe, revert and return
                    transform.rotation = reverseRot * transform.rotation;
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Searches for floor surfaces beneath (or near) the center of the bottom of the object-aligned bounding box.
        /// </summary>
        /// <param name="delta">A delta from the bottom center of the object-aligned bounding box.</param>
        /// <returns>A FloorHitData containing the results of the ray cast.</returns>
        private FloorHitData CenterRayCast(Vector3 delta = new Vector3(), bool ignoreRayCastLimit = false)
        {
            return FindFloorSurface(objectAlignedBoundingBox.ScaleBoundingVector(0, -1, 0) + delta, ignoreRayCastLimit);
        }

        /// <summary>
        /// Search for floor surfaces beneath each of the bottom 4 vertices of the object-aligned bounding box.
        /// </summary>
        /// <param name="delta">A delta from the bottom vertices of the object-aligned bounding box.</param>
        /// <returns>An array of FloorHitData containing the results of the ray casts.</returns>
        private FloorHitData[] BoundingBoxRayCast(Vector3 delta = new Vector3())
        {
            FloorHitData[] casts = new FloorHitData[squarePoints.Length];

            for (int i = 0; i < casts.Length; i++)
            {
                casts[i] = FindFloorSurface(objectAlignedBoundingBox.ScaleBoundingVector(squarePoints[i].x, -1, squarePoints[i].y) + delta);
            }

            return casts;
        }

        /// <summary>
        /// Find the Floor which is directly "beneath" a specified origin based on the defined DownDirection using a ray cast.
        /// </summary>
        /// <param name="rayOrigin">The origin of the ray cast.</param>
        /// <param name="ignoreRayCastLimit">If true, the ray cast is infinite. Otherwise, it is limited by MaxFloorGap.</param>
        private FloorHitData FindFloorSurface(Vector3 rayOrigin, bool ignoreRayCastLimit = false)
        {
            FloorHitData data = new FloorHitData();

            // Initialize the return values for found floor
            data.floor = null;
            data.floorNormal = new Vector3();
            data.floorDistance = 0f;

            GameObject[] floors = GameObject.FindGameObjectsWithTag(FLOOR_TAG);

            if (floors == null || floors.Length == 0) return data;

            // Set the layer of each floor
            int[] previousLayers = new int[floors.Length];
            for (int i = 0; i < floors.Length; i++)
            {
                previousLayers[i] = floors[i].layer;
                floors[i].layer = FloorLayer;
            }

            // Stores the result of the ray cast
            RaycastHit hit;
            // Restricts the ray cast to only include the layer with floors
            int layerMask = 1 << FloorLayer;

            // Try ray casting in the down direction, starting from the center of this object 
            // Using finite ray cast instead of infinity to prevent steep falls
            // TODO - add user customization of finite ray casts
            if (Physics.Raycast(rayOrigin, DownDirection, out hit, ignoreRayCastLimit ? Mathf.Infinity : currentFloorDistance + maxFloorGap, layerMask))
            {
                // found an object
                data.floor = hit.transform.gameObject;
                data.floorNormal = hit.normal;
                data.floorDistance = hit.distance;
                data.point = hit.point;
            }

            for (int i = 0; i < floors.Length; i++)
            {
                floors[i].layer = previousLayers[i];
            }

            return data;
        }
#endregion
    }
}