using System;
using System.Collections.Generic;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;
using AssetBundles;
using GSFC.ARVR.MRET.Common.Schemas;

public class PartMenuController : MonoBehaviour
{
    public ConfigurationManager configManager;
    public Dropdown partDropdown, filterDropdown;
    public GameObject projectObjectContainer;
    public GameObject headsetFollower, controller;
    public GameObject partPanelPrefab, grabCubePrefab;
    public Color partTouchHighlightColor;

    private List<AssetInfo> availableParts = new List<AssetInfo>();
    private List<string[]> availableFilters = new List<string[]>();
    private string[] partFilter = new string[] { "", "" };

    void Start ()
    {
        SetFilterDropdownOptions();
        SetPartDropdownOptions();

        // Handle Filter Updates.
        filterDropdown.onValueChanged.AddListener(delegate
        {
            UpdateFilter();
        });

        // Handle Part Updates.
        partDropdown.onValueChanged.AddListener(delegate
        {
            LoadPart();
            partDropdown.value = 0;
        });
    }

    public void UpdateFilter()
    {
        if (filterDropdown.value == 0)
        {
            // Turn off filters for none.
            partFilter = new string[] { "", "" };
        }
        else
        {
            // Set the filter.
            partFilter = availableFilters[filterDropdown.value - 1];
        }
        SetPartDropdownOptions();
        SetFilterDropdownOptions();
    }

    public void LoadPart()
    {
        if (partDropdown.value == 0)
        {
            // Do nothing.
        }
        else
        {
            // Deserialize Part File.
            try
            {
                StartCoroutine(InitializeAssetBundleManager());

                XmlSerializer ser = new XmlSerializer(typeof(PartType));
                XmlReader reader = XmlReader.Create(availableParts[partDropdown.value - 1].assetFile);
                PartType prt = (PartType) ser.Deserialize(reader);

                StartCoroutine(InstantiateAPart(prt, null));
            }
            catch (Exception e)
            {
                Debug.Log("[UnityProject->LoadFromXML] " + e.ToString());
            }
        }

        SetFilterDropdownOptions();
        SetPartDropdownOptions();
    }

    private void SetPartDropdownOptions()
    {
        List<string> dropdownLabels = new List<string>();
        dropdownLabels.Add("Back");

        List<AssetInfo> parts = new List<AssetInfo>();
        foreach (AssetInfo part in configManager.assets)
        {
            if (part.CheckFilter(partFilter[0], partFilter[1]))
            {
                parts.Add(part);
            }
        }
        
        foreach (AssetInfo part in parts)
        {
            dropdownLabels.Add(part.name);
        }

        partDropdown.ClearOptions();
        partDropdown.AddOptions(dropdownLabels);
        availableParts = parts;
    }

    private void SetFilterDropdownOptions()
    {
        List<string> dropdownLabels = new List<string>();
        dropdownLabels.Add("None");

        List<string[]> filters = configManager.assetFilters.Get();
        foreach (string[] filter in filters)
        {
            dropdownLabels.Add(filter[0] + "==" + filter[1]);
        }

        filterDropdown.ClearOptions();
        filterDropdown.AddOptions(dropdownLabels);
        availableFilters = filters;
    }

    private IEnumerator InstantiateAPart(PartType part, Transform parent)
    {
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
                    while (!part.Enclosure.loaded)
                    {
                        yield return new WaitForSeconds(0.1f);
                    }
                }
            }

            // Instantiate grab cube.
            GameObject grabCube = Instantiate(grabCubePrefab);
            grabCube.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            grabCube.transform.SetParent(part.transform);
            PlaceGrabCube(grabCube);
            grabCube.GetComponent<AssemblyGrabber>().assemblyRoot = part.transform.gameObject;
            if (part.Enclosure != null)
            {
                grabCube.GetComponent<AssemblyGrabber>().otherGrabbers.Add(part.Enclosure.transform.GetComponent<AssemblyGrabber>());
                part.Enclosure.transform.GetComponent<AssemblyGrabber>().otherGrabbers.Add(grabCube.GetComponent<AssemblyGrabber>());
            }
            part.transform.GetComponent<InteractablePart>().grabCube = grabCube;

            // If this isn't the root object, hide the grab cube.
            if (parent != null)
            {
                grabCube.SetActive(false);
            }
        }

        if (parent == null)
        {
            // If this is the root object, start placing it.
            VRTK.VRTK_ControllerEvents cEvents = controller.GetComponent<VRTK.VRTK_ControllerEvents>();
            if (cEvents != null)
            {
                part.transform.GetComponent<InteractablePart>().StartPlacing(controller.transform, cEvents);
            }
        }
    }

    protected IEnumerator InstantiateGameObjectAsync(PartType part, Transform parent)
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

            // Get/Set all Mandatory Components.
            ApplyStandardPropertiesToPart(obj, part);

            part.transform = obj.transform;
            part.loaded = true;
        }

        // Calculate and display the elapsed time.
        float elapsedTime = Time.realtimeSinceStartup - startTime;
        if (prefab == null)
        {
            Debug.Log("[UnityProject->InitializeGameObjectAsync] Failed to Load Part " + part.Name
                + " after " + elapsedTime + " seconds");
        }
        else
        {
            Debug.Log("[UnityProject->InitializeGameObjectAsync] Finished Loading Part " + part.Name
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
            enclosureGrabber.disableWhenIdle = false;
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
            Debug.Log("[UnityProject->InitializeGameObjectAsync] Failed to Load Part " + part.Name
                + " after " + elapsedTime + " seconds");
        }
        else
        {
            Debug.Log("[UnityProject->InitializeGameObjectAsync] Finished Loading Part " + part.Name
                + " in " + elapsedTime + " seconds");
        }
    }

    protected IEnumerator InitializeAssetBundleManager()
    {
        Debug.Log("[UnityProject->InitializeAssetBundleManager] Initializing Asset Bundles...");

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
        // Get/Set all Mandatory Components.
        VRTK.GrabAttachMechanics.VRTK_ChildOfControllerGrabAttach cGrabAttach = obj.AddComponent<VRTK.GrabAttachMechanics.VRTK_ChildOfControllerGrabAttach>();
        cGrabAttach.precisionGrab = true;
        obj.AddComponent<VRTK.SecondaryControllerGrabActions.VRTK_SwapControllerGrabAction>();
        InteractablePart interactablePart = obj.AddComponent<InteractablePart>();
        interactablePart.holdButtonToGrab = true;
        interactablePart.stayGrabbedOnTeleport = true;
        interactablePart.isGrabbable = true;
        interactablePart.isUsable = true;
        interactablePart.headsetObject = VRTK.VRTK_DeviceFinder.HeadsetTransform();
        interactablePart.partPanelPrefab = partPanelPrefab;
        interactablePart.touchHighlightColor = partTouchHighlightColor;
        interactablePart.disableWhenIdle = false;
        interactablePart.enabled = true;

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
                    Debug.Log(colliderToCheck.bounds.ToString() + " | " + grabCube.transform.position);
                    grabCube.transform.position = new Vector3(grabCube.transform.position.x + 0.5f,
                        grabCube.transform.position.y + 0.5f, grabCube.transform.position.z + 0.5f);
                }
            }
        }
    }

    private void OnEnable()
    {
        SetFilterDropdownOptions();
        SetPartDropdownOptions();
    }
}