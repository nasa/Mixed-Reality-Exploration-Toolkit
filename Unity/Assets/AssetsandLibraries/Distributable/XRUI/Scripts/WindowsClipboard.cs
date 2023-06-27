// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;
using GOV.NASA.GSFC.XR.XRUI.Keyboard;

namespace GOV.NASA.GSFC.XR.XRUI.Clipboard
{
    /*
     * This is a static class that implements the copying cutting and pasting functionality.
     * 
     */
    public class WindowsClipboard : MonoBehaviour
    {

        private static string clipboardText;
        private static InputField bodyField;
        private static string text;
        private static Texture2D convertedImg;
        private static Texture2D tex;
        private static KeyboardManager keyboardScript;

        //method to set the correct inst of the KeyboardManager script
        public static void setKeyboardScript(KeyboardManager script)
        {
            keyboardScript = script;
        }

        //method to set the text field
        public static void setField(InputField field)
        {
            bodyField = field;
        }

        //method for cutting
        public static void cut()
        {
            copy();

            int len = Math.Abs(bodyField.selectionFocusPosition - bodyField.selectionAnchorPosition);

            if (bodyField.selectionAnchorPosition < bodyField.selectionFocusPosition)
            {
                // TODO
                //keyboardScript.DeleteCharAtPosition(len);
            }
            else
            {
                // TODO
                //keyboardScript.DeleteCharAtPosition(len);
            }

        }

        //method for copying 
        public static void copy()
        {
            text = bodyField.text;

            int len = Math.Abs(bodyField.selectionFocusPosition - bodyField.selectionAnchorPosition);

            if (bodyField.selectionAnchorPosition > bodyField.selectionFocusPosition)
            {
                text = text.Substring(bodyField.selectionFocusPosition, len);
            }
            else
            {
                text = text.Substring(bodyField.selectionAnchorPosition, len);
            }

            setClipboard(text);

        }

        /*
         * Setting the text to the window's clipboard differs depending on if you are using the editor
         * or if you are not using the editor. Therefore these methods set the text to the window's clipboard
         * and uses the method call depending on if the editor is being used or not
         */

#if UNITY_EDITOR
        public static void setClipboard(string _text)
        {
            EditorGUIUtility.systemCopyBuffer = _text;
        }


        public static void pasteText()
        {
            int index = bodyField.caretPosition;

            text = EditorGUIUtility.systemCopyBuffer.ToString();

            if (index == 0)
            {
                string pre = text;
                string post = bodyField.text;
                string outputString = pre + post;
                bodyField.SetTextWithoutNotify(outputString);
            }
            else if (index == bodyField.text.Length)
            {
                string pre = bodyField.text;
                string post = text;
                string outputString = pre + post;
                bodyField.SetTextWithoutNotify(outputString);
            }
            else
            {
                string pre = bodyField.text.Substring(0, index);
                string mid = text;
                string post = bodyField.text.Substring(index, bodyField.text.Length - pre.Length);
                string outputString = pre + mid + post;
                bodyField.SetTextWithoutNotify(outputString);
            }

        }
#else
    public static void setClipboard(string _text)
    {
        //EditorGUIUtility.systemCopyBuffer = _text;
        GUIUtility.systemCopyBuffer = _text;
        GUIUtility.systemCopyBuffer = _text;
    }


    public static void pasteText()
    {
        int index = bodyField.caretPosition;

        //text = EditorGUIUtility.systemCopyBuffer.ToString();
        text = GUIUtility.systemCopyBuffer.ToString();
        text = GUIUtility.systemCopyBuffer.ToString();

        if (index == 0)
        {
            string pre = text;
            string post = bodyField.text;
            string outputString = pre + post;
            bodyField.SetTextWithoutNotify(outputString);
        }
        else if (index == bodyField.text.Length)
        {
            string pre = bodyField.text;
            string post = text;
            string outputString = pre + post;
            bodyField.SetTextWithoutNotify(outputString);
        }
        else
        {
            string pre = bodyField.text.Substring(0, index);
            string mid = text;
            string post = bodyField.text.Substring(index, bodyField.text.Length - pre.Length);
            string outputString = pre + mid + post;
            bodyField.SetTextWithoutNotify(outputString);
        }

    }
#endif
    }
}