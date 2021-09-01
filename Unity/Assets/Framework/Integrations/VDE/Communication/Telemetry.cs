/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
using System.Collections.Generic;

namespace Assets.VDE.Communication
{
    public struct Telemetry
    {
        public string message { get; set; }
        public Type type { get; set; }
        public enum Type 
        {
            notSet,
            status,
            progress,
            log
        }
        public Status[] status { get; set; }
        public List<Progress> progress { get; set; }
    }
}