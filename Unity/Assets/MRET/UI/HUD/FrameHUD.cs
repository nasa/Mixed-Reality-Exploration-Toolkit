// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;

namespace GOV.NASA.GSFC.XR.MRET.UI.HUD
{
    public class FrameHUD : MRETBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(FrameHUD);

        public Transform trackHeadset;
        [Tooltip("The maxmimum allowed distance between the desired offset of the frame " +
            "and the actual offset of the frame before the frame is repositioned to the " +
            "desired offset in front of the tracked headset.")]
        public float offsetThreshold = 0.2f;
        [Tooltip("The desired offset from the tracket headset.")]
        public Vector3 offset = new Vector3(0, 0, 0.5f);

        private bool zeroing = false;

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            base.MRETStart();

            // Make sure we have a head tracker assigned
            if (trackHeadset == null)
            {
                trackHeadset = MRET.InputRig.head.transform;
            }

            // Initialize the starting position
            transform.SetPositionAndRotation(
                trackHeadset.position + trackHeadset.TransformDirection(offset),
                Quaternion.LookRotation(trackHeadset.forward, Vector3.up));
        }

        void LateUpdate()
        {
            ZeroHUD();
        }

        public void ZeroHUD()
        {
            if (trackHeadset == null)
            {
                LogWarning("Track Headset is not assigned", nameof(ZeroHUD));
                return;
            }

            // Determine how far away from the desired offset we are by examining the
            // current world position and the desired world position
            Vector3 desiredPosition = trackHeadset.position + trackHeadset.TransformDirection(offset);
            Vector3 heading = desiredPosition - transform.position;

            // Determine if we are exceeding the distance threshold. Avoid using magnitude because it is CPU expensive.
            zeroing = (heading.sqrMagnitude > (offsetThreshold * offsetThreshold)) || zeroing;
            if (zeroing)
            {
                // How fast we are going to move toward zero. If we are more than 2x the offset,
                // we want to go fast, otherwise just a smooth transition
                float step = (heading.sqrMagnitude > (offsetThreshold * offsetThreshold) * 2f) ? 100f : 1f;

                // Close the angle and position
                transform.SetPositionAndRotation(
                    Vector3.MoveTowards(transform.position, desiredPosition, step * UnityEngine.Time.deltaTime),
                    Quaternion.LookRotation(trackHeadset.forward, Vector3.up));

                // Check if we are "close enough" to zero (10% of threshold)
                zeroing = (heading.sqrMagnitude > (offsetThreshold * offsetThreshold) * 0.1f);
            }
        }
    }
}