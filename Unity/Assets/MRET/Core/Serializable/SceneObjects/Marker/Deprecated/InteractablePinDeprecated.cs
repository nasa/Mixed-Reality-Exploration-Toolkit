// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;
using GOV.NASA.GSFC.XR.MRET.Data;
using GOV.NASA.GSFC.XR.MRET.Project;
using GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.Pin;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Pin
{
    /// <remarks>
    /// History:
    /// 3 October 2022: Created
    /// </remarks>
    ///
    /// <summary>
    /// InteractablePinDeprecated
    ///
    /// Manages Individual pins.
    /// <Works with cref="PinManagerDeprecated"/>
    ///
    /// Author: Sean Letavish
    /// </summary>
    /// 
    public class InteractablePinDeprecated : SceneObjectDeprecated
    {
        //Renderer to change material color
        Renderer rend;

        // Pin Panel prefab 
        private GameObject pinPanelPrefab;

        //Pin Panel used for IoT Settings
        private GameObject pinPanel;

        //Gameobject used for loading panel
        private GameObject pinPanelInfo = null;

        //Transform to get position of main camera
        private Transform headsetObject;

        //IoT things
        private DataManager.DataValueEvent dataPointChangeEvent;
        public static GameObject selectedPin;
        public string publicIotKey;
        public enum PartShadingMode { MeshDefault, MeshLimits }
        public PartShadingMode shadingMode = PartShadingMode.MeshDefault;
        string pinName;
        public bool shadeForLimitViolations = false;
        private Dictionary<MeshRenderer, Material[]> revertMaterials = null;

        public override string ClassName
        {
            get
            {
                return nameof(InteractablePinDeprecated);
            }
        }

        // Start is called before the first frame update
        protected override void MRETStart()
        {
            base.MRETStart();

            Initialize();
        }

        /// <summary>
        /// Initialize this pin's settings
        /// </summary>
        private void Initialize()
        {
            this.transform.SetParent(MRET.ProjectDeprecated.projectObjectContainer.transform);
            rend = GetComponent<Renderer>();
            headsetObject = MRET.InputRig.GetComponentInChildren<InputHead>().transform;
            pinPanelPrefab = ProjectManager.PinManagerDeprecated.individualPinPanel;
            SetColor();
            Rigidbody rb = gameObject.AddComponent<Rigidbody>();
            gameObject.AddComponent<BoxCollider>();
            rb.isKinematic = true;
            pinName = this.name;
            transform.localScale = MRET.InputRig.GetComponentInChildren<PinMarkerDeprecated>().transform.localScale;

            if (MRET.InputRig.GetComponentInChildren<PinMarkerDeprecated>() != null)
            {
                string dataPointKey = "GOV.NASA.GSFC.XR.MRET.IOT.PAYLOAD." + ProjectManager.PinManagerDeprecated.PinID.ToUpper();
                dataPointKey = dataPointKey.Replace('/', '.');
                AddDataPoint(dataPointKey);
                publicIotKey = dataPointKey;
            }
        }

        /// <summary>
        /// If not using IoT material, material is set <by cref="PinManagerDeprecated"/>
        /// </summary>
        public void SetColor()
        {
            if (ProjectManager.PinManagerDeprecated.pinAndPathColor == Color.white)
            {
                rend.material.color = Color.white;
            }

            else if (ProjectManager.PinManagerDeprecated.pinAndPathColor == Color.blue)
            {
                rend.material.color = Color.blue;
            }

            else if (ProjectManager.PinManagerDeprecated.pinAndPathColor == Color.green)
            {
                rend.material.color = Color.green;
            }

            else if (ProjectManager.PinManagerDeprecated.pinAndPathColor == Color.red)
            {
                rend.material.color = Color.red;
            }
        }

        /// <summary>
        /// Controls wherther to create or destroy PinPanel when clicked
        /// </summary>
        /// <param name="hand"></param>
        public override void Use(InputHand hand)
        {
            base.Use(hand);
            selectedPin = this.gameObject;
            if (!pinPanel)
            {
                LoadPinPanel(hand.gameObject, true);
            }
            else
            {
                pinPanelInfo = new GameObject();
                pinPanelInfo.transform.position = pinPanel.transform.position;
                DestroyPinPanel();
            }
        }
        /// <summary>
        /// Loads the Pin Panel
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="reinitialize"></param>
        public void LoadPinPanel(GameObject controller, bool reinitialize)
        {
            // TODO: Enclosures do not have prefab panels, but should they?
            if (pinPanelPrefab == null)
            {
                LogWarning("Part does not have a part panel prefab defined.", nameof(LoadPinPanel));
                return;
            }

            // Instantiate the part panel
            pinPanel = Instantiate(pinPanelPrefab);

            // If position hasn't been set or reinitializing, initialize panel position.
            if ((pinPanelInfo == null) || reinitialize)
            {
                Collider objectCollider = GetComponent<Collider>();
                if (objectCollider)
                {
                    //Vector3 selectedPosition = objectCollider.ClosestPointOnBounds(controller.transform.position);

                    // Move panel between selected point and headset.
                    pinPanel.transform.position = Vector3.Lerp(controller.transform.position, headsetObject.transform.position, 0.1f);

                    // Try to move panel outside of object. If the headset is in the object, there is
                    // nothing we can do.
                    if (!objectCollider.bounds.Contains(headsetObject.transform.position))
                    {
                        while (ExistsWithinPart(pinPanel.transform.position))
                        {
                            pinPanel.transform.position = Vector3.Lerp(pinPanel.transform.position,
                                headsetObject.transform.position, 0.1f);
                        }
                    }
                    pinPanel.transform.position = Vector3.Lerp(pinPanel.transform.position,
                                headsetObject.transform.position, 0.1f);
                }
                else
                {   // No mesh, so load panel close to controller.
                    pinPanel.transform.position = controller.transform.position;
                }
            }
            else
            {
                pinPanel.transform.position = pinPanelInfo.transform.position;

                // Check if position is inside of object. If so, initialize it.
                Collider objectCollider = GetComponent<Collider>();
                if (objectCollider)
                {
                    if (ExistsWithinPart(pinPanel.transform.position))
                    {
                        // Try to move panel outside of object. If the headset is in the object, there is
                        // nothing we can do.
                        if (!objectCollider.bounds.Contains(headsetObject.transform.position))
                        {
                            while (ExistsWithinPart(pinPanel.transform.position))
                            {
                                pinPanel.transform.position = Vector3.Lerp(pinPanel.transform.position,
                                    headsetObject.transform.position, 0.1f);
                            }
                        }
                        pinPanel.transform.position = Vector3.Lerp(pinPanel.transform.position,
                                    headsetObject.transform.position, 0.1f);
                    }
                }
            }

            // Finally, make the panel a child of its gameobject and point it at the camera.
            pinPanel.transform.rotation = Quaternion.LookRotation(headsetObject.transform.forward);
            //partPanel.transform.SetParent(transform);
            pinPanel.transform.SetParent(null);

        }

        /// <summary>
        /// Destroys Pin Panel
        /// </summary>
        public void DestroyPinPanel()
        {
            if (pinPanel)
            {
                Destroy(pinPanel);
                selectedPin = null;
            }
        }

        private bool ExistsWithinPart(Vector3 pos)
        {
            Bounds bou = new Bounds();
            foreach (Collider coll in GetComponentsInChildren<Collider>())
            {
                bou.Encapsulate(coll.bounds);
            }
            return bou.Contains(pos);
        }

        public List<string> dataPoints = new List<string>();

        public bool AddDataPoint(string dataPoint)
        {
            if (dataPoints.Contains(dataPoint))
            {
                LogWarning("Point " + dataPoint + " already exists on " + pinName + ".", nameof(AddDataPoint));
                return false;
            }

            // TODO MRETStart is not being called in time.
            if (dataPointChangeEvent == null)
            {
                dataPointChangeEvent = new DataManager.DataValueEvent();
                dataPointChangeEvent.AddListener(HandleDataPointChange);
            }

            dataPoints.Add(dataPoint);
            MRET.DataManager.SubscribeToPoint(dataPoint, dataPointChangeEvent);
            return true; //FIXME: at somepoint after subscribing and before the event triggers, the thresholds are cleared
        }

        public void HandleDataPointChange(DataManager.DataValue dataValue)
        {
            DataManager.DataValue.LimitState limitState = dataValue.limitState;
            if (limitState == DataManager.DataValue.LimitState.Undefined)
            {
                HandleUndefinedLimitState();
            }
            else
            {
                HandleLimitViolation(limitState);
            }
        }

        private void HandleUndefinedLimitState()
        {
            if (shadingMode == PartShadingMode.MeshLimits)
            {
                shadingMode = PartShadingMode.MeshDefault;
                RevertMeshLimitShader();
            }
        }

        private void HandleLimitViolation(DataManager.DataValue.LimitState limitState)
        {
            if (shadeForLimitViolations)
            {
                if (shadingMode == PartShadingMode.MeshLimits)
                {
                    RevertMeshLimitShader();
                }
                shadingMode = PartShadingMode.MeshLimits;
                ApplyMeshLimitShader(limitState);
            }
        }

        private void RevertMeshLimitShader()
        {
            if (revertMaterials == null)
            {
                LogError("No revert materials. Will not revert materials.", nameof(RevertMeshLimitShader));
                return;
            }

            RestoreObjectMaterials();
            revertMaterials = null;
        }

        public void ChangeKey()
        {
            string dataPointKey = "GOV.NASA.GSFC.XR.MRET.IOT.PAYLOAD." + PinPanelMenuControllerDeprecated.IoTClientName.ToUpper();
            dataPointKey = dataPointKey.Replace('/', '.');
            AddDataPoint(dataPointKey);
            publicIotKey = dataPointKey;
        }

        public void ApplyMeshLimitShader(DataManager.DataValue.LimitState limitType)
        {
            if (revertMaterials != null)
            {
                LogError("Revert materials not empty. Will not apply mesh limit shader.", nameof(ApplyMeshLimitShader));
                return;
            }

            RestoreObjectMaterials();
            SaveObjectMaterials();

            revertMaterials = new Dictionary<MeshRenderer, Material[]>();
            foreach (MeshRenderer rend in gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                revertMaterials.Add(rend, rend.materials);

                rend.materials = new Material[] { MRET.LimitMaterial };
                //TODO: revisit this switch statement with Unity Color if we can replace DataManager.DataValue.LimitState
                Color limitColor = Color.black;
                switch (limitType)
                {
                    case DataManager.DataValue.LimitState.Blue:
                        limitColor = Color.blue;
                        break;
                    case DataManager.DataValue.LimitState.Cyan:
                        limitColor = Color.cyan;
                        break;
                    case DataManager.DataValue.LimitState.DarkGray:
                        limitColor = new Color(0.25f, 0.25f, 0.25f);
                        break;
                    case DataManager.DataValue.LimitState.Gray:
                        limitColor = Color.gray;
                        break;
                    case DataManager.DataValue.LimitState.Green:
                        limitColor = Color.green;
                        break;
                    case DataManager.DataValue.LimitState.LightGray:
                        limitColor = new Color(0.75f, 0.75f, 0.75f);
                        break;
                    case DataManager.DataValue.LimitState.Magenta:
                        limitColor = Color.magenta;
                        break;
                    case DataManager.DataValue.LimitState.Orange:
                        limitColor = new Color(1f, 0.647058824f, 0f);
                        break;
                    case DataManager.DataValue.LimitState.Pink:
                        limitColor = new Color(1f, 0.752941176f, 0.796078431f);
                        break;
                    case DataManager.DataValue.LimitState.Red:
                        limitColor = Color.red;
                        break;
                    case DataManager.DataValue.LimitState.White:
                        limitColor = Color.white;
                        break;
                    case DataManager.DataValue.LimitState.Yellow:
                        limitColor = Color.yellow;
                        break;
                    case DataManager.DataValue.LimitState.Undefined:
                    case DataManager.DataValue.LimitState.Black:
                    default:
                        limitColor = Color.black;
                        break;
                }
                rend.materials[0].color = limitColor;
            }
        }
    }
}