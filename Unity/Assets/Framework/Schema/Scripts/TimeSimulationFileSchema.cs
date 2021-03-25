// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using UnityEngine;
using GSFC.ARVR.MRET.Time.Simulation;

namespace GSFC.ARVR.MRET.Common.Schemas.TimeSimulationTypes
{
    /**
     * Time simulation file schema<br>
     * 
     * Added the schema handling specifrics for the time simulation file schema.<br>
     * 
     * @author Jeffrey Hosler
     */
    public abstract class TimeSimulationFileSchema : BaseSchema
    {
        public new static readonly string NAME = nameof(TimeSimulationFileSchema);

        public static string fileExtension;
        public static Type serializedType;
        public static Type deserializedType;

        /**
         * Initializes the schema specifics for the time simulation schema<br>
         */
        public new static void InitializeSchema()
        {
            // Including the '.'
            fileExtension = ".mtime";

            serializedType = typeof(TimeSimulationType);

            deserializedType = typeof(TimeSimulation);
        }

        /**
         * Converts a time simulation XML file to the serialized <code>TimeSimulationType</code><br>
         * 
         * @param filePath The file, including path, of the XML file to process
         */
        public static object FromXML(string filePath)
        {
            // Assign the extension if the supplied file doesn't have one specified
            string extension = System.IO.Path.GetExtension(filePath);
            if (string.IsNullOrEmpty(extension))
            {
                filePath += fileExtension;
            }

            return FromXML(filePath, serializedType);
        }

        /**
         * Converts serialized <code>TimeSimulationType</code> to a time simulation XML file<br>
         * 
         * @param filePath The file, including path, of the XML file to write
         * @param deserialized The <code>TimeSimulationType</code> containing the contaents to serrialize
         *      out to the XML file
         */
        public static void ToXML(string filePath, object deserialized)
        {
            // Assign the extension if the supplied file doesn't have one specified
            string extension = System.IO.Path.GetExtension(filePath);
            if (string.IsNullOrEmpty(extension))
            {
                filePath += fileExtension;
            }

            ToXML(filePath, deserialized, serializedType);
        }

        /**
         * Obtains the description field for the time simulation.<br>
         * 
         * @param filePath The file, including path, of the XML file to write
         * 
         * TODO: This doesn't seem optimal to have to deserialize the entire file to get the description.
         *      Copied this logic from the other schema handlers to keep consistent, but could stand to
         *      revisit
         */
        public new static string GetDescriptionField(string filePath)
        {
            string result = "";

            try
            {
                TimeSimulationType xml = (TimeSimulationType)FromXML(filePath);

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
    }

}
 