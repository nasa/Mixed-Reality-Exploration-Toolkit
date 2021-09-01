/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
using Assets.VDE.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.VDE.Layouts
{
    /// <summary>
    /// this is the default Layout that other should extend.
    /// </summary>
    public class Layout
    {
        Log log;
        internal Data data;
        internal string name = "default";
        
        // to be deprecated
        internal bool
            initializing = false,
            initialized = false,
            populated = false,
            active = false,
            ready = false;

        internal State state;
        internal enum State
        {
            ready,
            active,
            populated,
            initialized,
            initializing,
            reinitializing
        }

        internal Variables variables;
        internal Dictionary<int, int[]> shallowShapes = new Dictionary<int, int[]> { };
        Entity groot;

        public Layout(string name)
        {
            this.name = name;
            log = new Log("Layout " + name);
        }
        /// <summary>
        /// cant use Start() nor constructor, because the configuration has to be loaded before initializing the scenes and
        /// Data cannot be passed into the constructor via Activator.CreateInstance.
        /// </summary>
        public async void Init(Data data)
        {
            this.data = data;
            variables = new Variables(this);
            
            try
            {
                while (data.VDE.controller is null)
                {
                    await data.UI.Sleep(90);
                }
                while (data.VDE.controller.cameraObserver is null)
                {
                    await data.UI.Sleep(90);
                }
                while (data.VDE.controller.cameraObserver.usableCamera is null)
                {
                    await data.UI.Sleep(90);
                }
                if (!data.VDE.controller.cameraObserver.usableCamera.TryGetComponent(out data.VDE.controller.cameraCollider))
                {
                    data.VDE.controller.cameraCollider = data.VDE.controller.cameraObserver.usableCamera.gameObject.AddComponent<CameraCollider>();
                    data.burnOnDestruction.Add(data.VDE.controller.cameraCollider.collie);
                    data.burnOnDestruction.Add(data.VDE.controller.cameraCollider);
                }
                data.VDE.controller.cameraCollider.radius = variables.floats["cameraColliderRadius"];
            }
            catch (Exception exe)
            {
                log.Entry("Wrong error: " + exe.StackTrace);
            }

            data.VDE.hud.notificationAreaOffset = variables.vectors["notificationPosition"];
            data.VDE.hud.HUDPosition = variables.vectors["HUDPosition"];

            initialized = (!initialized) || true;
            log = new Log("Layout " + name, data.messenger); 
            data.messenger.SubscribeToEvent(Populated, Layouts.LayoutEvent.LayoutPopulated.ToString(), 0);
            data.messenger.Post(new Communication.Message()
            {
                TelemetryType = Communication.Telemetry.Type.status,
                telemetry = new Communication.Telemetry()
                {
                    type = Communication.Telemetry.Type.status,
                    status = new Communication.Status[] {
                        new Communication.Status() {
                            status = false,
                            name = "Layout " + name,
                            description = "Layout " + name
                        }
                    },
                }
            });
            if (variables.flags["adjustToFPS"])
            {
                data.messenger.SubscribeToEvent(AdjustForSPS, Layouts.LayoutEvent.AdjustForFPS.ToString(), 0);
            }

            log.Entry("Ready", Log.Event.ToServer);
        }

        private IEnumerator AdjustForSPS(object[] anObject)
        {
            if (ready)
            {
                if ((int)anObject[0] < data.layouts.current.variables.indrek["targetFPS"] && data.entities.ActiveEntities() > data.entities.Count() / 2)
                {
                    data.entities.SetEntitiesVisibilityForFPS(5, false);
                }
                else if ((int)anObject[0] > data.layouts.current.variables.indrek["targetFPS"] + 5 && data.entities.ActiveEntities() < data.entities.Count())
                {
                    data.entities.SetEntitiesVisibilityForFPS(5, true);
                }
            }
            yield return true;
        }
        /// <summary>
        /// this triggers prior to layoutSPECIFIC finalization of the view: this.LayoutReady() will do that and THEN send message to Ready(), to (again) optimize the view.
        /// </summary>
        /// <param name="anObject"></param>
        /// <returns></returns>
        private IEnumerator Populated(object[] anObject)
        {
            if (!populated && anObject[0] as Layout == this && data.layouts.current == this)
            {
                populated = true;
                //data.messenger.SubscribeToEvent(Ready, Layouts.LayoutEvent.LayoutReady.ToString(), 0);
                data.VDE.SetPositionAndScale(variables.vectors["dashboardCenter"], variables.vectors["defaultScale"]);

                WiggleJoints();
                yield return data.UI.Sleep(2345,1234);
                ContractJointsAndUpdateShapes();

                int load = data.messenger.CheckLoad();
                while (load > 0 || !CheckForOutOfLineGroups())
                {
                    data.messenger.Post(new Communication.Message()
                    {
                        HUDEvent = UI.HUD.HUD.Event.Progress,
                        number = 2,
                        floats = new List<float> { data.entities.entities.Where(ent => ent.Value.ready && ent.Value.containers.GetContainer().state == Container.State.ready).Count() / data.entities.Count(), 1F },
                        message = "Loading layout " + name,
                        from = data.layouts.current.GetGroot()
                    });
                    yield return data.UI.Sleep(2345);
                    
                    load = data.messenger.CheckLoad();
                    data.messenger.Post(new Communication.Message()
                    {
                        HUDEvent = UI.HUD.HUD.Event.Progress,
                        number = 1,
                        floats = new List<float> { load / data.messenger.maxLoad, 1F },
                        message = "Loading layout " + name,
                        from = data.layouts.current.GetGroot()
                    });
                }

                data.messenger.Post(new Communication.Message()
                {
                    HUDEvent = UI.HUD.HUD.Event.Progress,
                    number = 2,
                    floats = new List<float> { 1, 1F },
                    message = "",
                    from = data.layouts.current.GetGroot()
                });
                data.messenger.Post(new Communication.Message()
                {
                    HUDEvent = UI.HUD.HUD.Event.Progress,
                    number = 1,
                    floats = new List<float> { 1, 1F },
                    message = "",
                    from = data.layouts.current.GetGroot()
                });

                Ready();
                data.messenger.Post(new Communication.Message()
                {
                    TelemetryType = Communication.Telemetry.Type.status,
                    telemetry = new Communication.Telemetry()
                    {
                        type = Communication.Telemetry.Type.status,
                        status = new Communication.Status[] {
                            new Communication.Status() {
                                status = true,
                                name = "Layout " + name,
                                description = "Layout " + name
                            }
                        },
                    }
                });
            }
            yield return true;
        }
        /// <summary>
        /// if NO groups that have gotten out of line are found, returns TRUE. 
        /// if out of line groups are found, orders their PARENTS to dicipline 'em (and returns FALSE).
        /// </summary>
        /// <returns></returns>
        private bool CheckForOutOfLineGroups()
        {
            return !data.entities.entities.Where(ent =>
                ent.Value.type == Entity.Type.Group &&
                ent.Value.containers.GetCurrentGroup(out UI.Group.Container cont) &&
                cont.CheckIfOutOfLine()
                ).Any();
        }

        private void WiggleJoints()
        {
            foreach (IEnumerable<UI.Joint> joints in data.entities.entities.Values.
                Where(ent => ent.type == Entity.Type.Group).
                OrderByDescending(ent => ent.distanceFromGroot).
                ThenByDescending(ent => ent.pos).
                Select(ent => ent.containers.GetGroup(this).joints.joints.
                    Where(joint => joint.src == ent)
                )
            ) {
                foreach (UI.Joint joint in joints)
                {
                    joint.Wiggle();
                }
            }
        }


        internal void ContractJointsAndUpdateShapes()
        {
            data.VDE.controller.inputObserver.inputEvent.Invoke(new UI.Input.Event
            {
                function = UI.Input.Event.Function.UpdateShapes,
                type = UI.Input.Event.Type.Bool,
                Bool = true
            });
        }

        internal void ForcePositionAhead()
        {
            // -2 will get to us into the tier 3 groups.
            int maxDepth = GetGroot().distanceFromGroot - 2;

            IEnumerable<int> calmTrunks = data.messenger.triggers.Where(trg => trg.Key.readyForTrigger).Select(trg => trg.Key.id);

            // iterate over groups, that directly do NOT contain entities (e.g. only groups of groups), starting with deepest leafs
            foreach (UI.Group.Container item in data.entities.entities.Where(ent => 
                !calmTrunks.Contains(ent.Value.tier2ancestorID) &&
                ent.Value.type == Entity.Type.Group && 
                ent.Value.distanceFromGroot > 1 &&
                ent.Value.containers.GetCurrentGroup().state == Container.State.triggering).OrderByDescending(ent => 
                    ent.Value.distanceFromGroot).Select(ent => 
                        ent.Value.containers.GetCurrentGroup()
                    )
                )
            {
                if (!(item is null))
                {
                    item.PositionSelfAmongstSiblings(false);
                    item.entity.ScheduleForTrigger();
                }
            }
        }

        internal void AdjustJointsToScale(float scale)
        {
            // -2 will get to us into the tier 3 groups.
            int maxDepth = GetGroot().distanceFromGroot - 2;

            // iterate over groups, that directly do NOT contain entities (e.g. only groups of groups), starting with deepest leafs
            foreach (UI.Group.Shape item in data.entities.entities.Where(ent => 
                ent.Value.type == Entity.Type.Group && 
                ent.Value.distanceFromGroot > 1).OrderBy(ent => 
                    ent.Value.distanceFromGroot).Select(ent => 
                        ent.Value.containers.GetGroupShape(this)
                    )
                )
            {
                if (!(item is null))
                {
                    item.container.joints.joints.ForEach(join => join.SetJointValues(scale));
                }
            }

            // also set the defaults so, that incoming entities would get correctly sized, boulder sized joints.
            foreach (UI.Joint.Type type in Enum.GetValues(typeof(UI.Joint.Type)))
            {
                variables.joints[type].maxDistance = variables.joints[type].maxDistance * scale;
                variables.joints[type].minDistance = variables.joints[type].minDistance * scale;
                variables.joints[type].tolerance = variables.joints[type].tolerance * scale;
            }

            data.VDE.controller.inputObserver.inputEvent.Invoke(new UI.Input.Event
            {
                function = UI.Input.Event.Function.UpdateLinks,
                type = UI.Input.Event.Type.Bool,
                Bool = true
            });
        }

        private void Ready()
        {
            this.LayoutReady();
            ready = true;
            state = State.ready;
            data.messenger.Post(new Communication.Message() { 
                TelemetryType = Communication.Telemetry.Type.status,
                telemetry = new Communication.Telemetry() { 
                    type = Communication.Telemetry.Type.status,
                    status = new Communication.Status[] { 
                        new Communication.Status() { 
                            status = true,
                            name = "Layout " + name,
                            description = "Layout " + name
                        }
                    },
                }
            });
        }

        internal void CompactJoints()
        {
            log.Entry("Compacting joints from " + variables.joints[UI.Joint.Type.MemberMember].minDistance);
            
            variables.joints[UI.Joint.Type.MemberMember].maxDistance = variables.joints[UI.Joint.Type.MemberMember].maxDistance * variables.vectors["defaultScale"].x;
            variables.joints[UI.Joint.Type.MemberMember].minDistance = variables.joints[UI.Joint.Type.MemberMember].minDistance * variables.vectors["defaultScale"].x;

            log.Entry("Compacting joints to " + variables.joints[UI.Joint.Type.MemberMember].minDistance);
            
            data.VDE.controller.inputObserver.inputEvent.Invoke(new UI.Input.Event
            {
                function = UI.Input.Event.Function.UpdateLinks,
                type = UI.Input.Event.Type.Bool,
                Bool = true
            });

            // if this happens after rotating trunks, it will mess up the shapes.
            data.VDE.controller.inputObserver.inputEvent.Invoke(new UI.Input.Event
            {
                function = UI.Input.Event.Function.UpdateShapes,
                type = UI.Input.Event.Type.Bool,
                Bool = true
            });
            
        }

        internal Entity GetGroot()
        {
            if (groot == null)
            {
                groot = data.entities.Get(0);
            }
            return groot;
        }

        /// <summary>
        /// looks for the falue from layout config; if undefined there, tries the default conf; if that fails too, returns 0F;
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public float GetValueFromConf(string key)
        {
            if (
                data.config.layouts.ContainsKey(name)
                && data.config.layouts[name].TryGetValue(key, out double response))
            {
                return (float)response;
            }
            else if (
                data.config.layouts.ContainsKey("default")
                && data.config.layouts["default"].TryGetValue(key, out double dResponse))
            {
                return (float)dResponse;
            }
            else
            {
                return 0F;
            }
        }
        internal float GetRigidJointValueFromConf(UI.Joint.Type type, string key)
        {
            if (
                data.config.layouts.ContainsKey(name)
                && !(data.config.layouts[name].rigidJoints is null)
                && data.config.layouts[name].rigidJoints.ContainsKey(type.ToString())
                && data.config.layouts[name].rigidJoints[type.ToString()].ContainsKey(key))
            {
                return (float)data.config.layouts[name].rigidJoints[type.ToString()][key];
            }
            else if (
                data.config.layouts.ContainsKey("default")
                && !(data.config.layouts["default"].rigidJoints is null)
                && data.config.layouts["default"].rigidJoints.ContainsKey(type.ToString())
                && data.config.layouts["default"].rigidJoints[type.ToString()].ContainsKey(key))
            {
                return (float)data.config.layouts["default"].rigidJoints[type.ToString()][key];
            }
            return 0F;
        }
        protected virtual void LayoutReady() { }
        internal virtual IEnumerator Reinitialize() { yield return true; }
        /// <summary>
        /// should be called only from a Container on itself. Does not alter container's position - container should do that itself.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        internal virtual Vector3 PositionEntity(Entity entity, Container container)
        {
            if (entity is null)
            {
                return Vector3.zero;
            }

            if (entity.id == 0 && entity.parentID == -1)
            {
                entity.standYourGround = true;
                container.SetPositionCorrection(Container.PositionCorrectionDirection.standYourGround);
                container.GetRigid(UI.Joint.Type.MemberGroot);
                return Vector3.zero;
            }

            if (entity.type == Entity.Type.Group)
            {
                if (entity.parent == GetGroot() && entity.distanceFromGroot <= 1)
                {
                    return PositionShallowGroup(container as UI.Group.Container);
                }
                else if (entity.distanceFromGroot >= 1)
                {
                    return PositionDeepGroup(container as UI.Group.Container);
                }
            }
            else if (entity.type == Entity.Type.Node)
            {
                if (
                    !(entity.vectors is null) &&
                    entity.vectors.Length > 2 &&
                    entity.vectors[1] > 0 &&
                    entity.vectors[2] > 0
                    )
                {
                    return PositionDeepGroupNode(container);
                }
                else
                {
                    return PositionShallowGroupNode(container);
                }
            }
            else
            {
                log.Entry("PositionEntity dont know hot to handle Entity with a type of: " + entity.type.ToString(), Log.Event.ToServer);
            }
            return Vector3.zero;
        }

        virtual internal Vector3 PositionDeepGroupNode(Container container)
        {
            return new Vector3(container.entity.vectors[0], container.entity.vectors[1], container.entity.vectors[2]);
        }

        virtual internal Vector3 PositionShallowGroupNode(Container container)
        {
            float nodeOffset = 0.2F;
            Entity entity = container.entity;
            if (
                container.defaultPosition.magnitude > 0 || 
                container.transform.localPosition.magnitude > 0 || 
                entity.siblings.First() == entity || 
                entity.siblings.OrderBy(ent => ent.id).First() == entity
                )
            {
                return container.transform.localPosition;
            }
            // at this point entity members' viewis not accessible yet, hence we need to estimated size of the datashape based on its (current) member count
            int nodesInShape = entity.parent.relations.Where(rel => rel.Value == Entity.Relation.Child).Count();

            if (nodesInShape > variables.indrek["maxNodesInShape"])
            {
                nodeOffset = variables.floats["nodeOffset"] / 2;
            }
            else
            {
                nodeOffset = variables.floats["nodeOffset"];
            }
            int dataShapeWidth = Mathf.Max((int)Mathf.Sqrt(nodesInShape), variables.indrek["minNodesInRow"]);

            // positions here are: x, Z, y. reason being, that we want the shape to grow upwards
            int[] max = new int[3] { dataShapeWidth, 1, 9999 };
            if (!shallowShapes.ContainsKey(entity.parent.id))
            {
                shallowShapes.Add(entity.parent.id, new int[3] { 0, 0, 0 });
            }
            //int[] current = new int[3] { 0, 0, 0 };
            return this.PositionKnownNode(container, max, nodeOffset);
        }
        private Vector3 PositionDeepGroup(UI.Group.Container container)
        {            
            Entity target;
            Vector3 toReturn = Vector3.zero;

            container.SetPositionCorrection(GetPositionCorrectionDirection(container.entity.distanceFromGroot));

            // the default layout alternates groups between distancing the groups FARTHER or UP.
            // depthOfRelations indicates, how many layers of groups are underneath the group, for which a container is being positioned.

            toReturn =
                (container.entity.pos * container.direction) +
                (container.entity.pos * variables.floats["groupPadding"] * container.direction);
            if (container.immediateSiblings.Count() > 0)
            {
                if (container.CheckIfFirstInGroup())
                {
                    container.SetPositionCorrection(Container.PositionCorrectionDirection.standYourGround);
                    target = container.immediateSiblings.Last();
                    toReturn = new Vector3(0, 0, 0);
                }
                else
                {
                    target = container.immediateSiblings.First();
                }
                container.AdoptSibling(target, target.containers.GetGroup(this));
            }
            else
            {
                container.SetPositionCorrection(Container.PositionCorrectionDirection.standYourGround);
                toReturn = Vector3.zero;
            }

            // in case the position has been precalculated in server, apply those instead.
            if (container.entity.vectors != null && container.entity.vectors.Length > 2)
            {
                toReturn = new Vector3(container.entity.vectors[0], container.entity.vectors[1], container.entity.vectors[2]);
            }

            container.AdoptParent(container.entity.parent, container.entity.parent.containers.GetGroup(this));

            return toReturn;
        }
        /// <summary>
        /// position a group that is next in hierarchy from groot. these are the Trunk containers.
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        virtual internal Vector3 PositionShallowGroup(UI.Group.Container container) {
            return Vector3.zero; 
        }

        internal virtual Vector3 GetContainerPositionOnSiblingsRing(Container container, float diameter)
        {
            //float circumference = Mathf.PI * diameter;
            float anger = (container.entity.pos - 1) * Mathf.PI * 2 / (container.entity.siblings.Count() - 1);
            return new Vector3(Mathf.Cos(anger), 0, Mathf.Sin(anger)) * diameter / 2;
        }

        internal virtual Vector3 PositionKnownNode(Container container, int[] max, float nodeOffset)
        {
            Entity entity = container.entity;

            // if reached max position of the row, increase Y position. Z just alterates a little.
            for (int index = 0; index < shallowShapes[entity.parent.id].Length; index++)
            {
                if (shallowShapes[entity.parent.id][index] > max[index])
                {
                    if (index < 2)
                    {
                        shallowShapes[entity.parent.id][index + 1]++;
                        shallowShapes[entity.parent.id][index] = 0;
                    }
                }
            }

            // positions in shalloShapes[entity.parent.name][] are: x, Z, y, because we want the shapea to grow upwards
            return new Vector3(
                nodeOffset * (float)shallowShapes[entity.parent.id][0]++ + (float)((shallowShapes[entity.parent.id][1] % 2 == 0) ? 0 : nodeOffset / 2),
                nodeOffset * (float)shallowShapes[entity.parent.id][2] + (float)((shallowShapes[entity.parent.id][1] % 2 == 0) ? 0 : nodeOffset / 2),
                nodeOffset * (float)shallowShapes[entity.parent.id][1]
                );
        }
        internal virtual Container.PositionCorrectionDirection GetPositionCorrectionDirection(int depthOfRelations)
        {
            if (depthOfRelations % 2 == 0)
            {
                return Container.PositionCorrectionDirection.up;
            }
            return Container.PositionCorrectionDirection.farther;
        }
    }
}
