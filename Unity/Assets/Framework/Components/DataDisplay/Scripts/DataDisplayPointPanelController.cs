// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem;
using GSFC.ARVR.MRET.Infrastructure.Framework;

public class DataDisplayPointPanelController : MonoBehaviour
{
    private enum PointState { redHigh, yellowHigh, green, yellowLow, redLow, unknown };

    public string pointKeyName;
    public int framesBetweenUpdates = 30;
    public Color32 greenConditionColor, yellowConditionColor, redConditionColor, unknownConditionColor;
    public Text titleLabel, stateLabel, valueLabel, minLabel, maxLabel;
    public GameObject plotContainer;
    public GameObject dataCapsulePrefab;
    public ChartAndGraph.GraphChart plotChart;

    private DataManager dataManager;

    void Start()
    {
        dataManager = FindObjectOfType<DataManager>();
        InitializePlot();
    }

    int frameCounter = 0;
    void Update()
    {
        if (frameCounter >= framesBetweenUpdates)
        {
            if (pointKeyName != "" && dataManager)
            {
                // Get point.
                DataManager.DataValue pointValue = dataManager.FindCompletePoint(pointKeyName);

                // Set title.
                titleLabel.text = pointKeyName;

                if (pointValue != null)
                {
                    // Handle state settings.
                    switch (pointValue.limitState)
                    {
                        case DataManager.DataValue.LimitState.Nominal:
                            stateLabel.color = valueLabel.color = greenConditionColor;
                            stateLabel.text = "Green (Nominal)";
                            break;

                        case DataManager.DataValue.LimitState.YellowHigh:
                            stateLabel.color = valueLabel.color = yellowConditionColor;
                            stateLabel.text = "Yellow (High)";
                            break;

                        case DataManager.DataValue.LimitState.YellowLow:
                            stateLabel.color = valueLabel.color = yellowConditionColor;
                            stateLabel.text = "Yellow (Low)";
                            break;

                        case DataManager.DataValue.LimitState.RedHigh:
                            stateLabel.color = valueLabel.color = redConditionColor;
                            stateLabel.text = "Red (High)";
                            break;

                        case DataManager.DataValue.LimitState.RedLow:
                            stateLabel.color = valueLabel.color = redConditionColor;
                            stateLabel.text = "Red (Low)";
                            break;

                        case DataManager.DataValue.LimitState.Undefined:
                        default:
                            stateLabel.color = valueLabel.color = unknownConditionColor;
                            stateLabel.text = "Unknown Limits";
                            break;
                    }

                    // Set current value.
                    valueLabel.text = pointValue.value.ToString();

                    // Set minimum value.
                    minLabel.text = "Minimum: " + pointValue.minimum;

                    // Set maximum value.
                    maxLabel.text = "Maximum: " + pointValue.maximum;

                    if (pointValue.IsNumber())
                    {
                        plotContainer.SetActive(true);
                        AddToPlot(System.Convert.ToDouble(pointValue.value));
                    }
                    else
                    {
                        plotContainer.SetActive(false);
                    }
                }
            }

            frameCounter = 0;
        }
        frameCounter++;
    }

    public void StartPinningDataPoint()
    {
        GameObject dataCapsule = Instantiate(dataCapsulePrefab);
        foreach (InputHand hand in MRET.InputRig.hands)
        {
            if (hand.handedness == InputHand.Handedness.right)
            {
                dataCapsule.transform.SetParent(hand.transform);
                dataCapsule.transform.localPosition = new Vector3(0, 0.008f, 0.061f);
                dataCapsule.transform.localRotation = Quaternion.Euler(60, 0, 90);
                dataCapsule.GetComponent<DataCapsuleController>().pointKeyName = pointKeyName;
                return;
            }
        }
    }

#region Plot
    public int totalPoints = 5;
    private void InitializePlot()
    {
        if (plotChart != null)
        {
            float x = 3f * totalPoints;
            plotChart.DataSource.StartBatch();
            plotChart.DataSource.ClearCategory("Value");
            plotChart.DataSource.EndBatch();
        }
    }

