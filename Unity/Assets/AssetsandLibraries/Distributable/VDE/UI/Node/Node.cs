/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */

using System;
using System.Collections;
using UnityEngine;

namespace Assets.VDE.UI.Node
{
#if MRET_2021_OR_LATER && TMP20210616
    internal class Container : Assets.VDE.UI.Container, GOV.NASA.GSFC.XR.MRET.Tools.Selection.ISelectable
#else
    internal class Container : Assets.VDE.UI.Container
#endif
    {
        private void Start()
        {
#if MRET_2021_OR_LATER && TMP20210616
            grabBehavior = GrabBehavior.Attach;
            touchBehavior = TouchBehavior.Highlight;
            grabbable = true;
            useable = true;
#endif
            // this must be changed if nodes are also depending on joints.
            if (!(messenger is null))
            {
                ready = true;
                messenger.Post(new Communication.Message()
                {
                    LayoutEvent = Layouts.Layouts.LayoutEvent.GotContainer,
                    EventOrigin = Layouts.Layouts.EventOrigin.Node,
                    obj = new object[] { entity, this, gameObject },
                    to = entity.parent,
                    from = entity
                });
            }
        }
        
        internal IEnumerator HuntForParent()
        {
            while (entity.parent is null || entity.parent.containers.GetGroup(entity.waitingForNodeInLayout) is null)
            {
                yield return new WaitForSeconds(123 / 100);
                //yield return data.UI.Sleep(123);
            }
            gameObject.transform.SetParent(entity.parent.containers.GetGroup(entity.waitingForNodeInLayout).transform);
            Init(entity, entity.waitingForNodeInLayout);

            if (!ready)
            {
                ready = true;
                messenger.Post(new Communication.Message()
                {
                    LayoutEvent = Layouts.Layouts.LayoutEvent.GotContainer,
                    EventOrigin = Layouts.Layouts.EventOrigin.Node,
                    obj = new object[] { entity, this, gameObject },
                    to = entity.parent,
                    from = entity
                });
            }
            yield return true;
        }
        internal override void Released(Hands.Hand hand)
        {
            isGrabbed = false;
            transform.SetParent(parentTrafo);
            transform.localRotation = localRotationPriorToGrab;
            Transporter vw = gameObject.AddComponent<Transporter>();
            vw.node = this;
            vw.targetPosition = parentTrafo.TransformPoint(defaultPosition);
            vw.targetRotation = defaultRotation;
            vw.targetLocalPosition = defaultPosition;
            vw.targetLocalRotation = defaultRotation;
            if (hand is null)
            {
                vw.SetVelocity(new Vector3[] { Vector3.zero, Vector3.zero });
            }
            else
            {
                vw.SetVelocity(hand.GetVelos());
            }
        }
#if MRET_2021_OR_LATER && TMP20210616
        public void Select(bool hierarchical = true)
        {
            log.Entry("selected");
        }

        public void Deselect(bool hierarchical = true)
        {
            log.Entry("deselected");
        }
        public override void BeginGrab(GOV.NASA.GSFC.XR.CrossPlatformInputSystem.InputHand hand)
        {
            log.Entry("BeginGrab (" + grabbable + ")");
            if (!grabbable)
            {
                return;
            }

            switch (grabBehavior)
            {
                case GrabBehavior.Attach:
                    AttachTo(hand.transform);
                    GrabbedBy(hand.gameObject);
                    break;

                case GrabBehavior.Custom:
                default:
                    Debug.LogWarning("BeginGrab() not implemented for SceneObject.");
                    break;
            }
        }

        public override void EndGrab(GOV.NASA.GSFC.XR.CrossPlatformInputSystem.InputHand hand)
        {
            log.Entry("EndGrab ("+grabbable+")");
            if (!grabbable)
            {
                return;
            }

            switch (grabBehavior)
            {
                case GrabBehavior.Attach:
                    Detach();
                    Released(null);
                    break;

                case GrabBehavior.Custom:
                default:
                    Debug.LogWarning("EndGrab() not implemented for SceneObject.");
                    break;
            }
        }
        protected override void BeginTouch(GOV.NASA.GSFC.XR.CrossPlatformInputSystem.InputHand hand)
        {
            log.Entry("BeginTouch");
            //(shapes.GetShape(Entity.Type.Node) as Shape).Select();
            data.entities.NodeInFocus = (Shape) shapes.GetShape(Entity.Type.Node);
            data.VDE.controller.inputObserver.inputEvent.Invoke(new Assets.VDE.UI.Input.Event
            {
                function = Assets.VDE.UI.Input.Event.Function.Select,
                type = Assets.VDE.UI.Input.Event.Type.Bool
            });
        }
        protected override void EndTouch()
        {
        }
#endif
    }
}
