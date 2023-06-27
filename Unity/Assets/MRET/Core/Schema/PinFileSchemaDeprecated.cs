// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Pin;

namespace GOV.NASA.GSFC.XR.MRET.Schema
{
    /// <remarks>
    /// History:
    /// 31 Oct 2022: Initial version (Sean Letavish)
    /// 05 Feb 2023: Updated to work with 22.1, and to use MarkerType instead of
    ///     PartType and deprecated until we reevaulate how we want to import/export
    ///     pins/markers  (Jeffrey Hosler)
    /// </remarks>
    ///
    /// <summary>
    /// PinFileSchemaDeprecated
    /// 
    /// The MRET pin file schema implementation.<br>
    ///
    /// Author: Jeffrey Hosler
    /// </summary>
    /// 
    public class PinFileSchemaDeprecated : BaseSchema
    {
        public new static readonly string NAME = nameof(PinFileSchemaDeprecated);

        /// <summary>Contants used to supply parent delegates.</summary>
        public static readonly string FILE_EXTENSION = ".mpin";
        public static readonly string SCHEMA_VERSION_v0_1 = "0.1";
        public static readonly string SCHEMA_VERSION_v0_9 = "0.9";
        public static readonly List<string> SCHEMA_VERSIONS = new List<string>
        {
            { SCHEMA_VERSION_v0_1 },
            { SCHEMA_VERSION_v0_9 }
        };
        public static readonly Dictionary<string, Type> VERSIONCHECKER_TYPES = new Dictionary<string, Type>
        {
            { SCHEMA_VERSIONS[0], typeof(PartVersionCheck) },
            { SCHEMA_VERSIONS[1], typeof(MarkerVersionCheck) }
        };
        public static readonly Dictionary<string, Type> SERIALIZABLE_TYPES = new Dictionary<string, Type>
        {
            { SCHEMA_VERSIONS[0], typeof(v0_1.PartType) },
            { SCHEMA_VERSIONS[1], typeof(v0_9.MarkerType) }
        };
        public static readonly Type DESERIALIZABLE_TYPE = typeof(InteractablePinDeprecated);

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

        /// <seealso cref="BaseSchema.GetDescriptionField"/>
        public new static string GetDescriptionField(string filePath)
        {
            string result = "";

            try
            {
                v0_9.MarkerType xml = (v0_9.MarkerType)FromXML(filePath);

                // Assign the description if we got a valid xml reference
                result = xml?.Description;
            }
            catch (Exception e)
            {
                string error = "[" + NAME + "->GetDescriptionField] " + e.ToString();
                Debug.LogError(error);
            }

            return result;
        }

        /// <seealso cref="BaseSchema.GetFilters"/>
        public new static SchemaFilter[] GetFilters(string filePath)
        {
            SchemaFilter[] result = null;

            try
            {
                v0_9.MarkerType xml = (v0_9.MarkerType)FromXML(filePath);
                if (xml != null)
                {
                    List<SchemaFilter> filters = new List<SchemaFilter>();

                    // Reference type
                    filters.Add(new SchemaFilter("Reference Basis", xml.ReferenceBasis.ToString()));

                    result = filters.ToArray();
                }
            }
            catch (Exception e)
            {
                string error = "[" + NAME + "->GetFilters] " + e.ToString();
                Debug.LogError(error);
            }

            return result;
        }
    }

    /// <seealso cref="VersionCheck"/>
    [System.Xml.Serialization.XmlRootAttribute("Marker")]
    public class MarkerVersionCheck : VersionCheck
    {
    }

}