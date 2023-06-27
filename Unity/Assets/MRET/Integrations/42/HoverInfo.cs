// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GOV.NASA.GSFC.XR.MRET.Integrations.FortyTwo
{
    public class HoverInfo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Text InfoTextBox;
        private GameObject childText = null;

        private FortyTwoMenuController fortyTwoMenuController;

        void Start()
        {
            if (InfoTextBox != null)
            {
                childText = InfoTextBox.gameObject;
                childText.SetActive(false);
            }

        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            childText.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            childText.SetActive(false);
        }
    }
}