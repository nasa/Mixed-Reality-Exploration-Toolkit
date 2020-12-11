using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using AssetBundles;
using GSFC.ARVR.MRET.Common.Schemas;

public class PartImportMenuManager : MonoBehaviour
{
    private class PartButtonInfo
    {
        public string name, description, timestamp;
        public Texture2D thumbnail;
        public UnityEngine.Events.UnityEvent eventToCall;

        public PartButtonInfo(string n, string d, string ts, Texture2D th, UnityEngine.Events.UnityEvent ev)
        {
            name = n;
            description = d;
            timestamp = ts;
            thumbnail = th;
            eventToCall = ev;
        }
    }

    public ScrollListManager filterListDisplay, partListDisplay;
    public TileListManager partTileDisplay;
    public GameObject partPanelPrefab, grabCubePrefab;
    public Color partTouchHighlightColor;
    public Texture2D defaultThumbnail;
    public GameObject listViewIcon, gridViewIcon;
    
    private GameObject projectObjectContainer, placingObjectContainer;
    private ConfigurationManager configMan;
    private List<AssetInfo> parts;
    private List<string[]> filters = new List<string[]>();
    private int currentSelection = -1, currentFilterSelection = -1;
    private string[] partFilter = new string[] { "", "" };
    private bool gridViewEnabled = false;
    private Queue<PartButtonInfo> partButtonsQueue = new Queue<PartButtonInfo>();
    private List<Material> defaultPartMaterials;
    private LoadingIndicatorManager loadingIndicatorManager = null;

    void Start()
    {
        configMan = FindObjectOfType<ConfigurationManager>();

        projectObjectContainer = GameObject.Find("/LoadedProject/GameObjects");

        if (VRDesktopSwitcher.isDesktopEnabled())
        {
            placingObjectContainer = GameObject.Find("/LoadedProject/PartsPlaced");
        }

        defaultPartMaterials = FindObjectOfType<SessionConfiguration>().defaultPartMaterials;

        loadingIndicatorManager = FindObjectOfType<LoadingIndicatorManager>();

        filterListDisplay.SetTitle("Filters");
        partListDisplay.SetTitle("Parts");
        partTileDisplay.SetTitle("Parts");
        PopulateFilterScrollList();
        PopulatePartLists();
	}

    private int partButtonCounter = 0;
    void Update()
    {
        if (partButtonCounter++ > 5)
        {
            partButtonCounter = 0;
        }
        else
        {
            return;
        }
        if (partButtonsQueue.Count > 0)
        {
            PartButtonInfo info = partButtonsQueue.Dequeue();

            partListDisplay.AddScrollListItem(info.name + "\n" + info.description + "\n"
                + info.timestamp, info.thumbnail, info.eventToCall);
            partTileDisplay.AddTileListItem(info.name, info.thumbnail, info.eventToCall);
        }
    }

    public void Import()
    {
        if (currentSelection > -1)
        {
            // Deserialize Part File.
            try
            {
                StartCoroutine(InitializeAssetBundleManager());

                XmlSerializer ser = new XmlSerializer(typeof(PartType));
                XmlReader reader = XmlReader.Create(parts[currentSelection].assetFile);
                PartType prt = (PartType) ser.Deserialize(reader);

                StartCoroutine(InstantiateAPart(prt,
                    VRDesktopSwitcher.isDesktopEnabled()? placingObjectContainer.transform : null));
            }
            catch (Exception e)
            {
                Debug.Log("[PartImportMenuManager->Import] " + e.ToString());
            }
        }
    }

    public void SetFilter(int listID)
    {
        if (listID == 0)
        {
            partFilter = new string[] { "", "" };
        }
        else
        {
            currentFilterSelection = listID - 1;
            if (filters[currentFilterSelection] != null)
            {
                partFilter = filters[currentFilterSelection];
            }
        }
        filterListDisplay.HighlightItem(listID);
        PopulatePartLists();
    }

