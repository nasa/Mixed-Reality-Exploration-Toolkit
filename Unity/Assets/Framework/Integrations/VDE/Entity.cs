/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
using System.Collections.Generic;
using Assets.VDE.Communication;
using Assets.VDE.Layouts;
using System.Collections;
using Newtonsoft.Json;
using Assets.VDE.UI;
using System.Linq;
using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using UnityEditor;
using Assets.VDE.UI.Group;
using Assets.VDE.UI.Node;

namespace Assets.VDE
{
    /// <summary>
    /// This group of values is importeed from VDE server via websocket and translated into an Entity.
    /// 
    /// An Entity can be a node, a group, or something completely different.
    /// Hence the internal relational tree consist of entities that may form groups.
    /// 
    /// ALAS: this has nothing in common with Unity's Entity Component System's entities. Yet.
    /// Once Unity Entity Component System is mature enough, VDE will be migrated away from GameObjects to ECS.
    /// 
    /// </summary>
    /// 
    public class Entity : IComparable
    {
        /// <summary>
        /// will be parsed to relations
        /// </summary>
        public string r { get; set; }
        /// <summary>
        /// will be parsed to geni
        /// </summary>
        public string g { get; set; }
        /// <summary>
        /// will be parsed to vector
        /// </summary>
        public string v { get; set; }
        public string info { get; set; }
        public string name { get; set; }

        /// <summary>
        /// server allocated ID, not persistent over sessions
        /// </summary>
        public int id { get; set; } = 0;
        /// <summary>
        /// max known depth of geni
        /// </summary>
        public int gm { get; set; }
        /// <summary>
        /// position amongst its group
        /// </summary>
        public int pos { get; set; } = 0;
        /// <summary>
        /// e.g. session count of networks connections
        /// </summary>
        public int count { get; set; } = 0;
        /// <summary>
        /// will be manually parsed to the alfa of the node's color
        /// </summary>
        public float a { get; set; } = 0.009F;
        public Type type { get; set; }
        /// <summary>
        /// for example in case of network layout, variables are passed on like this:
        /// theSubnetThatTheDeviceBelongsTo          = vector[0]
        /// theSubnetSubgroupThatTheDeviceBelongsTo  = vector[1]
        /// devicePositionInLogicalSubgroup          = vector[2]
        /// groupNr                                  = vector[3]
        /// </summary>
        public int[] vectors { get; set; }
        public int[] geni { get; set; }
        public Dictionary<int, Relation> relations = null;

        internal bool
            trash = false,
            ready = false;

        /// <summary>
        /// gameobjects, nodes, (datas)hapes, whatnot that represent this entity in different layouts
        /// </summary>
        internal Containers containers;
        internal IOrderedEnumerable<Entity>
            siblings,
            members;

        public List<Entity> siblingsTMP { get; private set; }
        public List<Entity> membersTMP { get; private set; }

        internal int
            // the ID of this entity's grandparent entity (trunk), whos parent is groot.
            tier2ancestorID = -1;
        internal Entity tier2ancestor;
        internal Entity parent = null;
        public bool shapeless { get; private set; }

        internal int parentID = 0;
        internal Layout waitingForNodeInLayout { get; private set; }
        internal bool isControlObject { get; private set; } = false;
        internal bool IamGROOT { get; private set; } = false;

        public enum Type
        {
            NotSet = 0,
            Node,
            Other,
            Group,
            Network,
            Template,
            Parent,
            Child
        }
        /// <summary>
        /// who is the member of "relations" to THIS entity?
        /// e.g. members of a group show up as children in a group's "relations", while groups are listed as parents in entitie's relations.
        /// </summary>
        public enum Relation
        {
            Template = 0,
            Sibling = 1,
            Parent = 2,
            Child = 3,
            Group = 4,
            Link = 5
        }
        Log log;
        Messenger messenger;
        internal Data data = null;
        internal bool standYourGround, readyForTrigger, triggerIsPulled, readyForTrunkTrigger, enabled;
        /// <summary>
        /// how deep is the entity structure (layers) underneath this entity. for example groot has 0, while nodes have highest DOR.
        /// </summary>
        internal int distanceFromGroot = 0;
        internal int stateOfMind = 0;
        internal Func<IEnumerator> trigger;
        internal bool enlightened;

