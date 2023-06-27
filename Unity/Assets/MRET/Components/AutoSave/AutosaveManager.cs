// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using System.IO;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Schema;
using GOV.NASA.GSFC.XR.MRET.Project;

namespace GOV.NASA.GSFC.XR.MRET.AutoSave
{
    /// <remarks>
    /// History:
    /// 27 June 2022: Created
    /// </remarks>
    /// <summary>
    /// AutosaveManager is a class that manages
    /// autosaves for open projects in MRET.
    /// Author: Jordan A. Ritchey
    /// </summary>
    public class AutosaveManager : MRETManager<AutosaveManager>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(AutosaveManager);

        [Tooltip("Interval in seconds to attempt to autosave.")]
        public int asaveInterval = 30;

        [Tooltip("Filename to use for generic autosave. Must end with \".mret\"!")]
        public string asaveName = "autosave.mret";

        [Tooltip("Prefix to use for autosaves.")]
        public string asavePrefix = "~";

        [Tooltip("Filename of the open project.")]
        public string storedFilename;

        [Tooltip("Keeps track of if the autosave is already running.")]
        private bool isRunning = false;

        private AutosaveDetector autosaveDetector;

        /// <seealso cref="MRETBehaviour.IntegrityCheck"/>
        protected override IntegrityState IntegrityCheck()
        {
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            return (
                (state == IntegrityState.Failure) ||
                (autosaveDetector == null)
                    ? IntegrityState.Failure   // Fail if base class fails, OR autosaveDetector is null
                    : IntegrityState.Success); // Otherwise, our integrity is valid
        }

        /// <seealso cref="MRETManager{M}.Initialize"/>
        public override void Initialize()
        {
            // Take the inherited behavior
            base.Initialize();

            // Initialize internal references
            autosaveDetector = gameObject.GetComponent<AutosaveDetector>();
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
            if (autosaveDetector != null)
            {
                autosaveDetector.BootDetect();
            }
            else
            {
                // Log an error
                LogError("Autosave detector is not set.", nameof(MRETStart));
            }
        }

        /// <summary>
        /// Starts our autosave process if it is not alredy running.
        /// </summary>
        public void StartAutosave()
        {
            if (!isRunning)
            {
                // indicate that we are running the autosave process
                isRunning = true;

                // make a duplicate of the loaded project if it is not an autosave itself
                if (!(Path.GetFileName(storedFilename).StartsWith(asavePrefix)))
                {
                    if (storedFilename.EndsWith(ProjectFileSchema.FILE_EXTENSION))
                    {
                        // split the stored filename so we can insert the autosave prefix
                        string[] splitFilename = FilenameSplitter(storedFilename);

                        // make a duplicate of the loaded project with our autosave prefix
                        File.Copy(storedFilename, (splitFilename[0] + asavePrefix + splitFilename[1]));
                    }
                    else
                    {
                        // make a duplicate of the loaded template to the generic autosave location
                        File.Copy(storedFilename, (asavePrefix + asaveName));
                    }
                }

                // start our autosave timer
                StartCoroutine(AutosaveTimer());

                // log a successful start
                Log("Autosave started successfully.", nameof(StartAutosave));
            }
        }

        /// <summary>
        /// Stops our autosave process if it is running.
        /// </summary>
        public void StopAutosave()
        {
            if (isRunning)
            {
                // indicate that we are not running the autosave process
                isRunning = false;

                // stop our autosave timer (and any others that could *possibly* be running)
                StopAllCoroutines();

                // log a successful stop
                Log("Autosave stopped successfully.", nameof(StopAutosave));
            }
        }

        /// <summary>
        /// Returns whether a generic autosave exists.
        /// </summary>
        public bool DoesGenericAutosaveExist()
        {
            return File.Exists(asavePrefix + asaveName);
        }

        /// <summary>
        /// Returns whether a project autosave exists for the stored project filename.
        /// </summary>
        public bool DoesProjectAutosaveExist()
        {
            // check if we have been passed a correct projectFilename, return false if not
            if (storedFilename.EndsWith(ProjectFileSchema.FILE_EXTENSION))
            {
                // split the stored filename so we can insert the autosave prefix
                string[] splitFilename = FilenameSplitter(storedFilename);

                // return if file (projectDirectory + asavePrefix + projectFilename) exists
                return File.Exists(splitFilename[0] + asavePrefix + splitFilename[1]);
            }
            return false;
        }

        /// <summary>
        /// Deletes the generic autosave and the project-specific autosave if they exist.
        /// </summary>
        public void DeleteAutosaves()
        {
            DeleteGenericAutosave();
            DeleteProjectAutosave();
        }

