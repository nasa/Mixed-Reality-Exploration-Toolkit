// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_1;

namespace GOV.NASA.GSFC.XR.MRET.Annotation
{
    [System.Obsolete("Refer to the " + nameof(GOV.NASA.GSFC.XR.MRET.Annotation.AudioPlayer) + " class")]
    public class AudioPlayerDeprecated : MonoBehaviour
    {
        private AudioRecording audioRecording;
        private AudioSource audioSource;
        private bool isPlaying = false;

        /*void Start()
        {
            Deserialize("C:/Users/dzbaker/Documents/Unity Projects/MRET/VR_TestProj/MixedRealityEngineeringToolkit/Assets/AudioRecording.xml");
            Play();
        }*/

        public void Deserialize(string audioRecordingFilePath)
        {
            XmlSerializer ser = new XmlSerializer(typeof(AudioRecording));
            XmlReader reader = XmlReader.Create(audioRecordingFilePath);
            audioRecording = (AudioRecording)ser.Deserialize(reader);

            if (audioRecording != null)
            {
                GameObject objectToAttachTo = GameObject.Find(audioRecording.AttachTo);
                if (objectToAttachTo)
                {
                    audioSource = objectToAttachTo.AddComponent<AudioSource>();

                    StartCoroutine(LoadAudioFile(audioRecording.AudioFile));
                }
            }
        }

        public void Play()
        {
            if (audioRecording != null)
            {
                isPlaying = true;
            }
        }

        public void Pause()
        {
            if (audioRecording != null && audioSource != null)
            {
                if (audioSource.isPlaying) audioSource.Pause();
                else audioSource.UnPause();
            }
        }

        public void Stop()
        {
            if (audioRecording != null && audioSource != null)
            {
                audioSource.Stop();
            }
        }

        private IEnumerator LoadAudioFile(string path)
        {
            using (WWW www = new WWW(path))
            {
                yield return www;
                audioSource.clip = www.GetAudioClip();
            }
        }

        void Update()
        {
            if (audioSource)
            {
                if (isPlaying && !audioSource.isPlaying)
                {
                    audioSource.time = audioRecording.StartTime;
                    audioSource.PlayDelayed(0);
                    audioSource.loop = audioRecording.Loop;
                }

                if (audioSource.isPlaying)
                {
                    isPlaying = false;

                    if (audioSource.time < audioRecording.StartTime)
                    {
                        audioSource.time = audioRecording.StartTime;
                    }
                    else if (audioSource.time > audioRecording.StartTime + audioRecording.Duration)
                    {
                        if (audioRecording.Loop) audioSource.time = audioRecording.StartTime;
                        else audioSource.Stop();
                    }
                }
            }
        }
    }
}