        public void Init(Data data)
        {
            this.data = data;
            messenger = data.messenger;
            log = new Log("Entity ("+id+":"+pos+")[" + name + "]",messenger);
            containers = new Containers(this);
            _ = Task.Run(WakeyWakey);
        }

        async internal void Update(Entity newerEntity)
        {
            if (newerEntity.id == id)
            {
                pos = newerEntity.pos;
                info = newerEntity.info;
                name = newerEntity.name;
                count = newerEntity.count;
                distanceFromGroot = gm = newerEntity.gm;

                // alpha
                if (a != newerEntity.a)
                {
                    //log.Entry("incoming: " + name + "[" + id + "] diff alpha: " + a + " vs " + newerEntity.a);
                    a = newerEntity.a;
                    if (type == Type.Node && containers.GetCurrentNodeShape(out UI.Node.Shape ns))
                    {
                        ns.SetColor();
                    }
                }
                // relations
                if (r != newerEntity.r)
                {
                    //log.Entry("incoming: " + name + "[" + id + "] diff relakas: " + r + " vs " + newerEntity.r);

                    r = newerEntity.r;
                    Dictionary<int,Relation> novelRelations = JsonConvert.DeserializeObject<Dictionary<int, Relation>>(r);

                    foreach (KeyValuePair<int,Relation> newRelative in novelRelations.Where(rel => !relations.ContainsKey(rel.Key)))
                    {
                        if (
                            newRelative.Value == Relation.Child ||
                            newRelative.Value == Relation.Parent ||
                            newRelative.Value == Relation.Sibling 
                            )
                        {
                            relations.Add(newRelative.Key, newRelative.Value);
                            messenger.SubscribeToEvent(ReceiveEntity, Entities.Event.NewEntity.ToString(), newRelative.Key);
                            messenger.SubscribeToEvent(ReceiveContainer, Layouts.Layouts.LayoutEvent.GotContainer.ToString(), newRelative.Key);
                            data.UI.nodeFactory.GetCreaturesOfEntity(newRelative.Key, ReceiveCreatures);
                        }
                    }
                    await GetMembersAndSiblings();
                }
                if (g != newerEntity.g)
                {
                    g = newerEntity.g;
                    geni = JsonConvert.DeserializeObject<int[]>(g);
                }
                if (v != newerEntity.v)
                {
                    v = newerEntity.v;
                }

                if (containers.GetContainer(out UI.Container container))
                {
                    container.ReSeat();
                }
                // containers.GetContainer().ReSeat();
            }
            messenger.Post(new Message()
            {
                EntityEvent = Entities.Event.ReNewEntity,
                obj = new object[] { this },
                from = this,
                to = parent
            });
        }

        async void WakeyWakey()
        {
            stateOfMind = 1;
            // the wonders of threading - init may happen before the values of the object are set.
            while (data.forrestIsRunning && (string.IsNullOrEmpty(name) || data == null))
            {
                await data.UI.Sleep(12);
            }

            TranslateJsonVars();
            stateOfMind = 2;

            // is this entity immediately under The Groot? if so, it shall be responsible for dragging around other entities underneath / above it.
            // if a layout needs to handle this differently, it shall extend Entity
            if (relations.ContainsKey(0) && relations[0] == Relation.Parent)
            {
                isControlObject = true;
            }

            waitingForNodeInLayout = data.layouts.current;

            if (relations.ContainsValue(Relation.Parent))
            {
                InitializeEntity();

                // if this entity is next in line from groot (a trunk), register it in the messenger so,
                // that triggers could notify its children, when its their time to compact. Or perform other choreographics.
                if (isControlObject)
                {
                    readyForTrunkTrigger = true;
                    messenger.RegisterTrunk(this);
                }
                else
                {
                    // entity needs to know its T2Aid before proceeding
                    while (!GetTier2AncestorID())
                    { 
                        await data.UI.Sleep(123);
                    }
                }
            }
            else if (name == "groot")
            {
                InitializeGroot();
            }
            else
            {
                log.Entry("received a weird entity, that doesnt have a parent, while its not groot.");
            }
            stateOfMind = 3;
        }

