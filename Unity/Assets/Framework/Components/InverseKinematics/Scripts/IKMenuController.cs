using UnityEngine;
using UnityEngine.UI;

public class IKMenuController : MonoBehaviour
{
    public Dropdown ikModeDropdown;
    public IKInteractionManager leftInteractionManager, rightInteractionManager;
    public MatlabIKInteractionManager leftMatlabInteractionManager, rightMatlabInteractionManager;

    private ControlMode controlMode;
    private bool initializingDropdown = true;

	void Start()
    {
        leftInteractionManager = VRTK.VRTK_DeviceFinder.GetControllerLeftHand().GetComponentInChildren<IKInteractionManager>();
        rightInteractionManager = VRTK.VRTK_DeviceFinder.GetControllerRightHand().GetComponentInChildren<IKInteractionManager>();
        leftMatlabInteractionManager = VRTK.VRTK_DeviceFinder.GetControllerLeftHand().GetComponentInChildren<MatlabIKInteractionManager>();
        rightMatlabInteractionManager = VRTK.VRTK_DeviceFinder.GetControllerRightHand().GetComponentInChildren<MatlabIKInteractionManager>();
        controlMode = FindObjectOfType<ControlMode>();

        if (leftInteractionManager.enabled == true && rightInteractionManager.enabled == true)
        {
            initializingDropdown = true;
            ikModeDropdown.value = 1;
        }
        else if (leftMatlabInteractionManager.enabled == true && rightMatlabInteractionManager.enabled == true)
        {
            initializingDropdown = true;
            ikModeDropdown.value = 2;
        }
        else
        {
            initializingDropdown = true;
            ikModeDropdown.value = 0;
        }

        // Handle IK Mode Updates.
        ikModeDropdown.onValueChanged.AddListener(delegate
        {
            if (!initializingDropdown)
            {
                HandleIKModeChange();
            }
            initializingDropdown = false;
        });
    }

    public void HandleIKModeChange()
    {
        switch (ikModeDropdown.value)
        {
            // Off.
            case 0:
                leftInteractionManager.enabled = rightInteractionManager.enabled = false;
                    controlMode.DisableAllControlTypes();
                break;
            
            // Low-Quality.
            case 1:
                leftInteractionManager.enabled = rightInteractionManager.enabled = true;
                leftMatlabInteractionManager.enabled = rightMatlabInteractionManager.enabled = false;
                controlMode.EnterInverseKinematicsMode();
                break;
            
            // Matlab.
            case 2:
                leftInteractionManager.enabled = rightInteractionManager.enabled = false;
                leftMatlabInteractionManager.enabled = rightMatlabInteractionManager.enabled = true;
                controlMode.EnterInverseKinematicsMode();
                break;
            
            // Unknown.
            default:
                leftInteractionManager.enabled = rightInteractionManager.enabled = false;
                leftMatlabInteractionManager.enabled = rightMatlabInteractionManager.enabled = false;
                controlMode.DisableAllControlTypes();
                break;
        }
    }
}