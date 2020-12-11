namespace GSFC.ARVR.XRC
{
    public class ActiveSessionEventParameters
    {
        public string id;
        public string tag;
        public int type;
        public string color;
        public string lcID;
        public string rcID;
        public string lpID;
        public string rpID;

        public ActiveSessionEventParameters(string _id, string _tag, int _type,
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