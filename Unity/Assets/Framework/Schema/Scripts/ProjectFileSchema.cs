// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;

namespace GSFC.ARVR.MRET.Common.Schemas
{
    public class ProjectFileSchema : BaseSchema
    {
        public static string fileExtension;
        public static Type serializedType;
        public static Type deserializedType;

        public new static void InitializeSchema()
        {
            // Including the ".".
            fileExtension = ".mret";

            serializedType = typeof(ProjectType);

            deserializedType = typeof(UnityProject);
        }

        public static object FromXML(string filePath)
        {
            return FromXML(filePath, serializedType);
        }

        public static void ToXML(string filePath, object deserialized)
        {
            ToXML(filePath, deserialized, serializedType);
        }

        public new static string GetDescriptionField(string filePath)
        {
            ProjectType xml = (ProjectType) FromXML(filePath);
            if (xml != null)
            {
                string description = "";
                if (xml.Items.Length == xml.ItemsElementName.Length)
                {
                    for (int i = 0; i < xml.Items.Length; i++)
                    {
                        if (xml.ItemsElementName[i] == ItemsChoiceType.Description)
                        {
                            description = (string) xml.Items[i];
                        }
                    }
                }
                return description;
            }
            else
            {
                return null;
            }
        }
    }
}