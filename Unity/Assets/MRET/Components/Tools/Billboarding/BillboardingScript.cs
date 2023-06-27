// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;

namespace GOV.NASA.GSFC.XR.MRET.Tools.Billboarding
{
    public class BillboardingScript : MonoBehaviour
    {
        /// <summary>
        /// Object to rotate towards.
        /// </summary>
        [Tooltip("Object to rotate towards. If not specified, the rig head will be used.")]
        public Transform rotateTowards;

        private void Start()
        {
            if (rotateTowards == null)
            {
                rotateTowards = MRET.InputRig.head.transform;
            }
        }

        void Update()
        {
            if ((rotateTowards != null) && ((rotateTowards.position - transform.position) != Vector3.zero))
            {
                transform.rotation = Quaternion.LookRotation((rotateTowards.position - transform.position) * -1, Vector3.up);
            }
        }
    }
}