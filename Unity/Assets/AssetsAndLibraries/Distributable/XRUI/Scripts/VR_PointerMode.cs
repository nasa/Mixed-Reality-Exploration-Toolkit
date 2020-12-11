using UnityEngine;
using VRTK;

public class VR_PointerMode : MonoBehaviour
{
    public enum PointerMode { Environment, UI, Disabled };

    public Color uiPointerColor, environmentPointerColor;
    public bool teleportationEnabled = true;
    public bool touchpadEnabled = true;
    public PointerMode pointerMode
    {
        get
        {
            return currentMode;
        }
    }
    public bool isUIActive
    {
        get
        {
            return uiIsActive;
        }
    }

    private VRTK_ControllerEvents controllerEvents;
    private VRTK_UIPointer uiPointer;
    private VRTK_Pointer pointer;
    private VRTK_StraightPointerRenderer pointerRenderer;
    private PointerMode currentMode = PointerMode.Environment;
    private ControllerUIRaycastDetectionManager raycastDetector;
    private bool uiIsActive = false;

    void Start()
    {
        if (VRDesktopSwitcher.isVREnabled())
        {
            controllerEvents = GetComponent<VRTK_ControllerEvents>();
            uiPointer = GetComponent<VRTK_UIPointer>();
            pointer = GetComponent<VRTK_Pointer>();
            pointerRenderer = GetComponent<VRTK_StraightPointerRenderer>();
            raycastDetector = GetComponent<ControllerUIRaycastDetectionManager>();
            if (!raycastDetector)
            {
                Debug.LogWarning("[VR_PointerMode] Unable to find controller raycast detection component.");
            }
        }

        EnterEnvironmentMode();
    }

    void Update()
    {
        if (currentMode == PointerMode.Environment && raycastDetector)
        {
            if (raycastDetector.intersectionStatus)
            {
                if (!uiIsActive && raycastDetector.intersectingObject.GetComponentInParent<ZenFulcrum.EmbeddedBrowser.Browser>() == null)
                {
                    if (VRDesktopSwitcher.isVREnabled())
                    {
                        uiPointer.activationButton = VRTK_ControllerEvents.ButtonAlias.Undefined;
                        uiPointer.activationMode = VRTK_UIPointer.ActivationMethods.AlwaysOn;
                        uiPointer.selectionButton = VRTK_ControllerEvents.ButtonAlias.TriggerPress;
                        uiPointer.clickMethod = VRTK_UIPointer.ClickMethods.ClickOnButtonDown;

                        pointer.activationButton = VRTK_ControllerEvents.ButtonAlias.Undefined;
                        pointer.holdButtonToActivate = false;
                        pointer.selectionButton = VRTK_ControllerEvents.ButtonAlias.TriggerPress;
                        pointer.enableTeleport = false;

                        pointerRenderer.validCollisionColor = uiPointerColor;
                        pointerRenderer.cursorScaleMultiplier = 5;

                        pointer.Toggle(true);
                        uiPointer.enabled = true;
                    }

                    uiIsActive = true;
                }
            }
            else if (uiIsActive)
            {
                if (VRDesktopSwitcher.isVREnabled())
                {
                    uiPointer.activationButton = VRTK_ControllerEvents.ButtonAlias.TriggerPress;
                    uiPointer.activationMode = VRTK_UIPointer.ActivationMethods.HoldButton;
                    uiPointer.selectionButton = VRTK_ControllerEvents.ButtonAlias.TriggerPress;
                    uiPointer.clickMethod = VRTK_UIPointer.ClickMethods.ClickOnButtonUp;

                    pointer.activationButton = VRTK_ControllerEvents.ButtonAlias.TriggerPress;
                    pointer.holdButtonToActivate = true;
                    pointer.selectionButton = VRTK_ControllerEvents.ButtonAlias.TriggerPress;
                    pointer.enableTeleport = teleportationEnabled;

                    pointerRenderer.validCollisionColor = environmentPointerColor;
                    pointerRenderer.cursorScaleMultiplier = 40;

                    pointer.Toggle(false);
                    uiPointer.enabled = false;
                }

                uiIsActive = false;
            }
        }
    }

    public void SwitchMode(PointerMode mode)
    {
        switch (mode)
        {
            case PointerMode.Environment:
                EnterEnvironmentMode();
                break;

            case PointerMode.UI:
                EnterUIMode();
                break;

            case PointerMode.Disabled:
                EnterDisabledMode();
                break;

            default:
                Debug.LogWarning("[VR_PointerMode] Unidentified mode to switch to.");
                break;
        }
    }

