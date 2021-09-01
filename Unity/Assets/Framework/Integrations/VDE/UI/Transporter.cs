/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
using UnityEngine;

namespace Assets.VDE.UI
{
    internal class Transporter : MonoBehaviour
    {
        internal Vector3 velocity = Vector3.zero;
        internal Quaternion targetRotation, targetLocalRotation;
        internal Vector3 targetPosition, targetLocalPosition;
        internal Rigidbody mrb;
        private void Awake()
        {
            TryGetComponent(out mrb);
        }
        internal bool SetVelocity(Vector3[] velos)
        {
            if (!(mrb is null) && velos.Length > 1)
            {
                mrb.isKinematic = mrb.useGravity = false;
                mrb.mass = 0;
                mrb.velocity = velos[0];
                mrb.angularVelocity = velos[1];
                mrb.drag = 1;
                mrb.angularDrag = 1;
                return true;
            }
            return false;
        }
    }
}
