// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using GSFC.ARVR.UTILITIES;

namespace GSFC.ARVR.XRC
{
    public class EntityEventParameters
    {
        public enum EntityType { sessionObject, sessionUser}

        public string tag;
        public EntityType type;
        public byte[] resource;
        public string category;
        public string subcategory;
        public string bundle;
        public string uuid;
        public string parentUUID;
        public InteractablePart.InteractablePartSettings settings;
        public string title;
        public string text;
        public Vector3d pos;
        public Quaterniond rot;
        public Vector3d scale;
        public UnitType posUnits;
        public UnitType rotUnits;
        public UnitType sclUnits;

        public EntityEventParameters(string _tag, int _type, byte[] _resource,
            string _category, string _subcategory, string _bundle,
            string _uuid, string _parentUUID,
            InteractablePart.InteractablePartSettings _settings,
            Vector3d _pos, Quaterniond _rot, Vector3d _scl,
            UnitType _posU = UnitType.meter, UnitType _rotU = UnitType.degrees,
            UnitType _sclU = UnitType.meter)
        {
            tag = _tag;
            type = _type == 0 ? EntityType.sessionObject : EntityType.sessionUser;
            resource = _resource;
            category = _category;
            subcategory = _subcategory;
            bundle = _bundle;
            uuid = _uuid;
            parentUUID = _parentUUID;
            settings = _settings;
            pos = _pos;
            rot = _rot;
            scale = _scl;
            posUnits = _posU;
            rotUnits = _rotU;
            sclUnits = _sclU;
        }

        public EntityEventParameters(string _tag, int _type, byte[] _resource,
            string _category, string _subcategory, string _bundle,
            string _uuid, string _parentUUID,
            InteractablePart.InteractablePartSettings _settings,
            string _title, string _text,
            double _xPos, double _yPos, double _zPos, double _xRot, double _yRot,
            double _zRot, double _wRot, double _xScl, double _yScl, double _zScl,
            UnitType _posU = UnitType.meter, UnitType _rotU = UnitType.degrees,
            UnitType _sclU = UnitType.meter)
        {
            tag = _tag;
            type = _type == 0 ? EntityType.sessionObject : EntityType.sessionUser;
            resource = _resource;
            category = _category;
            subcategory = _subcategory;
            bundle = _bundle;
            uuid = _uuid;
            parentUUID = _parentUUID;
            settings = _settings;
            title = _title;
            text = _text;
            pos = new Vector3d(_xPos, _yPos, _zPos);
            rot = new Quaterniond(_xRot, _yRot, _zRot, _wRot);
            scale = new Vector3d(_xScl, _yScl, _zScl);
            posUnits = _posU;
            rotUnits = _rotU;
            sclUnits = _sclU;
        }

        public EntityEventParameters(string _tag,
            string _category, string _subcategory,
            string _uuid, string _parentUUID,
            InteractablePart.InteractablePartSettings _settings)
        {
            tag = _tag;
            category = _category;
            subcategory = _subcategory;
            uuid = _uuid;
            parentUUID = _parentUUID;
            settings = _settings;
        }

        public static EntityType EntityTypeFromInt(int intVal)
        {
            switch (intVal)
            {
                case 0:
                    return EntityType.sessionObject;

                case 1:
                    return EntityType.sessionUser;

                default:
                    UnityEngine.Debug.LogError("[EntityEventParameter->EntityTypeFromInt] Invalid Integer Value.");
                    return EntityType.sessionObject;
            }
        }

        public static EntityType EntityTypeFromXRCEntityType(XRC.EntityType etype)
        {
            switch (etype)
            {
                case XRC.EntityType.entity:
                    return EntityType.sessionObject;

                case XRC.EntityType.user:
                    return EntityType.sessionUser;

                default:
                    return EntityType.sessionObject;
            }
        }
    }
}