    public void PopulateFilterScrollList()
    {
        ConfigurationManager configManager = FindObjectOfType<ConfigurationManager>();

        if (configManager)
        {
            filterListDisplay.ClearScrollList();
            filters = configManager.assetFilters.Get();

            UnityEngine.Events.UnityEvent firstClickEvent = new UnityEngine.Events.UnityEvent();
            firstClickEvent.AddListener(new UnityEngine.Events.UnityAction(() => { SetFilter(0); }));
            filterListDisplay.AddScrollListItem("None", firstClickEvent);

            int i = 1;
            foreach (string[] filter in filters)
            {
                int indexToSelect = i;
                i++;
                UnityEngine.Events.UnityEvent clickEvent = new UnityEngine.Events.UnityEvent();
                clickEvent.AddListener(new UnityEngine.Events.UnityAction(() => { SetFilter(indexToSelect); }));
                filterListDisplay.AddScrollListItem(filter[0] + " | " + filter[1], clickEvent);
            }
        }
    }

    public void PopulatePartLists()
    {
        ConfigurationManager configManager = FindObjectOfType<ConfigurationManager>();
        if (configManager)
        {
            partButtonsQueue.Clear();
            partListDisplay.ClearScrollList();
            partTileDisplay.ClearTileList();

            parts = configManager.assets;

            int buttonNum = 0;
            for (int i = 0; i < parts.Count; i++)
            {
                if (parts[i].CheckFilter(partFilter[0], partFilter[1]))
                {
                    int indexToSelect = i;
                    int btnToSelect = buttonNum++;
                    UnityEngine.Events.UnityEvent clickEvent = new UnityEngine.Events.UnityEvent();
                    clickEvent.AddListener(new UnityEngine.Events.UnityAction(() => { SetActiveSelection(indexToSelect, btnToSelect); }));

                    partButtonsQueue.Enqueue(new PartButtonInfo(parts[i].name, parts[i].description, parts[i].timeStamp.ToString(),
                        (parts[i].thumbnail == null) ? defaultThumbnail : parts[i].thumbnail, clickEvent));
                }
            }
        }
    }

    public void ToggleListView()
    {
        if (gridViewEnabled)
        {
            listViewIcon.SetActive(false);
            gridViewIcon.SetActive(true);
            SwitchToListView();
        }
        else
        {
            listViewIcon.SetActive(true);
            gridViewIcon.SetActive(false);
            SwitchToGridView();
        }
    }

    private void SwitchToListView()
    {
        gridViewEnabled = false;
        partListDisplay.gameObject.SetActive(true);
        partTileDisplay.gameObject.SetActive(false);
    }

    private void SwitchToGridView()
    {
        gridViewEnabled = true;
        partListDisplay.gameObject.SetActive(false);
        partTileDisplay.gameObject.SetActive(true);
    }

    private void SetActiveSelection(int listID, int buttonID)
    {
        currentSelection = listID;
        partListDisplay.HighlightItem(buttonID);
        partTileDisplay.HighlightItem(buttonID);
    }

    private void OnEnable()
    {
        PopulateFilterScrollList();
        PopulatePartLists();
    }

