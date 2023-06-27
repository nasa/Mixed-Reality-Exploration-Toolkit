// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.Events;
using GOV.NASA.GSFC.XR.MRET.Animation;
using GOV.NASA.GSFC.XR.MRET.Schema;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.UI.Tools.FileBrowser;

namespace GOV.NASA.GSFC.XR.MRET.UI.Animation
{
    public class AnimationFileBrowserHelper : MonoBehaviour
    {
        private MRETAnimationManager animationManager;

        public FileBrowserManager fileBrowserManager;

        public UnityEvent successEvent;
        public UnityEvent failEvent;

        public void OpenAnimationFile()
        {
            if (!fileBrowserManager)
            {
                Debug.LogWarning(
                    "[AnimationFileBrowserHelper] FileBrowserManager not set.");
                return;
            }

            object selectedFile = fileBrowserManager.GetSelectedFile();
            if (selectedFile == null)
            {
                Debug.LogWarning(
                    "[AnimationFileBrowserHelper] No valid file selected.");
                return;
            }

            string selectedFilePath = fileBrowserManager.GetSelectedFilePath();
            if (string.IsNullOrEmpty(selectedFilePath))
            {
                Debug.LogWarning(
                    "[AnimationFileBrowserHelper] Unable to get selected file path.");
                return;
            }

            if (selectedFile is ActionSequenceType)
            {
                IActionSequence anim = animationManager.DeserializeAnimation(selectedFile as ActionSequenceType);

                if (anim == null)
                {
                    Debug.LogWarning("[AnimationFileBrowserHelper] Error loading animation.");
                    failEvent.Invoke();
                    return;
                }

                animationManager.AddSelectAnimation(anim);
                successEvent.Invoke();
            }
        }

        public void SaveAnimationFile()
        {
            string filePath = fileBrowserManager.GetSaveFilePath();
            string animationName = System.IO.Path.GetFileNameWithoutExtension(filePath);

            if (!string.IsNullOrEmpty(animationName))
            {
                //animationManager.ActiveAnimation.Name = animationName;
            }

            ActionSequenceType anim = animationManager.SerializeAnimation();

            if (string.IsNullOrEmpty(System.IO.Path.GetExtension(filePath)))
            {
                filePath = filePath + AnimationFileSchema.FILE_EXTENSION;
            }

            SchemaHandler.WriteXML(filePath, anim);
            successEvent.Invoke();
        }

        private void Start()
        {
            if (animationManager == null)
            {
                animationManager = FindObjectOfType<MRETAnimationManager>();
                if (animationManager == null)
                {
                    Debug.LogWarning(
                        "[AnimationFileBrowserHelper] Unable to find AnimationManager.");
                }
            }

            if (fileBrowserManager == null)
            {
                fileBrowserManager = GetComponent<FileBrowserManager>();
                if (fileBrowserManager == null)
                {
                    Debug.LogWarning(
                        "[AnimationFileBrowserHelper] Unable to find FileBrowserManager.");
                }
            }

            fileBrowserManager.OpenDirectory(MRET.ConfigurationManager.defaultAnimationDirectory);
        }
    }
}