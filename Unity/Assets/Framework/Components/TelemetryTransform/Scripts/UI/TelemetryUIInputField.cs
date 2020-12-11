using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GSFC.ARVR.MRET.Telemetry.UI
{
    /**
     * TelemetryUIInputField
     * 
     * Extends TelemetryText to place the telemetry text value into the text field of an InputField
     * component. If an InputField component is not specified, the parent will be queried for an
     * InputField component reference to use.<br>
     * 
     * @see InputField
     * @see TelemetryText
     * 
     * @author Jeffrey Hosler
     */
    public class TelemetryUIInputField : TelemetryText
    {
        public new static readonly string NAME = nameof(TelemetryUIInputField);

        [Tooltip("The optional InputField component to place the telemetry string representation. If not specified, the parent will be queried for a InputField component.")]
        public InputField inputFieldComponent;

        /**
         * @inherited
         */
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

        /**
         * @inherited
         */
        protected override void Start()
        {
            // Take the inherited behavior
            base.Start();

            // Get a component reference to the parent InputField if not explicitly set
            if (inputFieldComponent == null)
            {
                inputFieldComponent = GetComponentInParent<InputField>();
            }
        }

    }
}
