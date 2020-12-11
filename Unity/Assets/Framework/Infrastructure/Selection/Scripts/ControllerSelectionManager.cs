using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

namespace GSFC.ARVR.MRET.Selection
{
    public class ControllerSelectionManager : MonoBehaviour
    {
        public enum SelectionMode { Selecting }; // TODO: implement per-controller selection mode system later on.

        // None: Not selecting, Single: Selecting Single Object, Multi: Selecting Multiple Objects
        public enum SelectionState { None, Single, Multi };

        public SelectionMode selectionMode = SelectionMode.Selecting;
        public SelectionManager selectionManager;
        public VRTK.VRTK_ControllerEvents controllerEvents;
        public long singleSelectPressDuration_ms = 10, multiSelectPressDuration_ms = 1000;
        public SelectionState currentSelectionState = SelectionState.None;
        public LineRenderer laserRenderer;
        public Material laserMat;
        public Toggle selectionToggle;
        public ControllerSelectionManager otherControllerSelectionManager;

        private float maxRaycast = 1000f, laserWidth = 0.01f;
        private ISelectable currentlySelected = null;
        private bool isRaycasting = false, isPressing = false, wasPressingOnLastUpdate = false,
            pointerOn = false;
        private Stopwatch pressSW = new Stopwatch();

        void Start()
        {
            laserRenderer = gameObject.AddComponent<LineRenderer>();
            laserRenderer.SetPositions(new Vector3[] { Vector3.zero, Vector3.zero });
            laserRenderer.startWidth = laserRenderer.endWidth = laserWidth;
            laserRenderer.material = laserMat;

            if (controllerEvents)
            {
                controllerEvents.TouchpadPressed += new VRTK.ControllerInteractionEventHandler(ProcessTouchpadPress);
                controllerEvents.TouchpadReleased += new VRTK.ControllerInteractionEventHandler(ProcessTouchpadRelease);
                controllerEvents.TouchpadTouchStart += new VRTK.ControllerInteractionEventHandler(ProcessTouchpadTouch);
                controllerEvents.TouchpadTouchEnd += new VRTK.ControllerInteractionEventHandler(ProcessTouchpadUntouch);
            }

            if (!selectionManager)
            {
                selectionManager = FindObjectOfType<SelectionManager>();
            }
        }
        
        void Update()
        {
            if (isRaycasting && pointerOn && selectionMode == SelectionMode.Selecting)
            {
                PerformRaycast();
            }
            else
            {
                laserRenderer.SetPositions(new Vector3[] { Vector3.zero, Vector3.zero });
            }

            long elapsedPressDuration;

            if (currentlySelected != null)
            {
                if (isPressing)
                {
                    // Start capturing the press duration if this is the first Update
                    // on which pressing was registered.
                    if (!wasPressingOnLastUpdate)
                    {
                        pressSW.Reset();
                        pressSW.Start();
                    }
                    wasPressingOnLastUpdate = true;
                }
                else if (wasPressingOnLastUpdate)
                {
                    wasPressingOnLastUpdate = false;
                    pressSW.Stop();

                    switch (currentSelectionState)
                    {
                        case SelectionState.None:
                            // Don't do anything.
                            break;

                        case SelectionState.Single:
                            elapsedPressDuration = pressSW.ElapsedMilliseconds;
                            if (elapsedPressDuration >= multiSelectPressDuration_ms)
                            {
                                // Enter multi-selection state and initialize selection with current object.
                                currentSelectionState = SelectionState.Multi;
                                selectionManager.ClearSelection();
                                selectionManager.AddToSelection(currentlySelected);

                            }
                            else if (elapsedPressDuration >= singleSelectPressDuration_ms)
                            {
                                // Stay in single-selection state and select the current object.
                                currentSelectionState = SelectionState.Single;
                                selectionManager.ClearSelection();
                                selectionManager.AddToSelection(currentlySelected);
                            }
                            break;

                        case SelectionState.Multi:
                            elapsedPressDuration = pressSW.ElapsedMilliseconds;
                            if (elapsedPressDuration >= multiSelectPressDuration_ms)
                            {
                                // Enter single-selection state and add current object to selection.
                                currentSelectionState = SelectionState.Single;
                                selectionManager.AddToSelection(currentlySelected);
                            }
                            else if (elapsedPressDuration >= singleSelectPressDuration_ms)
                            {
                                // Stay in multi-selection state and add current object to selection.
                                currentSelectionState = SelectionState.Multi;
                                selectionManager.AddToSelection(currentlySelected);
                            }
                            break;
                    }
                }
            }
            else
            {
                if (isPressing)
                {
                    // Start capturing the press duration if this is the first Update
                    // on which pressing was registered.
                    if (!wasPressingOnLastUpdate)
                    {
                        pressSW.Reset();
                        pressSW.Start();
                    }
                    wasPressingOnLastUpdate = true;
                }
                else if (wasPressingOnLastUpdate)
                {
                    wasPressingOnLastUpdate = false;
                    switch (currentSelectionState)
                    {
                        case SelectionState.None:
                            // Don't do anything.
                            break;

                        case SelectionState.Single:
                        case SelectionState.Multi:
                            // Enter single-selection state and deselect all.
                            currentSelectionState = SelectionState.Single;
                            selectionManager.ClearSelection();
                            break;
                    }
                }
            }
        }

