// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using System;
#if !HOLOLENS_BUILD
using System.Runtime.InteropServices;
#endif

namespace GOV.NASA.GSFC.XR.GMSEC
{
    public class MonoGMSEC : MonoBehaviour
    {
        static string GMSEC_PACKAGE_NAME = "UnityGMSEC";

#region GMSECDEFS
        public readonly int GMSEC_WAIT_FOREVER = -1;
#endregion
#if !HOLOLENS_BUILD
#region DLLImports
        [DllImport("GMSECUnityCPP", EntryPoint = "CreateConfig")]
        private static extern void I_CreateConfig();

        [DllImport("GMSECUnityCPP", EntryPoint = "AddToConfig")]
        private static extern void I_AddToConfig(string key, string value);

        [DllImport("GMSECUnityCPP", EntryPoint = "Connect")]
        private static extern void I_Connect();

        [DllImport("GMSECUnityCPP", EntryPoint = "Disconnect")]
        private static extern void I_Disconnect();

        [DllImport("GMSECUnityCPP", EntryPoint = "CreateNewMessage")]
        private static extern void I_CreateNewMessage(string subject);

        [DllImport("GMSECUnityCPP", EntryPoint = "AddBinaryFieldToMessage")]
        private static extern void I_AddBinaryFieldToMessage(string fieldName, byte value, int len);

        [DllImport("GMSECUnityCPP", EntryPoint = "AddBoolFieldToMessage")]
        private static extern void I_AddBoolFieldToMessage(string fieldName, bool value);

        [DllImport("GMSECUnityCPP", EntryPoint = "AddCharFieldToMessage")]
        private static extern void I_AddCharFieldToMessage(string fieldName, char value);

        [DllImport("GMSECUnityCPP", EntryPoint = "AddF32FieldToMessage")]
        private static extern void I_AddF32FieldToMessage(string fieldName, float value);

        [DllImport("GMSECUnityCPP", EntryPoint = "AddF64FieldToMessage")]
        private static extern void I_AddF64FieldToMessage(string fieldName, double value);

        [DllImport("GMSECUnityCPP", EntryPoint = "AddI8FieldToMessage")]
        private static extern void I_AddI8FieldToMessage(string fieldName, sbyte value);

        [DllImport("GMSECUnityCPP", EntryPoint = "AddI16FieldToMessage")]
        private static extern void I_AddI16FieldToMessage(string fieldName, Int16 value);

        [DllImport("GMSECUnityCPP", EntryPoint = "AddI32FieldToMessage")]
        private static extern void I_AddI32FieldToMessage(string fieldName, Int32 value);

        [DllImport("GMSECUnityCPP", EntryPoint = "AddI64FieldToMessage")]
        private static extern void I_AddI64FieldToMessage(string fieldName, Int64 value);

        [DllImport("GMSECUnityCPP", EntryPoint = "AddStringFieldToMessage")]
        private static extern void I_AddStringFieldToMessage(string fieldName, string value);

        [DllImport("GMSECUnityCPP", EntryPoint = "AddU8FieldToMessage")]
        private static extern void I_AddU8FieldToMessage(string fieldName, byte value);

        [DllImport("GMSECUnityCPP", EntryPoint = "AddU16FieldToMessage")]
        private static extern void I_AddU16FieldToMessage(string fieldName, UInt16 value);

        [DllImport("GMSECUnityCPP", EntryPoint = "AddU32FieldToMessage")]
        private static extern void I_AddU32FieldToMessage(string fieldName, UInt32 value);

        [DllImport("GMSECUnityCPP", EntryPoint = "AddU64FieldToMessage")]
        private static extern void I_AddU64FieldToMessage(string fieldName, UInt64 value);

        [DllImport("GMSECUnityCPP", EntryPoint = "ClearFieldsFromMessage")]
        private static extern void I_ClearFieldsFromMessage();

        [DllImport("GMSECUnityCPP", EntryPoint = "ClearFieldFromMessage")]
        private static extern void I_ClearFieldFromMessage(string fieldName);

        [DllImport("GMSECUnityCPP", EntryPoint = "PublishMessage")]
        private static extern void I_PublishMessage();

        [DllImport("GMSECUnityCPP", EntryPoint = "Subscribe")]
        private static extern void I_Subscribe(string subject);

        [DllImport("GMSECUnityCPP", EntryPoint = "Unsubscribe")]
        private static extern void I_Unsubscribe(string subject);

        [DllImport("GMSECUnityCPP", EntryPoint = "Receive")]
        private static extern void I_Receive(int timeout, byte[] buf);

        [DllImport("GMSECUnityCPP", EntryPoint = "Request")]
        private static extern void I_Request(string requestMessage, int timeout, byte[] receivedBuf);

        [DllImport("GMSECUnityCPP", EntryPoint = "Reply")]
        private static extern void I_Reply(string requestMessage, string replyMessage);
        #endregion
#endif

#region Wrappers
        public void CreateConfig()
        {
#if !HOLOLENS_BUILD
            I_CreateConfig();
#endif
        }

        public void AddToConfig(string name, string value)
        {
#if !HOLOLENS_BUILD
            I_AddToConfig(name, value);
#endif
        }

        public void Connect()
        {
#if !HOLOLENS_BUILD
            I_Connect();
#endif
        }

        public void Disconnect()
        {
#if !HOLOLENS_BUILD
            I_Disconnect();
#endif
        }

