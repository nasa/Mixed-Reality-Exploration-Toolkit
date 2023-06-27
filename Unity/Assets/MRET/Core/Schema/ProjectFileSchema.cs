// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Project;

namespace GOV.NASA.GSFC.XR.MRET.Schema
{
    /// <remarks>
    /// History:
    /// 29 Apr 2020: Created (Dylan Baker)
    /// 05 Sep 2021: Refactored for schema v0.9 handling (Jeffrey Hosler)
    /// </remarks>
    ///
    /// <summary>
    /// ProjectFileSchema
    /// 
    /// The MRET project file schema implementation.<br>
    ///
    /// Author: Dylan Baker/Jeffrey Hosler
    /// </summary>
    /// 
    public class ProjectFileSchema : BaseSchema
    {
        public new static readonly string NAME = nameof(ProjectFileSchema);

        /// <summary>Contants used to supply parent delegates.</summary>
        public static readonly string FILE_EXTENSION = ".mret";
        public static readonly string SCHEMA_VERSION_v0_1 = "0.1";
        public static readonly string SCHEMA_VERSION_v0_9 = "0.9";
        public static readonly List<string> SCHEMA_VERSIONS = new List<string>
        {
            { SCHEMA_VERSION_v0_1 },
            { SCHEMA_VERSION_v0_9 }
        };
        public static readonly Dictionary<string, Type> VERSIONCHECKER_TYPES = new Dictionary<string, Type>
        {
            { SCHEMA_VERSIONS[0], typeof(ProjectVersionCheck) },
            { SCHEMA_VERSIONS[1], typeof(ProjectVersionCheck) }
        };
        public static readonly Dictionary<string, Type> SERIALIZABLE_TYPES  = new Dictionary<string, Type>
        {
            { SCHEMA_VERSIONS[0], typeof(v0_1.ProjectType) },
            { SCHEMA_VERSIONS[1], typeof(v0_9.ProjectType) }
        };
        public static readonly Dictionary<string, string> SCHEMA_URIS = new Dictionary<string, string>
        {
            { SCHEMA_VERSIONS[0], "../Schema/XSD/SchemaFiles/v0.1/Project.xsd" },
            { SCHEMA_VERSIONS[1], "../Schema/XSD/SchemaFiles/v0.9/Project.xsd" }
        };
        public static readonly Type DESERIALIZABLE_TYPE = typeof(UnityProject); // TODO: Implement use of this

        // Schema cache
        protected static Dictionary<string, XmlSchemaSet> SCHEMAS;

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
            SchemaSets = delegate () { return GetXmlSchemaSet(); };
        }

        /// <summary>
        /// Gets the schema sets for each XML version
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string, XmlSchemaSet> GetXmlSchemaSet()
        {
            // Only do this once and then cache it
            if (SCHEMAS == null)
            {
                SCHEMAS = new Dictionary<string, XmlSchemaSet>();

                foreach (KeyValuePair<string, string> schemaUri in SCHEMA_URIS)
                {
                    try
                    {
                        XmlSchemaSet xmlSchemaSet = new XmlSchemaSet();
                        XmlReader schemaReader = XmlReader.Create(SCHEMA_URIS[schemaUri.Key]);
                        xmlSchemaSet.Add(XmlSchema.Read(schemaReader, SchemaValidationEventHandler));
                        SCHEMAS.Add(schemaUri.Key, xmlSchemaSet);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("There was a problem reading the XML schema for validation: " + e);
                        SCHEMAS = new Dictionary<string, XmlSchemaSet>(); // Empty dictionary
                    }
                }
            }

            return SCHEMAS;
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
                object xmlObj = FromXML(filePath);
                if (xmlObj is v0_9.ProjectType)
                {
                    v0_9.ProjectType xml = xmlObj as v0_9.ProjectType;

                    // Assign the description if we got a valid xml reference
                    result = xml?.Description;
                }
            }
            catch (Exception e)
            {
                string error = "[" + NAME + "->GetDescriptionField] A problem was " +
                    "encountered getting the description field from file: " +
                    filePath + "; " + e.ToString();
                Debug.LogError(error);
            }

            return result;
        }
    }

    /// <seealso cref="VersionCheck"/>
    [System.Xml.Serialization.XmlRootAttribute("Project")]
    public class ProjectVersionCheck : VersionCheck
    {
    }

}