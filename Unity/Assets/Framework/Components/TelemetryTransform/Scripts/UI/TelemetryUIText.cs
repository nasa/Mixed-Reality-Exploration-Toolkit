// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;

namespace GSFC.ARVR.MRET.Telemetry.UI
{
    /**
     * TelemetryUIText
     * 
     * Extends TelemetryText to place the telemetry text value into the text field of the Text
     * component. If a Text component is not specified, the parent will be queried for a Text
     * component reference to use.</br>
     * 
     * @see Text
     * @see TelemetryText
     * 
     * @author Jeffrey Hosler
     */
    public class TelemetryUIText : TelemetryText
    {
        public new static readonly string NAME = nameof(TelemetryUIText);

        [Tooltip("The optional Text component to place the telemetry string representation. If not specified, the parent will be queried for a Text component.")]
        public Text textComponent;

        /**
         * @inherited
         */
        protected override void SetValue(object newValue)
        {
            // Make sure to take the inherited behavior
            base.SetValue(newValue);

            // Check to see if we can update the text
            if (textComponent != null)
            {
                // Assign the Text value to the textComponent's text field
                textComponent.text = Text;
            }
        }

        /**
         * @inherited
         */
        private void Start()
        {
            // Get a component reference to the parent Text if not explicitly set
            if (textComponent == null)
            {
                textComponent = GetComponentInParent<Text>();
            }
        }

    }

}