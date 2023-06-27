/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
using System.Collections.Generic;

namespace Assets.VDE
{
    [System.Serializable]
    public class Config
    {
        public Dictionary<string, Layouts.Config> layouts = new Dictionary<string, Layouts.Config> { };
        public Dictionary<string, string> VDE = new Dictionary<string, string> { };
        public Dictionary<string, string> UI = new Dictionary<string, string> { };
    }
}
