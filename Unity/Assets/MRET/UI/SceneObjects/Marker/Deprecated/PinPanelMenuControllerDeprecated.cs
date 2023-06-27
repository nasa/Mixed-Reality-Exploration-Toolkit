// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using GOV.NASA.GSFC.XR.MRET.Project;
using GOV.NASA.GSFC.XR.MRET.SceneObjects;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Pin;
using GOV.NASA.GSFC.XR.MRET.UI.Data;

namespace GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.Pin
{
    /// <remarks>
    /// History:
    /// 3 October 2022: Created
    /// </remarks>
    ///
    /// <summary>
    /// PinPanelMenuControllerDeprecated
    ///
    /// Manages the individual pin panel. Mostly used for IoT settings
    ///
    /// Author: Sean Letavish
    /// </summary>
    /// 
    public class PinPanelMenuControllerDeprecated : SceneObjectDeprecated
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(PinPanelMenuControllerDeprecated);

        //Boolean to know if initialized
        private bool initialized = false;

        //Pin this panel is controlling
        private GameObject selectedPin;

        //Title of the pin
        private string objectTitle;

        //Components on panel
        public Text titleText;
        public InputField IoTInput;
        public Button changePinMaterialToIoT;

        //Colors to change button color
        Color GreenButton = new Color(45f, 255f, 0f, 255f);
        Color BlueButton = new Color(81f, 105f, 122f, 255f);

        //Client entered by user
        public static string IoTClientName;

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

        public void Close()
        {
            if (selectedPin == null)
            {
                Destroy(gameObject);
            }
            else
            {
                InteractablePinDeprecated iPin = selectedPin.GetComponent<InteractablePinDeprecated>();
                if (iPin)
                {
                    Destroy(gameObject);
                }
            }
        }

        public void SetTitle(string titleToSet)
        {
            titleText.text = titleToSet;
        }

        // Start is called before the first frame update
        protected override void MRETStart()
        {
            base.MRETStart();
            initialized = true;
            selectedPin = InteractablePinDeprecated.selectedPin;
            SetTitle(PinFileBrowserHelperDeprecated.selectedPinName);
            changePinMaterialToIoT.image.color = BlueButton;

            if (IoTInput)
            {
                IoTInput.onValueChanged.AddListener((v) =>
                {
                    SetIoTName(v);
                });
            }
        }

        void SetIoTName(string iotname)
        {
            IoTClientName = iotname;
        }

        public void GiveIoTName()
        {
            if (IoTClientName != null)
            {
                selectedPin.GetComponent<InteractablePinDeprecated>().ChangeKey();
            }

            GameObject DataPanel = Instantiate(ProjectManager.PinManagerDeprecated.DataDisplayPanel, transform.position, transform.rotation);
            DataPanel.GetComponent<DataDisplayPointPanelController>().pointKeyName = selectedPin.GetComponent<InteractablePinDeprecated>().publicIotKey;
        }

        public void ApplyLimitShader()
        {
            if (changePinMaterialToIoT.image.color == BlueButton)
            {
                selectedPin.GetComponent<InteractablePinDeprecated>().shadeForLimitViolations = true;
                changePinMaterialToIoT.image.color = GreenButton;
            }

            else
            {
                selectedPin.GetComponent<InteractablePinDeprecated>().shadeForLimitViolations = false;
                changePinMaterialToIoT.image.color = BlueButton;
                selectedPin.GetComponent<InteractablePinDeprecated>().SetColor();
            }
        }

    }
}