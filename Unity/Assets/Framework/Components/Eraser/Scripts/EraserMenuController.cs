using UnityEngine;
using UnityEngine.UI;

public class EraserMenuController : MonoBehaviour
{
    public Toggle eraserToggle;
    public EraserManager eraserManager;
    public ControlMode controlMode;

    private bool initializing = false;

    public void OnEnable()
    {
        initializing = true;
        eraserToggle.isOn = eraserManager.canErase;
        initializing = false;
    }

    public void ToggleEraser()
    {
        if (!initializing)
        {
            SwitchEraser(!eraserManager.canErase);
        }
    }

    public void SwitchEraser(bool on)
    {
        if (on)
        {
            eraserManager.Enable();
            controlMode.EnterEraserMode();
        }
        else
        {
            eraserManager.Disable();
            if (controlMode.activeControlType == ControlMode.ControlType.Eraser)
            {
                controlMode.DisableAllControlTypes();
            }
        }
    }

    public void ExitMode()
    {
        if (eraserToggle)
        {
            eraserToggle.isOn = false;
            eraserManager.Disable();
        }
    }
}