// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using EasyBuildSystem.Features.Scripts.Core.Base.Builder.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Event;
using EasyBuildSystem.Features.Scripts.Core.Base.Group;
using EasyBuildSystem.Features.Scripts.Core.Base.Manager;
using EasyBuildSystem.Features.Scripts.Core.Base.Piece;
using EasyBuildSystem.Features.Scripts.Core.Base.Piece.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Socket;
using EasyBuildSystem.Features.Scripts.Core.Base.Socket.Data;
using EasyBuildSystem.Features.Scripts.Extensions;
using System;
using UnityEngine;
using EasyBuildSystem.Features.Scripts.Core;

namespace GSFC.ARVR.MRET.Alignment
{
    [DefaultExecutionOrder(999)]
    [AddComponentMenu("Easy Build System/Features/Builders Behaviour/Builder Behaviour")]
    public class VRBuilderBehaviour : MonoBehaviour
    {
        #region Fields

        public static VRBuilderBehaviour Instance;

        public LayerMask RaycastLayer = 1 << 0;
        public float RaycastTopDownSnapThreshold = 5f;

        public float ActionDistance = 6f;
        public float SnapThreshold = 5f;
        public float OutOfRangeDistance = 0f;

        public float OverlapAngles = 35f;
        public bool LockRotation;
        public DetectionType RayDetection = DetectionType.Overlap;
        public RaycastType CameraType;
        public float SocketDetectionMaxAngles = 35f;
        public float RaycastActionDistance = 10f;
        public float RaycastMaxDistance = 0f;
        public Vector3 RaycastOffset = new Vector3(0, 0, 1);
        public Transform RaycastOriginTransform;
        public Transform RaycastAnchorTransform;
        public float raySteps = 0.1f;

        public MovementType PreviewMovementType;
        public bool PreviewMovementOnlyAllowed;
        public float PreviewGridSize = 1.0f;
        public float PreviewGridOffset;
        public float PreviewSmoothTime = 5.0f;

        public bool UsePlacementMode = true;
        public bool ResetModeAfterPlacement = false;
        public bool UseDestructionMode = true;
        public bool ResetModeAfterDestruction = false;
        public bool UseEditionMode = true;
        public bool ResetModeAfterEdition = false;

        public AudioSource Source;
        public AudioClip[] PlacementClips;
        public AudioClip[] DestructionClips;
        public AudioClip[] EditionClips;


        private Vector3 lastPos = Vector3.zero;
        private Quaternion lastRot = Quaternion.identity;
        private Ray? lastRay = null;
        private int rayUpdateCounter = 0;
        public virtual Ray? GetRay
        {
            get
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
                            Physics.Raycast(currentRay, out hit, OutOfRangeDistance, RaycastLayer);
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
            }
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

        private Transform _Caster;
        public virtual Transform GetCaster
        {
            get
            {
                if (_Caster == null)
                {
                    _Caster = CameraType != RaycastType.TopDown ? transform.parent != null ? transform.parent :
                        transform : RaycastAnchorTransform != null ? RaycastAnchorTransform : transform;
                }

                return _Caster;
            }
        }

        public BuildMode CurrentMode { get; set; }
        public BuildMode LastMode { get; set; }

        public PieceBehaviour SelectedPrefab { get; set; }

        public PieceBehaviour CurrentPreview { get; set; }
        public PieceBehaviour CurrentEditionPreview { get; set; }
        public PieceBehaviour CurrentRemovePreview { get; set; }

        public Vector3 CurrentRotationOffset { get; set; }

        public Vector3 InitialScale { get; set; }

        public bool AllowPlacement { get; set; }
        public bool AllowDestruction { get; set; }
        public bool AllowEdition { get; set; }

        public bool HasSocket { get; set; }

        public SocketBehaviour CurrentSocket { get; set; }
        public SocketBehaviour LastSocket { get; set; }

        public GameObject placingObject { get; set; }

        private RaycastHit TopDownHit;
        private Vector3 LastAllowedPosition;

