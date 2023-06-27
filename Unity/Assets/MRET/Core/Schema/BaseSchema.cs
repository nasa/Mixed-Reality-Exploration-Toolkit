// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Linq;
using UnityEngine;

namespace GOV.NASA.GSFC.XR.MRET.Schema
{
    /// <remarks>
    /// History:
    /// 29 Apr 2020: Created (Dylan Baker)
    /// 05 Sep 2021: Refactored for schema v0.9 handling (Jeffrey Hosler)
    /// </remarks>
    ///
    /// <summary>
    /// BaseSchema
    /// 
    /// The base schema implementation for all MRET schemas. This class is a static class so normal
    /// inheritance does not apply. Delegates are used to produce the effects of polymorphism.<br>
    ///
    /// Author: Dylan Baker/Jeffrey Hosler
    /// </summary>
    /// 
    public abstract class BaseSchema
    {
        public static readonly string NAME = nameof(BaseSchema);

        /// <summary>
        /// Delegates for static subclasses to supply implementations.<br>
        /// </summary>
        protected delegate string GetFileExtension();                           // File extension when serializing from file
        protected delegate List<string> GetSchemaVersions();                    // String list of versions
        protected delegate string GetCurrentSchemaVersion();                    // Current version in string list of versions
        protected delegate string GetFallbackSchemaVersion();                   // Fallback version in string list of versions
        protected delegate Dictionary<string, Type> GetVersionCheckerTypes();   // [version, IsSubclassOf(typeof(VersionCheck))]
        protected delegate Dictionary<string, Type> GetSerializableTypes();     // [version, SerializableType]
        protected delegate Type GetDeserializableType();                        // DeserializableType
        protected delegate Dictionary<string, XmlSchemaSet> GetSchemaSets();    // The XML schema set to validate against

        protected static GetFileExtension FileExtension { get; set; }
        protected static GetSchemaVersions SchemaVersions { get; set; }
        protected static GetCurrentSchemaVersion CurrentSchemaVersion { get; set; }
        protected static GetFallbackSchemaVersion FallbackSchemaVersion { get; set; }
        protected static GetVersionCheckerTypes VersionCheckerTypes { get; set; }
        protected static GetSerializableTypes SerializableTypes { get; set; }
        protected static GetDeserializableType DeserializableType { get; set; }
        protected static GetSchemaSets SchemaSets { get; set; }

