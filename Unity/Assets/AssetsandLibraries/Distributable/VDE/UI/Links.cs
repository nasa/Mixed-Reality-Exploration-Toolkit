/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
using Assets.VDE.Communication;
using Newtonsoft.Json;
using System;
using System.Collections;
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
        internal bool entitiesReady = false, linksReady = false;
        internal int
            taskCounter = 0,
            linksVisible = 0,
            processedLinks = 0,
            maxLinksVisible = 42,
            linksToImportCount = 0,
            duplicateLinksImported = 0,
            linksCreatedInLastBatch = 0,
            expectedNrOfVisibleLinks = 0,
            timedOutLinksInThisBatch = 0,
            unexpectedLinksInThisBatch = 0,
            postedForCreationInThisBatch = 0,
            postedForReCreationInThisBatch = 0,
            duplicateLinksImportedInLastBatch = 0,
            nrOfProcessedAndPossiblyVisibleLinks = 0,
            unnaturalLinksNOTImportedInLastBatch = 0;
        internal ConcurrentBag<Link> links = new ConcurrentBag<Link> { };
        internal ConcurrentBag<ImportLink> prelinks = new ConcurrentBag<ImportLink> { };
        internal IOrderedEnumerable<ImportLink> linksToImport;
        internal ConcurrentBag<Task> tasks = new ConcurrentBag<Task> { };
        private bool toglingLinks, positioningLinks, allowLinksBetweenAnyContainer = false;
#if !UNITY_2019_4_17 && !VDE_ML && !HDRP// to distinguish between pre-2020 and post-2020 MRET.
        private string materialColourName = "_BaseColor";
#else
        private string materialColourName = "_TintColor";