        private readonly RaycastHit[] Hits = new RaycastHit[PhysicExtension.MAX_ALLOC_COUNT];

        public Vector3 CurrentPreviewRotationOffset { get; set; }

        #endregion Fields

        #region Methods

        public virtual void Awake()
        {
            Instance = this;
        }

        public virtual void Start()
        {
            if (!Application.isPlaying)
            {
                return;
            }
        }

        public virtual void Update()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            UpdateModes();
        }

        #region Placement

        /// <summary>
        /// This method allows to update the placement preview.
        /// </summary>
        public void UpdatePreview()
        {
            HasSocket = false;

            if (CameraType == RaycastType.TopDown)
            {
                if (GetRay.HasValue)
                {
                    Ray ray = GetRay.Value;
                    Physics.Raycast(ray, out TopDownHit, Mathf.Infinity, LayerMask.GetMask(Constants.LAYER_SOCKET), QueryTriggerInteraction.Ignore);
                }
            }

            if (RayDetection == DetectionType.Raycast)
            {
                SocketBehaviour[] NeighboursSockets =
                    BuildManager.Instance.GetAllNearestSockets(transform.TransformPoint(Vector3.forward * ActionDistance), ActionDistance);

                SocketBehaviour[] ApplicableSockets = new SocketBehaviour[NeighboursSockets.Length];

                for (int i = 0; i < NeighboursSockets.Length; i++)
                {
                    if (NeighboursSockets[i] == null)
                    {
                        continue;
                    }

                    foreach (SocketBehaviour Socket in NeighboursSockets)
                    {
                        if (NeighboursSockets[i].gameObject.activeInHierarchy && !Socket.IsDisabled && Socket.AllowPiece(CurrentPreview))
                        {
                            ApplicableSockets[i] = NeighboursSockets[i];
                            break;
                        }
                    }
                }

                if (ApplicableSockets.Length > 0)
                {
                    UpdateMultipleSocket(ApplicableSockets);
                }
                else
                {
                    UpdateFreeMovement();
                }
            }
            else if (RayDetection == DetectionType.Raycast)
            {
                SocketBehaviour Socket = null;

                int ColliderCount = Physics.RaycastNonAlloc(GetRay.Value, Hits, RaycastActionDistance, LayerMask.GetMask(Constants.LAYER_SOCKET));
                for (int i = 0; i < ColliderCount; i++)
                {
                    if (Hits[i].collider.GetComponent<SocketBehaviour>() != null)
                    {
                        Socket = Hits[i].collider.GetComponent<SocketBehaviour>();
                    }
                }

                if (Socket != null)
                {
                    UpdateSingleSocket(Socket);
                }
                else
                {
                    UpdateFreeMovement();
                }
            }
            else if (RayDetection == DetectionType.Overlap)
            {
                SocketBehaviour[] NeighboursSockets =
                    BuildManager.Instance.GetAllNearestSockets(GetCaster.position, RaycastActionDistance);

                SocketBehaviour[] ApplicableSockets = new SocketBehaviour[NeighboursSockets.Length];

                for (int i = 0; i < NeighboursSockets.Length; i++)
                {
                    if (NeighboursSockets[i] == null)
                    {
                        continue;
                    }

                    foreach (SocketBehaviour Socket in NeighboursSockets)
                    {
                        if (NeighboursSockets[i].gameObject.activeInHierarchy && !Socket.IsDisabled && Socket.AllowPiece(CurrentPreview))
                        {
                            ApplicableSockets[i] = NeighboursSockets[i];
                            break;
                        }
                    }
                }

                if (ApplicableSockets.Length > 0)
                {
                    UpdateMultipleSocket(ApplicableSockets);
                }
                else
                {
                    UpdateFreeMovement();
                }
            }

            CurrentPreview.gameObject.ChangeAllMaterialsColorInChildren(CurrentPreview.Renderers.ToArray(),
                CheckPlacementConditions() ? CurrentPreview.PreviewAllowedColor : CurrentPreview.PreviewDeniedColor, SelectedPrefab.PreviewColorLerpTime);
        }

