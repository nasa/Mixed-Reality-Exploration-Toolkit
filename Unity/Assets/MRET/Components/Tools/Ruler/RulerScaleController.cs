// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Project;

namespace GOV.NASA.GSFC.XR.MRET.Tools.Ruler
{
    // TODO: This class is identical to PartPlacingScaleController
    public class RulerScaleController : MRETUpdateBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(RulerScaleController);

        public GameObject siRuler, imperialRuler;

        #region MRETUpdateBehaviour
        protected override void MRETUpdate()
        {
            base.MRETUpdate();

            // Update the ruler scales
            float scaleMultiplier = (ProjectManager.Project != null) ? ProjectManager.Project.ScaleMultiplier : 1f;
            siRuler.transform.localScale = new Vector3(scaleMultiplier, scaleMultiplier, scaleMultiplier);
            imperialRuler.transform.localScale = new Vector3(scaleMultiplier, scaleMultiplier, scaleMultiplier);
        }
        #endregion MRETUpdateBehaviour
    }
}