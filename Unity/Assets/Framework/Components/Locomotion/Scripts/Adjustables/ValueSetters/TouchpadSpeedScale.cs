using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(VRTK.VRTK_SlideObjectControlAction))]

public class TouchpadSpeedScale : MonoBehaviour
{
    public ControllerData data;

    private static TouchpadSpeedScale instance;

    public static void UpdateSettings()
    {
        VRTK.VRTK_SlideObjectControlAction speedMultiplier = instance.GetComponent<VRTK.VRTK_SlideObjectControlAction>();
        speedMultiplier.speedMultiplier = instance.data.touchpadSpeedScale;
        speedMultiplier.enabled = false;
        speedMultiplier.enabled = true;
    }

    private void Start()
    {
        instance = this;
        VRTK.VRTK_SlideObjectControlAction speedMultiplier = GetComponent<VRTK.VRTK_SlideObjectControlAction>();
        speedMultiplier.speedMultiplier = data.touchpadSpeedScale;
    }
}