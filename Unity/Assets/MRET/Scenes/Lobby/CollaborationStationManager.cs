// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using UnityEngine;

namespace GOV.NASA.GSFC.XR.MRET.Scenes.Lobby
{
    public class CollaborationStationManager : MonoBehaviour
    {
        public GameObject sharePanel, joinPanel;

        IEnumerator Start()
        {
            yield return new WaitForSeconds(1);
            sharePanel.SetActive(true);
            joinPanel.SetActive(true);
        }
    }
}