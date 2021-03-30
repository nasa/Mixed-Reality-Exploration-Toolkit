// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GSFC.ARVR.MRET.Components.UI;
using GSFC.ARVR.XRUI.WorldSpaceMenu;

namespace GSFC.ARVR.MRET.Components.Keyboard
{
    /// <remarks>
    /// History:
    /// 11 February 2021: Created
    /// </remarks>
    /// <summary>
    /// This script manages virtual keyboards in MRET.
    /// Author: Dylan Z. Baker
    /// </summary>
    public class KeyboardManager : MonoBehaviour
    {
        /// <summary>
        /// Type of keyboard.
        /// </summary>
        public enum KeyboardType { Full, Numeric }

        /// <summary>
        /// The full keyboard GameObject.
        /// </summary>
        [Tooltip("The full keyboard GameObject.")]
        public KeyboardController fullKeyboard;

        /// <summary>
        /// The numeric keyboard GameObject.
        /// </summary>
        [Tooltip("The numeric keyboard GameObject.")]
        public KeyboardController numericKeyboard;

        /// <summary>
        /// The instance of the keyboard manager.
        /// </summary>
        private static KeyboardManager instance;

        /// <summary>
        /// Returns the global keyboard, or a custom instance.
        /// </summary>
        /// <param name="inputField">Input field to attach to keyboard.</param>
        /// <param name="position">Position to place the keyboard.</param>
        /// <param name="rotation">Rotation to apply to the keyboard.</param>
        /// <param name="customInstance">If true, an instance will be returned.</param>
        /// <param name="type">Type of keyboard.</param>
        /// <returns>A reference to the instance of the requested keyboard.</returns>
        public KeyboardController GetKeyboard(VR_InputField inputField, Vector3 position, Quaternion rotation,
            bool customInstance = false, KeyboardType type = KeyboardType.Full)
        {
            fullKeyboard.gameObject.SetActive(false);
            fullKeyboard.gameObject.SetActive(false);
            fullKeyboard.textToControl = null;
            fullKeyboard.textToControl = null;

            KeyboardController keyboard = fullKeyboard;
            if (type == KeyboardType.Full)
            {
                if (customInstance)
                {
                    keyboard = Instantiate(fullKeyboard);
                }
                else
                {
                    keyboard = fullKeyboard;
                }
            }
            else if (type == KeyboardType.Numeric)
            {
                if (customInstance)
                {
                    keyboard = Instantiate(numericKeyboard);
                }
                else
                {
                    keyboard = numericKeyboard;
                }
            }
            
            keyboard.gameObject.SetActive(true);
            keyboard.transform.position = position;
            keyboard.transform.rotation = rotation;
            keyboard.textToControl = inputField;

            MoveKeyboardOut(keyboard.gameObject, inputField.gameObject);

            // Reset keyboard text.
            keyboard.enteredText = inputField.text;

            return keyboard;
        }

        /// <summary>
        /// Initializes the keyboard manager.
        /// </summary>
        public void Initialize()
        {
            instance = this;
        }

        private const int maxIterations = 1000;
        private void MoveKeyboardOut(GameObject keyboard, GameObject textObject)
        {
            // Get collider corresponding to keyboard.
            Collider keyboardCollider = keyboard.GetComponent<Collider>();
            if (keyboardCollider == null)
            {
                return;
            }

            // Get collider corresponding to menu.
            WorldSpaceMenuManager textMenu = textObject.GetComponentInParent<WorldSpaceMenuManager>();
            if (textMenu == null)
            {
                return;
            }

            Collider textMenuCollider = textMenu.GetComponent<Collider>();
            if (textMenuCollider == null)
            {
                return;
            }

            Transform originalParent = keyboard.transform.parent;
            keyboard.transform.SetParent(textMenu.transform);
            keyboard.transform.localPosition = Vector3.zero;
            keyboard.transform.localRotation = Quaternion.identity;

            int iterations = 0;
            float amtToIncrement = -0.1f;
            while (iterations < maxIterations)
            {
                if (keyboardCollider.bounds.Intersects(textMenuCollider.bounds))
                {
                    // TODO: I know this is hacky, I'm being lazy.
                    keyboard.transform.localPosition += new Vector3(0, amtToIncrement, 0);
                    amtToIncrement *= 1.25f;
                }
                else
                {
                    break;
                }

                iterations++;
            }

            keyboard.transform.SetParent(originalParent);
        }
    }
}