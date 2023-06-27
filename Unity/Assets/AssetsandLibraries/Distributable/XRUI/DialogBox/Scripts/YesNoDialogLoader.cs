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
    /// YesNoDialogLoader is a class that manages
    /// a dialog box with two assignable buttons.
    /// Author: Jordan A. Ritchey
    /// </summary>
    public class YesNoDialogLoader: WorldSpaceMenuLoader
    {
        [Tooltip("Text to display in dialog box.")]
        public string dialogBoxText;

        [Tooltip("Text to display on Yes button.")]
        public string dialogYesText = "Yes";
        [Tooltip("Event for the Yes button.")]
        public UnityEvent dialogYesEvent;

        [Tooltip("Text to display on No button.")]
        public string dialogNoText = "No";
        [Tooltip("Event for the No button.")]
        public UnityEvent dialogNoEvent;

        [Tooltip("UnityAction to execute when Yes button is pressed.")]
        private UnityAction yesPressAction;

        [Tooltip("UnityAction to execute when No button is pressed.")]
        private UnityAction noPressAction;

        /// <summary>
        /// Set up variables for dialog box.
        /// </summary>
        private void Start()
        {
            // set up the UnityActions for button presses
            yesPressAction += dialogYesEvent.Invoke;
            yesPressAction += DestroyMenu;
            noPressAction += dialogNoEvent.Invoke;
            noPressAction += DestroyMenu;
        }

        /// <seealso cref="WorldSpaceMenuLoader.ConfigureDisplay(GameObject)"/>
        protected override void ConfigureDisplay(GameObject dialogGO)
        {
            base.ConfigureDisplay(dialogGO);

            // set the DialogText field of currentDialog to dialogBoxText
            Text textbox = dialogGO.transform.Find("Canvas").transform.Find("Content").transform.Find("DialogText").GetComponent(typeof(Text)) as Text;
            textbox.text = dialogBoxText;

            // set the YesButton text field of currentDialog to dialogYesText
            textbox = dialogGO.transform.Find("Canvas").transform.Find("Content").transform.Find("YesButton").transform.Find("Text").GetComponent(typeof(Text)) as Text;
            textbox.text = dialogYesText;

            // set the NoButton text field of currentDialog to dialogNoText
            textbox = dialogGO.transform.Find("Canvas").transform.Find("Content").transform.Find("NoButton").transform.Find("Text").GetComponent(typeof(Text)) as Text;
            textbox.text = dialogNoText;

            // set the event that occurs when the Yes button is pressed to dialogYesEvent
            Button yesButton = dialogGO.transform.Find("Canvas").transform.Find("Content").transform.Find("YesButton").GetComponent(typeof(Button)) as Button;
            yesButton.onClick.AddListener(yesPressAction);

            // set the event that occurs when the No button is pressed to dialogNoEvent
            Button noButton = dialogGO.transform.Find("Canvas").transform.Find("Content").transform.Find("NoButton").GetComponent(typeof(Button)) as Button;
            noButton.onClick.AddListener(noPressAction);
        }

    }
}