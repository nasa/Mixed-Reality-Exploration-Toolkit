using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridLines : MonoBehaviour
{
    public GameObject cube;

    public void Start()
    {
        LineRenderer lineRenderer = cube.AddComponent<LineRenderer>();
        for(int c = 0; c < 4; c++)
        {
            lineRenderer.SetPosition(c, new Vector3(1, 1, 1));
        }
    }
}
