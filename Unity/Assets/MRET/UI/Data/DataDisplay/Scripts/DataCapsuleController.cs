// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using UnityEngine;
using UnityEngine.UI;
using GOV.NASA.GSFC.XR.MRET.SceneObjects;
using GOV.NASA.GSFC.XR.MRET.Data;

namespace GOV.NASA.GSFC.XR.MRET.UI.Data
{
    public class DataCapsuleController : MRETUpdateBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(DataCapsuleController);

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

        public IInteractable currentlyTouchingPart = null;
        public void HandleTouchpadPress()
        {
            if (currentlyTouchingPart != null)
            {
                if (!string.IsNullOrEmpty(pointKeyName))
                {
                    currentlyTouchingPart.AddDataPoint(pointKeyName);
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
            IInteractable interactable = collision.gameObject.GetComponentInParent<IInteractable>();
            if (interactable != null)
            {
                currentlyTouchingPart = interactable;
            }
        }

        public void OnTriggerExit(Collider collision)
        {
            IInteractable interactable = collision.gameObject.GetComponent<IInteractable>();
            if ((interactable != null) && (interactable == currentlyTouchingPart))
            {
                currentlyTouchingPart = null;
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