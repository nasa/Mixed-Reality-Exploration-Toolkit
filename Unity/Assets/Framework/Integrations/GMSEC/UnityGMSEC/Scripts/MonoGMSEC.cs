// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using System;
#if !HOLOLENS_BUILD
using System.Runtime.InteropServices;
#endif

namespace GSFC.ARVR.GMSEC
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

#region Wrappers
        public void CreateConfig()
        {
            I_CreateConfig();
        }

        public void AddToConfig(string name, string value)
        {
            I_AddToConfig(name, value);
        }

        public void Connect()
        {
            I_Connect();
        }

        public void Disconnect()
        {
            I_Disconnect();
        }

        public void CreateNewMessage(string subject)
        {
            I_CreateNewMessage(subject);
        }

        public void AddBinaryFieldToMessage(string fieldName, byte value, int len)
        {
            I_AddBinaryFieldToMessage(fieldName, value, len);
        }

        public void AddBoolFieldToMessage(string fieldName, bool value)
        {
            I_AddBoolFieldToMessage(fieldName, value);
        }

        public void AddCharFieldToMessage(string fieldName, char value)
        {
            I_AddCharFieldToMessage(fieldName, value);
        }

        public void AddF32FieldToMessage(string fieldName, float value)
        {
            I_AddF32FieldToMessage(fieldName, value);
        }

        public void AddF64FieldToMessage(string fieldName, double value)
        {
            I_AddF64FieldToMessage(fieldName, value);
        }

        public void AddI8FieldToMessage(string fieldName, sbyte value)
        {
            I_AddI8FieldToMessage(fieldName, value);
        }

        public void AddI16FieldToMessage(string fieldName, Int16 value)
        {
            I_AddI16FieldToMessage(fieldName, value);
        }

        public void AddI32FieldToMessage(string fieldName, Int32 value)
        {
            I_AddI32FieldToMessage(fieldName, value);
        }

        public void AddI64FieldToMessage(string fieldName, Int64 value)
        {
            I_AddI64FieldToMessage(fieldName, value);
        }

        public void AddStringFieldToMessage(string fieldName, string value)
        {
            I_AddStringFieldToMessage(fieldName, value);
        }

        public void AddU8FieldToMessage(string fieldName, byte value)
        {
            I_AddU8FieldToMessage(fieldName, value);
        }

        public void AddU16FieldToMessage(string fieldName, UInt16 value)
        {
            I_AddU16FieldToMessage(fieldName, value);
        }

        public void AddU32FieldToMessage(string fieldName, UInt32 value)
        {
            I_AddU32FieldToMessage(fieldName, value);
        }

        public void AddU64FieldToMessage(string fieldName, UInt64 value)
        {
            I_AddU64FieldToMessage(fieldName, value);
        }

        public void ClearFieldsFromMessage()
        {
            I_ClearFieldsFromMessage();
        }

        public void ClearFieldFromMessage(string fieldName)
        {
            I_ClearFieldFromMessage(fieldName);
        }

        public void PublishMessage()
        {
            I_PublishMessage();
        }

        public void Subscribe(string subject)
        {
            I_Subscribe(subject);
        }

        public void Unsubscribe(string subject)
        {
            I_Unsubscribe(subject);
        }

        public GMSECMessage Receive(int timeout)
        {   // Getting a string back via PInvoke is really complicated
            // ...and not fun.
            byte[] buf = new byte[65536];
            I_Receive(timeout, buf);
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
            byte[] receiveBuf = new byte[65536];
            I_Request(requestMessage.ToJSON(), timeout, receiveBuf);
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
            I_Reply(requestMessage.ToJSON(), replyMessage.ToJSON());
        }
#endregion
#endif

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