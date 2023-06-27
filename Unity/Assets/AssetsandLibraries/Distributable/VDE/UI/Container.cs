/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
using Assets.VDE.Communication;
using Assets.VDE.Layouts;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.VDE.UI
{
#if MRET_2021_OR_LATER && NOTMRET
    internal class Container : GOV.NASA.GSFC.XR.MRET.MRETBehaviour
#else
    internal class Container : MonoBehaviour
#endif
    {
        public int posse;

        internal Log log;
        internal Data data;
        internal Entity entity;
        internal Messenger messenger;
        internal Rigidbody rigidBody;
        internal BoxCollider containerCollider;
        internal Joints joints;
        internal Layout layout;
        internal Shapes shapes;
        internal float magPositionInPreviousFrame;
        internal bool driver, hasLinksToUpdate, ready, doNotFreeze;
        internal IEnumerable<Link> linksFromThisContainer;
        internal IEnumerable<Link> linksToThisContainer;
        /// <summary>
        /// Label to be created (on demand), that shall keep itself on a fixed position, by default below the visible shape.
        /// </summary>
        internal GameObject label;
        internal Entity.Type type;
        internal State state;
        internal IOrderedEnumerable<Entity> immediateSiblings;
        internal IOrderedEnumerable<Container> siblingsWithContainers;
        internal PositionCorrectionDirection positionCorrectionDirection = PositionCorrectionDirection.standYourGround;
        internal List<Entity> collidingSiblings = new List<Entity>();
        internal Vector3 direction;
        /// <summary>
        /// if set to other than zero, will be used by PositionAhead() in Group instead of recalculating.
        /// </summary>
        internal Vector3 resetToPosition = Vector3.zero;
        internal Transform parentTrafo;
        internal bool isGrabbed;
        internal Vector3 localPositionPriorToGrab, worldPositionPriorToGrab, defaultPosition;
        internal Quaternion localRotationPriorToGrab, worldRotationPriorToGrab, defaultRotation;
        private bool labelsLastKnownState;

        internal enum State
        {
            @fresh = 0,
            ready,
            triggering,
            beingAdopded,
            adopted
        }
        /// <summary>
        /// sequential number values are used in joint. dont change the enum's positions here!
        /// </summary>
        internal enum PositionCorrectionDirection
        {
            @default = 0,
            standYourGround,
            keepTheCentre,
            freeze,
            free,
            up,
            down,
            closer,
            farther,
            right,
            left
        }
        internal void Init(Entity entity, Layout layout)
        {
#if MRET_2021_OR_LATER && NOTMRET
            grabBehavior = GrabBehavior.Custom;
            touchBehavior = TouchBehavior.Custom;
            grabbable = false;
            useable = false;
#endif
            this.entity = entity;
            this.layout = layout;
            data = entity.data;
            type = entity.type;
            messenger = data.messenger;
            shapes = new Shapes(this);
            joints = new Joints(this);
            log = new Log(entity.name + " container", messenger);
            posse = entity.pos;
            if (
                !(label is null) &&
                entity.distanceFromGroot <= layout.GetValueFromConf("showLabelsMaxDepth") &&
                entity.distanceFromGroot >= layout.GetValueFromConf("showLabelsMinDepth") &&
                layout.labelsVisible &&
                // this may cause problems, if shapes have 0 children in the beginning but get populated later
                entity.relations.Where(rel => rel.Value == Entity.Relation.Child).Count() > 0
                )
            {
                label.SetActive(true);
            }

            ReSeat();
            SetScale(Vector3.one);

            parentTrafo = gameObject.transform.parent;
        }

        internal void ReSeat()
        {
            // may not be needed anymore
            if (layout.state == Layout.State.ready)
            {
                StartCoroutine(layout.Reinitialize());
            }
            GetSiblingsAndLinks();

            SetPosition(layout.PositionEntity(entity, this));
        }

        internal void GetSiblingsAndLinks()
        {
            immediateSiblings =
                Enumerable.Union(
                    (
                        from
                            siblings
                        in
                            entity.siblings
                        where
                            siblings.pos < entity.pos
                        select
                            siblings
                    ).OrderByDescending(sib => sib.pos).Take(1),
                    (
                        from
                            siblings
                        in
                            entity.siblings
                        where
                            siblings.pos > entity.pos
                        orderby
                            siblings.pos
                        select
                            siblings
                    ).Take(1)
                ).OrderBy(ent => ent.pos);

            siblingsWithContainers = entity.siblings.Where(sibi => sibi.containers.IsReady()).Select(sibi => sibi.containers.GetContainer()).OrderBy(sibi => sibi.entity.pos);

            linksToThisContainer = data.links.GetLinksTo(this);
            linksFromThisContainer = data.links.GetLinksFrom(this);
        }

        internal void SetMaterialsTo(Material setMaterialTo)
        {
            shapes.SetMaterialsTo(setMaterialTo);
        }
        internal void SetLabelState(bool setTo)
        {
            if (!(label is null))
            {
                // 20220111
                //label.SetActive(setTo);
                //return;
                if (label.activeSelf && !setTo)
                {
                    labelsLastKnownState = label.activeSelf;
                    label.SetActive(setTo);
                }
                else if (!label.activeSelf && layout.labelsVisible && setTo)
                {
                    label.SetActive(setTo);
                }
            }
        }
        internal Joint JoinWith(Entity target, Container destinationContainer, Joint.Type flavor)
        {
            return joints.Add(entity, target, destinationContainer, flavor);
        }
        /// <summary>
        /// this sets the position of this CONTAINER in relations to its PARENT container.
        /// </summary>
        /// <param name="position"></param>
        internal void SetPosition(Vector3 position)
        {
            transform.localPosition = position;
            defaultPosition = position;
            defaultRotation = transform.localRotation;
            UpdateLinks();
        }
        internal System.Collections.IEnumerator UpdateLinksAsync()
        {
            foreach (Link link in linksToThisContainer.Where(link => link.ready))
            {
                if (Time.deltaTime > data.UI.maxTimeForUpdatePerFrame)
                {
                    yield return new WaitForSeconds(data.UI.timeToWaitInUpdatePerFrame / 100);
                    //yield return data.UI.Sleep(data.UI.timeToWaitInUpdatePerFrame);
                }
                link.UpdatePosition(this);
            }
            foreach (Link link in linksFromThisContainer.Where(link => link.ready))
            {
                if (Time.deltaTime > data.UI.maxTimeForUpdatePerFrame)
                {
                    yield return new WaitForSeconds(data.UI.timeToWaitInUpdatePerFrame / 100);
                    //yield return data.UI.Sleep(data.UI.timeToWaitInUpdatePerFrame);
                }
                link.UpdatePosition(this);
            }
            yield return true;
        }
        internal void UpdateLinks()
        {
            foreach (Link link in linksToThisContainer.Where(link => link.ready))
            {
                link.UpdatePosition(this);
            }
            foreach (Link link in linksFromThisContainer.Where(link => link.ready))
            {
                link.UpdatePosition(this);
            }
        }      

        /// <summary>
        /// set the scale of this CONTAINER.
        /// should really be 1. most of the times.
        /// </summary>
        /// <param name="scale"></param>
        internal void SetScale(Vector3 scale)
        {
            transform.localScale = scale;
        }
        internal bool CheckIfFirstInGroup()
        {
            if (entity.siblings.Where(ent => ent.pos < entity.pos).Any())
            {
                return false;
            }
            return true;
        }
        internal bool CheckIfLastInGroup()
        {
            if (entity.siblings.Where(ent => ent.pos > entity.pos).Any())
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// for example if layout is switched, sets its state accordingly
        /// </summary>
        /// <param name="setTo"></param>
        internal void SetState(bool setTo)
        {
            gameObject.SetActive(setTo);
        }
        internal void GrabbedBy(GameObject grabber)
        {
            isGrabbed = true;
            if (TryGetComponent(out Transporter vw))
            {
                Destroy(vw);
            }
            worldPositionPriorToGrab = transform.position;
            worldRotationPriorToGrab = transform.rotation;
            localPositionPriorToGrab = transform.localPosition;
            localRotationPriorToGrab = transform.localRotation;
            transform.SetParent(grabber.transform);
            transform.localRotation = Quaternion.Euler(0, 0, 0);
            transform.localPosition = layout.variables.vectors["grabOffsetFromHand"];
#if MRET_2021_OR_LATER
            gameObject.AddComponent<GOV.NASA.GSFC.XR.MRET.SceneObjects.InteractableSceneObject>();
#endif
        }
        internal virtual void Released(Hands.Hand hand)
        {
            isGrabbed = false;
            transform.SetParent(parentTrafo);
            transform.localPosition = localPositionPriorToGrab;
            transform.localRotation = localRotationPriorToGrab;
            UpdateLinks();
        }
        /// <summary>
        /// shall check if this container is the first amongst its siblings and if so, roll a short joint with its parent.
        /// </summary>
        /// <param name="sibling">Parent</param>
        /// <param name="container">And it's container</param>
        internal void AdoptParent(Entity parent, Group.Container container, Joint.Type jointType = Joint.Type.MemberParent)
        {
            if (entity.doJoints && (CheckIfFirstInGroup() || jointType == Joint.Type.MemberGroot))
            {
                JoinWith(parent, container, jointType);
            }
        }

        /// <summary>
        /// Acknowledge the addition of a sibling and adjust self's position AND joints, if the new member is in self's immediate vicinity.
        /// </summary>
        /// <param name="incomingSibling">The Other</param>
        /// <param name="incomingSiblingContainer">And other's container</param>
        /// <param name="jointType">flavor of this joint</param>
        //internal void AdoptSibling(Entity incomingSibling, Container incomingSiblingContainer, Joint.Type jointType = Joint.Type.MemberMember) { }
        
        
        internal Rigidbody GetRigid(Joint.Type type)
        {
            if (rigidBody is null)
            {
                GetRigid(gameObject, type);
            }
            return rigidBody;
        }
        internal Rigidbody GetRigid(GameObject gameObject, Joint.Type type)
        {
            if(rigidBody is null)
            {
                if (gameObject.GetComponent<Rigidbody>() != null)
                {
                    rigidBody = gameObject.GetComponent<Rigidbody>();
                }
                else
                {
                    rigidBody = gameObject.AddComponent<Rigidbody>();
                }

                rigidBody.mass = layout.GetRigidJointValueFromConf(type, "RigidBodyMass");
                rigidBody.drag = layout.GetRigidJointValueFromConf(type, "RigidBodyDrag");
                rigidBody.isKinematic = false;
                rigidBody.useGravity = false;

                SetPositionCorrection();
                rigidBody.angularDrag = 1;

                messenger.Post(new Message()
                {
                    LayoutEvent = Layouts.Layouts.LayoutEvent.GotRigid,
                    obj = new object[] { entity, this, rigidBody },
                    layout = layout,
                    from = entity
                });
            }
            return rigidBody;
        }
        /// <summary>
        /// if called without a parameter, sets the positionCorrectionDirection of this container to its default.
        /// if calles WITH a parameter, sets the rigidbody accordingly, but doesnt change the positionCorrectionDirection.
        /// </summary>
        /// <param name="setTo"></param>
        internal void SetPositionCorrection(PositionCorrectionDirection setTo = PositionCorrectionDirection.@default, bool temporary = false)
        {
            if (setTo == PositionCorrectionDirection.@default)
            {
                setTo = positionCorrectionDirection;
            }
            else if(!temporary && setTo != PositionCorrectionDirection.@default)
            {
                positionCorrectionDirection = setTo;
            }
            if (!(rigidBody is null))
                rigidBody.isKinematic = false;

            switch (setTo)
            {
                case PositionCorrectionDirection.standYourGround:
                    if (!(rigidBody is null))
                    {
                        rigidBody.constraints = RigidbodyConstraints.FreezeAll;
                        rigidBody.isKinematic = true;
                    }
                    break;
                case PositionCorrectionDirection.free:
                    if (!(rigidBody is null))
                        rigidBody.constraints = RigidbodyConstraints.None;
                    break;
                case PositionCorrectionDirection.freeze:
                    if (!(rigidBody is null))
                        rigidBody.constraints = RigidbodyConstraints.FreezeAll;
                    break;
                case PositionCorrectionDirection.up:
                    if (!(rigidBody is null))
                        rigidBody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
                    direction = new Vector3(0, 1, 0);
                    break;
                case PositionCorrectionDirection.down:
                    if (!(rigidBody is null))
                        rigidBody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
                    direction = new Vector3(0, -1, 0);
                    break;
                case PositionCorrectionDirection.closer:
                    if (!(rigidBody is null))
                        rigidBody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
                    direction = new Vector3(0, 0, -1);
                    break;
                case PositionCorrectionDirection.farther:
                    if (!(rigidBody is null))
                        rigidBody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
                    direction = new Vector3(0, 0, 1);
                    break;
                case PositionCorrectionDirection.right:
                    if (!(rigidBody is null))
                        rigidBody.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
                    direction = new Vector3(1, 0, 0);
                    break;
                case PositionCorrectionDirection.left:
                    if (!(rigidBody is null))
                        rigidBody.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
                    direction = new Vector3(-1, 0, 0);
                    break;
                case PositionCorrectionDirection.keepTheCentre:
                    if (!(rigidBody is null))
                        rigidBody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;// Z | RigidbodyConstraints.FreezeRotationX;
                    break;
                default:
                    if (!(rigidBody is null))
                        rigidBody.constraints = RigidbodyConstraints.FreezeAll;
                    break;
            }
        }
        /// <summary>
        /// called once the metashape has finished positioning and its time to contract the joints to compact the shapes.
        /// </summary>
        virtual internal void SchedulePostprocess()
        {
        }
        internal void GetCollider()
        {
            if (gameObject.TryGetComponent(out BoxCollider lessie))
            {
                containerCollider = lessie;
            }
        }
        void RegisterCollision(GameObject game)
        {
            if (game.TryGetComponent(out Container collidingContainer) && !(collidingContainer.entity is null) && !(collidingContainer.entity.id > -2))
            {
                if (!collidingSiblings.Contains(collidingContainer.entity)
                    && immediateSiblings.Contains(collidingContainer.entity))
                {
                    collidingSiblings.Add(collidingContainer.entity);
                }
            }
        }
        void DeRegisterCollision(GameObject game)
        {
            if (game.TryGetComponent(out Container collidingContainer) && !(collidingContainer.entity is null) && !(collidingContainer.entity.id > -2))
            {
                if (collidingSiblings.Contains(collidingContainer.entity))
                {
                    collidingSiblings.Remove(collidingContainer.entity);
                }
            }
        }
        private void OnTriggerEnter(Collider other)
        {
            RegisterCollision(other.gameObject);
        }
        private void OnTriggerExit(Collider other)
        {
            DeRegisterCollision(other.gameObject);
        }
        private void OnCollisionEnter(Collision collision)
        {
            RegisterCollision(collision.gameObject);
        }
        private void OnCollisionExit(Collision collision)
        {
            DeRegisterCollision(collision.gameObject);
        }
    }
}
