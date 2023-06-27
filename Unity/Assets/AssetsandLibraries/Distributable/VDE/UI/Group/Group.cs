/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
using Assets.VDE.Layouts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Message = Assets.VDE.Communication.Message;

namespace Assets.VDE.UI.Group
{
    internal class Container : Assets.VDE.UI.Container
    {
        internal bool groupHasSettled;
        /*
        IEnumerable<Container> membersReady, siblingsReady;
        IEnumerable<Assets.VDE.UI.Container> membersWithShapes;
        IOrderedEnumerable<Container> membersAsGroupsWithShapes;
        */
        Action triggerTask;

        internal new void Init(Entity entity, Layout layout)
        {
            base.Init(entity, layout);

            FindSiblingsAndMembers(entity);

            messenger.Post(new Communication.Message()
            {
                LayoutEvent = Layouts.Layouts.LayoutEvent.GotContainer,
                EventOrigin = Layouts.Layouts.EventOrigin.Group,
                obj = new object[] { entity, this, gameObject },
                to = entity.parent,
                from = entity,
                message = "from Group"
            });
        }
        
        private void FindSiblingsAndMembers(Entity entity)
        {
            /*
            membersReady = entity.members.Where(member => !(member.containers is null) && member.containers.IsReady()).Select(member => member.containers.GetCurrentShape().container);
            siblingsReady = entity.siblings.Where(sibling => !(sibling.containers is null) && sibling.containers.IsReady()).Select(sibling => sibling.containers.GetCurrentShape().container);
            membersWithShapes = entity.members.Where(member => !(member.containers is null)).Select(member => member.containers.GetContainer());
            membersAsGroupsWithShapes = membersWithShapes.Where(member =>
                !(member is null) &&
                !(member.entity is null) &&
                member.type == Entity.Type.Group).Select(member =>
                    member as Container).OrderBy(member =>
                        member.entity.pos);
            */
        }
        
        #region methods
        private IEnumerable<Container> MembersReady()
        {
            return entity.members.Where(member => !(member.containers is null) && member.containers.IsReady()).Select(member => member.containers.GetCurrentShape().container);
        }
        private IEnumerable<Container> SiblingsReady()
        {
            return entity.siblings.Where(sibling => !(sibling.containers is null) && sibling.containers.IsReady()).Select(sibling => sibling.containers.GetCurrentShape().container);
        }
        private IEnumerable<Assets.VDE.UI.Container> MembersWithShapes()
        {
            return entity.members.Where(member => !(member.containers is null)).Select(member => member.containers.GetContainer());
        }
        private IOrderedEnumerable<Container> MembersAsGroupsWithShapes()
        {
            return MembersWithShapes().Where(member =>
                !(member is null) &&
                !(member.entity is null) &&
                member.type == Entity.Type.Group).Select(member =>
                    member as Container).OrderBy(member =>
                        member.entity.pos);
        }
        #endregion
        private void CheckIfToRelaxLastMember()
        {
            if (CheckIfMemberShapesAreReady())
            {
                if (type == Entity.Type.Group)
                {
                    // this exception is necessary for shallow groups that would not benefit from a trigger, as these dont have child groups.
                    if (entity.parent.IamGROOT && !entity.members.Where(member => member.type == Entity.Type.Group).Any())
                    {
                        ready = true;
                        GroupHasSettled();
                    }
                    else
                    {
                        entity.ScheduleForTrigger();
                    }
                }
            }
        }
        /// <summary>
        /// called from the main thread via trunk triggerers.
        /// </summary>
        /// <returns></returns>
        internal IEnumerator Trigger()
        {
            state = State.triggering;
            int groupSettlmentIteration = 0;
            while (!CheckIfAllMembersHaveShapes())
            {
                yield return new WaitForSeconds(data.random.Next(8, 16) / 100);
                //yield return data.UI.Sleep(2345);
            }
            while (!TryToRelaxGroupMembers())
            {
                yield return new WaitForSeconds(data.random.Next(8, 16) / 100);
                //yield return data.UI.Sleep(2345);
            }
            while (groupSettlmentIteration++ < 10 && !CheckIfGroupHasSettled(30F, 0.001F))
            {
                yield return new WaitForSeconds(data.random.Next(8, 16) / 100);
                //yield return data.UI.Sleep(2345,1234);
            }
            yield return new WaitForSeconds(data.random.Next(12, 23) / 100);
            //yield return data.UI.Sleep(2345,1234);
            GroupHasSettled();
            entity.YieldTrigger();
            state = State.ready;
            yield return true;
        }

