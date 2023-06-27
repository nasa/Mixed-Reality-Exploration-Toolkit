// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.IO;
using UnityEngine;
using GOV.NASA.GSFC.XR.XRUI.WorldSpaceMenu;

namespace GOV.NASA.GSFC.XR.MRET.AutoSave
{
    /// <remarks>
    /// History:
    /// 13 July 2022: Created
    /// </remarks>
    /// <summary>
    /// AutosaveDetector is a class that manages
    /// autosave detection on boot-up and project
    /// load of MRET.
    /// Author: Jordan A. Ritchey
    /// </summary>
    public class AutosaveDetector : MRETBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(AutosaveDetector);

        private YesNoDialogLoader dialogLoader;

        private int loadType = -1;  // 0 = generic, 1 = project

        /// <seealso cref="MRETBehaviour.IntegrityCheck"/>
        protected override IntegrityState IntegrityCheck()
        {
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            return (
                (state == IntegrityState.Failure) ||
                (dialogLoader == null)
                    ? IntegrityState.Failure   // Fail if base class fails, OR required components are null
                    : IntegrityState.Success); // Otherwise, our integrity is valid
        }

        /// <summary>
        /// Overridden to initialize the internal component references.
        /// </summary>
        /// <seealso cref="MRETBehaviour.MRETAwake"/>
        protected override void MRETAwake()
        {
            // Take the inherited behavior
            base.MRETAwake();

            // Initialize internal references
            dialogLoader = gameObject.GetComponentInChildren<YesNoDialogLoader>();
        }

        /// <summary>
        /// Overridden to run generic autosave detection on MRET boot.
        /// </summary>
        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            // Take the inherited behavior
            base.MRETStart();

            // Start behavior
            if (dialogLoader == null)
            {
                // Log an error
                LogError("Autosave load dialog is not set.", nameof(MRETStart));
            }
        }

        /// <summary>
        /// Event called on MRET boot, detects a generic autosave and prompts user.
        /// </summary>
        public void BootDetect()
        {
            // check if we have a generic autosave
            if (MRET.AutosaveManager.DoesGenericAutosaveExist())
            {
                // set our loadType to generic
                loadType = 0;

                // create yes/no dialog asking if user wants to open the generic autosave
                if (dialogLoader != null)
                {
                    dialogLoader.InstantiateMenu();
                }
            }
        }

        /// <summary>
        /// Event called on project load, detects a project autosave and prompts user.
        /// </summary>
        public void LoadDetect()
        {
            // check if we have a project autosave, if not then resume loading the project
            if (MRET.AutosaveManager.DoesProjectAutosaveExist() &&
                !(Path.GetFileName(MRET.AutosaveManager.storedFilename).StartsWith(MRET.AutosaveManager.asavePrefix)))
            {
                // set our loadType to project
                loadType = 1;

                // create yes/no dialog asking if user wants to open the project autosave
                if (dialogLoader != null)
                {
                    dialogLoader.InstantiateMenu();
                }
            }
            else
            {
                // resume loading the project
                MRET.ModeNavigator.OpenProject();
            }
        }

        /// <summary>
        /// Event called when user decides to load an autosave.
        /// </summary>
        public void LoadAutosave()
        {
            // load an autosave based on the stored type
            switch (loadType)
            {
                case 0:
                    // call function from AutosaveManager to load generic autosave
                    Log("Loading generic autosave.", nameof(LoadAutosave));
                    MRET.AutosaveManager.LoadGenericAutosave();
                    break;

                case 1:
                    // call function from AutosaveManager to load project autosave
                    Log("Loading project autosave.", nameof(LoadAutosave));
                    MRET.AutosaveManager.LoadProjectAutosave();
                    break;

                default:
                    // send an error message, invalid load type
                    LogError("Invalid load type.", nameof(LoadAutosave));
                    break;
            }
        }

        /// <summary>
        /// Event called when user decides not to load an autosave.
        /// </summary>
        public void DeleteAutosave()
        {
            //delete an autosave based on the stored type
            switch (loadType)
            {
                case 0:
                    // call function from AutosaveManager to delete generic autosave
                    Log("Deleting generic autosave.", nameof(DeleteAutosave)); //DEBUG
                    MRET.AutosaveManager.DeleteGenericAutosave();
                    break;

                case 1:
                    // call function from AutosaveManager to delete project autosave
                    Log("Deleting project autosave.", nameof(DeleteAutosave)); //DEBUG
                    MRET.AutosaveManager.DeleteProjectAutosave();
                    break;

                default:
                    // send an error message, invalid delete type
                    LogError("Invalid delete type.", nameof(DeleteAutosave));
                    break;
            }
        }
    }
}