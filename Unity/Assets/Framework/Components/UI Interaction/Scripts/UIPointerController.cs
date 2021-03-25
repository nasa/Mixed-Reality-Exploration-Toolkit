// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem;

namespace GSFC.ARVR.MRET.Infrastructure.Components.UIInteraction
{
    /// <remarks>
    /// History:
    /// 22 January 2021: Created
    /// </remarks>
    /// <summary>
    /// UIPointerController is a controller class that
    /// handles UI pointing for one hand. It should be included
    /// with the InputHand.
    /// Author: Dylan Z. Baker
    /// </summary>
    public class UIPointerController : MonoBehaviour
    {
        /// <summary>
        /// Hand to attach laser to.
        /// </summary>
        [Tooltip("Hand to attach laser to.")]
        public InputHand hand;

        /// <summary>
        /// Layer mask to use for the laser.
        /// </summary>
        [Tooltip("Layer mask to use for the laser.")]
        public LayerMask laserMask;

        /// <summary>
        /// Raycast laser to use.
        /// </summary>
        public RaycastLaser raycastLaser { get; private set; }

        /// <summary>
        /// Whether or not to show the pointer when in invalid state.
        /// </summary>
        public bool showInvalid
        {
            set
            {
                if (raycastLaser)
                {
                    raycastLaser.showInvalid = value;
                }
            }
        }
        private bool _showInvalid = false;

        /// <summary>
        /// Event to call when pointing begins.
        /// </summary>
        [Tooltip("Event to call when pointing begins.")]
        public UnityEvent onPoint;

        /// <summary>
        /// Event to call when pointing ends.
        /// </summary>
        [Tooltip("Event to call when pointing ends.")]
        public UnityEvent onStopPointing;

        private bool hardEnable = false;

        private Dropdown dropdownToDisable = null;

        private Vector3 hitPoint;

        private bool isScrolling = false;

        private Slider slidingSlider = null;

        /// <summary>
        /// Toggles the UI pointer on.
        /// </summary>
        /// <param name="soft">Whether or not this is a soft disable. Soft enables can be disabled with soft
        /// or hard disables, while hard enables can only be disabled with hard disables.</param>
        public void ToggleUIPointingOn(bool soft)
        {
            if (raycastLaser == null)
            {
                Debug.LogError("[UIPointerController] UI Pointer state error.");
                return;
            }

            hardEnable = !soft;
            raycastLaser.active = true;
        }

        /// <summary>
        /// Toggles the UI pointer off.
        /// </summary>
        /// <param name="soft">Whether or not this is a soft disable. Soft enables can be disabled with soft
        /// or hard disables, while hard enables can only be disabled with hard disables.</param>
        public void ToggleUIPointingOff(bool soft)
        {
            if (raycastLaser == null)
            {
                //Debug.LogError("[UIPointerController] UI pointer state error.");
                return;
            }

            if (hardEnable == false || soft == false)
            {
                raycastLaser.active = false;
            }

            FinishSelecting();
        }

        /// <summary>
        /// Performs the UI pointer operation.
        /// </summary>
        public void Select()
        {
            isScrolling = true;

            if (raycastLaser == null)
            {
                return;
            }

            if (raycastLaser.isHitting)
            {
                // Disable previous dropdown.
                if (dropdownToDisable != null)
                {
                    dropdownToDisable.Hide();
                }

                hitPoint = raycastLaser.hitObj.transform.InverseTransformPoint(raycastLaser.hitPos);

                // Get slider information.
                slidingSlider = raycastLaser.hitObj.transform.GetComponentInParent<Slider>();

                // Get selectables.
                Selectable objToSelect = raycastLaser.hitObj.GetComponent<Selectable>();
                if (objToSelect)
                {
                    if (objToSelect.interactable == false)
                    {
                        // Don't interact.
                    }
                    else if (objToSelect is Button)
                    {
                        ((Button) objToSelect).onClick.Invoke();
                    }
                    else if (objToSelect is Toggle)
                    {
                        bool currState = ((Toggle) objToSelect).isOn;
                        ((Toggle) objToSelect).isOn = !currState;
                    }
                    else if (objToSelect is InputField)
                    {
                        BaseEventData eventData = new BaseEventData(EventSystem.current);
                        ((InputField) objToSelect).OnSelect(eventData);
                    }
                    else if (objToSelect is Dropdown)
                    {
                        if ((Dropdown) objToSelect != dropdownToDisable)
                        {
                            PointerEventData eventData = new PointerEventData(EventSystem.current);
                            ((Dropdown) objToSelect).OnPointerClick(eventData);
                            dropdownToDisable = (Dropdown) objToSelect;
                        }
                        else
                        {
                            dropdownToDisable = null;
                        }
                    }
                }
            }

            // Disable previous dropdown if nothing selected.
            else if (dropdownToDisable != null)
            {
                dropdownToDisable.Hide();
                dropdownToDisable = null;
            }
        }

