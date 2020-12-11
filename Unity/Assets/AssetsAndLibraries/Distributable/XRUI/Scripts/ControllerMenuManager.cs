using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ControllerMenuManager : MonoBehaviour
{
    public GameObject menuObject;
    public ControllerMenuManager otherControllerMenuManager;
    public VRTK.VRTK_ControllerEvents controllerEvents;
    public VRTK.VRTK_Pointer pointer;
    public VRTK.VRTK_UIPointer uiPointer;
    public List<Button> menuButtons = new List<Button>();
    public List<GameObject> submenuPanels = new List<GameObject>();
    public Text infoText;
    public Transform rotateTowards;
    public VR_PointerMode vrPointerMode, otherControllerPointerMode;

    private int lastHighlightedSubmenuIndex = -1;
    private ColorBlock lastHighlightedSubmenuColorBlock;
    private bool isSetActive = false;

    public GameObject Rcover, Lcover; // protects buttons on side menus from being selected
    public GameObject Info;
    public Text InfoText;
    Animator anim;
    public int MenuNum = 0;
    public GameObject StartMenu, currMenu;

    public GameObject file, collab, edit, movement, controls, tools; //menu bar items

    public void ToggleMenu(bool active)
    {
        if (active)
        {
            vrPointerMode.SwitchMode(VR_PointerMode.PointerMode.UI);
        }
        else
        {
            vrPointerMode.SwitchMode(VR_PointerMode.PointerMode.Environment);

            // Destroy all world space menus that have been loaded.
            foreach (WorldSpaceMenuLoader menuLoader in transform.GetComponentsInChildren<WorldSpaceMenuLoader>(true))
            {
                menuLoader.DestroyMenu();
            }
            menuDimmed = false;
        }

        menuObject.SetActive(active);
        isSetActive = active;
    }

    public void SetActiveSubmenu(int subMenuNumber)
    {
        RevertLastActiveSubmenu();
        if (menuButtons[subMenuNumber])
        {
            lastHighlightedSubmenuIndex = subMenuNumber;
            lastHighlightedSubmenuColorBlock = menuButtons[subMenuNumber].colors;
            ColorBlock tempColors = menuButtons[subMenuNumber].colors;
            tempColors.normalColor = menuButtons[subMenuNumber].colors.pressedColor;
            menuButtons[subMenuNumber].colors = tempColors;
        }

        for (int i = 0; i < submenuPanels.Count; i++)
        {
            if (i != subMenuNumber)
            {
                submenuPanels[i].SetActive(false);
            }
        }

        if (submenuPanels[subMenuNumber])
        {
            submenuPanels[subMenuNumber].SetActive(true);
        }
    }

    public void ChangeMenuR(GameObject menu)
    {
        menu.SetActive(true);
        menuDimmed = false;
        anim.SetInteger("MenuNum", ++MenuNum);
        StartCoroutine(Delay());
        Lcover.transform.SetAsLastSibling();
        Rcover.transform.SetAsLastSibling();
        menu.transform.SetAsLastSibling();
        Info.transform.SetAsLastSibling();
        currMenu = menu;
        SetMenuBar();

        //Movement taken care of by the animation editor
    }

    public void ChangeMenuL(GameObject menu)
    {
        menu.SetActive(true);
        menuDimmed = false;
        anim.SetInteger("MenuNum", --MenuNum);
        StartCoroutine(Delay());
        Lcover.transform.SetAsLastSibling();
        Rcover.transform.SetAsLastSibling();
        menu.transform.SetAsLastSibling();
        Info.transform.SetAsLastSibling();
        currMenu = menu;
        SetMenuBar();

        //Movement taken care of by the animation editor
    }

    IEnumerator Delay()
    {
        yield return new WaitForSeconds(15f);
    }

    void SetMenuBar()
    {
        file.SetActive(true);
        collab.SetActive(true);
        edit.SetActive(true);
        movement.SetActive(true);
        controls.SetActive(true);
        tools.SetActive(true);

        //if menu selected
        if (currMenu.name == "HomeFileMenu") file.SetActive(false);
        if (currMenu.name == "Collaboration") collab.SetActive(false);
        if (currMenu.name == "EditMenu") edit.SetActive(false);
        if (currMenu.name == "MovementMenu") movement.SetActive(false);
        if (currMenu.name == "ControlsMenu") controls.SetActive(false);
        if (currMenu.name == "ToolsMenu") tools.SetActive(false);
    }

    //MenuBar buttons
    public void SelectMenuBar(int num)
    {
        InfoText.text = "";

        MenuNum = num;
        anim.SetInteger("MenuNum", MenuNum);
    }
    public void OrderMenus(GameObject menu)
    {
        menu.SetActive(true);
        menuDimmed = false;
        Lcover.transform.SetAsLastSibling();
        Rcover.transform.SetAsLastSibling();
        menu.transform.SetAsLastSibling();
        Info.transform.SetAsLastSibling();
        currMenu = menu;
        SetMenuBar();
    }

    public void OpenSubmenu(GameObject menu)
    {
        menu.SetActive(true);
        menuDimmed = false;
        menu.transform.SetAsLastSibling();
    }

    public void CloseSubmenu(GameObject menu)
    {
        menu.SetActive(false);
    }

    public void TogglePointer(bool active)
    {
        if (active)
        {
            if (pointer) pointer.Toggle(true);
            if (uiPointer) uiPointer.enabled = true;
        }
        else
        {
            if (pointer) pointer.Toggle(false);
            if (uiPointer) uiPointer.enabled = false;
        }
    }

    private bool menuDimmed = false;
    public void DimMenu()
    {
        menuObject.SetActive(false);
        menuDimmed = true;
    }

    public void UnDimMenu()
    {
        if (menuDimmed)
        {
            menuObject.SetActive(true);
            menuDimmed = false;
        }
    }

    public void UnDimMenu(GameObject menu)
    {
        if (menuDimmed) {
            menu.SetActive(true);
            menuDimmed = false;
        }
    }

    public bool IsDimmed()
    {
        return menuDimmed;
    }

    private void Start()
    {
        ToggleMenu(false);
        isSetActive = false;
        TogglePointer(false);

        if (controllerEvents) controllerEvents.ButtonTwoReleased += new VRTK.ControllerInteractionEventHandler(HandleToggleMenu);
        anim = this.GetComponent<Animator>();
        StartMenu.transform.SetAsLastSibling();
        currMenu = StartMenu;
    }

    private void HandleToggleMenu(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        if (otherControllerMenuManager)
        {
            if (otherControllerMenuManager.isSetActive)
            {   // Because VRTK is buggy! For some reason, you have to do this to actually hide the pointer.
                TogglePointer(false);
                TogglePointer(true);
                TogglePointer(false);
            }
        }

        // Turn this menu on or off, this pointer off.
        isSetActive = !isSetActive;
        StopCoroutine("TweenMenuScale");
        StartCoroutine("TweenMenuScale", isSetActive);

        // Ensure other menu is off.
        if (otherControllerMenuManager)
        {
            if (otherControllerMenuManager.isSetActive)
            {
                otherControllerMenuManager.ToggleMenu(false);
                otherControllerMenuManager.isSetActive = false;
            }
        }

        // If this menu is active, turn other pointer on.
        if (isSetActive)
        {
            if (otherControllerMenuManager) otherControllerMenuManager.TogglePointer(true);
            if (otherControllerPointerMode) otherControllerPointerMode.SwitchMode(VR_PointerMode.PointerMode.UI);
        }
        else
        {   // Don't ask why ...
            if (otherControllerMenuManager)
            {
                otherControllerMenuManager.TogglePointer(false);
                otherControllerMenuManager.TogglePointer(true);
                otherControllerMenuManager.TogglePointer(false);
            }
            if (otherControllerPointerMode) otherControllerPointerMode.SwitchMode(VR_PointerMode.PointerMode.Environment);
        }
    }

    private void RevertLastActiveSubmenu()
    {
        if (lastHighlightedSubmenuIndex != -1)
        {
            menuButtons[lastHighlightedSubmenuIndex].colors = lastHighlightedSubmenuColorBlock;
            lastHighlightedSubmenuIndex = -1;
        }
    }

    private float targetZoomScale = 1f;
    private IEnumerator TweenMenuScale(bool show)
    {
        if (show)
        {
            int i = 0;
            ToggleMenu(true);
            menuObject.transform.localScale = Vector3.zero;
            Vector3 frameDifference = new Vector3(4f, 4f, 4f);
            while (i < 250 && menuObject.transform.localScale.x < targetZoomScale)
            {
                menuObject.transform.localScale += frameDifference * Time.deltaTime * targetZoomScale;
                yield return true;
                i++;
            }
            menuObject.transform.localScale = new Vector3(targetZoomScale, targetZoomScale, targetZoomScale);
        }
        else
        {
            int i = 0;
            menuObject.transform.localScale = new Vector3(targetZoomScale, targetZoomScale, targetZoomScale);
            Vector3 frameDifference = new Vector3(4f, 4f, 4f);
            while (i < 250 && menuObject.transform.localScale.x > 0)
            {
                menuObject.transform.localScale -= frameDifference * Time.deltaTime * targetZoomScale;
                yield return true;
                i++;
            }
            menuObject.transform.localScale = Vector3.zero;
            ToggleMenu(false);
        }
        StopCoroutine("TweenMenuScale");
    }

    private void Update()
    {
        if (rotateTowards == null)
        {
            if (VRDesktopSwitcher.isVREnabled())
            {
                rotateTowards = VRTK.VRTK_DeviceFinder.HeadsetTransform();
                if (rotateTowards == null)
                {
                    //Debug.LogWarning("The Controller Menu could not automatically find an object to rotate towards.");
                }
            }
        }

        if (isSetActive)
        {
            if (rotateTowards != null)
            {
                transform.rotation = Quaternion.LookRotation((rotateTowards.position - transform.position) * -1, Vector3.up);
            }
        }
    }
}