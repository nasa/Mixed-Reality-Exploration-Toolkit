/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
using UnityEngine;

namespace Assets.VDE.UI.Group
{
    internal class KeepDirection : MonoBehaviour
    {
        internal GameObject 
            keepStaringAt,
            getSizeFrom,
            stearThis;

        void Update()
        {
            stearThis.transform.parent.LookAt(keepStaringAt.transform);
            stearThis.transform.localPosition = new Vector3(0 - (getSizeFrom.transform.localScale.x / 2), 0, 0);
        }
    }
}
