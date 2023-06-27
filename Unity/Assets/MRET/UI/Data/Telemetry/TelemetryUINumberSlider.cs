// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using GOV.NASA.GSFC.XR.MRET.Data.Telemetry;

namespace GOV.NASA.GSFC.XR.MRET.UI.Data.Telemetry
{
    /// <summary>
    /// TelemetryUINumberSlider
    /// 
    /// Extends TelemetryNumber to control the value of a Slider component. If a Slider component is
    /// not specified, the parent will be queried for a Slider component reference to use.</br>
    /// 
    /// @author Jeffrey Hosler
    /// </summary>
    public class TelemetryUINumberSlider : TelemetryNumber
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(TelemetryUINumberSlider);

        [Tooltip("The optional Slider component to control with the telemetry value. " +
            "If not specified, the parent will be queried for a Slider component.")]
        public Slider sliderComponent;

        // Used to track user interaction
        private bool sliderIsBeingDragged = false;

        /// <seealso cref="Telemetry.SetValue(object)"/>
        protected override void SetValue(object newValue)
        {
            // Make sure to take the inherited behavior
            base.SetValue(newValue);

            // Check to see if we can update the slider
            if ((sliderComponent != null) && !sliderIsBeingDragged)
            {
                // Control the Toggle state
                sliderComponent.value = FloatValue;
            }
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

    }
}
