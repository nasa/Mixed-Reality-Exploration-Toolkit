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
    internal class Shape : MonoBehaviour
    {
        internal Log log;
        internal Data data;
        internal Layout layout;
        internal Entity entity;
        internal Entity.Type type;
        internal bool cameraIsClose;
        internal Renderer rendererer;
        internal Group.Container container;
        internal Messenger messenger;
        internal Vector3 
            padding,
            margin,
            nodeSize;
        internal bool 
            ready = false;
        internal State state;
        internal enum State
        {
            Pure,
            Ready,
            Updating
        }
        internal float updateScheduled = 0F;
        internal List<GameObject> waitingForRigidBody = new List<GameObject> { };
        internal List<GameObject> readyForRigidBody = new List<GameObject> { };

        internal void Init(
            Entity _entity,
            Group.Container _group)
        {
            container = _group;
            entity = _entity;
            type = entity.type;
            data = entity.data;
            messenger = data.messenger;
            log = new Log("Shape of " + entity.name,messenger);
            if (container is null || container.layout is null)
            {
                layout = data.layouts.current;
            }
            else
            {
                layout = container.layout;
            }

            if (entity.doJoints && !(container.joints is null) && !(container.joints.joints is null))
            {
                foreach (Joint joint in container.joints.joints.Where(joint => joint.src == entity))
                {
                    joint.ReceiveCreaturesForSRC(new object[] { entity, container, this });
                }
            }

            if (entity.presetPosition != Vector3.zero)
            {
                SetPositionAndScale(
                    entity.presetPosition,
                    entity.presetScale);
                SetRotation(entity.presetRotation);
            }

            // Color is not nullabel, hence checking the c here.
            if (!(entity.c is null))
            {
                SetColor(entity.c);
            }

            messenger.Post(new Message()
            {
                LayoutEvent = Layouts.Layouts.LayoutEvent.GotVisibleShape,
                obj = new object[] { entity, this, gameObject },
                to = entity.parent,
                from = entity
            });
        }

        private void Start()
        {
            TryGetComponent(out rendererer);
        }
        /// <summary>
        /// Set the position of this SHAPE in relations to its parent CONTAINER.
        /// To set the position of a visible object, adjust the position of the CONTAINER instead of a shape.
        /// SetPositionAndScale reports that event to parent so, that parent could forward that to the last entity in this group to adjust its position.
        /// </summary>
        /// <param name="position"></param>
        internal void SetPositionAndScale(Vector3 position, Vector3 scale)
        {
            transform.localScale = scale;
            transform.localPosition = position;
            container.SetColliderPositionAndScale(position, scale);
            if (entity.parent.containers.GetCurrentGroupShape(out Group.Shape groupShape))
            {
                groupShape.ScheduleUpdate();
            }
            if (!(container.label is null))
            {
                container.label.transform.localPosition = new Vector3(container.label.transform.localPosition.x, scale.y, container.label.transform.localPosition.z);
            }
        }

        /// <summary>
        /// if you extend a shape, override these and do whatever it is you want to do in case of L8 focus.
        /// </summary>
        internal virtual void GotFocus() { }
        internal virtual void LostFocus() { }
        internal virtual void Relax() { }
        internal virtual void BePresentable() { }
        internal Color GetColor()
        {
            return GetComponent<MeshRenderer>().material.color;
        }
        internal void SetColor(string c)
        {
            if (!(c is null))
            {
                ColorUtility.TryParseHtmlString(entity.c, out entity.presetColour);
                SetColor(entity.presetColour);
            }
        }
        internal void SetColor(Color colour)
        {
            MeshRenderer render = GetComponent<MeshRenderer>();
            // depending on the material used by the shape, colour variables may have various names inside the material. hence this brute force.
            render.material.color = colour;

            // for non-URP shaders. just in any case.
            render.material.SetColor("_TintColor", colour);

            // _BaseColor alpha controls intensity, while _EmissiveColor controls color. go figure the logic..

            // if the colour is not white, adjust the
            if (colour.r + colour.g + colour.b < 3)
            {
                render.material.SetColor("_EmissiveColor", colour);
                render.material.SetColor("_BaseColor", new Color(1,1,1,colour.a));
                render.material.SetFloat("_EmissiveExposureWeight", 0);
                
            }
            // in case of HDRP / URP and HDRP/Lit shader, _EmissiveExposureWeight is brighter (colourless) white, if < 0.6
            // but darker with colour, if > 0.7 while < 1.1
            // and void black if > 1.1
            // hence we need to substract the alpha from the value, in case the colour is white.
            else
            {
                render.material.SetColor("_EmissiveColor", Color.white);
                render.material.SetColor("_BaseColor", new Color(1, 1, 1, colour.a));
                if (colour.a > 0.8)
                {
                    render.material.SetFloat("_EmissiveExposureWeight", 0 - colour.a);
                }
                else
                {
                    render.material.SetFloat("_EmissiveExposureWeight", 0);
                }
            }            
        }
        private void OnTriggerEnter(Collider other)
        {
            CheckIfToSetVisibility(other.gameObject, false);
        }
        private void OnTriggerExit(Collider other)
        {
            CheckIfToSetVisibility(other.gameObject, true);
        }
        virtual internal void CheckIfToSetVisibility(GameObject other, bool setTo) { }
        virtual internal void SetVisibility(bool setTo) { }

        internal void SetRotation(Vector3 vector3)
        {
            transform.eulerAngles = vector3;
        }
    }
}
