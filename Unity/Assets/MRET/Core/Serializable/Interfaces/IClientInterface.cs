// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.Interfaces
{
    /// <remarks>
    /// History:
    /// 1 Oct 2021: Created
    /// </remarks>
	///
	/// <summary>
	/// IClientInterface
	///
	/// A third party client interface in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IClientInterface : IThirdPartyInterface
    {
        /// <seealso cref="IThirdPartyInterface.CreateSerializedType"/>
        new public ClientInterfaceType CreateSerializedType();

        /// <summary>
        /// The client interface server.<br>
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// The client interface port.<br>
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Indicates whether or not the server requires certificates for connectivity
        /// </summary>
        public bool UsesCertificates { get; }

        /// <summary>
        /// The client interface connection timeout in seconds.<br>
        /// </summary>
        public float ConnectionTimeout { get; set; }

        /// <summary>
        /// Returns the protocol for connection string. Available for subclasses
        /// to override to provide formatting unique to the client, i.e. adding
        /// a prefix or combining "server:port".
        /// </summary>
        /// <returns>The full server protocol string</returns>
        public string GetConnectionProtocol();

        /// <summary>
        /// Indicates whether or not the GMSEC connection is established
        /// </summary>
        public bool Connected { get; }

        /// <summary>
        /// Called to connect to the server.
        /// </summary>
        public void Connect();

        /// <summary>
        /// Disconnect from the server.
        /// </summary>
        public void Disconnect();

        /// <seealso cref="IThirdPartyInterface.Deserialize(InterfaceType, Action{bool, string})"/>
        public void Deserialize(ClientInterfaceType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="IThirdPartyInterface.Serialize(InterfaceType, Action{bool, string})"/>
        public void Serialize(ClientInterfaceType serialized, Action<bool, string> onFinished = null);
    }

    /// <remarks>
    /// History:
    /// 1 Oct 2021: Created
    /// </remarks>
	///
	/// <summary>
	/// IClientInterface
	///
	/// A third party client interface in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IClientInterface<T> : IThirdPartyInterface<T>, IClientInterface
        where T : ClientInterfaceType
    {
    }
}
