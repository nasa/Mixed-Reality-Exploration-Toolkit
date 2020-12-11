using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using GSFC.ARVR.Utilities.Properties;

namespace GSFC.ARVR.MRET.Telemetry.UI
{
    /**
     * TelemetryUIDateTimeSlider
     * 
     * Extends Telemetry to control the value of a Slider component. If a Slider component is
     * not specified, the parent will be queried for a Slider component reference to use.</br>
     * 
     * @see Slider
     * @see Telemetry
     * 
     * @author Jeffrey Hosler
     * 
     * TODO: We might consider extending from TelemetryUINumberSlider if we decide that the TelemetryNumber
     * script will use DateTime.Ticks as the Number for DateTime types. There are limitations with this too
     * so let's just leave as is for now.
     */
    public class TelemetryUIDateTimeSlider : TelemetryDateTime
    {
        public new static readonly string NAME = nameof(TelemetryUIDateTimeSlider);

        [Tooltip("The optional Slider component to control with the telemetry value. If not specified, the parent will be queried for a Slider component.")]
        public Slider sliderComponent;

        // TODO: These should be driven by the project definition, so perhaps pull from the DataManager instead of the public field?
#if UNITY_EDITOR
        [Tooltip("The DateTime that represents the minimum Value of the Slider.")]
        public UDateTime sliderMinValueDateTime = default(DateTime);

        [Tooltip("The DateTime that represents the maximum Value of the Slider.")]
        public UDateTime sliderMaxValueDateTime = default(DateTime);
#else
        public DateTime sliderMinValueDateTime = default(DateTime);
        public DateTime sliderMaxValueDateTime = default(DateTime);
#endif

        // Used to track user interaction
        private bool sliderIsBeingDragged = false;

        /**
         * Performs a mapping of the supplied slider value to a DateTime. This function
         * does not alter the state of the slider, but returns the DateTime mapping to the
         * supplied slider value.
         * 
         * @param sliderValue The slider value to query
         * 
         * @see DateTime
         */
        public DateTime GetDateTime(float sliderValue)
        {
            DateTime fromMinDateTime = sliderMinValueDateTime;
            DateTime fromMaxDateTime = sliderMaxValueDateTime;

            // Get the tick value by remapping the slider range to the tick range
            double ticks = RemapValue1(sliderValue,
                sliderComponent.minValue, sliderComponent.maxValue,     // Slider range
                fromMinDateTime.Ticks, fromMaxDateTime.Ticks);          // DateTime range

            return new DateTime((long)ticks);
        }

        /**
         * Event handler for the Slider BeginDrag event<br>
         * 
         * @param pointerEventData The <code>PointerEventData</code> containing the event data
         * 
         * @see PointerEventData
         */
        protected void OnBeginDrag(PointerEventData pointerEventData)
        {
            sliderIsBeingDragged = true;
        }

        /**
         * Event handler for the Slider EndDrag event<br>
         * 
         * @param pointerEventData The <code>PointerEventData</code> containing the event data
         * 
         * @see PointerEventData
         */
        protected void OnEndDrag(PointerEventData pointerEventData)
        {
            sliderIsBeingDragged = false;
        }

        /**
         * @inherited
         */
        protected override void SetValue(object newValue)
        {
            // Make sure to take the inherited behavior
            base.SetValue(newValue);

            // Check to see if we can update the slider
            if ((sliderComponent != null) && !sliderIsBeingDragged)
            {
                // Get the DateTime
                DateTime fromDateTime = DateTimeValue;
                DateTime fromMinDateTime = sliderMinValueDateTime;
                DateTime fromMaxDateTime = sliderMaxValueDateTime;

                // Get the slider value by remapping the tick range to the slider range
                float value = (float)RemapValue1(fromDateTime.Ticks,
                    fromMinDateTime.Ticks, fromMaxDateTime.Ticks,           // DateTime range
                    sliderComponent.minValue, sliderComponent.maxValue);    // Slider range

                // Normalize the DateTime value to the slider range
                sliderComponent.value = value;
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
            if (sliderComponent == null)
            {
                sliderComponent = GetComponentInParent<Slider>();
            }

            if (sliderComponent != null)
            {
                // Obtain the event trigger
                EventTrigger trigger = sliderComponent.GetComponent<EventTrigger>();
                if (!trigger)
                {
                    // None available, so create an event trigger
                    sliderComponent.gameObject.AddComponent<EventTrigger>();
                }

                // Create our slider event listeners
                EventTrigger.Entry entry;
                
                // BeginDrag
                entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.BeginDrag;
                entry.callback.AddListener((data) => { OnBeginDrag((PointerEventData)data); });
                trigger.triggers.Add(entry);

                // EndDrag
                entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.EndDrag;
                entry.callback.AddListener((data) => { OnEndDrag((PointerEventData)data); });
                trigger.triggers.Add(entry);
            }

        }

        /**
         * Remaps a value within a range to a value in another range:
         * 
         * f(x) = (fromValue - fromMin) / (fromMax - fromMin) * (toMax - toMin) + toMin
         * 
         * @param fromValue The input value
         * @param fromMin The input minimum value
         * @param fromMax The input maximum value
         * @param toMin The output minimum value
         * @param toMax The output maximum value
         * 
         * @return The remapped value in the output range
         * 
         * TODO: Consider moving to a more generalized location so that other scripts may use
         */
        private double RemapValue1(double fromValue, double fromMin, double fromMax, double toMin, double toMax)
        {
            // Get the "from" range ratio
            double ratio = ((fromValue - fromMin) / (fromMax - fromMin));

            // Apply the ratio to the "to" range
            double toValue = ((toMax - toMin) * ratio) + toMin;

            return toValue;
        }

        /**
         * Remaps a value within a range to a value in another range, allowing for specifying the
         * precision:
         * 
         * @param fromValue The input value
         * @param fromMin The input minimum value
         * @param fromMax The input maximum value
         * @param toMin The output minimum value
         * @param toMax The output maximum value
         * @param precision The higher the number, the more precise the calculation. Default 5.
         * 
         * @return The normalized value in the output range
         * 
         * TODO: Consider moving to a more generalized location so that other scripts may use
         */
        private double RemapValue2(double fromValue, double fromMin, double fromMax, double toMin, double toMax, int precision = 5)
        {
            double scale = (toMax - toMin) / (fromMax - fromMin);
            double fromMinNeg = -1 * fromMin;
            double offset = (fromMinNeg * scale) + toMin;
            double toValue = (fromValue * scale) + offset;

            int calcScale = (int)Math.Pow(10, precision);
            return Math.Round(toValue * calcScale) / calcScale;
        }

    }
}
