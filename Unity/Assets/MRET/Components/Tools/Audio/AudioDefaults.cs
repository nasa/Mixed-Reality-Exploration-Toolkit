// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

namespace GOV.NASA.GSFC.XR.MRET.Tools.Audio
{
    public class AudioDefaults : MonoBehaviour
    {

        public static AudioDefaults instance = null;     //Allows other scripts to call functions from AudioDefaults.

        public AudioClip defaultDetachClip;
        public AudioClip defaultAttachClip;

        void Awake()
        {
            if (instance == null)
                instance = this;
            else if (instance != this)
                Destroy(gameObject);

            DontDestroyOnLoad(gameObject);
        }
    }
}