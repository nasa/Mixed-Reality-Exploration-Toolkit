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
using UnityEngine.Assertions.Must;

namespace Assets.VDE.UI
{
    /// <summary>
    /// gameobjects, nodes, (datas)hapes, whatnot that represent an entity in different layouts
    /// </summary>
    internal class Containers
    {
        Log log;
        Entity entity;
        internal bool ready;
        internal IEnumerable<Container> containersNotReady;
        internal List<Container> containers = new List<Container> { };
        private bool useDirector;

        internal Containers(Entity owner)
        {
            entity = owner;
            log = new Log(owner.name + " container");
            containersNotReady = containers.Where(container => !container.ready || !container.joints.IsReady() || !container.shapes.IsReady());
            owner.data.messenger.SubscribeToEvent(SetStateOfShapesInLayoutCallback, Layouts.Layouts.LayoutEvent.ActivateLayout.ToString(), owner.id);
        }
        /// <summary>
        /// sets the state of all containers that are NOT part of the Layout to disabled; 
        /// then sets the state of all containers that ARE part of the Layout to State;
        /// </summary>
        /// <param name="layout"></param>
        /// <param name="state"></param>
        internal void SetStateOfShapesInLayout(Layout layout, bool state)
        {
            foreach (Container container in containers.Where(cont => cont.layout == layout))
            {
                container.SetState(state);
            }
        }
        private IEnumerator SetStateOfShapesInLayoutCallback(object[] anObject)
        {
            if (anObject.Length == 2 && anObject[0].GetType().Name == "Layout")
            {
                SetStateOfShapesInLayout(anObject[0] as Layout, (bool) anObject[1]);
            }
            yield return true;
        }
        internal bool IsReady()
        {
            if (containers.Count() > 0 && !containersNotReady.Any())
                {
                ready = true;
                return true;
            }
            return false;
        }

        internal Shape GetShape(Layout layout, Entity.Type type = Entity.Type.Other)
        {
            try
            {
                if (type == Entity.Type.Other)
                {
                    type = entity.type;
                }
                return containers.FirstOrDefault(cont => cont.layout == layout && cont.type == type).shapes.GetShape(type);
            }
            catch (Exception)
            {
                return null;
            }
        }

        internal void SetMaterialsTo(Material setMaterialTo)
        {
            foreach (Container container in containers)
            {
                container.SetMaterialsTo(setMaterialTo);
            }
        }

        internal bool GetGroup(Layout layout, out Group.Container group)
        {
            group = containers.FirstOrDefault(cont => cont.layout == layout && cont.type == Entity.Type.Group) as Group.Container;
            if (group is null)
            {
                return false;
            }
            return true;
        }
        internal bool GetContainer(Layout layout, out Container container)
        {
            container = containers.FirstOrDefault(cont => cont.layout == layout) as Container;
            if (!(container is null))
            {
                return true;
            }
            return false;
        }
        internal bool GetContainer(out Container container)
        {
            if (GetContainer(entity.data.layouts.current, out container))
            {
                return true;
            }
            return false;
        }

        internal Container GetContainer()
        {
            if (GetContainer(out Container container))
            {
                return container;
            }
            return null;
        }
        internal Group.Container GetGroup(Layout layout)
        {
            return containers.FirstOrDefault(cont => cont.layout == layout && cont.type == Entity.Type.Group) as Group.Container;
        }
        internal Group.Container GetGroup(Layout layout, Callbacks.Callback callback)
        {
            Group.Container group = containers.FirstOrDefault(cont => cont.layout == layout && cont.type == Entity.Type.Group) as Group.Container;
            if (group is null)
            {
                entity.data.messenger.SubscribeToEvent(callback, Layouts.Layouts.LayoutEvent.GotContainer.ToString(), entity.id);
            }
            return group;
        }
        internal Node.Container GetNode(Layout layout)
        {
            return containers.FirstOrDefault(cont => cont.layout == layout && cont.type == Entity.Type.Node) as Node.Container;
        }
        internal Group.Shape GetGroupShape(Layout layout)
        {
            return (Group.Shape) GetShape(layout, Entity.Type.Group);
        }
        internal Shape GetNodeShape(Layout layout)
        {
            return GetShape(layout, Entity.Type.Node);
        }
        internal Group.Container GetCurrentGroup()
        {
            return GetGroup(entity.data.layouts.current);
        }
        internal bool GetCurrentGroup(out Group.Container group)
        {
            if (GetGroup(entity.data.layouts.current, out group))
            {
                return true;
            }
            return false;
        }
        internal Node.Container GetCurrentNode()
        {
            return GetNode(entity.data.layouts.current);
        }
        internal GameObject GetCurrentVisibleObject()
        {
            return GetCurrentShape().gameObject;
        }
        internal Shape GetCurrentShape()
        {
            return GetShape(entity.data.layouts.current, entity.type);
        }
        internal bool GetCurrentGroupShape(out Group.Shape shape)
        {
            shape = GetCurrentGroupShape();
            if (!(shape is null))
            {
                return true;
            }
            return false;
        }
        internal Group.Shape GetCurrentGroupShape()
        {
            return GetGroupShape(entity.data.layouts.current);
        }

        internal bool GetCurrentNodeShape(out Node.Shape shape)
        {
            shape = GetCurrentNodeShape() as Node.Shape;
            if (!(shape is null))
            {
                return true;
            }
            return false;
        }

        internal Shape GetCurrentNodeShape()
        {
            return GetNodeShape(entity.data.layouts.current);
        }
        internal void AddGroup(Group.Container container)
        {
            AddContainer(container);
        }
        internal void AddNode(Node.Container container)
        {
            AddContainer(container);
        }
        internal void AddContainer(Container container)
        {
            if (!containers.Contains(container))
            {
                containers.Add(container);
                if (useDirector)
                {
                    // 20200709 AddDirector(container);
                }
            }
        }

        internal void AddDirector(GameObject target, Container container = null)
        {
            if (container is null)
            {
                container = GetCurrentGroup();
            }
            if (!(container is null) && !(container.gameObject is null) && !container.gameObject.TryGetComponent(out Group.KeepDirection _))
            {
                Group.KeepDirection director = container.gameObject.AddComponent<Group.KeepDirection>();
                director.keepStaringAt = target;
                director.stearThis = container.gameObject;
                director.getSizeFrom = container.shapes.GetGroupShape().gameObject;
            }
        }

        internal void UseDirector()
        {
            useDirector = true;
            if (containers.Count > 0)
            {
                // 20200709 AddDirector(GetCurrentGroup());
            }
        }
    }
}
