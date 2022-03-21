using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CableDebug))]
public class TempEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CableDebug myScript = (CableDebug)target;
        if (GUILayout.Button("Spawn Cable"))
        {
            myScript.SpawnCable(); //spawns MRET cable
        }
        if (GUILayout.Button("Spawn PointCloud"))
        {
            myScript.SpawnPointCloud();
        }
    }
}