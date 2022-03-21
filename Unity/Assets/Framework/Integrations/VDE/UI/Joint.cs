/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
using Assets.VDE.Communication;
using Assets.VDE.Layouts;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.VDE.UI
{
    internal class Joint
    {
        Log log;
        float wiggle = 0.00001F;
        internal string name;
        internal Entity
            src,
            dst;
        internal Container
            srcC,
            dstC;
        internal Shape
            srcShape = null,
            dstShape = null;
        internal GameObject
            srcGO = null,
            dstGO = null;
        internal Rigidbody
            srcRB = null,
            dstRB = null;
        internal BoxCollider
            srcBC = null,
            dstBC = null;
        internal Vector3 defaultNodeSize;

        internal ConcurrentDictionary<Entity, ConcurrentDictionary<string, object>> elements = new ConcurrentDictionary<Entity, ConcurrentDictionary<string, object>> { };
        internal ConcurrentStack<float> tensionHistory = new ConcurrentStack<float> { };

        enum Element
        {
            Container,
            Shape,
            GameObject,
            Rigidbody,
            BocCollider
        }

        internal enum Event
        {
            NotSet,
            TooTense,
            Tense
        }

        internal SpringJoint joint { get; private set; } = null;
        internal float 
            spring = 1000,
            damper = 0F,
            tolerance = 0.01F,
            breakForce = Mathf.Infinity,
            breakTorque = Mathf.Infinity;
        internal Type type;
        internal enum Type
        {
            MemberMemberLong,
            MemberMember,
            MemberParent,
            MemberGroot,
            Edge,
            None
        }
        internal Vector3 lastKnownTargetSize = Vector3.zero;
        internal bool
            autoConfigureConnectedAnchor = false,
            enableCollision = false;
        Messenger messenger;
        Layout layout;
        private int tensionHistoryClearedCounter;

        public Joint(Entity src, Entity dst, Container srcC, Type type)
        {
            this.src = src;
            this.dst = dst;
            name = src.name + " => " + dst.name;
            this.srcC = srcC;
            this.layout = srcC.layout;
            this.type = type;
            messenger = src.data.messenger;
            log = new Log(name);
            defaultNodeSize = layout.variables.vectors["nodeSize"];
            elements.TryAdd(src, new ConcurrentDictionary<string, object> { });
            elements.TryAdd(dst, new ConcurrentDictionary<string, object> { });

            if (!(srcC is null))
            {
                elements[src].TryAdd(srcC.GetType().Name, srcC);
            }

            messenger.SubscribeToEvent(ReceiveCreaturesForSRC, Layouts.Layouts.LayoutEvent.GotVisibleShape.ToString(), src.id);
            messenger.SubscribeToEvent(ReceiveCreaturesForSRC, Layouts.Layouts.LayoutEvent.GotContainer.ToString(), src.id);
            messenger.SubscribeToEvent(ReceiveCreaturesForSRC, Layouts.Layouts.LayoutEvent.GotCollider.ToString(), src.id);
            messenger.SubscribeToEvent(ReceiveCreaturesForSRC, Layouts.Layouts.LayoutEvent.GotRigid.ToString(), src.id);

            messenger.SubscribeToEvent(ReceiveCreaturesForDST, Layouts.Layouts.LayoutEvent.GotVisibleShape.ToString(), dst.id);
            messenger.SubscribeToEvent(ReceiveCreaturesForDST, Layouts.Layouts.LayoutEvent.GotContainer.ToString(), dst.id);
            messenger.SubscribeToEvent(ReceiveCreaturesForDST, Layouts.Layouts.LayoutEvent.GotCollider.ToString(), dst.id);
            messenger.SubscribeToEvent(ReceiveCreaturesForDST, Layouts.Layouts.LayoutEvent.GotRigid.ToString(), dst.id);

            src.data.UI.nodeFactory.GetCreaturesOfEntity(src.id, ReceiveCreaturesForSRC);
            src.data.UI.nodeFactory.GetCreaturesOfEntity(dst.id, ReceiveCreaturesForDST);

            CheckIfItsTime();
        }

        internal bool Relaxed(float precision)
        {
            if (joint is null)
            {
                return false;
            }

            float tension = 0;
            try
            {
                tension =
                    Mathf.Abs(joint.currentForce.x) +
                    Mathf.Abs(joint.currentForce.y) +
                    Mathf.Abs(joint.currentForce.z);
            }
            catch (System.Exception)
            {
                // this is here because of a magical leaping device and its unity. otherwise: Null Objecte Reference
                //return true;
                tension = 0.01F;
            }

            if (tension < precision)
            {
                tensionHistory.Clear();
                return true;
            }
            else if (tensionHistory.Count > 20)
            {
                // tension hasnt really changed over the last 20 - edge closer to seek help upstream.
                if (
                    Mathf.RoundToInt(Mathf.Abs(tensionHistory.Average())) < Mathf.RoundToInt(Mathf.Abs(tensionHistory.Take(5).Average())) + precision &&
                    tensionHistoryClearedCounter++ > 4
                    )
                {
                    tensionHistory.Clear();
                    tensionHistoryClearedCounter = 0;
                    throw new Message()
                    {
                        JointEvent = Event.TooTense,
                        from = src,
                        to = dst
                    };
                }
                else
                {
                    tensionHistory.Clear();
                    throw new Message()
                    {
                        obj = new object[] { elements[src]["Container"] as Group.Container },
                        JointEvent = Event.Tense,
                        from = src,
                        to = dst
                    };
                }
            }

            tensionHistory.Push(tension);
            Wiggle();
            return false;
        }

        /// <summary>
        /// TODO: some process should occasionally check the sanity of elements and check if the joint is still rolled with correct objects in the correct layout. etc.
        /// </summary>
        /// <param name="anObject"></param>
        /// <param name="forEntity"></param>
        void ReceiveCreatures(object[] anObject, Entity forEntity)
        {
            forEntity = anObject[0] as Entity;
            if (
                anObject.Length == 3
                && !(anObject[2] is null)
                && !((anObject[2] as List<object>) is null)
                && (anObject[2] as List<object>).Count() > 0
            )
            {
                List<object> creatures = anObject[2] as List<object>;
                foreach (object creature in creatures)
                {
                    if (elements[forEntity].ContainsKey(creature.GetType().Name))
                    {
                        elements[forEntity][creature.GetType().Name] = creature;
                    }
                    else
                    {
                        elements[forEntity].TryAdd(creature.GetType().Name, creature);
                    }
                }

                CheckIfItsTime();
            }
            else if (
                anObject.Length == 3
                && anObject[1].GetType().Name == "Container"
                &&
                (
                    anObject[2].GetType().Name == "Rigidbody"
                    || anObject[2].GetType().Name == "BoxCollider"
                    || anObject[2].GetType().Name == "GameObject"
                    || anObject[2].GetType().Name == "Shape"
                )
            )
            {
                if (!elements[forEntity].ContainsKey("Container") && anObject[1].GetType().Name == "Container")
                {
                    elements[forEntity].TryAdd("Container", anObject[1] as Container);
                    elements[forEntity].TryAdd("ContainerBoxCollider", (anObject[1] as Container).GetComponent<BoxCollider>());
                }

                if (!elements[forEntity].ContainsKey("Rigidbody") && anObject[2].GetType().Name == "Rigidbody")
                {
                    elements[forEntity].TryAdd("Rigidbody", anObject[2] as Rigidbody);
                }
                else if (!elements[forEntity].ContainsKey("BoxCollider") && anObject[2].GetType().Name == "BoxCollider")
                {
                    elements[forEntity].TryAdd("ShapeBoxCollider", anObject[2] as BoxCollider);
                }
                else if (!elements[forEntity].ContainsKey("Shape") && anObject[2].GetType().Name == "Shape")
                {
                    elements[forEntity].TryAdd("Shape", anObject[2] as Shape);
                    elements[forEntity].TryAdd("ShapeBoxCollider", (anObject[2] as Shape).GetComponent<BoxCollider>());
                }
                else if (!elements[forEntity].ContainsKey("GameObject") && anObject[2].GetType().Name == "GameObject")
                {
                    elements[forEntity].TryAdd("GameObject", anObject[2] as GameObject);
                }

                CheckIfItsTime();
            }
            else if (
                anObject.Length == 3
                && anObject[1].GetType().Name == "Shape"
                && anObject[2].GetType().Name == "GameObject"
            )
            {
                if (!elements[forEntity].ContainsKey("Shape"))
                {
                    elements[forEntity].TryAdd("Shape", anObject[1] as Shape);
                    elements[forEntity].TryAdd("ShapeBoxCollider", (anObject[1] as Shape).GetComponent<BoxCollider>());
                }
                if (!elements[forEntity].ContainsKey("GameObject"))
                {
                    elements[forEntity].TryAdd("GameObject", anObject[2] as Shape);
                }
            }
            else {
                log.Entry("unexpected creatures: " + anObject[1].GetType().Name + "; " + anObject[2].GetType().Name);
            }
        }
        internal IEnumerator ReceiveCreaturesForSRC(object[] anObject)
        {
            if (joint is null)
            {
                ReceiveCreatures(anObject, src);
            }
            yield return true;
        }
        internal IEnumerator ReceiveCreaturesForDST(object[] anObject)
        {
            if (joint is null)
            {
                ReceiveCreatures(anObject, dst);
            }
            yield return true;
        }
        internal bool CheckIfReady(Entity entity)
        {
            if (elements[entity].ContainsKey("Container"))
            {
                Container cont = elements[entity]["Container"] as Container;
                if (!elements[entity].ContainsKey("GameObject"))
                {
                    elements[entity].TryAdd("GameObject", cont.gameObject);
                }
                if (!elements[entity].ContainsKey("Rigidbody"))
                {
                    elements[entity].TryAdd("Rigidbody", cont.GetRigid(type));
                }
                if (!elements[entity].ContainsKey("ContainerBoxCollider"))
                {
                    elements[entity].TryAdd("ContainerBoxCollider", cont.GetComponent<BoxCollider>());
                }

                if (!elements[entity].ContainsKey("Shape") && !(cont.shapes is null))
                {
                    Shape shape = cont.shapes.GetShape();
                    if (!(shape is null))
                    {
                        elements[entity].TryAdd("Shape", shape);
                        elements[entity].TryAdd("ShapeBoxCollider", shape.GetComponent<BoxCollider>());
                    }
                }

                // now decide based on the previous findings
                if (
                    elements[entity].ContainsKey("Rigidbody")
                    && elements[entity].ContainsKey("GameObject")
                    && elements[entity].ContainsKey("Shape")
                ) {
                    return true;
                }
                // if some of those are missing, order the factory to report back, once these have been produced.
                else
                {
                    if (entity == src)
                    {
                        src.data.UI.nodeFactory.GetCreaturesOfEntity(entity.id, ReceiveCreaturesForSRC);
                    }
                    else
                    {
                        src.data.UI.nodeFactory.GetCreaturesOfEntity(entity.id, ReceiveCreaturesForDST);
                    }
                }
            }
            else
            {
                if (entity == dst)
                {
                    if (dst.containers.GetCurrentGroup(out Group.Container dstContainer))
                    {
                        elements[entity].TryAdd("Container", dstContainer);
                    }
                }
                else
                {
                    if (src.containers.GetCurrentGroup(out Group.Container srcContainer))
                    {
                        elements[entity].TryAdd("Container", srcContainer);
                    }
                }
            }
            return false;
        }
        internal void CheckIfItsTime()
        {
            if (
                joint is null 
                && CheckIfReady(src) 
                && CheckIfReady(dst)
            ) {
                RollIt();
                messenger.Post(new Message()
                {
                    LayoutEvent = Layouts.Layouts.LayoutEvent.GotJoint,
                    obj = new object[] { srcShape, dstShape },
                    from = src,
                    to = dst,
                });
            }
        }
        void RollIt()
        {
            if (joint is null)
            {
                joint = (elements[src]["GameObject"] as GameObject).AddComponent<SpringJoint>();
                joint.connectedBody = elements[dst]["Rigidbody"] as Rigidbody;
                joint.enableCollision = type != Type.MemberParent;

                SetJointValues();

                SetAnchor((elements[dst]["Shape"] as Shape).transform.localScale);

                (elements[dst]["Container"] as Container).joints.RegisterAsDst(this);
            }
        }

        internal void SetJointValues(float scale = 1.0F)
        {
            joint.breakForce    = layout.variables.joints[type].breakForce;
            joint.breakTorque   = layout.variables.joints[type].breakTorque;
            joint.spring        = layout.variables.joints[type].springStrength;
            joint.damper        = layout.variables.joints[type].damper;
            joint.maxDistance = 0;// layout.variables.joints[type].maxDistance * scale;
            joint.minDistance = 0;// layout.variables.joints[type].minDistance * scale;
            joint.tolerance = 0;// layout.variables.joints[type].tolerance * scale;
            Wiggle();
        }

        internal void ZeroTolerance(float orNot = 0F)
        {
            joint.tolerance = orNot;
            joint.maxDistance = 0;
            Wiggle();
        }

        /// <summary>
        /// this masonry is needed to shake the joints to make them realign to new settings. 
        /// or any setting really. thats unity for you, ad 2021.
        /// </summary>
        internal void Wiggle()
        {
            Vector3 jiff = lastKnownTargetSize;

            if (jiff.x != 0)
            {
                jiff.x += wiggle;
            }
            if (jiff.y != 0)
            {
                jiff.y += wiggle;
            }
            if (jiff.z != 0)
            {
                jiff.z += wiggle;
            }

            if (wiggle > 0)
            {
                wiggle = 0 - wiggle;
            } 
            else
            {
                wiggle = Mathf.Abs(wiggle);
            }

            SetAnchor(jiff);
        }
        internal void SetAnchor(Vector3 targetSize)
        {
            if (!(joint is null))
            {
                try
                {
                    //joint.anchor = defaultNodeSize / 2;
                    lastKnownTargetSize = targetSize;
                    Vector3 padding = (elements[src]["Shape"] as Group.Shape).padding;
                    Container.PositionCorrectionDirection direction = (elements[src]["Container"] as Container).positionCorrectionDirection;

                    // this jolly trickery is necessary to have the last member of a group to roll a correctly sized joint with its partner.
                    if ((elements[src]["Container"] as Container).CheckIfLastInGroup())
                    {
                        direction = (elements[src]["Container"] as Container).positionCorrectionDirection;
                    }
                    if (type == Type.MemberParent)
                    {
                        direction = Container.PositionCorrectionDirection.standYourGround;
                    }

                    switch (direction)
                    {
                        case Container.PositionCorrectionDirection.right:
                            joint.autoConfigureConnectedAnchor = false;
                            joint.anchor = new Vector3((0 - defaultNodeSize.x - padding.x) / 2, 0, 0);
                            joint.connectedAnchor = new Vector3(targetSize.x - joint.minDistance, 0, 0);
                            break;
                        case Container.PositionCorrectionDirection.up:
                            joint.autoConfigureConnectedAnchor = false;
                            joint.anchor = new Vector3(0, (0 - defaultNodeSize.y - padding.y) / 2, 0);
                            joint.connectedAnchor = new Vector3(0, targetSize.y - joint.minDistance, 0);
                            break;
                        case Container.PositionCorrectionDirection.farther:
                            joint.autoConfigureConnectedAnchor = false;
                            joint.anchor = new Vector3(0, 0, (0 - defaultNodeSize.z - padding.z) / 2);
                            joint.connectedAnchor = new Vector3(0, 0, targetSize.z - joint.minDistance);
                            break;

                        case Container.PositionCorrectionDirection.left:
                            joint.autoConfigureConnectedAnchor = false;
                            joint.anchor = new Vector3((0 - defaultNodeSize.x - padding.x) / 2, 0, 0);
                            joint.connectedAnchor = new Vector3(targetSize.x + joint.minDistance, 0, 0);
                            break;
                        case Container.PositionCorrectionDirection.down:
                            joint.autoConfigureConnectedAnchor = false;
                            // this may need to be reversed
                            joint.anchor = new Vector3(0, (0 - defaultNodeSize.y - padding.y) / 2, 0);
                            joint.connectedAnchor = new Vector3(0, targetSize.y + joint.minDistance, 0);
                            break;
                        case Container.PositionCorrectionDirection.closer:
                            joint.autoConfigureConnectedAnchor = false;
                            joint.anchor = new Vector3(0, 0, (0 - defaultNodeSize.z - padding.z) / 2);
                            joint.connectedAnchor = new Vector3(0, 0, targetSize.z + joint.minDistance);
                            break;
                        default:
                            joint.autoConfigureConnectedAnchor = false;
                            joint.anchor = Vector3.zero;
                            joint.connectedAnchor = Vector3.zero;
                            break;
                    }
                }
                catch (System.Exception exe)
                {
                    log.Entry("Burnt joint kicks back.. ? " + exe.StackTrace);
                }
            }
        }

        internal void DestinationShapeChanged(BoxCollider lessie)
        {
            SetAnchor(lessie.size);
        }

        internal void Burn()
        {
            srcC.joints.joints.Remove(this);
            dstC.joints.joints.Remove(this);
            Object.Destroy(joint);
        }
    }
}
