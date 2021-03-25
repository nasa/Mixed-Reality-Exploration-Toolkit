// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class TextAnnotationPlayer : MonoBehaviour
{
    public TextAnnotation textAnnotation;
    public Text textObject;
    private bool isValid = false, isPlaying = false, isPaused = false, started = false;
    private int currentTextIndex = 0;
    private System.Diagnostics.Stopwatch ticker = new System.Diagnostics.Stopwatch();

    /*void Start()
    {
        ticker.Start();

        Deserialize("C:/Users/dzbaker/Documents/Unity Projects/MRET/VR_TestProj/MixedRealityEngineeringToolkit/Assets/TextAnnotation.xml");
        Play();
	}*/

    public void Deserialize(string textAnnotationFilePath)
    {
        XmlSerializer ser = new XmlSerializer(typeof(TextAnnotation));
        XmlReader reader = XmlReader.Create(textAnnotationFilePath);
        textAnnotation = (TextAnnotation) ser.Deserialize(reader);

        if (textAnnotation != null)
        {
            GameObject objectToAttachTo = GameObject.Find("LoadedProject/GameObjects/" + textAnnotation.AttachTo);
            if (objectToAttachTo)
            {
                textObject = objectToAttachTo.GetComponent<Text>();
                if (!textObject)
                {
                    textObject = objectToAttachTo.AddComponent<Text>();
                }
            }
        }

        isValid = true;
        if (textAnnotation.Texts == null)
        {
            isValid = false;
        }
        else if (textAnnotation.Texts.Texts == null)
        {
            isValid = false;
        }
        else if (textAnnotation.Texts.Texts.Length < 1)
        {
            isValid = false;
        }
    }

    public void Play()
    {
        if (textAnnotation != null)
        {
            if (isPaused)
            {
                Pause();
            }

            ticker.Start();
            isPlaying = true;
        }
    }

    public void Pause()
    {
        if (!isPaused)
        {
            isPaused = true;
            ticker.Stop();
        }
        else
        {
            ticker.Start();
            isPaused = false;
        }
    }

    public void Stop()
    {
        isPlaying = false;
        started = false;
        currentTextIndex = 0;
        ticker.Reset();
    }

    public void GoTo(int TextToGoTo)
    {
        if (isPlaying && TextToGoTo < textAnnotation.Texts.Texts.Length)
        {
            currentTextIndex = TextToGoTo;
        }
    }

    void Update()
    {
        if (isPlaying && isValid)
        {
            if (!isPaused)
            {
                if (TimeToSwitchText())
                {
                    textObject.text = textAnnotation.Texts.Texts[currentTextIndex++];

                    if (currentTextIndex >= textAnnotation.Texts.Texts.Length)
                    {
                        currentTextIndex = 0;

                        if (!textAnnotation.Loop)
                        {
                            Stop();
                        }
                    }
                }
            }
        }
    }
    
    bool TimeToSwitchText()
    {
        if (!started)
        {
            started = true;
            return true;
        }

        if (ticker.ElapsedMilliseconds / 1000 >= textAnnotation.TimePerText)
        {
            ticker.Reset();
            ticker.Start();
            return true;
        }
        return false;
    }
}