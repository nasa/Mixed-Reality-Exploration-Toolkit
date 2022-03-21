/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Assets.VDE.UI.Node
{
    internal class Shape : Assets.VDE.UI.Shape
    {
        internal new void Init(Entity entity, Group.Container group)
        {
            base.Init(entity, group);
            nodeSize = layout.variables.vectors["nodeSize"];
            padding = layout.variables.vectors["nodeOffset"];

            GetComponent<Collider>().enabled = true;

            MeshRenderer render = GetComponent<MeshRenderer>();

            SetColor();
            render.enabled = true;

            SetPositionAndScale(Vector3.zero, nodeSize);
            ready = true;
        }
        internal void SetColor()
        {
            if (entity.c is null)
            {
                SetColor(layout.variables.colours["nodeColour"] * new Color(1, 1, 1, entity.a));
            }
            else
            {
                SetColor(entity.c);
            }
        }
        internal override void GotFocus()
        {
            if (data.entities.NodeInFocus != this)
            {
                data.entities.NodeInFocus = this;
                entity.enlightened = true;
                data.VDE.controller.SetNotificationText(entity.name);
            }
        }
        internal void Select()
        {
            GotFocus();
            StartCoroutine(data.links.HighlightLinksFor(this));
        }

        internal override void LostFocus()
        {
            entity.enlightened = false;
            StartCoroutine(data.links.UnHighlightLinksFor(this));
        }
        internal void UnSelect()
        {
            LostFocus();
        }
        internal override void BePresentable()
        {
            data.links.SetColliderStateFor(this, true);
            container.SetLabelState(true);
            cameraIsClose = true;
        }
        internal override void Relax()
        {
            data.links.SetColliderStateFor(this, false);
            container.SetLabelState(false);
            cameraIsClose = false;
        }
    }
}
