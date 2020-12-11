using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof(DataManagerLocomotion))] 

public class DataManagerLocomotionEditor : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Apply"))
        {
            DataManagerLocomotion dm = (DataManagerLocomotion)target;
            dm.UpdateScriptableObject();
        }
    }

}
