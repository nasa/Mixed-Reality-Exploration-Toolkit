/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
using Assets.VDE.UI.Group;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using static Assets.VDE.Callbacks;

namespace Assets.VDE.Communication
{
    internal class Messenger : MonoBehaviour
    {
        Log log;
        internal Data data;
        int 
            ticker = 100,
            slowCounterLength = 1, 
            maxReadyForHunters = 1,
            maxMessagesPerFrame = 42, 
            maxWallHeightHistory = 0,
            maxFactoryQueueCount = 1, 
            maxShapeUpdatesPerFrame = 69, 
            maxShapeUpdateRequestsHistory = 0;

        private int[] 
            queueCountHistory, 
            fpsHistory, 
            trgHistory;

        ConcurrentQueue<Message> wall;
        ConcurrentQueue<Shape> shapeUpdateRequests;
        internal ConcurrentDictionary<int, ConcurrentDictionary<string, ConcurrentBag<Callback>>> eventSubscriptions;
        internal readonly ConcurrentDictionary<Entity, IOrderedEnumerable<Entity>> triggers = new ConcurrentDictionary<Entity, IOrderedEnumerable<Entity>> { };
        private float 
            timeTillNextFastCounter, 
            timeTillNextSlowCounter,
            forcedAhead, 
            forceStep = 5;

        Dictionary<string, int> triggerHistory = new Dictionary<string, int> { };
        private int eventSubscriptionsProcessedThisTick;
        private int maxEventSubscriptionsProcessedDuringATick;
        internal int maxLoad;
        private int forceCounter;

        private void Start()
        {
            StartCoroutine(Timer());
        }

