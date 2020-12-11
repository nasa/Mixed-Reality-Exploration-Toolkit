using UnityEngine;

public class TimeStampUtility
{
    public static void LogTime(string description)
    {
        System.DateTime now = System.DateTime.Now;
        Debug.Log("[TimeStampUtility] " + description + " "
            + now.Year + "-" + now.DayOfYear + "-" + now.Hour
            + ":" + now.Minute + ":" + now.Second + "." + now.Millisecond);
    }
}