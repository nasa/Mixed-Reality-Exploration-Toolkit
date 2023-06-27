// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;

namespace GOV.NASA.GSFC.XR.XRUI.WorldSpaceMenu
{
    /// <remarks>
    /// History:
    /// 27 June 2022: Created
    /// </remarks>
    /// <summary>
    /// PopupDialogLoader is a class that manages
    /// a dialog box with one assignable button.
    /// Author: Jordan A. Ritchey
    /// </summary>
    public class PopupDialogLoader : WorldSpaceMenuLoader
    {
        [Tooltip("Text to display in dialog box.")]
        public string dialogBoxText;

        [Tooltip("Text to display on OK button.")]
        public string dialogOkText = "OK";
        [Tooltip("Event for the OK button.")]
        public UnityEvent dialogOkEvent;

        [Tooltip("UnityAction to execute when OK button is pressed.")]
        private UnityAction okPressAction;

        /// <summary>
        /// Set up variables for dialog box.
        /// </summary>
        private void Start()
        {
            // set up the UnityAction for button press
            okPressAction += dialogOkEvent.Invoke;
            okPressAction += DestroyMenu;
        }

        /// <seealso cref="WorldSpaceMenuLoader.ConfigureDisplay(GameObject)"/>
        protected override void ConfigureDisplay(GameObject dialogGO)
        {
            base.ConfigureDisplay(dialogGO);

            // set the DialogText field of currentDialog to dialogBoxText
            Text textbox = dialogGO.transform.Find("Canvas").transform.Find("Content").transform.Find("DialogText").GetComponent(typeof(Text)) as Text;
            textbox.text = dialogBoxText;

            // set the OkButton text field of currentDialog to dialogOkText
            textbox = dialogGO.transform.Find("Canvas").transform.Find("Content").transform.Find("OkButton").transform.Find("Text").GetComponent(typeof(Text)) as Text;
            textbox.text = dialogOkText;

            // set the event that occurs when the OK button is pressed to dialogOkEvent
            Button okButton = dialogGO.transform.Find("Canvas").transform.Find("Content").transform.Find("OkButton").GetComponent(typeof(Button)) as Button;
            okButton.onClick.AddListener(okPressAction);
        }

    }
}