        internal void Init(Data data)
        {
            log = new Log("Messenger",this);
            this.data = data;
            wall = new ConcurrentQueue<Message> { };
            shapeUpdateRequests = new ConcurrentQueue<Shape> { };
            fpsHistory = new int[10] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 1 };
            trgHistory = new int[10] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 1 };
            queueCountHistory = new int[10] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 1 };
            eventSubscriptions = new ConcurrentDictionary<int, ConcurrentDictionary<string, ConcurrentBag<Callback>>> { };
        }

        internal void Post(Message message)
        {
            wall.Enqueue(message);
        }

        IEnumerator Timer()
        {
            var wait = new WaitForSecondsRealtime(0.01f);

            while (data.forrestIsRunning)
            {
                int messagesPerFrame = maxMessagesPerFrame;
                int shapeUpdatesPerFrame = maxShapeUpdatesPerFrame;
                eventSubscriptionsProcessedThisTick = 0;

                StartCoroutine(data.UI.nodeFactory.Produce());

                while (messagesPerFrame-- > 1 && wall.TryDequeue(out Message message))
                {
                    HandleMessage(message);
                }

                while (shapeUpdatesPerFrame-- > 1 && shapeUpdateRequests.TryDequeue(out Shape shape))
                {
                    shape.UpdateShape();
                }

                if (triggers.Count() > 0)
                {
                    HappyTrigger();
                }

                if (timeTillNextFastCounter <= Time.realtimeSinceStartup)
                {
                    if (data.UI.fps.currentFps < 60 && maxMessagesPerFrame >= 69)
                    {
                        maxMessagesPerFrame--;
                        maxShapeUpdatesPerFrame--;
                    }
                    else if (data.UI.fps.currentFps > 60 && maxMessagesPerFrame < 123)
                    {
                        maxMessagesPerFrame++;
                        maxShapeUpdatesPerFrame++;
                    }
                    CheckHowManyObjectSoShow(data.UI.fps.currentFps);
                    timeTillNextFastCounter = Time.realtimeSinceStartup + 0.1F;
                }

                if (timeTillNextSlowCounter <= Time.realtimeSinceStartup)
                {                    
                    if (wall.Count > maxWallHeightHistory)
                    {
                        maxWallHeightHistory = wall.Count;
                    }
                    if (shapeUpdateRequests.Count > maxShapeUpdateRequestsHistory)
                    {
                        maxShapeUpdateRequestsHistory = shapeUpdateRequests.Count;
                    }
                    if (maxReadyForHunters < data.UI.nodeFactory.readyForHunters.Count)
                    {
                        maxReadyForHunters = data.UI.nodeFactory.readyForHunters.Count;
                    }
                    if (eventSubscriptionsProcessedThisTick > maxEventSubscriptionsProcessedDuringATick)
                    {
                        maxEventSubscriptionsProcessedDuringATick = eventSubscriptionsProcessedThisTick;
                    }
                    if (ticker == 0)
                    {
                        ticker = 100;
                    }

                    Message teler = new Message()
                    {
                        TelemetryType = Telemetry.Type.progress,
                        telemetry = new Telemetry()
                        {
                            type = Telemetry.Type.progress,
                            progress = new List<Progress>
                            {
                                new Progress()
                                {
                                    name = "framerate",
                                    description = "Framerate",
                                    grade = (data.UI.fps.currentFps < 40) ? Progress.Grade.red : Progress.Grade.green,
                                    ints = new int[] { data.UI.fps.currentFps, 80 }
                                },
                                new Progress()
                                {
                                    name = "messagesPerFrame",
                                    description = "Internal messages processing capacity",
                                    grade = (messagesPerFrame < maxMessagesPerFrame / 2) ? Progress.Grade.red : Progress.Grade.green,
                                    ints = new int[] { messagesPerFrame, maxMessagesPerFrame }
                                },
                                new Progress()
                                {
                                    name = "maxWallHeightHistory",
                                    description = "Internal messages waiting to be processed",
                                    grade = (wall.Count > maxWallHeightHistory / 2) ? Progress.Grade.red : Progress.Grade.green,
                                    ints = new int[] { wall.Count, maxWallHeightHistory }
                                },
                                new Progress()
                                {
                                    name = "shapeUpdatesPerFrame",
                                    description = "Shape update processing capacity",
                                    grade = (shapeUpdatesPerFrame < maxShapeUpdatesPerFrame / 2) ? Progress.Grade.red : Progress.Grade.green,
                                    ints = new int[] { shapeUpdatesPerFrame, maxShapeUpdatesPerFrame }
                                },
                                new Progress()
                                {
                                    name = "maxShapeUpdateRequestsHistory",
                                    description = "Shape updates waiting to be processed",
                                    grade = (shapeUpdateRequests.Count > maxShapeUpdateRequestsHistory / 2) ? Progress.Grade.red : Progress.Grade.green,
                                    ints = new int[] { shapeUpdateRequests.Count, maxShapeUpdateRequestsHistory }
                                },
                                new Progress()
                                {
                                    name = "queue",
                                    description = "GameObject requests in queue",
                                    ints = new int[] { data.UI.nodeFactory.queue.Count, (data.UI.nodeFactory.queue.Count > maxFactoryQueueCount) ? maxFactoryQueueCount = data.UI.nodeFactory.queue.Count : maxFactoryQueueCount }
                                },
                                new Progress()
                                {
                                    name = "hunters",
                                    description = "Components looking for others",
                                    ints = new int[] { data.UI.nodeFactory.hunters.Count, data.UI.nodeFactory.creatures.Count  }
                                },
                                new Progress()
                                {
                                    name = "ready",
                                    description = "Components ready for others",
                                    ints = new int[] { data.UI.nodeFactory.readyForHunters.Count, maxReadyForHunters }
                                },
                                new Progress()
                                {
                                    name = "triggers",
                                    description = "Trunks ready for triggers",
                                    ints = new int[] { triggers.Where(trunk => trunk.Key.readyForTrunkTrigger).Count(), triggers.Count }
                                },
                                new Progress()
                                {
                                    name = "ticks from messenger",
                                    description = "Internal ticker",
                                    ints = new int[] { ticker--, 100 }
                                },
                                new Progress()
                                {
                                    name = "maxEventSubscriptionsProcessedDuringATick",
                                    description = "EventSubscriptions per tick",
                                    ints = new int[] { eventSubscriptionsProcessedThisTick, maxEventSubscriptionsProcessedDuringATick }
                                }
                            }
                        }
                    };

                    int triggerMaxSum = 0, triggersSum = 0, triggerless = triggers.Count();

                    foreach (var item in triggers)
                    {
                        bool maxed = false;
                        if (!triggerHistory.ContainsKey(item.Key.name))
                        {
                            triggerHistory.Add(item.Key.name, item.Value.Count());
                        }
                        else if (triggerHistory[item.Key.name] < item.Value.Count())
                        {
                            triggerHistory[item.Key.name] = item.Value.Count();
                            maxed = true;
                        }
                        if (triggerHistory[item.Key.name] > 0)
                        {
                            teler.telemetry.progress.Add(new Progress
                            {
                                name = "triggersOfTrunk" + item.Key.name,
                                description = "Triggers for " + item.Key.name,
                                grade = maxed ? Progress.Grade.red : Progress.Grade.green,
                                ints = new int[] { item.Value.Count(), triggerHistory[item.Key.name] }
                            });

                            triggerMaxSum += triggerHistory[item.Key.name];
                            triggersSum += item.Value.Count();
                        }
                        else
                        {
                            triggerless--;
                        }
                    }

                    if (triggersSum > 0 && triggerMaxSum > 0)// && triggerless > 0)
                    {
                        Post(new Message()
                        {
                            HUDEvent = UI.HUD.HUD.Event.Progress,
                            number = 1,
                            floats = new List<float> { 1 - ((float)triggersSum / (float)triggerMaxSum), 1F },
                            message = "Positioning entities and groups",
                            from = data.layouts.current.GetGroot()
                        });
                        Post(new Message()
                        {
                            HUDEvent = UI.HUD.HUD.Event.Progress,
                            number = 0,
                            floats = new List<float> { 1 - (data.UI.nodeFactory.queue.Count / ((data.UI.nodeFactory.queue.Count > maxFactoryQueueCount) ? maxFactoryQueueCount = data.UI.nodeFactory.queue.Count : maxFactoryQueueCount)), 1F },
                            message = "Positioning entities and groups",
                            from = data.layouts.current.GetGroot()
                        });
                        Post(new Message()
                        {
                            HUDEvent = UI.HUD.HUD.Event.Progress,
                            number = 2,
                            floats = new List<float> { 1 - (data.UI.nodeFactory.readyForHunters.Count / maxReadyForHunters), 1F },
                            message = "Positioning entities and groups",
                            from = data.layouts.current.GetGroot()
                        });

                        for (int hist = trgHistory.Count() - 1; hist > 0; hist--)
                        {
                            trgHistory[hist] = trgHistory[hist - 1];
                        }
                        trgHistory[0] = triggersSum;
                        
                        if (
                            shapeUpdateRequests.Count == 0 &&
                            forcedAhead < Time.realtimeSinceStartup ||
                            (
                                trgHistory[0] == trgHistory[1] &&
                                trgHistory[0] == trgHistory[2] &&
                                trgHistory[0] == trgHistory[3] &&
                                trgHistory[0] == trgHistory[4]
                            )
                        ){
                            /*
                            if (forceCounter++ > 666)
                            {
                                data.layouts.current.FreezeAll();
                                Post(new Message()
                                {
                                    LayoutEvent = Layouts.Layouts.LayoutEvent.LayoutPopulated,
                                    obj = new object[] { data.layouts.current },
                                    layout = data.layouts.current,
                                    from = data.layouts.current.GetGroot()
                                });
                            }
                            else
                            {
                                foreach (Container container in data.entities.entities.Where(ent => 
                                    ent.Value.type == Entity.Type.Group && 
                                    ent.Value.containers.containers.Where(cont => cont.state == UI.Container.State.triggering).Any()
                                    ).Select(ent => 
                                        ent.Value.containers.GetCurrentGroup()).OrderByDescending(cont => 
                                            cont.entity.distanceFromGroot
                                        )
                                    )
                                {
                                    foreach (Entity item in container.entity.members.Where(gmt => gmt.type == Entity.Type.Group).OrderBy(nmt => nmt.pos))
                                    {
                                        if (item.containers.GetCurrentGroup(out Container gdmp))
                                        {
                                            gdmp.shapes.GetGroupShape().UpdateShape();
                                            gdmp.PositionSelfAmongstSiblings(false, true);
                                            //gdmp.SchedulePostprocess();
                                            StartCoroutine(gdmp.Postprocess());
                                        }
                                    }
                                    container.shapes.GetGroupShape().UpdateShape();
                                    container.PositionSelfAmongstSiblings(false, true);
                                    container.SchedulePostprocess();
                                }

                                forcedAhead = Time.realtimeSinceStartup + forceStep;
                            }*/
                        }
                        
                    }
                    else if (triggersSum == 0 && triggerMaxSum > 0 && shapeUpdateRequests.Count == 0 && wall.Count == 0)
                    {
                        if (data.VDE.hud.ProgressorActive())
                        {
                            Post(new Message()
                            {
                                HUDEvent = UI.HUD.HUD.Event.Progress,
                                number = 1,
                                floats = new List<float> { 1F, 1F },
                                message = "",
                                from = data.layouts.current.GetGroot()
                            });
                        }
                        data.entities.SetShapesCollidersToTriggers(true);
                    }
                    
                    Post(teler);

                    timeTillNextSlowCounter = Time.realtimeSinceStartup + slowCounterLength;
                }
                for (int hist = 1; hist < queueCountHistory.Count(); hist++)
                {
                    queueCountHistory[hist] = queueCountHistory[hist - 1];
                }
                queueCountHistory[0] = wall.Count();
                data.VDE.CheckForrest();

                yield return wait;
            }
        }
        private void CheckHowManyObjectSoShow(int currentFps)
        {
            if (!(data.layouts.current is null) && data.layouts.current.variables.flags["adjustToFPS"])
            {
                float averageFps = 0;

                for (int hist = fpsHistory.Count() - 1; hist > 0; hist--)
                {
                    fpsHistory[hist] = fpsHistory[hist - 1];
                    averageFps += fpsHistory[hist];
                }
                fpsHistory[0] = currentFps;
                averageFps += currentFps;
                averageFps /= fpsHistory.Count();

                if (fpsHistory.Length == fpsHistory.Count())
                {
                    Post(new Message()
                    {
                        LayoutEvent = Layouts.Layouts.LayoutEvent.AdjustForFPS,
                        obj = new object[] { (int)averageFps },
                        from = data.layouts.current.GetGroot()
                    });
                }
            }
        }
        internal int CheckLoad()
        {
            int trig = 0;
            foreach (var trigger in triggers.Values)
            {
                trig += trigger.Count();
            }
            int load = wall.Count + trig;
            if (load > maxLoad)
            {
                maxLoad = load;
            }
            return load;
        }
        void HappyTrigger()
        {
            foreach (KeyValuePair<Entity,IOrderedEnumerable<Entity>> trunk in triggers.Where(trunk => trunk.Key.readyForTrunkTrigger && !trunk.Key.triggerIsPulled))
            {
                if (!(trunk.Value is null) && trunk.Value.Count() > 0)
                {
                    Entity toBeTriggered = trunk.Value.FirstOrDefault();
                    if (!(toBeTriggered is null) && toBeTriggered.containers.GetCurrentGroup(out UI.Group.Container container))
                    {
                        trunk.Key.triggerIsPulled = true;
                        if (toBeTriggered.trigger is null)
                        {
                            StartCoroutine(container.Trigger());
                        } 
                        else
                        {
                            StartCoroutine(toBeTriggered.trigger());
                            toBeTriggered.trigger = null;
                        }
                    }
                }
            }
        }
        void HandleMessage(Message message)
        {
            try
            {
                if (message.LinkEvent != UI.Link.Event.None)
                {
                    data.links.HandleEvent(message);
                }
                else if (message.TelemetryType != 0)
                {
                    switch (data.VDE.connectionType)
                    {
                        case VDE.ConnectionType.SIGNALR:
                            _ = Task.Run(() => data.VDE.signalRConnection.SendTelemetry(message.telemetry));
                            break;
                        case VDE.ConnectionType.GMSEC:
#if MRET_2021_OR_LATER
                            _ = Task.Run(() => data.VDE.gmsecConnection.EnqueueTelemetry(message.telemetry));
#endif
                            break;
                        default:
                            break;
                    }
                }
                else if (message.LayoutEvent != Layouts.Layouts.LayoutEvent.NotSet)
                {
                    HandleEvent(message, message.LayoutEvent.ToString());
                }
                else if (message.EntityEvent != Entities.Event.NotSet)
                {
                    HandleEvent(message, message.EntityEvent.ToString());
                }
                else if (message.LogingEvent != Log.Event.NotSet)
                {
                    HandleLogging(message);
                }
                else if (message.HUDEvent != UI.HUD.HUD.Event.NotSet)
                {
                    HandleHUD(message);
                }
            }
            catch (Exception exe)
            {
                log.Entry("exceptional processing of a message: " + exe.Message + "\n" + exe.StackTrace);
            }
        }
        private void HandleHUD(Message message)
        {
            switch (message.HUDEvent)
            {
                case UI.HUD.HUD.Event.NotSet:
                    break;
                case UI.HUD.HUD.Event.Notification:
                    break;
                case UI.HUD.HUD.Event.Progress:
                    {
                        if (!data.VDE.hud.progress[message.number].IsActive())
                        {
                            data.VDE.hud.progress[message.number].gameObject.SetActive(true);
                        }
                        data.VDE.hud.progress[message.number].fillAmount = message.floats[0];
                        data.VDE.hud.progress[message.number].color = new Color(data.VDE.hud.progress[message.number].color.r, data.VDE.hud.progress[message.number].color.g, data.VDE.hud.progress[message.number].color.b, message.floats[1]);

                        data.VDE.hud.SetBoardTextTo(message.message);

                        if (data.VDE.hud.progress.Where(img => img.Value.fillAmount == 1).Count() == data.VDE.hud.progress.Count)
                        {
                            data.VDE.hud.StartCoroutine(data.VDE.hud.FadeAwayAndDefault());
                        }
                        break;
                    }
                case UI.HUD.HUD.Event.SetText:
                    data.VDE.hud.SetBoardTextTo(message.message);
                    break;
                default:
                    break;
            }
        }
        private void HandleLogging(Message message)
        {
            switch (message.LogingEvent)
            {
                case Log.Event.FromServer:
                    break;
                case Log.Event.ToServer:
                    SendLogEntryToServer(message);
                    break;
                case Log.Event.Lethal:
                    break;
                case Log.Event.HUD:
                    data.VDE.hud.AddLineToBoard(message.message);
                    log.Entry("sent to HUD: " + message.message);//, Log.Event.ToServer);
                    break;
                default:
                    break;
            }
        }
        private void SendLogEntryToServer(Message message)
        {
            if (data.VDE.standalone)
            {
                log.Entry(message.message);
            }
            else
            {
                _ = Task.Run(() => data.VDE.signalRConnection.SendTelemetry(new Telemetry
                {
                    type = Telemetry.Type.log,
                    message = message.message,
                }));
            }
        }
        void HandleEvent(Message message, string eventName)
        {
            if (!(message.from is null))
            {
                if (!(message.to is null))
                {
                    StartCoroutine(message.to.Inbox(message));
                }
                else if (
                    eventSubscriptions.ContainsKey(message.from.id) &&
                    eventSubscriptions[message.from.id].ContainsKey(eventName) &&
                    eventSubscriptions[message.from.id][eventName].Count() > 0
                    )
                {
                    foreach (Callback callback in eventSubscriptions[message.from.id][eventName])
                    {
                        StartCoroutine(callback(message.obj));
                        eventSubscriptionsProcessedThisTick++;
                    }
                }
            }
            else if (message.LayoutEvent == Layouts.Layouts.LayoutEvent.InitializeAll)
            {
                data.layouts.InitializeLayouts();
            }
        }
        internal void RegisterTrunk(Entity entity)
        {
            if (!triggers.ContainsKey(entity))
            {
                triggers.TryAdd(
                    entity, 
                    data.entities.entities.
                        Where(ent => ent.Value.tier2ancestorID == entity.id || ent.Value.id == entity.id).Select(ent => ent.Value).
                        Where(trigger => trigger.readyForTrigger).
                        OrderByDescending(trigger => trigger.distanceFromGroot)
                    );
                log.Entry(entity.name + " registered as a trunk.", Log.Event.ToServer);
            }
        }
        internal void RequestShapeUpdate(Shape shape)
        {
            shapeUpdateRequests.Enqueue(shape);
        }
        internal void SubscribeToEvent(Callback callback, string layoutEvent, int targetEntityID)
        {
            if (!(callback is null))
            {
                if (!eventSubscriptions.ContainsKey(targetEntityID))
                {
                    eventSubscriptions.TryAdd(targetEntityID, new ConcurrentDictionary<string, ConcurrentBag<Callback>> { });
                    eventSubscriptions[targetEntityID].TryAdd(layoutEvent, new ConcurrentBag<Callback> { });
                }
                else if (!eventSubscriptions[targetEntityID].ContainsKey(layoutEvent))
                {
                    eventSubscriptions[targetEntityID].TryAdd(layoutEvent, new ConcurrentBag<Callback> { });
                }

                if (!eventSubscriptions[targetEntityID][layoutEvent].Contains(callback))
                {
                    eventSubscriptions[targetEntityID][layoutEvent].Add(callback);
                }
            }
        }
        internal void UnsubscribeFromEvent(Callback callback, string layoutEvent, int entityId)
        {
            if (!(callback is null))
            {
                if (
                    eventSubscriptions.ContainsKey(entityId) && 
                    eventSubscriptions[entityId].ContainsKey(layoutEvent) && 
                    eventSubscriptions[entityId][layoutEvent].Contains(callback))
                {
                    eventSubscriptions[entityId][layoutEvent].TryTake(out _);
                }
            }
        }
    }
}
