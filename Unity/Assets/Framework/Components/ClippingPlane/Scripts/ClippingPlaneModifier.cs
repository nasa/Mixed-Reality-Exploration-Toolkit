using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClippingPlaneModifier : MonoBehaviour
{
    public Text nearInputField;
    public Text farInputField;
    public Text nearInputFieldPlaceholder;
    public Text farInputFieldPlaceholder;
    public Transform camParent;

    private Camera cam;
    private bool valuesChanged = false;

    // Start is called before the first frame update
    void Start()
    {
        camParent = GameObject.Find("Camera (eye)").transform;
        cam = camParent.GetComponent<Camera>();
        updateLabels(cam.nearClipPlane.ToString(), cam.farClipPlane.ToString());   
    }

    // Update is called once per frame
    void Update()
    {
        if (valuesChanged)
        {
            updateLabels(cam.nearClipPlane.ToString(), cam.farClipPlane.ToString());
            valuesChanged = false;
        }   
    }

    private void updateLabels(float nearValue, float farValue)
    {
        nearInputField.GetComponent<Text>().text = nearValue.ToString();
        farInputField.GetComponent<Text>().text = farValue.ToString();
    }
    
    private void updateLabels(string nearValue, string farValue)
    {
        nearInputField.GetComponent<Text>().text = nearValue;
        farInputField.GetComponent<Text>().text = farValue;
        nearInputFieldPlaceholder.GetComponent<Text>().text = nearValue;
        farInputFieldPlaceholder.GetComponent<Text>().text = farValue;
    }

    private void changeNearClippingPlane(float newValue)
    {
        cam.nearClipPlane = newValue;
    }

    private void changeFarClippingPlane(float newValue)
    {
        cam.farClipPlane = newValue;
        
    }

    public void modifyClippingPlane()
    {
        float nearValue = float.Parse(nearInputField.text.ToString());
        float farValue = float.Parse(farInputField.text.ToString());

        if(farValue > nearValue)
        {
            changeFarClippingPlane(farValue);
            changeNearClippingPlane(nearValue);
            updateLabels(nearValue, farValue);
            valuesChanged = true;
        } else
        {
            Debug.LogWarning("Far value cannot be less than near value.");
        }
    }
    
}