    private void AddToPlot(double newValue)
    {
        plotChart.DataSource.AddPointToCategoryRealtime("Value", System.DateTime.Now, newValue, 0.1f);
    }
#endregion

#region Helpers
    private object FindMinimumValue(DataManager.DataValue pointValue)
    {
        object minimumValue = pointValue.value;
        foreach (DataManager.DataValue value in pointValue.previousValues)
        {
            if (value.value is double && minimumValue is double)
            {
                if ((double) value.value < (double) minimumValue)
                {
                    minimumValue = value.value;
                }
            }
            else if (value.value is float && minimumValue is float)
            {
                if ((float) value.value < (float) minimumValue)
                {
                    minimumValue = value.value;
                }
            }
            else if (value.value is int && minimumValue is int)
            {
                if ((int) value.value < (int) minimumValue)
                {
                    minimumValue = value.value;
                }
            }
            else if (value.value is uint && minimumValue is uint)
            {
                if ((uint) value.value < (uint) minimumValue)
                {
                    minimumValue = value.value;
                }
            }
            else if (value.value is short && minimumValue is short)
            {
                if ((short) value.value < (short) minimumValue)
                {
                    minimumValue = value.value;
                }
            }
            else if (value.value is ushort && minimumValue is ushort)
            {
                if ((ushort) value.value < (ushort) minimumValue)
                {
                    minimumValue = value.value;
                }
            }
            else if (value.value is long && minimumValue is long)
            {
                if ((long) value.value < (long) minimumValue)
                {
                    minimumValue = value.value;
                }
            }
            else if (value.value is ulong && minimumValue is ulong)
            {
                if ((ulong) value.value < (ulong) minimumValue)
                {
                    minimumValue = value.value;
                }
            }
            else if (value.value is decimal && minimumValue is decimal)
            {
                if ((decimal) value.value < (decimal) minimumValue)
                {
                    minimumValue = value.value;
                }
            }
            else if (value.value is byte && minimumValue is byte)
            {
                if ((byte) value.value < (byte) minimumValue)
                {
                    minimumValue = value.value;
                }
            }
            else if (value.value is sbyte && minimumValue is sbyte)
            {
                if ((sbyte) value.value < (sbyte) minimumValue)
                {
                    minimumValue = value.value;
                }
            }
        }
        return minimumValue;
    }

    private object FindMaximumValue(DataManager.DataValue pointValue)
    {
        object maximumValue = pointValue.value;
        foreach (DataManager.DataValue value in pointValue.previousValues)
        {
            if (value.value is double && maximumValue is double)
            {
                if ((double) value.value > (double) maximumValue)
                {
                    maximumValue = value.value;
                }
            }
            else if (value.value is float && maximumValue is float)
            {
                if ((float) value.value > (float) maximumValue)
                {
                    maximumValue = value.value;
                }
            }
            else if (value.value is int && maximumValue is int)
            {
                if ((int) value.value > (int) maximumValue)
                {
                    maximumValue = value.value;
                }
            }
            else if (value.value is uint && maximumValue is uint)
            {
                if ((uint) value.value > (uint) maximumValue)
                {
                    maximumValue = value.value;
                }
            }
            else if (value.value is short && maximumValue is short)
            {
                if ((short) value.value > (short) maximumValue)
                {
                    maximumValue = value.value;
                }
            }
            else if (value.value is ushort && maximumValue is ushort)
            {
                if ((ushort) value.value > (ushort) maximumValue)
                {
                    maximumValue = value.value;
                }
            }
            else if (value.value is long && maximumValue is long)
            {
                if ((long) value.value > (long) maximumValue)
                {
                    maximumValue = value.value;
                }
            }
            else if (value.value is ulong && maximumValue is ulong)
            {
                if ((ulong) value.value > (ulong) maximumValue)
                {
                    maximumValue = value.value;
                }
            }
            else if (value.value is decimal && maximumValue is decimal)
            {
                if ((decimal) value.value > (decimal) maximumValue)
                {
                    maximumValue = value.value;
                }
            }
            else if (value.value is byte && maximumValue is byte)
            {
                if ((byte) value.value > (byte) maximumValue)
                {
                    maximumValue = value.value;
                }
            }
            else if (value.value is sbyte && maximumValue is sbyte)
            {
                if ((sbyte) value.value > (sbyte) maximumValue)
                {
                    maximumValue = value.value;
                }
            }
        }
        return maximumValue;
    }
#endregion
}