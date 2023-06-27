// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using GOV.NASA.GSFC.XR.MRET.Data;

namespace GOV.NASA.GSFC.XR.MRET.Tools.Selection
{
    public class ControllerSelectionManager : MonoBehaviour
    {
        public static readonly string selectionKey = "MRET.INTERNAL.TOOLS.SELECTION";

        public enum SelectionMode { Selecting }; // TODO: implement per-controller selection mode system later on.

        // None: Not selecting, Single: Selecting Single Object, Multi: Selecting Multiple Objects
        public enum SelectionState { None, Single, Multi };

        public SelectionMode selectionMode = SelectionMode.Selecting;
        public SelectionManager selectionManager;
        public long singleSelectPressDuration_ms = 10, multiSelectPressDuration_ms = 1000;
        public SelectionState currentSelectionState = SelectionState.None;
        public LineRenderer laserRenderer;
        public Material laserMat;
        public ControllerSelectionManager otherControllerSelectionManager;
        public GameObject selectionRaycastRoot;
        public Text distanceText;

        private float maxRaycast = 1000f, laserWidth = 0.01f;
        private ISelectable currentlySelected = null;
        private bool isRaycasting = false, isPressing = false, wasPressingOnLastUpdate = false,
            pointerOn = false;
        private Stopwatch pressSW = new Stopwatch();

        public void Initialize()
        {
            MRET.DataManager.SaveValue(selectionKey, false);

            laserRenderer = gameObject.AddComponent<LineRenderer>();
            laserRenderer.SetPositions(new Vector3[] { Vector3.zero, Vector3.zero });
            laserRenderer.startWidth = laserRenderer.endWidth = laserWidth;
            laserRenderer.material = laserMat;

            if (!selectionManager)
            {
                selectionManager = MRET.SelectionManager;
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
                // TODO: Added check for null JCH
                if (laserRenderer != null)
                {
                    laserRenderer.SetPositions(new Vector3[] { Vector3.zero, Vector3.zero });
                }
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
            ToggleSelection(!((bool) MRET.DataManager.FindPoint(selectionKey)));
        }

        public void ToggleSelection(bool on)
        {
            if (on)
            {
                currentSelectionState = SelectionState.Single;
                pointerOn = otherControllerSelectionManager.pointerOn = true;
            }
            else
            {
                currentSelectionState = SelectionState.None;
                pointerOn = otherControllerSelectionManager.pointerOn = false;
            }

            // Save to DataManager.
            MRET.DataManager.SaveValue(new DataManager.DataValue(selectionKey, on));
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

        public void OnTouchpadTouch()
        {
            isRaycasting = true;
        }

        public void OnTouchpadUntouch()
        {
            isRaycasting = false;
        }

        public void OnTouchpadPress()
        {
            isPressing = true;
        }

        public void OnTouchpadRelease()
        {
            isPressing = false;
        }

        private void PerformRaycast()
        {
            Ray laserRaycast = new Ray(selectionRaycastRoot.transform.position, selectionRaycastRoot.transform.forward);
            RaycastHit hit;
            float distance = 0; // EDITED BY NATE
            bool enableDistance = true; // EDITED BY NATE -- This value would be linked to the Settings boolean, possibly through a static variable

            laserRenderer.SetPosition(0, selectionRaycastRoot.transform.position);
            if (Physics.Raycast(laserRaycast, out hit, maxRaycast))
            {
                ISelectable raycastSelectable = hit.transform.GetComponentInParent<ISelectable>();
                if (raycastSelectable != null)
                {
                    currentlySelected = raycastSelectable;
                    laserRenderer.SetPosition(1, hit.point);
                    distance = hit.distance; // EDITED BY NATE
                }
                else
                {
                    raycastSelectable = hit.transform.GetComponentInChildren<ISelectable>();
                    if (raycastSelectable != null)
                    {
                        currentlySelected = raycastSelectable;
                        laserRenderer.SetPosition(1, hit.point);
                        distance = hit.distance; // EDITED BY NATE
                    }
                    else
                    {
                        currentlySelected = null;
                        laserRenderer.SetPosition(1, selectionRaycastRoot.transform.position + selectionRaycastRoot.transform.forward * maxRaycast);
                        distance = 0; // EDITED BY NATE
                        enableDistance = false; // EDITED BY NATE
                    }
                }
            }
            else
            {
                currentlySelected = null;
                laserRenderer.SetPosition(1, selectionRaycastRoot.transform.position + selectionRaycastRoot.transform.forward * maxRaycast);
                distance = 0; // EDITED BY NATE
                enableDistance = false; // EDITED BY NATE
            }
            UpdateDistanceText(enableDistance, distance); // EDITED BY NATE
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

        // EDITED BY NATE ******------------------------------------
        public void UpdateDistanceText(bool enable, float distance)
        {
            float MAX_DISTANCE = 10f;
            float OFFSET = 2f; // Value used to shift text off of line
            Vector3 verticalVector = Vector3.down; // Vector used to get cross product

            if (!enable)
            {
                distanceText.text = ""; // Removes text, appearing invisible
                return; // exit method
            }
            else
            {
                Vector3 startPosition = laserRenderer.GetPosition(0);
                Vector3 endPosition = laserRenderer.GetPosition(1);
                Vector3 difference = Vector3.Normalize(endPosition - startPosition); // Unit vector of magnitude 1 in direction of laser renderer
                Vector3 offsetVector = Vector3.Cross(difference, verticalVector).normalized; // unit vector perpendicular to the difference vector

                if (distance > MAX_DISTANCE) // distance is greater than 10 meters
                {
                    distanceText.gameObject.transform.position = Vector3.Lerp(startPosition, endPosition,
                        MAX_DISTANCE * System.Math.Abs(Vector3.Distance(startPosition, endPosition))); //startPosition + (difference * MAX_DISTANCE) + (offsetVector * OFFSET); // set text at 10 meters
                }
                else // distance is less than or equal to 10 meters
                {
                    distanceText.gameObject.transform.position = Vector3.Lerp(startPosition, endPosition, 0.5f); //startPosition + (difference * (distance / 2)) + (offsetVector * OFFSET); // set text at halfway point
                }

                distanceText.text = distance.ToString("#.000") + " m"; // Update text to reflect the distance to the rig after it has been moved
            }
        }
    }
}