﻿// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GOV.NASA.GSFC.XR.MRET.UI.MiniMap
{
    public class Minimap : MonoBehaviour
    {
        public Transform player;
        [Tooltip("Should the minimap move with the minimap?")]
        public bool moveWithUser = true;
        [Tooltip("Should the minimap rotate with the minimap?")]
        public bool rotateWithUser = false;

        private void LateUpdate()
        {
            if (moveWithUser)
            {
                Vector3 newPosition = player.position;
                newPosition.y = transform.position.y;
                transform.position = newPosition;
            }

            if (rotateWithUser)
            {
                transform.rotation = Quaternion.Euler(90f, player.eulerAngles.y, 0f);
            }
        }
    }
}