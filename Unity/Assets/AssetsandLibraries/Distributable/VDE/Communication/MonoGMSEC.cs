#if MRET_2021_OR_LATER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace VDE.Communication
{
    public class MonoGMSEC : GOV.NASA.GSFC.XR.GMSEC.MonoGMSEC
    {
        [DllImport("GMSECUnityCPP", EntryPoint = "Request")]
        private static extern void I_Request(string requestMessage, int timeout, byte[] receivedBuf);

        [DllImport("GMSECUnityCPP", EntryPoint = "Receive")]
        private static extern void I_Receive(int timeout, byte[] buf);
        public new GMSECMessage Receive(int timeout)
        {   // Getting a string back via PInvoke is really complicated
            // ...and not fun.
            byte[] buf = new byte[6553600];
            I_Receive(timeout, buf);
            string result = System.Text.Encoding.ASCII.GetString(buf);
            if (result.StartsWith("0") == false)
            {
                VDE.Communication.GMSECMessage msg = new VDE.Communication.GMSECMessage(result);
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
            byte[] receiveBuf = new byte[6553600];
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
        public new void Initialize()
        {
            string gmsecPath;

            // Get the GMSEC package path
            gmsecPath = GOV.NASA.GSFC.XR.PackageLoader.GetPackagePath(UnityEngine.Application.dataPath, "UnityGMSEC");

            // Initialize the GMSEC DLL
            GOV.NASA.GSFC.XR.PackageLoader.InitializePackagePlugin(gmsecPath);
        }
    }
}
#endif