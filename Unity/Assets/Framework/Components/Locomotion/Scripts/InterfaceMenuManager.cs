using UnityEngine;
using UnityEngine.UI;
using GSFC.ARVR.MRET.Common.Schemas;

public class InterfaceMenuManager : MonoBehaviour
{
    public Toggle teleport, fly, touchpad, armswing, rotateX, rotateY, rotateZ, scale;
    public VRTK.VRTK_Pointer leftPointer, rightPointer;
    public SimpleMotionController leftMotionControl, rightMotionControl;
    public RotateObjectTransform leftROT, rightROT;
    public ScaleObjectTransform leftSOT, rightSOT;
    public VR_PointerMode leftPointerMode, rightPointerMode;
    public VRTK.VRTK_TouchpadControl leftTouch, rightTouch;
    public VRTK.VRTK_MoveInPlace playArea;
    public VRTK.VRTK_BodyPhysics body;
    public VRTK.VRTK_HeightAdjustTeleport height;
    public VRTK.VRTK_BodyPhysics bodyPhysics;

    private DataManager dataManager;
    private bool fall = true;
    void Start()
    {
        GameObject loadedProjectObject = GameObject.Find("LoadedProject");
        if (loadedProjectObject)
        {
            UnityProject loadedProject = loadedProjectObject.GetComponent<UnityProject>();
            if (loadedProject)
            {
                dataManager = loadedProject.dataManager;
            }
        }
    }

    public void OnEnable()
    {
        if (leftPointerMode) SwitchTeleport(leftPointerMode.teleportationEnabled);
        if (leftMotionControl) SwitchFlying(leftMotionControl.motionEnabled);
        if (leftTouch) SwitchTouchpad(leftTouch.enabled);
        if (playArea) SwitchArmswing(playArea.enabled);
        if (leftSOT) SwitchScaling(leftSOT.enabled);

        if (leftROT && rightROT && leftSOT && rightSOT)
        {
            if (leftROT.xEnabled && rightROT.xEnabled)
            {
                rotateX.isOn = true;
            }
            else if (!leftROT.xEnabled && !rightROT.xEnabled)
            {
                rotateX.isOn = false;
            }

            if (leftROT.yEnabled && rightROT.yEnabled)
            {
                rotateY.isOn = true;
            }
            else if (!leftROT.yEnabled && !rightROT.yEnabled)
            {
                rotateY.isOn = false;
            }

            if (leftROT.zEnabled && rightROT.zEnabled)
            {
                rotateZ.isOn = true;
            }
            else if (!leftROT.zEnabled && !rightROT.zEnabled)
            {
                rotateZ.isOn = false;
            }
        }

        InitializeBodyPhysics();
    }

    public void ToggleTeleport()
    {
        SwitchTeleport(teleport.isOn);
    }

    public void ToggleFlying()
    {
        SwitchFlying(fly.isOn);
    }

    public void ToggleTouchpad()
    {
        SwitchTouchpad(touchpad.isOn);
    }

    public void ToggleArmswing()
    {
        SwitchArmswing(armswing.isOn);
    }

    public void ToggleXRotation()
    {
        bool rotx = rotateX.isOn;
        if (rotx)
        {
            leftROT.enabled = rightROT.enabled = true;
        }
        else
        {
            leftROT.enabled = rightROT.enabled = false;
        }
        if (leftROT) leftROT.xEnabled = rotx;
        if (rightROT) rightROT.xEnabled = rotx;
    }

    public void ToggleYRotation()
    {
        bool roty = rotateY.isOn;
        if (roty)
        {
            if (leftROT) leftROT.enabled = rightROT.enabled = true;
        }
        else
        {
            if (leftROT) leftROT.enabled = rightROT.enabled = false;
        }
        if (leftROT) leftROT.yEnabled = roty;
        if (rightROT) rightROT.yEnabled = roty;
    }