        /// <summary>
        /// called from the main thread via trunk triggerers AFTER the metashape has been rendered, to trim the excess spacing between groups.
        /// has issues. as of 20220216
        /// </summary>
        internal IEnumerator Postprocess()
        {
            if (MembersAsGroupsWithShapes().Any())
            {
                // set the collider of the group to NOT collide but trigger (so that members wouldnt collide with it)
                SetColliderToTrigger(true);
                if (!(entity.parent is null) && entity.parent.containers.GetCurrentGroup(out Container parentsContainer))
                {
                    parentsContainer.SetColliderToTrigger(true);
                }
                // set members' colliders to COLLIDE with eachother
                SetGroupMembersJointAnchors(Vector3.zero); 
                //Container lastMember = membersAsGroupsWithShapes.Last();
                //lastMember.SetPositionCorrection(layout.GetPositionCorrectionDirection(lastMember.entity.depthOfRelations), true);
                UnFreezeGroupsMembers();
                bool triggersTo = false;
                SetGroupMembersCollidersTriggers(triggersTo);
                // need to give the members time to start moving before we check their readiness
                yield return new WaitForSeconds(data.random.Next(123, 234) / 100);
                //yield return data.UI.Sleep(3456, 2345);
                while (!CheckIfGroupHasSettled(1F, 0.01F, false))
                {
                    yield return new WaitForSeconds(data.random.Next(123, 234) / 100);
                    //yield return data.UI.Sleep(2345, 1234);
                    triggersTo = !triggersTo;
                    SetGroupMembersCollidersTriggers(triggersTo);
                }
                GroupHasSettled();
            }
            entity.YieldTrigger();
            yield return true;
        }

        internal void SetUpdaters(bool setu, bool siblingsToo)
        {
            foreach (var member in MembersReady())
            {
                SetUpdater(member, setu);
            }
            if (siblingsToo)
            {
                foreach (var member in SiblingsReady())
                {
                    SetUpdater(member, setu);
                }
            }
        }

        private void SetUpdater(Container member, bool setu)
        {
            if (member.gameObject.TryGetComponent(out Updater nupdater))
            {
                nupdater.enabled = setu;
            }
            else
            {
                Updater updater = member.gameObject.AddComponent<Updater>();
                updater.owner = this;
                updater.enabled = setu;
            }
        }
        /// <summary>
        /// to enable the shapes to find their position in life, they need some.. boundaries. 
        /// hence the containers shall have slightly larger colliders around them.
        /// colliders shall be turned on (isTrigger=false) ONLY during the positioning of the group, that this container's owner entity belongs to.
        /// otherwise it'll be a mess.
        /// triggers are handled globally by Messenger's queue: Messenger.TriggerHappy.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="scale"></param>
        internal void SetColliderPositionAndScale(Vector3 position, Vector3 scale)
        {
            if (containerCollider is null)
            {
                GetCollider();
            }
            // not else, as we're trying to set the value in previous segment.
            if (!(containerCollider is null))
            {
                containerCollider.center = position;
                containerCollider.size = scale;
                containerCollider.enabled = true;
                NotifyJointsOfShapeChange();
            }
        }

        private void NotifyJointsOfShapeChange()
        {
            SetPositionCorrection();
            //StartCoroutine(joints.NotifyOfShapeChange(containerCollider));
            joints.NotifyOfShapeChangeNE(containerCollider);
        }

