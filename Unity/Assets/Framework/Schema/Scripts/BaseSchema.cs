using System;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

namespace GSFC.ARVR.MRET.Common.Schemas
{
    public abstract class BaseSchema
    {
        public static readonly string NAME = nameof(BaseSchema);

        private BaseSchema instance;
        private string _fileExtension;

        // Must be called to initialize schema information.
        public static void InitializeSchema()
        {
            Debug.LogError("[BaseSchema] InitializeSchema not implemented.");
        }

        public static string GetDescriptionField(string filePath)
        {
            return null;
        }

        public static FileInfo.FileFilter[] GetFilters(string filePath)
        {
            return null;
        }

        public static object FromXML(string filePath, Type serializedType)
        {
            XmlSerializer ser = new XmlSerializer(serializedType);
            XmlReader reader = XmlReader.Create(filePath);
            try
            {
                object deserialized = ser.Deserialize(reader);
                reader.Close();
                return deserialized;
            }
            catch (Exception e)
            {
                Debug.LogError("[" + NAME + "->FromXML] " + e.ToString());
                reader.Close();
                return null;
            }
        }

        public static void ToXML(string filePath, object deserialized, Type serializedType)
        {
            XmlSerializer ser = new XmlSerializer(serializedType);
            XmlWriter writer = XmlWriter.Create(filePath);
            try
            {
                ser.Serialize(writer, deserialized);
                writer.Close();
            }
            catch (Exception e)
            {
                Debug.Log("[" + NAME + "->ToXML] " + e.ToString());
                writer.Close();
            }
        }
    }
}