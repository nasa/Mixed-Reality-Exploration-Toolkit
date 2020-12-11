﻿using UnityEngine;

// This class is designed to handle context awareness for the various control modes.
public class ControlMode : MonoBehaviour
{
    public enum ControlType { None, Camera, Drawing, Ruler, DualAxisRotation, InverseKinematics, Notes, Eraser };

    public ControlType activeControlType = ControlType.None;
    public CameraMenuController leftCameraMenuController, rightCameraMenuController;
    public DrawLineManager drawLineManager;
    public RulerMenuController leftRulerMenuController, rightRulerMenuController;
    public DualAxisRotationControl leftDualAxisRotationControl, rightDualAxisRotationControl;
    public IKInteractionManager leftIKManager, rightIKManager;
    public NotesMenuController leftNotesMenuController, rightNotesMenuController;
    public EraserMenuController leftEraserMenuController, rightEraserMenuController;

    public void EnterCameraMode()
    {
        DisableAllControlTypesExcept(ControlType.Camera);
        activeControlType = ControlType.Camera;
    }

	public void EnterDrawingMode()
    {
        DisableAllControlTypesExcept(ControlType.Drawing);
        activeControlType = ControlType.Drawing;
    }

    public void EnterRulerMode()
    {
        DisableAllControlTypesExcept(ControlType.Ruler);
        activeControlType = ControlType.Ruler;
    }

    public void EnterDualAxisRotationMode()
    {
        DisableAllControlTypesExcept(ControlType.DualAxisRotation);
        activeControlType = ControlType.DualAxisRotation;
    }

    public void EnterInverseKinematicsMode()
    {
        DisableAllControlTypesExcept(ControlType.InverseKinematics);
        activeControlType = ControlType.InverseKinematics;
    }

    public void EnterNotesMode()
    {
        DisableAllControlTypesExcept(ControlType.Notes);
        activeControlType = ControlType.Notes;
    }

    public void EnterEraserMode()
    {
        DisableAllControlTypesExcept(ControlType.Eraser);
        activeControlType = ControlType.Eraser;
    }

    public void DisableAllControlTypes()
    {
        activeControlType = ControlType.None;
        leftCameraMenuController.ExitMode();
        if (rightCameraMenuController) rightCameraMenuController.ExitMode();
        drawLineManager.ExitMode();
        leftRulerMenuController.ExitMode();
        if (rightRulerMenuController) rightRulerMenuController.ExitMode();
        if (leftDualAxisRotationControl) leftDualAxisRotationControl.ExitMode();
        if (rightDualAxisRotationControl) rightDualAxisRotationControl.ExitMode();
        if (leftIKManager) leftIKManager.enabled = false;
        if (rightIKManager) rightIKManager.enabled = false;
        leftNotesMenuController.ExitMode();
        if (rightNotesMenuController) rightNotesMenuController.ExitMode();
        leftEraserMenuController.ExitMode();
        if (rightEraserMenuController) rightEraserMenuController.ExitMode();
    }

    public void DisableAllControlTypesExcept(ControlType modeToKeep)
    {
        if (modeToKeep != ControlType.Camera)
        {
            leftCameraMenuController.ExitMode();
            if (rightCameraMenuController) rightCameraMenuController.ExitMode();
        }

        if (modeToKeep != ControlType.Drawing)
        {
            drawLineManager.ExitMode();
        }

        if (modeToKeep != ControlType.Ruler)
        {
            leftRulerMenuController.ExitMode();
            if (rightRulerMenuController) rightRulerMenuController.ExitMode();
        }

        if (modeToKeep != ControlType.DualAxisRotation)
        {
            if (leftDualAxisRotationControl) leftDualAxisRotationControl.ExitMode();
            if (rightDualAxisRotationControl) rightDualAxisRotationControl.ExitMode();
        }

        if (modeToKeep != ControlType.InverseKinematics)
        {
            if (leftIKManager) leftIKManager.enabled = false;
            if (rightIKManager) rightIKManager.enabled = false;
        }

        if (modeToKeep != ControlType.Notes)
        {
            leftNotesMenuController.ExitMode();
            if (rightNotesMenuController) rightNotesMenuController.ExitMode();
        }

        if (modeToKeep != ControlType.Eraser)
        {
            leftEraserMenuController.ExitMode();
            if (rightEraserMenuController) rightEraserMenuController.ExitMode();
        }
    }
}