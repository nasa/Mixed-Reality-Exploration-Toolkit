using UnityEngine;
using System.Collections.Generic;
using EasyBuildSystem.Runtimes;
using EasyBuildSystem.Runtimes.Events;
using EasyBuildSystem.Runtimes.Extensions;
using EasyBuildSystem.Runtimes.Internal.Area;
using EasyBuildSystem.Runtimes.Internal.Managers;
using EasyBuildSystem.Runtimes.Internal.Part;
using EasyBuildSystem.Runtimes.Internal.Socket;
using EasyBuildSystem.Runtimes.Internal.Socket.Data;
using EasyBuildSystem.Runtimes.Internal.Builder;

namespace GSFC.ARVR.MRET.Alignment
{
    
    public class VRBuilderBehaviour : MonoBehaviour
    {
        #region Public Fields

        public static VRBuilderBehaviour Instance;

        #region Base Settings

        public float ActionDistance = 6f;

        public float SnapThreshold = 5f;

        public float OutOfRangeDistance = 0f;

        public float OverlapAngles = 35f;

        public bool LockRotation;

        public DetectionType RayDetection = DetectionType.Overlap;

        public RayType CameraType;

        public float RaycastOffset = 1f;

        public Transform RaycastOriginTransform;

        public Transform RaycastAnchorTransform;

        public float raySteps = 0.1f;

        public GameObject placingObject;

        #endregion Base Settings

        #region Modes Settings

        public bool UsePlacementMode = true;
        public bool ResetModeAfterPlacement = false;
        public bool UseDestructionMode = true;
        public bool ResetModeAfterDestruction = false;
        public bool UseEditionMode = true;

        #endregion Modes Settings

        #region Preview Settings

        public bool UsePreviewCamera;
        public Camera PreviewCamera;
        public LayerMask PreviewLayer;

        public MovementType PreviewMovementType;

        public bool PreviewMovementOnlyAllowed;

        public float PreviewGridSize = 1.0f;
        public float PreviewGridOffset;
        public float PreviewSmoothTime = 5.0f;

        #endregion Preview Settings

        #region Audio Settings

        public AudioSource Source;

        public AudioClip[] PlacementClips;

        public AudioClip[] DestructionClips;

        #endregion Audio Settings

        [HideInInspector]
        public BuildMode CurrentMode;

        [HideInInspector]
        public PartBehaviour SelectedPrefab;

        [HideInInspector]
        public int SelectedIndex;

        [HideInInspector]
        public PartBehaviour CurrentPreview;

        [HideInInspector]
        public PartBehaviour CurrentEditionPreview;

        [HideInInspector]
        public PartBehaviour CurrentRemovePreview;

        [HideInInspector]
        public Vector3 CurrentRotationOffset;

        [HideInInspector]
        public bool AllowPlacement = true;

        [HideInInspector]
        public bool AllowDestruction = true;

        [HideInInspector]
        public bool AllowEdition = true;

        [HideInInspector]
        public bool HasSocket;

        [HideInInspector]
        public bool UseForNetwork;

        [HideInInspector]
        public SocketBehaviour CurrentSocket;

        [HideInInspector]
        public SocketBehaviour LastSocket;

        private Transform _Caster;
        public Transform GetCaster
        {
            get
            {
                if (_Caster == null)
                {
                    _Caster = CameraType != RayType.TopDown ? transform.parent != null ? transform.parent
                    : transform : RaycastAnchorTransform != null ? RaycastAnchorTransform : transform;
                }

                return _Caster;
            }
        }

        #endregion Public Fields

        #region Private Fields

        private Camera BuilderCamera;

        private RaycastHit TopDownHit;
        private Vector3 LastAllowedPosition;

        #endregion Private Fields

        #region Private Methods

        public virtual void Start()
        {
            if (!Application.isPlaying)
                return;

            Instance = this;

            if (PreviewCamera != null)
                PreviewCamera.enabled = UsePreviewCamera;

            BuilderCamera = GetComponent<Camera>();

            //if (BuilderCamera == null)
            //    Debug.Log("<b><color=cyan>[Easy Build System]</color></b> : No camera for the Builder Behaviour component.");
        }

        public virtual void Update()
        {
            if (!Application.isPlaying)
                return;

            UpdateModes();
        }

