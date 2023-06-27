// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using GOV.NASA.GSFC.XR.MRET.Data.Telemetry;

namespace GOV.NASA.GSFC.XR.MRET.UI.Data.Telemetry
{
    /// <summary>
    /// TelemetryUIToggle
    /// 
    /// Extends TelemetryBoolean to control the state of a Toggle component. If a Toggle component is
    /// not specified, the parent will be queried for a Toggle component reference to use.</br>
    /// 
    /// Author: Jeffrey Hosler
    /// </summary>
    public class TelemetryUIToggle : TelemetryBoolean
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(TelemetryUIToggle);

        [Tooltip("The optional Toggle component to control with the boolean result of the " +
            "telemetry value. If not specified, the parent will be queried for a Toggle component.")]
        public Toggle toggleComponent;

        [Tooltip("Indicates whether a logical negation should be applied to the value when " +
            "setting the toggle.")]
        public bool logicalNegation = false;

        /// <seealso cref="Telemetry.SetValue(object)"/>
        protected override void SetValue(object newValue)
        {
            // Make sure to take the inherited behavior
            base.SetValue(newValue);

            // Check to see if we can update the toggle
            if (toggleComponent != null)
            {
                // Control the Toggle state
                toggleComponent.isOn = logicalNegation ? !BooleanValue : BooleanValue;
            }
        }

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            base.MRETStart();

            // Get a component reference to the parent Toggle if not explicitly set
            if (toggleComponent == null)
            {
                toggleComponent = GetComponentInParent<Toggle>();
            }
        }

    }
}