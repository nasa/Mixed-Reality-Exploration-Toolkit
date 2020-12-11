using UnityEngine;

public class WorldSpaceMenuManager : MonoBehaviour
{
    public void DimMenu()
    {
        gameObject.SetActive(false);
        if (VRDesktopSwitcher.isDesktopEnabled())
        {
            ControllerMenuManager cmm = FindObjectOfType<ControllerMenuManager>();
            if (cmm)
            {
                cmm.UnDimMenu();
            }
        }
        else
        {
            ControllerMenuManager leftMan = VRTK.VRTK_DeviceFinder.GetControllerLeftHand().GetComponentInChildren<ControllerMenuManager>();
            if (leftMan)
            {
                leftMan.UnDimMenu();
            }

            ControllerMenuManager rightMan = VRTK.VRTK_DeviceFinder.GetControllerRightHand().GetComponentInChildren<ControllerMenuManager>();
            if (rightMan)
            {
                rightMan.UnDimMenu();
            }
        }
    }

    public void UnDimMenu()
    {
        gameObject.SetActive(true);
    }

    private void Awake()
    {
        // If in desktop mode, remove VRTK UI Canvases and add Graphic Raycaster.
        if (VRDesktopSwitcher.isDesktopEnabled())
        {
            foreach (VRTK.VRTK_UICanvas canvas in GetComponentsInChildren<VRTK.VRTK_UICanvas>())
            {
                canvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                Destroy(canvas);
            }
        }
    }
}