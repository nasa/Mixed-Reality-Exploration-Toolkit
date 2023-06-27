// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;
using GOV.NASA.GSFC.XR.MRET.Data;

namespace GOV.NASA.GSFC.XR.MRET.UI.Data
{
    public class DataDisplayPointPanelController : MRETUpdateBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName
        {
            get
            {
                return nameof(DataDisplayPointPanelController);
            }
        }

        private enum PointState { redHigh, yellowHigh, green, yellowLow, redLow, unknown };

        public string pointKeyName;
        public Color32 unknownConditionColor;
        public Text titleLabel, stateLabel, valueLabel, minLabel, maxLabel;
        public GameObject plotContainer;
        public GameObject dataCapsulePrefab;
        public GameObject graphChartContainer;

#if MRET_EXTENSION_CHARTANDGRAPH
        private ChartAndGraph.GraphChart plotChart;
#endif

        /// <seealso cref="MRETBehaviour.IntegrityCheck"/>
        protected override IntegrityState IntegrityCheck()
        {
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            return (
                (state == IntegrityState.Failure) // TODO: || (MyRequiredRef == null)
                    ? IntegrityState.Failure      // Fail is base class fails or anything is null
                    : IntegrityState.Success);    // Otherwise, our integrity is valid
        }

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            // Take the inherited behavior
            base.MRETStart();

            if (graphChartContainer != null)
            {
#if MRET_EXTENSION_CHARTANDGRAPH
                plotChart = graphChartContainer.GetComponentInChildren<ChartAndGraph.GraphChart>();
#else
                LogWarning("ChartAndGraph is not installed");
#endif
            }
            else
            {
                Debug.LogWarning("Graph chart container is not defined");
            }

            InitializePlot();
        }

        /// <seealso cref="MRETUpdateBehaviour.MRETUpdate"/>
        protected override void MRETUpdate()
        {
            // Take the inherited behavior
            base.MRETUpdate();

            if (!string.IsNullOrEmpty(pointKeyName))
            {
                // Get point.
                DataManager.DataValue pointValue = MRET.DataManager.FindCompletePoint(pointKeyName);

                // Set title.
                titleLabel.text = pointKeyName;

                if (pointValue != null)
                {
                    // Set state value
                    if (stateLabel)
                    {
                        stateLabel.color = unknownConditionColor;
                        stateLabel.text = pointValue.limitState.ToString();
                    }

                    // Set current value
                    if (valueLabel)
                    {
                        valueLabel.color = unknownConditionColor;
                        valueLabel.text = pointValue.value.ToString();
                    }

                    // Set minimum value
                    if (minLabel)
                    {
                        minLabel.text = "Minimum: " + pointValue.minimum;
                    }

                    // Set maximum value
                    if (maxLabel)
                    {
                        maxLabel.text = "Maximum: " + pointValue.maximum;
                    }

                    // Update the plot
                    if (plotContainer)
                    {
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
            }
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
#if MRET_EXTENSION_CHARTANDGRAPH
            if (plotChart != null)
            {
                float x = 3f * totalPoints;
                plotChart.DataSource.StartBatch();
                plotChart.DataSource.ClearCategory("Value");
                plotChart.DataSource.EndBatch();
            }
#endif
        }

        private void AddToPlot(double newValue)
        {
#if MRET_EXTENSION_CHARTANDGRAPH
            plotChart.DataSource.AddPointToCategoryRealtime("Value", System.DateTime.Now, newValue, 0.1f);
#endif
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
                    if ((double)value.value < (double)minimumValue)
                    {
                        minimumValue = value.value;
                    }
                }
                else if (value.value is float && minimumValue is float)
                {
                    if ((float)value.value < (float)minimumValue)
                    {
                        minimumValue = value.value;
                    }
                }
                else if (value.value is int && minimumValue is int)
                {
                    if ((int)value.value < (int)minimumValue)
                    {
                        minimumValue = value.value;
                    }
                }
                else if (value.value is uint && minimumValue is uint)
                {
                    if ((uint)value.value < (uint)minimumValue)
                    {
                        minimumValue = value.value;
                    }
                }
                else if (value.value is short && minimumValue is short)
                {
                    if ((short)value.value < (short)minimumValue)
                    {
                        minimumValue = value.value;
                    }
                }
                else if (value.value is ushort && minimumValue is ushort)
                {
                    if ((ushort)value.value < (ushort)minimumValue)
                    {
                        minimumValue = value.value;
                    }
                }
                else if (value.value is long && minimumValue is long)
                {
                    if ((long)value.value < (long)minimumValue)
                    {
                        minimumValue = value.value;
                    }
                }
                else if (value.value is ulong && minimumValue is ulong)
                {
                    if ((ulong)value.value < (ulong)minimumValue)
                    {
                        minimumValue = value.value;
                    }
                }
                else if (value.value is decimal && minimumValue is decimal)
                {
                    if ((decimal)value.value < (decimal)minimumValue)
                    {
                        minimumValue = value.value;
                    }
                }
                else if (value.value is byte && minimumValue is byte)
                {
                    if ((byte)value.value < (byte)minimumValue)
                    {
                        minimumValue = value.value;
                    }
                }
                else if (value.value is sbyte && minimumValue is sbyte)
                {
                    if ((sbyte)value.value < (sbyte)minimumValue)
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
                    if ((double)value.value > (double)maximumValue)
                    {
                        maximumValue = value.value;
                    }
                }
                else if (value.value is float && maximumValue is float)
                {
                    if ((float)value.value > (float)maximumValue)
                    {
                        maximumValue = value.value;
                    }
                }
                else if (value.value is int && maximumValue is int)
                {
                    if ((int)value.value > (int)maximumValue)
                    {
                        maximumValue = value.value;
                    }
                }
                else if (value.value is uint && maximumValue is uint)
                {
                    if ((uint)value.value > (uint)maximumValue)
                    {
                        maximumValue = value.value;
                    }
                }
                else if (value.value is short && maximumValue is short)
                {
                    if ((short)value.value > (short)maximumValue)
                    {
                        maximumValue = value.value;
                    }
                }
                else if (value.value is ushort && maximumValue is ushort)
                {
                    if ((ushort)value.value > (ushort)maximumValue)
                    {
                        maximumValue = value.value;
                    }
                }
                else if (value.value is long && maximumValue is long)
                {
                    if ((long)value.value > (long)maximumValue)
                    {
                        maximumValue = value.value;
                    }
                }
                else if (value.value is ulong && maximumValue is ulong)
                {
                    if ((ulong)value.value > (ulong)maximumValue)
                    {
                        maximumValue = value.value;
                    }
                }
                else if (value.value is decimal && maximumValue is decimal)
                {
                    if ((decimal)value.value > (decimal)maximumValue)
                    {
                        maximumValue = value.value;
                    }
                }
                else if (value.value is byte && maximumValue is byte)
                {
                    if ((byte)value.value > (byte)maximumValue)
                    {
                        maximumValue = value.value;
                    }
                }
                else if (value.value is sbyte && maximumValue is sbyte)
                {
                    if ((sbyte)value.value > (sbyte)maximumValue)
                    {
                        maximumValue = value.value;
                    }
                }
            }
            return maximumValue;
        }
        #endregion Helpers
    }
}