using UnityEngine;
using System.Collections.Generic;
using GSFC.ARVR.MRET.Common.Schemas;

public class RemoveChildPanelController : MonoBehaviour
{
    public GameObject selectedObject;
    public ScrollListManager currentChildList;

    private int currentSelection = -1;
    private List<InteractablePart> currentParts;
    private Transform rootParent;

    public void Initialize()
    {
        rootParent = FindObjectOfType<UnityProject>().projectObjectContainer.transform;

        int i = 0;
        currentParts = new List<InteractablePart>();
        currentChildList.ClearScrollList();
        currentChildList.SetTitle("Children");
        foreach (InteractablePart childPart in selectedObject.GetComponentsInChildren<InteractablePart>())
        {
            if (childPart.gameObject != selectedObject && childPart.transform.parent.gameObject == selectedObject)
            {
                currentParts.Add(childPart);
                int indexToSelect = i++;
                UnityEngine.Events.UnityEvent clickEvent = new UnityEngine.Events.UnityEvent();
                clickEvent.AddListener(new UnityEngine.Events.UnityAction(() => { SetActiveSelection(indexToSelect); }));
                currentChildList.AddScrollListItem(childPart.name, clickEvent);
            }
        }
    }

    public void Remove()
    {
        currentParts[currentSelection].transform.parent = rootParent;
    }

    private void SetActiveSelection(int listID)
    {
        currentSelection = listID;
        currentChildList.HighlightItem(listID);
    }
}