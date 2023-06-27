// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.Configuration;

namespace GOV.NASA.GSFC.XR.MRET
{
    /// <remarks>
    /// History:
    /// 13 Sep 2021: Created (Jeffrey Hosler)
    /// </remarks>
	///
	/// <summary>
	/// Serializable Group object<br>
	///
    /// Author: Jeffrey Hosler
	/// </summary>
    /// 
    /// <seealso cref="IGroup"/>
	/// 
	public abstract class Group<GT, GC, GI, CT, CC, CI> : VersionedMRETBehaviour<GT>, IGroup<GT, GI, CT, CI>
        where GT : GroupType, new()
        where GC : Group<GT, GC, GI, CT, CC, CI>, new()
        where GI : IGroup
        where CT : IdentifiableType, new()
        where CC : Identifiable<CT>, new()
        where CI : IIdentifiable
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(Group<GT, GC, GI, CT, CC, CI>);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private GT serializedGroup;

        #region IGroup
        /// <seealso cref="IGroup.group"/>
        public int group { get; private set; }

        /// <seealso cref="IGroup.CreateSerializedType"/>
        GroupType IGroup.CreateSerializedType() => CreateSerializedType();

        /// <seealso cref="IGroup.Deserialize(GroupType, Action{bool, string})"/>
        void IGroup.Deserialize(GroupType serialized, Action<bool, string> onFinished)
        {
            Deserialize(serialized as GT, onFinished);
        }

        /// <seealso cref="IGroup.Serialize(GroupType, Action{bool, string})"/>
        void IGroup.Serialize(GroupType serialized, Action<bool, string> onFinished)
        {
            Serialize(serialized as GT, onFinished);
        }

        /// <seealso cref="IGroup{GT, GI, CT, CI}.Children"/>
        public CI[] Children
        {
            get
            {
                //return gameObject.GetComponents<CI>();

                List<CI> childList = new List<CI>();
                foreach (Transform t in transform)
                {
                    CI child = t.GetComponent<CI>();
                    if (child != null)
                    {
                        childList.Add(child);
                    }
                }
                return childList.ToArray();
            }
        }

        /// <seealso cref="IGroup.Children"/>
        IIdentifiable[] IGroup.Children => Children as IIdentifiable[];

        /// <seealso cref="IGroup{GT, GI, CT, CI}.ChildGroups"/>
        public GI[] ChildGroups
        {
            get
            {
                /*
                // GetComponentsInChildren will include the parent (this group).
                // We only want the children, so remove the component in this game object
                GI[] childGroups = gameObject.GetComponents<GI>();
                HashSet<GI> hashSet = new HashSet<GI>(childGroups);
                hashSet.Remove(gameObject.GetComponent<GI>());
                return hashSet.ToArray();
                */

                List<GI> childGroupList = new List<GI>();
                foreach (Transform t in transform)
                {
                    GI childGroup = t.GetComponent<GI>();
                    if (childGroup != null)
                    {
                        childGroupList.Add(childGroup);
                    }
                }
                return childGroupList.ToArray();
            }
        }

        /// <seealso cref="IGroup.ChildGroups"/>
        IGroup[] IGroup.ChildGroups => ChildGroups as IGroup[];
        #endregion IGroup

        #region MRETUpdateBehaviour
        /// <seealso cref="MRETBehaviour.IntegrityCheck"/>
        protected override IntegrityState IntegrityCheck()
        {
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            return (
                (state == IntegrityState.Failure) // TODO: || (MyRequiredRef == null)

                    ? IntegrityState.Failure   // Fail is base class fails or anything is null
                    : IntegrityState.Success); // Otherwise, our integrity is valid
        }

        /// <seealso cref="MRETBehaviour.MRETAwake"/>
        protected override void MRETAwake()
        {
            base.MRETAwake();

            // Set the defaults
            group = 0;
        }

        /// <seealso cref="MRETBehaviour.MRETOnDestroy"/>
        protected override void MRETOnDestroy()
        {
            // Take the inherited behavior
            base.MRETOnDestroy();

            // Destroy the group
            DestroyGroup();
        }
        #endregion MRETUpdateBehaviour

