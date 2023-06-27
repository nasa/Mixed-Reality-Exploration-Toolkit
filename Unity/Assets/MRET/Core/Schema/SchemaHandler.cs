// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using UnityEngine;

namespace GOV.NASA.GSFC.XR.MRET.Schema
{
    /// <remarks>
    /// History:
    /// 29 Apr 2020: Created (Dylan Baker)
    /// 06 Oct 2020: Added TimeSimulation schema handling (Jeffrey Hosler)
    /// 05 Sep 2021: Updated for schema v0.9 handling (Jeffrey Hosler)
    /// </remarks>
    ///
    /// <summary>
    /// SchemaHandler
    /// 
    /// The schema handler singleton class to be used as an entry point for MRET classes that
    /// require access to the schema handling when the file type is either unknown or intended
    /// to remain file type agnostic.<br>
    ///
    /// Author: Dylan Baker
    /// </summary>
    /// 
    public class SchemaHandler : MRETBehaviour
    {
        public override string ClassName => nameof(SchemaHandler);

        public static SchemaHandler instance;

        /// <summary>
        /// Obtains the file extension for the supplied file schema type (subclass of BaseSchema).<br>
        /// </summary>
        /// 
        /// <paramref name="type">Subclass of <code>BaseSchema</code></paramref>
        /// <returns>A <code>string</code> file extension or NULL</returns>
        /// 
        /// <see cref="BaseSchema"/>
        /// 
        public static string GetFileExtension(Type type)
        {
            if (type == typeof(ProjectFileSchema))
            {
                return ProjectFileSchema.FILE_EXTENSION;
            }
            else if (type == typeof(PartFileSchema))
            {
                return PartFileSchema.FILE_EXTENSION;
            }
            else if (type == typeof(AnimationFileSchema))
            {
                return AnimationFileSchema.FILE_EXTENSION;
            }
            else if (type == typeof(HUDFileSchema))
            {
                return HUDFileSchema.FILE_EXTENSION;
            }
            else if (type == typeof(TemplateFileSchema))
            {
                return TemplateFileSchema.FILE_EXTENSION;
            }
            else if (type == typeof(AudioRecordingFileSchema))
            {
                return AudioRecordingFileSchema.FILE_EXTENSION;
            }
            else if (type == typeof(TextAnnotationFileSchema))
            {
                return TextAnnotationFileSchema.FILE_EXTENSION;
            }
            else if (type == typeof(StaticPointCloudFileSchema))
            {
                return StaticPointCloudFileSchema.FILE_EXTENSION;
            }
            else if (type == typeof(TimeSimulationFileSchema))
            {
                return TimeSimulationFileSchema.FILE_EXTENSION;
            }
            else if (type == typeof(TerrainFileSchema))
            {
                return TerrainFileSchema.FILE_EXTENSION;
            }
            else if (type == typeof(SceneObjectGeneratorFileSchema))
            {
                return SceneObjectGeneratorFileSchema.FILE_EXTENSION;
            }
            else
            {
                Debug.LogWarning("[" + instance.ClassName + "->GetFileExtension] Unknown schema type.");
                return null;
            }
        }

