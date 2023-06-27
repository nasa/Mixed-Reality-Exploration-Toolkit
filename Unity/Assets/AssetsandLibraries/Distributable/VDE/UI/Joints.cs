/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
using Assets.VDE.Communication;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.VDE.UI
{
    class Joints
    {
        Log log;
        Entity entity;
        Container owner;
        /// <summary>
        /// layout's joint policy is followed. if desired otherwise, the SetState should be used.
        /// </summary>
        bool mayRoll = true;
        internal bool readyToRoll = false;
        internal IEnumerable<Joint> jointsLeftToRoll;
        internal List<Joint> joints = new List<Joint> { };

        internal Joints(Container owner)
        {
            this.owner = owner;
            entity = owner.entity;
            log = new Log("Joints of " + entity.name);
            mayRoll = owner.layout.variables.flags["useJoints"];
            jointsLeftToRoll = joints.Where(joint => joint.joint is null);
        }
        internal bool IsReady()
        {
            return (joints.Count == 0 || (joints.Count() > 0 && jointsLeftToRoll.Count() == 0));
        }
        /// <summary>
        /// overrides the layout's policy.
        /// </summary>
        /// <param name="setTo">enable or disable</param>
        internal void SetState(bool setTo)
        {
            mayRoll = setTo;
        }
        internal void SetActive(bool setTo = true)
        {
            readyToRoll = setTo;
        }
        internal Joint Add(Entity src, Entity dst, Container dstC, Joint.Type flavor)
        {
            Joint joint = Add(src, dst, flavor);
            if (!(dstC is null) && joint.dstC is null)
            {
                joint.dstC = dstC;
            }
            return joint;
        }
        internal Joint Add(Entity src, Entity dst, Joint.Type flavor)
        {
            Joint joint = joints.FirstOrDefault(jointa => (jointa.src == src && jointa.dst == dst) || (jointa.dst == src && jointa.src == dst));

            if (joint is null)//!joints.Exists(joint => (joint.src == src && joint.dst == dst) || (joint.dst == src && joint.src == dst)))
            {
                joint = new Joint(src, dst, owner, flavor);
                joints.Add(joint);
                entity.data.messenger.Post(new Message()
                {
                    LayoutEvent = Layouts.Layouts.LayoutEvent.GotJoint,
                    EventOrigin = Layouts.Layouts.EventOrigin.Joint,
                    from = src,
                    to = dst
                });
            }
            return joint;
        }
        internal bool Relaxed(float precision = 1F)
        {
            try
            {
                if (!joints.Where(joint => !joint.Relaxed(precision)).Any())
                {
                    return true;
                }
            }
            catch (Message mess)
            {
                switch (mess.JointEvent)
                {
                    case Joint.Event.TooTense:
                        {
                            if (mess.to.parentID == mess.from.parentID)
                            {
                                mess.to.parent.containers.GetCurrentGroup().PositionGroupMembersAhead();
                            }
                            else
                            {
                                mess.from.parent.containers.GetCurrentGroup().PositionGroupMembersAhead();
                                mess.to.parent.containers.GetCurrentGroup().PositionGroupMembersAhead();
                            }
                        }
                        break;
                    case Joint.Event.Tense:
                        (mess.obj[0] as Group.Container).PositionSelfAmongstSiblings();
                        break;
                    default:
                        break;
                }
            }
            return false;
        }

        internal void RegisterAsDst(Joint incoming)
        {
            if (!joints.Exists(joint => (joint.src == incoming.src && joint.dst == incoming.dst) || (joint.dst == incoming.src && joint.src == incoming.dst)))
            {
                joints.Add(incoming);
            }
        }

        internal void SetDamp(int damp)
        {
            foreach (Joint joint in joints.Where(joint => joint.src == entity))
            {
                joint.damper = damp;
            }
        }

        internal void SetConnectedAnchors(Vector3 vector)
        {
            foreach (Joint joint in joints)
            {
                joint.SetAnchor(vector);
            }
        }

        internal IEnumerator NotifyOfShapeChange(BoxCollider lessie)
        {
            foreach (Joint joint in joints.Where(joint => joint.dst == entity))
            {
                joint.DestinationShapeChanged(lessie);
            }
            yield return true;
        }
        internal void NotifyOfShapeChangeNE(BoxCollider lessie)
        {
            foreach (Joint joint in joints.Where(joint => joint.dst == entity))
            {
                joint.DestinationShapeChanged(lessie);
            }
        }
    }
}
