/*
 * 
 *  This files handles switching between VR and desktop modes using the SDKSwitcher.
 *  It contains mostly static vars and functions because there should not be multiple
 *  instances. It behaves similarly to a singleton class. It sets VR enables when you click none
 *  on the SDKSwitcher dropdown. It will set the game objects needed for Desktop active.
 *  
 *  If you do not click None, the program defaults to VR so you will be in VR mode until you click None.
 *  
 *  This file provides developers with functions to get the current platform the game is
 *  running on (e.g. Desktop or VR).
 * 
 * 
 */

using UnityEngine;

public class VRDesktopSwitcher : MonoBehaviour
{
    public bool VREnabledByDefault = true;

    //public static vars
    public static GameObject FPSControl;
    public static GameObject SDKMenu;
    public static GameObject FPC;

    //private static vars
    private static bool desktopEnabled;
    private static bool VREnabled;
    private static bool initialized = false;

    // Start is called before the first frame update
    void Start()
    {
#if MRET_DESKTOP_MODE
        EnableDesktop();
#else
        EnableVR();
#endif
    }

    private static void EnableVR()
    {
        desktopEnabled = false;
        VREnabled = true;
    }

    private static void EnableDesktop()
    {
        desktopEnabled = true;

        if (VREnabled)
            VREnabled = false;

        FPSControl = GameObject.Find("DesktopComponents");

        foreach (Transform child in FPSControl.transform)
            if (!child.gameObject.activeSelf)
                child.gameObject.SetActive(true);
        FPSControl.SetActive(true);

        SDKMenu = GameObject.Find("SDKSetupSwitcher");

        if (SDKMenu)
        {
            foreach (Transform child in SDKMenu.transform)
            {
                if (child.gameObject.activeSelf)
                {
                    if (child.GetComponent<Camera>().isActiveAndEnabled)
                    {
                        child.gameObject.SetActive(false);
                    }
                }
            }
        }
    }

    //private function that toggles off the SDK Menu and it's children
    private static void toggleOffSDKMenus()
    {
        foreach (Transform child in SDKMenu.transform)
        {
            if (child.gameObject.activeSelf)
                child.gameObject.SetActive(false);
        }
        SDKMenu.SetActive(false);
    }

    //getter to check if VR is enabled
    public static bool isVREnabled()
    {
#if MRET_DESKTOP_MODE
        return false;
#else
        return true;
#endif
    }

    //getter to check if desktop is enabled
    public static bool isDesktopEnabled()
    {
#if MRET_DESKTOP_MODE
        return true;
#else
        return false;
#endif
    }
}