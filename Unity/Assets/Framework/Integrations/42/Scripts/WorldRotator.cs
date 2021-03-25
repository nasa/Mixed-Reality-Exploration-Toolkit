// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;

public class WorldRotator : MonoBehaviour
{
    public GameObject worldToRotate;
    public float rotationSpeed = -0.1f;

    float elapsed = 0;
	void Update()
    {
        elapsed += Time.deltaTime;
		if (elapsed > 0.05f)
        {
            elapsed = elapsed % 0.05f;
            worldToRotate.transform.Rotate(new Vector3(0, rotationSpeed / 30, -rotationSpeed));
        }
	}
}