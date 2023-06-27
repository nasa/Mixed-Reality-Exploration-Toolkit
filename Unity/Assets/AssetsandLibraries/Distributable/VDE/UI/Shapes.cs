/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.VDE.UI
{
    /// <summary>
    /// contains shapes used by a container of an entity. 
    /// does NOT contain shapes of other entities. 
    /// does NOT contain owner entity's shapes in other layouts.
    /// </summary>
    class Shapes
    {
        Container container;
        internal bool ready { private set; get; } = false;
        internal IEnumerable<Shape> shapesNotReady;
        internal List<Shape> shapes = new List<Shape> { };

        internal Shapes(Container owner)
        {
            container = owner;
            shapesNotReady = shapes.Where(shape => !shape.ready);
        }
        internal bool IsReady()
        {
            if (shapes.Count() > 0 && shapesNotReady.Count() == 0)
            {
                ready = true;
                return true;
            }
            return false;
        }
        internal void Add(Shape shape)
        {
            if (!shapes.Contains(shape))
            {
                shapes.Add(shape);
            }
        }
        internal Shape GetShape(Entity.Type type = Entity.Type.Other)
        {
            try
            {
                if (type == Entity.Type.Other)
                {
                    type = container.type;
                }
                return shapes.FirstOrDefault(shape => shape.type == type);
            }
            catch (Exception)
            {
                return null;
            }
        }
        internal Group.Shape GetGroupShape()
        {
            return GetShape(Entity.Type.Group) as Group.Shape;
        }
        internal Shape GetNodeShape()
        {
            return GetShape(Entity.Type.Node);
        }

        internal void SetMaterialsTo(Material setMaterialTo)
        {
            foreach (Shape shape in shapes)
            {
                if(shape.gameObject.TryGetComponent(out MeshRenderer renderer))
                {
                    renderer.material = setMaterialTo;
                }
            }
        }
    }
}
