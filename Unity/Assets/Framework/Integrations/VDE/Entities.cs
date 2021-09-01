/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
using Assets.VDE.Communication;
using Assets.VDE.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Assets.VDE.Callbacks;

namespace Assets.VDE
{
    /// <summary>
    /// The main challenges with entities are:
    /// 1. the handling of asynchronously awakening entities that are positioned in different depths of the structure tree;
    /// 2. creation of GameObjects for asynchronously awakening entities and assigning their parents correctly;
    /// 3. creating joints between awakening GameObjects only once these have rigid bodies, colliders etc and are NOT colliding. 
    ///    Oh, and joints are created in the opposite direction to the groups (of groups (of groups (..))) structure to 
    ///    allow the subgroups to expand before those would collide with their neighbors.
    /// </summary>
    public class Entities
    {
        Log log;
        Data data;
        ConcurrentDictionary<int, ConcurrentDictionary<Event, ConcurrentBag<Callback>>> eventSubscriptions = new ConcurrentDictionary<int, ConcurrentDictionary<Event, ConcurrentBag<Callback>>> { };// new ConcurrentDictionary<int, ConcurrentDictionary<LayoutEvent, Callback>> { };

        internal ConcurrentDictionary<int, Entity> entities = new ConcurrentDictionary<int, Entity> { };
        ConcurrentBag<Entity> trash = new ConcurrentBag<Entity> { };

        internal Callbacks callbacks = new Callbacks();

        internal UI.Node.Shape NodeInFocus { get; set; }

        internal enum Event
        {
            NotSet,
            ReceiveChild,
            ReceiveSibling,
            ChangeLayout,
            NewEntity,
            ReNewEntity
        }

        public struct ExportEntity
        {
            public string name { get; set; }
            public int id { get; set; }
            public float[] position { get; set; }
            public float[] rotation { get; set; }
            public float[] scale { get; set; }
            public float[] colour { get; set; }
        }

        public Entities(Data data)
        {
            log = new Log("VDE.Entities");
            this.data = data;
        }
        internal void SetMessenger(Messenger messenger)
        {
            log.SetMessenger(messenger);
        }
        /// <summary>
        /// returns entity if found, otherwise queues callback, once such an entity has been created.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        internal Entity Get(int id, Callback callback)
        {
            Entity found = Get(id);
            if (found is null)
            {
                callbacks.RequestCallback(id.ToString(), callback);
                return null;
            }
            return found;
        }

        internal System.Collections.IEnumerator SetMaterialsTo(int setTo)
        {
            UnityEngine.Material setNodeMaterialTo = data.VDE.node[setTo];
            UnityEngine.Material setGroupMaterialTo = data.VDE.group[setTo];
            foreach (Entity entity in entities.Values)
            {
                if (entity.type == Entity.Type.Group)
                {
                    entity.containers.SetMaterialsTo(setGroupMaterialTo);
                }
                else
                {
                    entity.containers.SetMaterialsTo(setNodeMaterialTo);
                }
                if (UnityEngine.Time.deltaTime > data.UI.maxTimeForUpdatePerFrame)
                {
                    yield return data.UI.Sleep(20);
                }
            }
        }

        internal void ExportShapesAndPositions()
        {
            if (!(data.VDE.connection is null))
            {
                List<ExportEntity> exportEntities = new List<ExportEntity> { };
                foreach (Entity entity in entities.Values)
                {
                    Shape entityShape = entity.containers.GetCurrentShape();
                    Color colour = entityShape.GetColor();
                    exportEntities.Add(new ExportEntity()
                    {
                        id = entity.id,
                        name = entity.name,
                        colour = new float[4] { colour.r, colour.g, colour.b, Math.Max(colour.a, entity.a) },
                        position = new float[3] { entityShape.transform.position.x, entityShape.transform.position.y, entityShape.transform.position.z },
                        rotation = new float[3] { entityShape.transform.eulerAngles.x, entityShape.transform.eulerAngles.y, entityShape.transform.eulerAngles.z },
                        scale = new float[3] { entityShape.transform.localScale.x, entityShape.transform.localScale.y, entityShape.transform.localScale.z }
                    });
                }
                data.VDE.connection.SendEntities(exportEntities);
            }
        }

        internal void SetEntitiesVisibilityForFPS(int target, bool state)
        {
            if (state)
            {
                foreach (Entity entity in entities.Values.Where(ent => ent.type == Entity.Type.Node && ent.enabled != state).OrderByDescending(ent => ent.count).Take(target))
                {
                    entity.SetVisibility(state);
                }
            }
            else
            {
                foreach (Entity entity in entities.Values.Where(ent => ent.type == Entity.Type.Node && ent.enabled != state).OrderBy(ent => ent.count).Take(target))
                {
                    entity.SetVisibility(state);
                }
            }
        }

        internal IEnumerable<Entity> GetGroup(Entity entity)
        {
            return entities.Where(ent => ent.Value.parentID == entity.id).Select(ent => ent.Value);
        }

        /// <summary>
        /// get entity by its ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal Entity Get(int id)
        {
            if (entities.TryGetValue(id, out Entity entity))
            {
                return entity;
            }
            else
            {
                return null;
            }
        }

