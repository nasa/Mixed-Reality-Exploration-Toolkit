using UnityEngine;

public class FortyTwoOrbitalPathDisplay : MonoBehaviour
{
    public string orbitName = "Unnamed Orbit";
    public Transform orbitedObject;
    public float orbitalPathLineWidth = 0.005f;
    public Material orbitMaterial;
    [Tooltip("Time resolution for updating orbital path. Higher value -> lower resolution.")]
    [Range(1, 32)]
    public int timeResolution = 8;
    [Tooltip("Spatial resolution for updating orbital path. Higher value -> lower resolution.")]
    public float spatialResolution = 8;

    private GameObject orbitalPathHolder;
    private LineRenderer orbitLine;
    private Vector3 lastPathPoint = Vector3.negativeInfinity;
    private int delayCount = 0;

	void Start()
    {
        orbitalPathHolder = new GameObject(orbitName);
        orbitalPathHolder.transform.SetParent(orbitedObject);
        orbitalPathHolder.transform.localPosition = Vector3.zero;
        orbitLine = orbitalPathHolder.AddComponent<LineRenderer>();
        orbitLine.widthMultiplier = orbitalPathLineWidth;
        orbitLine.material = orbitMaterial;
        orbitLine.useWorldSpace = false;
        orbitLine.positionCount = 0;
	}
	
	void Update()
    {
        Vector3 currentPos = transform.position;
        if (IsSignificantChange(orbitedObject.InverseTransformPoint(currentPos)))
        {
            orbitLine.positionCount++;
            orbitLine.SetPosition(orbitLine.positionCount - 1, orbitedObject.InverseTransformPoint(currentPos));
        }
	}

    private bool IsSignificantChange(Vector3 currentPos)
    {
        if (delayCount++ > timeResolution)
        {
            if ((currentPos - lastPathPoint).magnitude >= spatialResolution)
            {
                lastPathPoint = currentPos;
                return true;
            }
            delayCount = 0;
        }
        return false;
    }
}