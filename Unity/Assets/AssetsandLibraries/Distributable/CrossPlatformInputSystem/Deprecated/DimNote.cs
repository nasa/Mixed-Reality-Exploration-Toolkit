// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

/*
 * 
 * This is a simple class used to dim and undim the note panel for better use of the program
 * 
 * 
 */
using UnityEngine;

namespace GOV.NASA.GSFC.XR.CrossPlatformInputSystem.Legacy
{
    public class DimNote : MonoBehaviour
    {

        public GameObject note;

        // Start is called before the first frame update
        void Start()
        {
            EventManager.BKeyPressed += dimNote;
            EventManager.NKeyPressed += unDimNote;
        }

        //dim note
        void dimNote()
        {
            note.SetActive(false);
            EventManager.setTyping(false);
        }

        //undim note
        void unDimNote()
        {
            note.SetActive(true);
            EventManager.setTyping(true);
        }


    }
}