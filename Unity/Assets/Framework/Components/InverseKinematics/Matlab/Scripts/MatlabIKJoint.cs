using UnityEngine;

public class MatlabIKJoint : MonoBehaviour
{
    public enum RotationAxis { X, Y, Z };
    
    public RotationAxis rotationAxis = RotationAxis.X;
    public bool inverted = false;

    public void ApplyJointAngle(float angle)
    {
        int negativeMultiplier = inverted ? -1 : 1;
        switch (rotationAxis)
        {
            case RotationAxis.X:
                transform.localRotation = Quaternion.Euler(negativeMultiplier * angle, 0, 0);
                break;

            case RotationAxis.Y:
                transform.localRotation = Quaternion.Euler(0, negativeMultiplier * angle, 0);
                break;

            case RotationAxis.Z:
                transform.localRotation = Quaternion.Euler(0, 0, negativeMultiplier * angle);
                break;

            default:
                break;
        }
    }

    public float GetJointAngle()
    {
        int negativeMultiplier = inverted ? -1 : 1;
        switch (rotationAxis)
        {
            case RotationAxis.X:
                return transform.localRotation.eulerAngles.x * negativeMultiplier;

            case RotationAxis.Y:
                return transform.localRotation.eulerAngles.y * negativeMultiplier;

            case RotationAxis.Z:
                return transform.localRotation.eulerAngles.z * negativeMultiplier;

            default:
                return transform.localRotation.eulerAngles.x * negativeMultiplier;
        }
    }
}