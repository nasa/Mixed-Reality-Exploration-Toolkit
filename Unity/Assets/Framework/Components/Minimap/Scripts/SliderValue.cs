using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SliderValue : MonoBehaviour
{
    public TextMesh text;
    public Camera camera;
    public float zoomValue = 15;
    public float zoomValueF;

    private void FixedUpdate()
    {
        zoomValueF = float.Parse(text.text);
        camera.orthographicSize = zoomValueF;
    }
}