        /// <summary>
        /// Completes the UI pointer operation.
        /// </summary>
        public void FinishSelecting()
        {
            isScrolling = false;
            slidingSlider = null;
        }

        /// <summary>
        /// Enters UI pointer mode.
        /// </summary>
        public void EnterMode()
        {
            if (raycastLaser != null)
            {
                //Debug.LogWarning("Attempting duplicate interactor creation.");
                return;
            }

            raycastLaser = gameObject.AddComponent<RaycastLaser>();
            raycastLaser.layerMask = laserMask;
            raycastLaser.pointerType = RaycastLaser.PointerType.straight;
            raycastLaser.width = 0.005f;
            raycastLaser.maxLength = hand.maxUIPointerDistance;
            raycastLaser.validMaterial = hand.uiLaserMaterial;
            raycastLaser.invalidMaterial = hand.invalidLaserMaterial;
            raycastLaser.cursorPrefab = hand.uiCursor;
            raycastLaser.cursorScale = hand.uiCursorScale;
            raycastLaser.showInvalid = _showInvalid;
            raycastLaser.onHoverBegin = onPoint;
            raycastLaser.onHoverEnd = onStopPointing;

            raycastLaser.active = false;
        }

        /// <summary>
        /// Exits UI pointer mode.
        /// </summary>
        public void ExitMode()
        {
            Destroy(raycastLaser);
            raycastLaser = null;
        }

        private void Update()
        {
            if (raycastLaser == null)
            {
                return;
            }

            if (raycastLaser.isHitting)
            {
                if (isScrolling)
                {
                    // Get ScrollRect.
                    ScrollRect scrollRect = raycastLaser.hitObj.GetComponentInParent<ScrollRect>();
                    if (scrollRect == null)
                    {
                        isScrolling = false;
                    }
                    else
                    {
                        Vector2 currentHit = raycastLaser.hitObj.transform.InverseTransformPoint(raycastLaser.hitPos);
                        // Save the current position relative to the scroll rect.
                        if (hitPoint != null)
                        {
                            scrollRect.velocity += new Vector2(currentHit.x - hitPoint.x, currentHit.y - hitPoint.y);
                        }
                    }
                }

                if (slidingSlider != null)
                {
                    if (slidingSlider == null)
                    {
                        
                    }
                    else
                    {
                        RectTransform sRect = slidingSlider.GetComponent<RectTransform>();
                        if (sRect != null)
                        {
                            float scrollPos = sRect.InverseTransformPoint(raycastLaser.hitPos).x;
                            float sliderPercent = (scrollPos - sRect.rect.xMin) / (sRect.rect.xMax - sRect.rect.xMin);
                            // Wow, I actually remembered some algebra from high school. That's how I figured this out.
                            // Basically PERCENT = (X - MIN) / (MAX - MIN). Solve for X.
                            slidingSlider.value = (slidingSlider.maxValue - slidingSlider.minValue)
                                * sliderPercent + slidingSlider.minValue;
                        }
                    }
                }
            }
        }
    }
}