// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

namespace GOV.NASA.GSFC.XR.MRET.Tools.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class InstrumentAudioControl : MonoBehaviour
    {
        public AudioClip myDetachClip;
        public AudioClip myAttachClip;

        private AudioSource source;
        float nextSoundTime = 0;

        void Start()
        {
            source = GetComponent<AudioSource>();
            if (myDetachClip == null)
                myDetachClip = AudioDefaults.instance.defaultDetachClip;

            if (myAttachClip == null)
                myAttachClip = AudioDefaults.instance.defaultAttachClip;
        }

        public void PlayDetach()
        {
            if (UnityEngine.Time.time >= nextSoundTime)
            {
                source.clip = myDetachClip;
                source.Play();
                nextSoundTime = UnityEngine.Time.time + 0.1f;
                return;
            }
        }

        public void PlayAttach()
        {
            if (UnityEngine.Time.time >= nextSoundTime)
            {
                source.clip = myAttachClip;
                source.Play();
                nextSoundTime = UnityEngine.Time.time + 0.1f;
                return;
            }
        }
    }
}