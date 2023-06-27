// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Panel;
using GOV.NASA.GSFC.XR.MRET.Integrations.CPIS.Transforms;

namespace GOV.NASA.GSFC.XR.MRET.UI.HUD
{
    public class DisplayController : MonoBehaviour
    {
        /// <summary>
        /// Handles the menu and the configuring of the display itself. 
        /// Responsible for transforming the display into the different types of displays (World, Mini, HUD)
        /// </summary>
        /// 
        public enum Type { miniDisplay, worldDisplay, HUDDisplay };

        public Type type;

        public Toggle scale;
        public Toggle move;
        public Toggle pin;
        public Toggle size;

        public Toggle miniScale;
        public Toggle miniSize;

        public GameObject settingsMenuObject;
        public GameObject feedSettings;
        public GameObject miniSettingsMenuObject;
        public GameObject miniFeedSettings;
        public GameObject settings;
        public GameObject layout;
        public GameObject settingsButton;
        [Tooltip("GameObject that contains the HTML display")]
        public GameObject htmlGameObject;
        [Tooltip("GameObject that contains the contentImage")]
        public GameObject contentImageGameObject;
        GameObject lController;
        GameObject rController;

        public Material renderAbove;

        public int displayNumber;

        private bool settingsDimmed = true;

        void Start()
        {
            SetType();

            lController = MRET.HudManager.leftController;
            rController = MRET.HudManager.rightController;
        }

        public void SetType()
        {
            switch (type)
            {
                case Type.miniDisplay:
                    settings.SetActive(true);
                    settingsMenuObject.SetActive(false);
                    miniSettingsMenuObject.SetActive(true);
                    settingsButton.transform.localScale = new Vector3(2, 2, 1);
                    settingsButton.transform.localPosition = new Vector3(.1f, -0.05f, 0);
                    scale = miniScale;
                    size = miniSize;
                    break;

                case Type.worldDisplay:
                    settings.SetActive(true);
                    settingsMenuObject.SetActive(true);
                    if (miniSettingsMenuObject) miniSettingsMenuObject.SetActive(false);
                    settingsButton.transform.localScale = new Vector3(1, 1, 1);
                    settingsButton.transform.localPosition = new Vector3(.05f, -0.05f, 0);
                    break;

                case Type.HUDDisplay:
                    settings.SetActive(false);
                    Destroy(gameObject.GetComponent<InteractablePanel>());
                    Destroy(gameObject.GetComponent<ScaleObjectTransform>());
                    Destroy(gameObject.GetComponent<GripMeasure>());
                    Destroy(contentImageGameObject.GetComponentInParent<GraphicRaycaster>());
                    contentImageGameObject.GetComponentInChildren<RawImage>().material = renderAbove;
                    contentImageGameObject.GetComponentInChildren<Text>().material = renderAbove;

                    BoxCollider[] colliders = gameObject.GetComponentsInChildren<BoxCollider>();
                    foreach (BoxCollider box in colliders)
                    {
                        Destroy(box);
                    }
                    break;
                default:
                    break;
            }
        }

        public void ToggleSettings()
        {
            if (settingsDimmed)
            {
                switch (type)
                {
                    case Type.miniDisplay:
                        miniSettingsMenuObject.SetActive(true);
                        settingsDimmed = false;
                        break;
                    case Type.worldDisplay:
                        settingsMenuObject.SetActive(true);
                        settingsDimmed = false;
                        break;
                    case Type.HUDDisplay:
                        break;
                }

            }
            else
            {
                switch (type)
                {
                    case Type.miniDisplay:
                        miniSettingsMenuObject.SetActive(false);
                        settingsDimmed = true;
                        break;
                    case Type.worldDisplay:
                        settingsMenuObject.SetActive(false);
                        settingsDimmed = true;
                        break;
                    case Type.HUDDisplay:
                        break;
                }
            }
        }

        public void ToggleResize()
        {
            ResizeDisplay(size.isOn);
        }

        public void ResizeDisplay(bool on)
        {
            GripMeasure gm = GetComponent<GripMeasure>();
            if (gm != null)
            {
                size.isOn = gm.measureEnabled = on;
            }
        }

        public void TogglePin()
        {
            PinDisplay(pin.isOn);
        }

        public void PinDisplay(bool on)
        {


            if (on)
            {

            }

            else
            {

            }

        }

        public void ToggleScaling()
        {
            ScaleDisplay(scale.isOn);
        }

        public void ScaleDisplay(bool on)
        {
            ScaleObjectTransform sot = GetComponent<ScaleObjectTransform>();
            if (sot != null)
            {
                scale.isOn = sot.scalingEnabled = on;
            }
        }

        public void ToggleMove()
        {
            MoveDisplay(move.isOn);
        }

        public void MoveDisplay(bool on)
        {

            // TODO: Maybe slide the display with the mouse and stop doing so on click
            /*if (on)
            {
                rPoint.selectionButton = VRTK_ControllerEvents.ButtonAlias.GripPress;
                lPoint.selectionButton = VRTK_ControllerEvents.ButtonAlias.GripPress;

                rPoint.interactWithObjects = true;
                rPoint.grabToPointerTip = true;
                lPoint.interactWithObjects = true;
                lPoint.grabToPointerTip = true;
                rUI.enabled = false;
                lUI.enabled = false;
            }
            else
            {
                rPoint.selectionButton = VRTK_ControllerEvents.ButtonAlias.TriggerPress;
                lPoint.selectionButton = VRTK_ControllerEvents.ButtonAlias.TriggerPress;

                rPoint.interactWithObjects = false;
                rPoint.grabToPointerTip = false;
                lPoint.interactWithObjects = false;
                lPoint.grabToPointerTip = false;
                rUI.enabled = true;
                lUI.enabled = true;
            }*/

        }

        public void SnapToPlane()
        {
            switch (type)
            {
                case Type.miniDisplay:
                    transform.localRotation = Quaternion.identity;
                    transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 0);
                    break;
                case Type.worldDisplay:
                    break;
                case Type.HUDDisplay:
                    break;
                default:
                    break;
            }
        }

        public bool feedSettingsDimmed = true;

        public void SelectFeed()
        {
            if (feedSettingsDimmed)
            {
                switch (type)
                {
                    case Type.miniDisplay:
                        miniFeedSettings.SetActive(true);
                        feedSettingsDimmed = false;
                        break;
                    case Type.worldDisplay:
                        feedSettings.SetActive(true);
                        feedSettingsDimmed = false;
                        break;
                    case Type.HUDDisplay:
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (type)
                {
                    case Type.miniDisplay:
                        miniFeedSettings.SetActive(false);
                        feedSettingsDimmed = true;
                        break;
                    case Type.worldDisplay:
                        feedSettings.SetActive(false);
                        feedSettingsDimmed = true;
                        break;
                    case Type.HUDDisplay:
                        break;
                    default:
                        break;
                }
            }
        }

        public void DimDisplay()
        {
            switch (type)
            {
                case Type.miniDisplay:
                    HudMenu hudMenu = this.GetComponentInParent<HudMenu>();
                    hudMenu.localDisplayList.Remove(gameObject);
                    hudMenu.SaveHud();
                    Destroy(gameObject);
                    break;
                case Type.worldDisplay:
                    gameObject.SetActive(false);
                    break;
                case Type.HUDDisplay:

                    break;
                default:
                    break;
            }

        }

    }
}