        /// <summary>
        /// Deletes the generic autosave if it exists.
        /// </summary>
        public void DeleteGenericAutosave()
        {
            try
            {
                if (DoesGenericAutosaveExist())
                {
                    // try to delete the generic autosave and its metadata
                    File.Delete(asavePrefix + asaveName);
                    File.Delete(asavePrefix + asaveName + ".meta");
                    Log("Generic autosave deleted.", nameof(DeleteGenericAutosave)); // DEBUG
                }
            }
            catch (Exception e)
            {
                Log(e.ToString(), nameof(DeleteGenericAutosave));
            }
        }

        /// <summary>
        /// Deletes the project-specific autosave if it exists.
        /// </summary>
        public void DeleteProjectAutosave()
        {
            try
            {
                if (storedFilename.EndsWith(ProjectFileSchema.FILE_EXTENSION) && DoesProjectAutosaveExist())
                {
                    // split the stored filename so we can insert the autosave prefix
                    string[] splitFilename = FilenameSplitter(storedFilename);

                    // try to delete the project autosave and its metadata
                    File.Delete(splitFilename[0] + asavePrefix + splitFilename[1]);
                    File.Delete(splitFilename[0] + asavePrefix + splitFilename[1] + ".meta");
                    Log("Project autosave deleted.", nameof(DeleteProjectAutosave)); //DEBUG
                }
            }
            catch (Exception e)
            {
                Log(e.ToString(), nameof(DeleteProjectAutosave));
            }
        }

        /// <summary>
        /// Loads the generic autosave if it exists.
        /// </summary>
        public void LoadGenericAutosave()
        {
            if (DoesGenericAutosaveExist())
            {
                // call OpenProject to load the generic autosave
                MRET.ModeNavigator.OpenProject((asavePrefix + asaveName));
            }
        }

        /// <summary>
        /// Loads the project-specific autosave if it exists.
        /// </summary>
        public void LoadProjectAutosave()
        {
            if (DoesProjectAutosaveExist())
            {
                // split the stored filename so we can insert the autosave prefix
                string[] splitFilename = FilenameSplitter(storedFilename);

                // call OpenProject to load the project autosave
                MRET.ModeNavigator.OpenProject((splitFilename[0] + asavePrefix + splitFilename[1]));
            }
        }

        /// <summary>
        /// Creates an autosave and starts a timer for the next autosave.
        /// </summary>
        private void AutosaveAction()
        {
            // Split the stored filename so we can insert the autosave prefix
            string[] splitFilename = FilenameSplitter(storedFilename);

            // Perform the autosave
            bool saved = MRET.ProjectManager.SaveToXML(splitFilename[0] + asavePrefix + splitFilename[1]);
            if (saved)
            {
                // log a success
                Log("Autosave action successful.", nameof(AutosaveAction));

                // start another autosave timer
                StartCoroutine(AutosaveTimer());
            }
            else
            {
                // log an error and exit the autosave process
                LogError("Autosave action failed - Stored filename is \"" + storedFilename + "\"", nameof(AutosaveAction));
            }
        }

        /// <summary>
        /// Timer that runs between autosaves.
        /// </summary>
        private IEnumerator AutosaveTimer()
        {
            // wait for specified number of seconds, progressing only if the project is loaded
            // and there have been changes to the project
            Log("Autosave timer started.", nameof(AutosaveTimer));
            do
            {
                yield return new WaitForSeconds(asaveInterval);
            }
            while (!(ProjectManager.Loaded && ProjectManager.Project.Changed));

            // call our autosave action to create an autosave and start another timer
            AutosaveAction();
        }

        /// <summary>
        /// Takes a full file location as a string and returns it as an array containing the location at [0] and the filename at [1].
        /// </summary>
        private string[] FilenameSplitter(string filename)
        {
            // create a basic array using Path's functions
            string[] splitFilename = { Path.GetDirectoryName(filename), Path.GetFileName(filename) };

            // update slot [0] of the array to account for GetDirectoryName's tendency to forget a '/' or '\' at the end of a directory name
            // note that windows uses '\' but some other platforms use '/', so we check which is being used to make this code cross-platform :)
            if (splitFilename[0].Contains("\\") && !(splitFilename[0].EndsWith("\\"))) { splitFilename[0] += "\\"; }
            else if (splitFilename[0].Contains("/") && !(splitFilename[0].EndsWith("/"))) { splitFilename[0] += "/"; }

            // update slot [1] of the array to remove the autosave prefix from our filename if it exists
            // this prevents us from accidentally making autosaves of a manually loaded autosave, instead
            // we will save autosaves directly over the manually loaded autosave
            if (splitFilename[1].StartsWith(asavePrefix)) { splitFilename[1] = splitFilename[1].Substring(asavePrefix.Length); }

            // return the completed array
            return splitFilename;
        }
    }
}