    private IEnumerator InstantiateAPart(PartType part, Transform parent)
    {
        loadingIndicatorManager.ShowLoadingIndicator("Loading Part...");

        // Instantiate Game Object.
        if (part.AssetBundle != "NULL")
        {   // Load from an asset bundle.
            yield return InstantiateGameObjectAsync(part, parent);
        }
        else
        {   // Load an empty part.
            yield return InstantiateEmptyGameObjectAsync(part, parent);
        }

        // Wait for the parent's transform to be set before loading any children.
        while (!part.loaded)
        {
            yield return new WaitForSeconds(0.1f);
        }

        if (part.ChildParts.Parts != null)
        {
            // Instantiate children.
            foreach (PartType pt in part.ChildParts.Parts)
            {
                yield return InstantiateAPart(pt, part.transform);
            }

            // Wait for all children to be instantiated.
            foreach (PartType pt in part.ChildParts.Parts)
            {
                while (!pt.loaded)
                {
                    yield return new WaitForSeconds(0.1f);
                }
            }

            // Instantiate any enclosures if this is the parent.
            if (parent == null)
            {
                // Instantiate enclosure.
                if (part.Enclosure != null)
                {
                    if (part.Enclosure.AssetBundle != null)
                    {
                        yield return InstantiateEnclosureAsync(part.Enclosure, part.transform);
                    }
                    else
                    {
                        yield return InstantiateEmptyGameObjectAsync(part.Enclosure, part.transform);
                    }

                    // Wait for enclosure to be instantiated.
                    if (part.Enclosure != null)
                    {
                        while (!part.Enclosure.loaded)
                        {
                            yield return new WaitForSeconds(0.1f);
                        }
                    }
                }
            }

            if (!part.StaticAttachment)
            {
                // Instantiate grab cube.
                GameObject grabCube = Instantiate(grabCubePrefab);
                grabCube.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                grabCube.transform.SetParent(part.transform);
                PlaceGrabCube(grabCube);
                grabCube.GetComponent<AssemblyGrabber>().assemblyRoot = part.transform.gameObject;
                if (part.Enclosure != null)
                {
                    grabCube.GetComponent<AssemblyGrabber>().otherGrabbers.Add(part.Enclosure.transform.GetComponent<AssemblyGrabber>());

                    AssemblyGrabber grabber = part.Enclosure.transform.GetComponent<AssemblyGrabber>();
                    grabber.otherGrabbers.Add(grabCube.GetComponent<AssemblyGrabber>());
                    grabber.assetBundle = part.Enclosure.AssetBundle;
                    if (part.Enclosure.Description != null)
                    {
                        grabber.description = (part.Enclosure.Description[0] == null) ? "" : part.Enclosure.Description[0];
                    }
                    grabber.dimensions = new Vector3(part.Enclosure.PartTransform.Scale.X, part.Enclosure.PartTransform.Scale.Y, part.Enclosure.PartTransform.Scale.Z);
                    grabber.id = part.Enclosure.ID;
                    if (part.Enclosure.MinMass != null)
                    {
                        grabber.minMass = part.Enclosure.MinMass[0];
                    }
                    if (part.Enclosure.MaxMass != null)
                    {
                        grabber.maxMass = part.Enclosure.MaxMass[0];
                    }
                    grabber.serializationName = part.Enclosure.Name;
                    grabber.gameObject.name = part.Enclosure.Name;
                    grabber.notes = part.Enclosure.Notes;
                    if (part.Enclosure.PartFileName != null)
                    {
                        grabber.partFileName = (part.Enclosure.PartFileName[0] == null) ? "" : part.Enclosure.PartFileName[0];
                    }
                    if (part.Enclosure.PartName != null)
                    {
                        grabber.partName = (part.Enclosure.PartName[0] == null) ? "" : part.Enclosure.PartName[0];
                    }
                    if (part.Enclosure.PartType1 != null)
                    {
                        grabber.partType = (part.Enclosure.PartType1.Length < 1) ? PartTypePartType.Chassis : part.Enclosure.PartType1[0];
                    }
                    grabber.reference = part.Enclosure.Reference;
                    grabber.subsystem = part.Enclosure.Subsystem;
                    if (part.Enclosure.Vendor != null)
                    {
                        grabber.vendor = (part.Enclosure.Vendor[0] == null) ? "" : part.Enclosure.Vendor[0];
                    }
                    grabber.version = part.Enclosure.Version;
                }
                part.transform.GetComponent<InteractablePart>().grabCube = grabCube;

                // If this isn't the root object, hide the grab cube.
                if (parent != null)
                {
                    grabCube.SetActive(false);
                }
            }
        }

        // Store part information.
        InteractablePart iPart = part.transform.GetComponent<InteractablePart>();
        iPart.assetBundle = part.AssetBundle;
        if (part.Description != null)
        {
            iPart.description = (part.Description[0] == null) ? "" : part.Description[0];
        }
        iPart.dimensions = new Vector3(part.PartTransform.Scale.X, part.PartTransform.Scale.Y, part.PartTransform.Scale.Z);
        iPart.id = part.ID;
        if (part.MinMass != null)
        {
            iPart.minMass = part.MinMass[0];
        }
        if (part.MaxMass != null)
        {
            iPart.maxMass = part.MaxMass[0];
        }
        if (part.MassContingency != null)
        {
            iPart.massContingency = part.MassContingency[0];
        }
        iPart.serializationName = part.Name;
        iPart.gameObject.name = part.Name;
        iPart.notes = part.Notes;
        if (part.PartFileName != null)
        {
            iPart.partFileName = (part.PartFileName[0] == null) ? "" : part.PartFileName[0];
        }
        if (part.PartName != null)
        {
            iPart.partName = (part.PartName[0] == null) ? "" : part.PartName[0];
        }
        if (part.PartType1 != null)
        {
            iPart.partType = (part.PartType1.Length < 1) ? PartTypePartType.Chassis : part.PartType1[0];
        }
        iPart.idlePower = part.IdlePower;
        iPart.averagePower = part.AveragePower;
        iPart.peakPower = part.PeakPower;
        iPart.powerContingency = part.PowerContingency;
        iPart.reference = part.Reference;
        iPart.subsystem = part.Subsystem;
        if (part.Vendor != null)
        {
            iPart.vendor = (part.Vendor[0] == null) ? "" : part.Vendor[0];
        }
        iPart.version = part.Version;
        iPart.randomizeTexture = part.RandomizeTexture;
        iPart.guid = Guid.NewGuid();

        if (parent == null)
        {
            // This is the root object. Either attach to its attach to object or
            // start placing it.
            Transform transformToAttachTo = null;
            if (part.AttachToName != null && part.AttachToName != "")
            {
                transformToAttachTo = projectObjectContainer.transform.Find(part.AttachToName);
            }

            // Check that the transform position/rotation/scale contains all information.
            if (part.AttachToTransform == null || part.AttachToTransform.Position == null
                || part.AttachToTransform.Rotation == null)
            {
                transformToAttachTo = null;
            }

            if (transformToAttachTo)
            {
                if (part.IsOnlyAttachment)
                {
                    // If this is the only attachment, destroy all existing parts at same level.
                    foreach (Transform tr in transformToAttachTo)
                    {
                        if (tr != transformToAttachTo && tr.GetComponent<InteractablePart>() != null)
                        {
                            Destroy(tr.gameObject);
                        }
                    }
                }

                // If there is a transform to attach to, attach it.
                part.transform.SetParent(transformToAttachTo);
                part.transform.localPosition = new Vector3(part.AttachToTransform.Position.X,
                    part.AttachToTransform.Position.Y, part.AttachToTransform.Position.Z);
                part.transform.localRotation = new Quaternion(part.AttachToTransform.Rotation.X,
                    part.AttachToTransform.Rotation.Y, part.AttachToTransform.Rotation.Z,
                    part.AttachToTransform.Rotation.W);

                // If attachment is static, move colliders to its parent.
                if (part.StaticAttachment)
                {
                    // Remove colliders from that object to prevent moving of that part.
                    foreach (Collider coll in part.transform.GetComponentsInChildren<Collider>())
                    {
                        Destroy(coll);
                    }

                    // Generate a box collider for selecting the attachment.
                    // Recalculate the bounds.
                    Bounds bou = new Bounds(part.transform.position, Vector3.zero);
                    foreach (Renderer ren in part.transform.GetComponentsInChildren<Renderer>())
                    {
                        bou.Encapsulate(ren.bounds);
                    }

                    BoxCollider collider = part.transform.gameObject.AddComponent<BoxCollider>();
                    collider.size = Vector3.Scale(bou.size,
                        new Vector3(1 / part.transform.localScale.x, 1 / part.transform.localScale.y, 1 / part.transform.localScale.z));
                    collider.center = part.transform.InverseTransformPoint(bou.center);
                }
            }
            else
            {
                // If there isn't a transform to attach the object to, start placing it.
                if (VRDesktopSwitcher.isDesktopEnabled())
                {
                    part.transform.GetComponent<InteractablePart>().StartPlacing();
                }
                else
                {
                    GameObject controller = VRTK.VRTK_DeviceFinder.GetControllerRightHand();
                    VRTK.VRTK_ControllerEvents cEvents = controller.GetComponent<VRTK.VRTK_ControllerEvents>();
                    if (cEvents != null)
                    {
                        part.transform.GetComponent<InteractablePart>().StartPlacing(controller.transform, cEvents);
                    }
                }
            }
        }

        loadingIndicatorManager.StopLoadingIndicator();
    }