        private Vector3 lastPos = Vector3.zero;
        private Quaternion lastRot = Quaternion.identity;
        private Ray? lastRay = null;
        private Ray Ray = new Ray();
        private int rayUpdateCounter = 0;
        private Ray? GetRay()
        {
            float shortestDistance = OutOfRangeDistance <= 0 ? Mathf.Infinity : OutOfRangeDistance;
            Ray shortestRay = new Ray();
            GameObject hitObj = null;

            if (placingObject == null)
            {
                return new Ray();
            }

            if (Mathf.Abs(Vector3.Distance(lastPos, transform.position)) < 0.1 ||
                Mathf.Abs(Quaternion.Angle(lastRot, transform.rotation)) < 5 || rayUpdateCounter++ < 8)
            {
                return lastRay;
            }

            rayUpdateCounter = 0;

            for (float x = -1; x <= 1; x += raySteps)
            {
                for (float y = -1; y <= 1; y += raySteps)
                {
                    for (float z = -1; z <= 1; z += raySteps)
                    {
                        Ray currentRay = new Ray(placingObject.transform.position, new Vector3(x, y, z));
                        RaycastHit hit;
                        Physics.Raycast(currentRay, out hit, OutOfRangeDistance, BuildManager.Instance.FreeLayers);
                        if (!HitPlacingObject(hit) && hit.distance > 0.001 && hit.distance < shortestDistance)
                        {
                            shortestRay = currentRay;
                            shortestDistance = hit.distance;
                            hitObj = hit.collider.gameObject;
                        }
                    }
                }
            }

            // Set origin to negative infinity to indicate no hits.
            if (shortestDistance == (OutOfRangeDistance <= 0 ? Mathf.Infinity : OutOfRangeDistance))
            {
                lastRay = null;
                return null;
            }
            else
            {
                lastRay = shortestRay;
                return shortestRay;
            }

            //List<Ray> rays = new List<Ray>();
            /*foreach (Collider coll in CurrentPreview.gameObject.GetComponentsInChildren<Collider>())
            {
                if (coll.gameObject.layer != SessionConfiguration.raycastLayer)
                {
                    if (coll is BoxCollider)
                    {
                        rays.Add(GetRayBoxCollider((BoxCollider) coll));
                    }
                    else if (coll is MeshCollider)
                    {
                        rays.Add(GetRayMeshCollider((MeshCollider) coll));
                    }
                    else if (coll is SphereCollider)
                    {
                        rays.Add(GetRaySphereCollider((SphereCollider) coll));
                    }
                    else if (coll is CapsuleCollider)
                    {
                        rays.Add(GetRayCapsuleCollider((CapsuleCollider) coll));
                    }
                }
            }*/

            //return rays.ToArray();

            /*if (CameraType == RayType.TopDown)
                return BuilderCamera.ScreenPointToRay(Input.mousePosition + new Vector3(0, 0, RaycastOffset));
            else if (CameraType == RayType.FirstPerson)
                return new Ray(BuilderCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, RaycastOffset)), BuilderCamera.transform.forward);
            else if (CameraType == RayType.ThirdPerson)
                if (RaycastOriginTransform != null)
                    return new Ray(RaycastOriginTransform.position, BuilderCamera.transform.forward);

            return new Ray();*/
        }
        
        private bool HitPlacingObject(RaycastHit hit)
        {
            if (hit.collider == null) return false;

            Transform currentObject = hit.collider.gameObject.transform;

            while (currentObject != null)
            {
                if (currentObject.gameObject == placingObject)
                {
                    return true;
                }
                else if (CurrentPreview != null)
                {
                    if (currentObject.gameObject == CurrentPreview.gameObject)
                    {
                        return true;
                    }
                }
                currentObject = currentObject.transform.parent;
            }

            return false;
        }

        /*private Ray[] GetRaysBoxCollider(BoxCollider coll)
        {

        }

        private Ray[] GetRaysMeshCollider(MeshCollider coll)
        {

        }

        private Ray[] GetRaysSphereCollider(SphereCollider coll)
        {

        }

        private Ray GetRayCapsuleCollider(CapsuleCollider coll)
        {

        }*/

        #endregion Private Methods

        #region Public Methods

