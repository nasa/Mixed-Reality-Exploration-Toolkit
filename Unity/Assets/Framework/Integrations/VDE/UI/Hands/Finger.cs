/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
using UnityEngine;

namespace Assets.VDE.UI.Hands
{
    internal class Finger
    {
        internal string fingerName = "";
        internal GameObject gameObject;
        internal CapsuleCollider collider;
        internal ColliderBehaviour behave;

        public Finger(string name)
        {
            fingerName = name;
        }
    }
}
