// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Reflection;
using UnityEngine;
using GOV.NASA.GSFC.XR.XRC;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.Integrations.XRC
{
    public interface IEntityParameters
    {
        public string EntityID { get; }
        public string Category { get; }
        public EntityType Type { get; }
        public string ParentID { get; }
        public IdentifiableType SerializedEntity { get; }
    }

    public interface IEntityParameters<T> : IEntityParameters
        where T : IdentifiableType
    {
        new public T SerializedEntity { get; }
    }

    public class EntityParameters<T> : IEntityParameters<T>
        where T : IdentifiableType, new()
    {
        public string EntityID { get => XRCUnity.GetXRCEntityID(SerializedEntity); }
        public string Category { get => SerializedEntity.GetType().ToString(); }
        public EntityType Type { get; }
        public string ParentID { get; }
        public T SerializedEntity { get; private set; }

        IdentifiableType IEntityParameters.SerializedEntity => SerializedEntity;

        public EntityParameters(EntityType type, string parentID = "") : this(type, new T(), parentID)
        {
        }

        public EntityParameters(EntityType type, T serializedEntity, string parentID = "")
        {
            Type = type;
            ParentID = parentID;
            SerializedEntity = serializedEntity;
        }

        public EntityParameters(EntityType type, string serializableTypeName, string parentID = "")
        {
            Type = type;
            ParentID = parentID;
            try
            {
                SerializedEntity = CreateSerializedEntity(serializableTypeName);
            }
            catch (Exception e)
            {
                Debug.LogWarning("SerializedEntity could not be instantiated from name: " + serializableTypeName + "; " + e);
            }
        }

        public T CreateSerializedEntity(string fromSerializableTypeName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            Type serializableType = assembly.GetType(fromSerializableTypeName, true);
            return assembly.CreateInstance(serializableType.ToString(), true) as T;
        }

    }
}