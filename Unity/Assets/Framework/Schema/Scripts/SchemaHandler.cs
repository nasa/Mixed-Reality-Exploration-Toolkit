// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using UnityEngine;
using GSFC.ARVR.MRET.Common.Schemas;
using GSFC.ARVR.MRET.Common.Schemas.TimeSimulationTypes;

namespace GSFC.ARVR.MRET.Common
{
    public class SchemaHandler : MonoBehaviour
    {
        public static readonly string projectFileExtension = ".mret",
            partFileExtension = ".mpart", animationFileExtension = ".manim",
            hudFileExtension = ".mhud", templateFileExtension = ".mtmpl",
            audioRecordingFileExtension = ".maudio",
            textAnnotationFileExtension = ".mtext",
            staticPointCloudFileExtension = ".mpoints",
            timeSimulationFileExtension = ".mtime";

        public static SchemaHandler instance;

        private ProjectFileSchema projectFileSchema;
        private PartFileSchema partFileSchema;
        private AnimationFileSchema animationFileSchema;
        private HUDFileSchema hudFileSchema;
        private TemplateFileSchema templateFileSchema;
        private AudioRecordingFileSchema audioRecordingFileSchema;
        private TextAnnotationFileSchema textAnnotationFileSchema;
        private StaticPointCloudFileSchema staticPointCloudFileSchema;
        private TimeSimulationFileSchema timeSimulationFileSchema;

        public static string GetFileExtension(Type type)
        {
            if (type == typeof(ProjectFileSchema))
            {
                return ProjectFileSchema.fileExtension;
            }
            else if (type == typeof(PartFileSchema))
            {
                return PartFileSchema.fileExtension;
            }
            else if (type == typeof(AnimationFileSchema))
            {
                return AnimationFileSchema.fileExtension;
            }
            else if (type == typeof(HUDFileSchema))
            {
                return HUDFileSchema.fileExtension;
            }
            else if (type == typeof(TemplateFileSchema))
            {
                return TemplateFileSchema.fileExtension;
            }
            else if (type == typeof(AudioRecordingFileSchema))
            {
                return AudioRecordingFileSchema.fileExtension;
            }
            else if (type == typeof(TextAnnotationFileSchema))
            {
                return TextAnnotationFileSchema.fileExtension;
            }
            else if (type == typeof(StaticPointCloudFileSchema))
            {
                return StaticPointCloudFileSchema.fileExtension;
            }
            else if (type == typeof(TimeSimulationFileSchema))
            {
                return TimeSimulationFileSchema.fileExtension;
            }
            else
            {
                Debug.LogWarning("[SchemaHandler] Unknown schema type.");
                return null;
            }
        }

        public static Type GetTypeFromExtension(string extension)
        {
            if (extension == ProjectFileSchema.fileExtension)
            {
                return typeof(ProjectFileSchema);
            }
            else if (extension == PartFileSchema.fileExtension)
            {
                return typeof(PartFileSchema);
            }
            else if (extension == AnimationFileSchema.fileExtension)
            {
                return typeof(AnimationFileSchema);
            }
            else if (extension == HUDFileSchema.fileExtension)
            {
                return typeof(HUDFileSchema);
            }
            else if (extension == TemplateFileSchema.fileExtension)
            {
                return typeof(TemplateFileSchema);
            }
            else if (extension == AudioRecordingFileSchema.fileExtension)
            {
                return typeof(AudioRecordingFileSchema);
            }
            else if (extension == TextAnnotationFileSchema.fileExtension)
            {
                return typeof(TextAnnotationFileSchema);
            }
            else if (extension == StaticPointCloudFileSchema.fileExtension)
            {
                return typeof(StaticPointCloudFileSchema);
            }
            else if (extension == TimeSimulationFileSchema.fileExtension)
            {
                return typeof(TimeSimulationFileSchema);
            }
            else
            {
                Debug.LogWarning("[SchemaHandler] Unknown file extension " + extension + ".");
                return null;
            }
        }

        public static string GetDescriptionField(string filePath)
        {
            Type fileType = GetTypeFromExtension(System.IO.Path.GetExtension(filePath));
            if (fileType == null)
            {
                Debug.LogWarning("[SchemaHandler] Unknown file extension.");
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
            else
            {
                Debug.LogWarning("[SchemaHandler] Unable to match file extension.");
                return null;
            }
        }

        public static FileInfo.FileFilter[] GetFilters(string filePath)
        {
            Type fileType = GetTypeFromExtension(System.IO.Path.GetExtension(filePath));
            if (fileType == null)
            {
                Debug.LogWarning("[SchemaHandler] Unknown file extension.");
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
            else
            {
                Debug.LogWarning("[SchemaHandler] Unable to match file extension.");
                return null;
            }
        }

        public static object ReadXML(string filePath)
        {
            Type fileType = GetTypeFromExtension(System.IO.Path.GetExtension(filePath));
            if (fileType == null)
            {
                Debug.LogWarning("[SchemaHandler] Unknown file extension.");
                return null;
            }
            else if (fileType == typeof(ProjectFileSchema))
            {
                return ProjectFileSchema.FromXML(filePath);
            }
            else if (fileType == typeof(PartFileSchema))
            {
                return PartFileSchema.FromXML(filePath);
            }
            else if (fileType == typeof(AnimationFileSchema))
            {
                return AnimationFileSchema.FromXML(filePath);
            }
            else if (fileType == typeof(HUDFileSchema))
            {
                return HUDFileSchema.FromXML(filePath);
            }
            else if (fileType == typeof(TemplateFileSchema))
            {
                return TemplateFileSchema.FromXML(filePath);
            }
            else if (fileType == typeof(AudioRecordingFileSchema))
            {
                return AudioRecordingFileSchema.FromXML(filePath);
            }
            else if (fileType == typeof(TextAnnotationFileSchema))
            {
                return TextAnnotationFileSchema.FromXML(filePath);
            }
            else if (fileType == typeof(StaticPointCloudFileSchema))
            {
                return StaticPointCloudFileSchema.FromXML(filePath);
            }
            else if (fileType == typeof(TimeSimulationFileSchema))
            {
                return TimeSimulationFileSchema.FromXML(filePath);
            }
            else
            {
                Debug.LogWarning("[SchemaHandler] Unable to match file extension.");
                return null;
            }
        }

        public static void WriteXML(string filePath, object deserialized)
        {
            Type fileType = GetTypeFromExtension(System.IO.Path.GetExtension(filePath));
            if (fileType == null)
            {
                Debug.LogWarning("[SchemaHandler] Unknown file extension.");
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
            else
            {
                Debug.LogWarning("[SchemaHandler] Unable to match file extension.");
            }
            return;
        }

        public void InitializeSchemas()
        {
            ProjectFileSchema.InitializeSchema();
            PartFileSchema.InitializeSchema();
            AnimationFileSchema.InitializeSchema();
            HUDFileSchema.InitializeSchema();
            TemplateFileSchema.InitializeSchema();
            AudioRecordingFileSchema.InitializeSchema();
            TextAnnotationFileSchema.InitializeSchema();
            StaticPointCloudFileSchema.InitializeSchema();
            TimeSimulationFileSchema.InitializeSchema();
        }

        void Awake()
        {
            instance = this;
        }
    }
}