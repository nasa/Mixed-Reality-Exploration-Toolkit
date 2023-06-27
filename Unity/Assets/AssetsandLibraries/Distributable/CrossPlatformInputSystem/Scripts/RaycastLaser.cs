// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.Events;

namespace GOV.NASA.GSFC.XR.CrossPlatformInputSystem
{
    /// <remarks>
    /// History:
    /// 8 December 2020: Created
    /// </remarks>
    /// <summary>
    /// RaycastLaser is a class that provides a "laser"
    /// pointer that matches a cast ray from the InputHand.
    /// Author: Dylan Z. Baker
    /// </summary>
    public class RaycastLaser : MonoBehaviour
    {
        /// <summary>
        /// The type of pointer that is rendered.
        /// </summary>
        public enum PointerType { straight, arc }

        /// <summary>
        /// Whether or not the laser is active.
        /// </summary>
        public bool active
        {
            get
            {
                return _active;
            }
            set
            {
                _active = value;
                Destroy(renderedLine);
            }
        }
        private bool _active;

        /// <summary>
        /// The type of pointer that is rendered.
        /// </summary>
        [Tooltip("The type of pointer that is rendered.")]
        public PointerType pointerType = PointerType.straight;

        /// <summary>
        /// The width of the line.
        /// </summary>
        [Tooltip("The width of the line.")]
        public float width = 0.01f;

        /// <summary>
        /// Maximum length of laser.
        /// </summary>
        [Tooltip("Maximum length of laser.")]
        public float maxLength = 100f;

        /// <summary>
        /// The prefab to use for the cursor that gets placed at the end of the pointer.
        /// </summary>
        [Tooltip("The prefab to use for the cursor that gets placed at the end of the pointer.")]
        public GameObject cursorPrefab;

        /// <summary>
        /// The scale to apply to the cursor.
        /// </summary>
        [Tooltip("The scale to apply to the cursor.")]
        public Vector3 cursorScale;

        /// <summary>
        /// The color applied to the line when hitting.
        /// </summary>
        [Tooltip("The color applied to the line when hitting.")]
        public Material validMaterial;

        /// <summary>
        /// The color to apply to the line when not hitting.
        /// </summary>
        [Tooltip("The color to apply to the line when not hitting.")]
        public Material invalidMaterial;

        /// <summary>
        /// Layer Mask to use for raycast.
        /// </summary>
        [Tooltip("Layer Mask to use for raycast.")]
        public LayerMask layerMask;

        /// <summary>
        /// Reference to the rendered line.
        /// </summary>
        public LineRenderer renderedLine { get; private set; }

        /// <summary>
        /// Whether or not the laser is hitting something.
        /// </summary>
        public bool isHitting { get; private set; }

        /// <summary>
        /// The current raycasting position.
        /// </summary>
        public Vector3 hitPos { get; private set; }

        /// <summary>
        /// The current raycasting object.
        /// </summary>
        public GameObject hitObj { get; private set; }

        /// <summary>
        /// Whether or not to show the pointer when in invalid state.
        /// </summary>
        [Tooltip("Whether or not to show the pointer when in invalid state.")]
        public bool showInvalid;

        /// <summary>
        /// Event to call on hover begin.
        /// </summary>
        [Tooltip("Event to call on hover begin.")]
        public UnityEvent onHoverBegin;

        /// <summary>
        /// Event to call on hover end.
        /// </summary>
        [Tooltip("Event to call on hover end.")]
        public UnityEvent onHoverEnd;

        /// <summary>
        /// The current cursor.
        /// </summary>
        private GameObject cursorObject;