    public void ToggleZRotation()
    {
        bool rotz = rotateZ.isOn;
        if (rotz)
        {
            if (leftROT) leftROT.enabled = rightROT.enabled = true;
        }
        else
        {
            if (leftROT) leftROT.enabled = rightROT.enabled = false;
        }
        if (leftROT) leftROT.zEnabled = rotz;
        if (rightROT) rightROT.zEnabled = rotz;
    }

    public void ToggleScaling()
    {
        SwitchScaling(scale.isOn);
    }

    public void SwitchTeleport(bool on)
    {
        fall = true;
        teleport.isOn = on;
        if (on)
        {
            if (VRDesktopSwitcher.isVREnabled())
            {
                leftPointerMode.EnableTeleporation();
                rightPointerMode.EnableTeleporation();
                height.enabled = true;
                ToggleBodyPhysics();
            }
        }
        else
        {
            if (VRDesktopSwitcher.isVREnabled())
            {
                leftPointerMode.DisableTeleportation();
                rightPointerMode.DisableTeleportation();
                height.enabled = false;
                ToggleBodyPhysics();
            }
        }

        if (dataManager)
        {
            dataManager.SaveValue("MRET.Internal.Teleport", teleport.isOn);
        }
    }

    public void SwitchFlying(bool on)
    {
        fly.isOn = on;
        if (on)
        {
            leftMotionControl.motionEnabled = rightMotionControl.motionEnabled = true;
            if (fall)
            {
                if (VRDesktopSwitcher.isVREnabled())
                {
                    body.enabled = false;
                    fall = false;
                    ToggleBodyPhysics();
                }
            }
        }
        else
        {
            if (VRDesktopSwitcher.isVREnabled())
            {
                leftMotionControl.motionEnabled = rightMotionControl.motionEnabled = false;
                fall = false;
                ToggleBodyPhysics();
            }
        }

        if (dataManager)
        {
            dataManager.SaveValue("MRET.Internal.Fly", fly.isOn);
        }

    }

    public void SwitchTouchpad(bool on)
    {
        touchpad.isOn = on;
        fall = true;
        if (on)
        {
            leftTouch.enabled = rightTouch.enabled = true;
            if (VRDesktopSwitcher.isVREnabled())
            {
                body.enabled = true;
                ToggleBodyPhysics();
            }
        }
        else
        {
            leftTouch.enabled = rightTouch.enabled = false;
            if (VRDesktopSwitcher.isVREnabled())
            {
                ToggleBodyPhysics();
            }
        }

        if (dataManager)
        {
            dataManager.SaveValue("MRET.Internal.Touchpad", touchpad.isOn);
        }
    }
   
    public void SwitchArmswing(bool on)
    {
        fall = true;
        armswing.isOn = on;
        if (on)
        {
            playArea.enabled = true;
            body.enabled = true;
            ToggleBodyPhysics();
        }
        else
        {
            playArea.enabled = false;
            ToggleBodyPhysics();
        }

        if (dataManager)
        {
            dataManager.SaveValue("MRET.Internal.Armswing", fly.isOn);
        }
    }

    public void SwitchScaling(bool on)
    {
        scale.isOn = on;
        if (on)
        {
            if (leftSOT) leftSOT.enabled = rightSOT.enabled = true;
        }
        else
        {
            if (leftSOT) leftSOT.enabled = rightSOT.enabled = false;
        }
    }

    // TODO: This is a temporary solution. Locomotion state management needs to be improved, but
    // this will at least control when gravity is on/off.
    private void ToggleBodyPhysics()
    {
        if (VRDesktopSwitcher.isVREnabled())
        {
            bodyPhysics.enabled = true;
            bodyPhysics.enableBodyCollisions = armswing.isOn;
            bodyPhysics.enabled = false;
        }
    }

    private bool needToEnable = true;
    private int enableCycles = 0;
    void Update()
    {
        if (VRDesktopSwitcher.isVREnabled())
        {
            if (enableCycles++ > 8)
            {
                needToEnable = false;
                bodyPhysics.enabled = true;
                enableCycles = 0;
            }
        }
    }

    private void InitializeBodyPhysics()
    {
        //ToggleBodyPhysics(armswing.isOn);
    }
}