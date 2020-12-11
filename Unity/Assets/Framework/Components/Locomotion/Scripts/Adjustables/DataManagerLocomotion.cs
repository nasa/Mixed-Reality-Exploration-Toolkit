using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManagerLocomotion : MonoBehaviour
{
    public static DataManagerLocomotion instance;

    public ControllerData data;

    public float armswingSpeedScale;
    public float touchpadSpeedScale;
    public float laserDistance;
    public float flySpeedScale;

    private void Start()
    {
        instance = this;
    }

    public void UpdateScriptableObject()
    {
        data.moveInPlaceSpeedScale = armswingSpeedScale;
        data.touchpadSpeedScale = touchpadSpeedScale;
        data.laserDistance = laserDistance;
        data.flySpeedScale = flySpeedScale;

        LaserDistanceSetter.UpdateSettings();
        FlySpeedScaleSetter.UpdateSettings();
        TouchpadSpeedScale.UpdateSettings();
        MoveInPlaceValueSetter.UpdateSettings();
    }

}
