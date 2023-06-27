// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;

namespace GOV.NASA.GSFC.XR.MRET.Data.Telemetry
{
    /**
    /// Telemetry
    /// 
    /// Repeatedly extracts the telemetry point value identified by the key from the DataManager and
    /// stores the current telemetry value, accessible by the <Code>Value</Code> property.</br>
    /// 
    /// @author Jeffrey Hosler
     */
    public class Telemetry : MRETUpdateBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(Telemetry);

        [Tooltip("DataManager key for the telemetry value")]
        public string key;

        [Tooltip("Current telemetry value")]
        private object value;
        public object Value { get => value; }

        /// <summary>
        /// Called in the Update method to assign the new telemetry value. Available to
        /// subclasses to perform addition actions when the new telemetry value is assigned.
        /// </summary>
        /// <param name="newValue">The new telemetry value</param>
        protected virtual void SetValue(object newValue)
        {
            value = newValue;
        }

        /// <seealso cref="MRETUpdateBehaviour.MRETUpdate"/>
        protected override void MRETUpdate()
        {
            base.MRETUpdate();

            // Read the telemetry value and assign it to our private property
            SetValue(MRET.DataManager.FindPoint(key));
        }
    }

}