/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
using Assets.VDE.UI;
using System.Collections.Generic;

namespace Assets.VDE.Layouts
{
    public class Config
    {
        public string name;
        public Dictionary<string, double> settings;
        public Dictionary<string, Dictionary<string, double>> rigidJoints;

        internal bool TryGetValue(string key, out double response)
        {
            if (settings.ContainsKey(key))
            {
                response = settings[key];
                return true;
            }
            response = 0;
            return false;
        }

        internal float GetRigidJointValue(Joint.Type type, string key)
        {
            if (rigidJoints.ContainsKey(type.ToString()) && rigidJoints[type.ToString()].ContainsKey(key))
            {
                return (float) rigidJoints[type.ToString()][key];
            }
            return 0;
        }
    }
}
