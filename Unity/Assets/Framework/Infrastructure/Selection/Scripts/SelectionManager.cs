// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;
using GSFC.ARVR.MRET.Components.Notes;

namespace GSFC.ARVR.MRET.Selection
{
    public class SelectionManager : MonoBehaviour
    {
        public InteractablePart[] interactableParts
        {
            get
            {
                return (InteractablePart[]) GetAllOfType<InteractablePart>();
            }
        }

        public LineDrawing[] drawings
        {
            get
            {
                return (LineDrawing[]) GetAllOfType<LineDrawing>();
            }
        }

        public Note[] notes
        {
            get
            {
                return (Note[]) GetAllOfType<Note>();
            }
        }

        private List<object> selectedObjects = new List<object>();

        public void Initialize()
        {
            selectedObjects = new List<object>();
        }

        public void AddToSelection(object selectedObject)
        {
            if (selectedObject is ISelectable)
            {
                if (!selectedObjects.Contains(selectedObject))
                {
                    selectedObjects.Add(selectedObject);
                    ((ISelectable) selectedObject).Select();
                }
            }
        }

        public void RemoveFromSelection(object selectedObject)
        {
            if (selectedObject is ISelectable)
            {
                ((ISelectable) selectedObject).Deselect();
                selectedObjects.Remove(selectedObject);
            }
        }

        public void ClearSelection()
        {
            foreach (object selectedObject in selectedObjects)
            {
                if (selectedObject is ISelectable)
                {
                    ((ISelectable) selectedObject).Deselect();
                }
            }
            selectedObjects = new List<object>();
        }

        private object[] GetAllOfType<T>()
        {
            List<object> iParts = new List<object>();

            foreach (object obj in selectedObjects)
            {
                if (obj is GameObject)
                {
                    iParts.Add(((GameObject) obj).GetComponent<T>());
                }
            }

            return iParts.ToArray();
        }
    }
}