// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

namespace GOV.NASA.GSFC.XR.MRET.Schema
{
    /// <remarks>
    /// History:
    /// 29 Apr 2020: Created (Dylan Baker)
    /// 05 Sep 2021: Refactored for schema v0.9 handling (Jeffrey Hosler)
    /// </remarks>
    ///
    /// <summary>
    /// TemplateFileSchema
    /// 
    /// The MRET template file schema implementation.<br>
    ///
    /// Author: Dylan Baker/Jeffrey Hosler
    /// </summary>
    /// 
    public class TemplateFileSchema : ProjectFileSchema
    {
        public new static readonly string NAME = nameof(TemplateFileSchema);

        /// <summary>Contants used to supply parent delegates.</summary>
        public new static readonly string FILE_EXTENSION = ".mtmpl";

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
        protected new static void InitializeSchema()
        {
            FileExtension = delegate () { return FILE_EXTENSION; };
            SchemaVersions = delegate () { return SCHEMA_VERSIONS; };
            CurrentSchemaVersion = delegate () { return SCHEMA_VERSION_v0_9; };
            FallbackSchemaVersion = delegate () { return SCHEMA_VERSION_v0_1; };
            VersionCheckerTypes = delegate () { return VERSIONCHECKER_TYPES; };
            SerializableTypes = delegate () { return SERIALIZABLE_TYPES; };
            DeserializableType = delegate () { return DESERIALIZABLE_TYPE; };
        }

    }
}