        /// <summary>
        /// siblings may get misplaced in the group (joints will then restrict their movment back to correct sequence).
        /// PositionSelfAmongstSiblings will reset SIBLINGS' positions according to entity.siblings.*.pos and margins+padding set in layout conf.
        /// - this will NOT alter the members of this group.
        /// - should not be called directly, but via PositionGroupMembersAhead()
        /// </summary>
        internal void PositionSelfAmongstSiblings(bool freeze = true, bool comment = false)
        {
            Vector3 targetPosition = resetToPosition;

            _ = entity.GetMembersAndSiblings();
            foreach (Shape sibling in entity.siblings.
                Where(sibling => !(sibling.containers.GetCurrentShape() is null) && sibling.pos < entity.pos).
                Select(sibling => sibling.containers.GetCurrentShape()))
            {
                if (sibling.TryGetComponent(out Renderer siblingsRenderer))
                {
                    Vector3 target = siblingsRenderer.bounds.size;
                    if (
                        !(sibling.container.containerCollider is null) && 
                        sibling.container.containerCollider.bounds.size.magnitude > 0 &&
                        Vector3.Scale(sibling.container.containerCollider.bounds.size, direction).magnitude > Vector3.Scale(siblingsRenderer.bounds.size, direction).magnitude
                        )
                    {
                        target = sibling.container.containerCollider.bounds.size;
                    }
                    targetPosition += Vector3.Scale((
                        target
                        + sibling.padding 
                        + shapes.GetShape().padding
                    ), direction);
                }
            }
            SetPosition(targetPosition);

            // if this is the first or last member of the group, it shall hold its ground.
            if(freeze && !doNotFreeze && (CheckIfFirstInGroup() || CheckIfLastInGroup()))
            {
                SetPositionCorrection(PositionCorrectionDirection.freeze, true);
            }
        }
        /// <summary>
        /// check, if members of this group have all been:
        /// 1. created
        /// 2. managed to form shapes
        /// 3. rolled joints (if necessary)
        /// </summary>
        /// <returns></returns>
        internal bool CheckIfMemberShapesAreReady()
        {
            bool checkIfAllMembersHaveBeenCreated = CheckIfAllMembersHaveBeenCreated();
            bool checkIfAllMembersHaveShapes = CheckIfAllMembersHaveShapes();
            bool checkIfMemberShapesAreReady = !entity.members.Where(member => !member.containers.IsReady()).Any();

            if (
                   checkIfAllMembersHaveBeenCreated 
                && checkIfAllMembersHaveShapes 
                && checkIfMemberShapesAreReady
            )
            {
                return true;
            }
            return false;
        }
        private bool CheckIfAllMembersHaveBeenCreated()
        {
            return entity.members.Count() == entity.relations.Values.Where(value => value == Entity.Relation.Child).Count();
        }

        private bool CheckIfAllMembersHaveShapes()
        {
            return MembersWithShapes().Count() == entity.members.Count();
        }

        private bool CheckIfGroupHasSettled(float relaxationPrecision = 1F, float maxSpeed = 0.01F, bool positionAhead = true)
        {
            if (MembersAsGroupsWithShapes().Any() && !layout.frozen)
            {
                int groupsToCheck = MembersAsGroupsWithShapes().Count() - 1;
                Container lastMember = MembersAsGroupsWithShapes().Last();
                Rigidbody rigidBodyOfLastMember = lastMember.gameObject.GetComponent<Rigidbody>();
                float speed = rigidBodyOfLastMember.velocity.magnitude;

                foreach (Container member in MembersAsGroupsWithShapes().Where(member => !member.CheckIfFirstInGroup()))
                {
                    bool colliding = member.collidingSiblings.Any();
                    bool relaxed = member.joints.Relaxed(relaxationPrecision);
                    bool outOfLine = member.CheckIfOutOfLine();

                    if (relaxed && speed < maxSpeed && !colliding && !outOfLine)
                    {
                        groupsToCheck--;
                    }
                    else 
                    {
                        if (positionAhead && (colliding || outOfLine) )
                        { 
                            PositionGroupMembersAhead();
                        }
                        return false;
                    }
                }
                if (groupsToCheck == 0)
                {
                    shapes.GetGroupShape().ScheduleUpdate();
                    return true;
                }
            }
            else
            {
                return true;
            }

            return false;
        }

        internal bool CheckIfOutOfLine()
        {
            if (immediateSiblings.Any())
            {
                Entity sibling = immediateSiblings.First();
                if (!(sibling is null))
                {
                    Assets.VDE.UI.Container siblingContainer = sibling.containers.GetContainer();
                    if (!(siblingContainer is null))
                    {
                        return positionCorrectionDirection switch
                        {
                            PositionCorrectionDirection.up => immediateSiblings.First().containers.GetContainer().transform.localPosition.y >= transform.localPosition.y || transform.localPosition.y < 0,
                            PositionCorrectionDirection.down => immediateSiblings.First().containers.GetContainer().transform.localPosition.y <= transform.localPosition.y || transform.localPosition.y > 0,
                            PositionCorrectionDirection.closer => immediateSiblings.First().containers.GetContainer().transform.localPosition.z <= transform.localPosition.z || transform.localPosition.z > 0,
                            PositionCorrectionDirection.farther => immediateSiblings.First().containers.GetContainer().transform.localPosition.z >= transform.localPosition.z || transform.localPosition.z < 0,
                            PositionCorrectionDirection.right => immediateSiblings.First().containers.GetContainer().transform.localPosition.x >= transform.localPosition.x || transform.localPosition.x < 0,
                            PositionCorrectionDirection.left => immediateSiblings.First().containers.GetContainer().transform.localPosition.x <= transform.localPosition.x || transform.localPosition.x > 0,
                            _ => false,
                        };
                    }
                }
            }
            return false;
        }

