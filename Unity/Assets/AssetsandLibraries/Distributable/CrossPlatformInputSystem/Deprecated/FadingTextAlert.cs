// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

/*
 * 
 * This a simple file that enables text alerts to inform the user that certain features
 * are only availible in VR. It uses the same textbox used to display text
 * when a menu button is highlighted. You can set the warning text in the inspector
 * and it will be show when the button is pressed for a few moments and then
 * go away.
 * 
 */ 


using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GOV.NASA.GSFC.XR.CrossPlatformInputSystem.Legacy
{
    public class FadingTextAlert : MonoBehaviour
    {
        //public vars
        public Text textBox;

        //function to show the warning - takes a string (you set it in the editor ) as the sole arguement and will display this string as a warning
        public void showWarning(string warning)
        {
            Toggle toggle = GetComponent<Toggle>();

            if (toggle && toggle.isOn)
            {
                textBox.text = warning;
                textBox.gameObject.SetActive(true);
                StartCoroutine(DisableWarning());
            }

        }

        //IEnumerator function to disable the warning after a few seconds
        IEnumerator DisableWarning()
        {
            yield return new WaitForSeconds(4);
            textBox.gameObject.SetActive(false);
        }

    }
}