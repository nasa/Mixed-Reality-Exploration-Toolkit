using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class HudSaveMenuManager : MonoBehaviour {

    public Text saveHudName;
    private HudManager hudManager;
    private HudMenu hudMenu;
    private ConfigurationManager configManager;

    void Start ()
    {
        hudManager = FindObjectOfType<HudManager>();
        hudMenu = FindObjectOfType<HudMenu>();
        hudMenu.gameObject.SetActive(false);
        configManager = FindObjectOfType<ConfigurationManager>();
    }


    private void OnDisable()
    {
        hudMenu.gameObject.SetActive(true);
        Destroy(gameObject);
    }


    public void SaveHUD()
    {
        hudMenu.gameObject.SetActive(true);

        if(saveHudName.text == "" || saveHudName.text == null)
        {
            Debug.LogWarning("[HudSaveMenuManager->SaveHUD] No HUD name provided. HUD not saved.");
            return;
        }
        else if (saveHudName.text.Length >= 2)
        {
            if (saveHudName.text.Substring(0, 2) == "C:")
            {
                hudManager.SaveToXML(saveHudName.text + ".xml");
                Destroy(gameObject);
                return;
            }
            else
            {
                if(configManager != null && hudManager != null)
                {
                    if(configManager.config != null)
                    {
                        if(configManager.config.HudPath != null)
                        {
                            if(System.IO.Directory.Exists(Application.dataPath + Path.DirectorySeparatorChar + configManager.config.HudPath))
                            {
                                hudManager.SaveToXML(Application.dataPath + Path.DirectorySeparatorChar +
                                    configManager.config.HudPath + Path.DirectorySeparatorChar + saveHudName.text + ".xml");
                                //configManager.UpdateConfig();
                                Destroy(gameObject);
                                return;
                            }
                        }
                    }
                }
                Debug.LogWarning("[HudSaveMenuManager->SaveHUD] Unable to find location to save project. Project not saved.");
            }
        }

        Destroy(gameObject);

    }
}
