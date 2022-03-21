/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;

namespace Assets.VDE
{
    class Callbacks
    {   
        ConcurrentDictionary<string, ConcurrentQueue<Callback>> callbacks = new ConcurrentDictionary<string, ConcurrentQueue<Callback>> { };
        /// <summary>
        /// Use this to request another entity, shape, whatever to call back in case of gameObject, collision, rigidbody, etc. events.
        /// usually anObject[0] is an entity, 1 is a shape, container, link, etc of that entity and 2 is a gameObject/rigidBody/joint/covfefetable, thats being sent along.
        /// </summary>
        /// <param name="anObject"></param>
        internal delegate IEnumerator Callback(object[] anObject);
        /// <summary>
        /// request this shape to call back, once a "task" is done
        /// </summary>
        /// <param name="task"></param>
        /// <param name="callback"></param>
        internal void RequestCallback(string task, Callback callback)
        {
            if (!callbacks.ContainsKey(task))
            {
                callbacks.TryAdd(task, new ConcurrentQueue<Callback> { });
            }
            if (callbacks.ContainsKey(task) && !callbacks[task].Contains(callback))
            {
                callbacks[task].Enqueue(callback);
            }
        }
        /// <summary>
        /// initiate a Shape to call back to those, that have registered their callbacks for "task"
        /// </summary>
        /// <param name="task"></param>
        internal void CallBack(string task, object[] result)
        {
            if (callbacks.ContainsKey(task))
            {
                while (callbacks[task].TryDequeue(out Callback callback))
                {
                    callback(result);
                }
            }
        }
    }
}
