using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace GOV.NASA.GSFC.XR.VirtualCamera
{
    /// <remarks>
    /// History:
    /// June 2021: V1 Added into MRET
    /// </remarks>
    /// <summary>
    /// VirtualCameraManagerInspector is a class that modifies the Camera Manager on the 
    /// Inspector Window
    /// Author: Jonathan T. Reynolds
    /// </summary>
    [CustomEditor(typeof(VirtualCameraManager))]
    public class VirtualCameraManagerInspector : Editor
    {

        /// <summary>
        /// The local variable for the spawnpoint of new items which is just in front of the editor camera view
        /// </summary>
        Vector3 spawnpoint;

        /// <summary>
        /// This is the Gui modifications for the VirtualCameraManager Script
        /// <see cref="VirtualCameraManager"/> and it's variables for the base structure of the variable layout
        /// </summary>
        public override void OnInspectorGUI()
        {
            //This draws the base UI variables
            base.OnInspectorGUI();


            //Everything beyond this point is the additional GUI modifications on the script
            //======
            DrawUILine(Color.gray);

            VirtualCameraManager camManagerScript = (VirtualCameraManager)target;
            GUILayout.Label("Additional Quick Actions", EditorStyles.boldLabel);
            if (GUILayout.Button("Add Camera"))
            {
                spawnpoint = SceneView.lastActiveSceneView.camera.transform.TransformPoint(Vector3.forward * VirtualCameraUtilities.onCamSpawnDistance);
                camManagerScript.AddPresetCamera(spawnpoint);
            }

            
            if (GUILayout.Button("Add Preset Display"))
            {
                spawnpoint = SceneView.lastActiveSceneView.camera.transform.TransformPoint(Vector3.forward * VirtualCameraUtilities.onCamSpawnDistance);
                camManagerScript.AddPresetDisplay(spawnpoint);
            }
        }


        /// <summary>
        /// Quick helper function that draws a UI line.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="thickness"></param>
        /// <param name="padding"></param>
        public static void DrawUILine(Color color, int thickness = 2, int padding = 10)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            r.height = thickness;
            r.y += padding / 2;
            r.x -= 2;
            r.width += 6;
            EditorGUI.DrawRect(r, color);
        }


    }

}
