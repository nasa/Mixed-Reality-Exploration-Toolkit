// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using GOV.NASA.GSFC.XR.MRET.Project;
using GOV.NASA.GSFC.XR.MRET.SceneObjects;
using GOV.NASA.GSFC.XR.MRET.UI.Data;

namespace GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.Marker
{
    /// <remarks>
    /// History:
    /// 1 January 9999: Created
    /// </remarks>
	///
	/// <summary>
	/// MarkerConfigurationPanelController
	///
	/// TODO: Describe this class here...
	///
    /// Author: TODO
	/// </summary>
	/// 
	public class MarkerConfigurationPanelController : MRETBehaviour, IInteractableConfigurationPanelController
    {
		/// <seealso cref="MRETBehaviour.ClassName"/>
		public override string ClassName => nameof(MarkerConfigurationPanelController);

        public Text titleText;
        public InputField iotInputField;
        public Button enableLimitsButton;
        public Button displayGraphanaButton;

        private Color32 Selected = new Color32(45, 255, 0, 255);
        private Color32 Unselected = new Color32(81, 105, 122, 255);

        private string iotTopic;
        private string trackingDataPointKey;

        private DataDisplayPointPanelController dataDisplay;

        #region MRETBehaviour
        /// <seealso cref="MRETBehaviour.IntegrityCheck"/>
        protected override IntegrityState IntegrityCheck()
		{
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            return (
                (state == IntegrityState.Failure) // TODO: || (MyRequiredRef == null)
				
                    ? IntegrityState.Failure   // Fail is base class fails or anything is null
                    : IntegrityState.Success); // Otherwise, our integrity is valid
		}

		/// <seealso cref="MRETBehaviour.MRETAwake"/>
		protected override void MRETAwake()
		{
			// Take the inherited behavior
			base.MRETAwake();

			// TODO: Custom initialization (before deserialization)
			
		}
		
		/// <seealso cref="MRETBehaviour.MRETStart"/>
		protected override void MRETStart()
		{
			// Take the inherited behavior
			base.MRETStart();

			// TODO: Custom initialization (after deserialization)
			
		}

        /// <seealso cref="MRETBehaviour.MRETOnDestroy"/>
        protected override void MRETOnDestroy()
        {
            base.MRETOnDestroy();

            Destroy(dataDisplay);
        }
        #endregion MRETBehaviour

        #region IInteractableConfigurationPanelController
        /// <seealso cref="IInteractableConfigurationPanelController.ConfiguringInteractable"/>
        public IInteractable ConfiguringInteractable { get; private set; }

        /// <seealso cref="IInteractableConfigurationPanelController.PanelTitle"/>
        public string PanelTitle { get; private set; }

        /// <seealso cref="IInteractableConfigurationPanelController.Initialize(IInteractable, string)"/>
        public void Initialize(IInteractable configuringInteractable, string panelTitle = null)
        {
            if (configuringInteractable == null)
            {
                LogError("Supplied interactable is null");
                return;
            }

            // Initialize the key properties
            ConfiguringInteractable = configuringInteractable;
            PanelTitle = panelTitle ?? configuringInteractable.name;

            // Initialize the components
            if (titleText != null)
            {
                titleText.text = PanelTitle;
            }
            if (enableLimitsButton != null)
            {
                enableLimitsButton.image.color = Unselected;
            }
            if (displayGraphanaButton != null)
            {
                displayGraphanaButton.image.color = Unselected;
            }
            if (iotInputField != null)
            {
                iotInputField.onValueChanged.AddListener((v) =>
                {
                    SetIoTTopic(v);
                });
            }
        }
        #endregion IInteractableConfigurationPanelController

        /// <summary>
        /// Closes and destroys this configuration panel controller game object
        /// </summary>
        public void Close()
        {
            Destroy(dataDisplay);
            Destroy(gameObject);
        }

        void SetIoTTopic(string iotTopic)
        {
            this.iotTopic = iotTopic;
        }

        public void ApplyIoTTopic()
        {
            if (!string.IsNullOrEmpty(iotTopic))
            {
                // NOTE: We never remove topics so keep track of the one we are displaying
                string dataPointKey = InteractableSceneObject.DATA_POINT_KEY_PREFIX + iotTopic.Trim().ToUpper().Replace('/', '.');
                ConfiguringInteractable.AddDataPoint(dataPointKey);
                trackingDataPointKey = dataPointKey;

                // FIXME: DataDisplayPointPanelController can only display one telemetry key at a time
                if (dataDisplay != null)
                {
                    dataDisplay.pointKeyName = trackingDataPointKey;
                }
            }
        }

        public void DisplayGraphana()
        {
            if (displayGraphanaButton != null)
            {
                if (displayGraphanaButton.image.color == Unselected)
                {
                    if (dataDisplay == null)
                    {
                        // Create a data display
                        GameObject dataDisplayGO = Instantiate(ProjectManager.MarkerManager.dataDisplayPrefab, transform.position, transform.rotation);
                        dataDisplay = dataDisplayGO.GetComponent<DataDisplayPointPanelController>();
                    }

                    // Update only if we have a controller
                    if (dataDisplay != null)
                    {
                        if (!string.IsNullOrEmpty(trackingDataPointKey))
                        {
                            dataDisplay.pointKeyName = trackingDataPointKey;
                        }
                        displayGraphanaButton.image.color = Selected;
                    }
                }
                else
                {
                    Destroy(dataDisplay);
                    displayGraphanaButton.image.color = Unselected;
                }
            }
        }

        public void ApplyLimitShader()
        {
            if (enableLimitsButton != null)
            {
                if (enableLimitsButton.image.color == Unselected)
                {
                    ConfiguringInteractable.ShadeForLimitViolations = true;
                    enableLimitsButton.image.color = Selected;
                }
                else
                {
                    ConfiguringInteractable.ShadeForLimitViolations = false;
                    enableLimitsButton.image.color = Unselected;
                }
            }
        }
    }
}
