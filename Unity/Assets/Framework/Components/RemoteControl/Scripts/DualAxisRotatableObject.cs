using UnityEngine;

public class DualAxisRotatableObject : MonoBehaviour
{
    public enum RotationAxis { X, Y, Z };
    public enum ControlAttribute { Position, Rotation };

    public string objectName;
    public Transform horizontalObject, verticalObject;
    public RotationAxis horizontalObjectAxis, verticalObjectAxis;
    public ControlAttribute horizontalTransformItem = ControlAttribute.Rotation;
    public ControlAttribute verticalTransformItem = ControlAttribute.Rotation;
    public float motionSpeed = 0.39f;
    [Range(-180f, 180f)]
    public float minX = -180, maxX = 180, minY = -180, maxY = 180;
}