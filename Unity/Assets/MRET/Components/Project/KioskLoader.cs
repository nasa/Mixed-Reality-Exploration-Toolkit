// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System.Xml;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Configuration;

namespace GOV.NASA.GSFC.XR.MRET.Project
{
    /// <remarks>
    /// History:
    /// 6 February 2021: Updated to extend from MRETBehavior and reimplemented LoadScene to read an explicit
    ///     or relative path because the list loading of projects in ConfigurationManager is obsolete (J. Hosler)
    /// 16 February 2021: Extended MRETBehavior and updated logging (J. Hosler)
    /// </remarks>
	///
	/// <summary>
	/// KioskLoader
	///
	/// Provides for dynamic loading of a MRET project/scene
	///
    /// Author: Chris Trombley
	/// </summary>
	/// 
    public class KioskLoader : MRETBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(KioskLoader);

        public ModeNavigator modeNavigator;

        private XmlNodeList xmlMenuControls;
        private XmlNodeList xmlOtherAttributes;
        private string projectToLoad;

        /// <seealso cref="MRETBehaviour.IntegrityCheck"/>
        protected override IntegrityState IntegrityCheck()
        {
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            return (
                (state == IntegrityState.Failure) ||
                (modeNavigator == null)
                    ? IntegrityState.Failure   // Fail is base class fails or anything is null
                    : IntegrityState.Success); // Otherwise, our integrity is valid
        }

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            // Take the inherited behavior
            base.MRETStart();

            // Obtain a reference to a ModeNavigator if one not exlicitly assigned
            if (modeNavigator == null)
            {
                modeNavigator = MRET.ModeNavigator;
            }

            // Log an issue if we don't have a valid ModeNavigator
            if (modeNavigator == null)
            {
                LogError("Invalid state encountered. Unable to locate " + nameof(ModeNavigator) + "...");
            }
        }

        /// <summary>
        /// LoadKioskModeAsync
        /// 
        /// Provides asynchronous loading of the Kiosk scene
        /// </summary>
        /// <returns>IEnumerator</returns>
        IEnumerator LoadKioskModeAsync()
        {
            yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);
            LoadScene(projectToLoad);
            SetMenuControls();
            SetOtherAttributes();
        }

        /// <summary>
        /// LoadScene
        /// 
        /// Locates and loads the the supplied scene
        /// </summary>
        /// <param name="scene"></param>
        private void LoadScene(string scene)
        {
            // Locate the project file
            string projectFile = scene;
            if (!System.IO.File.Exists(projectFile))
            {
                projectFile = System.IO.Path.Combine(ConfigurationManager.Instance.defaultProjectDirectory, scene);
                if (!System.IO.File.Exists(projectFile))
                {
                    projectFile = System.IO.Path.Combine(Application.dataPath, scene);
                    if (!System.IO.File.Exists(projectFile))
                    {
                        projectFile = "";
                    }
                }
            }

            // if we have a valid project filename and valid ModeNavigator
            if (!string.IsNullOrEmpty(projectFile) && modeNavigator)
            {
                // Open projectidentified in the xml file
                modeNavigator.OpenProject(projectFile, false);
            }
            else
            {
                LogWarning("Unable to load project: '" + scene + "'", nameof(LoadScene));
            }
        }

        /// <summary>
        /// SetMenuControls
        /// 
        /// Initializes the menu controls for this scene
        /// </summary>
        private void SetMenuControls()
        {
            // Default to all controls enabled
            modeNavigator.EnableAllControls();

            // Go through each of the XML settings and override the enabled controls
            foreach (XmlNode ctrl in xmlMenuControls[0].ChildNodes)
            {
                string name = ctrl.Name;
                if (!string.IsNullOrEmpty(name))
                {
                    bool setting = bool.Parse(ctrl.InnerXml);

                    // Set the control
                    modeNavigator.SetMenuControl(name, setting);
                }
            }
        }

        /// <summary>
        /// SetOtherAttributes
        /// 
        /// Initializes the "Other" settings for the scene
        /// </summary>
        private void SetOtherAttributes()
        {
            foreach (XmlNode attr in xmlOtherAttributes[0].ChildNodes)
            {
                switch (attr.Name)
                {
                    case "ObjectPanels":
                        ProjectManager.ObjectConfigurationPanelEnabled = bool.Parse(attr.InnerXml.ToString());
                        break;

                    default:
                        LogWarning("Unknown Other Attribute " + attr.Name, nameof(SetOtherAttributes));
                        break;
                }
            }
        }

        /// <summary>
        /// LoadKioskMode
        /// 
        /// Called to load the Kiosk mode file specified by the supplied argument
        /// </summary>
        /// <param name="kioskModeFile">The Kiosk file to load</param>
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

    }
}