    public void EnableTeleporation()
    {
        teleportationEnabled = true;

        if (currentMode == PointerMode.Environment)
        {
            if (VRDesktopSwitcher.isVREnabled())
            {
                pointer.enableTeleport = true;
            }
        }
    }

    public void DisableTeleportation()
    {
        teleportationEnabled = false;

        if (currentMode == PointerMode.Environment)
        {
            if (VRDesktopSwitcher.isVREnabled())
            {
                pointer.enableTeleport = false;
            }
        }
    }

    public void EnableTouchpad()
    {
        touchpadEnabled = true;

        if (currentMode == PointerMode.Environment)
        {
            if (VRDesktopSwitcher.isVREnabled())
            {
                
            }
        }

    }


    public void DisableTouchpad()
    {
        touchpadEnabled = false;

        if (currentMode == PointerMode.Environment)
        {
            if (VRDesktopSwitcher.isVREnabled())
            {
                pointer.enableTeleport = false;
            }
        }

    }

    private void EnterUIMode()
    {
        if (VRDesktopSwitcher.isVREnabled())
        {
            if (controllerEvents && uiPointer && pointer)
            {
                uiPointer.activationButton = VRTK_ControllerEvents.ButtonAlias.Undefined;
                uiPointer.activationMode = VRTK_UIPointer.ActivationMethods.AlwaysOn;
                uiPointer.selectionButton = VRTK_ControllerEvents.ButtonAlias.TriggerPress;
                uiPointer.clickMethod = VRTK_UIPointer.ClickMethods.ClickOnButtonDown;

                pointerRenderer.gameObject.SetActive(true);
                pointer.enabled = true;
                pointer.activationButton = VRTK_ControllerEvents.ButtonAlias.Undefined;
                pointer.holdButtonToActivate = false;
                pointer.selectionButton = VRTK_ControllerEvents.ButtonAlias.TriggerPress;
                pointer.enableTeleport = false;

                pointerRenderer.validCollisionColor = uiPointerColor;
                pointerRenderer.cursorScaleMultiplier = 5;
            }
            else
            {
                Debug.LogError("[VR_PointerMode->EnterUIMode] Not initialized.");
            }

            currentMode = PointerMode.UI;
            uiIsActive = true;
        }
    }

    private void EnterEnvironmentMode()
    {
        if (VRDesktopSwitcher.isVREnabled())
        {
            if (controllerEvents && uiPointer && pointer)
            {
                uiPointer.activationButton = VRTK_ControllerEvents.ButtonAlias.TriggerPress;
                uiPointer.activationMode = VRTK_UIPointer.ActivationMethods.HoldButton;
                uiPointer.selectionButton = VRTK_ControllerEvents.ButtonAlias.TriggerPress;
                uiPointer.clickMethod = VRTK_UIPointer.ClickMethods.ClickOnButtonUp;

                pointerRenderer.gameObject.SetActive(true);
                pointer.enabled = true;
                pointer.activationButton = VRTK_ControllerEvents.ButtonAlias.TriggerPress;
                pointer.holdButtonToActivate = true;
                pointer.selectionButton = VRTK_ControllerEvents.ButtonAlias.TriggerPress;
                pointer.enableTeleport = teleportationEnabled;

                pointerRenderer.validCollisionColor = environmentPointerColor;
                pointerRenderer.cursorScaleMultiplier = 40;
            }
            else
            {
                Debug.LogWarning("[VR_PointerMode->EnterEnvrionmentMode] Not yet initialized.");
            }

            currentMode = PointerMode.Environment;
            uiIsActive = false;
        }
    }

    private void EnterDisabledMode()
    {
        if (VRDesktopSwitcher.isVREnabled())
        {
            if (controllerEvents && uiPointer && pointer)
            {
                uiPointer.activationButton = VRTK_ControllerEvents.ButtonAlias.Undefined;
                uiPointer.selectionButton = VRTK_ControllerEvents.ButtonAlias.Undefined;

                pointer.activationButton = VRTK_ControllerEvents.ButtonAlias.Undefined;
                pointer.selectionButton = VRTK_ControllerEvents.ButtonAlias.Undefined;
                pointer.enableTeleport = teleportationEnabled;
                pointerRenderer.gameObject.SetActive(false);
                pointer.enabled = false;
            }
            else
            {
                Debug.LogWarning("[VR_PointerMode->EnterEnvrionmentMode] Not yet initialized.");
            }

            currentMode = PointerMode.Disabled;
            uiIsActive = false;
        }
    }
}