        /// <summary>
        /// This method allows to update all the builder (Placement, Destruction, Edition).
        /// </summary>
        public virtual void UpdateModes()
        {
            if (BuildManager.Instance == null)
                return;

            if (BuildManager.Instance.PartsCollection == null)
                return;

            //if (!placingObject || !placingObject.transform.IsChildOf(transform))
            //    return;

            if (CurrentMode != BuildMode.None)
            {
                Ray? result = GetRay();
                if (result.HasValue) //&& !(result.Value.direction == Vector3.zero && result.Value.origin == Vector3.zero))
                {
                    Ray = result.Value;
                }
                else
                {
                    if (CurrentPreview)
                    {
                        Destroy(CurrentPreview.gameObject);
                        CurrentPreview = null;
                    }
                    return;
                }
            }

            if (SelectedPrefab == null)
                SelectedPrefab = placingObject.GetComponent<PartBehaviour>();
            SelectedPrefab.RotateAccordingSlope = true;

            if (CurrentPreview == null)
                CreatePreview(SelectedPrefab.gameObject);

            if (CurrentMode == BuildMode.Placement)
            {
                if (SelectedPrefab == null)
                    return;

                if (!PreviewExists())
                {
                    CreatePreview(SelectedPrefab.gameObject);
                    return;
                }
                else
                {
                    UpdatePreview();
                }
            }
            else if (CurrentMode == BuildMode.Destruction)
                UpdateRemovePreview();
            else if (CurrentMode == BuildMode.Edition)
                UpdateEditionPreview();
            else if (CurrentMode == BuildMode.None)
                ClearPreview();
        }

        #region Placement

        /// <summary>
        /// This method allows to update the placement preview.
        /// </summary>
        private readonly RaycastHit[] Hits = new RaycastHit[PhysicExtension.MAX_ALLOC_COUNT];
        public void UpdatePreview()
        {
            HasSocket = false;

            if (CameraType == RayType.TopDown)
                Physics.Raycast(Ray, out TopDownHit, Mathf.Infinity, LayerMask.NameToLayer(Constants.LAYER_SOCKET.ToLower()), QueryTriggerInteraction.Ignore);

            if (RayDetection == DetectionType.Overlap)
            {
                PartBehaviour[] NeighboursParts =
                    PhysicExtension.GetNeighborsTypesBySphere<PartBehaviour>(GetCaster.position, ActionDistance, LayerMask.NameToLayer(Constants.LAYER_SOCKET.ToLower()));

                PartBehaviour[] ApplicableParts = new PartBehaviour[NeighboursParts.Length];

                for (int i = 0; i < NeighboursParts.Length; i++)
                {
                    if (NeighboursParts[i].Sockets == null)
                        continue;

                    foreach (SocketBehaviour Socket in NeighboursParts[i].Sockets)
                    {
                        if (!Socket.IsDisabled && Socket.AllowPart(SelectedPrefab))
                        {
                            ApplicableParts[i] = NeighboursParts[i];
                            break;
                        }
                    }
                }

                /*if (ApplicableParts.Length > 0)
                    UpdateSnapsMovementOverlap(ApplicableParts);
                else*/
                    UpdateFreeMovement();
            }
            else
            {
                SocketBehaviour Socket = null;

                int ColliderCount = Physics.RaycastNonAlloc(Ray, Hits, ActionDistance, LayerMask.NameToLayer(Constants.LAYER_SOCKET.ToLower()));

                for (int i = 0; i < ColliderCount; i++)
                    if (Hits[i].collider.GetComponent<SocketBehaviour>() != null)
                        Socket = Hits[i].collider.GetComponent<SocketBehaviour>();

                if (Socket != null)
                    UpdateSnapsMovementLine(Socket);
                else
                    UpdateFreeMovement();
            }

            AllowPlacement = !HasCollision();

            if (AllowPlacement && CurrentPreview.RequireSockets && !HasSocket)
                AllowPlacement = false;

            if (AllowPlacement && OutOfRangeDistance != 0)
                AllowPlacement = Vector3.Distance(GetCaster.position, CurrentPreview.transform.position) < ActionDistance;

            CurrentPreview.gameObject.ChangeAllMaterialsColorInChildren(CurrentPreview.Renderers.ToArray(),
                AllowPlacement ? CurrentPreview.PreviewAllowedColor : CurrentPreview.PreviewDeniedColor, SelectedPrefab.PreviewColorLerpTime, SelectedPrefab.PreviewUseColorLerpTime);
        }

