// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

/*
 *  
 * This file enables interaction between the mouse and the parts/tools game objects. 
 * 
 * If left clicked and the raycast hits a tool game object, the tool will be equipped and the 
 * user can drag it around with the mouse as desired. To put it down the user can press the 
 * right click button and the tool will return to the original position.
 * 
 * When selecting a part from the parts menu, the part will automatically become equipped. The part will follow
 * the mouse cursor around until the user pressed a button and then the part will be statically set in the world space
 * position it was in when the event was registered.
 * 
 * 
 */

using UnityEngine;


public class MouseInteractableObject : MonoBehaviour
{

    //public variables
    public GameObject tool = null;
    public GameObject part = null;
    public GameObject partsPlacedGameObj;
    public GameObject partsEquippedSubMenu;
    public GameObject controller, partsSubMenu;
    public Transform guide;
    public Transform tempParent;
    public Transform parent;
    
    //private variables
    private Vector3 originalPos;
    private Vector3 mousePosition;
    private Transform part_transform;
    private bool toolEquiped;
    private bool part_equiped;
    private float zAxis = 2f;
    
    void Start()
    {
        //set event listeners and handlers 
        EventManager.OnRightClick += removeTool;
        EventManager.PartsChildrenIncreased += getPart;
        EventManager.OnLeftClick += putPart;
        //EventManager.NKeyPressed += putPart;
        EventManager.JKeyPressed += rotateitemX;
        EventManager.KKeyPressed += rotateitemY;
        EventManager.LKeyPressed += rotateitemZ;

        //set original position
        originalPos = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z);
    }

    void Update()
    {
        //if a tool is equiped move it frame by frame with the mouse
        if (toolEquiped && !part_equiped)
        {
            mousePosition = Input.mousePosition;
            mousePosition.z = 3f;
            tool.transform.position = Camera.main.ScreenToWorldPoint(mousePosition);
        }

        //if a part is equiped move it frame by frame with the mouse
        if (part_equiped)
        {

            mousePosition = Input.mousePosition;
            mousePosition.z = 3f;
            part.transform.position = Camera.main.ScreenToWorldPoint(mousePosition);

        }

    }

    //function that removes the tool equipped
    void removeTool()
    {
        if (toolEquiped && !part_equiped)
        { 
            Collider m_Collider = tool.GetComponent<Collider>();
            m_Collider.enabled = true;
            tool.GetComponent<Rigidbody>().useGravity = true;
            tool.GetComponent<Rigidbody>().isKinematic = false;
            tool.transform.SetParent(parent);
            tool.transform.position = originalPos;
            toolEquiped = false;
        }

        if(part_equiped)
        {
            Collider m_Collider = tool.GetComponent<Collider>();
            m_Collider.enabled = true;
            part.GetComponent<Rigidbody>().useGravity = false;
            part.GetComponent<Rigidbody>().isKinematic = false;
            part_equiped = false;
            toolEquiped = false;
            part = null;
        }

    }

    void rotateitemX()
    {
        if(toolEquiped && !part_equiped)
        {
            tool.transform.Rotate(20 * Time.deltaTime, 0.0f, 0.0f);
        }

        if(part_equiped)
        {

        }
    }

    void rotateitemY()
    {
        if (toolEquiped && !part_equiped)
        {
            tool.transform.Rotate(0.0f, 20 * Time.deltaTime, 0.0f);
        }

        if (part_equiped)
        {

        }
    }


    void rotateitemZ()
    {
        if (toolEquiped && !part_equiped)
        {
            tool.transform.Rotate(0.0f, 0.0f, 20 * Time.deltaTime);
        }

        if (part_equiped)
        {

        }
    }



    void OnMouseDown()
    {

        parent = tool.transform.parent;
        controller = GameObject.Find("FirstPersonCharacter");
        tempParent = controller.transform.parent;
        tool.GetComponent<Rigidbody>().useGravity = false;
        tool.GetComponent<Rigidbody>().isKinematic = true;
        tool.transform.SetParent(tempParent);
        tool.transform.localPosition = Vector3.zero;
        tool.transform.localRotation = Quaternion.identity;
        tool.transform.position = Input.mousePosition;
        controller = GameObject.Find("FPSController");
        Collider m_Collider = tool.GetComponent<Collider>();
        m_Collider.enabled = false;
        toolEquiped = true;

       


    }

    //function to get the part
    public void getPart()
    {
        Transform lastChild;
        lastChild = partsSubMenu.transform.GetChild(partsSubMenu.transform.childCount - 1);
        part = lastChild.gameObject;
        part.transform.parent = partsEquippedSubMenu.transform;
        part.transform.localPosition = Vector3.zero;
        part.transform.localRotation = Quaternion.identity;
        part.transform.position = Input.mousePosition;
        controller = GameObject.Find("FPSController");
        Collider m_Collider = part.GetComponent<Collider>();
        if (m_Collider)
        {
            m_Collider.enabled = false;
        }
        toolEquiped = true;
        part_equiped = true;

    }

    //function that places the part in world space
    void putPart()
    {
        if (part)
        {
            part.GetComponent<InteractablePart>().enabled = true;
            part.GetComponent<BoxCollider>().enabled = true;
            part.GetComponent<InteractablePart>().gameObject.SetActive(true);
            part.GetComponent<InteractablePart>().StopPlacing();
            part.transform.parent = partsPlacedGameObj.transform;
            part_equiped = false;
        }
    }

}