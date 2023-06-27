/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.VDE.UI.Input
{
    public class Observer : MonoBehaviour
    {
        [System.Serializable]
        internal class Event : UnityEvent<Input.Event> { }
        internal Event inputEvent = new Event();
        internal LayerMask gazeMask, pointerMask, grabingMask;
        /// <summary>
        /// DO NOT overwrite Start in an extension, othewrise the mask values will not be set and raycast wont work. at least not correctly.
        /// </summary>
        private void Start()
        {
            gazeMask = new LayerMask()
            {
                value = LayerMask.GetMask("gazable")
            };
            pointerMask = new LayerMask()
            {
                value = LayerMask.GetMask("pointable")
            };
            grabingMask = new LayerMask()
            {
                value = LayerMask.GetMask("grabbable")
            };
        }
        internal bool CastRay(Quaternion rot, Vector3 pos, out RaycastHit hit, LayerMask mask)
        {
            if (Physics.Raycast(pos, rot * Vector3.forward, out hit, 130F))
            {
                return true;
            }
            if (Physics.Raycast(pos, rot * Vector3.forward, out hit, 130F, mask, QueryTriggerInteraction.Collide))
            {
                return true;
            }
            return false;
        }
        internal bool CastRay(Vector3 rot, Vector3 pos, out RaycastHit hit, LayerMask mask)
        {
            if (Physics.Raycast(pos, rot, out hit, 130F))
            {
                return true;
            }
            if (Physics.Raycast(pos, rot, out hit, 130F, mask, QueryTriggerInteraction.Collide))
            {
                return true;
            }
            return false;
        }
        internal bool CastSphere(Vector3 pos, out RaycastHit hit, LayerMask mask, float range = 0.5F)
        {
            if (Physics.SphereCast(pos, range, transform.forward, out hit, 0, mask, QueryTriggerInteraction.Collide))
            {
                return true;
            }
            return false;
        }

        internal bool CastSphere(Transform trafo, Vector3 offset, out GameObject hit, LayerMask mask, float range = 0.5F)
        {
            System.Collections.Generic.List<RaycastHit> grabbingHits = Physics.SphereCastAll(
                trafo.position + offset,
                range,
                trafo.right,
                0.001f,
                mask
            ).ToList();

            if (grabbingHits.Count > 0)
            {
                grabbingHits.OrderByDescending(finding => Vector3.Distance(finding.point, trafo.position));
                hit = grabbingHits.FirstOrDefault().transform.gameObject;
                return true;
            }

            hit = null;
            return false;
        }
    }
}