    protected IEnumerator InstantiateGameObjectAsync(PartType part, Transform parent)
    {
        // This is simply to get the elapsed time for this phase of Asset Loading.
        float startTime = Time.realtimeSinceStartup;

        // Load asset from assetBundle.
        AssetBundleLoadAssetOperation request = AssetBundleManager.LoadAssetAsync(part.AssetBundle, part.PartName[0], typeof(GameObject));
        if (request == null)
        {
            yield break;
        }
        yield return StartCoroutine(request);

        // Get the asset.
        GameObject prefab = request.GetAsset<GameObject>();

        if (prefab != null)
        {
            prefab.transform.position = new Vector3(part.PartTransform.Position.X, part.PartTransform.Position.Y, part.PartTransform.Position.Z);
            GameObject obj = Instantiate(prefab);

            // Reset object rotation (for accurate render bounds).
            obj.transform.eulerAngles = Vector3.zero;

            // Create new bounds and add the bounds of all child objects together.
            var bou = new Bounds(obj.transform.position, Vector3.zero);
            foreach (Renderer ren in obj.GetComponentsInChildren<Renderer>())
            {
                bou.Encapsulate(ren.bounds);
            }
            
            Vector3 size = bou.size;
            Vector3 rescale = obj.transform.localScale;
            Vector3 dimensions = new Vector3(part.PartTransform.Scale.X, part.PartTransform.Scale.Y, part.PartTransform.Scale.Z);

            rescale.x = dimensions.x * rescale.x / size.x;
            rescale.y = dimensions.y * rescale.y / size.y;
            rescale.z = dimensions.z * rescale.z / size.z;

            obj.transform.localScale = rescale;

            /*BoxCollider bCollider = obj.GetComponent<BoxCollider>();
            if (bCollider == null)
            {
                // Recalculate the bounds.
                bou = new Bounds(obj.transform.position, Vector3.zero);
                foreach (Renderer ren in obj.GetComponentsInChildren<Renderer>())
                {
                    bou.Encapsulate(ren.bounds);
                }
                
                bCollider = obj.AddComponent<BoxCollider>();
                bCollider.size = Vector3.Scale(bou.size,
                    new Vector3(1 / obj.transform.localScale.x, 1 / obj.transform.localScale.y, 1 / obj.transform.localScale.z));
                bCollider.center = obj.transform.InverseTransformPoint(bou.center);
            }*/

            Collider collider = obj.GetComponent<Collider>();
            if (collider == null)
            {
                switch (configMan.colliderMode)
                {
                    case ConfigurationManager.ColliderMode.Box:
                        Debug.Log("No collider detected. Generating box collder...");
                    // Recalculate the bounds.
                    bou = new Bounds(obj.transform.position, Vector3.zero);
                    foreach (Renderer ren in obj.GetComponentsInChildren<Renderer>())
                    {
                        bou.Encapsulate(ren.bounds);
                    }

                    collider = obj.AddComponent<BoxCollider>();
                    ((BoxCollider)collider).size = Vector3.Scale(bou.size,
                        new Vector3(1 / obj.transform.localScale.x, 1 / obj.transform.localScale.y, 1 / obj.transform.localScale.z));
                    ((BoxCollider)collider).center = obj.transform.InverseTransformPoint(bou.center);
                    break;

                        case ConfigurationManager.ColliderMode.NonConvex:
                            Debug.Log("No collider detected. Generating non-convex colliders...");
                    foreach (MeshFilter mesh in obj.GetComponentsInChildren<MeshFilter>())
                    {
                        NonConvexMeshCollider ncmc = mesh.gameObject.AddComponent<NonConvexMeshCollider>();
                        ncmc.boxesPerEdge = 20;
                        ncmc.Calculate();
                    }
                    break;

                        case ConfigurationManager.ColliderMode.None:
                        default:
                            Debug.Log("No collider detected. Not generating collider.");
                    break;
                }
            }

            // Add colliders to be used for raycasting.
            PartLoader.AddRaycastMeshColliders(obj);

            //EasyBuildSystem.Runtimes.Internal.Part.PartBehaviour pb = obj.AddComponent<EasyBuildSystem.Runtimes.Internal.Part.PartBehaviour>();
            //pb.ChangeState(EasyBuildSystem.Runtimes.Internal.Part.StateType.Preview);

            obj.transform.SetParent((parent == null) ? projectObjectContainer.transform : parent);

            // Get/Set all Mandatory Components.
            ApplyStandardPropertiesToPart(obj, part);

            part.transform = obj.transform;
            part.loaded = true;
        }

        // Calculate and display the elapsed time.
        float elapsedTime = Time.realtimeSinceStartup - startTime;
        if (prefab == null)
        {
            Debug.Log("[PartImportMenuManager->InstantiateGameObjectAsync] Failed to Load Part " + part.Name
                + " after " + elapsedTime + " seconds");
        }
        else
        {
            Debug.Log("[PartImportMenuManager->InstantiateGameObjectAsync] Finished Loading Part " + part.Name
                + " in " + elapsedTime + " seconds");
        }
    }

