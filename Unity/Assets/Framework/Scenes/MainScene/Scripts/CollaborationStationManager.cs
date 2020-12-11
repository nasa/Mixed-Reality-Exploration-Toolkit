using System.Collections;
using UnityEngine;

public class CollaborationStationManager : MonoBehaviour
{
    public GameObject sharePanel, joinPanel;

	IEnumerator Start()
    {
        yield return new WaitForSeconds(1);
        sharePanel.SetActive(true);
        joinPanel.SetActive(true);
	}
}