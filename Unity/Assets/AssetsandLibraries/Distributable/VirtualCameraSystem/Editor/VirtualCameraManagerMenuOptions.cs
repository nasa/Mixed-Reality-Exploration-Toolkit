using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GOV.NASA.GSFC.XR.VirtualCamera
{
    /// <remarks>
    /// History:
    /// June 2021: V1 Added into MRET
    /// </remarks>
    /// <summary>
    /// VirtualCameraManagerMenuOptioons is a class that provides
    /// top-level editor control of The Virtual Camera System in MRET.
    /// Author: Jonathan T. Reynolds
    /// </summary>
    public class VirtualCameraManagerMenuOptions : EditorWindow
    {

        public GameObject VirtualCameraManagerPrefab;

        VirtualCameraManager camManagerScript;

        GUIStyle titleStyle = new GUIStyle();
        GUIStyle subTitleStyle = new GUIStyle();

        //Required Tags
        const string VCM = "VirtualCameraManager";
        const string BVC = "BaseVirtualCamera";
        const string BVD = "BaseVirtualDisplay";

        bool VCM_found = false;
        bool BVC_found = false;
        bool BVD_found = false;

        Vector3 spawnpoint;

        //TODO: I assume this will be needed to be added to a MRET menu. 
        /// <summary>
        /// This is where in the toolbar where the virtual Camera manager is held.
        /// Change this for the menu option.
        /// </summary>
        [MenuItem("VirtualCameraManager/ShowWizard")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow<VirtualCameraManagerMenuOptions>("VCM Wizard");
        }

        /// <summary>
        /// this is the Menu itself and the GUI representation of that
        /// </summary>
        private void OnGUI()
        {



            GameObject VirtualCamManagerGO = GameObject.Find("VirtualCameraManager");


            if (VirtualCamManagerGO == null)
            {
                EditorGUILayout.HelpBox("No Virtual Camera Manager Found", MessageType.Error);
                EditorGUILayout.LabelField("Press button below to add Manager to scene");
                if (GUILayout.Button("Add Virtual Camera Manager"))
                {
                    GameObject VCM = (GameObject)PrefabUtility.InstantiatePrefab(VirtualCameraManagerPrefab);
                    VCM.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
                    VirtualCameraManager VCMScript = VCM.GetComponent<VirtualCameraManager>();
                    VCMScript.RigHMDCamera = GameObject.FindGameObjectWithTag("MainCamera");
                    VCMScript.AddPresetCamera(VCMScript.RigHMDCamera);
                }
            }
            else
            {

                camManagerScript = VirtualCamManagerGO.GetComponent<VirtualCameraManager>();
                VirtualCameraUISetup();

                //Selected Object options
                SelectionOptions();

            }

            if (!VCM_found || !BVD_found || !BVC_found)
            {
                Object asset = AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset");
                if (asset != null)
                {
                    SerializedObject so = new SerializedObject(asset);
                    SerializedProperty tags = so.FindProperty("tags");

                    int numTags = tags.arraySize;

                    for (int i = 0; i < numTags; i++)
                    {
                        string existingTag = tags.GetArrayElementAtIndex(i).stringValue;
                        switch (existingTag)
                        {
                            case VCM:
                                VCM_found = true;
                                break;
                            case BVC:
                                BVC_found = true;
                                break;
                            case BVD:
                                BVD_found = true;
                                break;
                            default:
                                break;
                        }
                    }

                    string message = "Tags not found: ";
                    message += !VCM_found ? VCM : "";
                    if (!VCM_found) message +=", ";
                    message += !BVC_found ? BVC : "";
                    if (!VCM_found || !BVC_found) message += ", ";
                    message += !BVD_found ? BVD : "";

                    EditorGUILayout.HelpBox(message, MessageType.Error);
                    if (GUILayout.Button("Fix"))
                    {
                        if (!VCM_found)
                        {
                            UpdateTags(VCM, tags, so, numTags);
                        }

                        if (!BVC_found)
                        {
                            UpdateTags(BVC, tags, so, numTags);
                        }

                        if (!BVD_found)
                        {
                            UpdateTags(BVD, tags, so, numTags);
                        }
                    }
                }




            }


            
            
        }


        /// <summary>
        /// This it responsible for updating the Seralized tags menu
        /// </summary>
        /// <param name="valueToAdd"></param>
        /// <param name="tags"></param>
        /// <param name="so"></param>
        /// <param name="numTags"></param>
        void UpdateTags(string valueToAdd, SerializedProperty tags, SerializedObject so, int numTags)
        {
            tags.InsertArrayElementAtIndex(numTags);
            tags.GetArrayElementAtIndex(numTags).stringValue = valueToAdd;
            so.ApplyModifiedProperties();
            so.Update();
        }


        /// <summary>
        /// This is the UI setup for the VirtualCamera UI
        /// </summary>
        void VirtualCameraUISetup()
        {
            EditorGUILayout.LabelField("Preset Virtual Manager Options", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Preset Camera"))
            {
                spawnpoint = SceneView.lastActiveSceneView.camera.transform.TransformPoint(Vector3.forward * VirtualCameraUtilities.onCamSpawnDistance);
                camManagerScript.AddPresetCamera(spawnpoint);
            }
            //VirtualCameraUtilities.newFeedRenderTextureSize = EditorGUILayout.Vector2Field("Display Dimentions", VirtualCameraUtilities.newFeedRenderTextureSize);
            if (GUILayout.Button("Add Preset Display"))
            {
                spawnpoint = SceneView.lastActiveSceneView.camera.transform.TransformPoint(Vector3.forward * VirtualCameraUtilities.onCamSpawnDistance * 3);
                camManagerScript.AddPresetDisplay(spawnpoint);
            }
            EditorGUILayout.EndHorizontal();

        }

        /// <summary>
        /// These are the options for selections, single selection and multi selection
        /// </summary>
        void SelectionOptions()
        {

            GameObject[] selections = UnityEditor.Selection.gameObjects;
            if (selections.Length <= 1)
            {
                EditorGUILayout.Separator();
                EditorGUILayout.BeginVertical(EditorStyles.toolbar);
                bool obj_selected = SingleSelection();
                EditorGUILayout.EndVertical();
                if (obj_selected == false)
                {
                    EditorGUILayout.HelpBox("Select Scene GameObject to see additional options", MessageType.Warning);
                }
            }
            else if (selections.Length > 1)
            {
                EditorGUILayout.Separator();
                EditorGUILayout.BeginVertical(EditorStyles.toolbar);
                EditorGUILayout.LabelField("Multi Selection Options");
                MultiSelection(selections);
                EditorGUILayout.EndVertical();
            }

        }

        /// <summary>
        /// Handles the editor UI elements on a single selection
        /// </summary>
        /// <returns></returns>
        bool SingleSelection()
        {
            GameObject SelectedObject = UnityEditor.Selection.activeGameObject;
            if (SelectedObject != null)
            {
                EditorGUILayout.LabelField("Selected Object Options", EditorStyles.boldLabel);
                if (SelectedObject.tag != "VirtualCameraManager")
                {

                    if (SelectedObject.tag == "BaseVirtualCamera")
                    {
                        SelectedCameraOptions(SelectedObject);
                    }
                    else if (SelectedObject.tag == "BaseVirtualDisplay")
                    {
                        SelectedDisplayOptions(SelectedObject);
                    }
                    else
                    {
                        EditorGUILayout.LabelField("Non-Camera Object Selected"); //Object Selected but it's not a baseCamera
                        SelectedNonCameraOptions(SelectedObject);
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("VirtualCameraManager Selected", EditorStyles.miniBoldLabel);
                }
                EditorGUILayout.HelpBox("To link up Cameras to displays, select one camera and any number of displays.", MessageType.Info);
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// handles the UI elements for the multi selection
        /// This will grab the first camera found and add all selected BaseVirtualDisplays to it's output feed
        /// </summary>
        /// <param name="selections"></param>
        void MultiSelection(GameObject[] selections)
        {
            
            BaseVirtualCamera coreCam = null;
            List<BaseVirtualDisplay> selectedDisplays = new List<BaseVirtualDisplay>();
            bool moreThanOneCamFound = false;


            
            foreach(GameObject go in selections)
            {
                if(go.tag == BVC && coreCam == null)
                {
                    coreCam = go.GetComponent<BaseVirtualCamera>();
                }
                else if(go.tag == BVD)
                {
                    selectedDisplays.Add(go.GetComponent<BaseVirtualDisplay>());
                }
                else if(go.tag == BVC && coreCam != null)
                {
                    moreThanOneCamFound = true;
                    break;
                }
            }


            if(moreThanOneCamFound)
            {
                EditorGUILayout.Separator();
                EditorGUILayout.HelpBox("More Than one Camera selected. Must select only one camera for linking", MessageType.Warning);
                return;
            }

            if(coreCam == null)
            {
                EditorGUILayout.Separator();
                EditorGUILayout.HelpBox("No camera found in multi selection, must select one camera for linking", MessageType.Warning);
                return;
            }

            if(selectedDisplays.Count == 0)
            {
                EditorGUILayout.Separator();
                EditorGUILayout.HelpBox("Multi Selection includes no displays, select at least one display for linking", MessageType.Warning);
                return;
            }

            CamDisplayLink searchCam = camManagerScript.GetCamDisplayLink(coreCam);
            List<BaseVirtualDisplay> newDisplaysToAdd = new List<BaseVirtualDisplay>();
            if (searchCam != null)
            {
                foreach(var display in selectedDisplays)
                {
                    if(searchCam.FeedOutputs.Find((x) => x == display) == null)
                    {
                        newDisplaysToAdd.Add(display);
                    }
                }


                if(newDisplaysToAdd.Count == 0)
                {
                    EditorGUILayout.HelpBox("No New Displays selected to Add", MessageType.Info);
                }
                else
                {
                    if(GUILayout.Button("Link Displays to Camera Feed"))
                    {
                        camManagerScript.AddToExistingCamDisplayLink(searchCam, newDisplaysToAdd, false);
                    }
                }


                if (GUILayout.Button("Remove Camera From Display Link"))
                {
                    camManagerScript.RemoveCamDisplayLink(searchCam);
                }

                if (searchCam.FeedOutputs.Count != 0)
                {
                    if (GUILayout.Button("Remove Displays From Camera Link"))
                    {
                        camManagerScript.RemoveDisplaysFromCamDisplayLink(searchCam);
                    }
                }

                

            }
            else
            {

                if (GUILayout.Button("Link Displays to Camera Feed"))
                {
                    camManagerScript.AddCamDisplayLink(coreCam, selectedDisplays, false);
                }
            }

            

        }


        /// <summary>
        /// This is the base options for the selected Camera
        /// </summary>
        /// <param name="SelectedObject"></param>
        void SelectedCameraOptions(GameObject SelectedObject)
        {
            BaseVirtualCamera bsv = SelectedObject.GetComponent<BaseVirtualCamera>();
            if (!bsv.CameraFeedHasRenderTexture())
            {
                VirtualCameraUtilities.newFeedRenderTextureSize = EditorGUILayout.Vector2IntField("Feed RenderTexture Dimentions",
                    VirtualCameraUtilities.newFeedRenderTextureSize);
                if (GUILayout.Button("Add and Set Feed RenderTexture"))
                {
                    bsv.SetFeedRenderTexture(VirtualCameraUtilities.newFeedRenderTextureSize);
                }
            }
            else
            {
                if (GUILayout.Button("Remove Feed Render Texture"))
                {
                    bsv.RemoveFeedRenderTexture();
                }
            }

            //Cam selected now we check if it's in list, if not give option to add it. 
            int indx_of_cam = camManagerScript.CheckGOInPresetCams(SelectedObject);
            if (indx_of_cam == -1)
            {
                EditorGUILayout.HelpBox("Current Select Camera Not found in Manager Preset List,\n select below if you want to add it.", MessageType.Warning);
                if (GUILayout.Button("Add Selected Camera to Preset List"))
                {
                    camManagerScript.AddPresetCamera(SelectedObject);
                }
            }



        }

        /// <summary>
        /// This is if a display is selected and what options are avaiable
        /// </summary>
        /// <param name="SelectedObject"></param>
        void SelectedDisplayOptions(GameObject SelectedObject)
        {
            BaseVirtualDisplay bsd = SelectedObject.GetComponent<BaseVirtualDisplay>();


            //Cam selected now we check if it's in list, if not give option to add it. 
            int indx_of_cam = camManagerScript.CheckGOInPresetDisplays(SelectedObject);
            if (indx_of_cam == -1)
            {
                EditorGUILayout.HelpBox("Current Select Display Not found in Manager Preset List,\n select below if you want to add it.", MessageType.Warning);
                if (GUILayout.Button("Add Selected Display to Preset List"))
                {
                    camManagerScript.AddPresetDisplay(SelectedObject);
                }
            }
        }


        /// <summary>
        /// This is if you select a non-Camera object and what to do with it
        /// </summary>
        /// <param name="SelectedObject"></param>
        void SelectedNonCameraOptions(GameObject SelectedObject)
        {
            int indx_of_cam = camManagerScript.CheckGOInPresetCams(SelectedObject);
            if (indx_of_cam == -1)
            {
                if (GUILayout.Button("Add Selected object as Record Point."))
                {
                    camManagerScript.AddPresetCamera(SelectedObject);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Object Selected is set as a record point.\n" +
                    "Select Below to remove it.", MessageType.Info);
                if (GUILayout.Button("Remove Selected Object"))
                {
                    camManagerScript.RemovePresetCamera(indx_of_cam);
                }
            }

        }
    }
}