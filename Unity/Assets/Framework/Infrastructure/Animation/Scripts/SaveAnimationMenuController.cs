using System.IO;
using UnityEngine;
using UnityEngine.UI;
using GSFC.ARVR.MRET.Common.Schemas;

public class SaveAnimationMenuController : MonoBehaviour
{
    public Text saveAnimationName;
    public AnimationMenuController animationMenuController;

    private ConfigurationManager configManager;

    void Start()
    {
        configManager = FindObjectOfType<ConfigurationManager>();
    }

    public void SaveAnimation()
    {
        AnimationType anim = animationMenuController.activeAnimation.Serialize();
        anim.Loop = animationMenuController.loopToggle.isOn;
        anim.Autoplay = animationMenuController.autoplayToggle.isOn;
        int start = saveAnimationName.text.LastIndexOf("/");
        start = (start < 0) ? 0 : start + 1;
        int end = saveAnimationName.text.LastIndexOf(".");
        end = (end < 0) ? saveAnimationName.text.Length - 1 : end;
        anim.Name = saveAnimationName.text.Substring(start, end);

        if (saveAnimationName.text == "" || saveAnimationName.text == null)
        {
            Debug.LogWarning("[SaveAnimationMenuController->SaveAnimation] No animation name provided. Animation not saved.");
            return;
        }
        else if (saveAnimationName.text.Length >= 2)
        {
            if (saveAnimationName.text.Substring(0, 2) == "C:")
            {
                MRETAnimation.SaveToXML(saveAnimationName.text + ".mtanim",
                    animationMenuController.activeAnimation, animationMenuController.loopToggle.isOn,
                    animationMenuController.autoplayToggle.isOn);
                return;
            }
            else
            {
                if (configManager != null)
                {
                    if (configManager.config != null)
                    {
                        if (configManager.config.AnimationsPath != null)
                        {
                            if (System.IO.Directory.Exists(Application.dataPath + Path.DirectorySeparatorChar + configManager.config.AnimationsPath))
                            {
                                MRETAnimation.SaveToXML(Application.dataPath
                                    + Path.DirectorySeparatorChar + configManager.config.AnimationsPath + Path.DirectorySeparatorChar + saveAnimationName.text + ".mtanim",
                                    animationMenuController.activeAnimation, animationMenuController.loopToggle.isOn,
                                    animationMenuController.autoplayToggle.isOn);
                                return;
                            }
                        }
                    }
                }
                Debug.LogWarning("[SaveAnimationMenuController->SaveAnimation] Unable to find location to save animation. Animation not saved.");
            }
        }
    }
}