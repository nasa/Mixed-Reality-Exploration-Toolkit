using UnityEngine;

public class LocomotionMenuController : MonoBehaviour
{
    public GameObject teleportCheck, flyCheck, teleportX, flyX;
    public VRTK.VRTK_Pointer leftPointer, rightPointer;
    public VRTK.VRTK_BodyPhysics bodyPhysics;
    public SimpleMotionController leftMotionControl, rightMotionControl;

	public void OnEnable()
    {
        SwitchTeleport(leftPointer.enableTeleport);
        SwitchFlying(leftMotionControl.motionEnabled);
    }

    public void ToggleTeleport()
    {
        SwitchTeleport(!leftPointer.enableTeleport);
    }

    public void ToggleFlying()
    {
        SwitchFlying(!leftMotionControl.motionEnabled);
    }

    public void SwitchTeleport(bool on)
    {
        if (on)
        {
            teleportCheck.SetActive(true);
            teleportX.SetActive(false);
            leftPointer.enableTeleport = rightPointer.enableTeleport = true;
            ToggleBodyPhysics(false);
        }
        else
        {
            teleportCheck.SetActive(false);
            teleportX.SetActive(true);
            leftPointer.enableTeleport = rightPointer.enableTeleport = false;
            ToggleBodyPhysics(false);
        }
    }

    public void SwitchFlying(bool on)
    {
        if (on)
        {
            flyCheck.SetActive(true);
            flyX.SetActive(false);
            leftMotionControl.motionEnabled = rightMotionControl.motionEnabled = true;
            ToggleBodyPhysics(false);
        }
        else
        {
            flyCheck.SetActive(false);
            flyX.SetActive(true);
            leftMotionControl.motionEnabled = rightMotionControl.motionEnabled = false;
            ToggleBodyPhysics(false);
        }
    }

    private void ToggleBodyPhysics(bool on)
    {
        bodyPhysics.enabled = false;
        bodyPhysics.enabled = true;
        bodyPhysics.enableBodyCollisions = on;
    }
}