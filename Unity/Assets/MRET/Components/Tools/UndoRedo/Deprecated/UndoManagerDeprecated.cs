// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Integrations.XRC;
using GOV.NASA.GSFC.XR.MRET.Action;
using GOV.NASA.GSFC.XR.MRET.Animation;

namespace GOV.NASA.GSFC.XR.MRET.Tools.UndoRedo
{
    [System.Obsolete("Refer to the " + nameof(GOV.NASA.GSFC.XR.MRET.Tools.UndoRedo.UndoManager) + " class")]
    public class UndoManagerDeprecated : MonoBehaviour
    {
        private class ProjectDelta
        {
            private ProjectActionDeprecated undoAction;
            private ProjectActionDeprecated redoAction;
            private bool isActionPerformed;       // Keep track of if the action can be undone/redone.

            public ProjectDelta(ProjectActionDeprecated actionPerformed, ProjectActionDeprecated inverseAction)
            {
                undoAction = inverseAction;
                redoAction = actionPerformed;
                isActionPerformed = true;
            }

            public void Undo()
            {
                if (isActionPerformed)
                {
                    undoAction.PerformAction();
                    isActionPerformed = false;
                }
            }

            public void Redo()
            {
                if (!isActionPerformed)
                {
                    redoAction.PerformAction();
                    isActionPerformed = true;
                }
            }
        }

        public static UndoManagerDeprecated instance;

        public int undoActionCount = 10;

        private Stack<ProjectDelta> undoStack, redoStack;
        private MRETAnimationManagerDeprecated animationManager;
        private XRCManagerDeprecated xrcManager;

        public void AddAction(ProjectActionDeprecated actionPerformed, ProjectActionDeprecated inverseAction)
        {
            animationManager.RecordAction(actionPerformed, inverseAction);
            xrcManager.RecordAction(actionPerformed);

            undoStack.Push(new ProjectDelta(actionPerformed, inverseAction));

            // Clear Redo Stack.
            redoStack.Clear();
        }

        public void Undo()
        {
            if (undoStack.Count > 0)
            {
                ProjectDelta deltaToUndo = undoStack.Pop();
                deltaToUndo.Undo();
                redoStack.Push(deltaToUndo);
            }
        }

        public void Redo()
        {
            if (redoStack.Count > 0)
            {
                ProjectDelta deltaToRedo = redoStack.Pop();
                deltaToRedo.Redo();
                undoStack.Push(deltaToRedo);
            }
        }

        public void Initialize()
        {
            animationManager = FindObjectOfType<MRETAnimationManagerDeprecated>();
            xrcManager = FindObjectOfType<XRCManagerDeprecated>();
            InitializeStack();
            instance = this;
        }

        private void InitializeStack()
        {
            undoStack = new Stack<ProjectDelta>(undoActionCount);
            redoStack = new Stack<ProjectDelta>(undoActionCount);
        }
    }
}