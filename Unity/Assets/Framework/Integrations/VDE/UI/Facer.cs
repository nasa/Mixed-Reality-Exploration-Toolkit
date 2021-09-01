/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
using TMPro;
using UnityEngine;

namespace Assets.VDE.UI
{
    internal class Facer : MonoBehaviour
    {
        internal float 
            visibleMaxDistanceFromCamera = 1F, 
            visibleMinDistanceFromCamera = 0.2F,
            maxTextSize = 0.12F,
            maxTextSizeDistance = 10,
            minTextSize = 0.1F,
            minTextSizeDistance = 1;
        internal bool withOutline;
        internal bool grabbed = false;
        internal GameObject referenceShape;
        internal TextMeshPro text;
        internal MeshRenderer mehRenderer;

        void Awake()
        {
            text = GetComponent<TextMeshPro>();
            if (withOutline)
            {
                text.outlineWidth = 0.2F;
            }
            mehRenderer = GetComponent<MeshRenderer>();
        }

        internal void Resize(float distance)
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
            Vector3 fwd = Camera.main.transform.forward;
            float distance = Vector3.Distance(Camera.main.transform.position, transform.position);
            if (distance < visibleMaxDistanceFromCamera && distance > visibleMinDistanceFromCamera)
            {
                if (!text.enabled)
                {
                    text.enabled = true;
                    mehRenderer.enabled = true;
                }
                fwd.y = 0.0F;

                transform.rotation = Quaternion.LookRotation(fwd);
                transform.localPosition = new Vector3(referenceShape.transform.localScale.x / 2, referenceShape.transform.localScale.y, referenceShape.transform.localScale.z / 2);
                Resize(distance);
            } 
            else if (text.enabled || mehRenderer.enabled)
            {
                text.enabled = false;
                mehRenderer.enabled = false;
            }
        }
    }
}
