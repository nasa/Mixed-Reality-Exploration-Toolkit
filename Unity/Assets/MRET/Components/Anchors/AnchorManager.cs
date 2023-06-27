// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WindowsMR;
using UnityEngine.XR.ARSubsystems;

namespace GOV.NASA.GSFC.XR.MRET.Anchors
{
    //Spatial Anchors are used to tie a physical position with the position of an AR object
    //This is used to lock an AR scene in place so it matches a room when the app is reopened later, as well as marking which objects should be moved when the user "teleports"
    public class AnchorManager : MRETManager<AnchorManager>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(AnchorManager);

        public List<GameObject> anchoredObjects = new List<GameObject>();

        public XRAnchorStore MRTKAnchorManager;

        protected override async void MRETStart()
        {
            base.MRETStart();
            Debug.Log("AnchorStart");
            List<XRAnchorSubsystemDescriptor> anchorSubsystems = new List<XRAnchorSubsystemDescriptor>();
            SubsystemManager.GetSubsystemDescriptors<XRAnchorSubsystemDescriptor>(anchorSubsystems);
            if (anchorSubsystems[0] != null)
            {
                MRTKAnchorManager = await XRAnchorSubsystemExtensions.TryGetAnchorStoreAsync(anchorSubsystems[0].Create());
            }
            Debug.Log("AnchorStartComplete");
        }

        public TrackableId AttachAnchor(GameObject targetToAnchor)
        {
            anchoredObjects.Add(targetToAnchor);
            return MRTKAnchorManager.LoadAnchor(targetToAnchor.name);
        }

        public void removeAnchor(GameObject objectToRemove)
        {
            anchoredObjects.Remove(objectToRemove);
            MRTKAnchorManager.UnpersistAnchor(objectToRemove.name);
        }

        protected override void MRETOnDestroy()
        {
            if (MRTKAnchorManager != null)
            {
                MRTKAnchorManager.Dispose();
            }
        }
    }
}