/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
using UnityEngine;

namespace Assets.VDE.UI.Node
{
    internal class NodeFacer : Facer
    {
        Vector3 pos = new Vector3(0, -0.3F, 0);
        private void Start()
        {
            transform.localPosition = pos;
            if (!(textMess is null))
            {
                textMess.transform.localScale = Vector3.one * minTextSize;
            }
            else if(!(text is null))
            {
                text.transform.localScale = Vector3.one * minTextSize;
            }
        }
        private void Update()
        {
            Vector3 fwd = Camera.main.transform.forward;
            float distance = Vector3.Distance(Camera.main.transform.position, transform.position);
            if (distance < visibleOnceCameraIsCloserThan)
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
                transform.LookAt(Camera.main.transform.position);
                transform.Rotate(Vector3.up, 180);
                transform.localPosition = pos;
                transform.Translate(0, 0, -0.01F, Camera.main.transform);
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