        public void CreateNewMessage(string subject)
        {
#if !HOLOLENS_BUILD
            I_CreateNewMessage(subject);
#endif
        }

        public void AddBinaryFieldToMessage(string fieldName, byte value, int len)
        {
#if !HOLOLENS_BUILD
            I_AddBinaryFieldToMessage(fieldName, value, len);
#endif
        }

        public void AddBoolFieldToMessage(string fieldName, bool value)
        {
#if !HOLOLENS_BUILD
            I_AddBoolFieldToMessage(fieldName, value);
#endif
        }

        public void AddCharFieldToMessage(string fieldName, char value)
        {
#if !HOLOLENS_BUILD
            I_AddCharFieldToMessage(fieldName, value);
#endif
        }

        public void AddF32FieldToMessage(string fieldName, float value)
        {
#if !HOLOLENS_BUILD
            I_AddF32FieldToMessage(fieldName, value);
#endif
        }

        public void AddF64FieldToMessage(string fieldName, double value)
        {
#if !HOLOLENS_BUILD
            I_AddF64FieldToMessage(fieldName, value);
#endif
        }

        public void AddI8FieldToMessage(string fieldName, sbyte value)
        {
#if !HOLOLENS_BUILD
            I_AddI8FieldToMessage(fieldName, value);
#endif
        }

        public void AddI16FieldToMessage(string fieldName, Int16 value)
        {
#if !HOLOLENS_BUILD
            I_AddI16FieldToMessage(fieldName, value);
#endif
        }

        public void AddI32FieldToMessage(string fieldName, Int32 value)
        {
#if !HOLOLENS_BUILD
            I_AddI32FieldToMessage(fieldName, value);
#endif
        }

        public void AddI64FieldToMessage(string fieldName, Int64 value)
        {
#if !HOLOLENS_BUILD
            I_AddI64FieldToMessage(fieldName, value);
#endif
        }

        public void AddStringFieldToMessage(string fieldName, string value)
        {
#if !HOLOLENS_BUILD
            I_AddStringFieldToMessage(fieldName, value);
#endif
        }

        public void AddU8FieldToMessage(string fieldName, byte value)
        {
#if !HOLOLENS_BUILD
            I_AddU8FieldToMessage(fieldName, value);
#endif
        }

        public void AddU16FieldToMessage(string fieldName, UInt16 value)
        {
#if !HOLOLENS_BUILD
            I_AddU16FieldToMessage(fieldName, value);
#endif
        }

        public void AddU32FieldToMessage(string fieldName, UInt32 value)
        {
#if !HOLOLENS_BUILD
            I_AddU32FieldToMessage(fieldName, value);
#endif
        }

        public void AddU64FieldToMessage(string fieldName, UInt64 value)
        {
#if !HOLOLENS_BUILD
            I_AddU64FieldToMessage(fieldName, value);
#endif
        }

        public void ClearFieldsFromMessage()
        {
#if !HOLOLENS_BUILD
            I_ClearFieldsFromMessage();
#endif
        }

        public void ClearFieldFromMessage(string fieldName)
        {
#if !HOLOLENS_BUILD
            I_ClearFieldFromMessage(fieldName);
#endif
        }

        public void PublishMessage()
        {
#if !HOLOLENS_BUILD
            I_PublishMessage();
#endif
        }

        public void Subscribe(string subject)
        {
#if !HOLOLENS_BUILD
            I_Subscribe(subject);
#endif
        }

        public void Unsubscribe(string subject)
        {
#if !HOLOLENS_BUILD
            I_Unsubscribe(subject);
#endif
        }

        public GMSECMessage Receive(int timeout)
        {   // Getting a string back via PInvoke is really complicated
            // ...and not fun.
#if !HOLOLENS_BUILD
            byte[] buf = new byte[65536];
            I_Receive(timeout, buf);
#else
            byte[] buf = System.Text.Encoding.ASCII.GetBytes("0");
#endif

            string result = System.Text.Encoding.ASCII.GetString(buf);
            if (result.StartsWith("0") == false)
            {
                GMSECMessage msg = new GMSECMessage(result);
                return msg;
            }
            else
            {
                return null;
            }
        }

        public GMSECMessage Request(GMSECMessage requestMessage, int timeout)
        {   // Getting a string back via PInvoke is really complicated
            // ...and not fun.
#if !HOLOLENS_BUILD
            byte[] receiveBuf = new byte[65536];
            I_Request(requestMessage.ToJSON(), timeout, receiveBuf);
#else
            byte[] receiveBuf = System.Text.Encoding.ASCII.GetBytes("0");
#endif

            string result = System.Text.Encoding.ASCII.GetString(receiveBuf);
            if (result.StartsWith("0") == false)
            {
                GMSECMessage msg = new GMSECMessage(result);
                return msg;
            }
            else
            {
                return null;
            }
        }

        public void Reply(GMSECMessage requestMessage, GMSECMessage replyMessage)
        {
#if !HOLOLENS_BUILD
            I_Reply(requestMessage.ToJSON(), replyMessage.ToJSON());
#endif
        }
#endregion

        private void Awake()
        {
            string gmsecPath;

            // Get the GMSEC package path
            gmsecPath = PackageLoader.GetPackagePath(Application.dataPath, GMSEC_PACKAGE_NAME);

            // Initialize the GMSEC DLL
            PackageLoader.InitializePackagePlugin(gmsecPath);
        }

        public void Initialize()
        {
        }

    }
}