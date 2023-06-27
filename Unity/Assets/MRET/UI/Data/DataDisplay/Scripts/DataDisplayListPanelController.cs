// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using GOV.NASA.GSFC.XR.XRUI.Widget;
using GOV.NASA.GSFC.XR.MRET.Data;
using GOV.NASA.GSFC.XR.MRET.SceneObjects;

namespace GOV.NASA.GSFC.XR.MRET.UI.Data
{
    public class DataDisplayListPanelController : MRETBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(DataDisplayListPanelController);

        public int framesBetweenUpdates = 30;
        public ScrollListManager scrollListManager;
        public Text label;
        public GameObject dataPointPanelPrefab;

        private DataManager dataManager;
        private List<DataManager.DataValue> listedDataValues = new List<DataManager.DataValue>();
        private IInteractable interactable;

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

            if (dataManager == null)
            {
                dataManager = MRET.DataManager;
            }
            scrollListManager.SetTitle("Data Points");
            interactable = GetComponentInParent<IInteractable>();
            if (interactable != null)
            {
                label.text = interactable.name + " Data Points";
            }
            else
            {
                label.text = "Global Data Points";
            }
        }

        int frameCounter = 0;
        void Update()
        {
            if (frameCounter >= framesBetweenUpdates)
            {
                if (dataManager)
                {
                    DataManager.DataValue[] currentDataPoints = dataManager.GetAllDataPoints();

                    if (interactable != null)
                    {
                        List<DataManager.DataValue> matchingDataPoints = new List<DataManager.DataValue>();

                        foreach (DataManager.DataValue dataPoint in currentDataPoints)
                        {
                            foreach (string keyName in interactable.DataPoints)
                            {
                                if (keyName == dataPoint.key)
                                {
                                    matchingDataPoints.Add(dataPoint);
                                }
                            }
                        }

                        currentDataPoints = matchingDataPoints.ToArray();
                    }

                    foreach (DataManager.DataValue currentDataPoint in currentDataPoints)
                    {
                        int listIndex = GetDataValueListIndex(currentDataPoint.key);
                        if (listIndex > -1)
                        {   // Point already exists in list => update it.
                            scrollListManager.UpdateScrollListItemLabel(listIndex, currentDataPoint.key + " = " + currentDataPoint.value);
                            listedDataValues[listIndex] = currentDataPoint;
                        }
                        else
                        {   // Add point to list.
                            int indexToSelect = scrollListManager.numItems;
                            UnityEngine.Events.UnityEvent clickEvent = new UnityEngine.Events.UnityEvent();
                            clickEvent.AddListener(new UnityEngine.Events.UnityAction(() => { ExpandDataPoint(indexToSelect); }));
                            scrollListManager.AddScrollListItem(currentDataPoint.key + " = " + currentDataPoint.value, clickEvent);
                            listedDataValues.Add(currentDataPoint);
                        }
                    }
                }

                frameCounter = 0;
            }
            frameCounter++;
        }

        private void ExpandDataPoint(int listIndex)
        {
            OpenDataPointPanel(listedDataValues[listIndex].key);
        }

        private void OpenDataPointPanel(string pointName)
        {
            GameObject pointPanel = Instantiate(dataPointPanelPrefab);
            pointPanel.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - 0.1f);
            pointPanel.transform.rotation = transform.rotation;

            DataDisplayPointPanelController pointPanelController = pointPanel.GetComponent<DataDisplayPointPanelController>();
            if (pointPanelController)
            {
                pointPanelController.pointKeyName = pointName;
            }
        }

        private int GetDataValueListIndex(string key)
        {
            for (int i = 0; i < listedDataValues.Count; i++)
            {
                if (key == listedDataValues[i].key)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}