        /// <summary>
        /// This method allows to move the preview in free movement.
        /// </summary>
        public void UpdateFreeMovement()
        {
            if (CurrentPreview == null)
                return;

            float Distance = OutOfRangeDistance == 0 ? ActionDistance : OutOfRangeDistance;
            Debug.DrawRay(Ray.origin, Ray.direction, Color.green);
            if (Physics.Raycast(Ray, out RaycastHit Hit, Distance, BuildManager.Instance.FreeLayers))
            {
                Bounds bou = new Bounds(CurrentPreview.transform.position, Vector3.zero);
                foreach (Renderer ren in CurrentPreview.GetComponentsInChildren<Renderer>())
                {
                    bou.Encapsulate(ren.bounds);
                }

                //if (Hit.point != bou.min && Hit.point != bou.max)
                {
                    CurrentPreview.PreviewOffset = GetPreviewOffset(bou, Hit.point, CurrentPreview.transform.position);
                }

                if (PreviewMovementType == MovementType.Normal)
                {
                    if (PreviewMovementOnlyAllowed)
                    {
                        CurrentPreview.transform.position = Hit.point + CurrentPreview.PreviewOffset;

                        if (!HasCollision())
                            LastAllowedPosition = Hit.point + CurrentPreview.PreviewOffset;
                        else
                            CurrentPreview.transform.position = LastAllowedPosition;
                    }
                    else
                        CurrentPreview.transform.position = Hit.point + CurrentPreview.PreviewOffset;
                }
                else if (PreviewMovementType == MovementType.Grid)
                    CurrentPreview.transform.position = MathExtension.PositionToGridPosition(PreviewGridSize, PreviewGridOffset, Hit.point + CurrentPreview.PreviewOffset);
                else if (PreviewMovementType == MovementType.Smooth)
                    CurrentPreview.transform.position = Vector3.Lerp(CurrentPreview.transform.position, Hit.point + CurrentPreview.PreviewOffset, PreviewSmoothTime * UnityEngine.Time.deltaTime);

                if (!CurrentPreview.RotateAccordingSlope)
                {
                    if (LockRotation)
                        CurrentPreview.transform.rotation = GetCaster.rotation * SelectedPrefab.transform.localRotation * Quaternion.Euler(CurrentRotationOffset);
                    else
                        CurrentPreview.transform.rotation = Quaternion.Euler(CurrentRotationOffset);
                }
                else
                {
                    if (Hit.collider is TerrainCollider)
                    {
                        float SampleHeight = Hit.collider.GetComponent<UnityEngine.Terrain>().SampleHeight(Hit.point);

                        if (Hit.point.y - .1f < SampleHeight)
                        {
                            CurrentPreview.transform.rotation = Quaternion.FromToRotation(GetCaster.up, Hit.normal) * Quaternion.Euler(CurrentRotationOffset)
                                * GetCaster.rotation * SelectedPrefab.transform.localRotation * Quaternion.Euler(CurrentRotationOffset);
                        }
                        else
                            CurrentPreview.transform.rotation = GetCaster.rotation * SelectedPrefab.transform.localRotation * Quaternion.Euler(CurrentRotationOffset);
                    }
                    else
                    {
                        CurrentPreview.transform.rotation = GetCaster.rotation * SelectedPrefab.transform.localRotation * Quaternion.Euler(CurrentRotationOffset);
                        
                        //CurrentPreview.transform.rotation = Quaternion.FromToRotation(Ray.direction, Hit.normal) * CurrentPreview.transform.rotation;
                    }
                }

                return;
            }

            if (LockRotation)
                CurrentPreview.transform.rotation = GetCaster.rotation * SelectedPrefab.transform.localRotation * Quaternion.Euler(CurrentRotationOffset);
            else
                CurrentPreview.transform.rotation = Quaternion.Euler(CurrentRotationOffset);

            Transform StartTransform = (CurrentPreview.GroundUpperHeight == 0) ? GetCaster : transform;

            Vector3 LookDistance = StartTransform.position + StartTransform.forward * Distance;

            if (CurrentPreview.UseGroundUpper)
            {
                LookDistance.y = Mathf.Clamp(LookDistance.y, GetCaster.position.y - CurrentPreview.GroundUpperHeight,
                    GetCaster.position.y + CurrentPreview.GroundUpperHeight);
            }
            else
            {
                if (!CurrentPreview.FreePlacement)
                {
                    if (Physics.Raycast(CurrentPreview.transform.position + new Vector3(0, 0.3f, 0),
                        Vector3.down, out RaycastHit HitLook, Mathf.Infinity, BuildManager.Instance.FreeLayers, QueryTriggerInteraction.Ignore))
                        LookDistance.y = HitLook.point.y;
                }
                else
                    LookDistance.y = Mathf.Clamp(LookDistance.y, GetCaster.position.y,
                        GetCaster.position.y);
            }

            if (PreviewMovementType == MovementType.Normal)
                CurrentPreview.transform.position = LookDistance;
            else if (PreviewMovementType == MovementType.Grid)
                CurrentPreview.transform.position = MathExtension.PositionToGridPosition(PreviewGridSize, PreviewGridOffset, LookDistance + CurrentPreview.PreviewOffset);
            else if (PreviewMovementType == MovementType.Smooth)
                CurrentPreview.transform.position = Vector3.Lerp(CurrentPreview.transform.position, LookDistance, PreviewSmoothTime * UnityEngine.Time.deltaTime);

            CurrentSocket = null;

            LastSocket = null;

            HasSocket = false;
        }

