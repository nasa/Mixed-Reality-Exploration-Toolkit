// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.Action
{
    /// <remarks>
    /// History:
    /// 3 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// IAddIdentifiableObjectAction
	///
	/// An add identifiable object action in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IAddIdentifiableObjectAction : IIdentifiableObjectAction
    {
        /// <seealso cref="IIdentifiableObjectAction.CreateSerializedType"/>
        new public AddIdentifiableObjectActionType CreateSerializedType();

        /// <summary>
        /// The serialized identifiable object to instantiate
        /// </summary>
        public IdentifiableType SerializedActionObject { get; }

        /// <seealso cref="IIdentifiableObjectAction.Deserialize(IdentifiableObjectActionType, Action{bool, string})"/>
        public void Deserialize(AddIdentifiableObjectActionType serialized, Action<bool, string> onFinished = null);

        /// <seealso cref="IIdentifiableObjectAction.Serialize(IdentifiableObjectActionType, Action{bool, string})"/>
        public void Serialize(AddIdentifiableObjectActionType serialized, Action<bool, string> onFinished = null);
    }

    /// <remarks>
    /// History:
    /// 3 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// IIdentifiableObjectAction
	///
	/// A <generic> add identifiable object action in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IAddIdentifiableObjectAction<T, C, I> : IIdentifiableObjectAction<T, I>, IAddIdentifiableObjectAction
        where T : AddIdentifiableObjectActionType
        where C : IdentifiableType
        where I : IIdentifiable
    {
        /// <seealso cref="IAddIdentifiableObjectAction.SerializedActionObject"/>
        new public C SerializedActionObject { get; }
    }
}
