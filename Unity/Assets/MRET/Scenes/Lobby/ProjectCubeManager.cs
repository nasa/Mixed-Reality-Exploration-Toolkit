// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine.UI;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;
using GOV.NASA.GSFC.XR.MRET.Configuration;
using GOV.NASA.GSFC.XR.MRET.SceneObjects;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.Scenes.Lobby
{
    public class ProjectCubeManager : InteractableSceneObject<InteractableSceneObjectType>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(ProjectCubeManager);

        public enum CubeType { Projects, Templates, Collaborative };

        public Image screenshot;
        public Text projectName;
        public Text lastEdited1, lastEdited2;
        public ProjectInfo projectToOpen;
        public Image billboardImage;
        public Text billboardName, billboardEdited;
        public CubeType cubeType = CubeType.Projects;

        /// <seealso cref="InteractableSceneObject{T}.AfterBeginTouch(InputHand)"/>
        protected override void AfterBeginTouch(InputHand hand)
        {
            base.AfterBeginTouch(hand);

            billboardImage.sprite = screenshot.sprite;
            billboardName.text = projectName.text;
            billboardEdited.text = lastEdited1.text;
        }

        /// <seealso cref="InteractableSceneObject{T}.DoClick(InputHand)"/>
        protected override void DoClick(InputHand hand)
        {
            base.DoClick(hand);

            switch (cubeType)
            {
                case CubeType.Projects:
                    MRET.ConfigurationManager.AddRecentProject(projectToOpen);
                    MRET.ModeNavigator.OpenProject(projectToOpen, false);
                    break;

                case CubeType.Templates:
                    MRET.ConfigurationManager.AddRecentTemplate(projectToOpen);
                    MRET.ModeNavigator.OpenProject(projectToOpen, false);
                    break;

                case CubeType.Collaborative:
                    MRET.ConfigurationManager.AddRecentCollaboration(projectToOpen);
                    MRET.ModeNavigator.OpenProject(projectToOpen, true);
                    break;

                default:
                    break;
            }
        }
    }
}