// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GSFC.ARVR.MRET.Infrastructure.Framework;

// TODO: Consider updating to descend from the Telemetry class which would mean that
// the DataManager keys would have to be combined into a single "transform" key that produces a
// single object value (Vector3 or Quaternion) depending on the transform. All other fields would
// still be relevant.
public class TelemetryTransform : MonoBehaviour
{
    public enum TransformAttribute { None, GlobalPosition, LocalPosition, GlobalRotation, LocalRotation, Scale };

    [Tooltip("The object to control the transform of.")]
    public GameObject objectToControl;

    [Tooltip("True if the transform values are relative to another object (does not apply to local attributes).")]
    public bool valuesRelativeToObject = false;

    [Tooltip("Object that the transform values are relative to (does not apply to local attributes).")]
    public GameObject objectRelativeTo;

    [Tooltip("Does this control position, rotation, or scale?")]
    public TransformAttribute attributeToControl = TransformAttribute.None;

    [Tooltip("Data point name to use for the x-axis value.")]
    public string xPointName;

    [Tooltip("Whether or not to change the x-axis value with telemetry.")]
    public bool useXPointValue = true;

    [Tooltip("Whether or not to invert the x-axis value.")]
    public bool invertXPointValue = false;

    [Tooltip("Offset to apply to the x-axis value (for rotation, in degrees). If telemetry is not used, the x-axis value will be set to this.")]
    public float xPointOffset = 0;

    [Tooltip("Data point name to use for the y-axis value.")]
    public string yPointName;

    [Tooltip("Whether or not to change the y-axis value with telemetry.")]
    public bool useYPointValue = true;

    [Tooltip("Whether or not to invert the y-axis value.")]
    public bool invertYPointValue = false;

    [Tooltip("Offset to apply to the y-axis value (for rotation, in degrees). If telemetry is not used, the y-axis value will be set to this.")]
    public float yPointOffset = 0;

    [Tooltip("Data point name to use for the z-axis value.")]
    public string zPointName;

    [Tooltip("Whether or not to change the z-axis value with telemetry.")]
    public bool useZPointValue = true;

    [Tooltip("Whether or not to invert the z-axis value.")]
    public bool invertZPointValue = false;

    [Tooltip("Offset to apply to the z-axis value (for rotation, in degrees). If telemetry is not used, the z-axis value will be set to this.")]
    public float zPointOffset = 0;

    [Tooltip("Data point name to use for the w-axis value.")]
    public string wPointName;

    [Tooltip("Whether or not to change the w-axis value with telemetry.")]
    public bool useWPointValue = true;

    [Tooltip("Whether or not to invert the w-axis value.")]
    public bool invertWPointValue = false;

    [Tooltip("Offset to apply to the w-axis value (for rotation, in degrees). If telemetry is not used, the w-axis value will be set to this.")]
    public float wPointOffset = 0;

    [Tooltip("Set to true if the point values are in radians.")]
    public bool valuesAreInRadians = false;

    [Tooltip("Set to true if the rotational values are quaternions.")]
    public bool useQuaternions = false;

    [Tooltip("How often the transform should be updated. Lower number yields higher update rate.")]
    [Range(1, 64)]
    public int updateFrequency = 5;

    private int updateCounter = 0;

