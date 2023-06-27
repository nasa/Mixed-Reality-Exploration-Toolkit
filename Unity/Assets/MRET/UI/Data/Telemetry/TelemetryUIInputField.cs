// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using GOV.NASA.GSFC.XR.MRET.Data.Telemetry;

namespace GOV.NASA.GSFC.XR.MRET.UI.Data.Telemetry
{
    /// <summary>
    /// TelemetryUIInputField
    /// 
    /// Extends TelemetryText to place the telemetry text value into the text field of an InputField
    /// component. If an InputField component is not specified, the parent will be queried for an
    /// InputField component reference to use.<br>
    /// 
    /// Author: Jeffrey Hosler
    /// </summary>
    public class TelemetryUIInputField : TelemetryText
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(TelemetryUIInputField);

        [Tooltip("The optional InputField component to place the telemetry string representation." +
            " If not specified, the parent will be queried for a InputField component.")]
        public InputField inputFieldComponent;

        /// <seealso cref="Telemetry.SetValue(object)"/>
        protected override void SetValue(object newValue)
        {
            // Make sure to take the inherited behavior
            base.SetValue(newValue);

            // Check to see if we can update the text. Make sure the user isn't editing the
            // field, so ignore if focused
            if ((inputFieldComponent != null) && (!inputFieldComponent.isFocused))
            {
                // Assign the Text value to the inputFieldComponent's text field
                inputFieldComponent.text = Text;
            }
        }

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            base.MRETStart();

            // Get a component reference to the parent InputField if not explicitly set
            if (inputFieldComponent == null)
            {
                inputFieldComponent = GetComponentInParent<InputField>();
            }
        }

    }
}
