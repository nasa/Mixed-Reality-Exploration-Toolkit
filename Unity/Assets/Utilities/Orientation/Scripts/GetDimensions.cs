using UnityEngine;

public class GetDimensions : MonoBehaviour
{
    public GameObject objToMeasure;

	void Start()
    {
        Renderer[] rends = objToMeasure.GetComponentsInChildren<Renderer>();
        if (rends.Length > 0)
        {
            float minX = rends[0].bounds.min.x,
                  minY = rends[0].bounds.min.y,
                  minZ = rends[0].bounds.min.z,
                  maxX = rends[0].bounds.max.x,
                  maxY = rends[0].bounds.max.y,
                  maxZ = rends[0].bounds.max.z;

            foreach (Renderer rend in rends)
            {
                Vector3 mins = rend.bounds.min;
                Vector3 maxs = rend.bounds.max;

                if (mins.x < minX)
                {
                    minX = mins.x;
                }

                if (mins.y < minY)
                {
                    minY = mins.y;
                }

                if (mins.z < minZ)
                {
                    minZ = mins.z;
                }

                if (maxs.x > maxX)
                {
                    maxX = maxs.x;
                }

                if (maxs.y > maxY)
                {
                    maxY = maxs.y;
                }

                if (maxs.z > maxZ)
                {
                    maxZ = maxs.z;
                }
            }

            Debug.Log("X size is " + (maxX - minX));
            Debug.Log("Y size is " + (maxY - minY));
            Debug.Log("Z size is " + (maxZ - minZ));

            Debug.Log("X rotation is " + objToMeasure.transform.rotation.x);
            Debug.Log("Y rotation is " + objToMeasure.transform.rotation.y);
            Debug.Log("Z rotation is " + objToMeasure.transform.rotation.z);
            Debug.Log("W rotation is " + objToMeasure.transform.rotation.w);

            Debug.Log("X local rotation is " + objToMeasure.transform.localRotation.x);
            Debug.Log("Y local rotation is " + objToMeasure.transform.localRotation.y);
            Debug.Log("Z local rotation is " + objToMeasure.transform.localRotation.z);
            Debug.Log("W local rotation is " + objToMeasure.transform.localRotation.w);
        }
	}
}