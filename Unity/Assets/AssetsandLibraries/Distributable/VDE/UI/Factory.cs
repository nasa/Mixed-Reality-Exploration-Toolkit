/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.VDE.UI
{
    /// <summary>
    /// this trickery is necessary to chug out gameobjects to the entities that are running (initialy) in threads.
    /// </summary>
    class Factory : MonoBehaviour
    {
        Log log;
        Data data;
        internal enum ProduceType
        {
            Group,
            Node,
            Edge
        }
        private readonly ConcurrentBag<int> inProduction = new ConcurrentBag<int> { };
        internal readonly ConcurrentQueue<Entity> queue = new ConcurrentQueue<Entity>();
        internal readonly ConcurrentQueue<int> readyForHunters = new ConcurrentQueue<int>();
        internal readonly Queue<GameObject> 
            edgeProduce = new Queue<GameObject>(), 
            nodeShapeProduce = new Queue<GameObject>(), 
            groupShapeProduce = new Queue<GameObject>();
        internal readonly ConcurrentDictionary<int, List<object>> creatures = new ConcurrentDictionary<int, List<object>> { };
        internal readonly ConcurrentDictionary<int, ConcurrentStack<Callbacks.Callback>> hunters = new ConcurrentDictionary<int, ConcurrentStack<Callbacks.Callback>> { };

        internal void Init(Data data)
        {
            log = new Log("VDE.UI.Factory", data.messenger);
            this.data = data;
        }
        private void Start()
        {
            ProduceSomeProduce(1234, ProduceType.Node);
            ProduceSomeProduce(1234, ProduceType.Edge);
        }

        private void ProduceSomeProduce(int quota, ProduceType type)
        {
            switch (type)
            {
                case ProduceType.Group:
                    {
                        while (groupShapeProduce.Count() < quota)
                        {
                            GameObject product = Instantiate(data.VDE.groupShapeTemplate);
                            product.transform.SetParent(data.VDE.groupShapeTemplate.transform.parent);
                            product.SetActive(false);
                            groupShapeProduce.Enqueue(product);
                        }
                        break;
                    }
                case ProduceType.Node:
                    {
                        while (nodeShapeProduce.Count() < quota)
                        {
                            GameObject product = Instantiate(data.VDE.nodeShapeTemplate);
                            product.transform.SetParent(data.VDE.nodeShapeTemplate.transform.parent);
                            product.SetActive(false);
                            nodeShapeProduce.Enqueue(product);
                        }
                        break;
                    }
                case ProduceType.Edge:
                    {
                        while (edgeProduce.Count() < quota)
                        {
                            GameObject product = Instantiate(data.VDE.edgeTemplate);
                            product.transform.SetParent(data.VDE.edgeTemplate.transform.parent);
                            product.SetActive(false);
                            edgeProduce.Enqueue(product);
                        }
                        break;
                    }
                default:
                    break;
            }
        }

        internal GameObject NibbleAtProduce(ProduceType type)
        {
            switch (type)
            {
                case ProduceType.Group:
                    {
                        if (groupShapeProduce.Count < 5)
                        {
                            ProduceSomeProduce(123, type);
                        }
                        GameObject product = groupShapeProduce.Dequeue();
                        product.SetActive(true);
                        return product;
                    }
                case ProduceType.Node:
                    {
                        if (nodeShapeProduce.Count < 5)
                        {
                            ProduceSomeProduce(1234, type);
                        }
                        GameObject product = nodeShapeProduce.Dequeue();
                        product.SetActive(true);
                        return product;
                    }
                case ProduceType.Edge:
                    {
                        if (edgeProduce.Count < 5)
                        {
                            ProduceSomeProduce(1234, type);
                        }
                        GameObject product = edgeProduce.Dequeue();
                        product.SetActive(true);
                        return product;
                    }
                default:
                    return null;
            }
        }

        /// <summary>
        /// enqueue the creation and future notification of the requester, once the requested object has been excreted.
        /// </summary>
        /// <param name="entity"></param>
        internal void Request(Entity entity, Callbacks.Callback callback)
        {
            GetCreaturesOfEntity(entity.id, callback);
            queue.Enqueue(entity);
        }
        /// <summary>
        /// trickery to provide threads with gameobjects.
        /// </summary>
        /// <param name="entity"></param>
        internal void GetCreaturesOfEntity(int entityID, Callbacks.Callback callback)
        {
            if (!(callback is null))
            {
                if (!hunters.ContainsKey(entityID))
                {
                    hunters.TryAdd(entityID, new ConcurrentStack<Callbacks.Callback> { });
                    hunters[entityID].Push(callback);
                }
                else if (!hunters[entityID].Contains(callback))
                {
                    hunters[entityID].Push(callback);
                }

                if (creatures.ContainsKey(entityID))
                {
                    ReadyForHunters(entityID);
                }
            }
        }

        private void ReadyForHunters(int entityID)
        {
            if (!readyForHunters.Contains(entityID))
            {
                readyForHunters.Enqueue(entityID);
            }
        }

        internal IEnumerator Produce()
        {
            int productsPerFrame = 25;
            int gamesPerFrame = 25;

            if (queue.Count() > 0)
            {
                // this tries to ensure that framerate doesnt drop when queue is empty in case an internal function runs it dry and gets stuck there for the rest of the frame.
                if (nodeShapeProduce.Count() <= productsPerFrame)
                {
                    ProduceSomeProduce(productsPerFrame * 10, ProduceType.Node);
                    productsPerFrame -= 10;
                }
                while (productsPerFrame-- > 0 && queue.TryDequeue(out Entity entity))
                {
                    Container existing = GetContainer(entity.id);

                    if (existing is null && !inProduction.Contains(entity.id))
                    {
                        inProduction.Add(entity.id);
                        Handler(entity);
                    }
                    else if (!(existing is null))
                    {
                        ReadyForHunters(entity.id);
                    }
                }
            }

            // waitTimePerFrame for hunters are the same as for queue - products have priority over their delivery.
            if (readyForHunters.Count() > 0)
            {
                while (gamesPerFrame-- > 0 && readyForHunters.TryDequeue(out int game))
                {
                    Container result = GetContainer(game);
                    if (!(result is null) && hunters.ContainsKey(game))
                    {
                        foreach (Callbacks.Callback callback in hunters[game])
                        {
                            StartCoroutine(callback(new object[3] { result.entity, result, GetCreaturesFor(game) }));
                        }
                    }
                }
            }
            yield return true;
        }
        private void Handler(Entity entity)
        {
            try
            {
                switch (entity.type)
                {
                    case Entity.Type.Group:
                        {
                            CreateGroup(entity);
                        }
                        break;
                    case Entity.Type.Node:
                        {
                            CreateNode(entity);
                        }
                        break;
                    default:
                        log.Entry(entity.name + " emerged with an unexpected Entity.Type: " + entity.type.ToString(), Log.Event.ToServer);
                        break;
                }
            }
            catch (Exception exe)
            {
                log.Entry("Handler error while processing " + entity.name + ": " + exe.Message + "\n" + exe.StackTrace, Log.Event.ToServer);
            }
        }
        /// <summary>
        /// creates a control GameObject for that group, that itself is invisible, but contains all the visual elements underneath it.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private Group.Container CreateGroup(Entity entity)//, GameObject ngo)
        {
            GameObject ngo = NibbleAtProduce(ProduceType.Group);
            ngo.name = entity.name;
            ngo.tag = data.UI.dataShapeTagName;
            ngo.layer = LayerMask.NameToLayer("gazable");

            if (entity.parent is null)
            {
                ngo.transform.SetParent(data.VDE.transform);
            }
            else
            {
                ngo.transform.SetParent(entity.parent.containers.GetGroup(entity.waitingForNodeInLayout).transform);
            }
            ngo.transform.localPosition = Vector3.zero;
            ngo.transform.localRotation = Quaternion.Euler(Vector3.zero);

            Group.Container group = ngo.AddComponent<Group.Container>();
            group.SetPositionCorrection(Container.PositionCorrectionDirection.keepTheCentre);
            group.tag = data.UI.dataShapeTagName;

            GameObject vgo = CreateVisibleShape(ngo, ProduceType.Group);
            vgo.transform.localScale = Vector3.zero;
            vgo.transform.localRotation = Quaternion.Euler(Vector3.zero);

            Group.Shape shape = vgo.AddComponent<Group.Shape>();
            shape.Init(entity, group);
            if (entity.waitingForNodeInLayout.variables.flags["showLables"])
            {
                group.label = ActivateLabel(group, shape, Entity.Type.Group);
            }

            group.Init(entity, entity.waitingForNodeInLayout); 
            group.shapes.Add(shape);

            AddCreature(entity.id, group);
            AddCreature(entity.id, shape);
            ReadyForHunters(entity.id);

            return group;
        }

        private GameObject ActivateLabel(Container container, Shape shape, Entity.Type type)
        {
            Transform trafo = container.gameObject.transform.Find("Label");

            // workaround for non-textmessbro labels.
            if (trafo is null)
            {
                trafo = container.gameObject.transform.Find("LabelCanvas");
                trafo = trafo.Find("Label");
            }

            // the "out Text _" is to please the compiler, as it cant understand the two tries in || clause.
            if (!(trafo is null) && (trafo.TryGetComponent(out TextMeshPro tmp) || trafo.TryGetComponent(out Text _)))
            {
                Facer facer;
                if (!(tmp is null))
                {
                    tmp.text = container.name;
                } 
                else if(trafo.TryGetComponent(out Text text)) 
                {
                    text.text = container.name;
                }
                trafo.name = container.name + " visible label";

                if (type == Entity.Type.Group && shape.entity.distanceFromGroot > shape.layout.variables.indrek["showLabelsMaxDepth"])
                {
                    if (!trafo.gameObject.TryGetComponent(out facer))
                    {
                        facer = trafo.gameObject.AddComponent<Group.GroupFacer>();
                    }
                    //facer.visibleMaxDistanceFromCamera = shape.layout.variables.floats["groupLabelVisibleMaxDistanceFromCamera"];
                    //facer.visibleMinDistanceFromCamera = shape.layout.variables.floats["groupLabelVisibleMinDistanceFromCamera"];

                    // groupLabelVisibleMaxDistanceFromCamera = groot + 2
                    // groupLabelVisibleMinDistanceFromCamera = container.entity.md 

                    int labelsVisibleForGroupsThatAreFartherFromGrootThan = 0;// shape.layout.variables.indrek["showLabelsMaxDepth"];
                    float totalDistanceWhereLabelCouldBeVisible =
                        shape.layout.variables.floats["groupLabelVisibleMaxDistanceFromCamera"] -
                        shape.layout.variables.floats["groupLabelVisibleMinDistanceFromCamera"];
                    int numberOfGroupsThatWantToShowLabels = Math.Max(1, shape.entity.md - labelsVisibleForGroupsThatAreFartherFromGrootThan - 1); // 1 from nodes.
                    float widthOfVisibilityLayerForGroup = totalDistanceWhereLabelCouldBeVisible / numberOfGroupsThatWantToShowLabels;
                    facer.visibleOnceCameraIsCloserThan =
                        shape.layout.variables.floats["groupLabelVisibleMinDistanceFromCamera"] +
                        ((shape.entity.md - shape.entity.distanceFromGroot - labelsVisibleForGroupsThatAreFartherFromGrootThan) * widthOfVisibilityLayerForGroup);
                    facer.visibleUntilCameraIsFartherThan =
                        shape.layout.variables.floats["groupLabelVisibleMinDistanceFromCamera"] +
                        ((shape.entity.md - shape.entity.distanceFromGroot - labelsVisibleForGroupsThatAreFartherFromGrootThan - 1) * widthOfVisibilityLayerForGroup);

                    SetFacerDefaults(ref shape, ref facer);
                }
                else if (type == Entity.Type.Node)
                {
                    if (!trafo.gameObject.TryGetComponent(out facer))
                    {
                        facer = trafo.gameObject.AddComponent<Node.NodeFacer>();
                    }
                    facer.visibleOnceCameraIsCloserThan = shape.layout.variables.floats["nodeLabelVisibleSinceDistanceFromCamera"];
                    SetFacerDefaults(ref shape, ref facer);
                }

                trafo.gameObject.SetActive(true);
            }
            return trafo.gameObject;

            static void SetFacerDefaults(ref Shape shape, ref Facer facer)
            {
                facer.maxTextSize = shape.layout.variables.floats["maxTextSize"];
                facer.maxTextSizeDistance = shape.layout.variables.floats["maxTextSizeDistance"];
                facer.minTextSize = shape.layout.variables.floats["minTextSize"];
                facer.minTextSizeDistance = shape.layout.variables.floats["minTextSizeDistance"];
                facer.referenceShape = shape.gameObject;
            }
        }

        private Node.Container CreateNode(Entity entity)
        {
            GameObject ngo = NibbleAtProduce(ProduceType.Node);
            ngo.name = entity.name;
            ngo.tag = data.UI.nodeTagName;
            ngo.layer = LayerMask.NameToLayer("grabbable");

            Node.Container node = ngo.AddComponent<Node.Container>();
            GameObject vgo = CreateVisibleShape(ngo, ProduceType.Node);
            Node.Shape shape = vgo.AddComponent<Node.Shape>();
            if (entity.parent is null)
            {
                StartCoroutine(node.HuntForParent());
                shape.Init(entity, null);
            }
            else
            {
                ngo.transform.SetParent(entity.parent.containers.GetGroup(entity.waitingForNodeInLayout).transform);
                ngo.transform.localPosition = Vector3.zero;
                ngo.transform.localEulerAngles = Vector3.zero;
                node.Init(entity, entity.waitingForNodeInLayout);
                shape.Init(entity, entity.parent.containers.GetGroup(entity.waitingForNodeInLayout));
            }
            node.shapes.Add(shape);
            node.label = ActivateLabel(node, shape, Entity.Type.Node);
            node.SetLabelState(false);

            AddCreature(entity.id, node);
            AddCreature(entity.id, shape);
            ReadyForHunters(entity.id);

            return node;
        }
        void AddCreature(int entityID, object creature)
        {
            if (!creatures.ContainsKey(entityID))
            {
                creatures.TryAdd(entityID, new List<object> { });
            }
            if (!creatures[entityID].Contains(creature))
            {
                creatures[entityID].Add(creature);
            }
        }
        List<object> GetCreaturesFor(int entityID)
        {
            List<object> found = new List<object> { };

            if (creatures.ContainsKey(entityID))
            {
                foreach (var creature in creatures[entityID])
                {
                    found.Add(creature);
                }
            }

            if (found.Count() > 0)
            {
                return found;
            }
            return null;
        }
        Container GetContainer(int entityID)
        {
            if (creatures.ContainsKey(entityID))
            {
                return creatures[entityID].FirstOrDefault(creature =>
                    creature.GetType().Name == "Container" ||
                    creature.GetType().Name == "Group" ||
                    creature.GetType().Name == "Node") as Container;
            }
            return null;
        }
        private GameObject CreateVisibleShape(GameObject containersGameObject, ProduceType type)
        {
            GameObject cubeShape = NibbleAtProduce(type);
            cubeShape.name = "Visible Shape of " + containersGameObject;
            switch (type)
            {
                case ProduceType.Group:
                    cubeShape.layer = LayerMask.NameToLayer("gazable");
                    cubeShape.tag = data.UI.dataShapeTagName;
                    break;
                case ProduceType.Node:
                    cubeShape.layer = LayerMask.NameToLayer("pointable");
                    cubeShape.tag = data.UI.nodeTagName;
                    break;
                case ProduceType.Edge:
                    cubeShape.layer = LayerMask.NameToLayer("grabbable");
                    break;
                default:
                    cubeShape.layer = LayerMask.NameToLayer("grabbable");
                    break;
            }
            cubeShape.transform.SetParent(containersGameObject.transform);
            cubeShape.transform.localScale = Vector3.one;

            return cubeShape;
        }
    }
}
