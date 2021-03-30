// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using AOT;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace GSFC.ARVR.XRC
{
    public class DebugCPP : MonoBehaviour
    {

        // Use this for initialization
        void OnEnable()
        {
            RegisterDebugCallback(OnDebugCallback);
        }

        //------------------------------------------------------------------------------------------------
        [DllImport("XRCUnity.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern void RegisterDebugCallback(debugCallback cb);
        //Create string param callback delegate
        delegate void debugCallback(IntPtr request, int color, int size);
        enum Color { red, green, blue, black, white, yellow, orange };
        [MonoPInvokeCallback(typeof(debugCallback))]
        static void OnDebugCallback(IntPtr request, int color, int size)
        {
            //Ptr to string
            string debug_string = Marshal.PtrToStringAnsi(request, size);

            //Add Specified Color
            debug_string =
                String.Format("{0}{1}{2}{3}{4}",
                "<color=",
                ((Color)color).ToString(),
                ">",
                debug_string,
                "</color>"
                );

            UnityEngine.Debug.Log(debug_string);
        }
    }
}