        /// <summary>
        /// Obtains the file schema type (subclass of BaseSchema) from the supplied file extension.<br>
        /// </summary>
        /// 
        /// <paramref name="extension">A <code>string</code> file extension (includes the '.')</paramref>
        /// <returns>A <code>Type</code> sublcass of <code>BaseSchema</code> or NULL</returns>
        /// 
        /// <see cref="BaseSchema"/>
        /// 
        public static Type GetTypeFromExtension(string extension)
        {
            if (extension == ProjectFileSchema.FILE_EXTENSION)
            {
                return typeof(ProjectFileSchema);
            }
            else if (extension == PartFileSchema.FILE_EXTENSION)
            {
                return typeof(PartFileSchema);
            }
            else if (extension == AnimationFileSchema.FILE_EXTENSION)
            {
                return typeof(AnimationFileSchema);
            }
            else if (extension == HUDFileSchema.FILE_EXTENSION)
            {
                return typeof(HUDFileSchema);
            }
            else if (extension == TemplateFileSchema.FILE_EXTENSION)
            {
                return typeof(TemplateFileSchema);
            }
            else if (extension == AudioRecordingFileSchema.FILE_EXTENSION)
            {
                return typeof(AudioRecordingFileSchema);
            }
            else if (extension == TextAnnotationFileSchema.FILE_EXTENSION)
            {
                return typeof(TextAnnotationFileSchema);
            }
            else if (extension == StaticPointCloudFileSchema.FILE_EXTENSION)
            {
                return typeof(StaticPointCloudFileSchema);
            }
            else if (extension == TimeSimulationFileSchema.FILE_EXTENSION)
            {
                return typeof(TimeSimulationFileSchema);
            }
            else if (extension == TerrainFileSchema.FILE_EXTENSION)
            {
                return typeof(TerrainFileSchema);
            }
            else if (extension == SceneObjectGeneratorFileSchema.FILE_EXTENSION)
            {
                return typeof(SceneObjectGeneratorFileSchema);
            }
            else
            {
                Debug.LogWarning("[" + instance.ClassName + "->GetTypeFromExtension] Unknown file extension.");
                return null;
            }
        }

        /// <summary>
        /// Obtains the description from the contents of the supplied XML file.<br>
        /// </summary>
        /// 
        /// <paramref name="filePath">A <code>string</code> XML file</paramref>
        /// <returns>A <code>string</code> description of the supplied file or NULL</returns>
        /// 
        public static string GetDescriptionField(string filePath)
        {
            Type fileType = GetTypeFromExtension(System.IO.Path.GetExtension(filePath));
            if (fileType == null)
            {
                Debug.LogWarning("[" + instance.ClassName + "->GetDescriptionField] Unknown file extension.");
                return null;
            }
            else if (fileType == typeof(ProjectFileSchema))
            {
                return ProjectFileSchema.GetDescriptionField(filePath);
            }
            else if (fileType == typeof(PartFileSchema))
            {
                return PartFileSchema.GetDescriptionField(filePath);
            }
            else if (fileType == typeof(AnimationFileSchema))
            {
                return AnimationFileSchema.GetDescriptionField(filePath);
            }
            else if (fileType == typeof(HUDFileSchema))
            {
                return HUDFileSchema.GetDescriptionField(filePath);
            }
            else if (fileType == typeof(TemplateFileSchema))
            {
                return TemplateFileSchema.GetDescriptionField(filePath);
            }
            else if (fileType == typeof(AudioRecordingFileSchema))
            {
                return AudioRecordingFileSchema.GetDescriptionField(filePath);
            }
            else if (fileType == typeof(TextAnnotationFileSchema))
            {
                return TextAnnotationFileSchema.GetDescriptionField(filePath);
            }
            else if (fileType == typeof(StaticPointCloudFileSchema))
            {
                return StaticPointCloudFileSchema.GetDescriptionField(filePath);
            }
            else if (fileType == typeof(TimeSimulationFileSchema))
            {
                return TimeSimulationFileSchema.GetDescriptionField(filePath);
            }
            else if (fileType == typeof(TerrainFileSchema))
            {
                return TerrainFileSchema.GetDescriptionField(filePath);
            }
            else if (fileType == typeof(SceneObjectGeneratorFileSchema))
            {
                return SceneObjectGeneratorFileSchema.GetDescriptionField(filePath);
            }
            else
            {
                Debug.LogWarning("[" + instance.ClassName + "->GetDescriptionField] Unable to match file extension.");
                return null;
            }
        }

