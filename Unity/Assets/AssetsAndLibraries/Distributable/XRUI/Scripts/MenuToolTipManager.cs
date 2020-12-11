using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuToolTipManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string TooltipText;

    public Text InfoTextBox;

    private ControllerMenuManager controllerMenuManager;

    void Start()
    {
        if (InfoTextBox == null)
        {
            controllerMenuManager = GetComponentInParent<ControllerMenuManager>();
            InfoTextBox = controllerMenuManager.infoText;
        }
       
    }

    public void OnPointerEnter(PointerEventData eventData)
    {


        if (TooltipText != "" && InfoTextBox)
        {
            InfoTextBox.text = TooltipText;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipText != "")
        {
            if (InfoTextBox.text == TooltipText)
            {
                InfoTextBox.text = "";
            }
        }
    }
}