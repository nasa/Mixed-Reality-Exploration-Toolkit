// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections.Generic;

using System;

namespace GSFC.ARVR.MRET.Common.Schemas
{
    public class PartFileSchema : BaseSchema
    {
        public static string fileExtension;
        public static Type serializedType;
        public static Type deserializedType;

        public new static void InitializeSchema()
        {
            // Including the ".".
            fileExtension = ".mpart";

            serializedType = typeof(PartType);

            deserializedType = typeof(InteractablePart);
        }

        public static object FromXML(string filePath)
        {
            return FromXML(filePath, serializedType);
        }

        public static void ToXML(string filePath, object deserialized)
        {
            ToXML(filePath, deserialized, serializedType);
        }

        public new static FileInfo.FileFilter[] GetFilters(string filePath)
        {
            PartType xml = (PartType) FromXML(filePath);
            if (xml != null)
            {
                List<FileInfo.FileFilter> filters = new List<FileInfo.FileFilter>();
                // Check if Vendor field exists for part.
                if (xml.Vendor != null)
                {
                    if (xml.Vendor[0] != null && xml.Vendor[0] != "")
                    {
                        filters.Add(new FileInfo.FileFilter("Vendor", xml.Vendor[0]));
                    }
                }

                // Check if Subsystem field exists for part.
                if (xml.Subsystem != null)
                {
                    if (xml.Subsystem != "")
                    {
                        filters.Add(new FileInfo.FileFilter("Subsystem", xml.Subsystem));
                    }
                }
                return filters.ToArray();
            }
            else
            {
                return null;
            }
        }
    }
}