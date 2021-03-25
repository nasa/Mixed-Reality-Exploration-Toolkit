// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;

public class FOVVisualization : MonoBehaviour
{
    [Tooltip("Field-of-View angle in degrees.")]
    [Range(0, 180)]
    public float fovAngle;
    public float fovLength;

    // X, Y are base widths. Z is height.

	void Start()
    {
        // 2(FOVLength)(tan(theta/2))(0.5).
        float baseWidth = fovLength * Mathf.Tan(DegreesToRadians(fovAngle / 2));
        transform.localScale = new Vector3(baseWidth, baseWidth, fovLength / 2);
        transform.localPosition = new Vector3(transform.localPosition.x,
            transform.localPosition.y, transform.localPosition.z + fovLength / 4);
	}

#region HELPERS
    float DegreesToRadians(float valueInDegrees)
    {
        return valueInDegrees * Mathf.PI / 180;
    }
#endregion
}