        internal void PositionGroupMembersAhead()
        {
            SetGroupMembersCollidersTriggers(true);
            foreach (var item in MembersAsGroupsWithShapes().Where(mem => !mem.CheckIfFirstInGroup()).OrderBy(mem => mem.entity.pos))
            {
                item.PositionSelfAmongstSiblings();
            }
            TryToRelaxGroupMembers();
        }

        /// <summary>
        /// restricts the movement directions of all members of this group
        /// </summary>
        internal void FreezeGroupsMembers()
        {
            foreach (var member in MembersAsGroupsWithShapes())
            {
                member.SetPositionCorrection(PositionCorrectionDirection.freeze, true);
                member.SetUpdaters(false, false);
            }
        }
        /// <summary>
        /// sets loose all but the first member of this group
        /// </summary>
        private void UnFreezeGroupsMembers()
        {
            foreach (var member in MembersAsGroupsWithShapes().Where(member => !member.CheckIfFirstInGroup()))
            {
                member.SetPositionCorrection();
                member.SetUpdaters(true, false);
            }
        }
        /// <summary>
        /// called by a member during it's init. will check, if (shapes of the) members of this group are ready. if so, shall recalculate own shape.
        /// </summary>
        /// <param name="message"></param>
        internal void AdoptMember(Assets.VDE.UI.Container newMemberContainer)
        {
            if (MembersReady().Contains(newMemberContainer) || newMemberContainer.state == State.beingAdopded || newMemberContainer.state == State.adopted)
            {
                return;
            }
            newMemberContainer.state = State.beingAdopded; 
            FindSiblingsAndMembers(entity);

            // nodes doesnt need to know of their incoming siblings. do they.
            if (newMemberContainer.entity.type == Entity.Type.Group)
            {
                NotifyMembersOfTheirFreshSibling(newMemberContainer as Group.Container);
            }
            shapes.GetGroupShape().AdoptMember(newMemberContainer);
            PositionSelfAmongstSiblings();

            if (CheckIfAllMembersHaveShapes())
            {
                if (state == State.ready)
                {
                    if (!(entity.parent is null) && entity.parent.containers.GetCurrentGroup(out Container parentsContainer))
                    {
                        parentsContainer.SetGroupMembersCollidersTriggers(true);
                        if (!(parentsContainer.entity.parent is null) && parentsContainer.entity.parent.containers.GetCurrentGroup(out Container grandParentsContainer))
                        {
                            grandParentsContainer.SetGroupMembersCollidersTriggers(true);

                            if (!(grandParentsContainer.entity.parent is null) && grandParentsContainer.entity.parent.containers.GetCurrentGroup(out Container greatGrandParentsContainer))
                            {
                                greatGrandParentsContainer.SetGroupMembersCollidersTriggers(true);
                            }
                        }
                    }
                    PositionGroupMembersAhead();
                }
                CheckIfToRelaxLastMember();
            }
            if (newMemberContainer.state == State.beingAdopded)
            {
                newMemberContainer.state = State.adopted;
            }
        }

        private void NotifyMembersOfTheirFreshSibling(Assets.VDE.UI.Group.Container newMemberContainer)
        {
            //foreach (Entity member in entity.members.Where(memm => memm.type == Entity.Type.Group))
            foreach (Entity member in newMemberContainer.entity.siblings)
            {
                if (member.siblings.Contains(newMemberContainer.entity) && member.containers.GetCurrentGroup(out Group.Container memberGroupContainer))
                {
                    memberGroupContainer.AdoptSibling(newMemberContainer.entity, newMemberContainer);
                }
                else
                {
                    StartCoroutine(member.ReceiveEntity(new object[] { newMemberContainer.entity, newMemberContainer, newMemberContainer.gameObject }));
                }
            }
        }

