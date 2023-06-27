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
	/// IThirdPartyInterface
	///
	/// A third party interface in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IThirdPartyInterface : IIdentifiable
	{
        /// <seealso cref="IIdentifiable.CreateSerializedType"/>
        new public InterfaceType CreateSerializedType();

        /// <seealso cref="IIdentifiable.Deserialize(IdentifiableType, Action{bool, string})"/>
        public void Deserialize(InterfaceType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="IIdentifiable.Serialize(IdentifiableType, Action{bool, string})"/>
        public void Serialize(InterfaceType serialized, Action<bool, string> onFinished = null);
    }

    /// <remarks>
    /// History:
    /// 1 Oct 2021: Created
    /// </remarks>
	///
	/// <summary>
	/// IThirdPartyInterface
	///
	/// A third party interface in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IThirdPartyInterface<T> : IIdentifiable<T>, IThirdPartyInterface
        where T : InterfaceType
    {
    }
}
