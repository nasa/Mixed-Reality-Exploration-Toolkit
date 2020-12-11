using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayID : MonoBehaviour {

    public int DisplayIndex;

    public void OpenDisplay()
    {
        Text textBox = gameObject.GetComponentInChildren<Text>();
        Int32 index = Int32.Parse(textBox.text);

        GameObject Manager = GameObject.Find("HudManager");
        List<GameObject> Display = Manager.GetComponent<HudManager>().worldDisplays;
        Display[index - 1].SetActive(true);
    }
}
