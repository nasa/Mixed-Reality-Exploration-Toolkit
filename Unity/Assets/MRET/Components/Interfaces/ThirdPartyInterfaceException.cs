// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;

namespace GOV.NASA.GSFC.XR.MRET.Interfaces
{
    /// <remarks>
    /// History:
    /// 26 January 2021: Created
    /// </remarks>
	///
	/// <summary>
	/// ThirdPartyInterfaceException
	///
	/// Exception classes for the client/server interfaces in MRET
	///
    /// Author: Jeffrey Hosler (migrated from Tyler Kitts' IoT implementation)
	/// </summary>
	/// 
    public class ThirdPartyInterfaceException : Exception
    {
        public int reportingDelaySeconds = 0;

        public ThirdPartyInterfaceException(string message, int delaySeconds = 0) : base(message)
        {
            reportingDelaySeconds = delaySeconds;
        }
    }

    /// <summary>
    /// Client Interface Exception
    /// </summary>
    public class ClientInterfaceException : ThirdPartyInterfaceException
    {
        public ClientInterfaceException(string message, int delaySeconds = 0) : base(message, delaySeconds)
        {
        }
    }

    /// <summary>
    /// Server Exception
    /// </summary>
    public class ServerException : ClientInterfaceException
    {
        public ServerException(string message, int delaySeconds = 0) : base(message, delaySeconds)
        {
        }
    }

    /// <summary>
    /// Server Connection Exception
    /// </summary>
    public class ServerConnectionException : ServerException
    {
        public ServerConnectionException(string message, int delaySeconds = 0) : base(message, delaySeconds)
        {
        }
    }

    /// <summary>
    /// Server Disconnect Exception
    /// </summary>
    public class ServerDisconnectException : ServerException
    {
        public ServerDisconnectException(string message, int delaySeconds = 0) : base(message, delaySeconds)
        {
        }
    }

    /// <summary>
    /// Client Exception
    /// </summary>
    public class ClientException : ClientInterfaceException
    {
        public ClientException(string message, int delaySeconds = 0) : base(message, delaySeconds)
        {
        }
    }

    /// <summary>
    /// Client Connection Exception
    /// </summary>
    public class ClientConnectionException : ClientException
    {
        public ClientConnectionException(string message, int delaySeconds = 0) : base(message, delaySeconds)
        {
        }
    }
}
