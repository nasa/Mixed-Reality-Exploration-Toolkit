// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GSFC.ARVR.MRET.Infrastructure.Framework;

public class FrameHUD : MonoBehaviour
{

    public Transform trackHeadset;
    public float speed = 100;
    public float threshold = 25;
    public Vector3 Offset = new Vector3(0, 0, 0.5f);
    private bool zeroed = false;

    void LateUpdate()
    {
        ZeroHUD();
    }

    public void ZeroHUD()
    {
        float step = speed * Time.deltaTime;
        float Angle = Mathf.Abs(Quaternion.Angle(trackHeadset.rotation, transform.rotation));

        MRET.DataManager.SaveValue("MRET.Internal.Angle", Angle);

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
