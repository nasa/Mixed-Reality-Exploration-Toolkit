// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.Events;
using GSFC.ARVR.MRET.Common;
using GSFC.ARVR.MRET.Common.Schemas;

namespace GSFC.ARVR.MRET.Infrastructure.Framework.Animation
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

            if (selectedFile is AnimationType)
            {
                MRETBaseAnimation anim = animationManager.DeserializeAnimation((AnimationType) selectedFile);

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

            AnimationType anim = animationManager.SerializeAnimation();

            if (string.IsNullOrEmpty(System.IO.Path.GetExtension(filePath)))
            {
                filePath = filePath + AnimationFileSchema.fileExtension;
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

            fileBrowserManager.OpenDirectory(ConfigurationManager.instance.defaultAnimationDirectory);
        }
    }
}