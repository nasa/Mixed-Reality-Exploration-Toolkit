/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
namespace Assets.VDE.Communication
{
    public class Progress
    {
        public string name { get; set; } = "";
        public int[] ints { get; set; } = new int[] { };
        public string description { get; set; } = "";
        public float[] floats { get; set; } = new float[] { };
            
        public Grade grade { get; set; } = Grade.blue;
        public enum Grade
        {
            blue,
            green,
            red,
            yellow
        }
    }
}
