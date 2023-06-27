/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
using System;
using UnityEngine;

namespace Assets.VDE.UI.Node
{
    class Transporter : Assets.VDE.UI.Transporter
    {
        internal Container node;
        GOV.NASA.GSFC.XR.MRET.SceneObjects.User.IUser user;

#if MRET_2021_OR_LATER
        private void Awake()
        {
            TryGetComponent(out mrb);
        }
#endif
        private void Update()
        {
            if (Vector3.Distance(transform.position, targetPosition) < 0.003)
            {
                mrb.velocity = mrb.angularVelocity = Vector3.zero;
                mrb.isKinematic = true;
                transform.localRotation = targetLocalRotation;
                transform.localPosition = targetLocalPosition;
                node.UpdateLinks();
                UpdatePositionToXRC();
#if MRET_2021_OR_LATER
                if (node.TryGetComponent(out GOV.NASA.GSFC.XR.MRET.SceneObjects.IInteractable ipp))
                {
                    Destroy(ipp as Component);
                }
#endif
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
            UpdatePositionToXRC();
        }

        private void UpdatePositionToXRC()
        {
#if MRET_2021_OR_LATER
            try
            {
                /* Not needed. User is auto updated.
                user = GOV.NASA.GSFC.XR.MRET.MRET.CollaborationManager.GetLocalUser();
                if (!(user is null))
                {
                    user.transform.position = transform.position;
                    GOV.NASA.GSFC.XR.MRET.MRET.CollaborationManager.UpdateSessionEntityPosition(user);
                }
                */
            }
            catch (Exception exe)
            {
                //Debug.LogWarning("exe: " + exe.Message + "\n" + exe.StackTrace);
            }
#endif
        }
    }
}
