// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

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