    protected IEnumerator InstantiateEmptyGameObjectAsync(PartType part, Transform parent)
    {
        // Instantiate empty game object as child of parent and position it.
        GameObject obj = new GameObject(part.Name);
        obj.transform.position = new Vector3(part.PartTransform.Position.X, part.PartTransform.Position.Y, part.PartTransform.Position.Z);
        obj.transform.rotation = Quaternion.Euler(part.PartTransform.Rotation.X, part.PartTransform.Rotation.Y, part.PartTransform.Rotation.Z);
        obj.transform.localScale = new Vector3(part.PartTransform.Scale.X, part.PartTransform.Scale.Y, part.PartTransform.Scale.Z);
        obj.transform.SetParent((parent == null) ? projectObjectContainer.transform : parent);

        // Get/Set all Mandatory Components.
        ApplyStandardPropertiesToPart(obj, part);

        part.transform = obj.transform;
        part.loaded = true;
        yield return null;
    }

    protected IEnumerator InstantiateEnclosureAsync(PartType part, Transform parent)
    {
        // This is simply to get the elapsed time for this phase of Asset Loading.
        float startTime = Time.realtimeSinceStartup;

        // Load asset from assetBundle.
        AssetBundleLoadAssetOperation request = AssetBundleManager.LoadAssetAsync(part.AssetBundle, part.Name, typeof(GameObject));
        if (request == null)
        {
            yield break;
        }
        yield return StartCoroutine(request);

        // Get the asset.
        GameObject prefab = request.GetAsset<GameObject>();

        if (prefab != null)
        {
            prefab.transform.position = new Vector3(part.PartTransform.Position.X, part.PartTransform.Position.Y, part.PartTransform.Position.Z);
            prefab.transform.rotation = Quaternion.Euler(part.PartTransform.Rotation.X, part.PartTransform.Rotation.Y, part.PartTransform.Rotation.Z);
            prefab.transform.localScale = new Vector3(part.PartTransform.Scale.X, part.PartTransform.Scale.Y, part.PartTransform.Scale.Z);
            GameObject obj = Instantiate(prefab);

            obj.transform.SetParent((parent == null) ? projectObjectContainer.transform : parent);

            // Ensure that a rigidbody is attached.
            Rigidbody rBody = obj.GetComponent<Rigidbody>();
            if (rBody == null)
            {
                rBody = obj.AddComponent<Rigidbody>();
                rBody.mass = 1;
                rBody.angularDrag = 0.99f;
                rBody.drag = 0.99f;
                rBody.useGravity = false;
                rBody.isKinematic = false;
            }

            // Set up enclosure.
            part.transform = obj.transform;
            AssemblyGrabber enclosureGrabber = obj.AddComponent<AssemblyGrabber>();
            enclosureGrabber.isGrabbable = true;
            enclosureGrabber.isUsable = false;
            //enclosureGrabber.disableWhenIdle = false;
            enclosureGrabber.disableWhenIdle = true;
            enclosureGrabber.enabled = true;
            VRTK.GrabAttachMechanics.VRTK_FixedJointGrabAttach gAttach = obj.GetComponent<VRTK.GrabAttachMechanics.VRTK_FixedJointGrabAttach>();
            if (gAttach == null)
            {
                gAttach = obj.AddComponent<VRTK.GrabAttachMechanics.VRTK_FixedJointGrabAttach>();
            }
            gAttach.precisionGrab = true;
            enclosureGrabber.assemblyRoot = parent.gameObject;
            parent.GetComponent<InteractablePart>().enclosure = part.transform.gameObject;
            if (part.EnableCollisions[0])
            {
                rBody.isKinematic = false;
                if (part.EnableGravity[0])
                {
                    rBody.useGravity = true;
                }
            }
            else
            {
                rBody.isKinematic = true;
            }

            part.transform = obj.transform;
            part.loaded = true;
        }

        // Calculate and display the elapsed time.
        float elapsedTime = Time.realtimeSinceStartup - startTime;
        if (prefab == null)
        {
            Debug.Log("[PartImportMenuManager->InitializeGameObjectAsync] Failed to Load Part " + part.Name
                + " after " + elapsedTime + " seconds");
        }
        else
        {
            Debug.Log("[PartImportMenuManager->InitializeGameObjectAsync] Finished Loading Part " + part.Name
                + " in " + elapsedTime + " seconds");
        }
    }

