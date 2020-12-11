/*
 * 
 * This file encapsulates all keyboard and mouse events using c# delegates.
 * It provides developers the functionality to link keyboard and mouse events with 
 * functions using only one line of code.
 * 
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{

    //mouse events
    public delegate void MouseAction();
    public static event MouseAction OnLeftClick;
    public static event MouseAction OnLeftClickUp;
    public static event MouseAction OnRightClick;
    public static event MouseAction OnRightClickUp;
    public static event MouseAction PositiveScrollWheel;
    public static event MouseAction NegativeScrollWheel;


    //keyboard events
    public delegate void KeyboardAction();
    public static event KeyboardAction VKeyPressed;
    public static event KeyboardAction MKeyPressed;
    public static event KeyboardAction EscKeyPressed;
    public static event KeyboardAction ZKeyPressed;
    public static event KeyboardAction XKeyPressed;
    public static event KeyboardAction JKeyPressed;
    public static event KeyboardAction KKeyPressed;
    public static event KeyboardAction LKeyPressed;
    public static event KeyboardAction NKeyPressed;
    public static event KeyboardAction BKeyPressed;
    public static event KeyboardAction IKeyPressed;

    public static event KeyboardAction TKeyPressed;
    public static event KeyboardAction GKeyPressed;
    public static event KeyboardAction FKeyPressed;
    public static event KeyboardAction HKeyPressed;


    //value change events
    public delegate void ValueChanged();
    public static event ValueChanged PartsChildrenIncreased;
    public static event ValueChanged ToolsChildrenIncreased;

    public GameObject partsSubMenu;
    public GameObject partsPlacedSubMenu;
    public GameObject note;
    private int numberOfPartsChildren;
    private int newNumberOfPartsChildren;

    public GameObject toolsSubMenu;
    private int numberOfToolsChildren;


    //change this
    private bool foundPartsMenu = true;
    private bool foundToolsMenu = true;

    private static bool typing = false;

    private void Start()
    {
       
    }

    void Update()
    {
        
            //mouse events


            if (Input.GetMouseButtonDown(0))
            {
                if (OnLeftClick != null)
                    OnLeftClick();
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (OnLeftClickUp != null)
                    OnLeftClickUp();
            }

            if (Input.GetMouseButtonDown(1))
            {
                OnRightClick();
            }

            if (Input.GetMouseButtonUp(1))
            {
                OnRightClickUp();
            }

            if (Input.GetAxis("Mouse ScrollWheel") > 0)
            {
                if (PositiveScrollWheel != null)
                    PositiveScrollWheel();
            }

            if (Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                if (NegativeScrollWheel != null)
                    NegativeScrollWheel();
            }

            //keyboard events

            if (Input.GetKeyDown(KeyCode.V))
            {
                if (VKeyPressed != null)
                    VKeyPressed();
            }

            if (Input.GetKeyDown(KeyCode.M))
            {
                if (MKeyPressed != null)
                    MKeyPressed();
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (EscKeyPressed != null)
                    EscKeyPressed();
            }

            if(Input.GetKeyDown(KeyCode.N))
            {
                if (NKeyPressed != null)
                    NKeyPressed();
            }

            if (Input.GetKeyDown(KeyCode.N))
            {
                if (NKeyPressed != null)
                    NKeyPressed();
            }

            if (Input.GetKeyDown(KeyCode.I))
            {
                if (IKeyPressed != null)
                    IKeyPressed();
            }

            if (Input.GetKeyDown(KeyCode.Z))
            {
                if (ZKeyPressed != null)
                    ZKeyPressed();
            }

            //notice get key func instead of get key down
            if (Input.GetKey(KeyCode.J))
            {
                if (JKeyPressed != null)
                    JKeyPressed();
            }
            if (Input.GetKey(KeyCode.K))
            {
                if (KKeyPressed != null)
                    KKeyPressed();
            }

            if (Input.GetKey(KeyCode.T))
            {
                if (TKeyPressed != null)
                    TKeyPressed();
            }

            if (Input.GetKey(KeyCode.G))
            {
                if (GKeyPressed != null)
                    GKeyPressed();
            }

            if (Input.GetKey(KeyCode.F))
            {
                if (FKeyPressed != null)
                    FKeyPressed();
            }

            if (Input.GetKey(KeyCode.H))
            {
                if (HKeyPressed != null)
                    HKeyPressed();
            }

            if (Input.GetKey(KeyCode.L))
            {
                if (LKeyPressed != null)
                    LKeyPressed();
            }


            if (Input.GetKeyDown(KeyCode.X))
            {
                if (XKeyPressed != null)
                    XKeyPressed();
            }

            //change name
            if (partsSubMenu)
            {
                int newNumberOfPartsEquippedChildren = partsSubMenu.transform.childCount;

                if (newNumberOfPartsEquippedChildren > 0)
                {
                    PartsChildrenIncreased();
                    numberOfPartsChildren = newNumberOfPartsChildren;
                }
            }
            

            if(toolsSubMenu)
            {
                int newNumberOfToolsChildren = toolsSubMenu.transform.childCount;

                if (newNumberOfToolsChildren > numberOfToolsChildren)
                {
                    if (ToolsChildrenIncreased != null)
                    {
                        ToolsChildrenIncreased();
                        numberOfToolsChildren = newNumberOfToolsChildren;
                    }
                }
            }
            
        
        

    }

    public static void setTyping(bool isTyping)
    {
        typing = isTyping;
        
    }

    public static bool getTyping()
    {
        return typing;
    }

}