        /// <summary>
        /// Obtains the filters from the contents of the supplied XML file.<br>
        /// </summary>
        /// 
        /// <paramref name="filePath">A <code>string</code> XML file</paramref>
        /// <returns>A <code>FileInfo.FileFilter[]</code> of filters from the supplied file or NULL</returns>
        /// 
        public static SchemaFilter[] GetFilters(string filePath)
        {
            Type fileType = GetTypeFromExtension(System.IO.Path.GetExtension(filePath));
            if (fileType == null)
            {
                Debug.LogWarning("[" + instance.ClassName + "->GetFilters] Unknown file extension.");
                return null;
            }
            else if (fileType == typeof(ProjectFileSchema))
            {
                return ProjectFileSchema.GetFilters(filePath);
            }
            else if (fileType == typeof(PartFileSchema))
            {
                return PartFileSchema.GetFilters(filePath);
            }
            else if (fileType == typeof(AnimationFileSchema))
            {
                return AnimationFileSchema.GetFilters(filePath);
            }
            else if (fileType == typeof(HUDFileSchema))
            {
                return HUDFileSchema.GetFilters(filePath);
            }
            else if (fileType == typeof(TemplateFileSchema))
            {
                return TemplateFileSchema.GetFilters(filePath);
            }
            else if (fileType == typeof(AudioRecordingFileSchema))
            {
                return AudioRecordingFileSchema.GetFilters(filePath);
            }
            else if (fileType == typeof(TextAnnotationFileSchema))
            {
                return TextAnnotationFileSchema.GetFilters(filePath);
            }
            else if (fileType == typeof(StaticPointCloudFileSchema))
            {
                return StaticPointCloudFileSchema.GetFilters(filePath);
            }
            else if (fileType == typeof(TimeSimulationFileSchema))
            {
                return TimeSimulationFileSchema.GetFilters(filePath);
            }
            else if (fileType == typeof(TerrainFileSchema))
            {
                return TerrainFileSchema.GetFilters(filePath);
            }
            else if (fileType == typeof(SceneObjectGeneratorFileSchema))
            {
                return SceneObjectGeneratorFileSchema.GetFilters(filePath);
            }
            else
            {
                Debug.LogWarning("[" + instance.ClassName + "->GetFilters] Unable to match file extension.");
                return null;
            }
        }

        /// <summary>
        /// Reads the supplied XML file into a serialized object.<br>
        /// </summary>
        /// 
        /// <paramref name="filePath">A <code>string</code> XML file</paramref>
        /// <returns>A <code>object</code> representing the deserialization of the supplied file or NULL</returns>
        /// 
        public static object ReadXML(string filePath)
        {
            object result = null;

            try
            {
                Type fileType = GetTypeFromExtension(System.IO.Path.GetExtension(filePath));
                if (fileType == null)
                {
                    Debug.LogWarning("[" + instance.ClassName + "->ReadXML] Unknown file extension.");
                }
                else if (fileType == typeof(ProjectFileSchema))
                {
                    result = ProjectFileSchema.FromXML(filePath);
                }
                else if (fileType == typeof(PartFileSchema))
                {
                    result = PartFileSchema.FromXML(filePath);
                }
                else if (fileType == typeof(AnimationFileSchema))
                {
                    result = AnimationFileSchema.FromXML(filePath);
                }
                else if (fileType == typeof(HUDFileSchema))
                {
                    result = HUDFileSchema.FromXML(filePath);
                }
                else if (fileType == typeof(TemplateFileSchema))
                {
                    result = TemplateFileSchema.FromXML(filePath);
                }
                else if (fileType == typeof(AudioRecordingFileSchema))
                {
                    result = AudioRecordingFileSchema.FromXML(filePath);
                }
                else if (fileType == typeof(TextAnnotationFileSchema))
                {
                    result = TextAnnotationFileSchema.FromXML(filePath);
                }
                else if (fileType == typeof(StaticPointCloudFileSchema))
                {
                    result = StaticPointCloudFileSchema.FromXML(filePath);
                }
                else if (fileType == typeof(TimeSimulationFileSchema))
                {
                    result = TimeSimulationFileSchema.FromXML(filePath);
                }
                else if (fileType == typeof(TerrainFileSchema))
                {
                    result = TerrainFileSchema.FromXML(filePath);
                }
                else if (fileType == typeof(SceneObjectGeneratorFileSchema))
                {
                    result = SceneObjectGeneratorFileSchema.FromXML(filePath);
                }
                else
                {
                    Debug.LogWarning("[" + instance.ClassName + "->ReadXML] Unable to match file extension.");
                }
            }
            catch (Exception e)
            {
                string error = "[" + instance.ClassName + "->ReadXML] A problem was " +
                    "encountered deserializing the XML file: " +
                    filePath + "; " + e.ToString();
                Debug.LogError(error);
            }

