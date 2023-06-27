// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityEngine.UI;
using GOV.NASA.GSFC.XR.MRET.Interfaces;

namespace GOV.NASA.GSFC.XR.MRET.UI.Interfaces
{
    /// <remarks>
    /// History:
    /// 26 January 2023: Created
    /// </remarks>
    ///
    /// <summary>
    /// ThirdPartyInterfaceErrorDialogController
    ///
    /// The controller script for 3rd party interface error dialogs
    ///
    /// Author: Jeffrey Hosler
    /// </summary>
    /// 
    public abstract class ThirdPartyInterfaceErrorDialogController<T> : MRETBehaviour
        where T : IThirdPartyInterface
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(ThirdPartyInterfaceErrorDialogController<T>);

        public List<Button> buttons;
        public Text textComponent;
        public T thirdPartyInterface;

        public void SetInteractable(bool interactable)
        {
            foreach (Button button in buttons)
            {
                button.interactable = interactable;
            }
        }

        public virtual void DestroyPrompt()
        {
            ThirdPartyInterfaceManager.Instance.RemoveTopinterfaceErrorDialog();
        }

        /// <summary>
        /// Obtains the error message text from the supplied data manager data point
        /// </summary>
        /// <returns></returns>
        protected virtual string GetErrorMessageText()
        {
            string result = "Unavailable";

            // Attempt to get the error from the data manager
            object dataPoint = MRET.DataManager.FindPoint(ThirdPartyInterfaceManager.INTERFACE_ERROR_KEY);
            if ((dataPoint is List<object>) && ((dataPoint as List<object>).Count > 0))
            {
                object dataPointItem = (dataPoint as List<object>)[0];
                if (dataPointItem is ValueTuple<string, string>)
                {
                    // First value is the server, second is reason
                    (string, string) dataValues = (ValueTuple<string, string>)dataPointItem;

                    result = thirdPartyInterface.name + "\n";
                    result += "Connection to ";
                    result += dataValues.Item1 + "\n";
                    result += "failed due to:\n";
                    result += dataValues.Item2;
                }
                else if (dataPointItem is string)
                {
                    string reason = dataPointItem as string;

                    result = thirdPartyInterface.name + "\n";
                    result += "failed due to:\n";
                    result += reason;
                }
            }

            return result;
        }

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            // Take the inherited behavior
            base.MRETStart();

            // Custom initialization (after deserialization)

            // Check to see if we can update the text
            if (textComponent != null)
            {
                textComponent.text = GetErrorMessageText();
            }
        }
    }

    public class ThirdPartyInterfaceErrorDialogController : ThirdPartyInterfaceErrorDialogController<IThirdPartyInterface>
    {
    }

}
