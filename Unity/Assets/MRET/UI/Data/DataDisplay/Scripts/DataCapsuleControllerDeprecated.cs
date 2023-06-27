// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using UnityEngine;
using UnityEngine.UI;
using GOV.NASA.GSFC.XR.MRET.Data;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Part;

namespace GOV.NASA.GSFC.XR.MRET.UI.Data
{
    [System.Obsolete("Refer to the " + nameof(GOV.NASA.GSFC.XR.MRET.UI.Data.DataCapsuleController) + " class")]
    public class DataCapsuleControllerDeprecated : MRETUpdateBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName
        {
            get
            {
                return nameof(DataCapsuleControllerDeprecated);
            }
        }

        private enum PointState { redHigh, yellowHigh, green, yellowLow, redLow, unknown };

        public string pointKeyName;
        public Material greenMaterial, redMaterial, yellowMaterial, unknownMaterial;
        public Text label;

        private MeshRenderer meshRenderer;
        private PointState currentState = PointState.unknown;

        /// <seealso cref="MRETBehaviour.IntegrityCheck"/>
        protected override IntegrityState IntegrityCheck()
        {
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            return (
                (state == IntegrityState.Failure) // TODO: || (MyRequiredRef == null)
                    ? IntegrityState.Failure      // Fail is base class fails or anything is null
                    : IntegrityState.Success);    // Otherwise, our integrity is valid
        }

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            // Take the inherited behavior
            base.MRETStart();

            meshRenderer = GetComponent<MeshRenderer>();
        }


        /// <seealso cref="MRETUpdateBehaviour.MRETUpdate"/>
        protected override void MRETUpdate()
        {
            // Take the inherited behavior
            base.MRETUpdate();

            if (!string.IsNullOrEmpty(pointKeyName))
            {
                // Get point.
                DataManager.DataValue pointValue = MRET.DataManager.FindCompletePoint(pointKeyName);

                if (pointValue != null)
                {
                    if (label)
                    {
                        label.text = pointValue.key + ": " + pointValue.value.ToString();
                    }

                    // Handle state settings.
                    PointState pointState = DeterminePointState(pointValue);
                    switch (pointState)
                    {
                        case PointState.green:
                            if (pointState != PointState.green) ChangeToGreen();
                            break;

                        case PointState.yellowHigh:
                            if (pointState != PointState.yellowHigh) ChangeToYellow();
                            break;

                        case PointState.yellowLow:
                            if (pointState != PointState.yellowLow) ChangeToYellow();
                            break;

                        case PointState.redHigh:
                            if (pointState != PointState.redHigh) ChangeToRed();
                            break;

                        case PointState.redLow:
                            if (pointState != PointState.redLow) ChangeToRed();
                            break;

                        case PointState.unknown:
                        default:
                            if (pointState != PointState.unknown) ChangeToUnknown();
                            break;
                    }
                }
            }
        }

        public void ChangeToGreen()
        {
            meshRenderer.materials = new Material[] { greenMaterial };
        }

        public void ChangeToRed()
        {
            meshRenderer.materials = new Material[] { redMaterial };
        }

        public void ChangeToYellow()
        {
            meshRenderer.materials = new Material[] { yellowMaterial };
        }

        public void ChangeToUnknown()
        {
            meshRenderer.materials = new Material[] { unknownMaterial };
        }

        public InteractablePartDeprecated currentlyTouchingPart = null;
        public void HandleTouchpadPress()
        {
            if (currentlyTouchingPart)
            {
                if (pointKeyName != "")
                {
                    // Prevents duplicates.
                    bool duplicateFound = false;
                    foreach (string dataPoint in currentlyTouchingPart.dataPoints)
                    {
                        if (dataPoint == pointKeyName)
                        {
                            duplicateFound = true;
                            break;
                        }
                    }

                    if (!duplicateFound)
                    {
                        currentlyTouchingPart.dataPoints.Add(pointKeyName);
                    }
                }
                if (gameObject)
                {
                    //GetComponentInParent<VRTK_ControllerEvents>().UnsubscribeToButtonAliasEvent(VRTK_ControllerEvents.ButtonAlias.TouchpadPress, true, HandleTouchpadPress);
                    Destroy(gameObject);
                }
            }
        }

        public void OnTriggerEnter(Collider collision)
        {
            InteractablePartDeprecated iPart = collision.gameObject.GetComponentInParent<InteractablePartDeprecated>();
            if (iPart)
            {
                currentlyTouchingPart = iPart;
            }
        }

        public void OnTriggerExit(Collider collision)
        {
            InteractablePartDeprecated iPart = collision.gameObject.GetComponent<InteractablePartDeprecated>();
            if (iPart)
            {
                if (iPart == currentlyTouchingPart)
                {
                    currentlyTouchingPart = null;
                }
            }
        }

        #region Helpers
        private PointState DeterminePointState(DataManager.DataValue pointValue)
        {
            if (pointValue != null)
            {
                if (pointValue.arbitraryColorThresholds != null)
                {
                    foreach ((DataManager.DataValue.LimitState, object, object) threshold in pointValue.arbitraryColorThresholds)
                    {
                        if (Convert.ToDouble(pointValue.value) >= Convert.ToDouble(threshold.Item2) && Convert.ToDouble(pointValue.value) <= Convert.ToDouble(threshold.Item3))
                        {
                            return PointState.unknown;
                        }
                    }
                }
            }

            return PointState.unknown;
        }
        #endregion Helpers
    }
}