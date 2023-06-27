// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using GOV.NASA.GSFC.XR.Utilities.Properties;
using GOV.NASA.GSFC.XR.MRET.Data.Telemetry;

namespace GOV.NASA.GSFC.XR.MRET.UI.Data.Telemetry
{
    /// <summary>
    /// TelemetryUIDateTimeSlider
    /// 
    /// Extends Telemetry to control the value of a Slider component. If a Slider component is
    /// not specified, the parent will be queried for a Slider component reference to use.</br>
    /// 
    /// TODO: We might consider extending from TelemetryUINumberSlider if we decide that the TelemetryNumber
    /// script will use DateTime.Ticks as the Number for DateTime types. There are limitations with this too
    /// so let's just leave as is for now.
    /// 
    /// Author: Jeffrey Hosler
    /// </summary>
    public class TelemetryUIDateTimeSlider : TelemetryDateTime
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(TelemetryUIDateTimeSlider);

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

        /// <summary>
        /// Performs a mapping of the supplied slider value to a DateTime. This function
        /// does not alter the state of the slider, but returns the DateTime mapping to the
        /// supplied slider value.
        /// </summary>
        /// <param name="sliderValue">The slider value to query</param>
        /// <returns>The <code>DateTime</code> from the slider value</returns>
        /// <seealso cref="DateTime"/>
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

        /// <summary>
        /// Event handler for the Slider BeginDrag event<br>
        /// </summary>
        /// <param name="pointerEventData">The <code>PointerEventData</code> containing the event data</param>
        /// <seealso cref="PointerEventData"/>
        protected void OnBeginDrag(PointerEventData pointerEventData)
        {
            sliderIsBeingDragged = true;
        }

        /// <summary>
        /// Event handler for the Slider EndDrag event<br>
        /// </summary>
        /// <param name="pointerEventData">The <code>PointerEventData</code> containing the event data</param>
        /// <seealso cref="PointerEventData"/>
        protected void OnEndDrag(PointerEventData pointerEventData)
        {
            sliderIsBeingDragged = false;
        }

        /// <seealso cref="Telemetry.SetValue(object)"/>
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

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            base.MRETStart();

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

        /// <summary>
        /// Remaps a value within a range to a value in another range:
        /// 
        /// f(x) = (fromValue - fromMin) / (fromMax - fromMin)/// (toMax - toMin) + toMin
        /// 
        /// TODO: Consider moving to a more generalized location so that other scripts may use
        /// </summary>
        /// <param name="fromValue">The input value</param>
        /// <param name="fromMin">The input minimum value</param>
        /// <param name="fromMax">The input maximum value</param>
        /// <param name="toMin">The output minimum value</param>
        /// <param name="toMax">The output maximum value</param>
        /// <returns>The normalized value in the output range</returns>
        private double RemapValue1(double fromValue, double fromMin, double fromMax, double toMin, double toMax)
        {
            // Get the "from" range ratio
            double ratio = ((fromValue - fromMin) / (fromMax - fromMin));

            // Apply the ratio to the "to" range
            double toValue = ((toMax - toMin) * ratio) + toMin;

            return toValue;
        }

        /// <summary>
        /// Remaps a value within a range to a value in another range, allowing for specifying the
        /// precision:
        /// 
        /// TODO: Consider moving to a more generalized location so that other scripts may use
        /// </summary>
        /// <param name="fromValue">The input value</param>
        /// <param name="fromMin">The input minimum value</param>
        /// <param name="fromMax">The input maximum value</param>
        /// <param name="toMin">The output minimum value</param>
        /// <param name="toMax">The output maximum value</param>
        /// <param name="precision">The higher the number, the more precise the calculation. Default 5.</param>
        /// <returns>The normalized value in the output range</returns>
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
