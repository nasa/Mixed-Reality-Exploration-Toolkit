// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using GOV.NASA.GSFC.XR.Utilities;
using GOV.NASA.GSFC.XR.XRC;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Part;

namespace GOV.NASA.GSFC.XR.MRET.Integrations.XRC
{
    public class SessionEntityDeprecated
    {
        public string tag;
        public EntityEventParametersDeprecated.EntityType type = EntityEventParametersDeprecated.EntityType.sessionObject;
        public byte[] resource;
        public string category;
        public string subcategory;
        public string color;
        public string bundle;
        public string uuid;
        public int userType;
        public string parentUUID;
        public InteractablePartDeprecated.InteractablePartSettings settings;
        public string title;
        public string text;
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

        public SessionEntityDeprecated(string _tag, EntityEventParametersDeprecated.EntityType _type, byte[] _resource,
            string _category, string _subcategory, string _color, int _userType,
            string _bundle, string _uuid, string _parentUUID,
            InteractablePartDeprecated.InteractablePartSettings _settings,
            string _title, string _text, string _lcUUID, string _rcUUID,
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
            title = _title;
            text = _text;
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