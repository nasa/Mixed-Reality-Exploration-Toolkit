using UnityEngine;

public class ViewportOffset : MonoBehaviour
{

    public GameObject viewportToOffset;

    public void OffsetViewportHorizontal(int amount)
    {
        if (viewportToOffset)
        {
            Vector3 vPos = viewportToOffset.transform.localPosition;
            viewportToOffset.transform.localPosition = new Vector3(vPos.x + amount, vPos.y, vPos.z);
        }
    }

    public void OffsetViewportVertical(int amount)
    {
        if (viewportToOffset)
        {
            Vector3 vPos = viewportToOffset.transform.localPosition;
            viewportToOffset.transform.localPosition = new Vector3(vPos.x, vPos.y + amount, vPos.z);
        }
    }
}