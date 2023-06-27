/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
using TMPro;
using UnityEngine;

namespace Assets.VDE.UI.Hands
{
    internal class Notification : MonoBehaviour
    {
        internal TextMeshPro text;
        float 
            timeToLive, 
            lifeTime = 3;
        GameObject notification;
        internal Font font;

        internal void SetText(string setNotificationTextTo)
        {
            if (!(text is null))
            {
                text.text = setNotificationTextTo;
            }
            timeToLive = lifeTime + Time.realtimeSinceStartup;
        }
        private void Update()
        {
            if (timeToLive < Time.realtimeSinceStartup)
            {
                Destroy(this);
            }
        }
    }
}
