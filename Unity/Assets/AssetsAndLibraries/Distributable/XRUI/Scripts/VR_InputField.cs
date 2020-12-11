using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class VR_InputField : InputField
{
    private GameObject virtualKeyboard = null;

    public void SetKeyboard(GameObject keyboardToUse)
    {
        virtualKeyboard = keyboardToUse;

        KeyboardManager manager = virtualKeyboard.GetComponent<KeyboardManager>();
        manager.textToControl = this;
    }

    protected override void Start()
    {
        base.Start();
        if (!virtualKeyboard)
        {
            KeyboardManager manager = GetComponentInChildren<KeyboardManager>(true);
            if (manager)
            {
                virtualKeyboard = manager.gameObject;
                manager.textToControl = this;
            }
        }
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);

        ShowKeyboard();
    }

    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);

        ShowKeyboard();
    }

    private void ShowKeyboard()
    {
        if (virtualKeyboard)
        {
            virtualKeyboard.SetActive(true);
        }
    }
}