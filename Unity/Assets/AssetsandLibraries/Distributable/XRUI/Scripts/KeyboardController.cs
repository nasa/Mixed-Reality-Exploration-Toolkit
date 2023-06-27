// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace GOV.NASA.GSFC.XR.XRUI.Keyboard
{
    /// <remarks>
    /// History:
    /// 11 February 2021: Refactored
    /// </remarks>
    /// <summary>
    /// This script manages a virtual keyboards.
    /// Author: Dylan Z. Baker
    /// </summary>
    public class KeyboardController : MonoBehaviour
    {
        /// <summary>
        /// The text object to control.
        /// </summary>
        [Tooltip("The text object to control.")]
        public InputField textToControl;

        /// <summary>
        /// The main keyboard.
        /// </summary>
        [Tooltip("The main keyboard.")]
        public GameObject mainKeyboard;

        /// <summary>
        /// The shift keyboard.
        /// </summary>
        [Tooltip("The shift keyboard.")]
        public GameObject shiftKeyboard;

        /// <summary>
        /// Currently entered text.
        /// </summary>
        [Tooltip("Currently entered text.")]
        public string enteredText = "";

        /// <summary>
        /// Whether or not to allow a newline.
        /// </summary>
        [Tooltip("Whether or not to allow a newline.")]
        public bool allowNewLine = true;

        /// <summary>
        /// Whether or not to allow a tab.
        /// </summary>
        [Tooltip("Whether or not to allow a tab.")]
        public bool allowTab = true;

        /// <summary>
        /// The alphabet keys.
        /// </summary>
        [Tooltip("The alphabet keys.")]
        public List<Button> alphaKeys = new List<Button>();

        /// <summary>
        /// Whether or not this is a custom keyboard instance.
        /// </summary>
        [Tooltip("Whether or not this is a custom keyboard instance.")]
        public bool isCustomInstance = false;

        /// <summary>
        /// Whether or not caps is on.
        /// </summary>
        private bool capsOn = false;

        void Start()
        {
            CapsOff();
        }

        /// <summary>
        /// Closes the keyboard.
        /// </summary>
        public void Close()
        {
            if (isCustomInstance)
            {
                Destroy(this);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

#region String Management
        public void Enter()
        {
            if (allowNewLine)
            {
                AddNewLine();
            }
            else
            {
                Clear();
            }
        }

        public void AddString(string stringToAdd)
        {
            if (capsOn)
            {
                enteredText = enteredText + stringToAdd.ToUpper();
            }
            else
            {
                enteredText = enteredText + stringToAdd.ToLower();
            }
            UpdateControlledText();
        }

        public void AddTab()
        {
            if (allowTab)
            {
                enteredText = enteredText + "\t";
            }
            UpdateControlledText();
        }

        public void AddNewLine()
        {
            if (allowNewLine)
            {
                enteredText = enteredText + "\n";
            }
            UpdateControlledText();
        }

        public void DeleteCharAtPosition(int position)
        {
            if (position > -1)
            {
                enteredText.Remove(position, 1);
            }
            UpdateControlledText();
        }

        public void DeleteLastChar()
        {
            if (enteredText.Length > 0)
            {
                enteredText = enteredText.Remove(enteredText.Length - 1);
            }
            UpdateControlledText();
        }

        public void Clear()
        {
            enteredText = "";
            UpdateControlledText();
        }

        private void UpdateControlledText()
        {
            textToControl.text = enteredText;
        }
#endregion

#region Keyboard Management
        public void ToggleKeyboard()
        {
            if (mainKeyboard.activeSelf)
            {
                SwitchToShiftKeyboard();
            }
            else
            {
                SwitchToMainKeyboard();
            }
        }

        public void SwitchToMainKeyboard()
        {
            mainKeyboard.SetActive(true);
            shiftKeyboard.SetActive(false);
            CapsOff();
        }

        public void SwitchToShiftKeyboard()
        {
            shiftKeyboard.SetActive(true);
            mainKeyboard.SetActive(false);
            CapsOn();
        }

        public void ToggleCaps()
        {
            if (capsOn)
            {
                CapsOff();
            }
            else
            {
                CapsOn();
            }
        }

        public void KeyUp()
        {

        }

        public void KeyDown()
        {

        }

        public void KeyLeft()
        {

        }

        public void KeyRight()
        {

        }

        private void CapsOn()
        {
            capsOn = true;
            foreach (Button key in alphaKeys)
            {
                Text keyText = key.GetComponentInChildren<Text>();
                if (keyText)
                {
                    keyText.text = keyText.text.ToUpper();
                }
            }
        }

        private void CapsOff()
        {
            capsOn = false;
            foreach (Button key in alphaKeys)
            {
                Text keyText = key.GetComponentInChildren<Text>();
                if (keyText)
                {
                    keyText.text = keyText.text.ToLower();
                }
            }
        }
#endregion
    }
}