        private void TranslateJsonVars()
        {
            relations = JsonConvert.DeserializeObject<Dictionary<int, Relation>>(r);
            vectors = JsonConvert.DeserializeObject<int[]>(v);
            geni = JsonConvert.DeserializeObject<int[]>(g);
            distanceFromGroot = gm;
        }

        private void InitializeEntity()
        {
            parentID = relations.First(rel => rel.Value == Relation.Parent).Key;
            Entity parenting = data.entities.Get(parentID);

            // it may be, that the parent Entity is already existing, but is not yet ready.
            // if it is ready, it has already shouted out its NewEntity message and THIS entity would miss that. 
            // hence it has to check it here.
            if (!(parenting is null) && parenting.ready)
            {
                RegisterParentEntity(parenting);
            }

            messenger.SubscribeToEvent(ReceiveEntity, Entities.Event.NewEntity.ToString(), parentID);
            messenger.SubscribeToEvent(ReceiveContainer, Layouts.Layouts.LayoutEvent.GotContainer.ToString(), parentID);
            messenger.SubscribeToEvent(ReceiveContainer, Layouts.Layouts.LayoutEvent.GotContainer.ToString(), id);
            data.UI.nodeFactory.GetCreaturesOfEntity(parentID, ReceiveCreatures);
        }
        /// <summary>
        /// that singular event, when a groot has been born.
        /// </summary>
        private void InitializeGroot()
        {
            log.Entry("i am GROOT!");
            parentID = -1;
            IamGROOT = true;
            // TODO: review once other layouts are implemented.
            // creating here some dummy siblings, to keep other components sane.
            siblings = data.entities.entities.Where(ent => ent.Key == id).Select(ent => ent.Value).OrderBy(ent => ent.pos);
            members = data.entities.MembersOf(this);

            type = Type.Group;
            shapeless = true;
            messenger.SubscribeToEvent(ReceiveContainer, Layouts.Layouts.LayoutEvent.GotContainer.ToString(), id);
            data.UI.nodeFactory.Request(this, ReceiveContainer);

            messenger.Post(new Message()
            {
                EntityEvent = Entities.Event.NewEntity,
                obj = new object[] { this },
                from = this,
                to = parent
            });

            if (data.forrestIsRunning)
            {
                ready = true;
            }
        }
        /// <summary>
        /// for the groups to be able to schedule their compression, they need to be aware of the ancestor tree,
        /// most importantly the ancestor immediately next to the groot so, that to coordinate the enablings/disabling of the colliders of its subgroups.
        /// </summary>
        private bool GetTier2AncestorID()
        {
            // if T2Aid hasnt been found yet and i'm NOT groot.
            if (tier2ancestorID == -1 && parentID != -1)
            {
                Entity foundAncestor = parent;
                // move up the foodchain until the parent of found ancestor is groot. once found, move on.
                while (!(foundAncestor is null) && !foundAncestor.parent.IamGROOT)
                {
                    foundAncestor = foundAncestor.parent;
                }
                // if the foundAncestor.parent is indeed groot, schedule a trigger for itself.
                if (!(foundAncestor is null) && foundAncestor.parent.IamGROOT)
                {
                    tier2ancestorID = foundAncestor.id;
                    tier2ancestor = foundAncestor;

                    // a child may be ready before it gets its T2A, hence we need to check it here.
                    if (readyForTrigger)
                    {
                        ScheduleForTrigger();
                    }
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// if this is a T2A, its children will schedule their shape-compacting triggers through this function.
        /// </summary>
        internal void ScheduleForTrigger(Func<IEnumerator> trigger = null)
        {
            readyForTrigger = true;
            if (!(tier2ancestor is null))
            {
                this.trigger = trigger;
                tier2ancestor.ScheduleForTrunk();
            }
            else if (isControlObject)
            {
                this.trigger = trigger;
                ScheduleForTrunk();
            }
        }

        private void ScheduleForTrunk()
        {
            readyForTrunkTrigger = true;
        }

        internal void YieldTrigger()
        {   
            if (isControlObject)
            {
                readyForTrunkTrigger = true;
            }
            readyForTrigger = false;
            triggerIsPulled = false;
            if (!(tier2ancestor is null))
            {
                tier2ancestor.YieldTrigger();
            }
        }
        /// <summary>
        /// well, to register the Entity object of this entity's parent, of course :)
        /// sending the Relation.Sibling from VDE server would bloat the traffic. 
        /// hence its calculated here based on parent's relations.
        /// </summary>
        /// <param name="incomingParent"></param>
        private void RegisterParentEntity(Entity incomingParent)
        {
            if (parent is null)
            {
                parent = incomingParent;

                if (relations.Values.Contains(Relation.Parent))
                {
                    foreach (int siblingID in parent.relations.Where(rel => rel.Value == Relation.Child).Select(rel => rel.Key))
                    {
                        if (!relations.ContainsKey(siblingID))
                        {
                            relations.Add(siblingID, Relation.Sibling);
                        }
                    }
                }
                if (parent.parentID == 0)
                {
                    containers.UseDirector();
                }

                _ = GetMembersAndSiblings();

                // if this entity is next in line from groot, dont bother with shape
                // you may want to adjust that per layout requirements
                if (parent.IamGROOT && members.Where(member => member.type == Type.Group).Count() > 0)
                {
                    shapeless = true;
                }
            }
        }
        async Task GetMembersAndSiblings()
        {
            void GMAS()
            {
                siblings = data.entities.SiblingsOf(this);
                members = data.entities.MembersOf(this);
                siblingsTMP = data.entities.SiblingsOf(this).ToList();
                membersTMP = data.entities.MembersOf(this).ToList();
            }

            if (type == Type.Group)
            {
                while (relations.Where(relakas => !data.entities.entities.ContainsKey(relakas.Key)).Any())
                {
                    List<int> ints = relations.Where(rel => !data.entities.entities.ContainsKey(rel.Key)).Select(gdmp => gdmp.Key).ToList();

                    // for functions that "do something" with these relations meanwhile.
                    GMAS();

                    await data.UI.Sleep(1234);
                }
            }
            GMAS();

        }
        private void RegisterEntity(Entity incoming)
        {
            messenger.SubscribeToEvent(ReceiveContainer, Layouts.Layouts.LayoutEvent.GotContainer.ToString(), incoming.id);
            data.UI.nodeFactory.GetCreaturesOfEntity(incoming.id, ReceiveCreatures);
        }
        private void RequestContainer()
        {
            messenger.SubscribeToEvent(ReceiveContainer, Layouts.Layouts.LayoutEvent.GotContainer.ToString(), id);
            data.UI.nodeFactory.Request(this, ReceiveContainer);
        }
        private void AnnounceItself()
        {
            messenger.Post(new Message()
            {
                EntityEvent = Entities.Event.NewEntity,
                obj = new object[] { this },
                from = this,
                to = parent
            });

            // it may happen, that main thread is killed off while entities are still being processed in their threads. 
            if (data.forrestIsRunning)
            {
                ready = true;
            }
        }

        /// <summary>
        /// once this entity knows that the parent entity has a container, it may ask for its own.
        /// while on that, it'll also report to other entities, that it's ready for action.
        /// </summary>
        private void ParentGotContainer(Entity incoming)
        {
            string prefix = name + "[" + id + "]";

            if (parent is null)
            {
                RegisterParentEntity(incoming);
            }

            if (!(parent is null))
            {
                prefix = parent.name + " > " + prefix;
                if (!(parent.parent is null))
                {
                    prefix = parent.parent.name + " > " + prefix;
                    if (!(parent.parent.parent is null))
                    {
                        prefix = parent.parent.parent.name + " > " + prefix;
                    }
                }
            }
            log.SetPrefix(prefix);

            if (!containers.GetContainer(data.layouts.current, out _))
            {
                RequestContainer();
            }
            AnnounceItself();
        }

        internal void AddOrUpdateLink(Link link)
        {
            if (link.source == this)
            {
                if (!relations.ContainsKey(link.destination.id))
                {
                    relations.Add(link.destination.id, Relation.Link);
                }
            }
            else
            {
                if (!relations.ContainsKey(link.source.id))
                {
                    relations.Add(link.source.id, Relation.Link);
                }
            }
        }
        #region callbacks
        internal IEnumerator Inbox(Message message)
        {
            if (message.to == this)
            {
                if (
                    message.LayoutEvent == Layouts.Layouts.LayoutEvent.GotContainer
                    && (
                        message.EventOrigin == Layouts.Layouts.EventOrigin.Group ||
                        message.EventOrigin == Layouts.Layouts.EventOrigin.Node
                    )
                )
                {
                    ProcessContainer(message.obj);
                }
                else if (message.LayoutEvent == Layouts.Layouts.LayoutEvent.Ready)
                {
                    if (message.EventOrigin == Layouts.Layouts.EventOrigin.Node || message.EventOrigin == Layouts.Layouts.EventOrigin.Group)
                    {
                        yield return ReceiveContainer(message.obj);
                    }
                }
                else if (message.EntityEvent == Entities.Event.NewEntity || message.EntityEvent == Entities.Event.ReNewEntity)
                {
                    ReceiveEntity(message.obj);
                }
                else if (message.LayoutEvent == Layouts.Layouts.LayoutEvent.HasSettled)
                {
                    if (
                        relations.ContainsKey(message.from.id) &&
                        relations[message.from.id] == Relation.Child
                        && message.EventOrigin == Layouts.Layouts.EventOrigin.Group
                        && containers.GetCurrentGroupShape(out UI.Group.Shape groupShape)
                        )
                    {
                        if (groupShape.container.CheckIfMemberShapesAreReady())
                        {
                            groupShape.ScheduleUpdate();
                            ScheduleForTrigger();
                        } 
                        if (IamGROOT)
                        {
                            foreach (Entity member in message.from.members.Where(mem => mem.type == Type.Group))
                            {
                                member.containers.GetCurrentGroup().PositionSelfAmongstSiblings();
                            }
                            UI.Group.Container currentGroup = message.from.containers.GetCurrentGroup();
                            currentGroup.TryToRelaxGroupMembers();
                            if (!currentGroup.layout.populated && !members.Where(member => !member.containers.GetCurrentGroup().ready).Any())
                            {
                                messenger.Post(new Message()
                                {
                                    LayoutEvent = Layouts.Layouts.LayoutEvent.LayoutPopulated,
                                    obj = new object[] { currentGroup.layout },
                                    layout = currentGroup.layout,
                                    from = this
                                });
                            }
                            /*
                             * 20210713
                            else if (!members.Where(member => !member.containers.GetCurrentGroup().ready).Any())
                            {
                                data.entities.SetShapesCollidersToTriggers(true);
                            }
                            */
                        }
                    }
                }
            }
        }
        /// <summary>
        /// shall be called from main thread with StartCoroutine
        /// </summary>
        /// <param name="anObject"></param>
        internal IEnumerator ReceiveCreatures(object[] anObject)
        {
            if (anObject.Length == 3 && !(anObject[2] is null) && (anObject[2] as List<object>).Count() > 0)
            {
                Entity incoming = anObject[0] as Entity;
                List<object> creatures = anObject[2] as List<object>;
                foreach (UI.Container container in creatures.Where(beast => beast.GetType().Name == "Container" || beast.GetType().Name == "Group" || beast.GetType().Name == "Node"))
                {
                    yield return ReceiveContainer(new object[] { incoming, container, container.gameObject });
                }
            }
        }
        internal IEnumerator ReceiveContainer(object[] anObject)
        {
            ProcessContainer(anObject);
            yield return true;
        }
        internal void ProcessContainer(object[] anObject)
        {
            Entity incoming = anObject[0] as Entity;

            if (
                anObject.Length == 3 &&
                anObject[0].GetType().Name == "Entity" &&
                !(anObject[1] is null) &&
                (
                    anObject[1].GetType().Name == "Container" ||
                    anObject[1].GetType().Name == "Group" ||
                    anObject[1].GetType().Name == "Node"
                )
            )
            {
                UI.Container incomingContainer = anObject[1] as UI.Container;

                if (incoming == this)
                {
                    switch (incoming.type)
                    {
                        case Type.Node:
                            containers.AddContainer(anObject[1] as UI.Node.Container);
                            break;
                        case Type.Child:
                            containers.AddContainer(anObject[1] as UI.Node.Container);
                            break;
                        case Type.Group:
                            containers.AddContainer(anObject[1] as UI.Group.Container);
                            break;
                        case Type.Parent:
                            containers.AddContainer(anObject[1] as UI.Group.Container);
                            break;
                        default:
                            break;
                    }
                    // if this is first container for this entity, set its state of "ready" to true
                    if (containers.containers.Count() == 1)
                    {
                        enabled = true;
                    }
                }
                else if (incoming.id == parentID && anObject[2].GetType().Name == "GameObject")
                {
                    // TODO: this will not work correctly in multiple layouts.
                    ParentGotContainer(incoming);
                }
                // if the incoming entity is a relative (of this group)
                else if (relations.ContainsKey(incoming.id))
                {
                    UI.Node.Container currentNode = containers.GetCurrentNode();
                    UI.Group.Container currentGroup = containers.GetCurrentGroup();

                    if (
                        !(currentGroup is null)
                        && !(anObject[1] as UI.Group.Container is null)
                        && relations[incoming.id] == Relation.Child)
                    {
                        currentGroup.AdoptMember(anObject[1] as UI.Group.Container);
                    }
                    else if (
                        !(currentGroup is null)
                        && !(anObject[1] as UI.Node.Container is null)
                        && relations[incoming.id] == Relation.Child)
                    {
                        currentGroup.AdoptMember(anObject[1] as UI.Node.Container);
                    }
                    // shouldnt really make it here.
                    else if (
                        !(currentGroup is null)
                        && !(anObject[1] as UI.Group.Container is null)
                        && relations[incoming.id] == Relation.Parent)
                    {
                        currentGroup.AdoptParent(incoming, anObject[1] as UI.Group.Container);
                    }
                    // currentGROUP
                    else if (
                        !(currentGroup is null)
                        && (
                               !(anObject[1] as UI.Group.Container is null)
                            || !(anObject[1] as UI.Node.Container is null)
                        )
                        && relations[incoming.id] == Relation.Sibling)
                    {
                        if (incomingContainer.type == Type.Group)
                        {
                            currentGroup.AdoptSibling(incoming, anObject[1] as UI.Group.Container);
                        }
                        else if (incomingContainer.type == Type.Node || incomingContainer.type == Type.Child)
                        {
                            //currentGroup.AdoptSibling(incoming, anObject[1] as UI.Node.Container);
                        }
                    }
                    else if (
                        !(currentNode is null)
                        && !(anObject[1] as UI.Group.Container is null)
                        && relations[incoming.id] == Relation.Parent)
                    {
                        currentNode.AdoptParent(incoming, anObject[1] as UI.Group.Container);
                    }
                    // currentNODE
                    else if (
                        !(currentNode is null)
                        && (
                               !(anObject[1] as UI.Group.Container is null)
                            || !(anObject[1] as UI.Node.Container is null)
                        )
                        && relations[incoming.id] == Relation.Sibling)
                    {
                        // this should be a really odd case though..
                        if (incomingContainer.type == Type.Group)
                        {
                            //currentNode.AdoptSibling(incoming, anObject[1] as UI.Group.Container);
                        }
                        else if (incomingContainer.type == Type.Node || incomingContainer.type == Type.Child)
                        {
                            //currentNode.AdoptSibling(incoming, anObject[1] as UI.Node.Container);
                        }
                    }
                }
                else
                {
                    log.Entry("unexpected incoming container: " + anObject[1].GetType().Name + " from: " + (anObject[0] as Entity).name);
                }
            }
            else
            {
                log.Entry("unexpected incoming container: " + anObject[1].GetType().Name + " from: " + (anObject[0] as Entity).name);
            }
        }
        /// <summary>
        /// entity becomes available before it has a shape & gameobject
        /// </summary>
        /// <param name="anObject"></param>
        internal IEnumerator ReceiveEntity(object[] anObject)
        {
            if (anObject.Length > 0 && anObject[0].GetType().Name == "Entity")
            {
                Entity incoming = anObject[0] as Entity;
                if (incoming.id == parentID)
                {
                    RegisterParentEntity(incoming);
                }
                else if (relations.Keys.Contains(incoming.id) && relations[incoming.id] == Relation.Sibling)
                {
                    RegisterEntity(incoming);
                }
                else if (relations.Keys.Contains(incoming.id) && relations[incoming.id] == Relation.Child)
                {
                    RegisterEntity(incoming);
                }
                // speculatively add an incoming entity to own relations, anticipating an update from the server later on.
                else if (incoming.relations.ContainsKey(id) && !relations.ContainsKey(incoming.id))
                {
                    // what the incoming entity thinks of me
                    switch (incoming.relations[id])
                    {
                        case Relation.Sibling:
                            relations.Add(incoming.id, Relation.Sibling);
                            yield return GetMembersAndSiblings();
                            RegisterEntity(incoming);
                            ProcessContainer(anObject);
                            break;
                        case Relation.Parent:
                            if (relations.ContainsValue(Relation.Parent))
                            {
                                log.Entry("ReceiveEntity() got " + incoming.name + ", that's trying to pose as parent, while i already have " + parent.name + " as one.");
                            }
                            else
                            {
                                relations.Add(incoming.id, Relation.Child);
                                yield return GetMembersAndSiblings();
                                RegisterParentEntity(incoming);
                                ProcessContainer(anObject);
                            }
                            break;
                        case Relation.Child:
                            relations.Add(incoming.id, Relation.Parent);
                            yield return GetMembersAndSiblings();
                            RegisterEntity(incoming);
                            ProcessContainer(anObject);
                            break;
                        default:
                            log.Entry("unexpected entity received: " + incoming.name);
                            break;
                    }
                }
            }
            else
            {
                log.Entry("unexpected object received: " + anObject[0].GetType().Name);
            }
            yield return true;
        }
        internal void SetVisibility(bool state)
        {
            enabled = state;
            if(containers.GetContainer(out UI.Container currentContainer))
            {
                currentContainer.SetState(state);
            }
        }
#endregion
        public int CompareTo(object obj)
        {
            if (obj == null) return 1;
            Entity nodeToCompare = (Entity)obj;
            if (nodeToCompare.name != null)
            {
                return name.CompareTo(nodeToCompare.name);
            }
            else
            {
                return 1;
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (ReferenceEquals(obj, null))
            {
                return false;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return id;
        }

        public static bool operator ==(Entity left, Entity right)
        {
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(Entity left, Entity right)
        {
            return !(left == right);
        }

        public static bool operator <(Entity left, Entity right)
        {
            return ReferenceEquals(left, null) ? !ReferenceEquals(right, null) : left.CompareTo(right) < 0;
        }

        public static bool operator <=(Entity left, Entity right)
        {
            return ReferenceEquals(left, null) || left.CompareTo(right) <= 0;
        }

        public static bool operator >(Entity left, Entity right)
        {
            return !ReferenceEquals(left, null) && left.CompareTo(right) > 0;
        }

        public static bool operator >=(Entity left, Entity right)
        {
            return ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.CompareTo(right) >= 0;
        }
    }
}