        /// <summary>
        /// This method allows to check the internal placement conditions.
        /// </summary>
        public bool CheckPlacementConditions()
        {
            if (CurrentPreview == null)
            {
                return false;
            }

            if (RaycastMaxDistance != 0)
            {
                if (Vector3.Distance(GetCaster.position, CurrentPreview.transform.position) > RaycastActionDistance)
                {
                    return false;
                }
            }

            if (!CurrentPreview.CheckExternalPlacementConditions())
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// This method allows to rotate the current preview.
        /// </summary>
        public void RotatePreview(Vector3 rotateAxis)
        {
            if (CurrentPreview == null)
            {
                return;
            }

            CurrentRotationOffset += rotateAxis;
        }

        /// <summary>
        /// This method allows to move the preview in free movement.
        /// </summary>
        public void UpdateFreeMovement()
        {
            if (CurrentPreview == null)
            {
                return;
            }

            float Distance = OutOfRangeDistance == 0 ? ActionDistance : OutOfRangeDistance;

            if (GetRay.HasValue)
            {
                Ray ray = GetRay.Value;
                if (Physics.Raycast(ray, out RaycastHit Hit, Distance, RaycastLayer))
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

                            if (CheckPlacementConditions())
                            {
                                LastAllowedPosition = Hit.point + CurrentPreview.PreviewOffset;
                            }
                            else
                            {
                                CurrentPreview.transform.position = LastAllowedPosition;
                            }
                        }
                        else
                        {
                            CurrentPreview.transform.position = Hit.point + CurrentPreview.PreviewOffset;
                        }
                    }
                    else if (PreviewMovementType == MovementType.Grid)
                    {
                        CurrentPreview.transform.position = MathExtension.PositionToGridPosition(PreviewGridSize, PreviewGridOffset, Hit.point + CurrentPreview.PreviewOffset);
                    }
                    else if (PreviewMovementType == MovementType.Smooth)
                    {
                        CurrentPreview.transform.position = Vector3.Lerp(CurrentPreview.transform.position, Hit.point + CurrentPreview.PreviewOffset, PreviewSmoothTime * UnityEngine.Time.deltaTime);
                    }

                    if (!CurrentPreview.RotateAccordingSlope)
                    {
                        if (LockRotation)
                        {
                            CurrentPreview.transform.rotation = GetCaster.rotation * SelectedPrefab.transform.localRotation * Quaternion.Euler(CurrentRotationOffset);
                        }
                        else
                        {
                            CurrentPreview.transform.rotation = Quaternion.Euler(CurrentRotationOffset);
                        }
                    }
                    else
                    {
                        if (Hit.collider is TerrainCollider)
                        {
                            float SampleHeight = Hit.collider.GetComponent<UnityEngine.Terrain>().SampleHeight(Hit.point);

                            if (Hit.point.y - .1f < SampleHeight)
                            {
                                CurrentPreview.transform.rotation = Quaternion.FromToRotation(GetCaster.up, Hit.normal) * Quaternion.Euler(CurrentRotationOffset) *
                                    GetCaster.rotation * SelectedPrefab.transform.localRotation * Quaternion.Euler(CurrentRotationOffset);
                            }
                            else
                            {
                                CurrentPreview.transform.rotation = GetCaster.rotation * SelectedPrefab.transform.localRotation * Quaternion.Euler(CurrentRotationOffset);
                            }
                        }
                        else
                        {
                            CurrentPreview.transform.rotation = GetCaster.rotation * SelectedPrefab.transform.localRotation * Quaternion.Euler(CurrentRotationOffset);
                        }
                    }

                    return;
                }
            }

            if (LockRotation)
            {
                CurrentPreview.transform.rotation = GetCaster.rotation * SelectedPrefab.transform.localRotation * Quaternion.Euler(CurrentRotationOffset);
            }
            else
            {
                CurrentPreview.transform.rotation = Quaternion.Euler(CurrentRotationOffset);
            }

            Transform StartTransform = (CurrentPreview.GroundUpperHeight == 0) ? GetCaster : transform;

            Vector3 LookDistance = StartTransform.position + StartTransform.forward * Distance;

            if (CurrentPreview.UseGroundUpper)
            {
                LookDistance.y = Mathf.Clamp(LookDistance.y, GetCaster.position.y - CurrentPreview.GroundUpperHeight,
                    GetCaster.position.y + CurrentPreview.GroundUpperHeight);
            }
            else
            {
                if (!CurrentPreview.UseGroundUpper)
                {
                    if (Physics.Raycast(CurrentPreview.transform.position + new Vector3(0, 0.3f, 0),
                            Vector3.down, out RaycastHit HitLook, Mathf.Infinity, RaycastLayer, QueryTriggerInteraction.Ignore))
                    {
                        LookDistance.y = HitLook.point.y;
                    }
                }
                else
                {
                    LookDistance.y = Mathf.Clamp(LookDistance.y, GetCaster.position.y,
                        GetCaster.position.y);
                }
            }

            if (PreviewMovementType == MovementType.Normal)
            {
                CurrentPreview.transform.position = LookDistance;
            }
            else if (PreviewMovementType == MovementType.Grid)
            {
                CurrentPreview.transform.position = MathExtension.PositionToGridPosition(PreviewGridSize, PreviewGridOffset, LookDistance + CurrentPreview.PreviewOffset);
            }
            else if (PreviewMovementType == MovementType.Smooth)
            {
                CurrentPreview.transform.position = Vector3.Lerp(CurrentPreview.transform.position, LookDistance, PreviewSmoothTime * UnityEngine.Time.deltaTime);
            }

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
        public void UpdateMultipleSocket(SocketBehaviour[] sockets)
        {
            if (CurrentPreview == null || sockets == null)
            {
                return;
            }

            float closestAngle = Mathf.Infinity;

            CurrentSocket = null;

            RaycastHit raycastTopDown = new RaycastHit();

            if (CameraType == RaycastType.TopDown)
                Physics.Raycast(GetRay.Value, out raycastTopDown, Mathf.Infinity, LayerMask.GetMask(Constants.LAYER_SOCKET), QueryTriggerInteraction.Ignore);

            foreach (SocketBehaviour Socket in sockets)
            {
                if (Socket != null)
                {
                    if (Socket.gameObject.activeSelf && !Socket.IsDisabled)
                    {
                        if (Socket.AllowPiece(CurrentPreview) && !CurrentPreview.IgnoreSocket)
                        {
                            if ((Socket.transform.position - (CameraType != RaycastType.TopDown ? GetCaster.position : raycastTopDown.point)).sqrMagnitude <
                                Mathf.Pow(CameraType != RaycastType.TopDown ? RaycastActionDistance : RaycastTopDownSnapThreshold, 2))
                            {
                                float angle = Vector3.Angle(GetRay.Value.direction, Socket.transform.position - GetRay.Value.origin);

                                if (angle < closestAngle && angle < SocketDetectionMaxAngles)
                                {
                                    closestAngle = angle;

                                    if (CameraType != RaycastType.TopDown && CurrentSocket == null)
                                    {
                                        CurrentSocket = Socket;
                                    }
                                    else
                                    {
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
                Offset Offset = CurrentSocket.GetOffset(CurrentPreview);

                if (Offset != null)
                {
                    CurrentPreview.transform.position = CurrentSocket.transform.position + CurrentSocket.transform.TransformVector(Offset.Position);

                    CurrentPreview.transform.rotation = CurrentSocket.transform.rotation *
                        (CurrentPreview.RotateOnSockets ? Quaternion.Euler(Offset.Rotation + CurrentPreviewRotationOffset) : Quaternion.Euler(Offset.Rotation));

                    if (Offset.Scale != Vector3.one)
                    {
                        CurrentPreview.transform.localScale = Offset.Scale * 1.005f;
                    }

                    HasSocket = true;

                    if (!CheckPlacementConditions())
                    {
                        HasSocket = false;
                    }
                    else
                    {
                        LastSocket = CurrentSocket;
                        HasSocket = true;
                        return;
                    }
                }
            }

            UpdateFreeMovement();
        }

        /// <summary>
        /// This method allows to move the preview only on available socket.
        /// </summary>
        public void UpdateSingleSocket(SocketBehaviour socket)
        {
            if (CurrentPreview == null || socket == null)
            {
                return;
            }

            CurrentSocket = null;

            if (socket != null)
            {
                if (socket.gameObject.activeSelf && !socket.IsDisabled)
                {
                    if (socket.AllowPiece(CurrentPreview) && !CurrentPreview.IgnoreSocket)
                    {
                        CurrentSocket = socket;
                    }
                }
            }

            if (CurrentSocket != null)
            {
                Offset Offset = CurrentSocket.GetOffset(CurrentPreview);

                if (Offset != null)
                {
                    CurrentPreview.transform.position = CurrentSocket.transform.position + CurrentSocket.transform.TransformVector(Offset.Position);

                    CurrentPreview.transform.rotation = CurrentSocket.transform.rotation *
                        (CurrentPreview.RotateOnSockets ? Quaternion.Euler(Offset.Rotation + CurrentRotationOffset) : Quaternion.Euler(Offset.Rotation));

                    if (Offset.Scale != Vector3.one)
                    {
                        CurrentPreview.transform.localScale = Offset.Scale;
                    }

                    LastSocket = CurrentSocket;

                    HasSocket = true;

                    return;
                }
            }

            UpdateFreeMovement();
        }

        /// <summary>
        /// This method allows to place the current preview.
        /// </summary>
        public virtual void PlacePrefab(GroupBehaviour group = null)
        {
            if (!AllowPlacement)
            {
                return;
            }

            if (CurrentPreview == null)
            {
                return;
            }

            if (CurrentEditionPreview != null)
            {
                Destroy(CurrentEditionPreview.gameObject);
            }

            BuildManager.Instance.PlacePrefab(SelectedPrefab,
                CurrentPreview.transform.position,
                CurrentPreview.transform.eulerAngles,
                CurrentPreview.transform.localScale,
                group,
                CurrentSocket);

            if (Source != null)
            {
                if (PlacementClips.Length != 0)
                {
                    Source.PlayOneShot(PlacementClips[UnityEngine.Random.Range(0, PlacementClips.Length)]);
                }
            }

            CurrentRotationOffset = Vector3.zero;
            CurrentSocket = null;
            LastSocket = null;
            AllowPlacement = false;
            HasSocket = false;

            if (LastMode == BuildMode.Edit && ResetModeAfterEdition)
            {
                ChangeMode(BuildMode.None);
            }

            if (CurrentMode == BuildMode.Placement && ResetModeAfterPlacement)
            {
                ChangeMode(BuildMode.None);
            }

            if (CurrentPreview != null)
            {
                Destroy(CurrentPreview.gameObject);
            }
        }

        /// <summary>
        /// This method allows to create a preview.
        /// </summary>
        public virtual PieceBehaviour CreatePreview(GameObject prefab)
        {
            if (prefab == null)
            {
                return null;
            }

            CurrentPreview = Instantiate(prefab).GetComponent<PieceBehaviour>();
            CurrentPreview.transform.eulerAngles = Vector3.zero;
            CurrentRotationOffset = Vector3.zero;

            InitialScale = CurrentPreview.transform.localScale;

            if (GetRay.HasValue)
            {
                Ray ray = GetRay.Value;
                if (Physics.Raycast(ray, out RaycastHit Hit, Mathf.Infinity, RaycastLayer, QueryTriggerInteraction.Ignore))
                {
                    CurrentPreview.transform.position = Hit.point;
                }
            }

            CurrentPreview.ChangeState(StateType.Preview);

            SelectedPrefab = prefab.GetComponent<PieceBehaviour>();

            BuildEvent.Instance.OnPieceInstantiated.Invoke(CurrentPreview, null);

            CurrentSocket = null;

            LastSocket = null;

            AllowPlacement = false;

            HasSocket = false;

            return CurrentPreview;
        }

        /// <summary>
        /// This method allows to clear the current preview.
        /// </summary>
        public virtual void ClearPreview()
        {
            if (CurrentPreview == null)
            {
                return;
            }

            BuildEvent.Instance.OnPieceDestroyed.Invoke(CurrentPreview);

            Destroy(CurrentPreview.gameObject);

            AllowPlacement = false;

            CurrentPreview = null;
        }

        /// <summary>
        /// This method allows to get the piece that the camera is currently looking at.
        /// </summary>
        public PieceBehaviour GetTargetedPart()
        {
            Ray ray = GetRay.Value;
            if (Physics.SphereCast(CameraType == RaycastType.FirstPerson ? new Ray(transform.position, transform.forward) : ray,
                    .1f, out RaycastHit Hit, ActionDistance, Physics.AllLayers))
            {
                PieceBehaviour Part = Hit.collider.GetComponentInParent<PieceBehaviour>();

                if (Part != null)
                    return Part;
            }

            return null;
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
                CurrentRemovePreview.ChangeState(StateType.Remove);

                AllowPlacement = false;
            }

            Ray ray = GetRay.Value;
            if (Physics.Raycast(ray, out RaycastHit Hit, Distance, RaycastLayer))
            {
                PieceBehaviour Part = Hit.collider.GetComponentInParent<PieceBehaviour>();

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
                    {
                        CurrentRemovePreview = Part;
                    }
                }
                else
                {
                    ClearRemovePreview();
                }
            }
            else
            {
                ClearRemovePreview();
            }
        }

        /// <summary>
        /// This method allows to remove the current preview.
        /// </summary>
        public virtual void DestroyPrefab()
        {
            AllowDestruction = CheckDestructionConditions();

            if (!AllowDestruction)
            {
                return;
            }

            Destroy(CurrentRemovePreview.gameObject);

            if (Source != null)
            {
                if (DestructionClips.Length != 0)
                {
                    Source.PlayOneShot(DestructionClips[UnityEngine.Random.Range(0, DestructionClips.Length)]);
                }
            }

            CurrentSocket = null;

            LastSocket = null;

            AllowDestruction = false;

            HasSocket = false;

            if (ResetModeAfterDestruction)
            {
                ChangeMode(BuildMode.None);
            }
        }

        /// <summary>
        /// This method allows to check the internal destruction conditions.
        /// </summary>
        public bool CheckDestructionConditions()
        {
            if (CurrentRemovePreview == null)
            {
                return false;
            }

            if (!CurrentRemovePreview.CheckExternalDestructionConditions())
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// This method allows to clear the current remove preview.
        /// </summary>
        public virtual void ClearRemovePreview()
        {
            if (CurrentRemovePreview == null)
            {
                return;
            }

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
            {
                CurrentEditionPreview.ChangeState(StateType.Edit);
            }

            float Distance = OutOfRangeDistance == 0 ? ActionDistance : OutOfRangeDistance;

            Ray ray = GetRay.Value;
            if (Physics.Raycast(ray, out RaycastHit Hit, Distance, RaycastLayer))
            {
                PieceBehaviour Part = Hit.collider.GetComponentInParent<PieceBehaviour>();

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
                    {
                        CurrentEditionPreview = Part;
                    }
                }
                else
                {
                    ClearEditionPreview();
                }
            }
            else
            {
                ClearEditionPreview();
            }
        }

        /// <summary>
        /// This method allows to edit the current preview.
        /// </summary>
        public virtual void EditPrefab()
        {
            AllowEdition = CheckEditionConditions();

            if (!AllowEdition)
            {
                return;
            }

            PieceBehaviour Part = CurrentEditionPreview;

            Part.ChangeState(StateType.Edit);

            SelectPrefab(Part);

            SelectedPrefab.SkinIndex = Part.SkinIndex;

            ChangeMode(BuildMode.Placement);
        }

        /// <summary>
        /// This method allows to check the internal edition conditions.
        /// </summary>
        public bool CheckEditionConditions()
        {
            if (CurrentEditionPreview == null)
            {
                return false;
            }

            if (!CurrentEditionPreview.CheckExternalEditionConditions())
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// This method allows to clear the current edition preview.
        /// </summary>
        public void ClearEditionPreview()
        {
            if (CurrentEditionPreview == null)
            {
                return;
            }

            CurrentEditionPreview.ChangeState(CurrentEditionPreview.LastState);

            AllowEdition = false;

            CurrentEditionPreview = null;
        }

        #endregion Edition

        /// <summary>
        /// This method allows to update all the builder (Placement, Destruction, Edition).
        /// </summary>
        public virtual void UpdateModes()
        {
            if (BuildManager.Instance == null)
            {
                return;
            }

            if (BuildManager.Instance.Pieces == null)
            {
                return;
            }

            if (SelectedPrefab == null && placingObject != null)
            {
                SelectedPrefab = placingObject.GetComponent<PieceBehaviour>();
                SelectedPrefab.RotateAccordingSlope = true;
            }

            if (CurrentMode == BuildMode.Placement)
            {
                if (SelectedPrefab == null)
                {
                    return;
                }

                if (CurrentPreview == null)
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
            {
                UpdateRemovePreview();
            }
            else if (CurrentMode == BuildMode.Edit)
            {
                UpdateEditionPreview();
            }
            else if (CurrentMode == BuildMode.None)
            {
                ClearPreview();
            }
        }

        /// <summary>
        /// This method allows to change mode.
        /// </summary>
        public void ChangeMode(BuildMode mode)
        {
            if (CurrentMode == mode)
            {
                return;
            }

            if (mode == BuildMode.Placement && !UsePlacementMode)
            {
                return;
            }

            if (mode == BuildMode.Destruction && !UseDestructionMode)
            {
                return;
            }

            if (mode == BuildMode.Edit && !UseEditionMode)
            {
                return;
            }

            if (CurrentMode == BuildMode.Placement)
            {
                ClearPreview();
            }

            if (CurrentMode == BuildMode.Destruction)
            {
                ClearRemovePreview();
            }

            if (mode == BuildMode.None)
            {
                ClearPreview();
                ClearRemovePreview();
                ClearEditionPreview();
            }

            LastMode = CurrentMode;

            CurrentMode = mode;

            BuildEvent.Instance.OnChangedBuildMode.Invoke(CurrentMode);
        }

        /// <summary>
        /// This method allows to change mode.
        /// </summary>
        public void ChangeMode(string modeName)
        {
            if (CurrentMode.ToString() == modeName)
            {
                return;
            }

            if (modeName == BuildMode.Placement.ToString() && !UsePlacementMode)
            {
                return;
            }

            if (modeName == BuildMode.Destruction.ToString() && !UseDestructionMode)
            {
                return;
            }

            if (modeName == BuildMode.Edit.ToString() && !UseEditionMode)
            {
                return;
            }

            if (CurrentMode == BuildMode.Placement)
            {
                ClearPreview();
            }

            if (CurrentMode == BuildMode.Destruction)
            {
                ClearRemovePreview();
            }

            if (modeName == BuildMode.None.ToString())
            {
                ClearPreview();
                ClearRemovePreview();
                ClearEditionPreview();
            }

            LastMode = CurrentMode;

            CurrentMode = (BuildMode)Enum.Parse(typeof(BuildMode), modeName);

            BuildEvent.Instance.OnChangedBuildMode.Invoke(CurrentMode);
        }

        /// <summary>
        /// This method allows to select a prefab.
        /// </summary>
        public void SelectPrefab(PieceBehaviour prefab)
        {
            if (prefab == null)
            {
                return;
            }

            SelectedPrefab = BuildManager.Instance.GetPieceById(prefab.Id);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.magenta;

            if (GetRay.HasValue)
            {
                Ray ray = GetRay.Value;
                Gizmos.DrawLine(ray.origin, ray.direction * ActionDistance);
            }
        }

        #endregion Methods
    }
}