        internal void AdoptSibling(Entity incomingSibling, Container incomingSiblingContainer, Joint.Type jointType = Joint.Type.MemberMember)
        {
            List<Joint> deplorables = new List<Joint> { };

            GetSiblingsAndLinks();

            // if this entity is no longer FirstInGroup but still retains joint with the parent, burn that
            // and reset positionCorrectionDirection according to layout's.
            if (
                (
                    !CheckIfFirstInGroup() ||
                    incomingSibling.pos < entity.pos
                ) &&
                joints.joints.Where(joint =>
                    joint.dst == entity.parent &&
                    joint.type == Joint.Type.MemberParent).Any()
                )
            {
                foreach (Joint deplorable in joints.joints.Where(joint =>
                    joint.dst == entity.parent &&
                    joint.type == Joint.Type.MemberParent)
                )
                {
                    deplorables.Add(deplorable);
                }

                deplorables.ForEach(deplorable => deplorable.Burn());
                deplorables.Clear();
                SetPositionCorrection(layout.GetPositionCorrectionDirection(entity.distanceFromGroot));
            }

            if (
                // trunks etc
                jointType == Joint.Type.MemberMemberLong ||
                (
                    // joints between members, that's
                    jointType == Joint.Type.MemberMember &&
                    // pos is jointable and
                    incomingSibling.pos < entity.pos &&
                    (
                        (
                            // is either THIS entity's sibling or
                            !(immediateSiblings is null) &&
                            immediateSiblings.Count() > 0 &&
                            immediateSiblings.First() == incomingSibling
                        ) || (
                            // is THIS entity's sibling, that THIS entity doesnt YET know about (refreshed entity.r has not yet been processed to relations)
                            !(incomingSiblingContainer is null) &&
                            !(incomingSiblingContainer.immediateSiblings is null) &&
                            incomingSiblingContainer.immediateSiblings.Count() > 0 &&
                            incomingSiblingContainer.immediateSiblings.Last() == entity
                        )
                    )
                )
            )
            {
                // burn existing joints with other siblings before joining the new one
                foreach (Joint deplorable in joints.joints.Where(joint =>
                    !(joint.dst is null) &&
                    !(joint.dstC is null) &&
                    joint.dst != incomingSibling &&
                    joint.dst != entity &&
                    joint.type == Joint.Type.MemberMember)
                )
                {
                    deplorables.Add(deplorable);
                }

                deplorables.ForEach(deplorable => deplorable.Burn());
                deplorables.Clear();

                if (entity.doJoints)
                {
                    JoinWith(incomingSibling, incomingSiblingContainer, jointType);
                }
            }
        }

        override internal void SchedulePostprocess()
        {
            entity.ScheduleForTrigger(Postprocess);
        }

        internal bool TryToRelaxGroupMembers()
        {
            Entity lastMember = entity.members.LastOrDefault();
            if (!(lastMember is null))
            {
                if (lastMember.type == Entity.Type.Group)
                {
                    SetGroupMembersCollidersTriggers(false);
                    UnFreezeGroupsMembers();
                    if (lastMember.containers.GetCurrentGroup(out Container lastMembersGroupContainer))
                    {
                        lastMembersGroupContainer.SetPositionCorrection();
                    }
                    return true;
                }
                // currently only the groups are linked. hence the group can proceed with reshaping procession if the type != group.
                else
                {
                    return true;
                }

            }
            return false;
        }

        /// <summary>
        /// called by the last member of a group once it has relaxed its rigidbody's movement restriction AND all that group's members' colliders have met.
        /// </summary>
        internal void GroupHasSettled()
        {
            SetGroupMembersCollidersTriggers(true);
            SetColliderToTrigger(true);
            FreezeGroupsMembers();
            shapes.GetGroupShape().UpdateShape();
            groupHasSettled = true;
            ready = true;

            messenger.Post(new Message()
            {
                LayoutEvent = Layouts.Layouts.LayoutEvent.HasSettled,
                EventOrigin = Layouts.Layouts.EventOrigin.Group,
                obj = new object[] { entity, this, gameObject },
                to = entity.parent,
                from = entity
            });
        }
        /// <summary>
        /// if TRUE, members' colliders "isTrigger" is set to TRUE, e.g. colliders are NOT going to collide "physically";
        /// if FALSE, members' colliders "isTrigger" is set to FALSE, e.g. colliders ARE going to collide "physically".
        /// </summary>
        /// <param name="setTo"></param>
        internal void SetGroupMembersCollidersTriggers(bool setTo)
        {
            foreach (Container member in MembersAsGroupsWithShapes())
            {
                member.SetColliderToTrigger(setTo);
            }
            return;
        }
        internal void SetGroupMembersJointAnchors(Vector3 setTo)
        {
            foreach (Container member in entity.members.Where(mem => mem.type == Entity.Type.Group).Select(member => member.containers.GetCurrentGroup()))
            {
                if (!(member is null))
                {
                    member.joints.SetConnectedAnchors(setTo);
                }
            }
        }
        internal void SetColliderToTrigger(bool active)
        {
            if (containerCollider is null)
            {
                GetCollider();
            }
            containerCollider.isTrigger = active;
        }
    }
}
