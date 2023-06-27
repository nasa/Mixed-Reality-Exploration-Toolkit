// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;
#if HOLOLENS_BUILD
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem.SDK.Hololens;
#endif
using GOV.NASA.GSFC.XR.MRET.Integrations.CPIS;
using TMPro;

namespace GOV.NASA.GSFC.XR.MRET.UI.Interactions
{
    /// <remarks>
    /// History:
    /// 22 January 2021: Created
    /// 14 September 2021: Fixing minor bug where a null hitObj could be accessed.
    /// </remarks>
    /// <summary>
    /// UIPointerController is a controller class that
    /// handles UI pointing for one hand. It should be included
    /// with the InputHand.
    /// Author: Dylan Z. Baker
    /// </summary>
    public class UIPointerController : MRETUpdateBehaviour, ITouchBehavior
    {
        public override string ClassName => nameof(UIPointerController);

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
                _showInvalid = value;
                if (raycastLaser)
                {
                    raycastLaser.showInvalid = value;
                }
            }
        }

        [SerializeField]
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
        private TMP_Dropdown tmpDropdownToDisable = null;

        private Vector3 hitPoint;

        private bool isScrolling = false;

        private Slider slidingSlider = null;

        private GameObject currentMenu;

        /// <summary>
        /// Toggles the UI pointer on.
        /// </summary>
        /// <param name="soft">Whether or not this is a soft disable. Soft enables can be disabled with soft
        /// or hard disables, while hard enables can only be disabled with hard disables.</param>
        public void ToggleUIPointingOn(bool soft)
        {
            //Tell the finger tracker to handle ui interactions through touch using the onTouch method in UIPointerController
#if HOLOLENS_BUILD
            FingerTracker fingerTracker = hand.GetComponentInChildren<FingerTracker>();
            if (fingerTracker != null && !fingerTracker.hasCollisionListener(this))
            {
                fingerTracker.addCollisionListener(this);
            }
            return;
#endif

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
#if HOLOLENS_BUILD
            //FingerTracker fingerTracker= hand.GetComponentInChildren<FingerTracker>();
            //fingerTracker.removeCollisionListener(this);
            return;
#endif

            if (raycastLaser == null)
            {
                return;
            }

            if (hardEnable == false || soft == false)
            {
                raycastLaser.active = false;
            }

            FinishSelecting();
        }

        public bool OnTouch(Collider other)
        {
            if (hand.GetComponent<HandInteractor>().IsGrabbing() || hand.menuHolding)
            {
                return false;
            }
            
            currentMenu = other.gameObject;
            
            if (currentMenu != null && currentMenu.GetComponent<Selectable>() != null)
            {
                //pointerController.HoverOverUI(currentMenu.gameObject);
                HandSelect(currentMenu.gameObject);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Performs the UI pointer operation.
        /// </summary>
        public void Select()
        {
            isScrolling = true;

            #if !HOLOLENS_BUILD
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
                if (tmpDropdownToDisable != null)
                {
                    tmpDropdownToDisable.Hide();
                }

                hitPoint = raycastLaser.hitObj.transform.InverseTransformPoint(raycastLaser.hitPos);
                InteractWithUI(raycastLaser.hitObj);
                return;
            }
            #else
            if (currentMenu != null)
            {
                if (currentMenu.GetComponent<Selectable>() != null)
                {
                    InteractWithUI(currentMenu);
                }
                return;
            }
            #endif

            // Disable previous dropdown if nothing selected.
            if (dropdownToDisable != null)
            {
                dropdownToDisable.Hide();
                dropdownToDisable = null;
            }
        }

        public void HandSelect(GameObject hitObj)
        {
            currentMenu = hitObj;
            Select();
        }

        /// <summary>
        /// Interacts with the given UI gameobject
        /// This triggers when onSelect for VR and onTriggerExit for AR
        /// </summary>
        /// <param name="hitObj"></param>
        public void InteractWithUI(GameObject hitObj)
        {
            if (hitObj != null)
            {
                // Disable previous dropdown.
                if (dropdownToDisable != null)
                {
                    dropdownToDisable.Hide();
                }

#if HOLOLENS_BUILD
                // When using lasers click then drag sliders, when using touch 
                slidingSlider = currentMenu.transform.GetComponentInParent<Slider>();
#else
                // Get slider information.
                slidingSlider = hitObj.transform.GetComponentInParent<Slider>();
#endif

                // Get selectables.
                Selectable objToSelect = hitObj.GetComponent<Selectable>();
                if (objToSelect != null)
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
                    else if (objToSelect is TMP_Dropdown)
                    {
                        if ((TMP_Dropdown)objToSelect != tmpDropdownToDisable)
                        {
                            PointerEventData eventData = new PointerEventData(EventSystem.current);
                            ((TMP_Dropdown)objToSelect).OnPointerClick(eventData);
                            tmpDropdownToDisable = (TMP_Dropdown)objToSelect;
                        }
                        else
                        {
                            tmpDropdownToDisable = null;
                        }
                    }
                    else
                    {
                        LogWarning("UI object type unrecognized: " + objToSelect, nameof(InteractWithUI));
                    }
                }
            }
            currentMenu = null;
        }

        public void HoverOverUI(GameObject hitObj){

            if (hitObj != null)
            {
#if HOLOLENS_BUILD
                // When using lasers click then drag sliders, when using touch 
                slidingSlider = currentMenu.transform.GetComponentInParent<Slider>();
#else
                // Get slider information.
                slidingSlider = hitObj.transform.GetComponentInParent<Slider>();
#endif

                // Get selectables.
                Selectable objToSelect = hitObj.GetComponent<Selectable>();
                if (objToSelect)
                {
                    if (objToSelect.interactable == false)
                    {
                        // Don't interact.
                    }
                    else if (objToSelect is Button)
                    {
                        ((Button)objToSelect).Select();
                    }
                    else if (objToSelect is Toggle)
                    {
                        ((Toggle)objToSelect).Select();
                    }
                    else if (objToSelect is InputField)
                    {
                        ((InputField)objToSelect).Select();
                    }
                    else if (objToSelect is Dropdown)
                    {
                        ((Dropdown)objToSelect).Select();
                    }
                    else if (objToSelect is TMP_Dropdown)
                    {
                        ((TMP_Dropdown)objToSelect).Select();
                    }
                    else
                    {
                        LogWarning("UI object type unrecognized: " + objToSelect, nameof(HoverOverUI));
                    }
                }
            }
            currentMenu = hitObj;
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

        protected override void MRETUpdate()
        {
            if ((hand.inputRig.mode != InputRig.Mode.AR && raycastLaser == null) ||
                (hand.inputRig.mode == InputRig.Mode.AR && currentMenu == null))
            {
                return;
            }

            if ((hand.inputRig.mode != InputRig.Mode.AR && raycastLaser.isHitting) ||
                (hand.inputRig.mode == InputRig.Mode.AR && currentMenu != null))
            {
                if (isScrolling)
                {
                    GameObject hitObj;
#if !HOLOLENS_BUILD
                    hitObj = raycastLaser.hitObj;
#else
                    hitObj = currentMenu;
#endif
                    if (hitObj != null)
                    {
                        ScrollRect scrollRect = hitObj.GetComponentInParent<ScrollRect>();
                        if (scrollRect == null)
                        {
                            isScrolling = false;
                        }
                        else
                        {
                            Vector2 currentHit = hitObj.transform.InverseTransformPoint(raycastLaser.hitPos);
                            // Save the current position relative to the scroll rect.
                            if (hitPoint != null)
                            {
                                scrollRect.velocity += new Vector2(currentHit.x - hitPoint.x, currentHit.y - hitPoint.y);
                            }
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
#if HOLOLENS_BUILD
                            float scrollPos = sRect.InverseTransformPoint(hand.GetComponentInChildren<FingerTracker>().transform.position).x;
#else
                            float scrollPos = sRect.InverseTransformPoint(raycastLaser.hitPos).x;
#endif
                            Debug.Log(scrollPos);
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