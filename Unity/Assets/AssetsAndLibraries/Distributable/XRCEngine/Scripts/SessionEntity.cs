using GSFC.ARVR.UTILITIES;

namespace GSFC.ARVR.XRC
{
    public class SessionEntity
    {
        public string tag;
        public EntityEventParameters.EntityType type = EntityEventParameters.EntityType.sessionObject;
        public byte[] resource;
        public string category;
        public string subcategory;
        public string color;
        public string bundle;
        public string uuid;
        public int userType;
        public string parentUUID;
        public InteractablePart.InteractablePartSettings settings;
        public string lcUUID;
        public string rcUUID;
        public string lpUUID;
        public string rpUUID;
        public Vector3d position;
        public Quaterniond rotation;
        public Vector3d scale;
        public UnitType positionUnits;
        public ReferenceSpaceType positionRef;
        public UnitType rotationUnits;
        public ReferenceSpaceType rotationRef;
        public UnitType scaleUnits;
        public ReferenceSpaceType scaleRef;

        public SessionEntity(string _tag, EntityEventParameters.EntityType _type, byte[] _resource,
            string _category, string _subcategory, string _color, int _userType,
            string _bundle, string _uuid, string _parentUUID,
            InteractablePart.InteractablePartSettings _settings, string _lcUUID, string _rcUUID,
            string _lpUUID, string _rpUUID, Vector3d _pos, Quaterniond _rot, Vector3d _scl,
            UnitType _posUnits = UnitType.meter, ReferenceSpaceType _posRef = ReferenceSpaceType.global,
            UnitType _rotUnits = UnitType.meter, ReferenceSpaceType _rotRef = ReferenceSpaceType.global,
            UnitType _sclUnits = UnitType.meter, ReferenceSpaceType _sclRef = ReferenceSpaceType.global)
        {
            tag = _tag;
            type = _type;
            resource = _resource;
            category = _category;
            subcategory = _subcategory;
            color = _color;
            userType = _userType;
            bundle = _bundle;
            uuid = _uuid;
            parentUUID = _parentUUID;
            settings = _settings;
            lcUUID = _lcUUID;
            rcUUID = _rcUUID;
            lpUUID = _lpUUID;
            rpUUID = _rpUUID;
            position = _pos;
            rotation = _rot;
            scale = _scl;
            positionUnits = _posUnits;
            positionRef = _posRef;
            rotationUnits = _rotUnits;
            rotationRef = _rotRef;
            scaleUnits = _sclUnits;
            scaleRef = _sclRef;
        }
    }
}