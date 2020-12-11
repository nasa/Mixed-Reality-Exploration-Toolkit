using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace GSFC.ARVR.MRET.Infrastructure
{
    public class KioskLoader : MonoBehaviour
    {
        public ModeNavigator modeNavigator;
        public ConfigurationManager configurationManager;
        public SessionConfiguration sessionConfiguration;

        private XmlNodeList xmlMenuControls;
        private XmlNodeList xmlOtherAttributes;
        private string projectToLoad;

        private Dictionary<string, bool> controlSelection;
        private Dictionary<string, bool> instSelection;

        private List<GameObject> controlsToChange;
        private List<string> instToChange;

        public void LoadKioskMode(string kioskModeFile)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(kioskModeFile); //path to xml file
            XmlNodeList xmlScene = xmlDoc.GetElementsByTagName("SceneName");
            xmlMenuControls = xmlDoc.GetElementsByTagName("MenuControls");
            xmlOtherAttributes = xmlDoc.GetElementsByTagName("Other");

            projectToLoad = xmlScene[0].InnerXml.ToString();

            StartCoroutine(LoadKioskModeAsync());
        }

        IEnumerator LoadKioskModeAsync()
        {
            yield return new WaitForSeconds(0.1f);
            loadScene(projectToLoad);
            SetMenuControls();
            SetOtherAttributes();
        }

        void loadScene(string scene)
        {
            ModeNavigator modeNavigator = FindObjectOfType<ModeNavigator>();

            foreach (ProjectInfo proj in configurationManager.projects)
            {
               if (proj.name.ToString().Equals(scene) && modeNavigator)
                { 
                        modeNavigator.OpenProject(proj, false); //open project that is predefined in the xml file
                        return;
                }
            }
        }

        private void SetMenuControls()
        {
            modeNavigator.EnableAllControls();

            foreach (XmlNode ctrl in xmlMenuControls[0].ChildNodes)
            {
                string name = ctrl.Name.ToString();
                bool setting = bool.Parse(ctrl.InnerXml.ToString());

                if (!string.IsNullOrEmpty(name))
                {
                    modeNavigator.SetMenuControl(name, setting);
                }
            }
        }

        private void SetOtherAttributes()
        {
            foreach (XmlNode attr in xmlOtherAttributes[0].ChildNodes)
            {
                switch (attr.Name)
                {
                    case "ObjectPanels":
                        sessionConfiguration.partPanelEnabled = bool.Parse(attr.InnerXml.ToString());
                        break;

                    default:
                        Debug.LogWarning("[KioskLoader] Unknown Other Attribute " + attr.Name);
                        break;
                }
            }
        }
    }
}