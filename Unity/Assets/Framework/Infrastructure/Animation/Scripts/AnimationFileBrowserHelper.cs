using UnityEngine;
using UnityEngine.Events;
using GSFC.ARVR.MRET.Common;
using GSFC.ARVR.MRET.Common.Schemas;

namespace GSFC.ARVR.MRET.Animation
{
    public class AnimationFileBrowserHelper : MonoBehaviour
    {
        public FileBrowserManager fileBrowserManager;
        public AnimationMenuController animationMenuController;

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
                MRETAnimation anim = MRETAnimation.Deserialize((AnimationType) selectedFile);

                if (anim == null)
                {
                    Debug.LogWarning("[AnimationFileBrowserHelper] Error loading animation.");
                    failEvent.Invoke();
                    return;
                }

                animationMenuController.loopToggle.isOn = anim.loop;
                animationMenuController.autoplayToggle.isOn = anim.autoplay;
                animationMenuController.SetAnimation(anim);
                successEvent.Invoke();
            }
        }

        public void SaveAnimationFile()
        {
            AnimationType anim = animationMenuController.activeAnimation.Serialize(
                animationMenuController.loopToggle.isOn,
                    animationMenuController.autoplayToggle.isOn);
            anim.Loop = animationMenuController.loopToggle.isOn;
            anim.Autoplay = animationMenuController.autoplayToggle.isOn;

            SchemaHandler.WriteXML(fileBrowserManager.GetSaveFilePath(), anim);
            successEvent.Invoke();
        }

        private void Start()
        {
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