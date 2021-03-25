// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

/*
 * 
 * This file handles setting menus and submenus active and inactive for the desktop version of MRET. 
 * A empty gameobject called Menus located under the FPSController is required.
 * 
 */

using UnityEngine;

public class MenuToggle : MonoBehaviour
{

    //public vars
    public GameObject controller_menu;
    public GameObject create_menu;
    public GameObject save_menu;
    public GameObject open_menu;
    public GameObject preference_menu;
    public GameObject back_to_lobby_menu;
    public GameObject part_panel;
    public GameObject menuRoot;
   
    void Start()
    {

        //event listeners and handlers to toggle menu when key is pressed
        EventManager.EscKeyPressed += closeAllMenus;
        EventManager.MKeyPressed += openMenu;

    }

    void Update()
    {

    }

    
    //function that closes all menus
    public void closeAllMenus()
    {
        foreach(Transform submenu in menuRoot.transform)
        {
            if (submenu.gameObject.activeSelf && !submenu.gameObject.name.ToString().Contains("Camera"))
                submenu.gameObject.SetActive(false);
                Debug.Log(submenu.gameObject.name.ToString());
        }        
    }

    
    //function that opens the main 'controller' menu
    public void openMenu()
    {
        controller_menu.SetActive(true);
        
        foreach (Transform child in controller_menu.transform) {
            if(!child.gameObject.activeSelf)
                child.gameObject.SetActive(true);
        }
        
    }

    //function that opens the a menu specificied (check spelling) in the unity editor
    public void openMenuByGameObject(GameObject menuToOpen)
    {  

        if (menuToOpen)
        {
            closeAllMenus();

            menuToOpen.SetActive(true);

            if(menuToOpen.transform.parent != menuRoot.transform)
                menuToOpen.transform.parent = menuRoot.transform;
        }
        else
        {
            //throw a warning if the program could not find the menu otherwise set the menu as active and set the parent to menuRoot
            Debug.LogWarning("Could not find menu to open.");
        }
    }

}
