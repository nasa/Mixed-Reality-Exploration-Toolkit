/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */

using Assets.VDE.Communication;
using Assets.VDE.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.VDE.Layouts.NetworkTopology
{
    /// <summary>
    /// this is an example on how to add custom Layouts that would be activated with Activator.CreateSerializedType
    /// </summary>
    public class Layout : Assets.VDE.Layouts.Layout
    {
        Log log;
        List<Container> shapesOnRing = new List<Container> { };
        /*
         * reserved for future use
        List<Container> shapesOnPole = new List<Container> { };
        bool memberLongValueUpdated = false;
        int poleDistance = 50,
            poleDistanceFinal = 15;
        */
        public Layout(string name) : base(name)
        {
            log = new Log("Layout " + name);
        }
        internal new void Init(Data data)
        {
            base.Init(data);
            data.VDE.controller.cameraCollider.radius = variables.floats["cameraColliderRadius"];
            data.messenger.Post(new Message()
            {
                HUDEvent = UI.HUD.HUD.Event.Progress,
                number = 2,
                floats = new List<float> { 0.1F, 1F },
                message = "Loading layout " + name,
                from = data.layouts.current.GetGroot()
            });
        }
        override internal Vector3 GetTier1ContainerPosition(Container container, float diameter)
        {
            //float anger = (container.entity.pos + 1) * Mathf.PI * 2 / (container.entity.siblings.Count() - 1);
            float anger = container.entity.siblings.Where(sib => sib.pos < container.entity.pos).Count() * Mathf.PI * 2 / (container.entity.siblings.Count());
            return new Vector3(Mathf.Cos(anger), 0, Mathf.Sin(anger)) * diameter / 2;
        }
        /// <summary>
        /// probably obsolete as of 20210621
        /// </summary>
        /// <returns></returns>
        override internal System.Collections.IEnumerator Reinitialize()
        {
            state = State.reinitializing;
            Dictionary<Container, Quaternion> toReturnTo = new Dictionary<Container, Quaternion> { };
            foreach (Container shallowGroup in shapesOnRing)
            {
                toReturnTo.Add(shallowGroup, shallowGroup.transform.rotation);
                shallowGroup.transform.eulerAngles = Vector3.zero;
            }
            while (data.messenger.CheckLoad() > 0)
            {
                yield return new WaitForSeconds(data.random.Next(123, 234) / 100);
            }
            foreach (KeyValuePair<Container,Quaternion> shallowGroup in toReturnTo)
            {
                shallowGroup.Key.transform.rotation = shallowGroup.Value;
            }
            state = State.active;
            yield return true;
        }

        protected override void LayoutReady()
        {
            state = State.populated;
            data.messenger.Post(new Message()
            {
                HUDEvent = UI.HUD.HUD.Event.Progress,
                number = 2,
                floats = new List<float> { 0.5F, 1F },
                message = "Loading layout " + name,
                from = data.layouts.current.GetGroot()
            });

            /*
             * needs https://docs.unity3d.com/ScriptReference/Rigidbody-constraints.html fix
            foreach (Container item in shapesOnRing.OrderBy(cont => cont.entity.pos))
            {
                item.gameObject.transform.LookAt(grootsContainer.transform);
                item.gameObject.transform.Rotate(new Vector3(0, 180, 0));
            }*/

            // 20220222: after the rewrite of link creation procession, this is not needed anymore.
            // and would conflict with lines that are created hidden.
            //data.links.SetAllLinkStatusTo(true);
            data.entities.SetShapesCollidersToTriggers(true);

            data.messenger.Post(new Message()
            {
                LayoutEvent = Layouts.LayoutEvent.LayoutReady,
                EventOrigin = Layouts.EventOrigin.Group,
                from = GetGroot(),
                layout = this,
                message = name + " is ready",
                obj = new object[] { this, this, this },
            });

            data.messenger.Post(new Message()
            {
                HUDEvent = UI.HUD.HUD.Event.Progress,
                number = 2,
                floats = new List<float> { 1, 1 },
                message = "",
                from = data.layouts.current.GetGroot()
            });

            log.Entry("LayoutReady: " + data.entities.Count() + " & " + data.links.links.Count);
            try
            {
                data.VDE.startupScreen.SetActive(false);
            }
            catch { }
        }

        override internal Vector3 PositionShallowGroup(UI.Group.Container container)
        {
            Vector3 toReturn;
            container.SetPositionCorrection(Container.PositionCorrectionDirection.freeze);
            toReturn = GetTier1ContainerPosition(container, GetRigidJointValueFromConf(UI.Joint.Type.MemberGroot, "JointMaxDistance") * 20);
            if (!shapesOnRing.Contains(container))
            {
                shapesOnRing.Add(container);
            }
            container.resetToPosition = toReturn;
            return toReturn;
        }
        override internal Vector3 PositionDeepGroupNode(Container container)
        {
            return new Vector3(
                (container.entity.vectors[1] >= 1) ? container.entity.vectors[1] - 1 : container.entity.vectors[1],
                0,
                (container.entity.vectors[2] >= 1) ? container.entity.vectors[2] - 1 : container.entity.vectors[2]
            );
        }
    }
}