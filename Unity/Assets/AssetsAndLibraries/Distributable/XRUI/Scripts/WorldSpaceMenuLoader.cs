using UnityEngine;

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

            if (VRDesktopSwitcher.isDesktopEnabled())
            {
                GameObject menuRoot = GameObject.Find("Menus");
                newMenu.transform.parent = menuRoot.transform;
                Vector3 newPos = new Vector3(0f, 0f, 0f);
                newMenu.transform.position = transform.position;
                Quaternion rot = new Quaternion(0.0f, 0.0f, 0.0f, 0.0f);
                newMenu.transform.rotation = rot;
            }
            else
            {
                newMenu.transform.position = transform.position;
                newMenu.transform.rotation =
                    Quaternion.LookRotation((VRTK.VRTK_DeviceFinder.HeadsetTransform().position
                    - newMenu.transform.position) * -1, Vector3.up);
            }
            return newMenu;
        }

        if (worldSpaceMenuPrefab && !instantiatedMenu)
        {
            instantiatedMenu = Instantiate(worldSpaceMenuPrefab);

            if (VRDesktopSwitcher.isDesktopEnabled())
            {
                GameObject menuRoot = GameObject.Find("Menus");

                instantiatedMenu.transform.parent = menuRoot.transform;
                Vector3 newPos = new Vector3(0f, 0f, 0f);
                instantiatedMenu.transform.position = transform.position;
                Quaternion rot = new Quaternion(0.0f, 0.0f, 0.0f, 0.0f);
                instantiatedMenu.transform.rotation = rot;

            }
            else
            {
                instantiatedMenu.transform.position = transform.position;
                instantiatedMenu.transform.rotation =
                    Quaternion.LookRotation((VRTK.VRTK_DeviceFinder.HeadsetTransform().position
                    - instantiatedMenu.transform.position) * -1, Vector3.up);
            }
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