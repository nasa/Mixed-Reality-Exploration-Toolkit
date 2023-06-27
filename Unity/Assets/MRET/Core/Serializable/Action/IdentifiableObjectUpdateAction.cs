// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ComponentModel;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.Action
{
    /// <remarks>
    /// History:
    /// 3 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// Generic IdentifiableObjectUpdateAction
	///
	/// An identifiable object update action in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
    public class IdentifiableObjectUpdateAction<T, I> :
        IdentifiableObjectAction<T, I>,
        IIdentifiableObjectUpdateAction<T, I>
        where T : UpdateIdentifiableObjectActionType, new()
        where I : IIdentifiable
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(IdentifiableObjectUpdateAction<T,I>);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private UpdateIdentifiableObjectActionType serializedUpdateAttributeAction;

        /// <summary>
        /// Dictionary of maintained attributes being updated.
        /// </summary>
        private Dictionary<string, string> attributes = new Dictionary<string, string>();

        #region IIdentifiableObjectUpdateAction
        /// <seealso cref="IIdentifiableObjectUpdateAction.CreateSerializedType"/>
        UpdateIdentifiableObjectActionType IIdentifiableObjectUpdateAction.CreateSerializedType() => CreateSerializedType();

        /// <seealso cref="IIdentifiableObjectUpdateAction.Attributes"/>
        public string[] Attributes
        {
            get
            {
                return attributes.Keys.ToArray();
            }
        }

        /// <seealso cref="IIdentifiableObjectUpdateAction.SerializedAction"/>
        UpdateIdentifiableObjectActionType IIdentifiableObjectUpdateAction.SerializedAction => SerializedAction;

        /// <seealso cref="IIdentifiableObjectUpdateAction.Deserialize(UpdateIdentifiableObjectActionType, Action{bool, string})"/>
        void IIdentifiableObjectUpdateAction.Deserialize(UpdateIdentifiableObjectActionType serialized, Action<bool, string> onFinished)
        {
            Deserialize(serialized as T, onFinished);
        }

        /// <seealso cref="IIdentifiableObjectUpdateAction.Serialize(UpdateIdentifiableObjectActionType, Action{bool, string})"/>
        void IIdentifiableObjectUpdateAction.Serialize(UpdateIdentifiableObjectActionType serialized, Action<bool, string> onFinished)
        {
            Serialize(serialized as T, onFinished);
        }
        #endregion IIdentifiableObjectUpdateAction

        #region Serialization
        /// <summary>
        /// Asynchronously Deserializes the supplied serialized attribute and updates the
        /// supplied state with the deserialization state.
        /// </summary>
        /// <param name="serializedAttribute">The serialized <code>ActionAttributeType</code> attribute</param>
        /// <param name="attributeDeserializationState">The <code>SerializationState</code> to populate with the deserialized state.</param>
        /// 
        /// <see cref="SerializationState"/>
        protected virtual void DeserializeAttribute(ActionAttributeType serializedAttribute, SerializationState attributeDeserializationState)
        {
            // Determine if the attribute name is valid
            if (!string.IsNullOrEmpty(serializedAttribute.AttributeName))
            {
                string attributeName = serializedAttribute.AttributeName;

                // Use reflection to determine if the attribute is valid
                if (ActionObject != null)
                {
                    // TODO: Will this return the interface of the object type?
                    // Is that what we want or do we want to apply updates to the GameObject?
                    Type identifiableObjectType = ActionObject.GetType();

                    // Attempt to obtain the attribute property info
                    PropertyInfo attribute = identifiableObjectType.GetProperty(attributeName);
                    if (attribute == null)
                    {
                        // Error condition
                        attributeDeserializationState.Error("Attribute does not exist in the " +
                            "ActionObject: " + attributeName);
                        return;
                    }

                    // Deserialize the value
                    string value = serializedAttribute.Value;

                    // Convert the value string to the correct type
                    Type attributeType = attribute.PropertyType;
                    TypeConverter typeConverter = TypeDescriptor.GetConverter(attributeType);
                    try
                    {
                        object valueObj = typeConverter.ConvertFromString(value);
                        attribute.SetValue(ActionObject, value);

                        // Store the attribute in our dictionary (only save the value string)
                        attributes.Add(attributeName, value);
                    }
                    catch (NotSupportedException)
                    {
                        // Error condition
                        attributeDeserializationState.Error("Attribute assignment failed. " +
                            "Conversion is not supported when assigning " +
                            "'" + value + "' to ActionObject." + attributeName);
                        return;
                    }
                }
                else
                {
                    // Error condition
                    attributeDeserializationState.Error("ActionObject reference is invalid");
                }
            }
            else
            {
                // Error condition
                attributeDeserializationState.Error("Specified attribute name is not defined");
                return;
            }

            // Mark as complete
            attributeDeserializationState.complete = true;
        }

        /// <seealso cref="IdentifiableObjectAction{T,I}.Deserialize(T, SerializationState)"/>
        protected override void Deserialize(T serialized, SerializationState deserializationState)
        {
            base.Deserialize(serialized, deserializationState);

            // If the parent failed, there's no point in continuing
            if (deserializationState.IsError) return;

            // Start with an incomplete indicator
            deserializationState.Clear();

            // Save the serialized reference
            serializedUpdateAttributeAction = serialized;

            // Clear the attributes
            attributes.Clear();

            // Deserialize all the attributes we are updating
            foreach (ActionAttributeType serializedAttribute in serialized.Attributes)
            {
                // Process this object specific deserialization
                SerializationState deserializedAttributeState = new SerializationState();
                DeserializeAttribute(serializedAttribute, deserializedAttributeState);

                // Record the deserialization state
                deserializationState.Update(deserializedAttributeState);

                // If the attribute name loading failed, exit with an error
                if (deserializationState.IsError) return;
            }

            // Record the deserialization state as complete
            deserializationState.complete = true;
        }

        /// <summary>
        /// Asynchronously Serializes the supplied attributes and updates the supplied
        /// state with the serialization state.
        /// </summary>
        /// <param name="serializedAttributes">The list of <code>ActionAttributeType</code> serialized attribute</param>
        /// <param name="attributesSerializationState">The <code>SerializationState</code> to populate with the serialized state.</param>
        /// 
        /// <see cref="SerializationState"/>
        protected virtual void SerializeAttributes(ref ActionAttributeType[] serializedAttributes, SerializationState attributesSerializationState)
        {
            // Mark as incomplete
            attributesSerializationState.complete = false;

            // Make sure we have a valid ActionObject reference
            if (ActionObject != null)
            {
                // Use reflection to determine if the attribute is valid
                // TODO: Will this return the interface of the object type?
                // Is that what we want or do we want to apply updates to the GameObject?
                Type identifiableObjectType = ActionObject.GetType();

                // Start with a fresh list of serialized attributes
                List<ActionAttributeType> serializedAttributeList = new List<ActionAttributeType>();

                // Loop through our list of attribute names updating the associated value
                foreach (string attributeName in Attributes)
                {
                    // Extract the property information
                    PropertyInfo attribute = identifiableObjectType.GetProperty(attributeName);
                    if (attribute != null)
                    {
                        // Extract the value
                        object value = attribute.GetValue(ActionObject);
                        if (value == null)
                        {
                            // Error condition
                            attributesSerializationState.Error("Value for attribute '" +
                                attributeName + "' in ActionObject is invalid: " + value);
                            return;
                        }

                        // Update our internal attributes with the current value
                        attributes[attributeName] = value.ToString();

                        // Create a serialized attribute to hold the attribute information
                        ActionAttributeType serializedAttribute = new ActionAttributeType()
                        {
                            AttributeName = attributeName,
                            Value = attributes[attributeName]
                        };

                        // Add the serialized attribute to the list
                        serializedAttributeList.Add(serializedAttribute);
                    }
                    else
                    {
                        // Error condition
                        attributesSerializationState.Error("Attribute name reference " +
                            "is invalid for ActionObject: " + attributeName);
                        return;
                    }
                }

                // Assign the serialized attributes
                serializedAttributes = serializedAttributeList.ToArray();
            }
            else
            {
                // Error condition
                attributesSerializationState.Error("ActionObject reference is invalid");
                return;
            }

            // Mark as complete
            attributesSerializationState.complete = true;
        }

        /// <seealso cref="IdentifiableObjectAction{T,I}.Serialize(T, SerializationState)"/>
        protected override void Serialize(T serialized, SerializationState serializationState)
        {
            base.Serialize(serialized, serializationState);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) return;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Clear the serialized attribute list because we will build a new one
            serialized.Attributes = null;

            // Process this object specific serialization as a new coroutine in order to trap yield breaks
            SerializationState serializedAttributeState = new SerializationState();
            ActionAttributeType[] serializedAttributes = null;
            SerializeAttributes(ref serializedAttributes, serializedAttributeState);
            serialized.Attributes = serializedAttributes;

            // Record the serialization state
            serializationState.Update(serializedAttributeState);

            // If the attributes loading failed, exit with an error
            if (serializationState.IsError) return;

            // Save the final serialized reference
            serializedUpdateAttributeAction = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;
        }
        #endregion Serialization

        /// <seealso cref="Versioned{T}.Initialize"/>
        protected override void Initialize()
        {
            base.Initialize();

            // Set the defaults
            attributes.Clear();
        }

        /// <summary>
        /// Constructor for the <code>IdentifiableObjectUpdateAction</code>
        /// </summary>
        /// <param name="serializedAction">The serialized action</param>
        /// <seealso cref="IdentifiableObjectAction{T,C}.IdentifiableObjectAction(T)"/>
        public IdentifiableObjectUpdateAction(T serializedAction) : base(serializedAction)
        {
        }

        /// <summary>
        /// Constructor for the <code>IdentifiableObjectUpdateAction</code>
        /// </summary>
        /// <param name="identifiableObject">The <code>IIdentifiable</code> associated with this action</param>
        /// <param name="attributes">The <code>Dictionary</code> of attributes to update</param>
        /// <seealso cref="IdentifiableObjectAction{T,C}.IdentifiableObjectAction(IIdentifiable)"/>
        public IdentifiableObjectUpdateAction(I identifiableObject, Dictionary<string, string> attributes) :
            base(identifiableObject)
        {
            // Assign the unique settings for this action
            foreach (string key in attributes.Keys)
            {
                this.attributes.Add(key, attributes[key]);
            }
        }
    }

    /// <summary>
    /// Provides a non-generic implementation for the generic IdentifiableObjectUpdateAction class
    /// </summary>
    public class IdentifiableObjectUpdateAction : IdentifiableObjectUpdateAction<UpdateIdentifiableObjectActionType, IIdentifiable>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(IdentifiableObjectUpdateAction);

        /// <seealso cref="IdentifiableObjectUpdateAction{T,I}.IdentifiableObjectUpdateAction(UpdateIdentifiableObjectActionType)"/>
        public IdentifiableObjectUpdateAction(UpdateIdentifiableObjectActionType serializedAction) : base(serializedAction)
        {
        }

        /// <seealso cref="IdentifiableObjectUpdateAction{T,I}.IdentifiableObjectUpdateAction(I, Dictionary{string, string})"/>
        public IdentifiableObjectUpdateAction(IIdentifiable serializedSceneObject, Dictionary<string, string> attributes) :
            base(serializedSceneObject, attributes)
        {
        }
    }
}