using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class KeyboardManager : MonoBehaviour
{
    public InputField textToControl;
    public GameObject mainKeyboard, shiftKeyboard;
    public string enteredText = "";
    public bool allowNewLine = true, allowTab = true;
    public List<Button> alphaKeys = new List<Button>();

    private bool capsOn = false;

    void Start()
    {
        CapsOff();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    #region String Management
    public void Enter()
    {
        if (allowNewLine)
        {
            AddNewLine();
        }
        else
        {
            Clear();
        }
    }

    public void AddString(string stringToAdd)
    {
        if (capsOn)
        {
            enteredText = enteredText + stringToAdd.ToUpper();
        }
        else
        {
            enteredText = enteredText + stringToAdd.ToLower();
        }
        UpdateControlledText();
    }

    public void AddTab()
    {
        if (allowTab)
        {
            enteredText = enteredText + "\t";
        }
        UpdateControlledText();
    }

    public void AddNewLine()
    {
        if (allowNewLine)
        {
            enteredText = enteredText + "\n";
        }
        UpdateControlledText();
    }

    public void DeleteCharAtPosition(int position)
    {
        if (position > -1)
        {
            enteredText.Remove(position, 1);
        }
        UpdateControlledText();
    }

    public void DeleteLastChar()
    {
        if (enteredText.Length > 0)
        {
            enteredText = enteredText.Remove(enteredText.Length - 1);
        }
        UpdateControlledText();
    }

    public void Clear()
    {
        enteredText = "";
        UpdateControlledText();
    }

    private void UpdateControlledText()
    {
        textToControl.text = enteredText;
    }
    #endregion

    #region Keyboard Management
    public void ToggleKeyboard()
    {
        if (mainKeyboard.activeSelf)
        {
            SwitchToShiftKeyboard();
        }
        else
        {
            SwitchToMainKeyboard();
        }
    }

    public void SwitchToMainKeyboard()
    {
        mainKeyboard.SetActive(true);
        shiftKeyboard.SetActive(false);
        CapsOff();
    }

    public void SwitchToShiftKeyboard()
    {
        shiftKeyboard.SetActive(true);
        mainKeyboard.SetActive(false);
        CapsOn();
    }

    public void ToggleCaps()
    {
        if (capsOn)
        {
            CapsOff();
        }
        else
        {
            CapsOn();
        }
    }

    public void KeyUp()
    {

    }

    public void KeyDown()
    {

    }

    public void KeyLeft()
    {

    }

    public void KeyRight()
    {

    }

    private void CapsOn()
    {
        capsOn = true;
        foreach (Button key in alphaKeys)
        {
            Text keyText = key.GetComponentInChildren<Text>();
            if (keyText)
            {
                keyText.text = keyText.text.ToUpper();
            }
        }
    }

    private void CapsOff()
    {
        capsOn = false;
        foreach (Button key in alphaKeys)
        {
            Text keyText = key.GetComponentInChildren<Text>();
            if (keyText)
            {
                keyText.text = keyText.text.ToLower();
            }
        }
    }
    #endregion
}