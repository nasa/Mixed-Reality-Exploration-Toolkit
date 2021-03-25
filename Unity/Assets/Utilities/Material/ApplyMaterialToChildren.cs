// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;

[ExecuteInEditMode]
public class ApplyMaterialToChildren : MonoBehaviour
{
    public Material materialToSet;

    public bool run = false;

    private void Update()
    {
        if (run)
        {
            ApplyMaterial();
            run = false;
        }
    }

    private void ApplyMaterial()
    {
        foreach (MeshRenderer rend in GetComponentsInChildren<MeshRenderer>())
        {
            rend.material = materialToSet;
        }
        run = false;
    }
}