﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRTK;
using GSFC.ARVR.MRET.Common.Schemas;

public class FrameHUD : MonoBehaviour
{

    public Transform trackHeadset;
    public float speed = 100;
    public float threshold = 25;
    public Vector3 Offset = new Vector3(0, 0, 0.5f);
    private bool zeroed = false;

    private DataManager dataManager;

    void Start()
    {
        Offset = new Vector3(0, 0, 0.5f);
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

    void LateUpdate()
    {
        ZeroHUD();
    }

    public void ZeroHUD()
    {
        float step = speed * Time.deltaTime;
        float Angle = Mathf.Abs(Quaternion.Angle(trackHeadset.rotation, transform.rotation));

        if (dataManager)
        {
            dataManager.SaveValue("MRET.Internal.Angle", Angle);
        }

        if (Angle > threshold || !zeroed)
        {
            zeroed = false;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, trackHeadset.rotation, step);
            transform.position = trackHeadset.position;
            transform.Translate(Offset);
            if (Angle < 5)
            {
                zeroed = true;
            }
        }
    }
}
