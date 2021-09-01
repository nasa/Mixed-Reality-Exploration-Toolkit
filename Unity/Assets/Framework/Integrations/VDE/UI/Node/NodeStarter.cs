/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
using UnityEngine;

namespace Assets.VDE.UI.Node 
{
    internal class NodeStarter : MonoBehaviour
    {
        internal Container owner;
        internal Entity entity;
        internal Communication.Messenger messenger;
        private void Update()
        {
            if (!(owner is null) && !(messenger is null) && !(entity is null))
            {
                owner.messenger.Post(new Communication.Message()
                {
                    LayoutEvent = Layouts.Layouts.LayoutEvent.GotContainer,
                    EventOrigin = Layouts.Layouts.EventOrigin.Node,
                    obj = new object[] { owner.entity, owner, gameObject },
                    to = owner.entity.parent,
                    from = owner.entity
                });
                Destroy(this);
            }
            else
            {
                Debug.LogWarning(owner.name + " startup is still waiting for messenger to show up: " + !(owner.data.messenger is null) + !(owner is null) + !(owner.messenger is null) + !(owner.entity is null) + !(owner.messenger is null) + !(owner.entity is null));
            }
        }
    }
}
