﻿using System;

namespace GSFC.ARVR.MRET.Common.Schemas
{
    public class HUDFileSchema : BaseSchema
    {
        public static string fileExtension;
        public static Type serializedType;
        public static Type deserializedType;

        public new static void InitializeSchema()
        {
            // Including the ".".
            fileExtension = ".mhud";

            serializedType = typeof(HUDType);

            deserializedType = typeof(HUDType);
        }

        public static object FromXML(string filePath)
        {
            return FromXML(filePath, serializedType);
        }

        public static void ToXML(string filePath, object deserialized)
        {
            ToXML(filePath, deserialized, serializedType);
        }
    }
}