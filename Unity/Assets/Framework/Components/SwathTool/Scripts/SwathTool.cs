// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;

public class SwathTool : MonoBehaviour
{
    public double scaleMultiplier = 0.0001;

    [Tooltip("Swath width in meters.")]
    public double swathWidth = 200000;

    [Tooltip("Size (in meters) of each step of the swath to be rendered.")]
    public double swathStepResolution = 1000;

    [Tooltip("Scale of the segment prefab.")]
    public float segmentPrefabScale = 10;

    [Tooltip("Material to apply to the swath path.")]
    public Material swathPathMaterial;

    public GameObject swathStepPrefab;

    private GameObject swathArea;
    private MeshFilter swathAreaMesh;

    private float swathSceneWidth;
    private float swathSceneHeight;

    void Start()
    {
        swathSceneWidth = (float) (swathWidth * scaleMultiplier / segmentPrefabScale);
        swathSceneHeight = (float) (swathStepResolution * scaleMultiplier / segmentPrefabScale);

        swathArea = new GameObject("SwathArea");
        swathArea.transform.SetParent(transform);
        swathArea.transform.localPosition = Vector3.zero;
        swathAreaMesh = swathArea.AddComponent<MeshFilter>();
        MeshRenderer rend = swathArea.AddComponent<MeshRenderer>();
        rend.material = swathPathMaterial;
    }

	void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
        {
            if (!hit.collider.gameObject.GetComponent<SwathSegment>())
            {
                GameObject swathSegment = Instantiate(swathStepPrefab);
                swathSegment.transform.position = hit.point;
                swathSegment.transform.localScale = new Vector3(swathSceneWidth, 1, swathSceneHeight);
                swathSegment.transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
                SetSwathSegmentParent(swathSegment, hit.collider.gameObject);
                MeshFilter filt = swathSegment.GetComponent<MeshFilter>();
                
                if (filt)
                {
                    SetSwathArea(filt.transform.TransformPoint(filt.mesh.bounds.min),
                        filt.transform.TransformPoint(filt.mesh.bounds.max));
                }
            }
        }
    }

    private void SetSwathSegmentParent(GameObject segment, GameObject parentObject)
    {
        Transform swathParent = parentObject.transform.Find("Swath");
        if (swathParent == null)
        {
            GameObject sParentObj = new GameObject("Swath");
            sParentObj.transform.SetParent(parentObject.transform);
            swathParent = sParentObj.transform;
        }

        segment.transform.SetParent(swathParent);
    }

    private void SetSwathArea(Vector3 min, Vector3 max)
    {
        // Very ugly stuff. Setting up a double-sided triangle between min, max and transform.position.
        swathAreaMesh.mesh.Clear();
        swathAreaMesh.mesh.vertices = new Vector3[] { swathArea.transform.InverseTransformPoint(transform.position),
            swathArea.transform.InverseTransformPoint(min), swathArea.transform.InverseTransformPoint(max),
            swathArea.transform.InverseTransformPoint(transform.position), swathArea.transform.InverseTransformPoint(min),
            swathArea.transform.InverseTransformPoint(max)};
        swathAreaMesh.mesh.uv = new Vector2[] { Vector2.zero, new Vector2(0, 1),
            Vector2.one, Vector2.zero, new Vector2(0, 1), Vector2.one };
        swathAreaMesh.mesh.triangles = new int[] { 0, 1, 2, 2, 1, 0 };
    }
}