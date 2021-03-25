// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GSFC.ARVR.MRET.Components.UI
{
    /// <remarks>
    /// History:
    /// 8 February 2021: Created
    /// </remarks>
    /// <summary>
    /// This script is attached to all UI elements in MRET to enable interaction.
    /// It uses colliders for raycast/physical pressing.
    /// Author: Dylan Z. Baker
    /// </summary>
    public class MRETUIElement : MonoBehaviour
    {
        /// <summary>
        /// Layer to place element on. Ignored if not an interactable element.
        /// </summary>
        [Tooltip("Layer to place element on. Ignored if not an interactable element.")]
        public uint layer = 1;

        /// <summary>
        /// The UI element in question.
        /// </summary>
        private UIBehaviour uiElement;

        /// <summary>
        /// The selectable element.
        /// </summary>
        private Selectable selectableElement;

        /// <summary>
        /// The image element.
        /// </summary>
        private Image imageElement;

        /// <summary>
        /// The object's raycast collider.
        /// </summary>
        private BoxCollider raycastCollider;

        /// <summary>
        /// Pointer Event Data to use.
        /// </summary>
        PointerEventData pointerEventData;

        public void StartedHitting()
        {
            if (selectableElement)
            {
                selectableElement.OnPointerEnter(pointerEventData);
            }
        }

        public void StoppedHitting()
        {
            if (selectableElement)
            {
                selectableElement.OnPointerExit(pointerEventData);
            }
        }

        private void Start()
        {
            pointerEventData = new PointerEventData(EventSystem.current);

            SetUpColliders();
        }

        private void OnBecameVisible()
        {
            if (raycastCollider)
            {
                raycastCollider.enabled = true;
            }
        }

        private void OnBecameInvisible()
        {
            if (raycastCollider)
            {
                raycastCollider.enabled = false;
            }
        }

        /// <summary>
        /// Set up the colliders for the object.
        /// </summary>
        private void SetUpColliders()
        {
            // Get appropriate UI element.
            uiElement = gameObject.GetComponent<UIBehaviour>();
            if (uiElement == null)
            {
                Debug.LogError("[MRETUIElement] No UI element found on " + GetHierarchy() + ".");
                return;
            }

            // Get appopriate image element.
            imageElement = GetComponent<Image>();

            // Set up collider.
            raycastCollider = gameObject.AddComponent<BoxCollider>();
            raycastCollider.isTrigger = true;
            RectTransform rectTransform = GetComponent<RectTransform>();
            if (rectTransform)
            {
                raycastCollider.size = new Vector3(rectTransform.rect.width, rectTransform.rect.height, 1);
            }

            // If selectable, place in foreground. Otherwise, background.
            selectableElement = gameObject.GetComponent<Selectable>();
            if (selectableElement)
            {
                // Foreground element.
                raycastCollider.center = new Vector3(0, 0, -1 * layer);
            }
            else
            {
                // Background element.
                raycastCollider.center = new Vector3(0, 0, 1);
            }
        }

        // TODO: MRETBehaviour?
        /// <summary>
        /// Print a hierarchy string.
        /// </summary>
        /// <returns>A string listing the hierarchy of the object.</returns>
        private string GetHierarchy()
        {
            string hierarchy = gameObject.name;
            GameObject currentObject = gameObject;

            while (currentObject.transform.parent != null)
            {
                hierarchy = currentObject.name + "/" + gameObject.name;
                currentObject = currentObject.transform.parent.gameObject;
            }

            return hierarchy;
        }
    }
}