// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;

namespace GOV.NASA.GSFC.XR.MRET.Integrations.IoT
{
    /// <summary>
    /// IoT Exception
    /// </summary>
    [System.Obsolete("Refer to the " + nameof(GOV.NASA.GSFC.XR.MRET.Interfaces.ThirdPartyInterfaceException) + " class")]
    public class IoTExceptionDeprecated : Exception
    {
        public IoTExceptionDeprecated(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// IoT Server Exception
    /// </summary>
    [System.Obsolete("Refer to the " + nameof(GOV.NASA.GSFC.XR.MRET.Interfaces.ServerException) + " class")]
    public class IoTServerExceptionDeprecated : IoTExceptionDeprecated
    {
        public IoTServerExceptionDeprecated(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// IoT Server Connection Exception
    /// </summary>
    [System.Obsolete("Refer to the " + nameof(GOV.NASA.GSFC.XR.MRET.Interfaces.ServerConnectionException) + " class")]
    public class IoTServerConnectionExceptionDeprecated : IoTServerExceptionDeprecated
    {
        public IoTServerConnectionExceptionDeprecated(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// IoT Client Exception
    /// </summary>
    [System.Obsolete("Refer to the " + nameof(GOV.NASA.GSFC.XR.MRET.Interfaces.ClientException) + " class")]
    public class IoTClientExceptionDeprecated : IoTExceptionDeprecated
    {
        public IoTClientExceptionDeprecated(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// IoT Client Connection Exception
    /// </summary>
    [System.Obsolete("Refer to the " + nameof(GOV.NASA.GSFC.XR.MRET.Interfaces.ClientConnectionException) + " class")]
    public class IoTClientConnectionExceptionDeprecated : IoTClientExceptionDeprecated
    {
        public IoTClientConnectionExceptionDeprecated(string message) : base(message)
        {
        }
    }

}