        internal bool TryGet(int id, out Entity entity)
        {
            entity = Get(id);
            return !(entity is null);
        }
        /// <summary>
        /// get entity by its name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal Entity Get(string name)
        {
            Entity entity = null;
            try
            {
                entity = entities.Where(ent => ent.Value.name == name).FirstOrDefault().Value;
            }
            catch (Exception) { }
            return entity;
        }
        /// <summary>
        /// get an entity with specified name and specified parent's name
        /// </summary>
        /// <param name="entity">name of the entity</param>
        /// <param name="parent">name of the entity's parent</param>
        /// <returns></returns>
        internal Entity Get(string entityName, string parentName)
        {
            return entities.Where(ent =>
                ent.Value.name == entityName &&
                entities[ent.Value.relations.FirstOrDefault(rel => rel.Value == Entity.Relation.Parent).Key].name == parentName
            ).FirstOrDefault().Value;
        }

        internal bool Exists(int id)
        {
            return entities.ContainsKey(id);
        }

        internal void Destroy(Entity entity)
        {
            trash.Add(entity);
        }
        internal IOrderedEnumerable<Entity> MembersOf(Entity whom)
        {
            if (!(whom is null))
            {
                return RelativesOf(whom, Entity.Relation.Child);
            }
            return null;
        }
        internal IOrderedEnumerable<Entity> SiblingsOf(Entity whom)
        {
            if (!(whom is null))
            {
                return RelativesOf(whom, Entity.Relation.Sibling);
            }
            return null;
        }
        internal IOrderedEnumerable<Entity> RelativesOf(Entity whom, Entity.Relation relatedAs)
        {
            if (!(whom is null))
            {
                return (from entity in data.entities.entities
                        join relation in whom.relations on entity.Key equals relation.Key
                        where relation.Value == relatedAs
                        select entity.Value).OrderBy(ent => ent.pos);
            }
            return null;
        }

        /// <summary>
        /// get an entity with specified name and specified template
        /// </summary>
        /// <param name="entity">name of the entity</param>
        /// <param name="parent">name of the entity's parent</param>
        /// <returns></returns>
        public Entity Get(string entityName, int parentId)
        {
            return entities.Where(ent =>
                ent.Value.name == entityName &&
                ent.Value.relations.ContainsKey(parentId) &&
                ent.Value.relations[parentId] == Entity.Relation.Parent
            ).First().Value;
        }

        internal int Count()
        {
            return entities.Count;
        }
        internal int ActiveEntities()
        {
            return entities.Where(ent => ent.Value.enabled).Count();
        }

        public Entity GetParentOf(Entity child)
        {
            Entity parent = null;
            if (child.relations.Values.Contains(Entity.Relation.Parent))
            {
                parent = entities[child.relations.FirstOrDefault(relative => relative.Value == Entity.Relation.Parent).Key];
            }
            return parent;
        }
        public bool Contains(Entity entity)
        {
            return entities.Values.Contains(entity);
        }
        public bool AddOrUpdate(Entity entity)
        {
            try
            {
                int trand = data.random.Next(1, 99);
                if (entities.TryAdd(entity.id, entity))
                {
                    entities[entity.id].Init(data);
                    return true;
                }
                entities[entity.id].Update(entity);
                return false;
            }
            catch (Exception)
            {
                if (entities.ContainsKey(entity.id))
                {
                    entities[entity.id].Update(entity);
                    return true;
                }
                return false;
            }
        }
        internal void PointAt(GameObject gameObject)
        {
            if (gameObject.CompareTag("node") && gameObject.TryGetComponent(out UI.Node.Shape shape))
            {
                shape.GotFocus();
            }
        }
        internal void Select(GameObject gameObject)
        {
            if (gameObject.TryGetComponent(out UI.Node.Shape shape))
            {
                shape.Select();
            }
        }
        internal void Select(UI.Node.Shape nodeInFocus)
        {
            if (nodeInFocus is null)
            {
                data.links.ToggleAllLinks();
            }
            else
            {
                nodeInFocus.Select();
            }
        }
        public bool Update(Entity entity)
        {
            return entities.TryUpdate(entity.id, entity, entities[entity.id]);
        }
        public void Update(ConcurrentDictionary<int, Entity> import)
        {
            entities = import;
        }
        public void Import(string entities)
        {
            log.Entry("processing entities: " + entities.Length + " bytes.", Log.Event.ToServer);
            _ = System.Threading.Tasks.Task.Run(() => MonitorEntitiesCreationProgress(data));
            foreach (KeyValuePair<int, Entity> entity in JsonConvert.DeserializeObject<Dictionary<int, Entity>>(entities).OrderByDescending(ent => ent.Value.gm))
            {
                AddOrUpdate(entity.Value);
            }
            log.Entry("done processing entities", Log.Event.ToServer);
        }

        async void MonitorEntitiesCreationProgress(Data data)
        {
            await data.UI.Sleep(2345, 123);
            while (data.forrestIsRunning && data.messenger.CheckLoad() > 0)
            {
                float progress = (float) data.entities.entities.Where(ent => ent.Value.ready).Count() / (float) data.entities.Count();

                data.messenger.Post(new Communication.Message()
                {
                    HUDEvent = UI.HUD.HUD.Event.Progress,
                    number = 0,
                    floats = new List<float> { progress, 1 },
                    message = "Preparing entities", //: " + data.entities.entities.Where(ent => ent.Value.ready).Count() + "/" + data.entities.Count() + " = " + progress,
                    from = data.layouts.current.GetGroot()
                });
                if (progress < 1)
                {
                    await data.UI.Sleep(2345);
                } 
                else
                {
                    break;
                }
            }
        }

        internal void SetShapesCollidersToTriggers(bool setTo = true)
        {
            foreach (Shape shape in entities.Values.Select(ent => ent.containers.GetCurrentShape()))
            {
                if (!(shape is null) && !(shape.container is null))
                {
                    shape.container.SetColliderToTrigger(setTo);
                }
            }
        }
    }
}
