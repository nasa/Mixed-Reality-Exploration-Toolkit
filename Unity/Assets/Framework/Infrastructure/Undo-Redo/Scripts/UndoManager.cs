using System.Collections.Generic;
using UnityEngine;
using GSFC.ARVR.MRET.XRC;

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
    private AnimationManager animationManager;
    private XRCManager xrcManager;

    public void AddAction(ProjectAction actionPerformed, ProjectAction inverseAction)
    {
        System.DateTime now = System.DateTime.Now;
        TimeStampUtility.LogTime("AddAction 1");
        animationManager.RecordAction(actionPerformed, inverseAction);
        TimeStampUtility.LogTime("AddAction 2");
        xrcManager.RecordAction(actionPerformed);
        TimeStampUtility.LogTime("AddAction 3 ");

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
        animationManager = FindObjectOfType<AnimationManager>();
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