// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

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