#endif

        public Links(Data data)
        {
            this.data = data;
            log = new Log("Links", data.messenger);
        }

        private IEnumerator EntitiesReady(object[] anObject)
        {
            data.links.maxLinksVisible = data.layouts.current.variables.indrek["maxEdgesInView"];
            entitiesReady = true;
            yield return entitiesReady;
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
            UpdateActiveLinkCount();
        }

        private void UpdateActiveLinkCount()
        {
            linksVisible = links.Where(link => link.gameObject.activeSelf).Count();
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
                    yield return new WaitForSeconds(data.UI.timeToWaitInUpdatePerFrame / 100);
                    //yield return data.UI.Sleep(data.UI.timeToWaitInUpdatePerFrame);
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
                    yield return new WaitForSeconds(data.UI.timeToWaitInUpdatePerFrame / 100);
                    //yield return data.UI.Sleep(data.UI.timeToWaitInUpdatePerFrame);
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
                    yield return new WaitForSeconds(data.UI.timeToWaitInUpdatePerFrame / 100);
                    //yield return data.UI.Sleep(data.UI.timeToWaitInUpdatePerFrame);
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
                        yield return new WaitForSeconds(data.UI.timeToWaitInUpdatePerFrame / 100);
                        //yield return data.UI.Sleep(data.UI.timeToWaitInUpdatePerFrame);
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
                        yield return new WaitForSeconds(data.UI.timeToWaitInUpdatePerFrame / 100);
                        //yield return data.UI.Sleep(data.UI.timeToWaitInUpdatePerFrame);
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
        internal void CreateLink(ImportLink importLink, Entity source, Entity destination, int weight, float alpha, bool visibleOnCreation = true, bool highlightOnCreation = false)
        {
            if (source.type != Entity.Type.Node || destination.type != Entity.Type.Node)
            {
                importLink.status = ImportLink.Status.Unnatural;
                duplicateLinksImported++;
                unnaturalLinksNOTImportedInLastBatch++;
                return;
            }
            if (
                !links.Where(link => link.source == source && link.destination == destination).Any() &&
                (
                    source.type == Entity.Type.Node && destination.type == Entity.Type.Node ||
                    allowLinksBetweenAnyContainer
                )
            )
            {
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
                link.visibleOnCreation = visibleOnCreation;
                link.SetColour(alpha);
                links.Add(link);
                link.Init(data);

                if (highlightOnCreation)
                {
                    link.Highlight();
                }
                linksCreatedInLastBatch++;
                importLink.status = ImportLink.Status.Done;
                prelinks.Append(importLink);
            }
            else
            {
                try
                {
                    foreach (Link link in links.Where(link => link.source == source && link.destination == destination))
                    {
                        link.weight += weight;
                        link.SetColour(link.colour.a + alpha);
                    }
                }
                catch (Exception exe)
                {
                    log.Entry("Unsuccsessful update of a link: " + exe.Message);
                }
                importLink.status = ImportLink.Status.Duplicate;
                duplicateLinksImported++;
                duplicateLinksImportedInLastBatch++;
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
                    yield return new WaitForSeconds(data.UI.timeToWaitInUpdatePerFrame / 100);
                    //yield return data.UI.Sleep(data.UI.timeToWaitInUpdatePerFrame);
                }
            }
        }
        internal void Export()
        {
            List<ExportLink> exportLinks = new List<ExportLink>();
            foreach (Link link in links)
            {
                exportLinks.Add(new ExportLink
                {
                    d = link.destination.id,
                    s = link.source.id,
                    w = link.weight,
                    r = link.colour.r,
                    g = link.colour.g,
                    b = link.colour.b,
                    a = link.colour.a
                });
            }
            if (!(data.VDE.signalRConnection is null))
            {
                data.VDE.signalRConnection.SendLinks(exportLinks);
                log.Entry("relations sent to.. a server.");
            }
            if (data.VDE.cacheResponseFromServer)
            {
                System.IO.File.WriteAllText("c:\\data\\relations.json", JsonConvert.SerializeObject(exportLinks));
                log.Entry("relations shaved to c:\\data\\relations.json");
            }
        }
        public async void Import(string incomingLinks)
        {
            Task linksCreationProgressMonitor = null;
            int
                linksToProcessInBatch = 123,
                linkBatchesProcessed = 0,
                linksProcessedInThisBatch = 0,
                linksProcessed = 0;

            IEnumerator LinkReady(object[] anObject)
            {
                linksVisible++;
                linksProcessedInThisBatch++;
                yield return true;
            }
            try
            {
                data.messenger.SubscribeToEvent(LinkReady, "Link.Event.Ready", 0);
                if (data.entities.entities.Count > 0 && data.entities.entities.Where(ent => !ent.Value.ready).Any())
                {
                    data.messenger.SubscribeToEvent(EntitiesReady, Layouts.Layouts.LayoutEvent.LayoutPopulated.ToString(), 0);
                }
                else if (data.entities.entities.Count > 0 && !data.entities.entities.Where(ent => !ent.Value.ready).Any())
                {
                    entitiesReady = true;
                }
                linksToImport =
                    JsonConvert.DeserializeObject<List<ImportLink>>(incomingLinks).
                    OrderByDescending(imp => imp.w);

                linksToImportCount = linksToImport.Count();
                // this expectedNrOfVisibleLinks is initialized here for the first few runs of the monitoring process,
                // but updated below per each round.
                expectedNrOfVisibleLinks = Math.Min(maxLinksVisible, linksToImportCount);
                linksCreationProgressMonitor = Task.Run(() => MonitorLinksCreationProgress(data));

                if (!(linksToImport is null) && linksToImportCount > 0)
                {
                    int maxWeight = linksToImport.FirstOrDefault().w;
                    log.Entry("Got " + linksToImportCount + " links to import with maxWeight: " + maxWeight + ", waiting for entities to get ready.", Log.Event.ToServer);
                    await WaitForEntitiesToBeReady();

                    while (
                        data.forrestIsRunning
                        &&
                        linksVisible < Math.Min(maxLinksVisible, linksToImportCount)
                    )
                    {
                        IEnumerable<ImportLink> links = linksToImport.Skip(linksToProcessInBatch * linkBatchesProcessed++).Take(linksToProcessInBatch);
                        if (links.Count() == 0)
                        {
                            break;
                        }
                        await ImportLinks(links, maxWeight);

                        while (data.forrestIsRunning)
                        {
                            if (
                                linksProcessedInThisBatch +
                                timedOutLinksInThisBatch +
                                unexpectedLinksInThisBatch +
                                postedForCreationInThisBatch +
                                postedForReCreationInThisBatch +
                                duplicateLinksImportedInLastBatch +
                                unnaturalLinksNOTImportedInLastBatch
                                >= links.Count())
                            {
                                nrOfProcessedAndPossiblyVisibleLinks += postedForCreationInThisBatch;
                                expectedNrOfVisibleLinks = Math.Min(Math.Min(maxLinksVisible, linksToImportCount), nrOfProcessedAndPossiblyVisibleLinks);

                                linksProcessed +=
                                linksProcessedInThisBatch +
                                timedOutLinksInThisBatch +
                                unexpectedLinksInThisBatch +
                                postedForCreationInThisBatch +
                                postedForReCreationInThisBatch +
                                duplicateLinksImportedInLastBatch +
                                unnaturalLinksNOTImportedInLastBatch;

                                await WaitForLinksToBeReady();

                                linksCreatedInLastBatch = 0;
                                linksProcessedInThisBatch = 0;
                                timedOutLinksInThisBatch = 0;
                                unexpectedLinksInThisBatch = 0;
                                postedForCreationInThisBatch = 0;
                                postedForReCreationInThisBatch = 0;
                                duplicateLinksImportedInLastBatch = 0;
                                unnaturalLinksNOTImportedInLastBatch = 0;

                                break;
                            }
                            await data.UI.Sleep(99, 66);
                        }
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

            if (expectedNrOfVisibleLinks > 0)
            {
                await WaitForLinksToBeReady(expectedNrOfVisibleLinks);
            }
            linksReady = true;
            if (!(linksCreationProgressMonitor is null))
            {
                linksCreationProgressMonitor.Dispose();
            }
            SetImportProgressStatus(linksProcessed, linksProcessed);

            log.Entry("postedForCreation: " + postedForCreationInThisBatch + "; processedLinks: " + processedLinks + "; unexpectedLinks " + unexpectedLinksInThisBatch + " toImportCount: " + linksToImportCount + "; Not ready: " + links.Where(link => !link.ready).Count().ToString() + "; Ready: " + links.Where(link => link.ready).Count().ToString() + "; Count: " + links.Count().ToString() + "; from ingested total of: " + linksToImportCount, Log.Event.ToServer);

        }

        private void SetImportProgressStatus(int achual, int expected)
        {
            float proge = (float)achual / (float)expected;
            data.messenger.Post(new Message()
            {
                HUDEvent = HUD.HUD.Event.Progress,
                number = 2,
                floats = new List<float> { proge, 1F },
                message = "Preparing edges",
                from = data.layouts.current.GetGroot()
            });
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
                            ints = new int[] { expected - achual, expected }
                        }
                    }
                }
            });
        }

        private async Task ImportLinks(IEnumerable<ImportLink> toImport, int maxWeight)
        {
            foreach (ImportLink incomingLink in toImport)
            {
                // failsafe for cases, where the list of links contains references to nonexistent entities.
                // it is assumed here, that the ALL entities to be imported in this round are
                // already imported: hence the "await WaitForEntitiesToBeReady();" above.
                if (data.entities.entities.ContainsKey(incomingLink.s) && data.entities.entities.ContainsKey(incomingLink.d))
                {
                    taskCounter++;
                    tasks.Add(Task.Run(() => LinkEntities(incomingLink, maxWeight, true, 2)).ContinueWith(t => taskCounter--));
                    await WaitForEnoughLinesToBeReady(22);
                    processedLinks++;
                }
                else
                {
                    unexpectedLinksInThisBatch++;
                }
            }
        }

        private async Task WaitForEntitiesToBeReady()
        {
            while (!entitiesReady)
            {
                await data.UI.Sleep(1234, 765);
            }
        }

        private async Task WaitForEnoughLinesToBeReady(int waitFor)
        {
            int tiks = 0, taskaunter = taskCounter;
            while (data.forrestIsRunning && taskCounter > waitFor && links.Where(link => !link.ready).Count() > waitFor)
            {
                await data.UI.Sleep(++tiks * waitFor);
            }
        }

        private async Task WaitForLinksToBeReady()
        {
            await data.UI.Sleep(345, 234);
            while (data.forrestIsRunning && links.Where(link => !link.ready).Any())
            {
                await data.UI.Sleep(345, 234);
            }
        }
        private async Task WaitForLinksToBeReady(int waitForIt)
        {
            Stack<int> counts = new Stack<int> { };
            try
            {
                while (data.forrestIsRunning && (links.Count < 1 || links.Where(link => link.ready && !(link.gameObject is null) && link.gameObject.activeSelf).Count() < waitForIt))
                {
                    int curr = links.Count - links.Where(link => !link.ready).Count();
                    counts.Push(curr);
                    if (counts.Count > 5)
                    {
                        counts.Pop();
                    }
                    if (links.Count > 1 && counts.Count > 4 && !counts.Where(count => count != curr).Any())
                    {
                        break;
                    }
                    SetImportProgressStatus(links.Where(link => !link.ready).Count(), links.Count);
                    await data.UI.Sleep(789, 345);
                }

            }
            // using trycatch because in case of SignalR and standalone the links are imported by the main thread, while
            // with GMSEC we make it here indirectly and detecting this in a civilized manner is unfortunately impossible.
            // alas: gameObject cannot be accessed from other, but the main (unity) thread.
            catch (Exception)
            {
                log.Entry("Not running in main thread, WaitForLinksToBeReady using backup rountine.");
                while (data.forrestIsRunning && (links.Count < 1 || links.Where(link => link.ready).Count() < waitForIt))
                {
                    int curr = links.Count - links.Where(link => !link.ready).Count();
                    counts.Push(curr);
                    if (counts.Count > 5)
                    {
                        counts.Pop();
                    }
                    if (links.Count > 1 && counts.Count > 4 && !counts.Where(count => count != curr).Any())
                    {
                        break;
                    }
                    SetImportProgressStatus(links.Where(link => !link.ready).Count(), links.Count);
                    await data.UI.Sleep(789, 345);
                }
            }
        }
        internal async void MonitorLinksCreationProgress(Data data)
        {
            while (data.forrestIsRunning && (linksToImportCount == 0 || !entitiesReady))
            {
                await data.UI.Sleep(2345, 1234);
            }

            // the implicit expectaion here is, that this task will be disposed of in the Import function,
            // once all edges have been processed
            while (
                data.forrestIsRunning && 
                !linksReady &&
                nrOfProcessedAndPossiblyVisibleLinks < expectedNrOfVisibleLinks && 
                data.layouts.current.state != Layouts.Layout.State.populated
            ) {
                SetImportProgressStatus(nrOfProcessedAndPossiblyVisibleLinks, expectedNrOfVisibleLinks);
                await data.UI.Sleep(1234, 765);
            }
            data.messenger.Post(new Message()
            {
                LayoutEvent = Layouts.Layouts.LayoutEvent.LinksReady,
                number = 0,
                from = data.layouts.current.GetGroot()
            });
        }

        internal async Task LinkEntities(ImportLink link, int maxWeight, bool visibleOnCreation = false, int timeout = 12)
        {
            Entity srcEntity = null, dstEntity = null;
            while (timeout-- > 0 && !linksReady && data.forrestIsRunning && !data.entities.TryGet(link.s, out srcEntity))
            {
                await data.UI.Sleep(234, 123);
            }
            while (timeout-- > 0 && !linksReady && data.forrestIsRunning && !data.entities.TryGet(link.d, out dstEntity))
            {
                await data.UI.Sleep(234, 123);
            }
            if (!(srcEntity is null) && !(dstEntity is null))
            {
                link.status = ImportLink.Status.BeingCreated;
                LinkEntities(link, srcEntity, dstEntity, link.w, maxWeight, visibleOnCreation);
            }

            if (timeout == 0)
            {
                timedOutLinksInThisBatch++;
            }
        }
        internal void LinkEntities(ImportLink link, Entity source, Entity destination, int weight, int maxWeight, bool visibleOnCreation = false)
        {
            if (links.Where(lnk => lnk.source.id == source.id && lnk.destination.id == destination.id).Any())
            {
                // this trickery is necessary to get the main thread to call back to links so, that it could alter the MonoBehaving Link
                postedForReCreationInThisBatch++;
                data.messenger.Post(new Message()
                {
                    LinkEvent = Link.Event.Highlight,
                    obj = new object[] { link },
                    to = destination,
                    floats = new List<float>() { CalculateAlpha(weight, maxWeight), 1 },
                    number = weight,
                    from = source
                });
            }
            else
            {
                // this trickery is necessary to get the main thread to call back to links so, that it could add the MonoBehaving Link to the link's gameobject
                postedForCreationInThisBatch++;
                data.messenger.Post(new Message()
                {
                    LinkEvent = Link.Event.Create,

                    to = destination,
                    obj = new object[] { link },
                    floats =
                        visibleOnCreation ?
                            new List<float>() { CalculateAlpha(weight, maxWeight), 1 } :
                            new List<float>() { CalculateAlpha(weight, maxWeight), 0 },
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
                    bool
                        visibleOnCreation = false,
                        highlightOnCreation = false;
                    if (!(message.floats is null) && message.floats.Count > 1)
                    {
                        // only enable the link
                        if (message.floats[1] == 1)
                        {
                            visibleOnCreation = true;
                        }
                        // enable AND highlight the link
                        else if (message.floats[1] == 2)
                        {
                            visibleOnCreation = highlightOnCreation = true;
                        }
                    }
                    CreateLink((ImportLink)message.obj[0], message.from, message.to, message.number, message.floats[0], visibleOnCreation, highlightOnCreation);
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
                case Link.Event.Ready:
                    {
                        foreach (Callbacks.Callback callback in data.messenger.eventSubscriptions[0]["Link.Event.Ready"])
                        {
                            data.VDE.StartCoroutine(callback(message.obj));
                        }
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