    protected IEnumerator InitializeAssetBundleManager()
    {
        Debug.Log("[PartImportMenuManager->InitializeAssetBundleManager] Initializing Asset Bundles...");

        // Don't destroy this gameObject as we depend on it to run the loading script.
        //DontDestroyOnLoad(gameObject);

        AssetBundleManager.SetSourceAssetBundleURL("file://" + Application.dataPath + "/StreamingAssets/");

        // Initialize AssetBundleManifest which loads the AssetBundleManifest object.
        var request = AssetBundleManager.Initialize();
        if (request != null)
        {
            yield return StartCoroutine(request);
        }
    }

    private void ApplyStandardPropertiesToPart(GameObject obj, PartType part)
    {
        // Apply default textures.
        if (part.RandomizeTexture)
        {
            int texToApply = UnityEngine.Random.Range(0, 8);
            foreach (MeshRenderer rend in obj.GetComponentsInChildren<MeshRenderer>())
            {
                rend.material = defaultPartMaterials[texToApply];
            }
        }

        if (!part.StaticAttachment)
        {
            // Get/Set all Mandatory Components.
            Rigidbody rBody = obj.GetComponent<Rigidbody>();
            if (rBody == null)
            {
                rBody = obj.AddComponent<Rigidbody>();
                rBody.mass = 1;
                rBody.angularDrag = 0.99f;
                rBody.drag = 0.99f;
                rBody.useGravity = false;
                rBody.isKinematic = false;
            }

            VRTK.GrabAttachMechanics.VRTK_ChildOfControllerGrabAttach cGrabAttach = obj.AddComponent<VRTK.GrabAttachMechanics.VRTK_ChildOfControllerGrabAttach>();
            cGrabAttach.precisionGrab = true;
            VRTK.SecondaryControllerGrabActions.VRTK_SwapControllerGrabAction sGrabAct
                = obj.AddComponent<VRTK.SecondaryControllerGrabActions.VRTK_SwapControllerGrabAction>();
            InteractablePart interactablePart = obj.AddComponent<InteractablePart>();
            interactablePart.StartAligning(VRTK.VRTK_DeviceFinder.GetControllerRightHand()); // Ugh, so bad!
            interactablePart.holdButtonToGrab = true;
            interactablePart.stayGrabbedOnTeleport = true;
            interactablePart.isGrabbable = true;
            interactablePart.isUsable = true;
            interactablePart.headsetObject = VRTK.VRTK_DeviceFinder.HeadsetTransform();
            interactablePart.partPanelPrefab = partPanelPrefab;
            interactablePart.highlightColor = partTouchHighlightColor;
            //interactablePart.disableWhenIdle = false;
            interactablePart.disableWhenIdle = true;
            interactablePart.enabled = true;
            interactablePart.grabAttachMechanicScript = cGrabAttach;
            interactablePart.secondaryGrabActionScript = sGrabAct;

            // Apply configurations.
            interactablePart.isGrabbable = part.EnableInteraction[0];
            interactablePart.isUsable = !part.NonInteractable;
            if (part.EnableCollisions[0])
            {
                rBody.isKinematic = false;
                if (part.EnableGravity[0])
                {
                    rBody.useGravity = true;
                }
            }
            else
            {
                rBody.isKinematic = true;
            }
        }
        else
        {
            InteractablePart interactablePart = obj.AddComponent<InteractablePart>();
        }
    }

    private void PlaceGrabCube(GameObject grabCube)
    {
        // Get parent and initialize grab cube at 0.
        Transform parentTransform = grabCube.transform.parent;
        grabCube.transform.localPosition = new Vector3(0, 0, 0);

        // Continuously move out grab cube until it is not encapsulated by part of the assembly.
        Collider[] assemblyColliders = parentTransform.GetComponentsInChildren<Collider>();
        foreach (Collider colliderToCheck in assemblyColliders)
        {
            if (colliderToCheck.gameObject != grabCube)
            {
                // TODO: Theoretically, if the bounds were to expand as the grab cube moved out, this loop
                // would never be exited. It is an unlikely scenario (and would probably be intentional),
                // but this should protect against that.
                while (colliderToCheck.bounds.Contains(grabCube.transform.position))
                {   // The grab cube is within an assembly object, so it will be moved further out.
                    grabCube.transform.position = new Vector3(grabCube.transform.position.x + 0.5f,
                        grabCube.transform.position.y + 0.5f, grabCube.transform.position.z + 0.5f);
                }
            }
        }
    }
}