using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Screen_correction : MonoBehaviour
{
    // Start is called before the first frame update

    void Start()
    {
        Screen.SetResolution((int)Screen.width, (int)Screen.height, true);
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
