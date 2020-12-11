using System;
using UnityEngine;
using UnityEngine.UI;
using GSFC.ARVR.MRET.Time;
using GSFC.ARVR.Utilities.Renderer;

public class TimeIndicatorManager : MonoBehaviour
{
    private int updateCounter = 0;
    [Tooltip("Modulates the frequency of time display updates. The value represents a counter modulo to determine how many calls to Update will be skipped before refreshing.")]
    public int updateRateModulo = 1;

    [Tooltip("The TimeManager being used to supply the simulated date/time. If not supplied, one will be located at Start")]
    public TimeManager timeManager;
    [Tooltip("The DataManager to use for retrieving the simulated date/time. If not supplied, one will be located at Start")]
    public DataManager dataManager;

    [Tooltip("The format of the displayed date/time.")]
    public string timeFormat = "s";

    [Tooltip("The Text object used to display the date/time.")]
    public Text timeText;

    private void Awake()
    {
    }

    private void Start()
    {
        // Located the DataManager if one wasn't assigned to the script in the editor
        if (dataManager == null)
        {
            dataManager = FindObjectOfType<DataManager>();
        }

        // Located the TimeManager if one wasn't assigned to the script in the editor
        if (timeManager == null)
        {
            timeManager = FindObjectOfType<TimeManager>();
        }
    }

    private void Update()
    {
        // Performance management
        updateCounter++;
        if (updateCounter >= updateRateModulo)
        {
            // Reset the update counter
            updateCounter = 0;

            if ((dataManager != null) && (timeManager != null) && timeManager.enabled)
            {
                // Get the project time
                var projectTimeObj = dataManager.FindPoint(TimeManager.TIME_KEY_NOW);
                if (projectTimeObj is DateTime)
                {
                    // Extract the time
                    DateTime projectTime = (DateTime)projectTimeObj;

                    // Format the time
                    string timeStr = projectTime.ToString(timeFormat);

                    // Display the time
                    if (timeText != null)
                    {
                        timeText.text = timeStr;
                    }
                }
            }
        }
    }

    public void ResetTime()
    {
        // Store the reset request into the data manager
        if (timeManager != null)
        {
            // Mark for reset
            timeManager.ResetTime();
        }
    }
}