        void Update()
        {
            if (_active)
            {
                // Ensure that line exists.
                if (renderedLine == null)
                {
                    renderedLine = gameObject.GetComponent<LineRenderer>();
                    if (renderedLine == null)
                    {
                        renderedLine = gameObject.AddComponent<LineRenderer>();
                    }
                    renderedLine.generateLightingData = true;
                }

                // Update line properties.
                renderedLine.startWidth = renderedLine.endWidth = width;

                bool invokeHit = false, invokeUnhit = false;

                // Update the raycast information.
                RaycastHit hit;
                if (Physics.Raycast(transform.position, transform.forward, out hit, maxLength, layerMask))
                {
                    // Ensure that the cursor exists.
                    if (cursorObject == null)
                    {
                        cursorObject = Instantiate(cursorPrefab);
                        cursorObject.name = "Cursor";
                        cursorObject.transform.SetParent(transform);
                        cursorObject.transform.localScale = cursorScale;
                        MeshRenderer rend = cursorObject.GetComponent<MeshRenderer>();
                        if (rend)
                        {
                            rend.material = validMaterial;
                        }
                    }

                    // If just started hitting, send message.
                    if (hit.collider.gameObject != null)
                    {
                        if (hitObj != hit.collider.gameObject)
                        {
                            if (hitObj != null)
                            {
                                hitObj.SendMessage("StoppedHitting", SendMessageOptions.DontRequireReceiver);
                                invokeUnhit = true;
                            }
                            hit.collider.gameObject.SendMessage("StartedHitting", SendMessageOptions.DontRequireReceiver);
                            invokeHit = true;
                        }
                    }
                    isHitting = true;
                    hitPos = hit.point;
                    hitObj = hit.collider.gameObject;
                    renderedLine.material = validMaterial;
                }
                else
                {
                    // If was hitting, send message.
                    if (hitObj != null)
                    {
                        hitObj.SendMessage("StoppedHitting", SendMessageOptions.DontRequireReceiver);
                        invokeUnhit = true;
                    }

                    isHitting = false;
                    if (showInvalid == true)
                    {
                        hitPos = transform.position + transform.forward * maxLength;
                    }
                    else
                    {
                        hitPos = transform.position;
                    }
                    hitObj = null;
                    renderedLine.material = invalidMaterial;

                    // Destroy cursor object.
                    if (cursorObject != null)
                    {
                        Destroy(cursorObject);
                    }
                }
                if (cursorObject)
                {
                    cursorObject.transform.position = hitPos;
                }

                // Update the line renderer points
                switch (pointerType)
                {
                    case PointerType.arc:
                        // UMD: Rod, Olivia, and Kevin update to create bezier curve for teleportation
                        if (hitPos != transform.position)
                        {
                            renderedLine.positionCount = 200;
                            var point0 = transform.position;
                            var point2 = hitPos;
                            var point_mid = (transform.position + hitPos) / 2;
                            point_mid[1] *= 4;

                            float t = 0f;
                            Vector3 B = new Vector3(0, 0, 0);
                            for (int i = 0; i < renderedLine.positionCount; i++)
                            {
                                B = (1 - t) * (1 - t) * point0 + 2 * (1 - t) * t * point_mid + t * t * point2;
                                renderedLine.SetPosition(i, B);
                                t += (1 / (float)renderedLine.positionCount);
                            }
                        }
                        break;

                    case PointerType.straight:
                    default:
                        renderedLine.SetPositions(new Vector3[] { transform.position, hitPos });
                        break;
                }

                // Send events.
                if (invokeHit == true)
                {
                    if (onHoverBegin != null)
                    {
                        onHoverBegin.Invoke();
                    }
                }
                else if (invokeUnhit == true)
                {
                    if (onHoverEnd != null)
                    {
                        onHoverEnd.Invoke();
                    }
                }
            }
            else
            {
                // Reset the state
                isHitting = false;
                hitPos = Vector3.zero;
                hitObj = null;

                // Ensure that the line doesn't exist.
                if (renderedLine != null)
                {
                    Destroy(renderedLine);
                }

                // Destroy cursor object.
                if (cursorObject != null)
                {
                    Destroy(cursorObject);
                }
            }
        }

        private void OnDestroy()
        {
            if (cursorObject != null)
            {
                Destroy(cursorObject);
            }
        }
    }
}