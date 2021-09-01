/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
using UnityEngine;

namespace Assets.VDE.UI.Node
{
    class Transporter : Assets.VDE.UI.Transporter
    {
        internal Container node;

        private void Update()
        {
            if (Vector3.Distance(transform.position, targetPosition) < 0.003)
            {
                mrb.velocity = mrb.angularVelocity = Vector3.zero;
                mrb.isKinematic = true;
                transform.localRotation = targetLocalRotation;
                transform.localPosition = targetLocalPosition;
                node.UpdateLinks();
                Destroy(this);
            }
            else
            {
                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, Time.deltaTime / 0.06F);
            }
        }
        private void LateUpdate()
        {
            node.UpdateLinks();
        }
    }
}
