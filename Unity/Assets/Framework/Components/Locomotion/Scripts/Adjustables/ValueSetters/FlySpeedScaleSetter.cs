using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(SimpleMotionController))]

public class FlySpeedScaleSetter : MonoBehaviour
{
    public ControllerData data;

    private static FlySpeedScaleSetter instance;

    public static void UpdateSettings()
    {
        SimpleMotionController speedScale = instance.GetComponent<SimpleMotionController>();
        speedScale.stepMultiplier = instance.data.flySpeedScale;
    }

    private void Start()
    {
        instance = this;
        SimpleMotionController speedScale = GetComponent<SimpleMotionController>();
        speedScale.stepMultiplier = data.flySpeedScale;
    }
}