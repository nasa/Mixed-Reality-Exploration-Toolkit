// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Action;
using GOV.NASA.GSFC.XR.MRET.Project;

namespace GOV.NASA.GSFC.XR.MRET.Extensions.Ros.IK
{
    public class IKInteractionManager : MRETUpdateBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(IKInteractionManager);

        /// <summary>
        /// The hand to aim to follow.
        /// </summary>
        [Tooltip("The hand to aim to follow.")]
        public Transform handRoot;

#if MRET_EXTENSION_FINALIK
        private RootMotion.FinalIK.CCDIK finalIKScript, touchingScript;
#endif

        private bool controllingArm = false;
        private Vector3 lastSavedPosition;

        #region MRETUpdateBehaviour
        /// <seealso cref="MRETBehaviour.IntegrityCheck"/>
        protected override IntegrityState IntegrityCheck()
        {
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            return (
                (state == IntegrityState.Failure) // TODO: || (MyRequiredRef == null)

                    ? IntegrityState.Failure   // Fail is base class fails or anything is null
                    : IntegrityState.Success); // Otherwise, our integrity is valid
        }

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            base.MRETStart();

            // Set the defaults
#if !MRET_EXTENSION_FINALIK
            LogWarning("FinalIK not installed.");
#endif
        }

#if MRET_EXTENSION_FINALIK
        /// <seealso cref="MRETUpdateBehaviour.MRETUpdate"/>
        protected override void MRETUpdate()
        {
            base.MRETUpdate();

            if (controllingArm && finalIKScript != null)
            {
                finalIKScript.solver.SetIKPosition(handRoot.position);
            }
            CheckToggling();
        }
#endif
        #endregion MRETUpdateBehaviour

        public void ProcessTouchpadPress()
        {
#if MRET_EXTENSION_FINALIK
            if (touchingScript != null)
            {
                if (touchingScript != finalIKScript)
                {
                    lastSavedPosition = touchingScript.solver.GetIKPosition();
                    touchingScript.enabled = true;
                    finalIKScript = touchingScript;
                    touchingScript = null;
                    controllingArm = true;
                }
            }
#endif
        }

        public void ProcessTouchpadRelease()
        {
#if MRET_EXTENSION_FINALIK
            if (finalIKScript)
            {
                if (lastSavedPosition != null)
                {
                    Vector3 currentIKPos = finalIKScript.solver.GetIKPosition();
                    if (currentIKPos != lastSavedPosition)
                    {
                        ProjectManager.UndoManager.AddAction(
                            new SceneObjectTransformAction(GetFullPathAfter(finalIKScript.gameObject, ProjectManager.SceneObjectContainer), currentIKPos),
                            new SceneObjectTransformAction(GetFullPathAfter(finalIKScript.gameObject, ProjectManager.SceneObjectContainer), lastSavedPosition));
                    }
                }

                finalIKScript.enabled = false;
                finalIKScript = null;
                controllingArm = false;
            }
#endif
        }

        public void OnEnable()
        {
            controllingArm = false;
        }

#if MRET_EXTENSION_FINALIK
        public void OnTriggerEnter(Collider touchingObject)
        {
            RootMotion.FinalIK.CCDIK foundScript;
            if ((foundScript = touchingObject.GetComponentInParent<RootMotion.FinalIK.CCDIK>()) != null)
            {
                if (foundScript != touchingScript)
                {
                    touchingScript = foundScript;
                }
            }
    }

        public void OnTriggerExit(Collider previousTouchingObject)
        {
            RootMotion.FinalIK.CCDIK foundScript;
            if ((foundScript = previousTouchingObject.GetComponentInParent<RootMotion.FinalIK.CCDIK>()) != null)
            {
                if (foundScript == touchingScript)
                {
                    touchingScript = null;
                }
            }
        }
#endif

        private string GetFullPath(GameObject obj)
        {
            GameObject highestObj = obj;
            string fullPath = highestObj.name;

            while (highestObj.transform.parent != null)
            {
                highestObj = highestObj.transform.parent.gameObject;
                fullPath = highestObj.name + "/" + fullPath;
            }

            return fullPath;
        }

        private string GetFullPathAfter(GameObject obj, GameObject after)
        {
            GameObject highestObj = obj;
            string fullPath = highestObj.name;

            while (highestObj.transform.parent != after.transform && highestObj.transform.parent != null)
            {
                highestObj = highestObj.transform.parent.gameObject;
                fullPath = highestObj.name + "/" + fullPath;
            }

            return fullPath;
        }

        #region IKScriptToggler
        private bool needToTurnIKScriptOn = false, needToTurnIKScriptOff = false;
#if MRET_EXTENSION_FINALIK
        private RootMotion.FinalIK.CCDIK scriptToToggle;
#endif
        private int ikScriptTimer = 0;

#if MRET_EXTENSION_FINALIK
        public void ToggleFinalIKScript(RootMotion.FinalIK.CCDIK ikScript)
        {
            scriptToToggle = ikScript;
            needToTurnIKScriptOn = true;
        }

        void CheckToggling()
        {
            if (needToTurnIKScriptOn && scriptToToggle != null)
            {
                scriptToToggle.enabled = true;
                needToTurnIKScriptOn = false;
                needToTurnIKScriptOff = true;
                ikScriptTimer = 0;
            }
            else if (needToTurnIKScriptOff && scriptToToggle != null)
            {
                if (ikScriptTimer++ >= 16)
                {
                    scriptToToggle.enabled = false;
                    needToTurnIKScriptOff = false;
                }
            }
        }
#endif
        #endregion
    }
}