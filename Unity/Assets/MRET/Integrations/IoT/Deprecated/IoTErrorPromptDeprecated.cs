// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using GOV.NASA.GSFC.XR.MRET.Integrations.IoT;

namespace GOV.NASA.GSFC.XR.MRET.UI.Extensions.IoT
{
    [System.Obsolete("Refer to the " + nameof(GOV.NASA.GSFC.XR.MRET.UI.Interfaces.ClientInterfaceErrorDialogController) + " class")]
    public class IoTErrorPromptDeprecated : MRETBehaviour
    {
        public enum ErrorSourceType
        {
            Client,
            Server
        }

        [SerializeField]
        [Tooltip("The source of the error. This will control the type of cert that is requested from the user.")]
        public ErrorSourceType source = ErrorSourceType.Client;

        /// <summary>
        /// The source of the error. This will control the type of cert that is requested from the user.
        /// </summary>
        public ErrorSourceType Source
        {
            get => source;
            set
            {
                source = value;
                switch (source)
                {
                    case ErrorSourceType.Server:
                        EnableServerSource();
                        break;
                    case ErrorSourceType.Client:
                    default:
                        EnableClientSource();
                        break;
                }
            }
        }

        public List<Button> buttons;
        public Button clientSourceButton;
        public Button serverSourceButton;
        public Text textComponent;
        public IoTClientDeprecated client;

        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName
        {
            get
            {
                return nameof(IoTErrorPromptDeprecated);
            }
        }

        public void Connect()
        {
            if (client != null)
            {
                client.Connect();
            }
        }

        public void SetInteractable(bool interactable)
        {
            foreach (Button button in buttons)
            {
                button.interactable = interactable;
            }
        }

        public void DestroyPrompt()
        {
            IoTManagerDeprecated.instance.RemoveTopErrorPrompt();
        }

        protected void EnableClientSource()
        {
            clientSourceButton.gameObject.SetActive(true);
            serverSourceButton.gameObject.SetActive(false);
        }

        protected void EnableServerSource()
        {
            clientSourceButton.gameObject.SetActive(false);
            serverSourceButton.gameObject.SetActive(true);
        }

        //When instantiated, change the text of the error prompt
        /// <seealso cref="MRETBehaviour.MRETAwake"/>
        protected override void MRETAwake()
        {
            // Take the inherited behavior
            base.MRETAwake();

            // Custom initialization (before deserialization)
            // Check to see if we can update the text
            if (textComponent != null)
            {
                List<(string, string)> errorMessages = (List<(string, string)>)MRET.DataManager.FindPoint("GOV.NASA.GSFC.XR.MRET.IOT.ERROR");
                (string, string) errorMessage = errorMessages[0];
                // Assign the Text value to the textComponent's text field
                textComponent.text = "MQTT Connection to ";
                textComponent.text += errorMessage.Item1 + "\n";
                textComponent.text += "failed due to:\n";
                textComponent.text += errorMessage.Item2;
            }
        }
    }
}