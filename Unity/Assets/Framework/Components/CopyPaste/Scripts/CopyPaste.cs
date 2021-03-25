// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

/*
 * This is the script that can be attached to a GameObject and has the following
 * methods that can be called with an event such as a button click
 * 
 */

using UnityEngine;
using UnityEngine.UI;
using GSFC.ARVR.MRET.Components.Keyboard;

public class CopyPaste : MonoBehaviour
{

    public InputField titleField;
    public InputField textField;
    public GameObject keyboard;
    private string clipboard;
    KeyboardManager keyboardScript;

    // Start is called before the first frame update
    private void Start()
    {

        if(keyboard)
        {
            keyboardScript = keyboard.GetComponent<KeyboardManager>();
        }
           

        titleField.onValueChanged.AddListener(delegate { getTextFromNote(); });
        textField.onValueChanged.AddListener(delegate { getTextFromNote(); });
        WindowsClipboard.setField(textField);
        WindowsClipboard.setKeyboardScript(keyboardScript);

    }

    private void getTextFromNote()
    {   
        
        if (titleField.isFocused)
        {
            clipboard = titleField.text;
            WindowsClipboard.setField(titleField);
            
        }
        else if (textField.isFocused) {

            clipboard = textField.text;
            WindowsClipboard.setField(textField);

        }
        
    }
 
    public void paste()
    {
        WindowsClipboard.pasteText();
    }
    
    public void copy()
    {
        WindowsClipboard.copy();
        
    }

    public void cut()
    {
        WindowsClipboard.cut();
    } 

}