        Vector3 GetPreviewOffset(Bounds bounds, Vector3 attachPoint, Vector3 orig)
        {
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;

            float x, y, z;

            if (Mathf.Abs(min.x - attachPoint.x) <= Mathf.Abs(max.x - attachPoint.x))
            {
                // Min is closer, attach to min.
                x = orig.x - min.x;// - bounds.size.x;
            }
            else
            {
                // Max is closer, attach to max.
                x = orig.x - max.x;// + bounds.size.x;
            }

            if (Mathf.Abs(min.y - attachPoint.y) <= Mathf.Abs(max.y - attachPoint.y))
            {
                // Min is closer, attach to min.
                y = orig.y - min.y;// - bounds.size.y;
            }
            else
            {
                // Max is closer, attach to max.
                y = orig.y - max.y;// + bounds.size.y;
            }

            if (Mathf.Abs(min.z - attachPoint.z) <= Mathf.Abs(max.z - attachPoint.z))
            {
                // Min is closer, attach to min.
                z = orig.z - min.z;// - bounds.size.z;
            }
            else
            {
                // Max is closer, attach to max.
                z = orig.z - max.z;// + bounds.size.z;
            }

            return new Vector3(x, y, z);
        }

        /// <summary>
        /// This method allows to move the preview only on available socket(s).
        /// </summary>
        public void UpdateSnapsMovementOverlap(PartBehaviour[] parts)
        {
            if (CurrentPreview == null || parts == null)
                return;

            float ClosestAngle = Mathf.Infinity;

            CurrentSocket = null;

            foreach (PartBehaviour Part in parts)
            {
                if (Part == null || Part.Sockets.Length == 0)
                    continue;

                for (int x = 0; x < Part.Sockets.Length; x++)
                {
                    SocketBehaviour Socket = Part.Sockets[x];

                    if (Socket != null)
                    {
                        if (Socket.gameObject.activeSelf && !Socket.IsDisabled)
                        {
                            if (Socket.AllowPart(SelectedPrefab) && !Part.AvoidAnchoredOnSocket)
                            {
                                if ((Socket.transform.position - (CameraType != RayType.TopDown ? GetCaster.position : TopDownHit.point)).sqrMagnitude <
                                    Mathf.Pow(CameraType != RayType.TopDown ? ActionDistance : SnapThreshold, 2))
                                {
                                    float Angle = Vector3.Angle(Ray.direction, Socket.transform.position - Ray.origin);

                                    if (Angle < ClosestAngle && Angle < OverlapAngles)
                                    {
                                        ClosestAngle = Angle;

                                        if (CameraType != RayType.TopDown && CurrentSocket == null)
                                            CurrentSocket = Socket;
                                        else
                                            CurrentSocket = Socket;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (CurrentSocket != null)
            {
                PartOffset Offset = CurrentSocket.GetOffsetPart(SelectedPrefab.Id);

                if (CurrentSocket.CheckOccupancy(SelectedPrefab))
                    return;

                if (Offset != null)
                {
                    CurrentPreview.transform.position = CurrentSocket.transform.position + CurrentSocket.transform.TransformVector(Offset.Position);

                    CurrentPreview.transform.rotation = CurrentSocket.transform.rotation * (CurrentPreview.RotateOnSockets ? Quaternion.Euler(Offset.Rotation + CurrentRotationOffset) : Quaternion.Euler(Offset.Rotation));

                    if (Offset.UseCustomScale)
                        CurrentPreview.transform.localScale = Offset.Scale;

                    LastSocket = CurrentSocket;

                    HasSocket = true;

                    return;
                }
            }

            UpdateFreeMovement();
        }

        /// <summary>
        /// This method allows to move the preview only on available socket(s).
        /// </summary>
        public void UpdateSnapsMovementLine(SocketBehaviour socket)
        {
            if (CurrentPreview == null || socket == null)
                return;

            CurrentSocket = null;

            if (socket != null)
            {
                if (socket.gameObject.activeSelf && !socket.IsDisabled)
                {
                    if (socket.AllowPart(SelectedPrefab) && !CurrentPreview.AvoidAnchoredOnSocket)
                    {
                        CurrentSocket = socket;
                    }
                }
            }

            if (CurrentSocket != null)
            {
                PartOffset Offset = CurrentSocket.GetOffsetPart(SelectedPrefab.Id);

                if (CurrentSocket.CheckOccupancy(SelectedPrefab))
                    return;

                if (Offset != null)
                {
                    CurrentPreview.transform.position = CurrentSocket.transform.position + CurrentSocket.transform.TransformVector(Offset.Position);

                    CurrentPreview.transform.rotation = CurrentSocket.transform.rotation
                        * (CurrentPreview.RotateOnSockets ? Quaternion.Euler(Offset.Rotation + CurrentRotationOffset) : Quaternion.Euler(Offset.Rotation));

                    if (Offset.UseCustomScale)
                        CurrentPreview.transform.localScale = Offset.Scale;

                    LastSocket = CurrentSocket;

                    HasSocket = true;

                    return;
                }
            }

            UpdateFreeMovement();
        }

        /// <summary>
        /// This method allows to check if the preview has collision(s).
        /// </summary>
        public bool HasCollision()
        {
            if (CurrentPreview.CheckTerrainClipping())
                return true;

            if (!HasSocket)
            {
                if (CurrentPreview.CheckBuildSurface())
                    return true;
            }

            if (CurrentPreview.AvoidClipping && !HasSocket)
            {
                if (CurrentPreview.CheckElementsClipping())
                    return true;
            }
            else if (CurrentPreview.AvoidClippingOnSocket && HasSocket)
            {
                if (CurrentPreview.CheckElementsClippingOnSocket())
                    return true;
            }

            if (CurrentPreview.CheckAreas())
                return true;

            if (CurrentPreview.UseConditionalPhysics && CurrentPreview.PhysicsOnlyStablePlacement)
                if (!CurrentPreview.CheckStability())
                    return true;

            return false;
        }

        /// <summary>
        /// This method allows to place the current preview.
        /// </summary>
        public virtual void PlacePrefab()
        {
            if (!AllowPlacement)
                return;

            if (CurrentPreview == null)
                return;

            if (CurrentEditionPreview != null)
                Destroy(CurrentEditionPreview.gameObject);

            if (UseForNetwork)
            {
                BuildManager.Instance.PlacePrefabForNetwork(SelectedPrefab,
                    CurrentPreview.transform.position,
                    CurrentPreview.transform.eulerAngles,
                    CurrentPreview.transform.localScale);
            }
            else
            {
                BuildManager.Instance.PlacePrefab(SelectedPrefab,
                    CurrentPreview.transform.position,
                    CurrentPreview.transform.eulerAngles,
                    CurrentPreview.transform.localScale,
                    null,
                    CurrentSocket);
            }

            if (Source != null)
                if (PlacementClips.Length != 0)
                    Source.PlayOneShot(PlacementClips[Random.Range(0, PlacementClips.Length)]);

            CurrentRotationOffset = Vector3.zero;

            CurrentSocket = null;

            LastSocket = null;

            AllowPlacement = false;

            HasSocket = false;

            if (CurrentMode == BuildMode.Placement && ResetModeAfterPlacement)
                ChangeMode(BuildMode.None);

            if (CurrentPreview != null)
                Destroy(CurrentPreview.gameObject);
        }

        /// <summary>
        /// This method allows to create a preview.
        /// </summary>
        public virtual PartBehaviour CreatePreview(GameObject prefab)
        {
            if (prefab == null)
                return null;

            CurrentPreview = Instantiate(prefab).GetComponent<PartBehaviour>();
            CurrentPreview.transform.eulerAngles = Vector3.zero;
            CurrentRotationOffset = Vector3.zero;

            if (Physics.Raycast(Ray, out RaycastHit Hit, Mathf.Infinity, BuildManager.Instance.FreeLayers, QueryTriggerInteraction.Ignore))
                CurrentPreview.transform.position = Hit.point;

            CurrentPreview.ChangeState(StateType.Preview);

            SelectedPrefab = prefab.GetComponent<PartBehaviour>();

            if (UsePreviewCamera == true)
                CurrentPreview.gameObject.SetLayerRecursively(PreviewLayer);

            EventHandlers.PreviewCreated(CurrentPreview);

            CurrentSocket = null;

            LastSocket = null;

            AllowPlacement = false;

            HasSocket = false;

            foreach (Collider coll in CurrentPreview.GetComponentsInChildren<Collider>())
            {
                Destroy(coll);
            }

            return CurrentPreview;
        }

        /// <summary>
        /// This method allows to clear the current preview.
        /// </summary>
        public virtual void ClearPreview()
        {
            if (CurrentPreview == null)
                return;

            EventHandlers.PreviewCanceled(CurrentPreview);

            Destroy(CurrentPreview.gameObject);

            AllowPlacement = false;

            CurrentPreview = null;
        }

        /// <summary>
        /// This method allows to check if the current preview exists.
        /// </summary>
        public bool PreviewExists()
        {
            return CurrentPreview;
        }

        #endregion Placement

        #region Destruction

        /// <summary>
        /// This method allows to update the destruction preview.
        /// </summary>
        public void UpdateRemovePreview()
        {
            float Distance = OutOfRangeDistance == 0 ? ActionDistance : OutOfRangeDistance;

            if (CurrentRemovePreview != null)
            {
                AreaBehaviour NearestArea = BuildManager.Instance.GetNearestArea(CurrentRemovePreview.transform.position);

                if (NearestArea != null)
                    AllowDestruction = NearestArea.AllowDestruction;
                else
                    AllowDestruction = true;

                CurrentRemovePreview.ChangeState(StateType.Remove);

                AllowPlacement = false;
            }

            if (Physics.Raycast(Ray, out RaycastHit Hit, Distance, BuildManager.Instance.FreeLayers))
            {
                PartBehaviour Part = Hit.collider.GetComponentInParent<PartBehaviour>();

                if (Part != null)
                {
                    if (CurrentRemovePreview != null)
                    {
                        if (CurrentRemovePreview.GetInstanceID() != Part.GetInstanceID())
                        {
                            ClearRemovePreview();

                            CurrentRemovePreview = Part;
                        }
                    }
                    else
                        CurrentRemovePreview = Part;
                }
                else
                    ClearRemovePreview();
            }
            else
                ClearRemovePreview();
        }

        /// <summary>
        /// This method allows to remove the current preview.
        /// </summary>
        public virtual void RemovePrefab()
        {
            if (CurrentRemovePreview == null)
                return;

            if (CurrentRemovePreview.AvoidDestruction)
                return;

            if (!AllowDestruction)
                return;

            Destroy(CurrentRemovePreview.gameObject);

            if (Source != null)
                if (DestructionClips.Length != 0)
                    Source.PlayOneShot(DestructionClips[Random.Range(0, DestructionClips.Length)]);

            CurrentSocket = null;

            LastSocket = null;

            AllowDestruction = false;

            HasSocket = false;

            if (ResetModeAfterDestruction)
                ChangeMode(BuildMode.None);
        }

        /// <summary>
        /// This method allows to clear the current remove preview.
        /// </summary>
        public virtual void ClearRemovePreview()
        {
            if (CurrentRemovePreview == null)
                return;

            CurrentRemovePreview.ChangeState(CurrentRemovePreview.LastState);

            AllowDestruction = false;

            CurrentRemovePreview = null;
        }

        #endregion Destruction

        #region Edition

        /// <summary>
        /// This method allows to update the edition mode.
        /// </summary>
        public void UpdateEditionPreview()
        {
            AllowEdition = CurrentEditionPreview;

            if (CurrentEditionPreview != null && AllowEdition)
                CurrentEditionPreview.ChangeState(StateType.Edit);

            float Distance = OutOfRangeDistance == 0 ? ActionDistance : OutOfRangeDistance;

            if (Physics.Raycast(Ray, out RaycastHit Hit, Distance, BuildManager.Instance.FreeLayers))
            {
                PartBehaviour Part = Hit.collider.GetComponentInParent<PartBehaviour>();

                if (Part != null)
                {
                    if (CurrentEditionPreview != null)
                    {
                        if (CurrentEditionPreview.GetInstanceID() != Part.GetInstanceID())
                        {
                            ClearEditionPreview();

                            CurrentEditionPreview = Part;
                        }
                    }
                    else
                        CurrentEditionPreview = Part;
                }
                else
                {
                    ClearEditionPreview();
                }
            }
            else
                ClearEditionPreview();
        }

        /// <summary>
        /// This method allows to edit the current preview.
        /// </summary>
        public virtual void EditPrefab()
        {
            if (!AllowEdition)
                return;

            PartBehaviour Part = CurrentEditionPreview;

            Part.ChangeState(StateType.Edit);

            EventHandlers.EditedPart(CurrentEditionPreview, CurrentSocket);

            SelectPrefab(Part);

            SelectedPrefab.AppearanceIndex = Part.AppearanceIndex;

            ChangeMode(BuildMode.Placement);
        }

        /// <summary>
        /// This method allows to clear the current edition preview.
        /// </summary>
        public void ClearEditionPreview()
        {
            if (CurrentEditionPreview == null)
                return;

            CurrentEditionPreview.ChangeState(CurrentEditionPreview.LastState);

            AllowEdition = false;

            CurrentEditionPreview = null;
        }

        #endregion Edition

        #region Miscs

        /// <summary>
        /// This method allows to change mode.
        /// </summary>
        public void ChangeMode(BuildMode mode)
        {
            if (CurrentMode == mode)
                return;

            if (mode == BuildMode.Placement && !UsePlacementMode)
                return;

            if (mode == BuildMode.Destruction && !UseDestructionMode)
                return;

            if (mode == BuildMode.Edition && !UseEditionMode)
                return;

            if (CurrentMode == BuildMode.Placement)
                ClearPreview();

            if (CurrentMode == BuildMode.Destruction)
                ClearRemovePreview();

            if (mode == BuildMode.None)
            {
                ClearPreview();
                ClearRemovePreview();
                ClearEditionPreview();
            }

            CurrentMode = mode;

            EventHandlers.BuildModeChanged(CurrentMode);
        }

        /// <summary>
        /// This method allows to select a prefab.
        /// </summary>
        public void SelectPrefab(PartBehaviour prefab)
        {
            if (prefab == null)
                return;

            SelectedPrefab = BuildManager.Instance.GetPart(prefab.Id);
        }

        /// <summary>
        /// This method allows to rotate the current preview.
        /// </summary>
        public void RotatePreview(Vector3 rotateAxis)
        {
            if (CurrentPreview == null)
                return;

            CurrentRotationOffset += rotateAxis;
        }

        /// <summary>
        /// This method allows to get the object that the camera is currently looking at.
        /// </summary>
        public PartBehaviour GetTargetedPart()
        {
            if (Physics.SphereCast(CameraType == RayType.FirstPerson ? new Ray(BuilderCamera.transform.position, BuilderCamera.transform.forward) : Ray,
                .1f, out RaycastHit Hit, ActionDistance, LayerMask.NameToLayer(Constants.LAYER_DEFAULT.ToLower())))
            {
                PartBehaviour Part = Hit.collider.GetComponentInParent<PartBehaviour>();

                if (Part != null)
                    return Part;
            }

            return null;
        }

        #endregion Miscs

        #endregion Public Methods
    }
}