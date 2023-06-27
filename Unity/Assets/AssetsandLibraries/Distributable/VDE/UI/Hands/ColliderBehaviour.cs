/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
using UnityEngine;

namespace Assets.VDE.UI.Hands
{
    internal class ColliderBehaviour : MonoBehaviour
    {
        internal Hand hand;
        public Mode mode;
        internal enum Mode
        {
            GrippingPoint,
            Select,
            Grab,
            None
        }
        private void OnTriggerEnter(Collider other)
        {
            DoSomething(other, true);
        }
        private void OnTriggerStay(Collider other)
        {
            if (hand.grabbing)
            {
                DoSomething(other, true);
            }
        }
        private void OnTriggerExit(Collider other)
        {
            DoSomething(other, false);
        }
        private void DoSomething(Collider other, bool setTo)
        {
            switch (mode)
            {
                case Mode.Select:
                    if (!hand.grabbing && other.gameObject.TryGetComponent(out Node.Container conta1ner)) //&& other.gameObject.TryGetComponent(out Node.Shape shape))
                    {
                        if (setTo)
                        {
                            Node.Shape shape = (Node.Shape) conta1ner.shapes.GetNodeShape();
                            if (!(shape is null))
                            {
                                shape.Select();
                            }
                        }
                        else
                        {
                            //shape.UnSelect();
                        }
                    }
                    else if (!hand.grabbing && other.gameObject.TryGetComponent(out Link link))
                    {
                        link.SetFocus(setTo);
                    }
                    break;
                case Mode.Grab:
                    if (hand.grabbing && hand.grabbedContainer is null && other.gameObject.TryGetComponent(out Node.Container container))
                    {
                        hand.grabbedContainer = container;
                        if (setTo)
                        {
                            if(other.gameObject.TryGetComponent(out Node.Shape shape2))
                            {
                                shape2.Select();
                            }
                            container.GrabbedBy(hand.gripPoint);
                        }
                        else
                        {
                            container.Released(hand);
                        }
                    }
                    break;
                default:
                    break;
            }
        }
    }
}