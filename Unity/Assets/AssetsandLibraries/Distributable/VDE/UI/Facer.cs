/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.VDE.UI
{
    internal class Facer : MonoBehaviour
    {
        internal float 
            visibleOnceCameraIsCloserThan = 1F, 
            visibleUntilCameraIsFartherThan = 0.2F,
            maxTextSize = 0.12F,
            maxTextSizeDistance = 10,
            minTextSize = 0.1F,
            minTextSizeDistance = 1;
        internal bool withOutline;
        internal bool grabbed = false;
        internal GameObject referenceShape;
        internal TextMeshPro textMess;
        internal Text text;
        internal MeshRenderer mehRenderer;

        void Awake()
        {
            TryGetComponent<TextMeshPro>(out textMess);
            TryGetComponent<MeshRenderer>(out mehRenderer);
            //textMess = GetComponent<TextMeshPro>();
            if (withOutline && !(textMess is null))
            {
                textMess.outlineWidth = 0.2F;
            }
            //mehRenderer = GetComponent<MeshRenderer>();
        }

        /// <summary>
        /// resize for textmess.
        /// </summary>
        /// <param name="distance"></param>
        internal void ResizeTextMess(float distance)
        {
            if (distance > maxTextSizeDistance)
            {
                textMess.transform.localScale = Vector3.one * maxTextSize;
            }
            else if (distance < minTextSizeDistance)
            {
                textMess.transform.localScale = Vector3.one * minTextSize;
            }
            else
            {
                float scaledToDistance = (((distance - minTextSizeDistance) / (maxTextSizeDistance - minTextSizeDistance)) + 1) * minTextSize;
                if (scaledToDistance < minTextSize)
                {
                    textMess.transform.localScale = Vector3.one * minTextSize;
                }
                else
                {
                    textMess.transform.localScale = Vector3.one * scaledToDistance;
                }
            }
        }

        /// <summary>
        /// resize for UI.text.
        /// </summary>
        /// <param name="distance"></param>
        internal void ResizeText(float distance)
        {
            if (distance > maxTextSizeDistance)
            {
                text.transform.localScale = Vector3.one * maxTextSize;
            }
            else if (distance < minTextSizeDistance)
            {
                text.transform.localScale = Vector3.one * minTextSize;
            }
            else
            {
                float scaledToDistance = (((distance - minTextSizeDistance) / (maxTextSizeDistance - minTextSizeDistance)) + 1) * minTextSize;
                if (scaledToDistance < minTextSize)
                {
                    text.transform.localScale = Vector3.one * minTextSize;
                }
                else
                {
                    text.transform.localScale = Vector3.one * scaledToDistance;
                }
            }
        }

        private void Update()
        {
            if (!(referenceShape is null) && referenceShape.activeSelf && gameObject.activeSelf)
            {
                Vector3 fwd = Camera.main.transform.forward;
                float distance = Vector3.Distance(Camera.main.transform.position, transform.position);
                if (distance < visibleOnceCameraIsCloserThan && distance > visibleUntilCameraIsFartherThan)
                {
                    if (!(textMess is null) && !textMess.enabled)
                    {
                        textMess.enabled = true;
                        mehRenderer.enabled = true;
                    }
                    else if (!(text is null) && !text.enabled)
                    {
                        text.enabled = true;
                    }
                    fwd.y = 0.0F;

                    transform.rotation = Quaternion.LookRotation(fwd);
                    transform.localPosition = new Vector3(referenceShape.transform.localScale.x / 2, referenceShape.transform.localScale.y, referenceShape.transform.localScale.z / 2);
                    if (!(textMess is null))
                    {
                        ResizeTextMess(distance);
                    }
                    else if (!(text is null))
                    {
                        ResizeText(distance);
                    }
                }
                else if (!(textMess is null) && textMess.enabled)
                {
                    textMess.enabled = false;
                    mehRenderer.enabled = false;
                }
                else if (!(text is null) && text.enabled)
                {
                    text.enabled = false;
                }
            }
        }
    }
}
