using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GSFC.ARVR.MRET.Common
{
    public class MainHierarchyManager : MonoBehaviour
    {
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
        private InteractablePart iPart;
        private float size;
        public Sprite mms, mmsUser;
        private bool minimapDisplay = false;
        public MinimapController mmController;
        private Button selectedMmoBtn;

        private void Start()
        {
            filterListDisplay.SetTitle("Filters");
            hierarchyListDisplay.SetTitle("Parts");
            layerNum = 0;
            AddInteractableGO();
            PopulatePartLists(hierarchyList);
            menuSequence.Add(hierarchyList);
            filterSequence.Add(new List<string>());
            physics.isOn = false;
            gravity.isOn = false;
            //place minimap icon to mark user's location
            MinimapDrawnObject userMdo = ModeNavigator.instance.headsetFollower.AddComponent<MinimapDrawnObject>();
            userMdo.minimapSprite = mmsUser;
        }

        private void Update()
        {
            Transform trans = Camera.main.transform;
            userPositionLabel.text = "My Position:  " + trans.position.ToString();
        }

        private void OnEnable()
        {
            PopulatePartLists(hierarchyList);
        }

        private void OnDisable()
        {
            resetColor(selected);
        }

        public void toggleMinimap()
        {
            //toggle minimap display on and off
            minimapDisplay = !minimapDisplay;
            if (minimapDisplay == true)
                minimapPanel.SetActive(true);
            else
                minimapPanel.SetActive(false);
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
                Transform transformToSet = ModeNavigator.instance.headsetFollower.transform.parent.parent;
                float currYPos = transformToSet.position.y;
                //get current position of selected part
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
                //rotate camera rig and camera to face selected part
                transformToSet.LookAt(displayedList[currentSelection].transform);
                transformToSet.Rotate(new Vector3(0, -1 * ModeNavigator.instance.headsetFollower.transform.parent.localEulerAngles.y, 0));
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
            Transform transformToSet = ModeNavigator.instance.headsetFollower.transform.parent.parent;
            float currYPos = transformToSet.position.y;
            //get current position of selected part
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
            //rotate camera rig and camera to face selected part
            transformToSet.LookAt(obj.transform);
            transformToSet.Rotate(new Vector3(0, -1 * ModeNavigator.instance.headsetFollower.transform.parent.localEulerAngles.y, 0));
            transformToSet.eulerAngles = new Vector3(origX, transformToSet.eulerAngles.y, origZ);


        }

        public void ToggleGravity()
        {
            //allow toggle gravity only for the main interactable parts displayed on first page
            if (currentSelection > -1 && currentSelection < displayedList.Count && layerNum == 0)
            {
                bool state = gravity.isOn;

                // Initialize gravity toggle               
                Rigidbody rBody = displayedList[currentSelection].GetComponent<Rigidbody>();

                // Only turn on gravity if physics is on
                if ((!physics.isOn) || (!state))
                {
                    rBody.useGravity = false;
                    gravity.isOn = false;
                }

                else if (physics.isOn)
                {
                    gravity.isOn = !displayedList[currentSelection].GetComponent<Rigidbody>().useGravity;
                    rBody.useGravity = true;
                }

                InteractablePart iPart = displayedList[currentSelection].GetComponent<InteractablePart>();
                UndoManager.instance.AddAction(ProjectAction.UpdateObjectSettingsAction(iPart.name,
                    new InteractablePart.InteractablePartSettings(iPart.isGrabbable,
                    !rBody.isKinematic, rBody.useGravity), iPart.guid.ToString()),
                    ProjectAction.UpdateObjectSettingsAction(iPart.name,
                    new InteractablePart.InteractablePartSettings(iPart.isGrabbable,
                    !rBody.isKinematic, !rBody.useGravity), iPart.guid.ToString()));

                //get selected part's object panel controller and display its gravity toggle state as "on" or "off"
                if (displayedList[currentSelection].GetComponent<InteractablePart>() != null)
                {
                    iPart = displayedList[currentSelection].GetComponent<InteractablePart>();
                    if (iPart.GetComponent<ObjectPanelController>() != null)
                    {
                        objPanel = iPart.GetComponent<ObjectPanelController>();
                        objPanel.gravityToggle(gravity.isOn);
                    }
                }
            }
        }

        public void TogglePhysics()
        {
            if (currentSelection > -1 && currentSelection < displayedList.Count && layerNum == 0)
            {
                // Initialize physics toggle.
                physics.isOn = displayedList[currentSelection].GetComponent<Rigidbody>().isKinematic;
                bool state = physics.isOn;

                // Turn off gravity if physics is off.
                if (!state)
                {
                    if (gravity.isOn)
                    {
                        ToggleGravity();
                    }
                }

                Rigidbody rBody = displayedList[currentSelection].GetComponent<Rigidbody>();
                rBody.isKinematic = !state;

                InteractablePart iPart = displayedList[currentSelection].GetComponent<InteractablePart>();
                UndoManager.instance.AddAction(ProjectAction.UpdateObjectSettingsAction(iPart.name,
                    new InteractablePart.InteractablePartSettings(iPart.isGrabbable,
                    !rBody.isKinematic, rBody.useGravity), iPart.guid.ToString()),
                    ProjectAction.UpdateObjectSettingsAction(iPart.name,
                    new InteractablePart.InteractablePartSettings(iPart.isGrabbable,
                    rBody.isKinematic, rBody.useGravity), iPart.guid.ToString()));

                //get selected part's object panel controller and display its physics toggle state as "on" or "off"
                if (displayedList[currentSelection].GetComponent<InteractablePart>() != null)
                {
                    iPart = displayedList[currentSelection].GetComponent<InteractablePart>();
                    if (iPart.GetComponent<ObjectPanelController>() != null)
                    {
                        objPanel = iPart.GetComponent<ObjectPanelController>();
                        objPanel.physicsToggle(physics.isOn);
                    }
                }
            }
        }

        private void SetActiveSelection(int listID)
        {
            Dictionary<GameObject, Button> mmoDict = mmController.getMmoDict();

            //reset color of previously selected part
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

            //physics and gravity toggle matches what's displayed in the object's ObjectPanel
            if (obj.GetComponent<InteractablePart>() != null)
            {                
                iPart = obj.GetComponent<InteractablePart>();
                if (iPart.GetComponent<ObjectPanelController>() != null)
                {
                    objPanel = iPart.GetComponent<ObjectPanelController>();
                    gravity.isOn = objPanel.gravity.isOn;
                    physics.isOn = objPanel.physics.isOn;
                }
            }
            //highlight object in scene
            changeColor(obj);
        }

        private void changeColor(GameObject selectedPart)
        {
            objectRenderers = selectedPart.GetComponentsInChildren<MeshRenderer>();

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

            foreach (MeshRenderer rend in selectedPart.GetComponentsInChildren<MeshRenderer>())
            {
                int rendMatCount = rend.materials.Length;
                Material[] rendMats = new Material[rendMatCount];
                for (int j = 0; j < rendMatCount; j++)
                {
                    rendMats[j] = highlightMaterial;
                }
                rend.materials = rendMats;
            }
            selected = selectedPart;
        }

        private void resetColor(GameObject unselectedPart)
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

        public void PopulatePartLists(List<GameObject> list)
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
                                hierarchyListDisplay.AddScrollListItem(list[i].name + " (Has Subparts) \n" + list[i].transform.position, defaultThumbnail, clickEvent);
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
                    hierarchyListDisplay.SetTitle("Subpart Layer " + layerNum);
                    //populate parts list with correct items in hierarchy sequence
                    PopulatePartLists(filterList(filterWords, menuSequence[menuSequence.Count - 1]));
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
                //populate filter and parts list with correct items in  hierarchy sequence
                PopulatePartLists(filterList(filterSequence[filterSequence.Count - 1], menuSequence[menuSequence.Count - 1]));
                PopulateFilterScrollList(filterSequence[filterSequence.Count - 1]);
                if (layerNum > 0)
                {
                    layerNum--;
                }
                if (layerNum == 0)
                {
                    hierarchyListDisplay.SetTitle("Parts");
                }
                else
                {
                    hierarchyListDisplay.SetTitle("Subpart Layer " + layerNum);
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
                    PopulatePartLists(filterList(filterWords, menuSequence[menuSequence.Count - 1]));
                    PopulateFilterScrollList(filterWords);
                }
            }
            else
            {
                if (!filterSequence[filterSequence.Count - 1].Contains(filterString))
                {
                    filterSequence[filterSequence.Count - 1].Add(filterString);
                    PopulatePartLists(filterList(filterSequence[filterSequence.Count - 1], menuSequence[menuSequence.Count - 1]));
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
            PopulatePartLists(menuSequence[menuSequence.Count - 1]);
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
            PopulatePartLists(filterList(filterWords, menuSequence[menuSequence.Count - 1]));
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
            foreach (GameObject obj in GameObject.FindObjectsOfType(typeof(GameObject)))
            {
                //if ((obj.transform.parent == null) && (obj.GetComponent<InteractablePart>() != null))
                if ((obj.GetComponent<InteractablePart>() != null) && (hierarchyList.Contains(obj) == false) && (obj.transform.parent.GetComponent<InteractablePart>() == null))
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
