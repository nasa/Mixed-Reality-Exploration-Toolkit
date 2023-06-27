// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;

namespace GOV.NASA.GSFC.XR.CSPICE
{
    [Serializable]
    class CspiceException : Exception
    {
        private string shortStr;
        public string ShortMessage
        {
            get
            {
                return shortStr;
            }

            set
            {
                shortStr = value;
            }
        }

        private string explainStr;
        public string ExplainMessage
        {
            get
            {
                return explainStr;
            }

            set
            {
                explainStr = value;
            }
        }

        private string longStr;
        public string LongMessage
        {
            get
            {
                return longStr;
            }

            set
            {
                longStr = value;
            }
        }

        public override string Message => ShortMessage;

        public CspiceException() : base()
        {
        }
    }
}