        /// <summary>
        /// Validates the required delegate fields for the implementing schema class to operate properly.<br>
        /// </summary>
        /// 
        /// <returns>A <code>bool</code> indicating whether the implementing schema class is configured properly</returns>
        /// 
        /// <see cref="FileExtension"/>
        /// <see cref="SchemaVersions"/>
        /// <see cref="CurrentSchemaVersion"/>
        /// <see cref="FallbackSchemaVersion"/>
        /// <see cref="VersionCheckerTypes"/>
        /// <see cref="SerializableTypes"/>
        /// <see cref="DeserializableType"/>
        /// 
        protected static bool ConfigurationValid()
        {
            // Make sure we have versions defined
            List<string> versions = SchemaVersions();
            if ((versions == null) || (versions.Count == 0))
            {
                Debug.LogError("[" + NAME + "] Schema configuration failed. Schema versions not defined.");
                return false;
            }

            // Make sure the list of versions are distinct
            List<string> distinctVersions = versions.Distinct().ToList();
            if (versions.Count != distinctVersions.Count)
            {
                Debug.LogError("[" + NAME + "] Schema configuration failed. Schema versions contains duplicates.");
                return false;
            }

            // Validate the settings for each schema version.
            foreach (string version in versions)
            {
                // Make sure the version has an actual value
                if (string.IsNullOrEmpty(version))
                {
                    Debug.LogError("[" + NAME + "] Schema configuration failed. Schema version must be defined.");
                    return false;
                }

                // Check the version checker class
                Dictionary<string, Type> versionCheckerTypes = VersionCheckerTypes();
                if (versionCheckerTypes == null)
                {
                    Debug.LogError("[" + NAME + "] Schema configuration failed. Version checker types are not defined.");
                    return false;
                }

                try
                {
                    // Make sure the version is present in the VersionCheck. This will throw an exception if not.
                    Type versionCheckerType = versionCheckerTypes[version];

                    // Make sure the version checker is defined
                    if (versionCheckerType == null)
                    {
                        Debug.LogError("[" + NAME + "] Schema configuration failed. Version checker type is not defined for version: " +
                            version);
                        return false;
                    }

                    // Make sure the version checker is a subclass of VersionCheck
                    if (!versionCheckerType.IsSubclassOf(typeof(VersionCheck)) || versionCheckerType == typeof(VersionCheck))
                    {
                        Debug.LogError("[" + NAME + "] Schema configuration failed. Version checker type is not properly defined for schema version: " +
                            version);
                        return false;
                    }
                }
                catch (KeyNotFoundException)
                {
                    Debug.LogError("[" + NAME + "] Schema configuration failed. Version checker type not found for version: " +
                        version);
                    return false;
                }

                // Make sure we have valid serializable types
                Dictionary<string, Type> serializedTypes = SerializableTypes();
                if (serializedTypes == null)
                {
                    Debug.LogError("[" + NAME + "] Schema configuration failed. Serializable types are not defined.");
                    return false;
                }

                try
                {
                    // Make sure the version is present in the SerializableTypes. This will throw an exception if not.
                    Type serializableType = serializedTypes[version];

                    // Make sure the serializable type is defined
                    if (serializableType == null)
                    {
                        Debug.LogError("[" + NAME + "] Schema configuration failed. Serializable type is not defined for version: " +
                            version);
                        return false;
                    }
                }
                catch (KeyNotFoundException)
                {
                    Debug.LogError("[" + NAME + "] Schema configuration failed. Serializable type not found for version: " +
                        version);
                    return false;
                }
            }

            // Validate the current schema version
            string currentSchemaVersion = CurrentSchemaVersion();
            if (string.IsNullOrEmpty(currentSchemaVersion))
            {
                Debug.LogError("[" + NAME + "] Schema configuration failed. Current schema version is not defined.");
                return false;
            }

            // Make sure the current version exists in the schema versions list
            if (versions.IndexOf(currentSchemaVersion) < 0)
            {
                Debug.LogError("[" + NAME + "] Schema configuration failed. Current schema version does not exist in the schema versions.");
                return false;
            }

            // Validate the fallback schema version
            string fallbackSchemaVersion = FallbackSchemaVersion();
            if (string.IsNullOrEmpty(fallbackSchemaVersion))
            {
                Debug.LogError("[" + NAME + "] Schema configuration failed. Fallback schema version is not defined.");
                return false;
            }

            // Make sure the fallback version exists in the schema versions list
            if (versions.IndexOf(fallbackSchemaVersion) < 0)
            {
                Debug.LogError("[" + NAME + "] Schema configuration failed. Fallback schema version does not exist in the schema versions.");
                return false;
            }

            // Check the deserializable type. We only deserialize for the latest version
            if (DeserializableType() == null)
            {
                Debug.LogError("[" + NAME + "] Schema configuration failed. Deserialized type is not defined.");
                return false;
            }

            // Check the file extension
            if (string.IsNullOrEmpty(FileExtension()))
            {
                Debug.LogError("[" + NAME + "] Schema configuration failed. File extension is not defined.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Performs an upgrade of the supplied deserialized object to the newest version of the schema.
        /// This static method is intended to be reimplemented by static subclasses with eht 'new' keyword.<br>
        /// </summary>
        /// 
        /// <paramref name="deserialized">Deserialized object (from <code>SerializableTypes</code>)</paramref>
        /// <paramref name="version">Version of the deserialized object (from <code>SchemaVersions</code>)</paramref>
        /// <returns>An <code>object</code> representing the current serialized type (from <code>CurrentSchemaVersion</code>)</returns>
        /// 
        /// <see cref="SerializableTypes"/>
        /// <see cref="SchemaVersions"/>
        /// <see cref="CurrentSchemaVersion"/>
        /// 
        public static object UpgradeSchemaVersion(object serialized, string version)
        {
            Type serializedVersionType = serialized.GetType();
            Type currentVersionType = SerializableTypes()[CurrentSchemaVersion()];

            // Upgrade to the newest version
            int serializedVersionIndex = SchemaVersions().IndexOf(version);
            int currentVersionIndex = SchemaVersions().IndexOf(CurrentSchemaVersion());
            for (int i = serializedVersionIndex; (i < currentVersionIndex) && (serialized is IUpgradable); i++)
            {
                // Attempt an upgrade
                serialized = (serialized as IUpgradable).Upgrade();

                // Make sure an upgrade occurred
                Type upgradedType = serialized.GetType();
                if (upgradedType == serializedVersionType)
                {
                    // No upgrade occurred so abort with a warning
                    string warning = "[" + NAME + "->FromXML] Schema upgrade from version '" +
                        SchemaVersions()[serializedVersionIndex] + "' to '" +
                        CurrentSchemaVersion() + "' not performed";
                    Debug.LogWarning(warning);
                    break;
                }

                // Move to the next version
                serializedVersionType = serialized.GetType();
            }

            return serialized;
        }

        /// <summary>
        /// Obtains the description from the contents of the supplied XML file.
        /// This static method is intended to be reimplemented by static subclasses with eht 'new' keyword.<br>
        /// </summary>
        /// 
        /// <paramref name="filePath">A <code>string</code> XML file</paramref>
        /// <returns>A <code>string</code> description of the supplied file or NULL</returns>
        /// 
        public static string GetDescriptionField(string filePath)
        {
            return null;
        }

        /// <summary>
        /// Obtains the filters from the contents of the supplied XML file.<br>
        /// This static method is intended to be reimplemented by static subclasses with eht 'new' keyword.<br>
        /// </summary>
        /// 
        /// <paramref name="filePath">A <code>string</code> XML file</paramref>
        /// <returns>A <code>FileInfo.FileFilter[]</code> of filters from the supplied file or NULL</returns>
        /// 
        public static SchemaFilter[] GetFilters(string filePath)
        {
            return null;
        }

        /// <summary>
        /// Schema validation handler. Uses the MRET configuration manager to control response behavior
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="args">Validation arguments</param>
        /// <exception cref="XmlSchemaException">Thrown if the response is to error on validation errors</exception>
        protected static void SchemaValidationEventHandler(object sender, ValidationEventArgs args)
        {
            // Look at the configuration manager to decide how to respond
            switch (MRET.ConfigurationManager.validateXmlType)
            {
                case v0_1.XmlValidationType.Error:
                    // Throw an exception if we had an error
                    switch(args.Severity)
                    {
                        case XmlSeverityType.Error:
                            throw args.Exception;
                        default:
                            // Do nothing
                            break;
                    }
                    break;
                case v0_1.XmlValidationType.Warning:
                    // Log all errors or warnings
                    switch (args.Severity)
                    {
                        case XmlSeverityType.Error:
                        case XmlSeverityType.Warning:
                            Debug.LogWarning(args.Message);
                            break;
                        default:
                            // Do nothing
                            break;
                    }
                    break;
                default:
                    // Do nothing
                    break;
            }
        }

        private static void ValidateXML(string xmlFileName, FileStream xml, string version)
        {
            // Make sure we are allowed to validate
            if (!MRET.ConfigurationManager.validateXml) return;

            // Make sure we have schemas to validate against
            if ((SchemaSets() == null) || (SchemaSets().Count == 0)) return;

            // Save the stream position
            long savedPosition = xml.Position;

            try
            {
                // Log a message in case there's an exception
                Debug.Log("Validating XML file: " + xmlFileName);

                XmlReaderSettings settings = new XmlReaderSettings();
                settings.ValidationType = ValidationType.Schema;
                settings.Schemas = SchemaSets()[version];
                settings.ValidationFlags =
                    XmlSchemaValidationFlags.ProcessIdentityConstraints |
                    XmlSchemaValidationFlags.ReportValidationWarnings;
                settings.ValidationEventHandler += SchemaValidationEventHandler;

                XmlReader schemaReader = XmlReader.Create(xml, settings);
                while (schemaReader.Read());

                // Report success
                Debug.Log("XML file validated: " + xmlFileName);
            }
            finally
            {
                // Restore the stream position
                xml.Seek(savedPosition, SeekOrigin.Begin);
            }
        }

        /// <summary>
        /// Deserializes the supplied XML file into a serialized object.<br>
        /// </summary>
        /// 
        /// <paramref name="filePath">A <code>string</code> XML file</paramref>
        /// <returns>A <code>object</code> representing the deserialization of the supplied file or NULL</returns>
        /// <exception cref="Exception">If there was an issue deserializing from the XML file.</exception>
        /// 
        public static object FromXML(string filePath)
        {
            object result = null;

            // Make sure the configuration is valid before proceeding
            if (ConfigurationValid())
            {
                // Assign the extension if the supplied file doesn't have one specified
                string extension = Path.GetExtension(filePath);
                if (string.IsNullOrEmpty(extension))
                {
                    filePath += FileExtension();
                }

                // Open the file for streaming
                FileStream fs = new FileStream(filePath, FileMode.Open);
                try
                {
                    // Deserialize the XML
                    XmlSerializer serializer;

                    // Start with the fallback version
                    string version = FallbackSchemaVersion();
                    List<string> versions = SchemaVersions();

                    // First check for the version. We need to check each version because the root element of the XML may be different
                    Dictionary<string, Type> versionCheckerTypes = VersionCheckerTypes();
                    foreach (KeyValuePair<string, Type> versionCheckerEntry in versionCheckerTypes)
                    {
                        try
                        {
                            // Make sure the version checker is a subclass of VersionCheck
                            serializer = new XmlSerializer(versionCheckerEntry.Value);
                            fs.Seek(0, SeekOrigin.Begin); // Make sure we are at the beginning of the stream
                            VersionCheck v = (VersionCheck)serializer.Deserialize(new NamespaceIgnorantXmlTextReader(fs));

                            // Attempt to locate the key in the schema versions list
                            if (versions.IndexOf(v.version) >= 0)
                            {
                                // Only assign the version if it is valid. Otherwise just keep the fallback
                                version = v.version;
                                break;
                            }
                        }
                        catch (Exception)
                        {
                            // That one didn't work, so go to next version checker (root element may have been different)
                        }
                    }

                    // Deserialize the correct version
                    Dictionary<string, Type> serializableTypes = SerializableTypes();
                    Type versionType = serializableTypes[version];
                    serializer = new XmlSerializer(versionType);
                    fs.Seek(0, SeekOrigin.Begin); // Make sure we are at the beginning of the stream

                    // Validate the XML
                    ValidateXML(filePath, fs, version);

                    // Deserialize
                    object serialized = serializer.Deserialize(fs);
                    fs.Close();

                    // See if the serialized object needs upgrading
                    if (serialized is IUpgradable)
                    {
                        serialized = UpgradeSchemaVersion(serialized as IUpgradable, version);
                    }

                    /***********FIXME: Remove***********
                    string versionStr = "_v" + CurrentSchemaVersion().Replace('.', '_');
                    string upgradeVerificationFile = !filePath.Contains(versionStr)
                        ? filePath.Replace(extension, versionStr + extension)
                        : filePath;
                    ToXML(upgradeVerificationFile, serialized);
                    ***********************************/

                    result = serialized;
                }
                catch (Exception e)
                {
                    string error = "[" + NAME + "->FromXML] A problem was " +
                        "encountered deserializing the XML file: " +
                        filePath + "; " + e.ToString();
                    Debug.LogError(error);
                    fs.Close();
                    throw new Exception(error, e);
                }
            }
            else
            {
                string error = "[" + NAME + "->FromXML] Failed to deserialize the XML file: " +
                    filePath + ". Configuration for this schema class is invalid.";
                Debug.LogError(error);
                throw new Exception(error);
            }

            return result;
        }

        /// <summary>
        /// Serializes the supplied object to an XML file.<br>
        /// </summary>
        /// 
        /// <paramref name="filePath">A <code>string</code> XML file</paramref>
        /// <paramref name="deserialized">A deserialized type <code>object</code> to serialize to the XML file</paramref>
        /// <exception cref="Exception">If there was an issue serializing to the XML file.</exception>
        /// 
        public static void ToXML(string filePath, object deserialized)
        {
            // Make sure the configuration is valid before proceeding
            if (ConfigurationValid())
            {
                // Assign the extension if the supplied file doesn't have one specified
                string extension = System.IO.Path.GetExtension(filePath);
                if (string.IsNullOrEmpty(extension))
                {
                    filePath += FileExtension();
                }

                // Create the formatting options
                XmlWriterSettings settings = new XmlWriterSettings
                {
                    Indent = true,
                    NewLineOnAttributes = false
                };
                XmlWriter writer = XmlWriter.Create(filePath, settings);
                try
                {
                    Dictionary<string, Type> serializableTypes = SerializableTypes();
                    XmlSerializer serializer = new XmlSerializer(serializableTypes[CurrentSchemaVersion()]);
                    serializer.Serialize(writer, deserialized);
                    writer.Close();
                }
                catch (Exception e)
                {
                    string error = "[" + NAME + "->ToXML] " + e.ToString();
                    Debug.LogError(error);
                    writer.Close();
                    throw new Exception(error, e);
                }
            }
            else
            {
                string error = "[" + NAME + "->ToXML] Failed to serialize to the XML file. Configuration for this schema class is invalid.";
                Debug.LogError(error);
                throw new Exception(error);
            }
        }
    }

    /// <summary>
    /// VersionCheck
    /// 
    /// The base version check class implementation for all MRET schemas. This class simply defines
    /// a serialization attribute for the 'version'. Static subclasses must extent this type to define
    /// the root node so that the serialization does not throw an exception. The extended type(s) is
    /// supplied in the <code>SerializableTypes</code> delegate.<br>
    ///
    /// Author: Jeffrey Hosler
    /// </summary>
    /// 
    [System.Xml.Serialization.XmlTypeAttribute()]
    public class VersionCheck
    {
        private string versionField;

        public VersionCheck()
        {
            this.versionField = "";
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute("")]
        public string version
        {
            get
            {
                return this.versionField;
            }
            set
            {
                this.versionField = value;
            }
        }
    }

    /// <summary>
    /// NamespaceIgnorantXmlTextReader
    /// 
    /// Helper class to ignore namespaces when de-serializing.<br>
    ///
    /// Author: Jeffrey Hosler
    /// </summary>
    /// 
    public class NamespaceIgnorantXmlTextReader : XmlTextReader
    {
        public NamespaceIgnorantXmlTextReader(FileStream reader) : base(reader) { }

        public override string NamespaceURI
        {
            get { return ""; }
        }
    }

}