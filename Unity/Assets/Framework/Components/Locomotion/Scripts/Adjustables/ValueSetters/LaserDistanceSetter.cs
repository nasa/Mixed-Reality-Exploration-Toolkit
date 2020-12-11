using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(VRTK.VRTK_StraightPointerRenderer))]

public class LaserDistanceSetter : MonoBehaviour
{

    public ControllerData data;

    private static LaserDistanceSetter instance;

    public static void UpdateSettings()
    {
        VRTK.VRTK_StraightPointerRenderer laserDistance = instance.GetComponent<VRTK.VRTK_StraightPointerRenderer>();
        laserDistance.maximumLength = instance.data.laserDistance;
        laserDistance.enabled = false;
        laserDistance.enabled = true;
    }

    private void Start()
    {
        instance = this;
        VRTK.VRTK_StraightPointerRenderer laserDistance = GetComponent<VRTK.VRTK_StraightPointerRenderer>();
        laserDistance.maximumLength = 100;// data.laserDistance;
    }

}