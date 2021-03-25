// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GSFC.ARVR.MRET.Common.Schemas;

namespace GSFC.ARVR.MRET.Infrastructure.Framework.Animation
{
    /// <summary>
    /// The main Animation Panel controller responsible for managing the different animation 
    /// related panels.
    /// </summary>
    public class AnimationPanelsMenuController : MonoBehaviour
    {
        public static readonly string NAME = nameof(AnimationPanelsMenuController);

        [Tooltip("The animation panel to display that allows the user to play an animation")]
        public GameObject animationPanel;

        [Tooltip("The loading panel to display that allows the user to select an animation file")]
        public GameObject loadPanel;

        [Tooltip("The save panel to display that allows the user to save out the animation to a file")]
        public GameObject savePanel;

        [Tooltip("The editor panel to display that allows the user to edit animations")]
        public GameObject editorPanel;

        public void Start()
        {
            Initialize();
        }

        /// <summary>
        /// Initializes the animation panel and verifies the properties are properly set. Called from 
        /// Start().
        /// </summary>
        public void Initialize()
        {
            //UnityProject proj = FindObjectOfType<UnityProject>();
            //if (proj)
            //{
            //    transform.SetParent(proj.animationPanelsContainer.transform);
            //}
            bool errorState = false;

            // Validate properties
            if (animationPanel == null)
            {
                Debug.LogError("[" + NAME + "]: Animation Panel is not assigned.");
                errorState = true;
            }
            if (loadPanel == null)
            {
                Debug.LogError("[" + NAME + "]: Load Panel is not assigned.");
                errorState = true;
            }
            if (savePanel == null)
            {
                Debug.LogError("[" + NAME + "]: Save Panel is not assigned.");
                errorState = true;
            }
            if (editorPanel == null)
            {
                //Debug.LogError("[" + NAME + "]: Editor Panel is not assigned.");
                //errorState = true;
            }

            // Open up the main animation panel
            if (!errorState)
            {
                OpenMainPanel();
            }
        }

        /// <summary>
        /// Opens the main animation panel and deactivates other panels.
        /// </summary>
        public void OpenMainPanel()
        {
            animationPanel.SetActive(true);
            loadPanel.SetActive(false);
            savePanel.SetActive(false);
            //editorPanel.SetActive(false);
        }

        /// <summary>
        /// Opens the animation load panel and deactivates other panels.
        /// </summary>
        public void OpenLoadPanel()
        {
            animationPanel.SetActive(false);
            loadPanel.SetActive(true);
            savePanel.SetActive(false);
            //editorPanel.SetActive(false);
        }

        /// <summary>
        /// Opens the animation save panel and deactivates other panels.
        /// </summary>
        public void OpenSavePanel()
        {
            animationPanel.SetActive(false);
            loadPanel.SetActive(false);
            savePanel.SetActive(true);
            //editorPanel.SetActive(false);
        }

        /// <summary>
        /// Opens the animation editor panel and deactivates other panels.
        /// </summary>
        public void OpenEditorPanel()
        {
            animationPanel.SetActive(false);
            loadPanel.SetActive(false);
            savePanel.SetActive(false);
            //editorPanel.SetActive(true);
        }
    }
}