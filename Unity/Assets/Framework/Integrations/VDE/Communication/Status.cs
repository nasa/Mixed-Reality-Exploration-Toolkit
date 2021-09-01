/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
namespace Assets.VDE.Communication
{
    public class Status
    {
        public bool status { get; set; } = false;
        public string name { get; set; } = "";
        public int[] ints { get; set; } = new int[] { };
        public string description { get; set; } = "";
        public string failureDescription { get; set; } = "";
        public float[] floats { get; set; } = new float[] { };
    }
}