        public void ToggleSelection()
        {
            if (selectionToggle)
            {
                if (selectionToggle.isOn)
                {
                    currentSelectionState = SelectionState.Single;
                    pointerOn = otherControllerSelectionManager.pointerOn = true;
                }
                else
                {
                    currentSelectionState = SelectionState.None;
                    pointerOn = otherControllerSelectionManager.pointerOn = false;
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning("ControllerSelectionManager->TogglePointer: selectionToggle not set.");
            }
        }

        public void OnTriggerEnter(Collider touchingObject)
        {
            TryEnter(touchingObject.gameObject);
        }

        public void OnCollisionEnter(Collision touchingObject)
        {
            TryEnter(touchingObject.gameObject);
        }

        public void OnTriggerExit(Collider previousTouchingObject)
        {
            TryExit(previousTouchingObject.gameObject);
        }

        public void OnCollisionExit(Collision previousTouchingObject)
        {
            TryExit(previousTouchingObject.gameObject);
        }

        public void ProcessTouchpadTouch(object sender, VRTK.ControllerInteractionEventArgs e)
        {
            isRaycasting = true;
        }

        public void ProcessTouchpadUntouch(object sender, VRTK.ControllerInteractionEventArgs e)
        {
            isRaycasting = false;
        }

        public void ProcessTouchpadPress(object sender, VRTK.ControllerInteractionEventArgs e)
        {
            isPressing = true;
        }

        public void ProcessTouchpadRelease(object sender, VRTK.ControllerInteractionEventArgs e)
        {
            isPressing = false;
        }

        private void PerformRaycast()
        {
            Ray laserRaycast = new Ray(transform.position, transform.forward);
            RaycastHit hit;

            laserRenderer.SetPosition(0, transform.position);
            if (Physics.Raycast(laserRaycast, out hit, maxRaycast))
            {
                ISelectable raycastSelectable = hit.transform.GetComponentInParent<ISelectable>();
                if (raycastSelectable != null)
                {
                    currentlySelected = raycastSelectable;
                    laserRenderer.SetPosition(1, hit.point);
                }
                else
                {
                    raycastSelectable = hit.transform.GetComponentInChildren<ISelectable>();
                    if (raycastSelectable != null)
                    {
                        currentlySelected = raycastSelectable;
                        laserRenderer.SetPosition(1, hit.point);
                    }
                    else
                    {
                        currentlySelected = null;
                        laserRenderer.SetPosition(1, transform.position + transform.forward * maxRaycast);
                    }
                }
            }
            else
            {
                currentlySelected = null;
                laserRenderer.SetPosition(1, transform.position + transform.forward * maxRaycast);
            }
        }

        private void TryEnter(GameObject touchingObject)
        {
            ISelectable touchingSelectable = touchingObject.GetComponentInParent<ISelectable>();
            if (touchingSelectable != null)
            {
                currentlySelected = touchingSelectable;
            }
        }

        private void TryExit(GameObject previousTouchingObject)
        {
            ISelectable previousSelectable = previousTouchingObject.GetComponentInParent<ISelectable>();
            if (previousSelectable != null)
            {
                if (previousSelectable == currentlySelected)
                {
                    currentlySelected = null;
                }
            }
        }
    }
}