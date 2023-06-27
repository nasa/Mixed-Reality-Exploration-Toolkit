// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace GOV.NASA.GSFC.XR.MRET.Extensions.MultimediaNotes
{
    /// <summary>
    /// This file resizes a TMP text box vertically as the user types so the text box wraps the text.
    /// </summary>
    public class DynamicTMPTextResizer : MonoBehaviour
    {
        public TMP_Text TextComponent;
        public Transform viewportTransform;
        public RectMask2D rectMask2D;
        public GameObject root;

        /// <summary>
        /// each iteration of update checks to see if the height of the typed text is greater than the textbox size and if
        /// so then the text box is resized to match the size of the typed text
        /// </summary>
        private void Update()
        {
            string text;

            text = TextComponent.text;
            float fontSize = TextComponent.fontSize;

            float textHeight = GetComponent<TMP_InputField>().textComponent.preferredHeight;
            float rectHeight = rectMask2D.GetComponent<RectTransform>().rect.height;

            if (textHeight > rectHeight)
            {
                Vector3 position;
                position = viewportTransform.localPosition;
                rectMask2D.GetComponent<RectTransform>().sizeDelta = new Vector2(rectMask2D.GetComponent<RectTransform>().sizeDelta.x, rectMask2D.GetComponent<RectTransform>().sizeDelta.y + fontSize);
                viewportTransform.localPosition = position;
                root.GetComponent<LayoutElement>().preferredHeight += fontSize;
            }
        }

    }
}