            return result;
        }

        /// <summary>
        /// Writes the contents of the supplied deserialized object to the supplied XML file.<br>
        /// </summary>
        /// 
        /// <paramref name="filePath">A <code>string</code> output XML file</paramref>
        /// <paramref name="deserialized">An <code>object</code> representing the deserialization of a serialized object</paramref>
        /// 
        public static void WriteXML(string filePath, object deserialized)
        {
            try
            {
                Type fileType = GetTypeFromExtension(System.IO.Path.GetExtension(filePath));
                if (fileType == null)
                {
                    Debug.LogWarning("[" + instance.ClassName + "->WriteXML] Unknown file extension.");
                }
                else if (fileType == typeof(ProjectFileSchema))
                {
                    ProjectFileSchema.ToXML(filePath, deserialized);
                }
                else if (fileType == typeof(PartFileSchema))
                {
                    PartFileSchema.ToXML(filePath, deserialized);
                }
                else if (fileType == typeof(AnimationFileSchema))
                {
                    AnimationFileSchema.ToXML(filePath, deserialized);
                }
                else if (fileType == typeof(HUDFileSchema))
                {
                    HUDFileSchema.ToXML(filePath, deserialized);
                }
                else if (fileType == typeof(TemplateFileSchema))
                {
                    TemplateFileSchema.ToXML(filePath, deserialized);
                }
                else if (fileType == typeof(AudioRecordingFileSchema))
                {
                    AudioRecordingFileSchema.ToXML(filePath, deserialized);
                }
                else if (fileType == typeof(TextAnnotationFileSchema))
                {
                    TextAnnotationFileSchema.ToXML(filePath, deserialized);
                }
                else if (fileType == typeof(StaticPointCloudFileSchema))
                {
                    StaticPointCloudFileSchema.ToXML(filePath, deserialized);
                }
                else if (fileType == typeof(TimeSimulationFileSchema))
                {
                    TimeSimulationFileSchema.ToXML(filePath, deserialized);
                }
                else if (fileType == typeof(TerrainFileSchema))
                {
                    TerrainFileSchema.ToXML(filePath, deserialized);
                }
                else if (fileType == typeof(SceneObjectGeneratorFileSchema))
                {
                    SceneObjectGeneratorFileSchema.ToXML(filePath, deserialized);
                }
                else
                {
                    Debug.LogWarning("[" + instance.ClassName + "->WriteXML] Unable to match file extension.");
                }
            }
            catch (Exception e)
            {
                string error = "[" + instance.ClassName + "->WriteXML] " + e.ToString();
                Debug.LogError(error);
            }
        }

        /// <seealso cref="MRETUpdateBehaviour.MRETUpdate"/>
        protected override void MRETAwake()
        {
            // Take the inherited behavior
            base.MRETAwake();

            // Assign out instance variable
            instance = this;
        }
    }

    public class SchemaFilter
    {
        public string key;
        public string value;

        public SchemaFilter(string _key, string _value)
        {
            key = _key;
            value = _value;
        }
    }

}