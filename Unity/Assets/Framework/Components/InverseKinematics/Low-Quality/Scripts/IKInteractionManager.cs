﻿using UnityEngine;
using GSFC.ARVR.MRET.Common.Schemas;

public class IKInteractionManager : MonoBehaviour
{
    public VRTK.VRTK_ControllerEvents controllerEvents;

    private RootMotion.FinalIK.CCDIK finalIKScript, touchingScript;
    private bool controllingArm = false;
    private UndoManager undoManager;
    private GameObject partContainer;
    private Vector3 lastSavedPosition;

    public void Start()
    {
        controllerEvents.TouchpadPressed += new VRTK.ControllerInteractionEventHandler(ProcessTouchpadPress);
        controllerEvents.TouchpadReleased += new VRTK.ControllerInteractionEventHandler(ProcessTouchpadRelease);

        partContainer = FindObjectOfType<UnityProject>().projectObjectContainer;
        undoManager = FindObjectOfType<UndoManager>();
    }

    public void ProcessTouchpadPress(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        if (touchingScript != null)
        {
            if (touchingScript != finalIKScript)
            {
                lastSavedPosition = touchingScript.solver.GetIKPosition();
                touchingScript.enabled = true;
                finalIKScript = touchingScript;
                touchingScript = null;
                controllingArm = true;
            }
        }
    }

    public void ProcessTouchpadRelease(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        if (finalIKScript)
        {
            if (lastSavedPosition != null)
            {
                Vector3 currentIKPos = finalIKScript.solver.GetIKPosition();
                if (currentIKPos != lastSavedPosition)
                {
                    undoManager.AddAction(ProjectAction.SetFinalIKPosAction(GetFullPathAfter(finalIKScript.gameObject, partContainer), currentIKPos),
                        ProjectAction.SetFinalIKPosAction(GetFullPathAfter(finalIKScript.gameObject, partContainer), lastSavedPosition));
                }
            }

            finalIKScript.enabled = false;
            finalIKScript = null;
            controllingArm = false;
        }
    }

    public void OnEnable()
    {
        controllingArm = false;
    }

    public void OnTriggerEnter(Collider touchingObject)
    {
        RootMotion.FinalIK.CCDIK foundScript;
        if ((foundScript = touchingObject.GetComponentInParent<RootMotion.FinalIK.CCDIK>()) != null)
        {
            if (foundScript != touchingScript)
            {
                touchingScript = foundScript;
            }
        }
    }

    public void OnTriggerExit(Collider previousTouchingObject)
    {
        RootMotion.FinalIK.CCDIK foundScript;
        if ((foundScript = previousTouchingObject.GetComponentInParent<RootMotion.FinalIK.CCDIK>()) != null)
        {
            if (foundScript == touchingScript)
            {
                touchingScript = null;
            }
        }
    }

    public void Update()
    {
        if (controllingArm && finalIKScript != null)
        {
            finalIKScript.solver.SetIKPosition(transform.parent.position);
        }
        CheckToggling();
    }

    private string GetFullPath(GameObject obj)
    {
        GameObject highestObj = obj;
        string fullPath = highestObj.name;

        while (highestObj.transform.parent != null)
        {
            highestObj = highestObj.transform.parent.gameObject;
            fullPath = highestObj.name + "/" + fullPath;
        }

        return fullPath;
    }

    private string GetFullPathAfter(GameObject obj, GameObject after)
    {
        GameObject highestObj = obj;
        string fullPath = highestObj.name;

        while (highestObj.transform.parent != after.transform && highestObj.transform.parent != null)
        {
            highestObj = highestObj.transform.parent.gameObject;
            fullPath = highestObj.name + "/" + fullPath;
        }

        return fullPath;
    }

#region IKScriptToggler
    private bool needToTurnIKScriptOn = false, needToTurnIKScriptOff = false;
    private RootMotion.FinalIK.CCDIK scriptToToggle;
    private int ikScriptTimer = 0;
    public void ToggleFinalIKScript(RootMotion.FinalIK.CCDIK ikScript)
    {
        scriptToToggle = ikScript;
        needToTurnIKScriptOn = true;
    }

    void CheckToggling()
    {
        if (needToTurnIKScriptOn && scriptToToggle != null)
        {
            scriptToToggle.enabled = true;
            needToTurnIKScriptOn = false;
            needToTurnIKScriptOff = true;
            ikScriptTimer = 0;
        }
        else if (needToTurnIKScriptOff && scriptToToggle != null)
        {
            if (ikScriptTimer++ >= 16)
            {
                scriptToToggle.enabled = false;
                needToTurnIKScriptOff = false;
            }
        }
    }
#endregion
}