using UnityEngine;

namespace GSFC.ARVR.MRET.Telemetry
{
    /**
     * Telemetry
     * 
     * Repeatedly extracts the telemetry point value identified by the key from the DataManager and
     * stores the current telemetry value, accessible by the <Code>Value</Code> property.</br>
     * 
     * @author Jeffrey Hosler
     */
    public class Telemetry : MonoBehaviour
    {
        public static readonly string NAME = nameof(Telemetry);

        [Tooltip("The DataManager to use for retrieving the telemetry. If not supplied, one will be located at Start")]
        public DataManager dataManager;

        [Tooltip("DataManager key for the telemetry value")]
        public string key;

        [Tooltip("Current telemetry value")]
        private object value;
        public object Value
        {
            get
            {
                return this.value;
            }
        }

        // Performance Management
        private int updateCounter = 0;
        [Tooltip("Modulates the frequency of date/time updates published to the DataManager. The value represents a counter modulo to determine how many calls to Update will be skipped before publishing.")]
        public int updateRateModulo = 1;

        /**
         * Called in the Update method to assign the new telemetry value. Available to
         * subclasses to perform addition actions when the new telemetry value is assigned.
         * 
         * @param newValue The new telemetry value
         */
        protected virtual void SetValue(object newValue)
        {
            value = newValue;
        }

        /**
         * Start is called before the first frame update
         */
        protected virtual void Start()
        {
            // Try to get a reference to the data manager if not explictly set
            if (dataManager == null)
            {
                dataManager = SessionManager.instance.dataManager;
                if (dataManager == null)
                {
                    Debug.LogError("[" + NAME + "] Unable to obtain a reference to a DataManager");
                }
            }
        }

        /**
         * Update is called once per frame
         */
        protected virtual void Update()
        {
            // Performance management
            updateCounter++;
            if (updateCounter >= updateRateModulo)
            {
                // Reset the update counter
                updateCounter = 0;

                // Make sure we have a valid DataManager
                if (dataManager != null)
                {
                    // Read the telemetry value and assign it to our private property
                    SetValue(dataManager.FindPoint(key));
                }
            }
        }
    }

}