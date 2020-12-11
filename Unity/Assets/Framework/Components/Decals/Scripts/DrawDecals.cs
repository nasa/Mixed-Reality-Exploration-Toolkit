using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class to draw decals on a surface -- e.g. used for footprints on the moon.
/// </summary>
public class DrawDecals : MonoBehaviour
{
    [SerializeField]
    private GameObject leftDecalPrefab;
    [SerializeField]
    private GameObject rightDecalPrefab;
    private Vector3 initHitPos;
    private Quaternion initHitRot;
    private int interval = 1;
    private float nextTime = 0;
    private bool walking = false;

    enum SideOfBody
    {
        left,
        right
    }
    private SideOfBody foot;

    /// <summary>
    /// Spawns a decal on the surface directly below the HMD -- i.e. at the "hit".
    /// </summary>
    /// <param name="hit">This is the hit point for where to place the decal.</param>
    /// <param name="upDir">This is the up-direction that the decal should have.</param>
    private void SpawnDecal(RaycastHit hit, Vector3 upDir)
    {
        // Alternate footprints for each decal generated.
        if(foot == SideOfBody.left)
        {
            var leftDecal = Instantiate(leftDecalPrefab);
            leftDecal.transform.position = hit.point;
            leftDecal.transform.rotation = Quaternion.LookRotation(-hit.normal, upDir);
            foot = SideOfBody.right;
        }
        else
        {
            var rightDecal = Instantiate(rightDecalPrefab);
            rightDecal.transform.position = hit.point;
            rightDecal.transform.rotation = Quaternion.LookRotation(-hit.normal, upDir);
            foot = SideOfBody.left;
        }
    }

    /// <summary>
    /// Called from the VR GUI to start dropping footprints.
    /// </summary>
    public void startWalking()
    {
        walking = true;
    }

    /// <summary>
    /// Called from the VR GUI to stop dropping footprints.
    /// </summary>
    public void stopWalking()
    {
        walking = false;

    }

    /// <summary>
    /// This routine is called from MRET VR GUI to drop decals on a surface.
    /// </summary>
    public void updateTerrain()
    {
        Transform headSetTrans = VRTK.VRTK_DeviceFinder.DeviceTransform(
            VRTK.VRTK_DeviceFinder.Devices.Headset);
        var position1 = headSetTrans.position;
        var forwardDir = headSetTrans.forward;
        RaycastHit hit;
        Vector3 dir;
        dir = new Vector3(0f, -1f, 0f);
        if (Physics.Raycast(position1, dir, out hit))
        {
            initHitPos = hit.transform.position;
            initHitRot = hit.transform.rotation;
            Vector3 decalNormal = hit.normal * -1f;
            Vector3 forwardProjected = Vector3.ProjectOnPlane(forwardDir, decalNormal);
            SpawnDecal(hit, forwardProjected);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        foot = SideOfBody.left;
    }

    // Update is called once per frame
    void Update()
    {
        if (walking && Time.time >= nextTime )
        {
            Transform headSetTrans = VRTK.VRTK_DeviceFinder.DeviceTransform(
            VRTK.VRTK_DeviceFinder.Devices.Headset);
            var curPos = headSetTrans.position;
            var curRot = headSetTrans.rotation;
            if (Vector3.Distance(curPos, initHitPos) >= 1 || Quaternion.Angle(curRot, initHitRot) >= 45)
            {
                updateTerrain();
                initHitPos = curPos;
                initHitRot = curRot;
            }
            nextTime += interval;
        }
    }
}
