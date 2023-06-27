// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;

namespace GOV.NASA.GSFC.XR.MRET.UI.MiniMap
{
    public class MinimapMenuController : MonoBehaviour
    {

        //public VR_InputField zoomAmount;
        public TextMesh zoomAmount;

        public Minimap minimap;

        private string previousZoomAmount;

        public float numZoomAmount;

        private void Start()
        {
            //zoomAmount.text = minimap.zoomValue.ToString();
        }

        private void Update()
        {

            SetZoomAmount(zoomAmount);

            //if (zoomAmount.text != previousZoomAmount)
            //{
            //   previousZoomAmount = zoomAmount.text;
            //  SetZoomAmount(zoomAmount);
            // }
        }

        //public void SetZoomAmount(VR_InputField zoomToSet)
        public void SetZoomAmount(TextMesh zoomToSet)
        {
            numZoomAmount = string.IsNullOrEmpty(zoomToSet.text) ? 1 : float.Parse(zoomToSet.text);

            //  if (widthToSet.text != "")
            //  {
            //      if (float.TryParse(widthToSet.text, out float param)) {

            //        minimap.zoomValue = param;

            //   widthToSet = param;
            //SetZoomAmount(float.TryParse(widthToSet.text));\
            //       }
            //  }
        }


        /*
        public Toggle minimapToggle;
        public MinimapController minimapManager;

        private bool isInitalized = false;

        public void OnEnable()
        {
            isInitalized = true;
            eraserToggle.isOn = minimapManager.canErase;
            isInitalized = false;
        }

        public void ToggleMinimap()
        {

        }
        */



    }
}