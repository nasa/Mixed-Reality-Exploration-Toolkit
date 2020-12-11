using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditSubMenu : MonoBehaviour {

    public HudMenu hudMenu;

    public void OnEnable()
    {
        hudMenu.miniHUD.SetActive(true);
        hudMenu.GetHud();

    }


    public void OnDisable()
    {
        hudMenu.SaveHud();
        hudMenu.miniHUD.SetActive(false);
    }

}
