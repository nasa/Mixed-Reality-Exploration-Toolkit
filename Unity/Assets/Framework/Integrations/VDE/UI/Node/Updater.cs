/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
using System.Linq;
using UnityEngine;

namespace Assets.VDE.UI.Node
{
    class Updater : Assets.VDE.UI.Updater
    {
        bool workingHard = false;
        float magPositionInPreviousFrame, nextUpdate;
        internal Assets.VDE.UI.Container owner;

        private void Update()
        {
            if (
                !workingHard && 
                transform.hasChanged && 
                owner.hasLinksToUpdate && 
                (
                    owner.isGrabbed ||
                    nextUpdate < Time.realtimeSinceStartup
                ) && 
                magPositionInPreviousFrame != transform.position.magnitude
                )
            {
                magPositionInPreviousFrame = transform.position.magnitude;
                UpdateLinks();
                transform.hasChanged = false;
                nextUpdate = Time.realtimeSinceStartup + (owner.data.VDE.rando.Next(3,10) / 10);
            }
        }
        internal void UpdateLinks()
        {
            workingHard = true;
            foreach (Link link in owner.linksToThisContainer.Where(link => link.ready && link.gameObject.activeSelf))
            {
                link.UpdatePosition(owner);
            }
            foreach (Link link in owner.linksFromThisContainer.Where(link => link.ready && link.gameObject.activeSelf))
            {
                link.UpdatePosition(owner);
            }
            nextUpdate = Time.realtimeSinceStartup + (owner.data.VDE.rando.Next(3, 10) / 100);
            workingHard = false;
        }
        internal System.Collections.IEnumerator UpdateLinksAsync()
        {
            workingHard = true;
            foreach (Link link in owner.linksToThisContainer.Where(link => link.ready && link.enabled))
            {
                if (Time.deltaTime > owner.data.UI.maxTimeForUpdatePerFrame)
                {
                    yield return owner.data.UI.Sleep(owner.data.UI.timeToWaitInUpdatePerFrame);
                }
                link.UpdatePosition(owner);
            }
            foreach (Link link in owner.linksFromThisContainer.Where(link => link.ready && link.enabled))
            {
                if (Time.deltaTime > owner.data.UI.maxTimeForUpdatePerFrame)
                {
                    yield return owner.data.UI.Sleep(owner.data.UI.timeToWaitInUpdatePerFrame);
                }
                link.UpdatePosition(owner);
            }
            nextUpdate = Time.realtimeSinceStartup + (owner.data.VDE.rando.Next(3, 10) / 100);
            workingHard = false;
            yield return true;
        }
    }
}
