/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
//#define MRET
using UnityEngine;

namespace Assets.VDE.UI
{
    public class Lazer : Input.Observer
    {
        public Transform laserBeamTransform;

        float defaultPointerLength = 6;

        private void Awake()
        {
            if (laserBeamTransform is null)
            {
                try
                {
#if MRET
                    GSFC.ARVR.MRET.Infrastructure.Components.UIInteraction.UIPointerController uip = gameObject.GetComponentInChildren<GSFC.ARVR.MRET.Infrastructure.Components.UIInteraction.UIPointerController>();
                    if (!(uip is null))
                    {
                        laserBeamTransform = uip.transform;
                    }
#endif
                }
                catch (System.Exception)
                {
                    throw;
                }
            }
            else
            {
                laserBeamTransform.gameObject.SetActive(false);
            }
        }

        protected void Update()
        {
            float pointerLengthInThisFrame = defaultPointerLength;

            if (laserBeamTransform && laserBeamTransform.gameObject.activeSelf)
            {
                RaycastHit hit;
                if (Physics.Raycast(this.transform.position, this.transform.forward, out hit, pointerLengthInThisFrame, pointerMask, QueryTriggerInteraction.Collide))
                {
                    pointerLengthInThisFrame = Vector3.Distance(hit.point, this.transform.position);
                    inputEvent.Invoke(new Input.Event
                    {
                        function = Input.Event.Function.PointAt,
                        type = Input.Event.Type.GameObject,
                        GameObject = hit.collider.gameObject
                    });
                } else
                {
                    inputEvent.Invoke(new Input.Event
                    {
                        function = Input.Event.Function.PointAt,
                        type = Input.Event.Type.Bool,
                        Bool = false
                    });
                }
                laserBeamTransform.localScale = new Vector3(laserBeamTransform.localScale.x, laserBeamTransform.localScale.y, pointerLengthInThisFrame);
                laserBeamTransform.localPosition = new Vector3(0, 0, pointerLengthInThisFrame / 2);
            }
        }
    }
}
