// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace GOV.NASA.GSFC.XR.Utilities.Asynchronous
{
    public static class MethodDelayer
    {
        public async static void DelayMethodByTimeAsync(System.Action action, float timeToDelay)
        {

            float t = 0f;

            while (t < timeToDelay)
            {
                t += UnityEngine.Time.deltaTime;
                await Task.Yield();
            }
            action.Invoke();

        }

        /// <summary>
        /// Delays method while the predicate is FALSE. Finally runs it when predicate is TRUE.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="predicate"></param>
        public async static void DelayMethodByPredicateAsync(System.Action action, Func<bool> predicate)
        {

            for (int i = 0; i < 10; i++)
            {
                await Task.Yield();
            }

            while (!predicate())
            {
                await Task.Yield();
            }

            for (int i = 0; i < 10; i++)
            {
                await Task.Yield();
            }

            action.Invoke();

        }

    }
}