using UnityEngine;
using UnityEngine.UI;

public class RulerMenuController : MonoBehaviour
{
    public GameObject siRuler, imperialRuler;
    public Toggle siRulerToggle, imperialRulerToggle, noRulerToggle;
    public ControlMode controlMode;
    public RulerMenuController otherRulers;
    public GameObject menuPanel;
    public bool mostRecent = false;

    private int toggleCountDown = 0;
    private bool needToReinitializeToggles = false;

	public void Start()
    {
        ExitMode();
	}

    public void Update()
    {
        if (toggleCountDown > 0)
        {
            toggleCountDown--;
            if (toggleCountDown == 0 )
            {
                if (needToReinitializeToggles)
                {
                    if (otherRulers.siRulerToggle.isOn)
                    {
                        siRulerToggle.isOn = true;
                    }
                    else if (otherRulers.imperialRulerToggle.isOn)
                    {
                        imperialRulerToggle.isOn = true;
                    }
                    else
                    {
                        noRulerToggle.isOn = true;
                    }
                    needToReinitializeToggles = false;
                }
                else
                {
                    if (siRuler.activeSelf)
                    {
                        siRulerToggle.isOn = true;
                    }
                    else if (imperialRuler.activeSelf)
                    {
                        imperialRulerToggle.isOn = true;
                    }
                    else
                    {
                        noRulerToggle.isOn = true;
                    }
                }
            }
        }
    }

    public void OnEnable()
    {
        toggleCountDown = 3;
        if (otherRulers.mostRecent)
        {
            needToReinitializeToggles = true;
        }
        else
        {
            needToReinitializeToggles = false;
        }
        mostRecent = true;
        otherRulers.mostRecent = false;
    }

    public void OnDisable()
    {
        toggleCountDown = -1;
    }

    public void DisableAllRulers()
    {
        if (!menuPanel.activeInHierarchy || toggleCountDown > 0)
        {
            return;
        }
        if (siRuler.activeSelf || imperialRuler.activeSelf)
        {
            if (noRulerToggle.isOn)
            {
                siRuler.SetActive(false);
                imperialRuler.SetActive(false);
                controlMode.DisableAllControlTypes();
            }
        }
    }

    // Exit ruler without setting the global control mode.
    public void ExitMode()
    {
        siRuler.SetActive(false);
        imperialRuler.SetActive(false);
        noRulerToggle.isOn = true;
    }

    public void EnableSIRuler()
    {
        if (siRulerToggle.isOn && toggleCountDown == 0)
        {
            otherRulers.ExitMode();
            siRuler.SetActive(true);
            imperialRuler.SetActive(false);
            controlMode.EnterRulerMode();
        }
    }

    public void EnableImperialRuler()
    {
        if (imperialRulerToggle.isOn && toggleCountDown == 0)
        {
            otherRulers.ExitMode();
            siRuler.SetActive(false);
            imperialRuler.SetActive(true);
            controlMode.EnterRulerMode();
        }
    }
}