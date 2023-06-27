// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections.Generic;

namespace GOV.NASA.GSFC.XR.MRET.Schema
{
    /// <remarks>
    /// History:
    /// 29 Apr 2020: Created (Dylan Baker)
    /// 05 Sep 2021: Refactored for schema v0.9 handling (Jeffrey Hosler)
    /// </remarks>
    ///
    /// <summary>
    /// HUDFileSchema
    /// 
    /// The MRET HUD file schema implementation.<br>
    ///
    /// Author: Dylan Baker/Jeffrey Hosler
    /// </summary>
    /// 
    public class HUDFileSchema : BaseSchema
    {
        public new static readonly string NAME = nameof(HUDFileSchema);

        /// <summary>Contants used to supply parent delegates.</summary>
        public static readonly string FILE_EXTENSION = ".mhud";
        public static readonly string SCHEMA_VERSION_v0_1 = "0.1";
        public static readonly string SCHEMA_VERSION_v0_9 = "0.9";
        public static readonly List<string> SCHEMA_VERSIONS = new List<string>
        {
            { SCHEMA_VERSION_v0_1 },
            { SCHEMA_VERSION_v0_9 }
        };
        public static readonly Dictionary<string, Type> VERSIONCHECKER_TYPES = new Dictionary<string, Type>
        {
            { SCHEMA_VERSIONS[0], typeof(HUDVersionCheck) },
            { SCHEMA_VERSIONS[1], typeof(HUDVersionCheck) }
        };
        public static readonly Dictionary<string, Type> SERIALIZABLE_TYPES = new Dictionary<string, Type>
        {
            { SCHEMA_VERSIONS[0], typeof(v0_1.HUDType) },
            { SCHEMA_VERSIONS[1], typeof(v0_9.HUDType) }
        };
        public static readonly Type DESERIALIZABLE_TYPE = typeof(object);

        /// <summary>
        /// Initializes the delegates required by the parent <code>BaseSchema</code>.<br>
        /// </summary>
        /// 
        /// <returns>A <code>string</code> file extension or NULL</returns>
        /// 
        /// <see cref="BaseSchema"/>
        /// <see cref="BaseSchema.FileExtension"/>
        /// <see cref="BaseSchema.SchemaVersions"/>
        /// <see cref="BaseSchema.CurrentSchemaVersion"/>
        /// <see cref="BaseSchema.FallbackSchemaVersion"/>
        /// <see cref="BaseSchema.VersionCheckerTypes"/>
        /// <see cref="BaseSchema.SerializableTypes"/>
        /// <see cref="BaseSchema.DeserializableType"/>
        /// 
        protected static void InitializeSchema()
        {
            FileExtension = delegate () { return FILE_EXTENSION; };
            SchemaVersions = delegate () { return SCHEMA_VERSIONS; };
            CurrentSchemaVersion = delegate () { return SCHEMA_VERSION_v0_9; };
            FallbackSchemaVersion = delegate () { return SCHEMA_VERSION_v0_1; };
            VersionCheckerTypes = delegate () { return VERSIONCHECKER_TYPES; };
            SerializableTypes = delegate () { return SERIALIZABLE_TYPES; };
            DeserializableType = delegate () { return DESERIALIZABLE_TYPE; };
        }

        /// <seealso cref="BaseSchema.FromXML"/>
        public new static object FromXML(string filePath)
        {
            // Make sure to initialize for this schema
            InitializeSchema();

            // Now, we can defer to the base
            return BaseSchema.FromXML(filePath);
        }

        /// <seealso cref="BaseSchema.ToXML"/>
        public new static void ToXML(string filePath, object deserialized)
        {
            // Make sure to initialize for this schema
            InitializeSchema();

            // Now, we can defer to the base
            BaseSchema.ToXML(filePath, deserialized);
        }

    }

    /// <seealso cref="VersionCheck"/>
    [System.Xml.Serialization.XmlRootAttribute("HUD")]
    public class HUDVersionCheck : VersionCheck
    {
    }

}