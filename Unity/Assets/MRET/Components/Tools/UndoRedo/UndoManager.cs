// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections.Generic;
using GOV.NASA.GSFC.XR.MRET.Action;
using GOV.NASA.GSFC.XR.MRET.Animation;
using GOV.NASA.GSFC.XR.MRET.Project;

namespace GOV.NASA.GSFC.XR.MRET.Tools.UndoRedo
{
    public class UndoManager : MRETManager<UndoManager>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(UndoManager);

        private class ProjectDelta
        {
            private IAction undoAction;
            private IAction redoAction;
            private bool isActionPerformed;       // Keep track of if the action can be undone/redone.

            public ProjectDelta(IAction actionPerformed, IAction inverseAction)
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

        public int undoActionCount = 10;

        private Stack<ProjectDelta> undoStack, redoStack;
        private MRETAnimationManager animationManager;

        public void AddAction(IAction actionPerformed, IAction inverseAction)
        {
            animationManager.RecordAction(actionPerformed, inverseAction);

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

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            base.MRETStart();

            // TODO: AnimationManager should be accessible in either MRET or MRET.ProjectManager
            if (animationManager == null)
            {
                animationManager = ProjectManager.AnimationManager;
            }
        }

        /// <seealso cref="MRETManager{M}.Initialize"/>
        public override void Initialize()
        {
            // Take the inherited behavior
            base.Initialize();

            undoStack = new Stack<ProjectDelta>(undoActionCount);
            redoStack = new Stack<ProjectDelta>(undoActionCount);
        }
    }
}