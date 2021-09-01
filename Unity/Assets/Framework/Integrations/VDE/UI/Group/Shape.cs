/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.VDE.UI.Group
{
    internal class Shape : Assets.VDE.UI.Shape
    {
        IEnumerable<Assets.VDE.UI.Shape> membersWithShapes;
        internal new void Init(Entity entity, Group.Container group)
        {
            base.Init(entity, group);

            padding = layout.variables.vectors["groupPadding"];
            margin = layout.variables.vectors["groupMargin"];
            nodeSize = layout.variables.vectors["nodeSize"];

            MembersWithShapes(entity);
        }

        private void MembersWithShapes(Entity entity)
        {
            membersWithShapes = entity.members.
                Where(mem =>
                    mem.containers.containers.Where(cont => cont.shapes.IsReady() && cont.layout == layout).Count() > 0).
                Select(mem => mem.containers.GetShape(layout)).
                Where(member => member.gameObject.TryGetComponent(out Renderer _));
        }

        private void Start()
        {
            SetColor(layout.variables.colours["groupColour"]);
            GetComponent<MeshRenderer>().enabled = !entity.shapeless;
            GetComponent<MeshRenderer>().material.EnableKeyword("_EmissiveExposureWeight");
        }
        internal override void GotFocus()
        {
            data.VDE.hud.CreateLabel(entity);
        }
        /// <summary>
        /// this shall be called ONLY from main thread via trunk scheduling.
        /// </summary>
        /// <returns></returns>
        internal bool UpdateShape()
        {
            if (state == State.Updating || entity.shapeless || !membersWithShapes.Any())
            {
                return true;
            }
            state = State.Updating;
            if (layout.state == Layouts.Layout.State.populated || layout.state == Layouts.Layout.State.ready)
            {
                return true;
            }
            bool[] boo = new bool[3] { false, false, false };
            Quaternion t2rot = transform.rotation;
            bool turn = false;
            if (!(entity is null) && !(entity.tier2ancestor is null) && entity.tier2ancestor.isControlObject)
            {
                turn = true;
            }
            if (turn)
            {
                t2rot = entity.tier2ancestor.containers.GetContainer().transform.rotation;
                entity.tier2ancestor.containers.GetContainer().transform.eulerAngles = Vector3.forward;
            }
            Vector3 initialPosition = transform.localPosition;
            Vector3 initialScale = transform.localScale;
            transform.localScale = Vector3.zero;

            Assets.VDE.UI.Shape x = membersWithShapes.
                OrderByDescending(member => member.transform.position.x + member.gameObject.GetComponent<Renderer>().bounds.extents.x).
                FirstOrDefault();

            Assets.VDE.UI.Shape y = membersWithShapes.
                OrderByDescending(member => member.transform.position.y + member.gameObject.GetComponent<Renderer>().bounds.extents.y).
                FirstOrDefault();

            Assets.VDE.UI.Shape z = membersWithShapes.
                OrderByDescending(member => member.transform.position.z + member.gameObject.GetComponent<Renderer>().bounds.extents.z).
                FirstOrDefault();

            boo[0] = EncapsulateShape(x);
            if (x != y)
            {
                boo[1] = EncapsulateShape(y);
            }
            if (x != z && y != z)
            {
                boo[2] = EncapsulateShape(z);
            }
            if (turn)
            {
                entity.tier2ancestor.containers.GetContainer().transform.rotation = t2rot;
            }
            if (boo.Contains(true))
            {
                ReportChange();
            }
            state = State.Ready;
            return true;
        }

        internal void ReportChange()
        {
            if (!membersWithShapes.Where(mem => !mem.ready).Any())
            {
                ready = true;
                messenger.Post(new Communication.Message()
                {
                    LayoutEvent = Layouts.Layouts.LayoutEvent.Ready,
                    EventOrigin = Layouts.Layouts.EventOrigin.Shape,
                    obj = new object[] { entity, this, gameObject },
                    to = entity.parent,
                    from = entity
                });
            }
            //else
            {
            //    Debug.LogError(name + " NOT reporting change, because: " + ready + ":" + (membersWithShapes.Count() == entity.members.Count()));
            }
        }
        /// <summary>
        /// yes, unity does have a cute bounds encapsulation method. no, it does not work (correctly). 
        /// hence we need to encapsulate the contained objects through a reptile's reproductive organ.
        /// </summary>
        /// <param name="other"></param>
        private bool EncapsulateShape(Assets.VDE.UI.Shape other)
        {
            Vector3
                newScale,
                newScaleAbs,
                newScaleMax,
                newScaleMin,
                newPosition,
                otherShapesMinPoint,
                otherShapesMaxPoint;

            if (!(other is null) && other.TryGetComponent(out Renderer otherRenderer))
            {
                // get the local position of the other container, in the local relative space of THIS shape
                if (other.entity.type == Entity.Type.Node)
                {
                    otherShapesMaxPoint = container.transform.InverseTransformPoint(other.transform.position + otherRenderer.bounds.size);
                    otherShapesMinPoint = container.transform.InverseTransformPoint(other.transform.position - otherRenderer.bounds.size);
                }
                else
                {
                    otherShapesMaxPoint = container.transform.InverseTransformPoint(other.transform.position + otherRenderer.bounds.extents);
                    otherShapesMinPoint = container.transform.InverseTransformPoint(other.transform.position - otherRenderer.bounds.extents);
                }

                newScaleMax = otherShapesMaxPoint;

                newScaleMax = new Vector3(
                    (newScaleMax.x > transform.localScale.x) ? newScaleMax.x : transform.localScale.x,
                    (newScaleMax.y > transform.localScale.y) ? newScaleMax.y : transform.localScale.y,
                    (newScaleMax.z > transform.localScale.z) ? newScaleMax.z : transform.localScale.z);

                newScaleMin = otherShapesMinPoint;

                newScaleMin = new Vector3(
                    (newScaleMin.x < transform.localScale.x) ? newScaleMin.x : transform.localScale.x,
                    (newScaleMin.y < transform.localScale.y) ? newScaleMin.y : transform.localScale.y,
                    (newScaleMin.z < transform.localScale.z) ? newScaleMin.z : transform.localScale.z);

                newScaleAbs = new Vector3(
                    Mathf.Abs((newScaleMin.x < transform.localScale.x) ? newScaleMin.x : transform.localScale.x),
                    Mathf.Abs((newScaleMin.y < transform.localScale.y) ? newScaleMin.y : transform.localScale.y),
                    Mathf.Abs((newScaleMin.z < transform.localScale.z) ? newScaleMin.z : transform.localScale.z));

                newScale = new Vector3(
                    Mathf.Max(newScaleAbs.x, newScaleMax.x),
                    Mathf.Max(newScaleAbs.y, newScaleMax.y),
                    Mathf.Max(newScaleAbs.z, newScaleMax.z));

                if (other.entity.type == Entity.Type.Node)
                {
                    newPosition = (newScaleMax / 2) - ((newScaleAbs - newScaleMin) / 4);

                    if (newPosition.x > nodeSize.x / 2)
                    {
                        newPosition.x -= nodeSize.x / 2;
                    }
                    if (newPosition.y > nodeSize.y / 2)
                    {
                        newPosition.y -= nodeSize.y / 2;
                    }
                    if (newPosition.z > nodeSize.z / 2)
                    {
                        newPosition.z -= nodeSize.z / 2;
                    }
                }
                else
                {
                    newPosition = (newScaleMax / 2) - ((newScaleAbs - newScaleMin) / 4);
                    if (newPosition.z > nodeSize.z / 2)
                    {
                        newPosition -= margin / 2;
                    }
                    newScale += margin;
                }

                SetPositionAndScale(newPosition, newScale);
                return true;
            }
            return false;
        }

        internal void ScheduleUpdate()
        {
            messenger.RequestShapeUpdate(this);
        }
        internal void AdoptMember(Assets.VDE.UI.Container otherContainer)
        {
            if (!membersWithShapes.Contains(otherContainer.shapes.GetShape()))
            {
                MembersWithShapes(entity);
            }

            if (membersWithShapes.Contains(otherContainer.shapes.GetShape()))
            {
                ScheduleUpdate();
            }
        }
        /// <summary>
        /// set the visibilitiy of this shape and notify the members of this group that a camera has entered the building.
        /// </summary>
        /// <param name="other"></param>
        /// <param name="setTo"></param>
        override internal void CheckIfToSetVisibility(GameObject other, bool setTo)
        {
            if (other.CompareTag("MainCamera"))
            {
                SetVisibility(setTo);
            }
        }
        override internal void SetVisibility(bool setTo)
        {
            if (rendererer is null)
            {
                TryGetComponent(out rendererer);
            }

            if (!(rendererer is null))
            {
                rendererer.enabled = setTo;
                foreach (Entity member in entity.members.Where(mem => mem.type == Entity.Type.Node))
                {
                    Node.Container ns = member.containers.GetCurrentNode();
                    if (!(ns is null) && !(ns.label is null))
                    {
                        ns.label.SetActive(!setTo);
                    }
                }
            }
        }
    }
}
