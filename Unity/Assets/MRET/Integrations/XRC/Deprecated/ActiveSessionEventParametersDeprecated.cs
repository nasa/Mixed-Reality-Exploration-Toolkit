// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;

namespace GOV.NASA.GSFC.XR.MRET.Integrations.XRC
{
    [Obsolete("Refer to " + nameof(GOV.NASA.GSFC.XR.MRET.Integrations.XRC.IEntityParameters))]
    public class ActiveSessionEventParametersDeprecated
    {
        public string id;
        public string tag;
        public int type;
        public string color;
        public string lcID;
        public string rcID;
        public string lpID;
        public string rpID;

        public ActiveSessionEventParametersDeprecated(string _id, string _tag, int _type,
            string _color, string _lcID, string _rcID, string _lpID, string _rpID)
        {
            id = _id;
            tag = _tag;
            type = _type;
            color = _color;
            lcID = _lcID;
            rcID = _rcID;
            lpID = _lpID;
            rpID = _rpID;
        }
    }
}