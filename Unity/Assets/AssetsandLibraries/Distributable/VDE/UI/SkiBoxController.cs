/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
using System.Linq;
using UnityEngine;
//using UnityEngine.Rendering.HighDefinition;

namespace Assets.VDE.UI
{
    public class SkiBoxController : MonoBehaviour
    {
        /*
        public UnityEngine.Rendering.CubemapParameter[] skiboxes;

        int skiBox = 0;
        HDRISky HDRSky;
        UnityEngine.Rendering.Volume volume;
        UnityEngine.Rendering.CubemapParameter ski;
        public UnityEngine.Rendering.VolumeProfile profile;

        private void Start()
        {
            volume = gameObject.GetComponent<UnityEngine.Rendering.Volume>();
            foreach (var item in volume.sharedProfile.components)
            {
                if (item.name == "HDRISky")
                {
                    ski = (item as HDRISky).hdriSky;
                    HDRSky = item as HDRISky;
                }
            }
        }

        internal void SetToNextSkibox()
        {
            int setTo = 0;
            if (skiboxes.Count() > skiBox + 1)
            {
                setTo = skiBox + 1;
            }
            skiBox = setTo;

            if (!(HDRSky is null))
            {
                ski.Override(skiboxes[setTo].value);
            }
        }
        */
    }
}
