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
        SynchronizedUser user;
#if MRET_2021_OR_LATER
        private void Awake()
        {
            TryGetComponent(out mrb);
            user = GSFC.ARVR.MRET.Infrastructure.Framework.MRET.XRCManager.GetControlledUser();
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
                if (node.TryGetComponent(out InteractablePart ipp))
                {
                    Destroy(ipp);
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
                user = GSFC.ARVR.MRET.Infrastructure.Framework.MRET.XRCManager.GetControlledUser();
                if (!(user is null))
                {
                    GSFC.ARVR.MRET.XRC.XRCUnity.UpdateEntityPosition(user.userAlias, GSFC.ARVR.MRET.XRC.XRCManager.OBJECTCATEGORY, transform.position, (node.entity.uuid is null) ? "000" : node.entity.uuid, user.uuid.ToString(), GSFC.ARVR.XRC.UnitType.meter);
                }
            }
            catch (Exception exe)
            {
                //Debug.LogWarning("exe: " + exe.Message + "\n" + exe.StackTrace);
            }
#endif
        }
    }
}
