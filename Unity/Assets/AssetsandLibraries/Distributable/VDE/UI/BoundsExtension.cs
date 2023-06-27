/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
using UnityEngine;

namespace Assets.VDE.UI
{
    public static class BoundsExtension
    {
        public static Bounds GrowBounds(this Bounds a, Bounds b)
        {
            Vector3 max = Vector3.Max(a.max, b.max);
            Vector3 min = Vector3.Min(a.min, b.min);

            a = new Bounds((max + min) * 0.5f, max - min);
            return a;
        }
    }
}
