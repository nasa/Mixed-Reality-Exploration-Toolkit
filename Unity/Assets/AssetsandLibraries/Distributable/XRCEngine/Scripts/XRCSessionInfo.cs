// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

namespace GOV.NASA.GSFC.XR.XRC
{
    public class XRCSessionInfo : IRemoteSession
    {
        public long NumUsers { get; private set; }
        public string id { get; private set; }
        public string Group { get; private set; }
        public string Project { get; private set; }
        public string Session { get; private set; }
        public string Platform { get; private set; }

        public XRCSessionInfo(long _users, string _id, string _group, string _proj, string _sess, string _plat)
        {
            NumUsers = _users;
            id = _id;
            Group = _group;
            Project = _proj;
            Session = _sess;
            Platform = _plat;
        }
    }
}