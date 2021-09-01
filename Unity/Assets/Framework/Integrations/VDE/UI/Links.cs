/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
using Assets.VDE.Communication;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.VDE.UI
{
    class Links
    {
        Log log;
        private Data data;
        GameObject container;
        int expectedEntities = 0;
        internal ConcurrentBag<Link> links = new ConcurrentBag<Link> { };
        private bool toglingLinks, positioningLinks, allowLinksBetweenAnyContainer = false;
#if !UNITY_2019_4_17 // to distinguish between pre-2020 and post-2020 MRET.
        private string materialColourName = "_BaseColor";
#else
        private string materialColourName = "_TintColor";
#endif

        public Links(Data data)
        {
            this.data = data;
            log = new Log("Links", data.messenger);
        }
        internal void SetContainer(GameObject container)
        {
            this.container = container;
        }
        internal void Add(Link link)
        {
            if (!links.Contains(link))
            {
                links.Add(link);
            }
        }
        internal void SetAllLinkStatusTo(bool setTo = false)
        {
            foreach (Link link in links.Where(link => link.gameObject.activeSelf != setTo))
            {
                link.gameObject.SetActive(setTo);
            }
        }
        internal void DisableAllLinksButFor(Entity entity)
        {
            foreach (Link link in links.Where(link => (link.source != entity || link.destination != entity) && link.gameObject.activeSelf))
            {
                link.Disable();
            }
        }

        internal System.Collections.IEnumerator HighlightLinksFor(Node.Shape shape)
        {
            foreach (Link link in links.Where(link => (link.source == shape.entity || link.destination == shape.entity)))
            {
                // if user selects another node while this here is still trying to enlighten its edges, cease and desist.
                if (data.entities.NodeInFocus != shape)
                {
                    break;
                }

                link.Highlight();

                if (Time.deltaTime > data.UI.maxTimeForUpdatePerFrame)
                {
                    yield return data.UI.Sleep(data.UI.timeToWaitInUpdatePerFrame);
                }
            }
            yield return true;
        }

        internal System.Collections.IEnumerator UnHighlightLinksFor(Node.Shape shape)
        {
            foreach (Link link in links.Where(link => (link.source == shape.entity || link.destination == shape.entity) && link.gameObject.activeSelf))
            {
                link.UnHighlight();

                if (Time.deltaTime > data.UI.maxTimeForUpdatePerFrame)
                {
                    yield return data.UI.Sleep(data.UI.timeToWaitInUpdatePerFrame);
                }
            }
            yield return true;
        }

        internal void UnHighlightLinks()
        {
            foreach (Link link in links.Where(lnk => lnk.highlighted))
            {
                link.UnHighlight();
            }
        }

        internal void SetMessenger(Messenger messenger)
        {
            log.SetMessenger(messenger);
        }

        internal System.Collections.IEnumerator EnableLinksFor(Node.Shape shape)
        {
            DisableAllLinksButFor(shape.entity);
            HighlightLinksFor(shape);
            yield return true;
        }
        internal System.Collections.IEnumerator SetColliderStateFor(Node.Shape shape, bool setTo)
        {
            foreach (Link link in links.Where(link => (link.source == shape.entity || link.destination == shape.entity) && link.gameObject.activeSelf))
            {
                link.SetCollie(setTo);

                if (Time.deltaTime > data.UI.maxTimeForUpdatePerFrame)
                {
                    yield return data.UI.Sleep(data.UI.timeToWaitInUpdatePerFrame);
                }
            }
            yield return true;
        }
        internal System.Collections.IEnumerator ToggleAllLinks()
        {
            if (!toglingLinks)
            {
                toglingLinks = true;
                bool togleTo = false;
                if (links.Where(link => !link.gameObject.activeSelf).Count() > 0)
                {
                    togleTo = true;
                }
                foreach (Link link in links.Where(link => link.gameObject.activeSelf != togleTo))
                {
                    link.gameObject.SetActive(togleTo);
                    if (Time.deltaTime > data.UI.maxTimeForUpdatePerFrame)
                    {
                        yield return data.UI.Sleep(data.UI.timeToWaitInUpdatePerFrame);
                    }
                }
                toglingLinks = false;
            }
        }

        internal System.Collections.IEnumerator UpdateLinks()
        {
            if (!positioningLinks)
            {
                positioningLinks = true;
                float currentVDEscale = data.VDE.transform.localScale.x;
                int nrOf = 0;
                foreach (Link link in links)
                {
                    link.GlobalScaleChangedTo(currentVDEscale);
                    link.UpdatePositions();
                    nrOf++;
                    if (Time.deltaTime > data.UI.maxTimeForUpdatePerFrame)
                    {
                        yield return data.UI.Sleep(data.UI.timeToWaitInUpdatePerFrame);
                    }
                }
                positioningLinks = false;
                log.Entry("updated " + nrOf + " links", Log.Event.ToServer);
            }
        }

        internal IEnumerable<Link> GetLinksOf(Entity entity)
        {
            return links.Where(link => link.source == entity || link.destination == entity);
        }
        internal IEnumerable<Link> GetLinksFrom(Entity from)
        {
            return links.Where(link => link.source == from);
        }
        internal IEnumerable<Link> GetLinksTo(Entity to)
        {
            return links.Where(link => link.destination == to);
        }
        internal IEnumerable<Link> GetLinksFrom(Container from)
        {
            return links.Where(link => link.sourceContainer == from);
        }
        internal IEnumerable<Link> GetLinksTo(Container to)
        {
            return links.Where(link => link.destinationContainer == to);
        }
        /// <summary>
        /// this shall be called only from the main thread.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="weight"></param>
        internal void CreateLink(Entity source, Entity destination, int weight, float alpha, bool highlightOnCreation = false)
        {
            if (
                !links.Where(link => link.source == source && link.destination == destination).Any() &&
                (
                    source.type == Entity.Type.Node && destination.type == Entity.Type.Node ||
                    allowLinksBetweenAnyContainer
                )
            ) {
                GameObject linkGO = data.UI.nodeFactory.NibbleAtProduce(Factory.ProduceType.Edge);
                linkGO.name = source.name + " => " + destination.name;
                linkGO.transform.SetParent(container.transform);

                if (!linkGO.TryGetComponent(out Link link))
                {
                    link = linkGO.AddComponent<Link>();
                }
                link.materialColourName = materialColourName;
                link.source = source;
                link.destination = destination;
                link.weight = weight;
                link.visibleOnCreation = highlightOnCreation;
                link.SetColour(alpha);
                links.Add(link);
                link.Init(data);

                if (highlightOnCreation)
                {
                    link.Highlight();
                }
            }
        }

        internal System.Collections.IEnumerator SetMaterialsTo(int setTo)
        {
            Material setLinkMaterialTo = data.VDE.edge[setTo];
            foreach (Link link in links)
            {
                link.SetMaterialTo(setLinkMaterialTo);
                if (Time.deltaTime > data.UI.maxTimeForUpdatePerFrame)
                {
                    yield return data.UI.Sleep(data.UI.timeToWaitInUpdatePerFrame);
                }
            }
        }

        public async void Import(string incomingLinks)
        {
            try
            {
                IOrderedEnumerable<ImportLink> toImport = JsonConvert.DeserializeObject<List<ImportLink>>(incomingLinks).OrderByDescending(imp => imp.w);
                if (!(toImport is null) && toImport.Count() > 0)
                {
                    int maxWeight = toImport.FirstOrDefault().w; 
                    log.Entry("got " + toImport.Count() + " links to import", Log.Event.ToServer);

                    foreach (ImportLink incomingLink in toImport)
                    {
                        _ = Task.Run(() => LinkEntities(incomingLink.s, incomingLink.d, incomingLink.w, maxWeight));
                        expectedEntities++;
                    }
                }
                else
                {
                    log.Entry("Received empty set of links from server.", Log.Event.ToServer);
                }
            }
            catch (Exception exe)
            {
                log.Entry("Error importing links: " + exe.Message, Log.Event.ToServer);
            }
            while (data.forrestIsRunning && links.Where(link => !link.ready).Any())
            {
                int left = links.Count();

                data.messenger.Post(new Message()
                {
                    TelemetryType = Telemetry.Type.progress,
                    telemetry = new Telemetry()
                    {
                        type = Telemetry.Type.progress,
                        progress = new List<Progress>
                        {
                            new Progress()
                            {
                                name = "links",
                                description = "Links left to process",
                                grade = (left < expectedEntities / 2) ? Progress.Grade.red : Progress.Grade.green,
                                ints = new int[] { links.Where(link => !link.ready).Count(), links.Count() }
                            }
                        }
                    }
                });
                await data.UI.Sleep(999, 666);
            }
            data.messenger.Post(new Message()
            {
                TelemetryType = Telemetry.Type.progress,
                telemetry = new Telemetry()
                {
                    type = Telemetry.Type.progress,
                    progress = new List<Progress>
                    {
                        new Progress()
                        {
                            name = "links",
                            description = "Links left to process",
                            grade = Progress.Grade.green,
                            ints = new int[] { expectedEntities - links.Count(), expectedEntities }
                        }
                    }
                }
            });
            log.Entry("imported: " + links.Count() + " vs " + expectedEntities, Log.Event.ToServer);
        }
        internal async Task LinkEntities(int src, int dst, int count, int maxWeight, bool visibleOnCreation = false)
        {
            Entity srcEntity = null;
            while (data.forrestIsRunning && !data.entities.TryGet(src, out srcEntity))
            {
                await data.UI.Sleep(234);
            }
            if (!(srcEntity is null))
            {
                await LinkEntities(srcEntity, dst, count, maxWeight, visibleOnCreation);
            }
        }
        internal async Task LinkEntities(Entity source, int destinaion, int weight, int maxWeight, bool visibleOnCreation = false)
        {
            Entity dstEntity = null;
            while (data.forrestIsRunning && !data.entities.TryGet(destinaion, out dstEntity))
            {
                await data.UI.Sleep(123);
            }

            if (!(dstEntity is null))
            {
                LinkEntities(source, dstEntity, weight, maxWeight, visibleOnCreation);
            }
        }
        internal void LinkEntities(Entity source, Entity destination, int weight, int maxWeight, bool visibleOnCreation = false)
        {
            if (links.Where(lnk => lnk.source.id == source.id && lnk.destination.id == destination.id).Any())
            {
                // this trickery is necessary to get the main thread to call back to links so, that it could alter the MonoBehaving Link
                data.messenger.Post(new Message()
                {
                    LinkEvent = Link.Event.Highlight,
                    to = destination,
                    floats = new List<float>() { CalculateAlpha(weight, maxWeight), 1 },
                    number = weight,
                    from = source
                });
            }
            else
            {
                // this trickery is necessary to get the main thread to call back to links so, that it could add the MonoBehaving Link to the link's gameobject
                data.messenger.Post(new Message()
                {
                    LinkEvent = Link.Event.Create,
                    to = destination,
                    floats = 
                        visibleOnCreation ? 
                            new List<float>() { CalculateAlpha(weight, maxWeight), 1 } : 
                            new List<float>() { CalculateAlpha(weight, maxWeight) },
                    number = weight,
                    from = source
                });
            }
        }

        private static float CalculateAlpha(int weight, int maxWeight)
        {
            float edgeAlpha = ((float)weight / (float)maxWeight);

            if (edgeAlpha > 0.9F)
            {
                edgeAlpha = Color.white.a;
            }
            else if (edgeAlpha > 0.8F)
            {
                edgeAlpha = 0.3F;
            }
            else if (edgeAlpha < 0.2F)
            {
                edgeAlpha = 0.1F;
            }
            else
            {
                edgeAlpha = (float)Math.Min(Math.Max(edgeAlpha, 0.1F), 0.2F);
            }

            return edgeAlpha;
        }

        internal void HandleEvent(Message message)
        {
            switch (message.LinkEvent)
            {
                case Link.Event.Create:
                    CreateLink(message.from, message.to, message.number, message.floats[0], message.floats.Count > 1);
                    break;
                case Link.Event.Highlight:
                    {
                        foreach (Link lnk in links.Where(link => link.source == message.from && link.destination == message.to))
                        {
                            lnk.SetColour(message.floats[0]);
                            lnk.Highlight();
                        } 
                    }
                    break;
                case Link.Event.Delete:
                    break;
                case Link.Event.Initialize:
                    break;
                default:
                    break;
            }
        }
    }
}
