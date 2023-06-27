// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using GOV.NASA.GSFC.XR.XRC;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.Integrations.XRC
{
    public interface IUserParameters : IEntityParameters
    {
        new public UserType SerializedEntity { get; }
    }

    public interface IUserParameters<T> : IEntityParameters<T>, IUserParameters
        where T : UserType
    {
    }

    public class UserParameters<T> : EntityParameters<T>, IUserParameters<T>
        where T : UserType, new()
    {
        public string UserID { get => XRCUnity.GetXRCEntityID(SerializedEntity); }

        UserType IUserParameters.SerializedEntity => SerializedEntity;

        public UserParameters(T serializedUser) : base(EntityType.user, serializedUser, null)
        {
        }
    }
}