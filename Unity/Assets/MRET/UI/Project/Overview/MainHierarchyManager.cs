// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using GOV.NASA.GSFC.XR.XRUI.Widget;
using GOV.NASA.GSFC.XR.MRET.Action;
using GOV.NASA.GSFC.XR.MRET.Project;
using GOV.NASA.GSFC.XR.MRET.SceneObjects;
using GOV.NASA.GSFC.XR.MRET.UI.MiniMap;
using GOV.NASA.GSFC.XR.MRET.UI.SceneObjects;

namespace GOV.NASA.GSFC.XR.MRET.UI.Project
{
    /// <remarks>
    /// History:
    /// August 10 2022: Updated code to work with the updated "Project Overview Panel".
    /// </remarks>
    /// <summary>
    /// MainHierarchyManager is a class that manages
    /// functionality of the Project Overview Panel, 
    /// which is presently located under the Objects
    /// Submenu in a user's Controller Menu.
    /// Author: Undocumented
    /// Summer 2022 Updates: Jordan A. Ritchey
    /// </summary>
    public class MainHierarchyManager : MRETUpdateBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(MainHierarchyManager);

        [Tooltip("Name to apply to the filter scroll menu.")]
        public string filterListTitle = "Filters";

        [Tooltip("Name to apply to the file scroll menu.")]
        public string fileListTitle = "Files";

        [Tooltip("Name to apply to the filter tile menu.")]
        public string fileTilesTitle = "Files";

        [Tooltip("File extensions to search for. Any if empty.")]
        public List<string> searchFileExtensions;

        [Tooltip("File extension to use for writing files.")]
        public string usedFileExtension = ".mret";

        [Tooltip("Whether or not to show files (in addition to directories).")]
        public bool showFiles = true;

        public ScrollListManager filterListDisplay, hierarchyListDisplay;
        public Texture2D defaultThumbnail;
        public Texture2D fileThumbnail;
        private ObjectPanelController objPanel;
        public GameObject headsetFollower;
        public GameObject minimapPanel;
        public GameObject filterPanel;

        private Transform[] objTransform;
        private List<GameObject> hierarchyList = new List<GameObject>(), displayedList = new List<GameObject>();
        private int displayedListCount, layerNum;
        private List<List<GameObject>> menuSequence = new List<List<GameObject>>();
        private List<List<string>> filterSequence = new List<List<string>>();
        private Dictionary<GameObject, List<GameObject>> GODict = new Dictionary<GameObject, List<GameObject>>();
        public Text userPositionLabel;
        public InputField filterInputField;
        private List<string> filterWords = new List<string>();
        private int currentSelection = -1, currentFilterSelection = -1, backCount = 0;

        private Material highlightMaterial;
        private MeshRenderer[] objectRenderers;
        private Material[] objectMaterials;
        private List<MeshRenderer[]> meshRendererList, objOriginalMesh;
        private List<Material> objOriginalMaterials;
        private GameObject selected;
        public Toggle physics, gravity;
        private IInteractable sceneObject;
        private float size;
        public Sprite mms, mmsUser;
        private bool minimapDisplay = false;
        private bool filterDisplay = false;
        private bool maximized = false;
        public MinimapController mmController;
        private Button selectedMmoBtn;

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            base.MRETStart();

