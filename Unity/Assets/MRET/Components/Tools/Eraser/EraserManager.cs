// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Action;
using GOV.NASA.GSFC.XR.MRET.Project;
using GOV.NASA.GSFC.XR.MRET.SceneObjects;

namespace GOV.NASA.GSFC.XR.MRET.Tools.Eraser
{
    public class EraserManager : MRETManager<EraserManager>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(EraserManager);

        public GameObject leftEraserObject, rightEraserObject;
        public bool canErase { get; private set; } = false;

        private GameObject leftTouching, rightTouching;
        private IAction leftUndo, rightUndo, leftRedo, rightRedo;

        public void Enable()
        {
            leftEraserObject.SetActive(true);
            if (rightEraserObject) rightEraserObject.SetActive(true);
            canErase = true;
        }

        public void Disable()
        {
            leftEraserObject.SetActive(false);
            if (rightEraserObject) rightEraserObject.SetActive(false);
            canErase = false;
        }

        protected void Erase(GameObject eraseObject, IAction redoAction, IAction undoAction)
        {
            if (eraseObject)
            {
                ProjectManager.UndoManager.AddAction(redoAction, undoAction);

                // Destroy all children first.
                foreach (Transform t in eraseObject.transform)
                {
                    Destroy(t.gameObject);
                }

                Destroy(eraseObject);
            }
        }

        public void LTouchpadPressed()
        {
            if (canErase)
            {
                // Erase the object
                Erase(leftTouching, leftRedo, leftUndo);
            }
        }

        public void RTouchpadPressed()
        {
            if (canErase)
            {
                // Erase the object
                Erase(rightTouching, rightRedo, rightUndo);
            }
        }

        /// <summary>
        /// Registers the erase actions for the supplied GameObject
        /// </summary>
        /// <param name="go">The <code>GameObject</code> being registered</param>
        protected void RegisterLEraseActions(GameObject go)
        {
            if (ProjectManager.SceneObjectManager != null)
            {
                IInteractable interactable = ProjectManager.SceneObjectManager.GetParent<IInteractable>(go);
                if (interactable != null)
                {
                    // Serialize the interactable
                    var serializedInteractable = interactable.CreateSerializedType();
                    interactable.Serialize(serializedInteractable);

                    // Create the project actions
                    leftUndo = new AddSceneObjectAction(serializedInteractable);
                    leftRedo = new DeleteIdentifiableObjectAction(serializedInteractable);

                    leftTouching = go;
                }
            }
        }

        /// <summary>
        /// Registers the erase actions for the supplied GameObject
        /// </summary>
        /// <param name="go">The <code>GameObject</code> being registered</param>
        protected void RegisterREraseActions(GameObject go)
        {
            if (ProjectManager.SceneObjectManager != null)
            {
                IInteractable interactable = ProjectManager.SceneObjectManager.GetParent<IInteractable>(go);
                if (interactable != null)
                {
                    // Serialize the interactable
                    var serializedInteractable = interactable.CreateSerializedType();
                    interactable.Serialize(serializedInteractable);

                    // Create the project actions
                    rightUndo = new AddSceneObjectAction(serializedInteractable);
                    rightRedo = new DeleteIdentifiableObjectAction(serializedInteractable);

                    rightTouching = go;
                }
            }
        }

        public void LControllerTouched(GameObject go)
        {
            if (canErase) RegisterLEraseActions(go);
        }

        public void RControllerTouched(GameObject go)
        {
            if (canErase) RegisterREraseActions(go);
        }

        public void LControllerUnTouched()
        {
            if (canErase)
            {
                leftUndo = leftRedo = null;
                leftTouching = null;
            }
        }

        public void RControllerUnTouched()
        {
            if (canErase)
            {
                rightUndo = rightRedo = null;
                rightTouching = null;
            }
        }
    }
}