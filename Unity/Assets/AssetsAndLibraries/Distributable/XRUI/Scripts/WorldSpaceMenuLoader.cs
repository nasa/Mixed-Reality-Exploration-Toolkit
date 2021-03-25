// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GSFC.ARVR.MRET.Infrastructure.Framework;

public class WorldSpaceMenuLoader : MonoBehaviour
{
    [Tooltip("Prefab of menu to be loaded in world space.")]
    public GameObject worldSpaceMenuPrefab;

    [Tooltip("Whether or not this menu will be parented by the controller menu.")]
    public bool childOfControllerMenu = true;

    private GameObject instantiatedMenu = null;

    public void InstantiateMenu()
    {
        InstantiateMenu_RTN();
    }

    public GameObject InstantiateMenu_RTN()
    {
        if (!childOfControllerMenu)
        {
            GameObject newMenu = Instantiate(worldSpaceMenuPrefab);
            newMenu.transform.position = transform.position + new Vector3(0, 0.25f, 0.1f); // TODO: Hacky, fix.
            newMenu.transform.rotation =
                Quaternion.LookRotation((MRET.InputRig.head.transform.position
                - newMenu.transform.position) * -1, Vector3.up);

            return newMenu;
        }

        if (worldSpaceMenuPrefab && !instantiatedMenu)
        {
            instantiatedMenu = Instantiate(worldSpaceMenuPrefab);
            instantiatedMenu.transform.position = transform.position + new Vector3(0, 0.25f, 0.1f); // TODO: Hacky, fix.
            instantiatedMenu.transform.rotation =
                Quaternion.LookRotation((MRET.InputRig.head.transform.position
                - instantiatedMenu.transform.position) * -1, Vector3.up);
        }
        else if (instantiatedMenu && !instantiatedMenu.activeSelf)
        {
            instantiatedMenu.SetActive(true);
        }

        return instantiatedMenu;
    }

    public void DestroyMenu()
    {
        Destroy(instantiatedMenu);
    }
}