using System.Collections.Generic;
using GSFC.ARVR.MRET.Common.Schemas;

namespace GSFC.ARVR.UTILITIES
{
    public class TypeConversion
    {
        public static string Vector3TypeToCSV(Vector3Type[] input)
        {
            string output = "";
            foreach (Vector3Type v3 in input)
            {
                output = output + v3.X + "," + v3.Y + "," + v3.Z + ";";
            }

            return output;
        }

        public static Vector3Type[] CSVToVector3Type(string input)
        {
            List<Vector3Type> output = new List<Vector3Type>();
            foreach (string coordPoint in input.Split(new char[] { ';' }))
            {
                string[] coordVals = coordPoint.Split(new char[] { ',' });
                if (coordVals.Length != 3)
                {
                    continue;
                }
                else
                {
                    output.Add(new Vector3Type()
                    {
                        X = float.Parse(coordVals[0]),
                        Y = float.Parse(coordVals[1]),
                        Z = float.Parse(coordVals[2])
                    });
                }
            }

            return output.ToArray();
        }

        public static LineDrawingUnitsType XRCToLDUnits(XRC.UnitType input)
        {
            switch (input)
            {
                case XRC.UnitType.centimeter:
                    return LineDrawingUnitsType.centimeters;

                case XRC.UnitType.foot:
                    return LineDrawingUnitsType.feet;

                case XRC.UnitType.inch:
                    return LineDrawingUnitsType.inches;

                case XRC.UnitType.meter:
                    return LineDrawingUnitsType.meters;

                case XRC.UnitType.millimeter:
                    return LineDrawingUnitsType.millimeters;

                case XRC.UnitType.yard:
                    return LineDrawingUnitsType.yards;

                default:
                    return LineDrawingUnitsType.meters;
            }
        }

        public static XRC.UnitType LDToXRCUnits(LineDrawingUnitsType input)
        {
            switch (input)
            {
                case LineDrawingUnitsType.centimeters:
                    return XRC.UnitType.centimeter;

                case LineDrawingUnitsType.feet:
                    return XRC.UnitType.foot;

                case LineDrawingUnitsType.inches:
                    return XRC.UnitType.inch;

                case LineDrawingUnitsType.meters:
                    return XRC.UnitType.meter;

                case LineDrawingUnitsType.millimeters:
                    return XRC.UnitType.millimeter;

                case LineDrawingUnitsType.yards:
                    return XRC.UnitType.yard;

                default:
                    return XRC.UnitType.meter;
            }
        }
    }
}