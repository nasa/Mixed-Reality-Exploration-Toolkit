using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Data", menuName = "Locomotion Configuration Settings")]

public class ControllerData : ScriptableObject
{
    public float moveInPlaceSpeedScale;
    public float touchpadSpeedScale;
    public float flySpeedScale;
    public float laserDistance;

    /*
    [SerializeField] private float armswingSpeedScale;
    [SerializeField] private float touchpadSpeedScale;
    [SerializeField] private float laserDistance;
    [SerializeField] private float flySpeedScale;

    public float ArmswingSpeedScale => armswingSpeedScale;
    public float TouchpadSpeedScale => touchpadSpeedScale;
    public float LaserDistance => laserDistance;
    public float FlySpeedScale => flySpeedScale;


    public void SetArmswingSpeedScale(float sc)
    {
        armswingSpeedScale = sc;
    }

    public void SetTouchpadSpeedScale(float sc)
    {
        touchpadSpeedScale = sc;
    }

    public void SetLaserDistance(float sc)
    {
        laserDistance = sc;
    }

    public void SetFlySpeedScale(float sc)
    {
        flySpeedScale = sc;
    
    */
}
