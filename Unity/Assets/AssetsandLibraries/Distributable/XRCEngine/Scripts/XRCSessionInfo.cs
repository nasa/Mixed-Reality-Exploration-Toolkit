// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

namespace GSFC.ARVR.XRC
{
    public class XRCSessionInfo
    {
        public long numUsers;
        public string id;
        public string group;
        public string project;
        public string session;
        public string platform;

        public XRCSessionInfo(long _users, string _id, string _group, string _proj, string _sess, string _plat)
        {
            numUsers = _users;
            id = _id;
            group = _group;
            project = _proj;
            session = _sess;
            platform = _plat;
        }
    }
}