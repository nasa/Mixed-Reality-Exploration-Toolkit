// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using GOV.NASA.GSFC.XR.Utilities.Math;
using GOV.NASA.GSFC.XR.MRET.Configuration;
using GOV.NASA.GSFC.XR.MRET.Project;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Part;
using GOV.NASA.GSFC.XR.MRET.Schema;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.Integrations.RMIT
{
    /// <remarks>
    /// History:
    /// 5 August 2022: Created (Henry Chen)
    /// 4 February 2023: Updated to support asynchronous conversion and
    ///     integrated into 22.1 (Jeffrey Hosler)
    /// </remarks>
	///
	/// <summary>
	/// RMIT controller class.<br>
	///
    /// Author: Henry Chen
	/// </summary>
	/// 
    public class RMITController : MRETBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(RMITController);

        public const float DECIMATE_MIN = 0.0001f;
        public const float DECIMATE_MAX = 0.99999999f;

        public Text decimateSliderText;
        public Slider decimateSlider;
        public Button convertButton;
        public Button convertAndImportButton;
        public float decimationValue;

        /// <summary>
        /// The source file to convert and optionally import
        /// </summary>
        [SerializeField]
        [Tooltip("The source file to convert and optionally import")]
        private string sourceFile;

        /// <summary>
        /// The source file to convert and optionally import
        /// </summary>
        public string SourceFile
        {
            get => sourceFile;
            set
            {
                sourceFile = value;

                // Source file changed, so update the controls state
                UpdateControlsState();
            }
        }

        private bool decimateSliderIsBeingDragged = false;

        #region MRETBehaviour
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

            // Initialize the fields
            decimateSliderText.text = decimateSlider.value.ToString();
            decimationValue = decimateSlider.value;

            // Initialize the controls state
            UpdateControlsState();
        }
        #endregion MRETBehaviour

        #region Slider Event handlers
        public void StartDragging()
        {
            decimateSliderIsBeingDragged = true;
        }

        public void StopDragging()
        {
            decimateSliderIsBeingDragged = false;
        }

        public void SliderValueChanged()
        {
            if (decimateSliderIsBeingDragged)
            {
                decimateSliderText.text = decimateSlider.value.ToString();
                decimationValue = decimateSlider.value;
            }
        }
        #endregion Slider Event handlers

        /// <summary>
        /// Updates the interactable state of the controls attached to this controller
        /// </summary>
        public void UpdateControlsState()
        {
            // Disable the convert buttons until a source file is specified
            if (convertButton != null)
            {
                convertButton.interactable = !string.IsNullOrEmpty(SourceFile);
            }
            if (convertAndImportButton != null)
            {
                convertAndImportButton.interactable = !string.IsNullOrEmpty(SourceFile);
            }
        }

        /// <summary>
        /// Asynchronous method to start the RMIT conversion process
        /// </summary>
        /// <param name="importAfterConvert">Indicates if the part should be
        ///     imported after the conversion completes</param>
        public async void StartRMITProcess(bool importAfterConvert)
        {
            // Check assertions
            if (string.IsNullOrEmpty(SourceFile))
            {
                LogError("Source file is not defined", nameof(StartRMITProcess));
                return;
            }
            if (!File.Exists(SourceFile))
            {
                LogError("Specified source file does not exist: " + SourceFile, nameof(StartRMITProcess));
                return;
            }

            // Begin the conversion process
            Task<RMITTaskResult> rmitTask = StartRMITProcess(SourceFile);
            RMITTaskResult rmitArgs = await rmitTask;

            if (rmitArgs != null)
            {
                // Convert the GLTF to a part
                GltfToPart(rmitArgs.file, rmitArgs.args, importAfterConvert);
            }
            else
            {
                LogError("A problem occurred running the RMIT task.");
            }
        }

        protected class RMITTaskResult
        {
            public string file;
            public string args;
        }

        /// <summary>
        /// Starts the RMIT task to perform the conversion
        /// </summary>
        /// <param name="importFile"></param>
        /// <returns></returns>
        private async Task<RMITTaskResult> StartRMITProcess(string importFile)
        {
            RMITTaskResult result = null;

            // Make sure RMIT is available
            if (!MRET.ConfigurationManager.RMITAvailable)
            {
                LogWarning("RMIT is not available. The RMIT installation directory needs to be configured.");
                return result;
            }

#if !HOLOLENS_BUILD
            await Task.Run(() => // <-- This code makes it async, however there are threading issues with MRET right now
            {
                Process process = new Process();
                try
                {
                    string rmitPath = Path.GetFullPath(MRET.ConfigurationManager.rmitDirectory);

                    // Blender
                    // string blenderPath = "blender.exe"; // added to path
                    // string blenderPath = rmitPath + "D:\\RMIT_v2.0\\RMIT_V2.0_Release\\blender_for_RMIT\\blender.exe";
                    string blenderPath = Path.Combine(rmitPath, "blender_for_RMIT", "blender.exe");
                    blenderPath = blenderPath.Replace("/", Path.DirectorySeparatorChar.ToString());

                    // RMIT Blender Driver
                    // string rmitDriverPath = "D:\\RMIT_v2.0\\RMIT_V2.0_Release\\RMIT_BlenderDriver.py";
                    // string rmitDriverPath = Path.Combine("D:", "RMIT_v2.0", "RMIT_V2.0_Release", "RMIT_BlenderDriver.py");
                    string rmitDriverPath = Path.Combine(rmitPath, "RMIT_BlenderDriver.py");
                    rmitDriverPath = rmitDriverPath.Replace("/", Path.DirectorySeparatorChar.ToString());

                    // Import file
                    string importFilePath = importFile.Replace("/", Path.DirectorySeparatorChar.ToString());

                    // Export file
                    string exportFilePath = Path.Combine(MRET.ConfigurationManager.defaultPartDirectory, Path.GetFileNameWithoutExtension(importFilePath) + ".gltf");
                    // string finalGLTFPath = ConfigurationManager.instance.defaultPartDirectory.Replace("/","\\") + "\\" + Path.GetFileNameWithoutExtension(sourceFilePath) + ".gltf";
                    exportFilePath = exportFilePath.Replace("/", Path.DirectorySeparatorChar.ToString());

                    // Construct the RMIT command line arguments

                    /* For Future Reference:
                        Current index of values being passed through function string
                        decimate 0  -- Lowest value: 0.0001 , Mid value: 0.01, Max value: 0.99999999
                        cleanup 1
                        delete hidden 2
                        flatten tree 3
                        merge 4
                        merge materials 5
                        center 6
                        split 7
                        decimate to poly 8

                        // Arguments Split by '/'
                        // splitting the value by its index and corresponding second value. ex 0;.1 is decimation 10%
                        example: "1;0.005/2;0/3;0/5;0/7;0" from Jose Chavez (KSC)
                    */

                    string commandLineArgs = "";
                    if (MathUtil.ApproximatelyEquals(decimationValue, DECIMATE_MIN))
                    {
                        commandLineArgs = "0;";
                        commandLineArgs += DECIMATE_MIN;
                    }
                    else if (MathUtil.ApproximatelyEquals(decimationValue, DECIMATE_MAX))
                    {
                        commandLineArgs = "0;";
                        commandLineArgs += DECIMATE_MAX;
                    }
                    else if ((decimationValue > DECIMATE_MIN) && (decimationValue < DECIMATE_MAX))
                    {
                        commandLineArgs = "0;";
                        commandLineArgs += decimationValue.ToString();
                    }
                    else
                    {
                        commandLineArgs = "-1";
                        UnityEngine.Debug.Log("Decimation Value is invalid. It must be a float between 0.0001 and 0.999999: " + decimationValue);
                    }


                    /*
                    process.StartInfo.Arguments = "Start-Process -NoNewWindow -FilePath \'"
                                                + blenderPath + "\' -ArgumentList '-b', '-P', \'"
                                                + rmitDriverPath + "\', " + "'--',  '--import', \'"
                                                + sourceFilePath + "\', '--export', \'"
                                                + finalGLTFPath + "\', '--unittesting', 'False', '--function '" + commandLineArgs + "' --datasmith', 'False'";
                    */

                    process.StartInfo.FileName = "powershell.exe";

                    // Build the RMIT arguments
                    process.StartInfo.Arguments =
                        "Start-Process -NoNewWindow -FilePath \"" + blenderPath + "\" " +
                        "-ArgumentList " +
                        "'-b', '-P', '\"" + rmitDriverPath + "\"', " +
                        "'--', " +
                        "'--import', '\"" + importFilePath + "\"', " +
                        "'--export', '\"" + exportFilePath + "\"', " +
                        "'--unittesting', 'False', " +
                        "'--function', '" + commandLineArgs + "', " +
                        "'--datasmith', 'False'";

                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.CreateNoWindow = true;

                    // Hide powershell window from user
                    if (process.StartInfo.UseShellExecute)
                    {
                        process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    }

                    Log("Executing command: \n" + process.StartInfo.Arguments);

                    // Notify user once the RMIT load process is over.
                    // process.EnableRaisingEvents = true;
                    // process.Exited += new EventHandler(OnProcessExit);
                    // process.Exited += (sender, e) => { OnRMITProcessFinished(sender, eventArgs); };
                    // process.Exited += new EventHandler(OnRMITProcessFinished);

                    process.Start();
                    string stdout = process.StandardOutput.ReadToEnd();
                    string stderr = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    // Log results
                    if (!string.IsNullOrEmpty(stdout))
                    {
                        Log(stdout);
                    }
                    if (!string.IsNullOrEmpty(stderr))
                    {
                        LogError(stderr);
                    }

                    // Assign the result args
                    result = new RMITTaskResult
                    {
                        file = exportFilePath,
                        args = commandLineArgs
                    };
                }
                catch (Exception e)
                {
                    Log("A problem occurred running the RMIT process: " + e.Message);
                }

                // Close the process
                process.Close();
            });
#endif
            return result;
        }

        /*
        protected void OnRMITProcessFinished(object sender, EventArgs e)
        {
            Log("RMIT process finished: " + finalGLTFPath + "; " + commandLineArgs);

            // Convert the GLTF
            GltfToInteractablePart(finalGLTFPath, commandLineArgs);
        }
        */

        private IEnumerator ToXML(PartType serializedPart, SerializationState serializedState)
        {
            try
            {
                // Create a filesystem friendly file name based upon the part name
                string filename = string.Join("_", serializedPart.Name.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
                filename += PartFileSchema.FILE_EXTENSION;

                // Create the file path in the default parts directory
                string filePath = Path.Combine(MRET.ConfigurationManager.defaultPartDirectory, filename);

                // Save the imported part out to XML
                PartFileSchema.ToXML(filePath, serializedPart);

                // Mark as complete
                serializedState.complete = true;
            }
            catch (Exception e)
            {
                serializedState.Error("There was a problem serializing the RMIT part to XML: " + e.Message);
                yield break;
            }

            yield return null;
        }

        /// <summary>
        /// Converts the supplied GLTF file to a serialized MRET part
        /// </summary>
        /// <param name="gltfFilePath">The GLST file to convert</param>
        /// <param name="rmitSettings">The settings used by RMIT to perform the GLTF conversion</param>
        /// <param name="importAfterConvert">Indicates wether the serialized part should be instantiated
        ///     (imported) into the project after converting to a part</param>
        private void GltfToPart(string gltfFilePath, string rmitSettings, bool importAfterConvert)
        {
            StartCoroutine(GltfToPartReentrant(gltfFilePath, rmitSettings, importAfterConvert));
        }

        /// <summary>
        /// Reentrant method to convert the supplied GLTF file to a serialized MRET part
        /// </summary>
        /// <param name="gltfFilePath">The GLST file to convert</param>
        /// <param name="rmitSettings">The settings used by RMIT to perform the GLTF conversion</param>
        /// <param name="importAfterConvert">Indicates wether the serialized part should be instantiated
        ///     (imported) into the project after converting to a part</param>
        /// <returns>An <code>IEnumerator</code> for reentrance during coroutine processing</returns>
        private IEnumerator GltfToPartReentrant(string gltfFilePath, string rmitSettings, bool importAfterConvert)
        {
            Log("Entering GLTF import...");

            if (!File.Exists(gltfFilePath))
            {
                LogWarning("Specified GLTF file does not exist: " + gltfFilePath);
                yield break;
            }

            // Build the serialized part description
            PartType serializedPart = new PartType();

            // Name (required)
            serializedPart.Name = Path.GetFileNameWithoutExtension(gltfFilePath);

            // Generate the ID from the name
            serializedPart.ID = MRET.UuidRegistry.CreateUniqueIDFromName(serializedPart.Name);

            // Create the model structure with the normalized file path
            string dataPath = Path.GetFullPath(Path.Combine(Application.dataPath,"..")).Replace("/", Path.DirectorySeparatorChar.ToString());
            string filePath = Path.GetFullPath(gltfFilePath).Replace("/", Path.DirectorySeparatorChar.ToString());
            serializedPart.Model = new ModelType
            {
                // GLTF File
                Item = new ModelFileType
                {
                    format = ModelFormatType.GLTF,
                    // Make the path relative if it is in the data path. Otherwise replace will leave it alone.
                    Value = filePath.Replace(dataPath, ".")
                }
            };

            // Create the transform. By default, blender exports .fbx files to Unity with an
            // adjusted scale of 100 and rotation of -89.98
            serializedPart.Transform = new TransformType
            {
                Item = new TransformEulerRotationType
                {
                    X = -90
                },
                Item1 = new TransformScaleType
                {
                    X = 0.01f,
                    Y = 0.01f,
                    Z = 0.01f,
                    referenceSpace = ReferenceSpaceType.Relative
                }
            };

            // Set randomize textures
            serializedPart.RandomizeTextures = true;

            // Create the part specifications
            serializedPart.PartSpecifications = new PartSpecificationsType
            {
                Vendor = "Unknown",
                Version = "Unknown",
                Notes = "Converted by RMIT with settings: " + rmitSettings,
                Reference = "https://github.com/nasa/Rapid-Model-Import-Tool"
            };

            /*
            DONE: partFile.AssetBundle = "GLTF";
            DONE: partFile.PartName = new string[] { finalGLTFPath };
            DONE: partFile.NonInteractable = false;
            DONE: partFile.Notes = "Converted by RMIT";
            DONE: partFile.Subsystem = "Structures";
            DONE: partFile.Reference = "https://github.com/nasa/Rapid-Model-Import-Tool";
            DONE: partFile.EnableCollisions = new bool[] { false };
            DONE: partFile.EnableGravity = new bool[] { false };
            DONE: partFile.EnableInteraction = new bool[] { true };
            DONE: partFile.PartType1 = new PartTypePartType[] { PartTypePartType.Chassis };
            DONE: partFile.PartTransform = new UnityTransformType();
            DONE: partFile.PartTransform.Position = new Vector3Type();
            DONE: partFile.PartTransform.Rotation = new QuaternionType();
            DONE: partFile.PartTransform.Scale = new Vector3Type() { X = 1, Y = 1, Z = 1 };
            partFile.Name = Path.GetFileNameWithoutExtension(finalGLTFPath);
            */

            // Save out to the parts directory
            SerializationState partSerializedState = new SerializationState();
            StartCoroutine(ToXML(serializedPart, partSerializedState));

            // Wait for the coroutine to complete
            while (!partSerializedState.IsComplete) yield return serializedPart;

            // If the serialization failed, exit with an error
            if (partSerializedState.IsError)
            {
                LogError(partSerializedState.ErrorMessage, nameof(GltfToPartReentrant));
                yield break;
            }

            // Import part into scene if the project is loaded and we were told to import
            if (ProjectManager.Loaded && importAfterConvert)
            {
                bool loadingComplete = false;
                Action<InteractablePart> PartLoadedAction = (InteractablePart loaded) =>
                {
                    loadingComplete = true;
                    if (loaded != null)
                    {
                        Log("Successfully loaded imported RMIT file: " + loaded.name, nameof(GltfToPartReentrant));
                    }
                    else
                    {
                        LogError("A problem problem occurred instantiating the part: " + serializedPart.Name);
                    }
                };

                Log("Importing RMIT file: " + serializedPart.Name, nameof(GltfToPartReentrant));

                // Instantiate the part
                ProjectManager.PartManager.InstantiatePart(serializedPart, true, null, PartLoadedAction);

                // Wait for the instantiation to complete
                while (!loadingComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);
            }
        }
    }
}