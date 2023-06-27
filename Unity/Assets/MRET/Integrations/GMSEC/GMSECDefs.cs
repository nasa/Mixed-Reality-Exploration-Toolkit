// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;

namespace GOV.NASA.GSFC.XR.MRET.Integrations.GMSEC
{
    public class GMSECDefs : MonoBehaviour
    {
        public enum ConnectionTypes { bolt, mb, amq383, amq384, ws71, ws75, ws80 };

        public static int ConnectionTypesToInt(ConnectionTypes connType)
        {
            switch (connType)
            {
                case ConnectionTypes.amq383:
                    return 2;

                case ConnectionTypes.amq384:
                    return 3;

                case ConnectionTypes.bolt:
                    return 0;

                case ConnectionTypes.mb:
                    return 1;

                case ConnectionTypes.ws71:
                    return 4;

                case ConnectionTypes.ws75:
                    return 5;

                case ConnectionTypes.ws80:
                    return 6;

                default:
                    return 0;
            }
        }
    }
}