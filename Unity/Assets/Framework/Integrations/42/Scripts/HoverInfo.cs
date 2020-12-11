using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoverInfo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public Text InfoTextBox;
    private GameObject childText = null;

    private FortyTwoMenuController fortyTwoMenuController;

    void Start()
    {
        if(InfoTextBox != null)
        {
            childText = InfoTextBox.gameObject;
            childText.SetActive(false);
        }

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        childText.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        childText.SetActive(false);
    }
}