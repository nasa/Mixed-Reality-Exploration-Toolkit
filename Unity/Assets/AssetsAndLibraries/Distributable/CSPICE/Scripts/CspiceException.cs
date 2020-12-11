using System;
using CSPICE;

namespace CSPICE
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