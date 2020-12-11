using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent (typeof(VRTK.VRTK_MoveInPlace))]

public class MoveInPlaceValueSetter : MonoBehaviour
{
    public ControllerData data;

    private static MoveInPlaceValueSetter instance;

    public static void UpdateSettings()
    {
        VRTK.VRTK_MoveInPlace moveInPlace = instance.GetComponent<VRTK.VRTK_MoveInPlace>();
        moveInPlace.speedScale = instance.data.moveInPlaceSpeedScale;
        bool origState = moveInPlace.enabled;
        moveInPlace.enabled = false;
        moveInPlace.enabled = true;
        moveInPlace.enabled = origState;
    }

    private void Start()
    {
        instance = this;
        VRTK.VRTK_MoveInPlace moveInPlace = GetComponent<VRTK.VRTK_MoveInPlace>();
        moveInPlace.speedScale = data.moveInPlaceSpeedScale;
    }

}