        #region Serialization
        /// <seealso cref="VersionedMRETBehaviour{T}.Deserialize(T, SerializationState)"/>
        protected override IEnumerator Deserialize(GT serialized, SerializationState deserializationState)
        {
            StartCoroutine(base.Deserialize(serialized, deserializationState));

            // Wait for the coroutine to complete
            while (!deserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (deserializationState.IsError) yield break;

            // Start with an incomplete indicator
            deserializationState.Clear();

            // Process this object specific deserialization

            // Save the serialized reference
            serializedGroup = serialized;

            // Deserialize the group
            group = serialized.group;

            // Deserialize the children
            VersionedType[] items = ReadSerializedItems(serializedGroup);
            if (items != null)
            {
                foreach (object serializedChild in items)
                {
                    // Deserialize based upon the item
                    if (serializedChild is GT)
                    {
                        // Child Group
                        Log(typeof(GC).Name + " deserialization starting.", nameof(Deserialize));

                        // Perform the deserialization, but supply out delegate function so that subclasses can
                        // change how the instantiation/deserialization occurs
                        VersionedSerializationState<GC> childGroupDeserializationState = new VersionedSerializationState<GC>();
                        StartCoroutine(DeserializeVersioned(serializedChild as GT, null, gameObject.transform,
                            childGroupDeserializationState, InstantiateSerializable));

                        // Wait for the coroutine to complete
                        while (!childGroupDeserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                        // Update the deserialization state
                        deserializationState.Update(childGroupDeserializationState);

                        // Make sure the resultant child group type is not null
                        if (childGroupDeserializationState.versioned is null)
                        {
                            deserializationState.Error("Deserialized child group cannot be null, denoting a possible internal issue.");
                        }

                        // If the group deserialization failed, there's no point in continuing. Something is wrong
                        if (deserializationState.IsError) yield break;

                        // Notify subclasses we completed deserialization
                        AfterDeserialization(childGroupDeserializationState.versioned);

                        Log(typeof(GC).Name + " deserialization complete.", nameof(Deserialize));
                    }
                    else if (serializedChild is CT)
                    {
                        // Child
                        Log(typeof(CC).Name + " deserialization starting.", nameof(Deserialize));

                        // Perform the deserialization, but supply out delegate function so that subclasses can
                        // change how the instantiation/deserialization occurs
                        VersionedSerializationState<CC> childDeserializationState = new VersionedSerializationState<CC>();
                        StartCoroutine(DeserializeVersioned(serializedChild as CT, null, gameObject.transform,
                            childDeserializationState, InstantiateSerializable));

                        // Wait for the coroutine to complete
                        while (!childDeserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                        // Update the deserialization state
                        deserializationState.Update(childDeserializationState);

                        // Make sure the resultant child group type is not null
                        if (childDeserializationState.versioned is null)
                        {
                            deserializationState.Error("Deserialized child cannot be null, denoting a possible internal issue.");
                        }

                        // If the group deserialization failed, there's no point in continuing. Something is wrong
                        if (deserializationState.IsError) yield break;

                        // Notify subclasses we completed deserialization
                        AfterDeserialization(childDeserializationState.versioned);

                        Log(typeof(CC).Name + " deserialization complete.", nameof(Deserialize));
                    }
                }
            }

            // Record the deserialization state as complete
            deserializationState.complete = true;

            yield return null;
        }

        /// <seealso cref="VersionedMRETBehaviour{T}.Serialize(T, SerializationState)"/>
        protected override IEnumerator Serialize(GT serialized, SerializationState serializationState)
        {
            StartCoroutine(base.Serialize(serialized, serializationState));

            // Wait for the coroutine to complete
            while (!serializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) yield break;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Serialize the group
            serialized.group = group;

            // Start with a null child array 
            List<VersionedType> serializedItemsList = new List<VersionedType>();
            WriteSerializedItems(serialized, null);

            // Serialize the child objects first, but only assign a value if we have children
            CI[] children = Children;
            if (children.Length > 0)
            {
                SerializationState childSerializationState = new SerializationState();

                // Attempt to serialize out each child
                foreach (CI child in children)
                {
                    // Get the serialized type instance
                    var serializedChild = child.CreateSerializedType() as CT;

                    // This should be a safe cast from the interface to the instance
                    CC serializable = child as CC;

                    //Log(typeof(CC).Name + " serialization starting.", nameof(Serialize));

                    // Notify subclasses we are about to perform serialization
                    BeforeSerialization(serializable);

                    // Perform the serialization
                    StartCoroutine(serializable.SerializeWithLogging(serializedChild, childSerializationState));

                    // Wait for the coroutine to complete
                    while (!childSerializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                    // Record the serialization state
                    serializationState.Update(childSerializationState);

                    // If the child serialization failed, there's no point in continuing. Something is wrong
                    if (serializationState.IsError) yield break;

                    // Notify subclasses we completed serialization
                    AfterSerialization(serializable);

                    // Add the serialized child to the list
                    serializedItemsList.Add(serializedChild);

                    //Log(typeof(CC).Name + " serialization complete.", nameof(Serialize));

                    // Clear the state
                    childSerializationState.Clear();
                }

                // Write out the serialized child objects
                WriteSerializedItems(serialized, serializedItemsList.ToArray());
            }

            // If there were no child objects, try to serialize the child groups
            if (ReadSerializedItems(serialized) == null)
            {
                // Serialize the child groups, but only assign a value if we have any
                GI[] childGroups = ChildGroups;
                if (childGroups.Length > 0)
                {
                    SerializationState childGroupSerializationState = new SerializationState();

                    foreach (GI childGroup in childGroups)
                    {
                        // Get the serialized type instance
                        var serializedChildGroup = childGroup.CreateSerializedType() as GT;

                        // This should be a safe cast from the interface to the instance
                        GC serializable = childGroup as GC;

                        //Log(typeof(GC).Name + " serialization starting.", nameof(Serialize));

                        // Notify subclasses we are about to perform serialization
                        BeforeSerialization(serializable);

                        // Perform the serialization
                        StartCoroutine(serializable.SerializeWithLogging(serializedChildGroup, childGroupSerializationState));

                        // Wait for the coroutine to complete
                        while (!childGroupSerializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                        // Record the serialization state
                        serializationState.Update(childGroupSerializationState);

                        // If the child serialization failed, there's no point in continuing. Something is wrong
                        if (serializationState.IsError) yield break;

                        // Notify subclasses we completed serialization
                        AfterSerialization(serializable);

                        // Add the serialized child group to the list
                        serializedItemsList.Add(serializedChildGroup);

                        //Log(typeof(GC).Name + " serialization complete.", nameof(Serialize));

                        // Clear the state
                        childGroupSerializationState.Clear();
                    }

                    // Write out the serialized child object group
                    WriteSerializedItems(serialized, serializedItemsList.ToArray());
                }
            }

            // Save the final serialized reference
            serializedGroup = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;

            yield return null;
        }

        /// <summary>
        /// Instantiates and deserializes the serialized child. Available for subclasses
        /// to override for altered behavior.
        /// </summary>
        /// <param name="serializedChild">The serialized child reference</param>
        /// <param name="go">The <code>GameObject</code> to add the child to, or null if
        ///     one should be created</param>
        /// <param name="parent">The parent <code>Transform</code> for the game object</param>
        /// <param name="onFinished">The <code>Action</code> to be called upon completion. The
        ///     argument will be the deserialized instance, or null on failure.</param>
        protected virtual void InstantiateSerializable(CT serializedChild, GameObject go, Transform parent,
            Action<CC> onFinished = null)
        {
            MRET.ProjectManager.InstantiateObject(serializedChild,
                null, gameObject.transform, onFinished);
        }

        /// <summary>
        /// Instantiates the serialized child group with the supplied serialized configuration.
        /// Available for subclasses to override for altered behavior.
        /// </summary>
        /// <param name="serializedChildGroup">The serialized child group configuration</param>
        /// <param name="go">The <code>GameObject</code> to add the child group to, or null if
        ///     one should be created</param>
        /// <param name="parent">The parent <code>Transform</code> for the game object</param>
        /// <param name="onFinished">The <code>Action</code> to be called upon completion. The
        ///     argument will be the deserialized instance, or null on failure.</param>
        protected virtual void InstantiateSerializable(GT serializedChildGroup, GameObject go, Transform parent,
            Action<GC> onFinished = null)
        {
            MRET.ProjectManager.InstantiateObject(serializedChildGroup,
                null, gameObject.transform, onFinished);
        }

        /// <summary>
        /// Reads the array of group items from the serialized group reference
        /// </summary>
        /// <param name="serializedGroup">The serialized group reference for reading</param>
        /// <returns>The read <code>VersionedType[]</code> array of items, or null if none</returns>
        protected abstract VersionedType[] ReadSerializedItems(GT serializedGroup);

        /// <summary>
        /// Writes the array of group items to the serialized group reference
        /// </summary>
        /// <param name="serializedGroup">The serialized group reference for writing</param>
        /// <param name="serializedItems">A <code>VersionedType[]</code> array of items to write, or null if none</param>
        protected abstract void WriteSerializedItems(GT serializedGroup, VersionedType[] serializedItems);

        /// <summary>
        /// Called prior to deserialization of a child group serializable.
        /// Available for subclasses to derive behavior
        /// </summary>
        /// <param name="serializable">The child group serializable instance</param>
//        protected virtual void BeforeDeserialization(GC serializable) { }

        /// <summary>
        /// Called after deserialization of a child group serializable.
        /// Available for subclasses to derive behavior
        /// </summary>
        /// <param name="serializable">The child group serializable instance</param>
        protected virtual void AfterDeserialization(GC serializable) { }

        /// <summary>
        /// Called prior to deserialization of a child serializable.
        /// Available for subclasses to derive behavior
        /// </summary>
        /// <param name="serializable">The child serializable instance</param>
//        protected virtual void BeforeDeserialization(CC serializable) { }

        /// <summary>
        /// Called after deserialization of a child serializable.
        /// Available for subclasses to derive behavior
        /// </summary>
        /// <param name="serializable">The child serializable instance</param>
        protected virtual void AfterDeserialization(CC serializable) { }

        /// <summary>
        /// Called prior to serialization of a child group serializable.
        /// Available for subclasses to derive behavior
        /// </summary>
        /// <param name="serializable">The child group serializable instance</param>
        protected virtual void BeforeSerialization(GC serializable) { }

        /// <summary>
        /// Called after serialization of a child group serializable.
        /// Available for subclasses to derive behavior
        /// </summary>
        /// <param name="serializable">The child group serializable instance</param>
        protected virtual void AfterSerialization(GC serializable) { }

        /// <summary>
        /// Called prior to serialization of a child serializable.
        /// Available for subclasses to derive behavior
        /// </summary>
        /// <param name="serializable">The child serializable instance</param>
        protected virtual void BeforeSerialization(CC serializable) { }

        /// <summary>
        /// Called after serialization of a child serializable.
        /// Available for subclasses to derive behavior
        /// </summary>
        /// <param name="serializable">The child serializable instance</param>
        protected virtual void AfterSerialization(CC serializable) { }

        #endregion Serialization

        /// <summary>
        /// Called prior to a child object being destroyed. This method is available
        /// to subclasses to perform any necessary processing prior to the child being
        /// destroyed.<br>
        /// </summary>
        /// <param name="child">The child being destroyed</param>
        protected virtual void BeforeDestroyChild(CI child)
        {
        }

        /// <summary>
        /// Destroys the supplied child.
        /// </summary>
        /// <param name="child">The child to destroy</param>
        protected void DestroyChild(CI child)
        {
            // Check for a valid reference
            if (child != null)
            {
                // Provide the child to subclasses prior to being destroyed
                BeforeDestroyChild(child);

                // Destroy the child game object
                Destroy(child.gameObject);

                // Destroy the child object
                Destroy(child as UnityEngine.Object);
            }
        }

        /// <summary>
        /// Destroys the child objects directly under this object instance.<br>
        /// </summary>
        protected void DestroyChildren()
        {
            foreach (Transform transform in transform)
            {
                CI[] children = transform.GetComponents<CI>();
                foreach (CI child in children)
                {
                    if (child != null)
                    {
                        // Destroy the child object
                        DestroyChild(child);
                    }
                }
            }
        }

        /// <summary>
        /// Called prior to a child group being destroyed. This method is available
        /// to subclasses to perform any necessary processing prior to the child group
        /// being destroyed.<br>
        /// </summary>
        /// <param name="childGroup">The child group being destroyed</param>
        protected virtual void BeforeDestroyChildGroup(GI childGroup)
        {
        }

        /// <summary>
        /// Destroys the supplied child group.
        /// </summary>
        /// <param name="childGroup">The child group to destroy</param>
        protected void DestroyChildGroup(GI childGroup)
        {
            // Check for a valid reference
            if (childGroup != null)
            {
                // Provide the child to subclasses prior to being destroyed
                BeforeDestroyChildGroup(childGroup);

                // Destroy all child groups in the group
                childGroup.DestroyGroup();

                // Destroy the child group
                Destroy(childGroup as UnityEngine.Object);
            }
        }

        /// <summary>
        /// Destroys the child groups directly under this object instance.<br>
        /// </summary>
        protected void DestroyChildGroups()
        {
            foreach (Transform transform in transform)
            {
                GI[] childGroups = transform.GetComponents<GI>();
                foreach (GI childGroup in childGroups)
                {
                    if (childGroup != null)
                    {
                        // Destroy the child group
                        DestroyChildGroup(childGroup);
                    }
                }
            }
        }

        /// <summary>
        /// Called prior to this group being destroyed. This method is available
        /// to subclasses to perform any necessary processing prior to the group
        /// being destroyed.<br>
        /// </summary>
        protected virtual void BeforeDestroyGroup()
        {
        }

        /// <summary>
        /// Destroys this group.
        /// </summary>
        public void DestroyGroup(bool destroyGameObject = false)
        {
            // Provide the child to subclasses prior to being destroyed
            BeforeDestroyGroup();

            // Destroy all child groups in the group
            DestroyChildGroups();

            // Destroy the children in the group
            DestroyChildren();

            // Destroy the gameobject
            if (destroyGameObject) Destroy(gameObject);
        }

    }

}
