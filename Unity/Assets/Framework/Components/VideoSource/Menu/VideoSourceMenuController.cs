using UnityEngine;
using UnityEngine.UI;

public class VideoSourceMenuController : MonoBehaviour
{
    public Toggle leftToggle, rightToggle;
    public GameObject leftScreen, rightScreen;

    private int toggleCountDown = 0, leftToggleCountDown = 0, rightToggleCountDown = 0;
    private bool menuJustToggled = false;

    public void Update()
    {
        if (toggleCountDown > 0)
        {
            toggleCountDown--;
            if (toggleCountDown == 0)
            {
                SwitchLeftScreen(leftScreen.activeSelf);
                SwitchRightScreen(rightScreen.activeSelf);
            }
        }

        if (leftToggleCountDown > 0)
        {
            leftToggleCountDown--;
            if (leftToggleCountDown == 0)
            {
                SwitchLeftScreen(!leftScreen.activeSelf);
            }
        }

        if (rightToggleCountDown > 0)
        {
            rightToggleCountDown--;
            if (rightToggleCountDown == 0)
            {
                SwitchRightScreen(!rightScreen.activeSelf);
            }
        }
    }

    public void OnEnable()
    {
        toggleCountDown = 3;
    }

    public void OnDisable()
    {
        toggleCountDown = 3;
    }

    public void ToggleLeftScreen()
    {
        if (toggleCountDown == 0)
        {
            leftToggleCountDown = 7;
        }
    }

    public void ToggleRightScreen()
    {
        if (toggleCountDown == 0)
        {
            Debug.Log("toggling");
            rightToggleCountDown = 7;
        }
    }

    public void SwitchLeftScreen(bool on)
    {
        if (on)
        {
            leftToggle.isOn = true;
            leftScreen.SetActive(true);
        }
        else
        {
            leftToggle.isOn = false;
            leftScreen.SetActive(false);
        }
    }

    public void SwitchRightScreen(bool on)
    {
        if (on)
        {
            rightToggle.isOn = true;
            rightScreen.SetActive(true);
        }
        else
        {
            rightToggle.isOn = false;
            rightScreen.SetActive(false);
        }
    }
}