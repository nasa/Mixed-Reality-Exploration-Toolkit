// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace GOV.NASA.GSFC.XR.XRUI.Keyboard
{
    /// <remarks>
    /// History:
    /// 11 February 2021: Refactored
    /// </remarks>
    /// <summary>
    /// This a virtual reality input field that works with virtual keyboards.
    /// Author: Dylan Z. Baker
    /// </summary>
    public class VR_InputField : InputField
    {
        /// <summary>
        /// The positional offset for keyboards.
        /// </summary>
        [Tooltip("The positional offset for keyboards.")]
        public Vector3 positionalOffset = Vector3.zero;

        /// <summary>
        /// The rotational offset for keyboards.
        /// </summary>
        [Tooltip("The rotational offset for keyboards.")]
        public Vector3 rotationalOffset = Vector3.zero;

        /// <summary>
        /// Whether or not a personal keyboard will be created for this input field.
        /// </summary>
        [Tooltip("Whether or not a personal keyboard will be created for this input field.")]
        public bool personalKeyboard = false;

        /// <summary>
        /// Event handler for pointer click events.
        /// </summary>
        /// <param name="eventData">Event data.</param>
        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);

            ShowKeyboard();
        }

        /// <summary>
        /// Event handler for pointer select events.
        /// </summary>
        /// <param name="eventData">Event data.</param>
        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);

            ShowKeyboard();
        }

        /// <summary>
        /// Shows the keyboard.
        /// </summary>
        private void ShowKeyboard()
        {
            KeyboardManager.KeyboardType type = KeyboardManager.KeyboardType.Full;
            if (contentType == ContentType.DecimalNumber || contentType == ContentType.IntegerNumber)
            {
                type = KeyboardManager.KeyboardType.Numeric;
            }

            MRET.MRET.KeyboardManager.GetKeyboard(this, transform.position + positionalOffset,
                Quaternion.Euler(transform.rotation.eulerAngles + rotationalOffset), personalKeyboard,
                type);
        }
    }
}