            filterListDisplay.SetTitle("Filters");
            hierarchyListDisplay.SetTitle("Scene Objects");
            layerNum = 0;
            AddInteractableGO();
            PopulateSceneObjectLists(hierarchyList);
            menuSequence.Add(hierarchyList);
            filterSequence.Add(new List<string>());
            physics.isOn = false;
            gravity.isOn = false;
            //place minimap icon to mark user's location
            MinimapDrawnObject userMdo = MRET.InputRig.head.gameObject.AddComponent<MinimapDrawnObject>();
            userMdo.minimapSprite = mmsUser;
        }

        /// <seealso cref="MRETBehaviour.MRETUpdate"/>
        protected override void MRETUpdate()
        {
            base.MRETUpdate();

            Transform trans = Camera.main.transform;
            userPositionLabel.text = "My Position:  " + trans.position.ToString();
        }

        private void OnEnable()
        {
            PopulateSceneObjectLists(hierarchyList);
        }

        private void OnDisable()
        {
            resetColor(selected);
        }

        public void toggleMinimap()
        {
            //toggle minimap display on and off
            minimapDisplay = !minimapDisplay;
            if (minimapDisplay == true) {
                minimapPanel.SetActive(true);
            }
            else {
                minimapPanel.SetActive(false);
            }
        }

        public void toggleFilterPanel()
        {
            //toggle filter panel on and off
            filterDisplay = !filterDisplay;
            if (filterDisplay == true) {
                filterPanel.SetActive(true);
            }
            else {
                filterPanel.SetActive(false);
            }
        }

        public void toggleSize()
        {
            //toggle the maximize feature on and off
            maximized = !maximized;
            if(maximized == true) {
                //reset the transforms
                //MAIN PANEL
                RectTransform currentRect = gameObject.transform.Find("Canvas").GetComponent(typeof(RectTransform)) as RectTransform;
                currentRect.offsetMax = new Vector2(currentRect.offsetMax.x, currentRect.offsetMax.y + 500);
                currentRect = gameObject.transform.Find("Canvas").transform.Find("Content").transform.Find("LowerPanel").GetComponent(typeof(RectTransform)) as RectTransform;
                currentRect.localPosition = new Vector3(currentRect.localPosition.x, currentRect.localPosition.y - 500, currentRect.localPosition.z);
                currentRect = gameObject.transform.Find("Canvas").transform.Find("Content").transform.Find("MiddlePanel").transform.Find("ListView").transform.Find("ScrollArea").transform.Find("ScrollDown").GetComponent(typeof(RectTransform)) as RectTransform;
                currentRect.localPosition = new Vector3(currentRect.localPosition.x, currentRect.localPosition.y - 770, currentRect.localPosition.z);
                currentRect = gameObject.transform.Find("Canvas").transform.Find("Content").transform.Find("MiddlePanel").transform.Find("ListView").transform.Find("ScrollArea").transform.Find("Scroll View").GetComponent(typeof(RectTransform)) as RectTransform;
                currentRect.offsetMin = new Vector2(currentRect.offsetMin.x, currentRect.offsetMin.y - 770);
                currentRect = gameObject.transform.Find("Canvas").transform.Find("Content").transform.Find("Tabs").transform.Find("MinimapButton").GetComponent(typeof(RectTransform)) as RectTransform;
                currentRect.offsetMin = new Vector2(currentRect.offsetMin.x, currentRect.offsetMin.y - 500);
                currentRect = gameObject.transform.Find("Canvas").transform.Find("Content").transform.Find("Tabs").transform.Find("FilterButton").GetComponent(typeof(RectTransform)) as RectTransform;
                currentRect.offsetMin = new Vector2(currentRect.offsetMin.x, currentRect.offsetMin.y - 500);
                BoxCollider currentBox = gameObject.GetComponent(typeof(BoxCollider)) as BoxCollider;
                currentBox.size = new Vector3(currentBox.size.x, currentBox.size.y + 500, currentBox.size.z);
                currentBox.center = new Vector3(currentBox.center.x, currentBox.center.y + 250, currentBox.center.z);
                //FILTER PANEL
                currentRect = gameObject.transform.Find("FilterPanel").transform.Find("Canvas").GetComponent(typeof(RectTransform)) as RectTransform;
                currentRect.offsetMax = new Vector2(currentRect.offsetMax.x, currentRect.offsetMax.y + 500);
                currentRect.localPosition = new Vector3(currentRect.localPosition.x, currentRect.localPosition.y - 250, currentRect.localPosition.z);
                currentRect = gameObject.transform.Find("FilterPanel").GetComponent(typeof(RectTransform)) as RectTransform;
                currentRect.localPosition = new Vector3(currentRect.localPosition.x, currentRect.localPosition.y + 250, currentRect.localPosition.z);
                currentRect = gameObject.transform.Find("FilterPanel").transform.Find("Canvas").transform.Find("Content").transform.Find("LowerPanel").GetComponent(typeof(RectTransform)) as RectTransform;
                currentRect.localPosition = new Vector3(currentRect.localPosition.x, currentRect.localPosition.y - 500, currentRect.localPosition.z);
                currentRect = gameObject.transform.Find("FilterPanel").transform.Find("Canvas").transform.Find("Content").transform.Find("MiddlePanel").transform.Find("Filters").transform.Find("ScrollArea").transform.Find("ScrollDown").GetComponent(typeof(RectTransform)) as RectTransform;
                currentRect.localPosition = new Vector3(currentRect.localPosition.x, currentRect.localPosition.y - 770, currentRect.localPosition.z);
                currentRect = gameObject.transform.Find("FilterPanel").transform.Find("Canvas").transform.Find("Content").transform.Find("MiddlePanel").transform.Find("Filters").transform.Find("ScrollArea").transform.Find("Scroll View").GetComponent(typeof(RectTransform)) as RectTransform;
                currentRect.offsetMin = new Vector2(currentRect.offsetMin.x, currentRect.offsetMin.y - 770);
                currentBox = gameObject.transform.Find("FilterPanel").GetComponent(typeof(BoxCollider)) as BoxCollider;
                currentBox.size = new Vector3(currentBox.size.x, currentBox.size.y + 500, currentBox.size.z);
                //MINIMAP PANEL
                currentRect = gameObject.transform.Find("MinimapPanel").transform.Find("Canvas").GetComponent(typeof(RectTransform)) as RectTransform;
                currentRect.offsetMax = new Vector2(currentRect.offsetMax.x + 500, currentRect.offsetMax.y + 500);
                currentRect = gameObject.transform.Find("MinimapPanel").transform.Find("Canvas").transform.Find("Content").GetComponent(typeof(RectTransform)) as RectTransform;
                currentRect.localPosition = new Vector3(currentRect.localPosition.x + 250, currentRect.localPosition.y - 500, currentRect.localPosition.z);
                currentRect = gameObject.transform.Find("MinimapPanel").transform.Find("Canvas").transform.Find("Content").transform.Find("Canvas").GetComponent(typeof(RectTransform)) as RectTransform;
                currentRect.localPosition = new Vector3(currentRect.localPosition.x - 300, currentRect.localPosition.y + 250, currentRect.localPosition.z);
                currentRect.offsetMax = new Vector2(currentRect.offsetMax.x + 100, currentRect.offsetMax.y + 100);
                currentBox = gameObject.transform.Find("MinimapPanel").GetComponent(typeof(BoxCollider)) as BoxCollider;
                currentBox.size = new Vector3(currentBox.size.x + 500, currentBox.size.y + 500, currentBox.size.z);
                currentBox.center = new Vector3(currentBox.center.x + 250, currentBox.center.y + 250, currentBox.center.z);

            }
            else {
                //maximize the transforms
                //MAIN PANEL
                RectTransform currentRect = gameObject.transform.Find("Canvas").GetComponent(typeof(RectTransform)) as RectTransform;
                currentRect.offsetMax = new Vector2(currentRect.offsetMax.x, currentRect.offsetMax.y - 500);
                currentRect = gameObject.transform.Find("Canvas").transform.Find("Content").transform.Find("LowerPanel").GetComponent(typeof(RectTransform)) as RectTransform;
                currentRect.localPosition = new Vector3(currentRect.localPosition.x, currentRect.localPosition.y + 500, currentRect.localPosition.z);
                currentRect = gameObject.transform.Find("Canvas").transform.Find("Content").transform.Find("MiddlePanel").transform.Find("ListView").transform.Find("ScrollArea").transform.Find("ScrollDown").GetComponent(typeof(RectTransform)) as RectTransform;
                currentRect.localPosition = new Vector3(currentRect.localPosition.x, currentRect.localPosition.y + 770, currentRect.localPosition.z);
                currentRect = gameObject.transform.Find("Canvas").transform.Find("Content").transform.Find("MiddlePanel").transform.Find("ListView").transform.Find("ScrollArea").transform.Find("Scroll View").GetComponent(typeof(RectTransform)) as RectTransform;
                currentRect.offsetMin = new Vector2(currentRect.offsetMin.x, currentRect.offsetMin.y + 770);
                currentRect = gameObject.transform.Find("Canvas").transform.Find("Content").transform.Find("Tabs").transform.Find("MinimapButton").GetComponent(typeof(RectTransform)) as RectTransform;
                currentRect.offsetMin = new Vector2(currentRect.offsetMin.x, currentRect.offsetMin.y + 500);
                currentRect = gameObject.transform.Find("Canvas").transform.Find("Content").transform.Find("Tabs").transform.Find("FilterButton").GetComponent(typeof(RectTransform)) as RectTransform;
                currentRect.offsetMin = new Vector2(currentRect.offsetMin.x, currentRect.offsetMin.y + 500);
                BoxCollider currentBox = gameObject.GetComponent(typeof(BoxCollider)) as BoxCollider;
                currentBox.size = new Vector3(currentBox.size.x, currentBox.size.y - 500, currentBox.size.z);
                currentBox.center = new Vector3(currentBox.center.x, currentBox.center.y - 250, currentBox.center.z);
                //FILTER PANEL
                currentRect = gameObject.transform.Find("FilterPanel").transform.Find("Canvas").GetComponent(typeof(RectTransform)) as RectTransform;
                currentRect.offsetMax = new Vector2(currentRect.offsetMax.x, currentRect.offsetMax.y - 500);
                currentRect.localPosition = new Vector3(currentRect.localPosition.x, currentRect.localPosition.y + 250, currentRect.localPosition.z);
                currentRect = gameObject.transform.Find("FilterPanel").GetComponent(typeof(RectTransform)) as RectTransform;
                currentRect.localPosition = new Vector3(currentRect.localPosition.x, currentRect.localPosition.y - 250, currentRect.localPosition.z);
                currentRect = gameObject.transform.Find("FilterPanel").transform.Find("Canvas").transform.Find("Content").transform.Find("LowerPanel").GetComponent(typeof(RectTransform)) as RectTransform;
                currentRect.localPosition = new Vector3(currentRect.localPosition.x, currentRect.localPosition.y + 500, currentRect.localPosition.z);
                currentRect = gameObject.transform.Find("FilterPanel").transform.Find("Canvas").transform.Find("Content").transform.Find("MiddlePanel").transform.Find("Filters").transform.Find("ScrollArea").transform.Find("ScrollDown").GetComponent(typeof(RectTransform)) as RectTransform;
                currentRect.localPosition = new Vector3(currentRect.localPosition.x, currentRect.localPosition.y + 770, currentRect.localPosition.z);
                currentRect = gameObject.transform.Find("FilterPanel").transform.Find("Canvas").transform.Find("Content").transform.Find("MiddlePanel").transform.Find("Filters").transform.Find("ScrollArea").transform.Find("Scroll View").GetComponent(typeof(RectTransform)) as RectTransform;
                currentRect.offsetMin = new Vector2(currentRect.offsetMin.x, currentRect.offsetMin.y + 770);
                currentBox = gameObject.transform.Find("FilterPanel").GetComponent(typeof(BoxCollider)) as BoxCollider;
                currentBox.size = new Vector3(currentBox.size.x, currentBox.size.y - 500, currentBox.size.z);
                //MINIMAP PANEL
                currentRect = gameObject.transform.Find("MinimapPanel").transform.Find("Canvas").GetComponent(typeof(RectTransform)) as RectTransform;
                currentRect.offsetMax = new Vector2(currentRect.offsetMax.x - 500, currentRect.offsetMax.y - 500);
                currentRect = gameObject.transform.Find("MinimapPanel").transform.Find("Canvas").transform.Find("Content").GetComponent(typeof(RectTransform)) as RectTransform;
                currentRect.localPosition = new Vector3(currentRect.localPosition.x - 250, currentRect.localPosition.y + 500, currentRect.localPosition.z);
                currentRect = gameObject.transform.Find("MinimapPanel").transform.Find("Canvas").transform.Find("Content").transform.Find("Canvas").GetComponent(typeof(RectTransform)) as RectTransform;
                currentRect.localPosition = new Vector3(currentRect.localPosition.x + 300, currentRect.localPosition.y - 250, currentRect.localPosition.z);
                currentRect.offsetMax = new Vector2(currentRect.offsetMax.x - 100, currentRect.offsetMax.y - 100);
                currentBox = gameObject.transform.Find("MinimapPanel").GetComponent(typeof(BoxCollider)) as BoxCollider;
                currentBox.size = new Vector3(currentBox.size.x - 500, currentBox.size.y - 500, currentBox.size.z);
                currentBox.center = new Vector3(currentBox.center.x - 250, currentBox.center.y - 250, currentBox.center.z);
            }
        }

        public void Teleport()
        {
            if (currentSelection > -1 && currentSelection < displayedList.Count)
            {

                //get gameobject volume
                if (displayedList[currentSelection].GetComponentInChildren<Collider>() != null)
                {
                    Collider rend = displayedList[currentSelection].GetComponentInChildren<Collider>();
                    Vector3 vec = rend.bounds.size;
                    size = vec.x;
                }
                float scaleY = displayedList[currentSelection].transform.lossyScale.y;
                //get current position of camera rig
                Transform transformToSet = MRET.InputRig.transform;
                float currYPos = transformToSet.position.y;
                //get current position of selected scene object
                float newXPos = displayedList[currentSelection].transform.position.x;
                float newYPos = displayedList[currentSelection].transform.position.y;
                float newZPos = displayedList[currentSelection].transform.position.z;
                //set position to teleport to, scale teleport distance based on gameobject size
                if (size > 1)
                {
                    if (size > 10)
                        size /= 2;
                    transformToSet.position = new Vector3((float)(newXPos + size + scaleY), (float)(newYPos), (float)(newZPos + size + scaleY));
                }
                else
                    transformToSet.position = new Vector3((float)(newXPos + 2), (float)(newYPos - 1), (float)(newZPos + 2));
                float origX = transformToSet.eulerAngles.x;
                float origZ = transformToSet.eulerAngles.z;
                //rotate camera rig and camera to face selected scene object
                transformToSet.LookAt(displayedList[currentSelection].transform);
                transformToSet.Rotate(new Vector3(0, -1 * MRET.InputRig.head.transform.parent.localEulerAngles.y, 0));
                transformToSet.eulerAngles = new Vector3(origX, transformToSet.eulerAngles.y, origZ);
            }
        }

        public void mmTeleport(GameObject obj)
        {
            //get gameobject volume
            if (obj.GetComponentInChildren<Collider>() != null)
            {
                Collider rend = obj.GetComponentInChildren<Collider>();
                Vector3 vec = rend.bounds.size;
                size = vec.x;
            }
            float scaleY = obj.transform.lossyScale.y;
            //get current position of camera rig
            Transform transformToSet = MRET.InputRig.transform;
            float currYPos = transformToSet.position.y;
            //get current position of selected scene object
            float newXPos = obj.transform.position.x;
            float newYPos = obj.transform.position.y;
            float newZPos = obj.transform.position.z;
            //set position to teleport to, scale teleport distance based on gameobject size
            if (size > 1)
            {
                if (size > 10)
                    size /= 2;
                transformToSet.position = new Vector3((float)(newXPos + size + scaleY), (float)(newYPos), (float)(newZPos + size + scaleY));
            }
            else
                transformToSet.position = new Vector3((float)(newXPos + 2), (float)(newYPos - 1), (float)(newZPos + 2));
            float origX = transformToSet.eulerAngles.x;
            float origZ = transformToSet.eulerAngles.z;
            //rotate camera rig and camera to face selected scene object
            transformToSet.LookAt(obj.transform);
            transformToSet.Rotate(new Vector3(0, -1 * MRET.InputRig.transform.localEulerAngles.y, 0));
            transformToSet.eulerAngles = new Vector3(origX, transformToSet.eulerAngles.y, origZ);


        }

        public void ToggleGravity()
        {
            //allow toggle gravity only for the main physical scene objects displayed on first page
            if (currentSelection > -1 && currentSelection < displayedList.Count && layerNum == 0)
            {
                IPhysicalSceneObject physicalSceneObject = displayedList[currentSelection].GetComponent<IPhysicalSceneObject>();
                if (physicalSceneObject != null)
                {
                    // Initialize gravity toggle               
                    bool state = gravity.isOn;

                    // Only turn on gravity if physics is on
                    if ((!physics.isOn) || (!state))
                    {
                        physicalSceneObject.EnableGravity = false;
                        gravity.isOn = false;
                    }
                    else if (physics.isOn)
                    {
                        physicalSceneObject.EnableGravity = true;
                        gravity.isOn = !physicalSceneObject.EnableGravity;
                    }

                    // Record the action
                    ProjectManager.UndoManager.AddAction(
                        new UpdatePhysicsAction(physicalSceneObject, physicalSceneObject.EnableCollisions, physicalSceneObject.EnableGravity, physicalSceneObject.Mass),
                        new UpdatePhysicsAction(physicalSceneObject, physicalSceneObject.EnableCollisions, !physicalSceneObject.EnableGravity, physicalSceneObject.Mass));

                    // get selected physical scene object's configuration panel controller and display its gravity toggle state as "on" or "off"
                    objPanel = physicalSceneObject.gameObject.GetComponent<ObjectPanelController>();
                    if (objPanel != null)
                    {
                        objPanel.GravityToggle(gravity.isOn);
                    }
                }
            }
        }

        public void TogglePhysics()
        {
            if (currentSelection > -1 && currentSelection < displayedList.Count && layerNum == 0)
            {
                IPhysicalSceneObject physicalSceneObject = displayedList[currentSelection].GetComponent<IPhysicalSceneObject>();
                if (physicalSceneObject != null)
                {
                    // Initialize physics toggle
                    physics.isOn = physicalSceneObject.EnableCollisions;
                    bool state = physics.isOn;

                    // Turn off gravity if physics is off.
                    if (!state)
                    {
                        if (gravity.isOn)
                        {
                            ToggleGravity();
                        }
                    }

                    // Toggle physics
                    physicalSceneObject.EnableCollisions = !state;

                    // Record the action
                    ProjectManager.UndoManager.AddAction(
                        new UpdatePhysicsAction(physicalSceneObject, physicalSceneObject.EnableCollisions, physicalSceneObject.EnableGravity, physicalSceneObject.Mass),
                        new UpdatePhysicsAction(physicalSceneObject, !physicalSceneObject.EnableCollisions, physicalSceneObject.EnableGravity, physicalSceneObject.Mass));

                    // get selected physical scene object's configuration panel controller and display its physics toggle state as "on" or "off"
                    objPanel = physicalSceneObject.gameObject.GetComponent<ObjectPanelController>();
                    if (objPanel != null)
                    {
                        objPanel.PhysicsToggle(physics.isOn);
                    }
                }
            }
        }

        private void SetActiveSelection(int listID)
        {
            Dictionary<GameObject, Button> mmoDict = mmController.getMmoDict();

            //reset color of previously selected scene object
            resetColor(selected);
            resetBtnColor(selectedMmoBtn);
            currentSelection = listID;
            hierarchyListDisplay.HighlightItem(listID);

            GameObject obj = displayedList[currentSelection];

            //change color of minimap icon when corresponding list item is selected
            Button mmoBtn;
            mmoDict.TryGetValue(obj, out mmoBtn);
            if (mmoBtn != null)
            {
                changeBtnColor(mmoBtn);
                selectedMmoBtn = mmoBtn;
            }

            // physics and gravity toggle matches what's displayed in the object's ObjectPanel
            sceneObject = obj.GetComponent<IInteractable>();
            if (sceneObject is IPhysicalSceneObject)
            {                
                objPanel = sceneObject.gameObject.GetComponent<ObjectPanelController>();
                if (objPanel != null)
                {
                    gravity.isOn = objPanel.gravity.isOn;
                    physics.isOn = objPanel.physics.isOn;
                }
            }

            //highlight object in scene
            changeColor(obj);
        }

        private void changeColor(GameObject selectedSceneObject)
        {
            objectRenderers = selectedSceneObject.GetComponentsInChildren<MeshRenderer>();

            List<GameObject> meshObjList = new List<GameObject>();
            List<Material> objMatList = new List<Material>();
            foreach (MeshRenderer rend in objectRenderers)
            {
                meshObjList.Add(rend.gameObject);
                foreach (Material mat in rend.materials)
                {
                    objMatList.Add(mat);
                }
            }
            objectMaterials = objMatList.ToArray();

            foreach (MeshRenderer rend in selectedSceneObject.GetComponentsInChildren<MeshRenderer>())
            {
                int rendMatCount = rend.materials.Length;
                Material[] rendMats = new Material[rendMatCount];
                for (int j = 0; j < rendMatCount; j++)
                {
                    rendMats[j] = highlightMaterial;
                }
                rend.materials = rendMats;
            }
            selected = selectedSceneObject;
        }

        private void resetColor(GameObject unselectedSceneObject)
        {
            if (selected != null)
            {
                int i = 0;
                foreach (MeshRenderer rend in objectRenderers)
                {
                    int rendMatCount = rend.materials.Length;
                    Material[] rendMats = new Material[rendMatCount];
                    for (int j = 0; j < rendMatCount; j++)
                    {
                        rendMats[j] = objectMaterials[i++];
                    }
                    rend.materials = rendMats;
                    selected = null;
                }
            }
        }

        private void changeBtnColor(Button btn)
        {
            //change color of minimap icon 
            if (btn != null)
            {
                ColorBlock colors = btn.colors;
                colors.normalColor = Color.black;
                colors.highlightedColor = new Color32(225, 225, 225, 255);
                btn.colors = colors;
            }
        }

        private void resetBtnColor(Button btn)
        {
            if (btn != null)
            {
                ColorBlock colors = btn.colors;
                colors.normalColor = Color.white;
                colors.highlightedColor = new Color32(225, 225, 225, 255);
                btn.colors = colors;
            }
        }

        private void SetActiveSelectionFilterID(int ID)
        {
            filterListDisplay.HighlightItem(ID);
            clearFilterByID(ID);
        }

        public void PopulateSceneObjectLists(List<GameObject> list)
        {
            if (list != null)
            {
                hierarchyListDisplay.ClearScrollList();

                displayedList = list;
                displayedListCount = list.Count;
                List<GameObject> DictList;

                for (int i = 0; i < displayedListCount; i++)
                {
                    int indexToSelect = i;
                    UnityEvent clickEvent = new UnityEvent();
                    clickEvent.AddListener(new UnityAction(() => { SetActiveSelection(indexToSelect); }));
                    if (list[i].name == null)
                    {
                        hierarchyListDisplay.AddScrollListItem("Null");
                    }
                    else
                    {
                        if (GODict.TryGetValue(list[i], out DictList))
                        {
                            if (DictList.Count < 1)
                            {
                                hierarchyListDisplay.AddScrollListItem(list[i].name + "\n" + list[i].transform.position, defaultThumbnail, clickEvent);
                            }
                            else
                            {
                                hierarchyListDisplay.AddScrollListItem(list[i].name + " (Has Children) \n" + list[i].transform.position, defaultThumbnail, clickEvent);
                            }
                        }
                        else
                        {
                            hierarchyListDisplay.AddScrollListItem(list[i].name + "\n" + list[i].transform.position, defaultThumbnail, clickEvent);
                        }
                    }
                }
            }
        }

        public void Open()
        {
            if (currentSelection > -1 && currentSelection < displayedListCount)
            {
                backCount = 0;
                GameObject obj = displayedList[currentSelection];
                List<GameObject> list;
                GODict.TryGetValue(obj, out list);
                if (list != null && list.Count > 0)
                {
                    menuSequence.Add(list);
                    List<string> words = new List<string>();
                    foreach (string str in filterWords)
                    {
                        words.Add(str);
                    }
                    filterSequence.Add(words);
                    filterWords.Clear();
                    
                    if (list.Count > 0)
                    {
                        layerNum++;
                    }
                    currentSelection = -1;
                    hierarchyListDisplay.SetTitle("Child Layer " + layerNum);
                    //populate scene object list with correct items in hierarchy sequence
                    PopulateSceneObjectLists(filterList(filterWords, menuSequence[menuSequence.Count - 1]));
                    filterListDisplay.ClearScrollList();
                }
            }
        }

        public void Back()
        {
            if (menuSequence.Count > 0 && layerNum > 0)
            {
                resetColor(selected);
                currentSelection = -1;

                if (backCount != 0)
                {
                    menuSequence.Remove(menuSequence[menuSequence.Count - 1]);
                    filterSequence.Remove(filterSequence[filterSequence.Count - 1]);
                }
                else if (backCount == 0)
                {
                    menuSequence.Remove(menuSequence[menuSequence.Count - 1]);
                }
                backCount++;
                //populate filter and scene object list with correct items in  hierarchy sequence
                PopulateSceneObjectLists(filterList(filterSequence[filterSequence.Count - 1], menuSequence[menuSequence.Count - 1]));
                PopulateFilterScrollList(filterSequence[filterSequence.Count - 1]);
                if (layerNum > 0)
                {
                    layerNum--;
                }
                if (layerNum == 0)
                {
                    hierarchyListDisplay.SetTitle("Scene Objects");
                }
                else
                {
                    hierarchyListDisplay.SetTitle("Child Layer " + layerNum);
                }
                foreach (GameObject obj in menuSequence[menuSequence.Count - 1])
                {
                    resetColor(obj);
                }
            }
        }

        public void addFilter()
        {
            string filterString = filterInputField.text;

            if (backCount == 0)
            {
                if (!filterWords.Contains(filterString))
                {
                    filterWords.Add(filterString);
                    PopulateSceneObjectLists(filterList(filterWords, menuSequence[menuSequence.Count - 1]));
                    PopulateFilterScrollList(filterWords);
                }
            }
            else
            {
                if (!filterSequence[filterSequence.Count - 1].Contains(filterString))
                {
                    filterSequence[filterSequence.Count - 1].Add(filterString);
                    PopulateSceneObjectLists(filterList(filterSequence[filterSequence.Count - 1], menuSequence[menuSequence.Count - 1]));
                    PopulateFilterScrollList(filterSequence[filterSequence.Count - 1]);
                }
            }
        }

        public List<GameObject> filterList(List<string> wordList, List<GameObject> objList)
        {
            //apply list of filter words to the list of gameobjects and return filtered list
            if (wordList.Count > 0)
            {
                List<GameObject> filteredList = new List<GameObject>();

                foreach (GameObject obj in objList)
                {
                    bool add = true;

                    foreach (string word in wordList)
                    {
                        if (obj.name.IndexOf(word, StringComparison.OrdinalIgnoreCase) < 0)
                        {
                            add = false;
                        }
                    }
                    if (add == true && !filteredList.Contains(obj))
                        filteredList.Add(obj);
                }
                return filteredList;
            }
            return objList;
        }

        public void clearFilter()
        {
            //remove all filters from layer
            if (filterSequence.Count > 0)
                filterSequence[filterSequence.Count - 1].Clear();
            filterWords.Clear();
            PopulateSceneObjectLists(menuSequence[menuSequence.Count - 1]);
            filterListDisplay.ClearScrollList();
            resetColor(selected);
            currentSelection = -1;
        }

        public void clearFilterByID(int id)
        {
            //remove single selected filter word
            if (filterSequence.Count > 1 && backCount != 0)
                filterSequence[filterSequence.Count - 1].RemoveAt(id);
            else
                filterWords.RemoveAt(id);
            PopulateSceneObjectLists(filterList(filterWords, menuSequence[menuSequence.Count - 1]));
            PopulateFilterScrollList(filterWords);
        }

        public void PopulateFilterScrollList(List<string> list)
        {
            //display filter words on menu 
            filterListDisplay.ClearScrollList();

            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    int indexToSelect = i;
                    UnityEvent newEvent = new UnityEvent();
                    newEvent.AddListener(new UnityAction(() => { SetActiveSelectionFilterID(indexToSelect); }));
                    filterListDisplay.AddScrollListItem(list[i], newEvent);
                }
            }
        }

        public void AddInteractableGO()
        {
            //gets all game objs, disregards hierarchy
            foreach (GameObject obj in FindObjectsOfType(typeof(GameObject)))
            {
                if ((obj.GetComponent<IInteractable>() != null) && (hierarchyList.Contains(obj) == false) && (obj.transform.parent.GetComponent<IInteractable>() == null))
                {
                    hierarchyList.Add(obj);
                    printGOHierarchy(obj);

                }
            }
            hierarchyList.Sort(sortByName);
            foreach (GameObject mmObj in hierarchyList)
            {
                //add minimap drawn object script to main gameobjects
                MinimapDrawnObject mmdo = mmObj.AddComponent<MinimapDrawnObject>();
                mmdo.minimapSprite = mms; 
            }
        }

        public void destroyPanel()
        {
            GameObject[] destroyPanel = GameObject.FindGameObjectsWithTag("hierarchyview");
            foreach (GameObject obj in destroyPanel)
                Destroy(obj);
        }

        public void printGOHierarchy(GameObject obj)
        {
            //find gameobject hierarchy and store 
            List<GameObject> childList = new List<GameObject>();
            string objTransformStr = "";
            objTransform = obj.GetComponentsInChildren<Transform>();
            foreach (Transform t in objTransform)
            {
                if (t != null && t.gameObject != null && hierarchyList.Contains(t.gameObject) == false && t.parent == obj.transform)
                {
                    objTransformStr += t.gameObject + ", ";
                    childList.Add(t.gameObject);
                    if (t.childCount > 0)
                        printGOHierarchy(t.gameObject);
                }

            }
            childList.Sort(sortByName);
            GODict.Add(obj, childList);
        }

        public static int sortByName(GameObject obj1, GameObject obj2)
        {
            return obj1.name.CompareTo(obj2.name);
        }
    }
}
