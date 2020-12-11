using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioRecorder : MonoBehaviour
{
	void Start()
    {
        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        foreach (string device in Microphone.devices)
        {
            Debug.Log(device);
        }
	}
}