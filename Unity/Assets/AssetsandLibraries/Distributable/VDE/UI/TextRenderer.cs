/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
using UnityEngine;
using UnityEngine.UI;

namespace Assets.VDE.UI
{
    public class TextRenderer : MonoBehaviour
    {
        public Font fontForRenderer;

        public static GameObject CreateTextMeshBox(
            string textToDisplay,
            GameObject parentGameObject,
            Vector3 localScale,
            Vector3 offsetToParentPosition,
            Font font,
            TextAnchor textAnchor = TextAnchor.MiddleCenter,
            string tag = "nodeText",
            bool isEnabled = true,
            int fontSize = 42,
            float czarSize = 0.33F,
            bool positionToLocal = false)
        {
            RectTransform rect;
            TextMesh text;
            GameObject CreatedTextMeshBox = null;

            if (CreatedTextMeshBox is null)
            {
                CreatedTextMeshBox = new GameObject(tag + "_" + textToDisplay);
                rect = CreatedTextMeshBox.AddComponent<RectTransform>();
                text = CreatedTextMeshBox.AddComponent<TextMesh>();
            }
            else
            {
                rect = CreatedTextMeshBox.GetComponent<RectTransform>();
                text = CreatedTextMeshBox.GetComponent<TextMesh>();
            }

            CreatedTextMeshBox.transform.SetParent(parentGameObject.transform);
            CreatedTextMeshBox.transform.localScale = localScale;
            if (positionToLocal)
            {
                CreatedTextMeshBox.transform.localPosition = parentGameObject.transform.localPosition + offsetToParentPosition;
            }
            else
            {
                CreatedTextMeshBox.transform.position = parentGameObject.transform.position + offsetToParentPosition;
            }
            CreatedTextMeshBox.tag = tag;

            rect.pivot.Set(0.5F, 0.5F);
            rect.localScale = new Vector3(0.45F, 0.45F, 0.45F);

            text.anchor = textAnchor;
            text.characterSize = czarSize;
            text.text = textToDisplay;
            text.font = font;
            text.tabSize = 4;
            text.fontSize = fontSize;
            text.color = Color.white;
            MeshRenderer rend = text.GetComponentInChildren<MeshRenderer>();
            rend.material = text.font.material;
            CreatedTextMeshBox.SetActive(isEnabled);

            return CreatedTextMeshBox;
        }
        public GameObject CreateTextBox(string text, GameObject parentGameObject, Vector3 localScale, Vector3 offsetToWorldPosition, string tag = "nodeText", bool isEnabled = true, float dynamicPixelsPerUnit = 50F, float scaleFactor = 1F)
        {
            Vector3 fwd = Camera.main.transform.forward;
            fwd.y = 0.0F;

            GameObject go = new GameObject(tag + "_" + text);
            go.transform.parent = parentGameObject.transform;
            go.transform.SetPositionAndRotation(parentGameObject.transform.position + offsetToWorldPosition, Quaternion.LookRotation(fwd));
            go.tag = tag;

            Canvas canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            CanvasScaler cs = go.AddComponent<CanvasScaler>();
            cs.scaleFactor = scaleFactor;
            cs.dynamicPixelsPerUnit = dynamicPixelsPerUnit;
            cs.referencePixelsPerUnit = 1f;

            go.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 3.0f);
            go.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 3.0f);
            go.GetComponent<RectTransform>().localScale = localScale;

            Text t = go.AddComponent<Text>();
            t.alignment = TextAnchor.MiddleCenter;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.font = fontForRenderer;
            t.fontSize = 1;
            t.text = text;
            t.enabled = true;
            t.color = Color.white;

            go.SetActive(isEnabled);

            return go;
        }
    }
}
