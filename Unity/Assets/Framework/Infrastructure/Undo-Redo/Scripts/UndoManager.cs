// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;
using GSFC.ARVR.MRET.XRC;
using GSFC.ARVR.MRET.Infrastructure.Framework.Animation;

public class UndoManager : MonoBehaviour
{
    private class ProjectDelta
    {
        private ProjectAction undoAction;
        private ProjectAction redoAction;
        private bool isActionPerformed;       // Keep track of if the action can be undone/redone.

        public ProjectDelta(ProjectAction actionPerformed, ProjectAction inverseAction)
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

    public static UndoManager instance;

    public int undoActionCount = 10;

    private Stack<ProjectDelta> undoStack, redoStack;
    private MRETAnimationManager animationManager;
    private XRCManager xrcManager;

    public void AddAction(ProjectAction actionPerformed, ProjectAction inverseAction)
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

	void Start()
    {
        animationManager = FindObjectOfType<MRETAnimationManager>();
        xrcManager = FindObjectOfType<XRCManager>();
        InitializeStack();
        instance = this;
	}

    private void InitializeStack()
    {
        undoStack = new Stack<ProjectDelta>(undoActionCount);
        redoStack = new Stack<ProjectDelta>(undoActionCount);
    }
}