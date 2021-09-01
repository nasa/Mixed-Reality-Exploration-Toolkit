/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.VDE
{

    public class Demo
    {
        public string name;
        public float lastActive = 0;
        public List<Stop> stops = new List<Stop> { };
        public Stop targetStop;
        public bool active, die, loop = true;
        public Demo() { }

        [Serializable]
        class Message : System.Exception
        {
            public Status status;
            public enum Status
            {
                OK,
                NotActive,
                Started,
                ItsOver
            }

            public Message(string message) : base(message)
            {
            }

            public Message(string message, Exception innerException) : base(message, innerException)
            {
            }

            protected Message(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext)
            {
                throw new NotImplementedException();
            }

            public Message()
            {
            }
        }
        public class Stop
        {
            public Vector3 position;
            public Quaternion rotation;
            public float duration, reachedAt;
            public Transition transitionToNextStop;
            public Stop() { }
        }
        public class Transition
        {
            public float maxSpeed = 2F, angularVelocity = 0F;
            public Vector3 currentVelocity;
            public TransitionType transitionType;
            public enum TransitionType
            {
                lerp,
                smooth
            }
            public Transition() { }
        }

        internal void Move(Transform transform, float currentBooster, float defaultBooster)
        {
            if (
                (
                    targetStop.reachedAt == 0 &&
                    Vector3.Distance(targetStop.position, transform.position) < 0.01F
                ) &&
                Quaternion.Angle(targetStop.rotation, transform.rotation) < 0.01F
                )
            {
                transform.position = targetStop.position;
                transform.rotation = targetStop.rotation;
                targetStop.reachedAt = Time.realtimeSinceStartup;
            }
            else
            {
                transform.SetPositionAndRotation(
                    Vector3.SmoothDamp(
                        transform.position,
                        Vector3.MoveTowards(
                            transform.position,
                            targetStop.position,
                            targetStop.transitionToNextStop.maxSpeed
                            ),
                        ref targetStop.transitionToNextStop.currentVelocity,
                        currentBooster * Time.deltaTime
                        ),
                    Quaternion.SlerpUnclamped(
                        transform.rotation,
                        // here we use defaultBooster, because currentBooster is adjusted when camera is inside a datashape - but we dont want to slow down camera speed in a demo.
                        Quaternion.RotateTowards(
                            transform.rotation,
                            targetStop.rotation,
                            targetStop.transitionToNextStop.maxSpeed / (Time.deltaTime * defaultBooster * 4)
                            ),
                        defaultBooster * Time.deltaTime
                    )
                );
            }
        }

        public IEnumerator Play()
        {
            if (die)
            {
                throw new Message()
                {
                    status = Message.Status.ItsOver
                };
            }
            while (active)
            {
                lastActive = Time.realtimeSinceStartup;
                if (targetStop == null)
                {
                    targetStop = stops.ElementAt(0);
                }
                else if (targetStop.reachedAt > 0 && targetStop.reachedAt + targetStop.duration <= Time.realtimeSinceStartup)
                {
                    targetStop.reachedAt = 0;
                    if (stops.Count > (stops.IndexOf(targetStop) + 1))
                    {
                        targetStop = stops.ElementAt(stops.IndexOf(targetStop) + 1);
                    }
                    else if (loop)
                    {
                        targetStop = stops.ElementAt(0);
                    }
                    else
                    {
                        active = false;
                        yield return null;
                    }
                }

                if (targetStop.reachedAt > 0)
                {
                    yield return new WaitForSeconds(targetStop.duration - (Time.realtimeSinceStartup - targetStop.reachedAt));
                    yield return null;
                }
                else if (active)
                {
                    yield return new WaitForSeconds(0.5F);
                    yield return null;
                }
            }
        }
        public void SaveStop(Camera camera)
        {
            stops.Add(new Stop()
            {
                position = camera.transform.position,
                rotation = camera.transform.rotation,
                duration = 5,
                transitionToNextStop = new Transition()
                {
                    maxSpeed = 4,
                    transitionType = Transition.TransitionType.lerp
                }
            });
        }
        public static void Start(Camera camera, Demo demo, List<Demo> demos)
        {
            demos.ForEach(ademo => ademo.active = false);
            demo.active = true;
            camera.transform.parent.GetComponent<UI.CameraController>().demo = demo;
        }
    }
}

