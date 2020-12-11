using UnityEngine;
using UnityEngine.UI;

namespace GSFC.ARVR.MRET.Telemetry.UI
{
    /**
     * TelemetryUIToggle
     * 
     * Extends TelemetryBoolean to control the state of a Toggle component. If a Toggle component is
     * not specified, the parent will be queried for a Toggle component reference to use.</br>
     * 
     * @see Toggle
     * @see TelemetryBoolean
     * 
     * @author Jeffrey Hosler
     */
    public class TelemetryUIToggle : TelemetryBoolean
    {
        public new static readonly string NAME = nameof(TelemetryUIToggle);

        [Tooltip("The optional Toggle component to control with the boolean result of the telemetry value. If not specified, the parent will be queried for a Toggle component.")]
        public Toggle toggleComponent;

        [Tooltip("Indicates whether a logical negation should be applied to the value when setting the toggle.")]
        public bool logicalNegation = false;

        /**
         * @inherited
         */
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

        /**
         * @inherited
         */
        protected override void Start()
        {
            // Take the inherited behavior
            base.Start();

            // Get a component reference to the parent Toggle if not explicitly set
            if (toggleComponent == null)
            {
                toggleComponent = GetComponentInParent<Toggle>();
            }
        }

    }
}