    void Update()
    {
        updateCounter++;
        if (updateCounter >= updateFrequency)
        {
            updateCounter = 0;
            object rawXPointVal = MRET.DataManager.FindPoint(xPointName);
            object rawYPointVal = MRET.DataManager.FindPoint(yPointName);
            object rawZPointVal = MRET.DataManager.FindPoint(zPointName);
            object rawWPointVal = MRET.DataManager.FindPoint(wPointName);
            Debug.Log("datamanager points: " + rawWPointVal + rawYPointVal+ rawZPointVal + rawWPointVal);

            float xPointVal = (float) ((rawXPointVal == null) ? 0f : rawXPointVal) * (invertXPointValue ? -1 : 1);
            float yPointVal = (float) ((rawYPointVal == null) ? 0f : rawYPointVal) * (invertYPointValue ? -1 : 1);
            float zPointVal = (float) ((rawZPointVal == null) ? 0f : rawZPointVal) * (invertZPointValue ? -1 : 1);
            float wPointVal = (float) ((rawWPointVal == null) ? 0f : rawWPointVal) * (invertWPointValue ? -1 : 1);

            float finalXPointValue = xPointOffset;
            float finalYPointValue = yPointOffset;
            float finalZPointValue = zPointOffset;
            float finalWPointValue = wPointOffset;
            if (objectToControl != null)
            {
                switch (attributeToControl)
                {
                    case TransformAttribute.LocalPosition:
                        if (useXPointValue)
                        {
                            finalXPointValue = xPointVal + xPointOffset;
                        }
                        if (useYPointValue)
                        {
                            finalYPointValue = yPointVal + yPointOffset;
                        }
                        if (useZPointValue)
                        {
                            finalZPointValue = zPointVal + zPointOffset;
                        }
                        objectToControl.transform.localPosition = new Vector3(finalXPointValue, finalYPointValue, finalZPointValue);
                        break;

                    case TransformAttribute.GlobalPosition:
                        if (useXPointValue)
                        {
                            finalXPointValue = xPointVal + xPointOffset;
                        }
                        if (useYPointValue)
                        {
                            finalYPointValue = yPointVal + yPointOffset;
                        }
                        if (useZPointValue)
                        {
                            finalZPointValue = zPointVal + zPointOffset;
                        }
                        if (valuesRelativeToObject && objectRelativeTo != null)
                        {
                            objectToControl.transform.position =
                                objectRelativeTo.transform.TransformPoint(finalXPointValue, finalYPointValue, finalZPointValue);
                        }
                        else
                        {
                            objectToControl.transform.position = new Vector3(finalXPointValue, finalYPointValue, finalZPointValue);
                        }
                        break;

                    case TransformAttribute.LocalRotation:
                        if (useXPointValue)
                        {
                            finalXPointValue = FormatAngleValue(xPointVal) + xPointOffset;
                        }
                        if (useYPointValue)
                        {
                            finalYPointValue = FormatAngleValue(yPointVal) + yPointOffset;
                        }
                        if (useZPointValue)
                        {
                            finalZPointValue = FormatAngleValue(zPointVal) + zPointOffset;
                        }
                        if (useWPointValue)
                        {
                            finalWPointValue = FormatAngleValue(wPointVal) + wPointOffset;
                        }
                        objectToControl.transform.localRotation = useQuaternions ?
                            new Quaternion(finalXPointValue, finalYPointValue, finalZPointValue, finalWPointValue) :
                            Quaternion.Euler(finalXPointValue, finalYPointValue, finalZPointValue);
                        break;

                    case TransformAttribute.GlobalRotation:
                        if (useXPointValue)
                        {
                            finalXPointValue = FormatAngleValue(xPointVal) + xPointOffset;
                        }
                        if (useYPointValue)
                        {
                            finalYPointValue = FormatAngleValue(yPointVal) + yPointOffset;
                        }
                        if (useZPointValue)
                        {
                            finalZPointValue = FormatAngleValue(zPointVal) + zPointOffset;
                        }
                        if (useWPointValue)
                        {
                            finalWPointValue = FormatAngleValue(wPointVal) + wPointOffset;
                        }
                        if (valuesRelativeToObject && objectRelativeTo != null)
                        {
                            Transform originalParent = objectToControl.transform.parent;
                            objectToControl.transform.SetParent(objectRelativeTo.transform);
                            objectToControl.transform.rotation = useQuaternions ?
                                new Quaternion(finalXPointValue, finalYPointValue, finalZPointValue, finalWPointValue) :
                                Quaternion.Euler(finalXPointValue, finalYPointValue, finalZPointValue);
                            objectToControl.transform.SetParent(originalParent);
                        }
                        else
                        {
                            objectToControl.transform.rotation = useQuaternions ?
                                new Quaternion(finalXPointValue, finalYPointValue, finalZPointValue, finalWPointValue) :
                                Quaternion.Euler(finalXPointValue, finalYPointValue, finalZPointValue);
                        }
                        break;

                    case TransformAttribute.Scale:
                        if (useXPointValue)
                        {
                            finalXPointValue = xPointVal + xPointOffset;
                        }
                        if (useYPointValue)
                        {
                            finalYPointValue = yPointVal + yPointOffset;
                        }
                        if (useZPointValue)
                        {
                            finalZPointValue = zPointVal + zPointOffset;
                        }
                        objectToControl.transform.localScale = new Vector3(finalXPointValue, finalYPointValue, finalZPointValue);
                        break;

                    case TransformAttribute.None:
                    default:
                        break;
                }
            }
        }
	}

#region HELPERS
    private float FormatAngleValue(float rawAngle)
    {
        if (valuesAreInRadians)
        {
            return Normalize(RadsToDegrees(rawAngle));
        }
        else
        {
            return Normalize(rawAngle);
        }
    }
    private float RadsToDegrees(float radAngle)
    {
        return radAngle * 180 / (float) System.Math.PI;
    }

    private float Normalize(float rawAngle)
    {
        if (rawAngle < 360 && rawAngle > 0)
        {
            return rawAngle;
        }
        else if (rawAngle > 360)
        {
            while (rawAngle > 360)
            {
                rawAngle -= 360;
            }
            return rawAngle;
        }
        else
        {
            while (rawAngle < 0)
            {
                rawAngle += 360;
            }
            return rawAngle;
        }
    }
#endregion
}