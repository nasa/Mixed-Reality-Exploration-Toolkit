using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "pathvisulization points", order = 1)]

public class PathVisualizationData : ScriptableObject
{
    // scriptable object that holds a Vector3 array of positions to be drawn
    public Vector3[] positions;
}
