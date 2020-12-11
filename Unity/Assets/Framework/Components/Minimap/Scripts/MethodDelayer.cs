using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public static class MethodDelayer
{
    public async static void DelayMethodByTimeAsync(Action action, float timeToDelay)
    {

        float t = 0f;

        while (t < timeToDelay)
        {
            t += Time.deltaTime;
            await Task.Yield();
        }
        action.Invoke();

    }

    /// <summary>
    /// Delays method while the predicate is FALSE. Finally runs it when predicate is TRUE.
    /// </summary>
    /// <param name="action"></param>
    /// <param name="predicate"></param>
    public async static void DelayMethodByPredicateAsync(Action action, Func<bool> predicate)
    {

        for(int i = 0; i < 10; i++)
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
