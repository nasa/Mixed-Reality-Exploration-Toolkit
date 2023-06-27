using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET;

/// <summary>
/// This is used for the Motion Constraints demo scene to enable controls for testing purposes.
/// </summary>
public class EnableAllControls : MonoBehaviour
{
    void Start()
    {
        MRET.ModeNavigator.EnableAllControls();
    }
}
