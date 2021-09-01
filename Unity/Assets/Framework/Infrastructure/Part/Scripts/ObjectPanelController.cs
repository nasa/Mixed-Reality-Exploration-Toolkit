// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using GSFC.ARVR.MRET.Infrastructure.Framework.SceneObject;

public class ObjectPanelController : MonoBehaviour
{
    public GameObject selectedObject;
    public Toggle physics, explode, visible, locked; //public Toggle physics, gravity, explode, visible, locked;
    public Slider opacity;
    public Text titleText;
    public Toggle gravity;

    private bool enabling = false;
    private bool initialized = false;

    public void Initialize()
    {
        if (selectedObject == null)
        {
            Debug.LogWarning("[ObjectPanelController] Selected object not set.");
            return;
        }

        // Initialize physics toggle.
        physics.isOn = !selectedObject.GetComponent<Rigidbody>().isKinematic;

        // Initialize gravity toggle.
        gravity.isOn = selectedObject.GetComponent<Rigidbody>().useGravity;

        // Initialize explode toggle.
        explode.isOn = selectedObject.GetComponent<InteractablePart>().isExploded;

        // Initialize visibility toggle.
        visible.isOn = selectedObject.GetComponentInChildren<MeshRenderer>().enabled;

        //Initialize locked toggle
        locked.isOn = SceneObject.GetSceneObjectForGameObject(selectedObject).locked;
    }

    public void OnEnable()
    {
        if (initialized == false)
        {
            return;
        }

        if (selectedObject == null)
        {
            Debug.LogWarning("[ObjectPanelController] No selected object.");
            return;
        }

        enabling = true;

        // Initialize physics toggle.
        physics.isOn = !selectedObject.GetComponent<Rigidbody>().isKinematic;

        // Initialize gravity toggle.
        gravity.isOn = selectedObject.GetComponent<Rigidbody>().useGravity;

        // Initialize explode toggle.
        explode.isOn = selectedObject.GetComponent<InteractablePart>().isExploded;

        // Initialize visibility toggle.
        visible.isOn = selectedObject.GetComponentInChildren<MeshRenderer>().enabled;

        //Initialize locked toggle
        locked.isOn = SceneObject.GetSceneObjectForGameObject(selectedObject).locked;

        enabling = false;
    }

    public void SetTitle(string titleToSet)
    {
        if (titleText != null)
        {
            // Limit title to 15 characters.
            titleText.text = titleToSet.Substring(0, System.Math.Min(titleToSet.Length, 15));
            if (titleToSet.Length > 15)
            {
                titleText.text = titleText.text + "...";
            }
        }
    }

    public void ToggleGravity()
    {
        if (!enabling)
        {
            bool state = gravity.isOn;
            if (selectedObject != null)
            {
                Rigidbody rBody = selectedObject.GetComponent<Rigidbody>();

                // Only turn on gravity if physics is on.
                if ((!physics.isOn) || (!state))
                {
                    rBody.useGravity = false;
                    gravity.isOn = false;
                }
                else if (physics.isOn)
                {
                    rBody.useGravity = true;
                }

                InteractablePart iPart = selectedObject.GetComponent<InteractablePart>();
                UndoManager.instance.AddAction(ProjectAction.UpdateObjectSettingsAction(iPart.name,
                    new InteractablePart.InteractablePartSettings(iPart.grabbable,
                    !rBody.isKinematic, rBody.useGravity), iPart.guid.ToString()),
                    ProjectAction.UpdateObjectSettingsAction(iPart.name,
                    new InteractablePart.InteractablePartSettings(iPart.grabbable,
                    !rBody.isKinematic, !rBody.useGravity), iPart.guid.ToString()));
            }
        }
    }

    public void TogglePhysics()
    {
        if (!enabling)
        {
            bool state = physics.isOn;
            if (selectedObject != null)
            {
                // Turn off gravity if physics is off.
                if (!state)
                {
                    if (gravity.isOn)
                    {
                        ToggleGravity();
                    }
                }

                Rigidbody rBody = selectedObject.GetComponent<Rigidbody>();
                rBody.isKinematic = !state;

                InteractablePart iPart = selectedObject.GetComponent<InteractablePart>();
                UndoManager.instance.AddAction(ProjectAction.UpdateObjectSettingsAction(iPart.name,
                    new InteractablePart.InteractablePartSettings(iPart.grabbable,
                    !rBody.isKinematic, rBody.useGravity), iPart.guid.ToString()),
                    ProjectAction.UpdateObjectSettingsAction(iPart.name,
                    new InteractablePart.InteractablePartSettings(iPart.grabbable,
                    rBody.isKinematic, rBody.useGravity), iPart.guid.ToString()));
            }
        }
    }

    public void ToggleExplosion()
    {
        if (!enabling)
        {
            bool state = explode.isOn;
            if (selectedObject != null)
            {
                InteractablePart iPart = selectedObject.GetComponent<InteractablePart>();
                if (iPart)
                {
                    if (state)
                    {
                        // Unexplode.
                        iPart.Explode();
                    }
                    else
                    {
                        // Explode.
                        iPart.Unexplode();
                    }
                }
            }
        }
    }

    public void ToggleVisibility()
    {
        if (!enabling)
        {
            bool state = visible.isOn;
            if (selectedObject != null)
            {
                foreach (MeshRenderer rend in selectedObject.GetComponentsInChildren<MeshRenderer>())
                {
                    rend.enabled = state;
                }
            }
        }
    }

    public void ToggleLock()
    {
        if (!enabling)
        {
            bool state = locked.isOn;
            SceneObject selectedSceneObject = SceneObject.GetSceneObjectForGameObject(selectedObject);
            if (selectedSceneObject != null)
            {
                selectedSceneObject.locked = state;
            }
        }
    }

    public void ChangeOpacity()
    {
        Renderer[] rends = selectedObject.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < rends.Length; i++)
        {
            Color colorToChange = rends[i].material.color;
            colorToChange.a = opacity.value;
            rends[i].material.color = colorToChange;
        }
    }

    public void physicsToggle(bool state)
    {
        physics.isOn = state;
    }

    public void gravityToggle(bool state)
    {
        gravity.isOn = state;
    }
}