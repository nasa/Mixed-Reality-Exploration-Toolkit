// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GOV.NASA.GSFC.XR.MRET.Tools.Colliders
{
    public class InstrumentGroup : MonoBehaviour
    {

        [HideInInspector]
        public List<GameObject> childrenWithColliders = new List<GameObject>();
        public List<GameObject> childrenWithTriggers = new List<GameObject>();
        private Collider[] compoundColliders;

        [HideInInspector]
        public List<Renderer> highlightList = new List<Renderer>();

        void Awake()
        {
            FindColliders();
        }

        void FindColliders()
        {
            compoundColliders = GetComponentsInChildren<Collider>();
            foreach (Collider compoundCollider in compoundColliders)
            {
                if (!compoundCollider.isTrigger)
                {
                    childrenWithColliders.Add(compoundCollider.gameObject);
                }
                else
                {
                    childrenWithTriggers.Add(compoundCollider.gameObject);
                }
            }
        }
    }
}