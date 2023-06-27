// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;
using UnityEngine.Events;
using GOV.NASA.GSFC.XR.Utilities.Math;
using GOV.NASA.GSFC.XR.XRC;
using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;
using GOV.NASA.GSFC.XR.MRET.Annotation;
using GOV.NASA.GSFC.XR.MRET.SceneObjects;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using AOT;

namespace GOV.NASA.GSFC.XR.MRET.Integrations.XRC
{
    public class XRCUnity : MRETUpdateSingleton<XRCUnity>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(XRCUnity);

        public static readonly int RESOURCEBUFSIZE = 65536;

        /// <summary>
        /// Attribute IDs
        /// </summary>
        private static readonly string
            KEY_TYPE = "TYPE",
            KEY_CATEGORY = "CATEGORY",
            KEY_PARENTID = "PARENT-ID",

            VERSION = ".VERSION",

            GROUP = ".GROUP",

            UUID = ".UUID",
            ID = ".ID",
            NAME = ".NAME",
            DESCRIPTION = ".DESCRIPTION",

            SCENE_OBJECT_CHILDREN = ".CHILDREN",

            XR_AR_ENABLED = ".XR-AR-ENABLED",
            XR_VR_ENABLED = ".XR-VR-ENABLED",

            INTERACTABLE_INTERACTIONS = ".INTERACTIONS",
            INTERACTABLE_ENABLE_INTERACTION = ".ENABLE-INTERACTION",
            INTERACTABLE_ENABLE_USABILITY = ".ENABLE-USABILITY",
            INTERACTABLE_TELEMETRY_SETTINGS = ".TELEMETRY_SETTINGS",
            INTERACTABLE_TELEMETRY_SHADE_FOR_LIMITS = ".SHADE_FOR_LIMITS",
            INTERACTABLE_TELEMETRY_KEYS = ".KEYS",
            INTERACTABLE_HIGHLIGHT_MATERIAL = ".HIGHLIGHT-MATERIAL",
            INTERACTABLE_SELECTION_MATERIAL = ".SELECTION-MATERIAL",

            COLOR_COMPONENT_R = ".R",
            COLOR_COMPONENT_G = ".G",
            COLOR_COMPONENT_B = ".B",
            COLOR_COMPONENT_A = ".A",
            COLOR_GRADIENT_TIME = ".TIME",
            COLOR_GRADIENT_COLOR = ".COLOR",
            COLOR_GRADIENT_RGBA = ".RGBA",

            ASSET_NAME = ".NAME",
            ASSET_BUNDLE = ".BUNDLE",

            MATERIAL_ASSET = ".ASSET",
            MATERIAL_FILE = ".FILE",
            MATERIAL_FILE_FORMAT = ".FORMAT",
            MATERIAL_FILE_VALUE = ".VALUE",
            MATERIAL_SHADER = ".SHADER",
            MATERIAL_SHADER_TYPE = ".TYPE",
            MATERIAL_SHADER_VALUE = ".VALUE",
            MATERIAL_RGBA = ".RGBA",
            MATERIAL_COLOR = ".COLOR",

            MODEL = ".MODEL",
            MODEL_ASSET = ".ASSET",
            MODEL_FILE = ".FILE",
            MODEL_FILE_FORMAT = ".FORMAT",
            MODEL_FILE_VALUE = ".VALUE",
            MODEL_MATERIAL = ".MATERIAL",
            MODEL_SHAPE = ".SHAPE",
            MODEL_SHAPE_TYPE = ".TYPE",
            PHYSICS_SETTINGS = ".PHYSICS-SETTINGS",
            PHYSICS_ENABLE_COLLISIONS = ".ENABLE-COLLISIONS",
            PHYSICS_ENABLE_GRAVITY = ".ENABLE-GRAVITY",
            PHYSICS_MASS = ".MASS",
            SPECS = ".SPECS",
            SPECS_MASS = ".MASS",
            SPECS_MASS_MIN = ".MIN",
            SPECS_MASS_MAX = ".MAX",
            SPECS_MASS_CONTINGENCY = ".CONTINGENCY",
            SPECS_NOTES = ".NOTES",
            SPECS_REFERENCE = ".REFERENCE",
            RANDOMIZE_TEXTURES = ".RANDOMIZE-TEXTURES",
            OPACITY = ".OPACITY",
            VISIBLE = ".VISIBLE",

            PART_SPECS = ".PART-SPECS",
            PART_VENDOR = ".VENDOR",
            PART_VERSION = ".VERSION",
            PART_POWER = ".POWER",
            PART_POWER_MIN = ".MIN",
            PART_POWER_MAX = ".MAX",
            PART_POWER_CONTINGENCY = ".CONTINGENCY",
            PART_CATEGORY = ".CATEGORY",
            PART_SUBSYSTEM = ".SUBSYSTEM",
            PART_ROS_INTERFACE = ".ROS-INTERFACE",
            PART_CHILDREN = ".CHILD-PARTS",
            PART_ENCLOSURE = ".ENCLOSURE",

            ASSEMBLY_INTERACTIONS = ".ASSEMBLY-INTERACTIONS",

            DRAWING_MATERIAL = ".MATERIAL",
            DRAWING_COLOR = ".COLOR",
            DRAWING_RGBA = ".RGBA",
            DRAWING_GRADIENT = ".GRADIENT",
            DRAWING_WIDTH = ".WIDTH",
            DRAWING_POINTS = ".POINTS",
            DRAWING_RENDER_TYPE = ".TYPE",
            DRAWING_DISPLAY_MEASUREMENT = ".DISPLAY-MEASUREMENT",
            DRAWING_UNITS = ".UNITS",
            DRAWING_LENGTH_LIMIT = ".LENGTH-LIMIT",

            DISPLAY_PARENTID = ".PARENTID",
            DISPLAY_TITLE = ".TITLE",
            DISPLAY_WIDTH = ".WIDTH",
            DISPLAY_HEIGHT = ".HEIGHT",
            DISPLAY_STATE = ".STATE",
            DISPLAY_ZORDER = ".ZORDER",

            NOTE_CONTENT = ".CONTENT",
            NOTE_DRAWING = ".DRAWING",

            ANNOTATION_START_DELAY = ".START-DELAY",
            ANNOTATION_LOOP = ".LOOP",
            ANNOTATION_ATTACHTO = ".ATTACHTO",
            TEXT_ANNOTATION_TEXT = ".TEXT",
            TEXT_ANNOTATION_TEXT_INDEX = ".TEXT-INDEX",
            TEXT_ANNOTATION_TIME_PER_TEXT = ".TIME-PER-TEXT",
            SOURCE_ANNOTATION_START_TIME = ".START-TIME",
            SOURCE_ANNOTATION_DURATION = ".DURATION",
            SOURCE_ANNOTATION_SPEED = ".SPEED",

            CONNECTION_SERVER = ".SERVER",
            CONNECTION_PORT = ".PORT",
            CONNECTION_TIMEOUT = ".CONNECTION-TIMEOUT",
            CONNECTION_UPDATE = ".CONNECTION-UPDATE",

            ROS_PROTOCOL = ".PROTOCOL",
            ROS_SERIALIZER = ".SERIALIZER",
            ROS_SUBSCRIBE_TOPIC = ".SUBSCRIBE-TOPIC",
            ROS_PUBLISH_TOPIC = ".PUBLISH-TOPIC",
            ROS_DESCRIPTION = ".DESCRIPTION",

            USER = ".USER",
            USER_ID = USER + ".ID",
            USER_ALIAS = ".ALIAS",
            USER_RGBA = ".RGBA",
            USER_COLOR = ".COLOR",
            USER_HAND = ".HAND",
            USER_HAND_ID = USER_HAND + ".ID",
            USER_HAND_CONTROLLER = USER_HAND + ".CONTROLLER",
            USER_HAND_HANDEDNESS = USER_HAND + ".HANDEDNESS",
            USER_HAND_POINTER = USER_HAND + ".POINTER";

        public LogLevel logLevel;

        public UnityEvent entityCreatedUnityEvent;
        public UnityEvent entityDestroyedUnityEvent;
        public UnityEvent entityReinitializedUnityEvent;
        public UnityEvent entityUpdatedUnityEvent;
        public UnityEvent entityEditedUnityEvent;
        public UnityEvent sessionJoinedUnityEvent;
        public UnityEvent sessionParticipantAddedUnityEvent;
        public UnityEvent sessionParticipantResyncedUnityEvent;
        public UnityEvent sessionParticipantDeletedUnityEvent;
        public UnityEvent remoteSessionAddedUnityEvent;
        public UnityEvent remoteSessionUpdatedUnityEvent;
        public UnityEvent remoteSessionDeletedUnityEvent;

        public static Queue<IEntityParameters> entityCreatedEventQueue = new Queue<IEntityParameters>();
        public static Queue<IEntityParameters> entityDestroyedEventQueue = new Queue<IEntityParameters>();
        public static Queue<IEntityParameters> entityReinitializedEventQueue = new Queue<IEntityParameters>();
        public static Queue<IEntityParameters> entityUpdatedEventQueue = new Queue<IEntityParameters>();
        public static Queue<IEntityParameters> entityEditedEventQueue = new Queue<IEntityParameters>();
        public static Queue<IUserParameters> asParticipantAddedEventQueue = new Queue<IUserParameters>();
        public static Queue<IUserParameters> asParticipantResyncedEventQueue = new Queue<IUserParameters>();
        public static Queue<IUserParameters> asParticipantDeletedEventQueue = new Queue<IUserParameters>();
        public static Queue<RemoteSessionEventParameters> rsAddedEventQueue = new Queue<RemoteSessionEventParameters>();
        public static Queue<RemoteSessionEventParameters> rsUpdatedEventQueue = new Queue<RemoteSessionEventParameters>();
        public static Queue<RemoteSessionEventParameters> rsDeletedEventQueue = new Queue<RemoteSessionEventParameters>();

        private static int maxInvokesPerFrame = 3;

        private ConcurrentQueue<UnityEvent> invocationQueue = new ConcurrentQueue<UnityEvent>();

        /// <summary>
        /// Internal function to obtain an entity blob attribute value from the XRCInterface.
        /// </summary>
        /// <param name="attributeId">The resultant value</param>
        /// <param name="dest">The resultant byte array</param>
        /// <param name="destsz">The max size of the dest array</param>
        /// <param name="dataLen">The number of bytes written to the dest array</param>
        /// <param name="stagedEntityId">If specified, will obtain the value from the specified staged
        ///     entity ID. Otherwise, will assume it was an event that triggered the call.</param>
        /// <returns>A boolean value indicating success</returns>
        public static bool GetEntityBlobAttribute(string attributeId, byte[] dest, long destsz, out int dataLen, string stagedEntityId)
        {
            bool result;

            if (string.IsNullOrEmpty(stagedEntityId))
            {
                result = XRCInterface.GetEventEntityBlobAttribute(attributeId, dest, destsz, out dataLen);
            }
            else
            {
                result = XRCInterface.GetEntityBlobAttribute(stagedEntityId, attributeId, dest, destsz, out dataLen);
            }

            return result;
        }

        /// <summary>
        /// Internal function to set an entity blob attribute value in the XRCInterface.
        /// </summary>
        /// <param name="stagedEntityId">The staged entity ID associated with the attribute</param>
        /// <param name="attributeId">The name of the attribute</param>
        /// <param name="data">The attribute byte array containing the data to stage</param>
        /// <returns>A boolean value indicating success</returns>
        public static bool SetEntityBlobAttribute(string stagedEntityId, string attributeId, byte[] data)
        {
            return XRCInterface.SetEntityBlobAttribute(stagedEntityId, attributeId, data, data.Length);
        }

        /// <summary>
        /// Internal function to obtain an entity boolean attribute value from the XRCInterface.
        /// </summary>
        /// <param name="attributeId">The name of the attribute</param>
        /// <param name="value">The resultant value</param>
        /// <param name="stagedEntityId">If specified, will obtain the value from the specified staged
        ///     entity ID. Otherwise, will assume it was an event that triggered the call.</param>
        /// <returns>A boolean value indicating success</returns>
        private static bool GetEntityBooleanAttribute(string attributeId, out bool value, string stagedEntityId)
        {
            bool result;

            if (string.IsNullOrEmpty(stagedEntityId))
            {
                result = XRCInterface.GetEventEntityBooleanAttribute(attributeId, out value);
            }
            else
            {
                result = XRCInterface.GetEntityBooleanAttribute(stagedEntityId, attributeId, out value);
            }

            return result;
        }

        /// <summary>
        /// Internal function to set an entity boolean attribute value in the XRCInterface.
        /// </summary>
        /// <param name="stagedEntityId">The staged entity ID associated with the attribute</param>
        /// <param name="attributeId">The name of the attribute</param>
        /// <param name="value">The attribute value to stage</param>
        /// <returns>A boolean value indicating success</returns>
        public static bool SetEntityBooleanAttribute(string stagedEntityId, string attributeId, bool value)
        {
            return XRCInterface.SetEntityBooleanAttribute(stagedEntityId, attributeId, value);
        }

        /// <summary>
        /// Internal function to obtain an entity string attribute value from the XRCInterface.
        /// </summary>
        /// <param name="attributeId">The ID of the attribute</param>
        /// <param name="value">The resultant value</param>
        /// <param name="stagedEntityId">If specified, will obtain the value from the specified staged
        ///     entity ID. Otherwise, will assume it was an event that triggered the call.</param>
        /// <returns>A boolean value indicating success</returns>
        private static bool GetEntityStringAttribute(string attributeId, out string value, string stagedEntityId)
        {
            bool result;

            if (string.IsNullOrEmpty(stagedEntityId))
            {
                result = XRCInterface.GetEventEntityStringAttribute(attributeId, out value);
            }
            else
            {
                result = XRCInterface.GetEntityStringAttribute(stagedEntityId, attributeId, out value);
            }

            return result;
        }

        /// <summary>
        /// Internal function to set an entity string attribute value in the XRCInterface.
        /// </summary>
        /// <param name="stagedEntityId">The staged entity ID associated with the attribute</param>
        /// <param name="attributeId">The name of the attribute</param>
        /// <param name="value">The attribute value to stage</param>
        /// <returns>A boolean value indicating success</returns>
        public static bool SetEntityStringAttribute(string stagedEntityId, string attributeId, string value)
        {
            return XRCInterface.SetEntityStringAttribute(stagedEntityId, attributeId, value);
        }

        /// <summary>
        /// Internal function to obtain an entity integer attribute value from the XRCInterface.
        /// </summary>
        /// <param name="attributeId">The name of the attribute</param>
        /// <param name="value">The resultant value</param>
        /// <param name="stagedEntityId">If specified, will obtain the value from the specified staged
        ///     entity ID. Otherwise, will assume it was an event that triggered the call.</param>
        /// <returns>A boolean value indicating success</returns>
        private static bool GetEntityIntegerAttribute(string attributeId, out int value, string stagedEntityId)
        {
            bool result;

            if (string.IsNullOrEmpty(stagedEntityId))
            {
                result = XRCInterface.GetEventEntityIntegerAttribute(attributeId, out value);
            }
            else
            {
                result = XRCInterface.GetEntityIntegerAttribute(stagedEntityId, attributeId, out value);
            }

            return result;
        }

        /// <summary>
        /// Internal function to set an entity integer attribute value in the XRCInterface.
        /// </summary>
        /// <param name="stagedEntityId">The staged entity ID associated with the attribute</param>
        /// <param name="attributeId">The name of the attribute</param>
        /// <param name="value">The attribute value to stage</param>
        /// <returns>A boolean value indicating success</returns>
        public static bool SetEntityIntegerAttribute(string stagedEntityId, string attributeId, int value)
        {
            return XRCInterface.SetEntityIntegerAttribute(stagedEntityId, attributeId, value);
        }

        /// <summary>
        /// Internal function to obtain an entity float attribute value from the XRCInterface.
        /// </summary>
        /// <param name="attributeId">The name of the attribute</param>
        /// <param name="value">The resultant value</param>
        /// <param name="stagedEntityId">If specified, will obtain the value from the specified staged
        ///     entity ID. Otherwise, will assume it was an event that triggered the call.</param>
        /// <returns>A boolean value indicating success</returns>
        private static bool GetEntityFloatAttribute(string attributeId, out float value, string stagedEntityId)
        {
            bool result;

            if (string.IsNullOrEmpty(stagedEntityId))
            {
                result = XRCInterface.GetEventEntityFloatAttribute(attributeId, out value);
            }
            else
            {
                result = XRCInterface.GetEntityFloatAttribute(stagedEntityId, attributeId, out value);
            }

            return result;
        }

        /// <summary>
        /// Internal function to set an entity float attribute value in the XRCInterface.
        /// </summary>
        /// <param name="stagedEntityId">The staged entity ID associated with the attribute</param>
        /// <param name="attributeId">The name of the attribute</param>
        /// <param name="value">The attribute value to stage</param>
        /// <returns>A boolean value indicating success</returns>
        public static bool SetEntityFloatAttribute(string stagedEntityId, string attributeId, float value)
        {
            return XRCInterface.SetEntityFloatAttribute(stagedEntityId, attributeId, value);
        }

        /// <summary>
        /// Internal function to obtain an entity transform attribute value from the XRCInterface.
        /// </summary>
        /// <param name="xPos">The resultant xPos</param>
        /// <param name="yPos">The resultant yPos</param>
        /// <param name="zPos">The resultant zPos</param>
        /// <param name="unitsPos">The resultant unitsPos</param>
        /// <param name="refPos">The resultant refPos</param>
        /// <param name="xScl">The resultant xScl</param>
        /// <param name="yScl">The resultant yScl</param>
        /// <param name="zScl">The resultant zScl</param>
        /// <param name="unitsScl">The resultant unitsScl</param>
        /// <param name="refScl">The resultant refScl</param>
        /// <param name="xRot">The resultant xRot</param>
        /// <param name="yRot">The resultant yRot</param>
        /// <param name="zRot">The resultant zRot</param>
        /// <param name="wRot">The resultant wRot</param>
        /// <param name="unitsRot">The resultant unitsRot</param>
        /// <param name="refRot">The resultant refRot</param>
        /// <param name="stagedEntityId">If specified, will obtain the value from the specified staged
        ///     entity ID. Otherwise, will assume it was an event that triggered the call.</param>
        /// <returns>A boolean value indicating success</returns>
        private static bool GetEntityQTransform(
            out double xPos, out double yPos, out double zPos, out UnitType unitsPos, out XR.XRC.ReferenceSpaceType refPos,
            out double xScl, out double yScl, out double zScl, out UnitType unitsScl, out XR.XRC.ReferenceSpaceType refScl,
            out double xRot, out double yRot, out double zRot, out double wRot, out UnitType unitsRot, out XR.XRC.ReferenceSpaceType refRot,
            string stagedEntityId)
        {
            bool result;

            if (string.IsNullOrEmpty(stagedEntityId))
            {
                result = XRCInterface.GetEventEntityQTransform(
                    out xPos, out yPos, out zPos, out unitsPos, out refPos,
                    out xScl, out yScl, out zScl, out unitsScl, out refScl,
                    out xRot, out yRot, out zRot, out wRot, out unitsRot, out refRot);
            }
            else
            {
                result = XRCInterface.GetEntityQTransform(stagedEntityId,
                    out xPos, out yPos, out zPos, out unitsPos, out refPos,
                    out xScl, out yScl, out zScl, out unitsScl, out refScl,
                    out xRot, out yRot, out zRot, out wRot, out unitsRot, out refRot);
            }

            return result;
        }

        /// <summary>
        /// Internal function to set a staged entity transform attribute value in the XRCInterface.
        /// </summary>
        /// <param name="stagedEntityId">The staged entity ID associated with the transform</param>
        /// <param name="position">The world space <code>Vector3</code> position to stage</param>
        /// <param name="posReference">The <code>ReferenceSpaceType</code> specifying the reference
        ///     space for the position</param>
        /// <param name="rotation">The world space <code>Quaternion</code> rotation to stage</param>
        /// <param name="rotReference">The <code>ReferenceSpaceType</code> specifying the reference
        ///     space for the rotation</param>
        /// <param name="scale">The <code>Vector3</code> scale to stage</param>
        /// <returns>A boolean value indicating success</returns>
        public static bool SetEntityQTransform(string stagedEntityId,
            Vector3 position, Schema.v0_9.ReferenceSpaceType posReference,
            Quaternion rotation, Schema.v0_9.ReferenceSpaceType rotReference,
            Vector3 scale)
        {
            return XRCInterface.SetEntityQTransform(stagedEntityId,
                position.x, position.y, position.z, UnitType.meter,
                (posReference == Schema.v0_9.ReferenceSpaceType.Global) ?
                    XR.XRC.ReferenceSpaceType.global :
                    XR.XRC.ReferenceSpaceType.relative,
                scale.x, scale.y, scale.z, UnitType.unitless,
                    XR.XRC.ReferenceSpaceType.relative,
                rotation.x, rotation.y, rotation.z, rotation.w, UnitType.unitless,
                (rotReference == Schema.v0_9.ReferenceSpaceType.Global) ?
                    XR.XRC.ReferenceSpaceType.global :
                    XR.XRC.ReferenceSpaceType.relative);
        }

        /// <summary>
        /// Internal function to set an entity position value in the XRCInterface.
        /// </summary>
        /// <param name="stagedEntityId">The staged entity ID associated with the rotation</param>
        /// <param name="position">The world space <code>Vector3</code> position value to stage</param>
        /// <param name="referenceSpace">The <code>ReferenceSpaceType</code> specifying the reference
        ///     space for the position</param>
        /// <returns>A boolean value indicating success</returns>
        public static bool SetEntityPosition(string stagedEntityId, Vector3 position,
            Schema.v0_9.ReferenceSpaceType referenceSpace)
        {
            return XRCInterface.SetEntityPosition(stagedEntityId,
                position.x, position.y, position.z,
                UnitType.meter,
                (referenceSpace == Schema.v0_9.ReferenceSpaceType.Global) ?
                    XR.XRC.ReferenceSpaceType.global :
                    XR.XRC.ReferenceSpaceType.relative);
        }

        /// <summary>
        /// Internal function to set an entity rotation value in the XRCInterface.
        /// </summary>
        /// <param name="stagedEntityId">The staged entity ID associated with the rotation</param>
        /// <param name="rotation">The world space <code>Quaterion</code> rotation value to stage</param>
        /// <param name="referenceSpace">The <code>ReferenceSpaceType</code> specifying the reference
        ///     space for the rotation</param>
        /// <returns>A boolean value indicating success</returns>
        public static bool SetEntityQRotation(string stagedEntityId, Quaternion rotation,
            Schema.v0_9.ReferenceSpaceType referenceSpace)
        {
            return XRCInterface.SetEntityQRotation(stagedEntityId,
                rotation.x, rotation.y, rotation.z, rotation.w,
                UnitType.unitless,
                (referenceSpace == Schema.v0_9.ReferenceSpaceType.Global) ?
                    XR.XRC.ReferenceSpaceType.global :
                    XR.XRC.ReferenceSpaceType.relative);
        }

        /// <summary>
        /// Internal function to set an entity scale value in the XRCInterface.
        /// </summary>
        /// <param name="stagedEntityId">The staged entity ID associated with the rotation</param>
        /// <param name="scale">The <code>Vector3</code> scale value to stage</param>
        /// <returns>A boolean value indicating success</returns>
        public static bool SetEntityScale(string stagedEntityId, Vector3 scale)
        {
            return XRCInterface.SetEntityScale(stagedEntityId,
                scale.x, scale.y, scale.z,
                UnitType.unitless, XR.XRC.ReferenceSpaceType.relative);
        }

        #region XRCToSerialized
        #region Types
        private static bool XRCToSerialized(TransformType serialized, string stagedEntityId)
        {
            // Transform (XRC has special provisions for transforms)
            if (GetEntityQTransform(
                out double xPos, out double yPos, out double zPos, out UnitType posUnits, out XR.XRC.ReferenceSpaceType posRef,
                out double xScl, out double yScl, out double zScl, out UnitType sclUnits, out XR.XRC.ReferenceSpaceType sclRef,
                out double xRot, out double yRot, out double zRot, out double wRot, out UnitType rotUnits, out XR.XRC.ReferenceSpaceType rotRef,
                stagedEntityId))
            {
                LengthUnitType units;

                // Position
                units = XRCManager.XRCUnitsToLengthUnits(posUnits);
                serialized.Position = new TransformPositionType
                {
                    X = SchemaUtil.LengthToUnityUnits((float)xPos, units),
                    Y = SchemaUtil.LengthToUnityUnits((float)yPos, units),
                    Z = SchemaUtil.LengthToUnityUnits((float)zPos, units),
                    referenceSpace = XRCManager.XRCReferenceSpaceToReferenceSpace(posRef)
                };

                // Scale
                units = XRCManager.XRCUnitsToLengthUnits(sclUnits);
                serialized.Item1 = new TransformScaleType
                {
                    X = SchemaUtil.LengthToUnityUnits((float)xScl, units),
                    Y = SchemaUtil.LengthToUnityUnits((float)yScl, units),
                    Z = SchemaUtil.LengthToUnityUnits((float)zScl, units),
                    referenceSpace = XRCManager.XRCReferenceSpaceToReferenceSpace(sclRef)
                };

                // Rotation (Ignore units for quaternions)
                serialized.Item = new TransformQRotationType
                {
                    X = (float)xRot,
                    Y = (float)yRot,
                    Z = (float)zRot,
                    W = (float)wRot,
                    referenceSpace = XRCManager.XRCReferenceSpaceToReferenceSpace(rotRef)
                };
            }

            return true;
        }

        private static bool XRCToSerialized(MaterialShaderType serialized, string attributePrefix, string stagedEntityId)
        {
            // Shader Type (Optional but has value)
            if (!GetEntityStringAttribute(attributePrefix + MATERIAL_SHADER_TYPE, out string shader, stagedEntityId))
            {
                Debug.LogWarning(nameof(MaterialShaderType) + " create event: Required field is null: " + MATERIAL_SHADER_TYPE);
                return false;
            }
            if (!Enum.TryParse(shader, true, out MaterialShaderPredefinedType shaderType))
            {
                Debug.LogWarning(nameof(MaterialShaderType) + " create event: Unexpected material file format value: " + shader);
                return false;
            }
            serialized.shader = shaderType;

            // Value (required)
            if (!GetEntityStringAttribute(attributePrefix + MATERIAL_SHADER_VALUE, out string value, stagedEntityId))
            {
                Debug.LogError(nameof(MaterialShaderType) + " create event: Required field is null: " + MATERIAL_SHADER_VALUE);
                return false;
            }
            serialized.Value = value;

            return true;
        }

        private static bool XRCToSerialized(MaterialFileType serialized, string attributePrefix, string stagedEntityId)
        {
            // Format (Optional but has value)
            if (!GetEntityStringAttribute(attributePrefix + MATERIAL_FILE_FORMAT, out string format, stagedEntityId))
            {
                Debug.LogWarning(nameof(MaterialFileType) + " create event: Required field is null: " + MATERIAL_FILE_FORMAT);
                return false;
            }
            if (!Enum.TryParse(format, true, out MaterialFormatType fileFormat))
            {
                Debug.LogWarning(nameof(MaterialFileType) + " create event: Unexpected material file format value: " + format);
                return false;
            }
            serialized.format = fileFormat;

            // Value (required)
            if (!GetEntityStringAttribute(attributePrefix + MATERIAL_FILE_VALUE, out string value, stagedEntityId))
            {
                Debug.LogError(nameof(MaterialFileType) + " create event: Required field is null: " + MATERIAL_FILE_VALUE);
                return false;
            }
            serialized.Value = value;

            return true;
        }

        private static bool XRCToSerialized(ColorComponentsType serialized, string attributePrefix, string stagedEntityId)
        {
            // R (optional)
            GetEntityIntegerAttribute(attributePrefix + COLOR_COMPONENT_R, out int r, stagedEntityId);
            serialized.R = (byte)MathUtil.Normalize(r, 0, 255, 0, 255);

            // G (optional)
            GetEntityIntegerAttribute(attributePrefix + COLOR_COMPONENT_G, out int g, stagedEntityId);
            serialized.G = (byte)MathUtil.Normalize(g, 0, 255, 0, 255);

            // B (optional)
            GetEntityIntegerAttribute(attributePrefix + COLOR_COMPONENT_B, out int b, stagedEntityId);
            serialized.B = (byte)MathUtil.Normalize(b, 0, 255, 0, 255);

            // A (optional)
            GetEntityIntegerAttribute(attributePrefix + COLOR_COMPONENT_A, out int a, stagedEntityId);
            serialized.A = (byte)MathUtil.Normalize(a, 0, 255, 0, 255);

            return true;
        }

        private static bool XRCToSerialized(ColorGradientType serialized, string attributePrefix, string stagedEntityId)
        {
            // Gradient segments
            List<float> times = new List<float>();
            List<object> colors = new List<object>();
            int index = 0;
            while (GetEntityBooleanAttribute(attributePrefix + "." + index, out bool segment, stagedEntityId))
            {
                string segmentPrefix = attributePrefix + "." + index;
                float time = float.NaN;
                object color = null;

                // Time
                string segmentAttribute = segmentPrefix + COLOR_GRADIENT_TIME;
                GetEntityFloatAttribute(segmentAttribute, out time, stagedEntityId);

                // Color Components (optional)
                segmentAttribute = segmentPrefix + COLOR_GRADIENT_RGBA;
                if (GetEntityBooleanAttribute(segmentAttribute, out bool rgba, stagedEntityId))
                {
                    ColorComponentsType item = new ColorComponentsType();
                    if (!XRCToSerialized(item, segmentAttribute, stagedEntityId))
                    {
                        Debug.LogWarning(nameof(ColorGradientType) + " create event: A problem ocurred reading the gradient RGBA");
                        return false;
                    }
                    color = item;
                }

                // Predefined Color (Optional but has value)
                segmentAttribute = segmentPrefix + COLOR_GRADIENT_COLOR;
                if (!rgba && GetEntityStringAttribute(segmentAttribute, out string predefinedColor, stagedEntityId))
                {
                    if (!Enum.TryParse(predefinedColor, true, out ColorPredefinedType item))
                    {
                        Debug.LogWarning(nameof(ColorGradientType) + " create event: Unexpected gradient color value: " + predefinedColor);
                        return false;
                    }
                    color = item;
                }

                // Make sure we got valid segment values
                if (float.IsNaN(time) || (color == null))
                {
                    Debug.LogWarning(nameof(ColorGradientType) + " create event: Undefined gradient segment encountered");
                    return false;
                }

                // Record the segment and move to the next
                times.Add(time);
                colors.Add(color);
                index++;
            }

            // Make sure we got valid segment values
            if (times.Count == 0)
            {
                Debug.LogWarning(nameof(ColorGradientType) + " create event: Gradient undefined");
                return false;
            }

            // Save the result
            serialized.Time = times.ToArray();
            serialized.Items = colors.ToArray();

            return true;
        }

        private static bool XRCToSerialized(AssetType serialized, string attributePrefix, string stagedEntityId)
        {
            // Asset Name (required)
            string attribute = attributePrefix + ASSET_NAME;
            if (!GetEntityStringAttribute(attribute, out string assetName, stagedEntityId))
            {
                Debug.LogError(nameof(AssetType) + " create event: Required field is null: " + attribute);
                return false;
            }
            serialized.AssetName = assetName;

            // Asset Bundle (required)
            attribute = attributePrefix + ASSET_BUNDLE;
            if (!GetEntityStringAttribute(attribute, out string assetBundle, stagedEntityId))
            {
                Debug.LogError(nameof(AssetType) + " create event: Required field is null: " + attribute);
                return false;
            }
            serialized.AssetBundle = assetBundle;

            return true;
        }

        private static bool XRCToSerialized(MaterialType serialized, string attributePrefix, string stagedEntityId)
        {
            // Create our item list we will convert to an array later
            List<object> itemList = new List<object>();

            // Asset (optional)
            string attribute = attributePrefix + MATERIAL_ASSET;
            if (GetEntityBooleanAttribute(attribute, out bool asset, stagedEntityId))
            {
                AssetType item = new AssetType();
                if (!XRCToSerialized(item, attribute, stagedEntityId))
                {
                    Debug.LogWarning(nameof(MaterialType) + " create event: A problem ocurred reading the material asset");
                    return false;
                }
                itemList.Add(item);
            }

            // Asset (optional)
            attribute = attributePrefix + MATERIAL_FILE;
            if (GetEntityBooleanAttribute(attribute, out bool file, stagedEntityId))
            {
                MaterialFileType item = new MaterialFileType();
                if (!XRCToSerialized(item, attribute, stagedEntityId))
                {
                    Debug.LogWarning(nameof(MaterialType) + " create event: A problem ocurred reading the material file");
                    return false;
                }
                itemList.Add(item);
            }

            // Shader File (optional)
            attribute = attributePrefix + MATERIAL_SHADER;
            if (GetEntityBooleanAttribute(attribute, out bool shader, stagedEntityId))
            {
                MaterialShaderType item = new MaterialShaderType();
                if (!XRCToSerialized(item, attribute, stagedEntityId))
                {
                    Debug.LogWarning(nameof(MaterialType) + " create event: A problem ocurred reading the material shader");
                    return false;
                }
                itemList.Add(item);
            }

            // Color Components (optional)
            attribute = attributePrefix + MATERIAL_RGBA;
            if (GetEntityBooleanAttribute(attribute, out bool rgba, stagedEntityId))
            {
                ColorComponentsType item = new ColorComponentsType();
                if (!XRCToSerialized(item, attribute, stagedEntityId))
                {
                    Debug.LogWarning(nameof(MaterialType) + " create event: A problem ocurred reading the material RGBA");
                    return false;
                }
                itemList.Add(item);
            }

            // Predefined Color (Optional but has value)
            attribute = attributePrefix + MATERIAL_COLOR;
            if (!rgba && GetEntityStringAttribute(attribute, out string color, stagedEntityId))
            {
                if (!Enum.TryParse(color, true, out ColorPredefinedType item))
                {
                    Debug.LogWarning(nameof(MaterialType) + " create event: Unexpected material color value: " + color);
                    return false;
                }
                itemList.Add(item);
            }

            // Make sure we got at least one item
            if (itemList.Count == 0)
            {
                Debug.LogWarning(nameof(MaterialType) + " create event: No material definition defined");
                return false;
            }
            serialized.Items = itemList.ToArray();

            return true;
        }

        private static bool XRCToSerialized(ModelFileType serialized, string attributePrefix, string stagedEntityId)
        {
            // Format (Optional but has value)
            string attribute = attributePrefix + MODEL_FILE_FORMAT;
            if (!GetEntityStringAttribute(attribute, out string format, stagedEntityId))
            {
                Debug.LogWarning(nameof(ModelFileType) + " create event: Required field is null: " + attribute);
                return false;
            }
            if (!Enum.TryParse(format, true, out ModelFormatType fileFormat))
            {
                Debug.LogWarning(nameof(ModelFileType) + " create event: Unexpected model file format value: " + format);
                return false;
            }
            serialized.format = fileFormat;

            // Value (required)
            attribute = attributePrefix + MODEL_FILE_VALUE;
            if (!GetEntityStringAttribute(attribute, out string value, stagedEntityId))
            {
                Debug.LogError(nameof(ModelFileType) + " create event: Required field is null: " + attribute);
                return false;
            }
            serialized.Value = value;

            return true;
        }

        private static bool XRCToSerialized(ref PrimitiveShapeType serialized, string attributePrefix, string stagedEntityId)
        {
            // Shape (Optional but has value)
            string attribute = attributePrefix + MODEL_SHAPE_TYPE;
            if (!GetEntityStringAttribute(attribute, out string shapeType, stagedEntityId))
            {
                Debug.LogWarning(nameof(PrimitiveShapeType) + " create event: Required field is null: " + attribute);
                return false;
            }
            if (!Enum.TryParse(shapeType, true, out PrimitiveShapeType modelShape))
            {
                Debug.LogWarning(nameof(PrimitiveShapeType) + " create event: Unexpected model shape value: " + shapeType);
                return false;
            }
            serialized = modelShape;

            return true;
        }

        private static bool XRCToSerialized(ModelType serialized, string attributePrefix, string stagedEntityId)
        {
            // Material (optional)
            string attribute = attributePrefix + MODEL_MATERIAL;
            if (GetEntityBooleanAttribute(attribute, out bool material, stagedEntityId))
            {
                serialized.Material ??= new MaterialType();
                if (!XRCToSerialized(serialized.Material, attribute, stagedEntityId))
                {
                    Debug.LogWarning(nameof(ModelType) + " create event: A problem ocurred reading the model material");
                    return false;
                }
            }

            // Asset (optional)
            attribute = attributePrefix + MODEL_ASSET;
            if (GetEntityBooleanAttribute(attribute, out bool asset, stagedEntityId))
            {
                serialized.Item ??= new AssetType();
                if (!XRCToSerialized(serialized.Item as AssetType, attribute, stagedEntityId))
                {
                    Debug.LogWarning(nameof(ModelType) + " create event: A problem ocurred reading the model asset");
                    return false;
                }
            }

            // File (optional)
            attribute = attributePrefix + MODEL_FILE;
            if (GetEntityBooleanAttribute(attribute, out bool modelFile, stagedEntityId))
            {
                serialized.Item ??= new ModelFileType();
                if (!XRCToSerialized(serialized.Item as ModelFileType, attribute, stagedEntityId))
                {
                    Debug.LogWarning(nameof(ModelType) + " create event: A problem ocurred reading the model file");
                    return false;
                }
            }

            // Shape (optional)
            attribute = attributePrefix + MODEL_SHAPE;
            if (GetEntityBooleanAttribute(attribute, out bool modelShape, stagedEntityId))
            {
                serialized.Item = null;
                PrimitiveShapeType shape = PrimitiveShapeType.Cube;
                if (!XRCToSerialized(ref shape, attribute, stagedEntityId))
                {
                    Debug.LogWarning(nameof(ModelType) + " create event: A problem ocurred reading the model shape");
                    return false;
                }
                serialized.Item = shape;
            }

            return true;
        }

        private static bool XRCToSerialized(InteractionSettingsType serialized, string attributePrefix, string stagedEntityId)
        {
            // Enable Interaction (optional)
            bool enableInteraction = serialized.EnableInteraction;
            GetEntityBooleanAttribute(attributePrefix + INTERACTABLE_ENABLE_INTERACTION, out enableInteraction, stagedEntityId);
            serialized.EnableInteraction = enableInteraction;

            // Enable Usability (optional)
            bool enableUsability = serialized.EnableUsability;
            GetEntityBooleanAttribute(attributePrefix + INTERACTABLE_ENABLE_USABILITY, out enableUsability, stagedEntityId);
            serialized.EnableUsability = enableUsability;

            return true;
        }

        private static bool XRCToSerialized(TelemetrySettingsType serialized, string attributePrefix, string stagedEntityId)
        {
            // Shade limit violation (optional)
            bool shadeLimits = serialized.ShadeForLimitViolations;
            GetEntityBooleanAttribute(attributePrefix + INTERACTABLE_TELEMETRY_SHADE_FOR_LIMITS, out shadeLimits, stagedEntityId);
            serialized.ShadeForLimitViolations = shadeLimits;

            // Telemetry keys (optional)
            List<string> telemetryKeys = new List<string>();
            int index = 0;
            string attribute = attributePrefix + INTERACTABLE_TELEMETRY_KEYS;
            while (GetEntityBooleanAttribute(attribute + "." + index, out bool hasTelemetry, stagedEntityId))
            {
                if (!GetEntityStringAttribute(attribute + "." + index, out string telemetryKey, stagedEntityId))
                {
                    Debug.LogWarning(nameof(TelemetrySettingsType) + " create event: A problem ocurred reading the telemetry keys: " + attribute + "." + index);
                    return false;
                }
                telemetryKeys.Add(telemetryKey);
                index++;
            }
            serialized.TelemetryKey = telemetryKeys.ToArray();

            return true;
        }

        private static bool XRCToSerialized(PhysicsSettingsType serialized, string attributePrefix, string stagedEntityId)
        {
            // Enable Collisions (optional)
            bool enableCollisions = serialized.EnableCollisions;
            GetEntityBooleanAttribute(attributePrefix + PHYSICS_ENABLE_COLLISIONS, out enableCollisions, stagedEntityId);
            serialized.EnableCollisions = enableCollisions;

            // Enable Gravity (optional)
            bool enableGravity = serialized.EnableGravity;
            GetEntityBooleanAttribute(attributePrefix + PHYSICS_ENABLE_GRAVITY, out enableGravity, stagedEntityId);
            serialized.EnableGravity = enableGravity;

            return true;
        }
        #endregion Types

        #region Core
        private static bool XRCToSerialized(VersionedType serialized, string attributePrefix, string stagedEntityId)
        {
            // Version
            if (!GetEntityStringAttribute(attributePrefix + VERSION, out string version, stagedEntityId))
            {
                Debug.LogError(nameof(VersionedType) + " create event: Required field is null: " + VERSION);
                return false;
            }
            serialized.version = version;

            return true;
        }

        private static bool XRCToSerialized(XRType serialized, string attributePrefix, string stagedEntityId)
        {
            // AR Enabled (optional)
            bool arEnabled = serialized.AREnabled;
            GetEntityBooleanAttribute(attributePrefix + XR_AR_ENABLED, out arEnabled, stagedEntityId);
            serialized.AREnabled = arEnabled;

            // VR Enabled (optional)
            bool vrEnabled = serialized.VREnabled;
            GetEntityBooleanAttribute(attributePrefix + XR_VR_ENABLED, out vrEnabled, stagedEntityId);
            serialized.VREnabled = vrEnabled;

            // Move up the hierarchy
            return XRCToSerialized(serialized as VersionedType, attributePrefix, stagedEntityId);
        }

        private static bool XRCToSerialized(IdentifiableType serialized, string attributePrefix, string stagedEntityId)
        {
            // UUID
            string attribute = attributePrefix + UUID;
            if (!GetEntityStringAttribute(attribute, out string uuid, stagedEntityId))
            {
                Debug.LogWarning(nameof(IdentifiableType) + " create event: Required field is null: " + attribute);
                return false;
            }
            serialized.UUID = uuid;

            // ID
            attribute = attributePrefix + ID;
            if (!GetEntityStringAttribute(attribute, out string id, stagedEntityId))
            {
                Debug.LogWarning(nameof(IdentifiableType) + " create event: Required field is null: " + attribute);
                return false;
            }
            serialized.ID = id;

            // Name
            attribute = attributePrefix + NAME;
            if (!GetEntityStringAttribute(attribute, out string name, stagedEntityId))
            {
                Debug.LogWarning(nameof(IdentifiableType) + " create event: Required field is null: " + attribute);
                return false;
            }
            serialized.Name = name;

            // Description
            attribute = attributePrefix + DESCRIPTION;
            if (!GetEntityStringAttribute(attribute, out string description, stagedEntityId))
            {
                Debug.LogWarning(nameof(IdentifiableType) + " create event: Required field is null: " + attribute);
                return false;
            }
            serialized.Description = description;

            return XRCToSerialized(serialized as XRType, attributePrefix, stagedEntityId);
        }

        private static bool XRCToSerialized(SceneObjectType serialized, string attributePrefix, string stagedEntityId)
        {
            // Transform
            serialized.Transform ??= new TransformType();
            if (!XRCToSerialized(serialized.Transform, stagedEntityId))
            {
                Debug.LogWarning(nameof(SceneObjectType) + " create event: A problem ocurred reading the transform");
                return false;
            }

            // TODO: Scripts

            // Child Scene Objects
            string attribute = attributePrefix + SCENE_OBJECT_CHILDREN;
            if (GetEntityBooleanAttribute(attribute, out bool children, stagedEntityId))
            {
                serialized.ChildSceneObjects ??= new SceneObjectsType();
                if (!XRCToSerialized(serialized.ChildSceneObjects, attribute, stagedEntityId))
                {
                    Debug.LogWarning(nameof(SceneObjectType) + " create event: A problem ocurred reading the child scene objects");
                    return false;
                }
            }

            // Move up the hierarchy
            return XRCToSerialized(serialized as IdentifiableType, attributePrefix, stagedEntityId);
        }

        private static bool XRCToSerialized(InteractableSceneObjectType serialized, string attributePrefix, string stagedEntityId)
        {
            // Interaction Settings
            string attribute = attributePrefix + INTERACTABLE_INTERACTIONS;
            if (GetEntityBooleanAttribute(attribute, out bool interactions, stagedEntityId))
            {
                serialized.Interactions ??= new InteractionSettingsType();
                if (!XRCToSerialized(serialized.Interactions, attribute, stagedEntityId))
                {
                    Debug.LogWarning(nameof(InteractableSceneObjectType) + " create event: A problem ocurred reading the object interactions");
                    return false;
                }
            }

            // Telemetry Settings
            attribute = attributePrefix + INTERACTABLE_TELEMETRY_SETTINGS;
            if (GetEntityBooleanAttribute(attribute, out bool telemetrySettings, stagedEntityId))
            {
                serialized.Telemetry ??= new TelemetrySettingsType();
                if (!XRCToSerialized(serialized.Telemetry, attribute, stagedEntityId))
                {
                    Debug.LogWarning(nameof(InteractableSceneObjectType) + " create event: A problem ocurred reading the object telemetry settings");
                    return false;
                }
            }

            // Visble (optional but has value)
            bool visible = serialized.Visible;
            GetEntityBooleanAttribute(attributePrefix + VISIBLE, out visible, stagedEntityId);
            serialized.Visible = visible;

            // Opacity (optional but has value)
            GetEntityIntegerAttribute(attributePrefix + OPACITY, out int opacity, stagedEntityId);
            serialized.Opacity = (byte)MathUtil.Normalize(opacity, 0, 255, 0, 255);

            // Highlight Material (optional)
            attribute = attributePrefix + INTERACTABLE_HIGHLIGHT_MATERIAL;
            if (GetEntityBooleanAttribute(attribute, out bool highlight, stagedEntityId))
            {
                serialized.HighlightMaterial ??= new MaterialType();
                if (!XRCToSerialized(serialized.HighlightMaterial, attribute, stagedEntityId))
                {
                    Debug.LogWarning(nameof(InteractableSceneObjectType) + " create event: A problem ocurred reading the object highlight material");
                    return false;
                }
            }

            // Selection Material (optional)
            attribute = attributePrefix + INTERACTABLE_SELECTION_MATERIAL;
            if (GetEntityBooleanAttribute(attribute, out bool selection, stagedEntityId))
            {
                serialized.SelectionMaterial ??= new MaterialType();
                if (!XRCToSerialized(serialized.SelectionMaterial, attribute, stagedEntityId))
                {
                    Debug.LogWarning(nameof(InteractableSceneObjectType) + " create event: A problem ocurred reading the object selection material");
                    return false;
                }
            }

            // Move up the hierarchy
            return XRCToSerialized(serialized as SceneObjectType, attributePrefix, stagedEntityId);
        }

        private static bool XRCToSerialized(MassSpecificationsType serialized, string attributePrefix, string stagedEntityId)
        {
            // Min Mass (optional)
            if (GetEntityFloatAttribute(attributePrefix + SPECS_MASS_MIN, out float minMass, stagedEntityId))
            {
                serialized.Min ??= new MassType();
                SchemaUtil.SerializeMass(minMass, serialized.Min);
            }

            // Max Mass (optional)
            if (GetEntityFloatAttribute(attributePrefix + SPECS_MASS_MAX, out float maxMass, stagedEntityId))
            {
                serialized.Max ??= new MassType();
                SchemaUtil.SerializeMass(maxMass, serialized.Max);
            }

            // Contingency Mass (optional)
            if (GetEntityFloatAttribute(attributePrefix + SPECS_MASS_CONTINGENCY, out float contingencyMass, stagedEntityId))
            {
                serialized.Contingency ??= new MassType();
                SchemaUtil.SerializeMass(contingencyMass, serialized.Contingency);
            }

            return true;
        }

        private static bool XRCToSerialized(PhysicalSpecificationsType serialized, string attributePrefix, string stagedEntityId)
        {
            // Mass (optional)
            string attribute = attributePrefix + SPECS_MASS;
            if (GetEntityBooleanAttribute(attribute, out bool specMass, stagedEntityId))
            {
                serialized.Mass ??= new MassSpecificationsType();
                if (!XRCToSerialized(serialized.Mass, attribute, stagedEntityId))
                {
                    Debug.LogWarning(nameof(PhysicalSpecificationsType) + " create event: A problem ocurred reading the mass specifications");
                    return false;
                }
            }

            // Notes (optional)
            if (GetEntityStringAttribute(attributePrefix + SPECS_NOTES, out string notes, stagedEntityId))
            {
                serialized.Notes = notes;
            }

            // Reference (optional)
            if (GetEntityStringAttribute(attributePrefix + SPECS_REFERENCE, out string reference, stagedEntityId))
            {
                serialized.Reference = reference;
            }

            return true;
        }

        private static bool XRCToSerialized(PhysicalSceneObjectType serialized, string attributePrefix, string stagedEntityId)
        {
            // Model (optional)
            string attribute = attributePrefix + MODEL;
            if (GetEntityBooleanAttribute(attribute, out bool model, stagedEntityId))
            {
                serialized.Model ??= new ModelType();
                if (!XRCToSerialized(serialized.Model, attribute, stagedEntityId))
                {
                    Debug.LogWarning(nameof(PhysicalSceneObjectType) + " create event: A problem ocurred reading the model");
                    return false;
                }
            }

            // Physics Settings (optional)
            attribute = attributePrefix + PHYSICS_SETTINGS;
            if (GetEntityBooleanAttribute(attribute, out bool physicsSettings, stagedEntityId))
            {
                serialized.Physics ??= new PhysicsSettingsType();
                if (!XRCToSerialized(serialized.Physics, attribute, stagedEntityId))
                {
                    Debug.LogWarning(nameof(PhysicalSceneObjectType) + " create event: A problem ocurred reading the physics settings");
                    return false;
                }
            }

            // Physical Specifications (optional)
            attribute = attributePrefix + SPECS;
            if (GetEntityBooleanAttribute(attribute, out bool physicalSpecs, stagedEntityId))
            {
                serialized.Specifications ??= new PhysicalSpecificationsType();
                if (!XRCToSerialized(serialized.Specifications, attribute, stagedEntityId))
                {
                    Debug.LogWarning(nameof(PartType) + " create event: A problem ocurred reading the physical specifications");
                    return false;
                }
            }

            // Randomize Textures (optional)
            bool randomizeTextures = serialized.RandomizeTextures;
            GetEntityBooleanAttribute(attributePrefix + RANDOMIZE_TEXTURES, out randomizeTextures, stagedEntityId);
            serialized.RandomizeTextures = randomizeTextures;

            // Move up the hierarchy
            return XRCToSerialized(serialized as InteractableSceneObjectType, attributePrefix, stagedEntityId);
        }
        #endregion Core

        #region User
        private static bool XRCToSerialized(UserComponentType serialized, string attributePrefix, string stagedEntityId)
        {
            // User ID
            string attribute = attributePrefix + USER_ID;
            if (!GetEntityStringAttribute(attribute, out string id, stagedEntityId))
            {
                Debug.LogWarning(nameof(UserComponentType) + " create event: Required field is null: " + attribute);
                return false;
            }
            serialized.UserId = id;

            // Move up the hierarchy
            return XRCToSerialized(serialized as SceneObjectType, attributePrefix, stagedEntityId);
        }

        private static bool XRCToSerialized(UserHandComponentType serialized, string attributePrefix, string stagedEntityId)
        {
            // Hand ID
            string attribute = attributePrefix + USER_HAND_ID;
            if (!GetEntityStringAttribute(attribute, out string id, stagedEntityId))
            {
                Debug.LogWarning(nameof(UserHandComponentType) + " create event: Required field is null: " + attribute);
                return false;
            }
            serialized.HandId = id;

            // Handedness (Optional but has value)
            attribute = attributePrefix + USER_HAND_HANDEDNESS;
            if (!GetEntityStringAttribute(attribute, out string handedness, stagedEntityId))
            {
                Debug.LogWarning(nameof(UserHandComponentType) + " create event: Required field is null: " + attribute);
                return false;
            }
            if (!Enum.TryParse(handedness, true, out InputHand.Handedness controllerHandedness))
            {
                Debug.LogWarning(nameof(UserHandComponentType) + " create event: Unexpected handedness value: " + controllerHandedness);
                return false;
            }
            serialized.Handedness = controllerHandedness;

            // Move up the hierarchy
            return XRCToSerialized(serialized as UserComponentType, attributePrefix, stagedEntityId);
        }

        private static bool XRCToSerialized(UserHeadType serialized, string attributePrefix, string stagedEntityId)
        {
            // Move up the hierarchy
            return XRCToSerialized(serialized as UserComponentType, attributePrefix, stagedEntityId);
        }

        private static bool XRCToSerialized(UserTorsoType serialized, string attributePrefix, string stagedEntityId)
        {
            // Move up the hierarchy
            return XRCToSerialized(serialized as UserComponentType, attributePrefix, stagedEntityId);
        }

        private static bool XRCToSerialized(UserHandType serialized, string attributePrefix, string stagedEntityId)
        {
            // Handedness (Optional but has value)
            string attribute = attributePrefix + USER_HAND_HANDEDNESS;
            if (!GetEntityStringAttribute(attribute, out string handedness, stagedEntityId))
            {
                Debug.LogWarning(nameof(UserHandType) + " create event: Required field is null: " + attribute);
                return false;
            }
            if (!Enum.TryParse(handedness, true, out InputHand.Handedness handHandedness))
            {
                Debug.LogWarning(nameof(UserHandType) + " create event: Unexpected handedness value: " + handHandedness);
                return false;
            }
            serialized.Handedness = handHandedness;

            // Move up the hierarchy
            return XRCToSerialized(serialized as UserComponentType, attributePrefix, stagedEntityId);
        }

        private static bool XRCToSerialized(UserControllerType serialized, string attributePrefix, string stagedEntityId)
        {
            // Move up the hierarchy
            return XRCToSerialized(serialized as UserHandComponentType, attributePrefix, stagedEntityId);
        }

        private static bool XRCToSerialized(UserPointerType serialized, string attributePrefix, string stagedEntityId)
        {
            // Move up the hierarchy
            return XRCToSerialized(serialized as UserHandComponentType, attributePrefix, stagedEntityId);
        }

        private static bool XRCToSerialized(UserType serialized, string attributePrefix, string stagedEntityId)
        {
            // Alias (Optional but has value)
            string attribute = attributePrefix + USER_ALIAS;
            if (GetEntityStringAttribute(stagedEntityId, out string userAlias, attribute))
            {
                Debug.LogWarning(nameof(UserType) + " A problem ocurred reading the user alias");
                return false;
            }
            serialized.Alias = userAlias;

            // Color Components (optional)
            object userColor = null;
            attribute = attributePrefix + USER_RGBA;
            if (GetEntityBooleanAttribute(attribute, out bool rgba, stagedEntityId))
            {
                ColorComponentsType item = new ColorComponentsType();
                if (!XRCToSerialized(item, attribute, stagedEntityId))
                {
                    Debug.LogWarning(nameof(UserType) + " create event: A problem ocurred reading the user RGBA");
                    return false;
                }
                userColor = item;
            }

            // Predefined Color (Optional but has value)
            attribute = attributePrefix + USER_COLOR;
            if (!rgba && GetEntityStringAttribute(attribute, out string color, stagedEntityId))
            {
                if (!Enum.TryParse(color, true, out ColorPredefinedType item))
                {
                    Debug.LogWarning(nameof(UserType) + " create event: Unexpected user color value: " + color);
                    return false;
                }
                userColor = item;
            }

            // Assign the user color
            serialized.Item = userColor;

            // Move up the hierarchy
            return XRCToSerialized(serialized as SceneObjectType, attributePrefix, stagedEntityId);
        }
        #endregion User

        #region Interface
        private static bool XRCToSerialized(InterfaceType serialized, string attributePrefix, string stagedEntityId)
        {
            // Move up the hierarchy
            return XRCToSerialized(serialized as IdentifiableType, attributePrefix, stagedEntityId);
        }

        private static bool XRCToSerialized(ClientInterfaceType serialized, string attributePrefix, string stagedEntityId)
        {
            // Server (Required)
            string attribute = attributePrefix + CONNECTION_SERVER;
            if (!GetEntityStringAttribute(attribute, out string server, stagedEntityId))
            {
                Debug.LogWarning(nameof(ClientInterfaceType) + " create event: Required field is null: " + attribute);
                return false;
            }
            serialized.Server = server;

            // Port (Required)
            attribute = attributePrefix + CONNECTION_PORT;
            if (!GetEntityIntegerAttribute(attribute, out int port, stagedEntityId))
            {
                Debug.LogWarning(nameof(ClientInterfaceType) + " create event: Required field is null: " + attribute);
                return false;
            }
            serialized.Port = port;

            // Connection Timeout Frequency (optional)
            if (GetEntityFloatAttribute(attributePrefix + CONNECTION_TIMEOUT, out float timeoutFrequency, stagedEntityId))
            {
                serialized.ConnectionTimeoutFrequency ??= new FrequencyType();
                SchemaUtil.SerializeFrequency(timeoutFrequency, serialized.ConnectionTimeoutFrequency);
            }

            // Connection Update Frequency (optional)
            if (GetEntityFloatAttribute(attributePrefix + CONNECTION_UPDATE, out float updateFrequency, stagedEntityId))
            {
                serialized.ConnectionUpdateFrequency ??= new FrequencyType();
                SchemaUtil.SerializeFrequency(updateFrequency, serialized.ConnectionUpdateFrequency);
            }

            // Move up the hierarchy
            return XRCToSerialized(serialized as InterfaceType, attributePrefix, stagedEntityId);
        }

        private static bool XRCToSerialized(ROSInterfaceType serialized, string attributePrefix, string stagedEntityId)
        {
            // Protocol (Optional but has value)
            string attribute = attributePrefix + ROS_PROTOCOL;
            if (!GetEntityStringAttribute(attribute, out string protocol, stagedEntityId))
            {
                Debug.LogWarning(nameof(ROSInterfaceType) + " create event: Required field is null: " + attribute);
                return false;
            }
            if (!Enum.TryParse(protocol, true, out ROSProtocolType rosProtocol))
            {
                Debug.LogWarning(nameof(ROSInterfaceType) + " create event: Unexpected ROS protocol value: " + protocol);
                return false;
            }
            serialized.Protocol = rosProtocol;

            // Serializer (Optional but has value)
            attribute = attributePrefix + ROS_SERIALIZER;
            if (!GetEntityStringAttribute(attribute, out string serializer, stagedEntityId))
            {
                Debug.LogWarning(nameof(ROSInterfaceType) + " create event: Required field is null: " + attribute);
                return false;
            }
            if (!Enum.TryParse(serializer, true, out ROSSerializerType rosSerializer))
            {
                Debug.LogWarning(nameof(ROSInterfaceType) + " create event: Unexpected ROS serializer value: " + serializer);
                return false;
            }
            serialized.Serializer = rosSerializer;

            // JointStateSubscriberTopic (Optional but has value)
            if (GetEntityStringAttribute(attributePrefix + ROS_SUBSCRIBE_TOPIC, out string subscribeTopic, stagedEntityId))
            {
                serialized.JointStateSubscriberTopic = subscribeTopic;
            }

            // JointStatePublisherTopic (Optional but has value)
            if (GetEntityStringAttribute(attributePrefix + ROS_PUBLISH_TOPIC, out string publishTopic, stagedEntityId))
            {
                serialized.JointStatePublisherTopic = publishTopic;
            }

            // Robot Description (required)
            attribute = attributePrefix + ROS_DESCRIPTION;
            if (!GetEntityStringAttribute(attribute, out string description, stagedEntityId))
            {
                Debug.LogWarning(nameof(ROSInterfaceType) + " create event: Required field is null: " + attribute);
                return false;
            }
            serialized.RobotDescription = description;

            // Move up the hierarchy
            return XRCToSerialized(serialized as ClientInterfaceType, attributePrefix, stagedEntityId);
        }
        #endregion Interface

        #region Part
        private static bool XRCToSerialized(PowerSpecificationsType serialized, string attributePrefix, string stagedEntityId)
        {
            // Min Power (optional)
            if (GetEntityFloatAttribute(attributePrefix + PART_POWER_MIN, out float minPower, stagedEntityId))
            {
                serialized.Min ??= new PowerType();
                SchemaUtil.SerializePower(minPower, serialized.Min);
            }

            // Max Power (optional)
            if (GetEntityFloatAttribute(attributePrefix + PART_POWER_MAX, out float maxPower, stagedEntityId))
            {
                serialized.Max ??= new PowerType();
                SchemaUtil.SerializePower(maxPower, serialized.Max);
            }

            // Contingency Power (optional)
            if (GetEntityFloatAttribute(attributePrefix + PART_POWER_CONTINGENCY, out float contingencyPower, stagedEntityId))
            {
                serialized.Contingency ??= new PowerType();
                SchemaUtil.SerializePower(contingencyPower, serialized.Contingency);
            }

            return true;
        }

        private static bool XRCToSerialized(PartSpecificationsType serialized, string attributePrefix, string stagedEntityId)
        {
            // Vendor (required)
            string attribute = attributePrefix + PART_VENDOR;
            if (!GetEntityStringAttribute(attribute, out string vendor, stagedEntityId))
            {
                Debug.LogWarning(nameof(PartSpecificationsType) + " create event: Required field is null: " + attribute);
                return false;
            }
            serialized.Vendor = vendor;

            // Version (required)
            attribute = attributePrefix + PART_VERSION;
            if (!GetEntityStringAttribute(attribute, out string version, stagedEntityId))
            {
                Debug.LogWarning(nameof(PartSpecificationsType) + " create event: Required field is null: " + attribute);
                return false;
            }
            serialized.Version = version;

            // Part mass (optional)
            attribute = attributePrefix + SPECS_MASS;
            if (GetEntityBooleanAttribute(attribute, out bool partMass, stagedEntityId))
            {
                serialized.Mass1 ??= new MassSpecificationsType();
                if (!XRCToSerialized(serialized.Mass1, attribute, stagedEntityId))
                {
                    Debug.LogWarning(nameof(PartSpecificationsType) + " create event: A problem ocurred reading the part mass");
                    return false;
                }
            }

            // Part power (optional)
            attribute = attributePrefix + PART_POWER;
            if (GetEntityBooleanAttribute(attribute, out bool partPower, stagedEntityId))
            {
                serialized.Power ??= new PowerSpecificationsType();
                if (!XRCToSerialized(serialized.Power, attribute, stagedEntityId))
                {
                    Debug.LogWarning(nameof(PartSpecificationsType) + " create event: A problem ocurred reading the part power");
                    return false;
                }
            }

            // Notes (optional)
            if (GetEntityStringAttribute(attributePrefix + SPECS_NOTES, out string notes, stagedEntityId))
            {
                serialized.Notes1 = notes;
            }

            // Reference (optional)
            if (GetEntityStringAttribute(attributePrefix + SPECS_REFERENCE, out string reference, stagedEntityId))
            {
                serialized.Reference1 = reference;
            }

            return true;
        }

        private static bool XRCToSerialized(PartType serialized, string attributePrefix, string stagedEntityId)
        {
            // Part Specifications
            string attribute = attributePrefix + PART_SPECS;
            if (GetEntityBooleanAttribute(attribute, out bool partSpecs, stagedEntityId))
            {
                serialized.PartSpecifications ??= new PartSpecificationsType();
                if (!XRCToSerialized(serialized.PartSpecifications, attribute, stagedEntityId))
                {
                    Debug.LogWarning(nameof(PartType) + " create event: A problem ocurred reading the part specifications");
                    return false;
                }
            }

            // Category (Optional but has value)
            attribute = attributePrefix + PART_CATEGORY;
            if (!GetEntityStringAttribute(attribute, out string category, stagedEntityId))
            {
                Debug.LogWarning(nameof(PartType) + " create event: Required field is null: " + attribute);
                return false;
            }
            if (!Enum.TryParse(category, true, out PartCategoryType partCategory))
            {
                Debug.LogWarning(nameof(PartType) + " create event: Unexpected part category value: " + category);
                return false;
            }
            serialized.Category = partCategory;

            // Subsystem (Optional but has value)
            attribute = attributePrefix + PART_SUBSYSTEM;
            if (!GetEntityStringAttribute(attribute, out string subsystem, stagedEntityId))
            {
                Debug.LogWarning(nameof(PartType) + " create event: Required field is null: " + attribute);
                return false;
            }
            if (!Enum.TryParse(subsystem, true, out PartSubsystemType partSubsystem))
            {
                Debug.LogWarning(nameof(PartType) + " create event: Unexpected part subsystem value: " + subsystem);
                return false;
            }
            serialized.Subsystem = partSubsystem;

            // ROS
            attribute = attributePrefix + PART_ROS_INTERFACE;
            if (GetEntityBooleanAttribute(attribute, out bool rosInterface, stagedEntityId))
            {
                serialized.ROSInterface ??= new ROSInterfaceType();
                if (!XRCToSerialized(serialized.ROSInterface, attribute, stagedEntityId))
                {
                    Debug.LogWarning(nameof(PartType) + " create event: A problem ocurred reading the ROS interface");
                    return false;
                }
            }

            // Child Parts
            attribute = attributePrefix + PART_CHILDREN;
            if (GetEntityBooleanAttribute(attribute, out bool children, stagedEntityId))
            {
                serialized.ChildParts ??= new PartsType();
                if (!XRCToSerialized(serialized.ChildParts, attribute, stagedEntityId))
                {
                    Debug.LogWarning(nameof(PartType) + " create event: A problem ocurred reading the child parts");
                    return false;
                }
            }

            // Move up the hierarchy
            return XRCToSerialized(serialized as PhysicalSceneObjectType, attributePrefix, stagedEntityId);
        }

        private static bool XRCToSerialized(EnclosureType serialized, string attributePrefix, string stagedEntityId)
        {
            // Move up the hierarchy
            return XRCToSerialized(serialized as PhysicalSceneObjectType, attributePrefix, stagedEntityId);
        }
        #endregion Part

        #region Groups
        private static bool XRCToSerialized(GroupType serialized, string attributePrefix, string stagedEntityId)
        {
            // Group
            string attribute = attributePrefix + GROUP;
            if (!GetEntityIntegerAttribute(attribute, out int group, stagedEntityId))
            {
                Debug.LogWarning(nameof(GroupType) + " create event: Required field is null: " + attribute);
                return false;
            }
            serialized.group = group;

            return XRCToSerialized(serialized as VersionedType, attributePrefix, stagedEntityId);
        }

        private static bool XRCToSerialized(SceneObjectsType serialized, string attributePrefix, string stagedEntityId)
        {
            // FIXME: Need to build list of scene objects

            return XRCToSerialized(serialized as GroupType, attributePrefix, stagedEntityId);
        }

        private static bool XRCToSerialized(PartsType serialized, string attributePrefix, string stagedEntityId)
        {
            // Assembly Interaction Settings
            string attribute = attributePrefix + ASSEMBLY_INTERACTIONS;
            if (GetEntityBooleanAttribute(attribute, out bool interactions, stagedEntityId))
            {
                serialized.AssemblyInteractions ??= new InteractionSettingsType();
                if (!XRCToSerialized(serialized.AssemblyInteractions, attribute, stagedEntityId))
                {
                    Debug.LogWarning(nameof(PartsType) + " create event: A problem ocurred reading the assembly interactions");
                    return false;
                }
            }

            // Enclosure
            attribute = attributePrefix + PART_ENCLOSURE;
            if (GetEntityBooleanAttribute(attribute, out bool enclosure, stagedEntityId))
            {
                serialized.Enclosure ??= new EnclosureType();
                if (!XRCToSerialized(serialized.Enclosure, attribute, stagedEntityId))
                {
                    Debug.LogWarning(nameof(PartsType) + " create event: A problem ocurred reading the enclosure");
                    return false;
                }
            }

            // FIXME: Need to build list of child parts

            return XRCToSerialized(serialized as GroupType, attributePrefix, stagedEntityId);
        }
        #endregion Groups

        #region Displays
        private static bool XRCToSerialized(DisplayType serialized, string attributePrefix, string stagedEntityId)
        {
            // Parent ID
            string parentID = serialized.ParentID;
            GetEntityStringAttribute(attributePrefix + DISPLAY_PARENTID, out parentID, stagedEntityId);
            serialized.ParentID = parentID;

            // Title
            string title = serialized.Title;
            GetEntityStringAttribute(attributePrefix + DISPLAY_TITLE, out title, stagedEntityId);
            serialized.Title = title;

            // Width
            string attribute = attributePrefix + DISPLAY_WIDTH;
            if (!GetEntityFloatAttribute(attribute, out float width, stagedEntityId))
            {
                Debug.LogWarning(nameof(DisplayType) + " create event: Required field is null: " + attribute);
                return false;
            }
            serialized.Width = width;

            // Height
            attribute = attributePrefix + DISPLAY_HEIGHT;
            if (!GetEntityFloatAttribute(attribute, out float height, stagedEntityId))
            {
                Debug.LogWarning(nameof(DisplayType) + " create event: Required field is null: " + attribute);
                return false;
            }
            serialized.Height = height;

            // State (Optional but has value)
            attribute = attributePrefix + DISPLAY_STATE;
            if (!GetEntityStringAttribute(attribute, out string state, stagedEntityId))
            {
                Debug.LogWarning(nameof(DisplayType) + " create event: Required field is null: " + attribute);
                return false;
            }
            if (!Enum.TryParse(state, true, out DisplayStateType displayState))
            {
                Debug.LogWarning(nameof(DisplayType) + " create event: Unexpected display state value: " + state);
                return false;
            }
            serialized.State = displayState;

            // Z-Order
            attribute = attributePrefix + DISPLAY_ZORDER;
            if (!GetEntityIntegerAttribute(attribute, out int zOrder, stagedEntityId))
            {
                Debug.LogWarning(nameof(DisplayType) + " create event: Required field is null: " + attribute);
                return false;
            }
            serialized.Zorder = zOrder;

            // Move up the hierarchy
            return XRCToSerialized(serialized as InteractableSceneObjectType, attributePrefix, stagedEntityId);
        }

        private static bool XRCToSerialized(NoteType serialized, string attributePrefix, string stagedEntityId)
        {
            // Content
            string content = serialized.Content;
            GetEntityStringAttribute(attributePrefix + NOTE_CONTENT, out content, stagedEntityId);
            serialized.Content = content;

            // Drawings
            List<Drawing2dType> noteDrawings = new List<Drawing2dType>();
            int index = 0;
            string attribute = attributePrefix + NOTE_DRAWING;
            while (GetEntityBooleanAttribute(attribute + "." + index, out bool drawing, stagedEntityId))
            {
                Drawing2dType serializedDrawing = new Drawing2dType();
                if (!XRCToSerialized(serializedDrawing, attribute + "." + index, stagedEntityId))
                {
                    Debug.LogWarning(nameof(NoteType) + " create event: A problem ocurred reading the note drawing: " + attribute + "." + index);
                    return false;
                }
                noteDrawings.Add(serializedDrawing);
                index++;
            }
            serialized.Drawings = noteDrawings.ToArray();

            // Move up the hierarchy
            return XRCToSerialized(serialized as DisplayType, attributePrefix, stagedEntityId);
        }
        #endregion Displays

        #region Drawings
        private static bool XRCToSerialized(PointsType serialized, string attributePrefix, string stagedEntityId)
        {
            int resLen = 0;
            byte[] resource = new byte[RESOURCEBUFSIZE];
            if (GetEntityBlobAttribute(attributePrefix + DRAWING_POINTS, resource, RESOURCEBUFSIZE, out resLen, stagedEntityId))
            {
                serialized.Items = XRCManager.FromCSV<PointType>(System.Text.Encoding.UTF8.GetString(resource));
            }

            return true;
        }

        private static bool XRCToSerialized(DrawingType serialized, string attributePrefix, string stagedEntityId)
        {
            // Material (optional)
            string attribute = attributePrefix + DRAWING_MATERIAL;
            if (GetEntityBooleanAttribute(attribute, out bool material, stagedEntityId))
            {
                serialized.Material ??= new MaterialType();
                if (!XRCToSerialized(serialized.Material, attribute, stagedEntityId))
                {
                    Debug.LogWarning(nameof(DrawingType) + " create event: A problem ocurred reading the drawing material");
                    return false;
                }
            }

            // Color Gradient (optional)
            attribute = attributePrefix + DRAWING_GRADIENT;
            if (GetEntityBooleanAttribute(attribute, out bool gradient, stagedEntityId))
            {
                serialized.Item ??= new ColorGradientType();
                if (!XRCToSerialized(serialized.Item as ColorGradientType, attribute, stagedEntityId))
                {
                    Debug.LogWarning(nameof(DrawingType) + " create event: A problem ocurred reading the drawing color gradient");
                    return false;
                }
            }

            // Color Components (optional)
            attribute = attributePrefix + DRAWING_RGBA;
            if (GetEntityBooleanAttribute(attribute, out bool rgba, stagedEntityId))
            {
                ColorComponentsType item = new ColorComponentsType();
                if (!XRCToSerialized(item, attribute, stagedEntityId))
                {
                    Debug.LogWarning(nameof(DrawingType) + " create event: A problem ocurred reading the drawing RGBA");
                    return false;
                }
                serialized.Item = item;
            }

            // Predefined Color (Optional but has value)
            attribute = attributePrefix + DRAWING_COLOR;
            if (!rgba && GetEntityStringAttribute(attribute, out string color, stagedEntityId))
            {
                if (!Enum.TryParse(color, true, out ColorPredefinedType item))
                {
                    Debug.LogWarning(nameof(DrawingType) + " create event: Unexpected drawing color value: " + color);
                    return false;
                }
                serialized.Item = item;
            }

            // Width (optional)
            attribute = attributePrefix + DRAWING_WIDTH;
            if (GetEntityFloatAttribute(attribute, out float width, stagedEntityId))
            {
                serialized.Width ??= new LengthType();
                SchemaUtil.SerializeLength(width, serialized.Width);
            }

            // Points
            attribute = attributePrefix + DRAWING_POINTS;
            if (GetEntityBooleanAttribute(attribute, out bool points, stagedEntityId))
            {
                serialized.Points ??= new PointsType();
                if (!XRCToSerialized(serialized.Points, attribute, stagedEntityId))
                {
                    Debug.LogWarning(nameof(DrawingType) + " create event: A problem ocurred reading the drawing points");
                    return false;
                }
            }

            // Move up the hierarchy
            return XRCToSerialized(serialized as InteractableSceneObjectType, attributePrefix, stagedEntityId);
        }

        private static bool XRCToSerialized(Drawing2dType serialized, string attributePrefix, string stagedEntityId)
        {
            // Move up the hierarchy
            return XRCToSerialized(serialized as DrawingType, attributePrefix, stagedEntityId);
        }

        private static bool XRCToSerialized(Drawing3dType serialized, string attributePrefix, string stagedEntityId)
        {
            // Render Type (Optional but has value)
            string attribute = attributePrefix + DRAWING_RENDER_TYPE;
            if (!GetEntityStringAttribute(attribute, out string renderType, stagedEntityId))
            {
                Debug.LogWarning(nameof(Drawing3dType) + " create event: Required field is null: " + attribute);
                return false;
            }
            if (!Enum.TryParse(renderType, true, out DrawingRender3dType drawingRenderType))
            {
                Debug.LogWarning(nameof(Drawing3dType) + " create event: Unexpected drawing render type value: " + renderType);
                return false;
            }
            serialized.Type = drawingRenderType;

            // Display Measurement (Optional but has value)
            attribute = attributePrefix + DRAWING_DISPLAY_MEASUREMENT;
            if (!GetEntityBooleanAttribute(attribute, out bool displayMeasurement, stagedEntityId))
            {
                Debug.LogWarning(nameof(AnnotationType) + " create event: Required field is null: " + attribute);
                return false;
            }
            serialized.DisplayMeasurement = displayMeasurement;

            // Units (Optional but has value)
            attribute = attributePrefix + DRAWING_UNITS;
            if (!GetEntityStringAttribute(attribute, out string units, stagedEntityId))
            {
                Debug.LogWarning(nameof(Drawing3dType) + " create event: Required field is null: " + attribute);
                return false;
            }
            if (!Enum.TryParse(units, true, out LengthUnitType drawingUnits))
            {
                Debug.LogWarning(nameof(Drawing3dType) + " create event: Unexpected drawing units: " + units);
                return false;
            }
            serialized.Units = drawingUnits;

            // Length Limit (optional)
            attribute = attributePrefix + DRAWING_LENGTH_LIMIT;
            if (GetEntityFloatAttribute(attribute, out float lengthLimit, stagedEntityId))
            {
                serialized.LengthLimit ??= new LengthType();
                SchemaUtil.SerializeLength(lengthLimit, serialized.LengthLimit);
            }

            // Move up the hierarchy
            return XRCToSerialized(serialized as DrawingType, attributePrefix, stagedEntityId);
        }
        #endregion Drawings

        #region Annotations
        private static bool XRCToSerialized(AnnotationType serialized, string attributePrefix, string stagedEntityId)
        {
            // Start delay (Optional but has value)
            string attribute = attributePrefix + ANNOTATION_START_DELAY;
            if (!GetEntityFloatAttribute(attribute, out float startDelay, stagedEntityId))
            {
                Debug.LogWarning(nameof(AnnotationType) + " create event: Required field is null: " + attribute);
                return false;
            }
            serialized.StartDelay ??= new DurationType();
            SchemaUtil.SerializeDuration(startDelay, serialized.StartDelay);

            // Loop
            attribute = attributePrefix + ANNOTATION_LOOP;
            if (!GetEntityBooleanAttribute(attribute, out bool loop, stagedEntityId))
            {
                Debug.LogWarning(nameof(AnnotationType) + " create event: Required field is null: " + attribute);
                return false;
            }
            serialized.Loop = loop;

            // AttachTo
            attribute = attributePrefix + ANNOTATION_ATTACHTO;
            if (!GetEntityStringAttribute(attribute, out string attachTo, stagedEntityId))
            {
                Debug.LogWarning(nameof(AnnotationType) + " create event: Required field is null: " + attribute);
                return false;
            }
            serialized.AttachTo = attachTo;

            // Move up the hierarchy
            return XRCToSerialized(serialized as IdentifiableType, attributePrefix, stagedEntityId);
        }

        private static bool XRCToSerialized(TextAnnotationType serialized, string attributePrefix, string stagedEntityId)
        {
            // Text (required)
            List<string> texts = new List<string>();
            int index = 0;
            string attribute = attributePrefix + TEXT_ANNOTATION_TEXT;
            while (GetEntityStringAttribute(attribute + "." + index, out string text, stagedEntityId))
            {
                texts.Add(text);
                index++;
            }
            if (texts.Count == 0)
            {
                Debug.LogWarning(nameof(TextAnnotationType) + " create event: Required field is null: " + attribute);
                return false;
            }
            serialized.Texts ??= new TextsType();
            serialized.Texts.Items = texts.ToArray();

            // Text Index (Optional but has value)
            attribute = attributePrefix + TEXT_ANNOTATION_TEXT_INDEX;
            if (!GetEntityIntegerAttribute(attribute, out int textIndex, stagedEntityId))
            {
                Debug.LogWarning(nameof(TextAnnotationType) + " create event: Required field is null: " + attribute);
                return false;
            }
            if ((textIndex < 0) && (textIndex > (texts.Count - 1)))
            {
                Debug.LogWarning(nameof(TextAnnotationType) + " create event: Invalid index supplied: " + textIndex);
                return false;
            }
            serialized.TextIndex = textIndex;

            // Time per text (Optional but has value)
            attribute = attributePrefix + TEXT_ANNOTATION_TIME_PER_TEXT;
            if (!GetEntityFloatAttribute(attribute, out float timePerText, stagedEntityId))
            {
                Debug.LogWarning(nameof(TextAnnotationType) + " create event: Required field is null: " + attribute);
                return false;
            }
            serialized.TimePerText ??= new DurationType();
            SchemaUtil.SerializeDuration(timePerText, serialized.TimePerText);

            // Move up the hierarchy
            return XRCToSerialized(serialized as AnnotationType, attributePrefix, stagedEntityId);
        }

        private static bool XRCToSerialized(AudioAnnotationType serialized, string attributePrefix, string stagedEntityId)
        {
            // Move up the hierarchy
            return XRCToSerialized(serialized as SourceAnnotationType, attributePrefix, stagedEntityId);
        }

        private static bool XRCToSerialized(SourceAnnotationType serialized, string attributePrefix, string stagedEntityId)
        {
            // Start time (Optional but has a value)
            string attribute = attributePrefix + SOURCE_ANNOTATION_START_TIME;
            if (!GetEntityFloatAttribute(attribute, out float startTime, stagedEntityId))
            {
                Debug.LogWarning(nameof(SourceAnnotationType) + " create event: Required field is null: " + attribute);
                return false;
            }
            serialized.StartTime ??= new DurationType();
            SchemaUtil.SerializeDuration(startTime, serialized.StartTime);

            // Duration (Optional but has a value)
            attribute = attributePrefix + SOURCE_ANNOTATION_DURATION;
            if (!GetEntityFloatAttribute(attribute, out float duration, stagedEntityId))
            {
                Debug.LogWarning(nameof(SourceAnnotationType) + " create event: Required field is null: " + attribute);
                return false;
            }
            serialized.Duration ??= new DurationType();
            SchemaUtil.SerializeDuration(duration, serialized.Duration);

            // Speed (Optional but has a value)
            attribute = attributePrefix + SOURCE_ANNOTATION_SPEED;
            if (!GetEntityFloatAttribute(attribute, out float speed, stagedEntityId))
            {
                Debug.LogWarning(nameof(SourceAnnotationType) + " create event: Required field is null: " + attribute);
                return false;
            }
            serialized.Speed = speed;

            // FIXME: Source not implemented

            // Move up the hierarchy
            return XRCToSerialized(serialized as AnnotationType, attributePrefix, stagedEntityId);
        }
        #endregion Annotation
        #endregion XRCToSerialized

        #region SerializedToXRC
        #region Types
        private static bool SerializedToXRC(string entityId, TransformPositionType serialized)
        {
            // Transform position
            Vector3 position = Vector3.zero;
            SchemaUtil.DeserializeTransformPosition(serialized, ref position);

            // Transform (XRC has special provisions for transforms)
            return SetEntityPosition(entityId, position, serialized.referenceSpace);
        }

        private static bool SerializedToXRC(string entityId, TransformScaleType serialized)
        {
            // Transform scale
            Vector3 scale = Vector3.one;
            SchemaUtil.DeserializeTransformScale(serialized, ref scale);

            // Transform (XRC has special provisions for transforms)
            return SetEntityScale(entityId, scale);
        }

        private static bool SerializedToXRC(string entityId, TransformRotationType serialized)
        {
            // Transform rotation
            Quaternion rotation = new Quaternion();
            if (serialized is TransformEulerRotationType)
            {
                TransformEulerRotationType eulerRotationType = serialized as TransformEulerRotationType;

                // Convert Euler to Quaternion
                Vector3 euler = Vector3.zero;
                SchemaUtil.DeserializeTransformRotation(eulerRotationType, ref euler);
                rotation = Quaternion.Euler(euler);
            }
            else
            {
                TransformQRotationType qRotationType = serialized as TransformQRotationType;

                // Extract the Quaternion
                SchemaUtil.DeserializeTransformRotation(qRotationType, ref rotation);
            }

            // Transform (XRC has special provisions for transforms)
            return SetEntityQRotation(entityId, rotation, serialized.referenceSpace);
        }

        private static bool SerializedToXRC(string entityId, TransformType serialized)
        {
            // Transform position
            if (!SerializedToXRC(entityId, serialized.Position))
            {
                Debug.LogWarning(nameof(TransformType) + " A problem ocurred adding the transform position");
                return false;
            }

            // Transform scale
            if (!SerializedToXRC(entityId, serialized.Item1))
            {
                Debug.LogWarning(nameof(TransformType) + " A problem ocurred adding the transform scale");
                return false;
            }

            // Transform rotation
            if (!SerializedToXRC(entityId, serialized.Item))
            {
                Debug.LogWarning(nameof(TransformType) + " A problem ocurred adding the transform rotation");
                return false;
            }

            return true;
        }

        private static bool SerializedToXRC(string entityId, MaterialShaderType serialized, string attributePrefix)
        {
            if (string.IsNullOrEmpty(serialized.Value))
            {
                Debug.LogWarning(nameof(MaterialShaderType) + " Invalid shader definition. Value must be defined.");
                return false;
            }

            // Shader Type (Optional but has value)
            SetEntityStringAttribute(entityId, attributePrefix + MATERIAL_SHADER_TYPE, serialized.shader.ToString());

            // Value (required)
            SetEntityStringAttribute(entityId, attributePrefix + MATERIAL_SHADER_VALUE, serialized.Value);

            return true;
        }

        private static bool SerializedToXRC(string entityId, MaterialFileType serialized, string attributePrefix)
        {
            if (string.IsNullOrEmpty(serialized.Value))
            {
                Debug.LogWarning(nameof(MaterialFileType) + " Invalid Material file definition. Value must be defined.");
                return false;
            }

            // Format (Optional but has value)
            SetEntityStringAttribute(entityId, attributePrefix + MATERIAL_FILE_FORMAT, serialized.format.ToString());

            // Value (required)
            SetEntityStringAttribute(entityId, attributePrefix + MATERIAL_FILE_VALUE, serialized.Value);

            return true;
        }

        private static bool SerializedToXRC(string entityId, ColorComponentsType serialized, string attributePrefix)
        {
            // R (optional)
            SetEntityIntegerAttribute(entityId, attributePrefix + COLOR_COMPONENT_R, serialized.R);

            // G (optional)
            SetEntityIntegerAttribute(entityId, attributePrefix + COLOR_COMPONENT_G, serialized.G);

            // B (optional)
            SetEntityIntegerAttribute(entityId, attributePrefix + COLOR_COMPONENT_B, serialized.B);

            // A (optional)
            SetEntityIntegerAttribute(entityId, attributePrefix + COLOR_COMPONENT_A, serialized.A);

            return true;
        }

        private static bool SerializedToXRC(string entityId, ColorGradientType serialized, string attributePrefix)
        {
            if ((serialized.Time == null) || (serialized.Items == null))
            {
                Debug.LogWarning(nameof(ColorGradientType) + " Invalid segment definition. Array lengths for time and colors cannot be null.");
                return false;
            }
            if (serialized.Time.Length != serialized.Items.Length)
            {
                Debug.LogWarning(nameof(ColorGradientType) + " Inconsistent segment definition. Array lengths for time and colors do not match.");
                return false;
            }
            if (serialized.Time.Length == 0)
            {
                Debug.LogWarning(nameof(ColorGradientType) + " Invalid segment definition. Array lengths for time and colors cannot be 0.");
                return false;
            }

            // Gradient segments
            for (int index = 0; index < serialized.Time.Length; index++)
            {
                string segmentPrefix = attributePrefix + "." + index;

                // Segment Indicator
                SetEntityBooleanAttribute(entityId, segmentPrefix, true);

                // Time
                string segmentAttribute = segmentPrefix + COLOR_GRADIENT_TIME;
                SetEntityFloatAttribute(entityId, segmentAttribute, serialized.Time[index]);

                // Determine the segment color definition
                object item = serialized.Items[index];
                if (item is ColorComponentsType)
                {
                    // Color Components
                    segmentAttribute = segmentPrefix + COLOR_GRADIENT_RGBA;
                    if (!SerializedToXRC(entityId, item as ColorComponentsType, segmentAttribute))
                    {
                        Debug.LogWarning(nameof(ColorGradientType) + " A problem ocurred adding the gradient RGBA");
                        return false;
                    }
                    SetEntityBooleanAttribute(entityId, segmentAttribute, true);
                }
                else if (item is ColorPredefinedType type)
                {
                    // Predefined color
                    segmentAttribute = segmentPrefix + COLOR_GRADIENT_COLOR;
                    SetEntityStringAttribute(entityId, segmentAttribute, type.ToString());
                }
                else
                {
                    // Unknown
                    Debug.LogWarning(nameof(ColorGradientType) + " Invalid segment definition. Unrecognized gradient segment color.");
                    return false;
                }
            }

            return true;
        }

        private static bool SerializedToXRC(string entityId, AssetType serialized, string attributePrefix)
        {
            if (string.IsNullOrEmpty(serialized.AssetName) || string.IsNullOrEmpty(serialized.AssetBundle))
            {
                Debug.LogError(nameof(AssetType) + " Invalid asset definition: Asset name and bundle must be defined: " +
                    "AssetName: " + serialized.AssetName + "; " +
                    "AssetBundle: " + serialized.AssetBundle);
                return false;
            }

            // Asset Name (required)
            string attribute = attributePrefix + ASSET_NAME;
            SetEntityStringAttribute(entityId, attribute, serialized.AssetName);

            // Asset Bundle (required)
            attribute = attributePrefix + ASSET_BUNDLE;
            SetEntityStringAttribute(entityId, attribute, serialized.AssetBundle);

            return true;
        }

        private static bool SerializedToXRC(string entityId, MaterialType serialized, string attributePrefix)
        {
            if (serialized.Items.Length == 0)
            {
                Debug.LogWarning(nameof(MaterialType) + " Invalid material definition.");
                return false;
            }

            foreach (object item in serialized.Items)
            {
                string attribute;

                // Determine the material definition
                if (item is AssetType)
                {
                    // Asset
                    attribute = attributePrefix + MATERIAL_ASSET;
                    if (!SerializedToXRC(entityId, item as AssetType, attribute))
                    {
                        Debug.LogWarning(nameof(MaterialType) + " Required field could not be added: " + attribute);
                        return false;
                    }
                    SetEntityBooleanAttribute(entityId, attribute, true);
                }
                else if (item is MaterialFileType)
                {
                    // Material File
                    attribute = attributePrefix + MATERIAL_FILE;
                    if (!SerializedToXRC(entityId, item as MaterialFileType, attribute))
                    {
                        Debug.LogWarning(nameof(MaterialType) + " Required field could not be added: " + attribute);
                        return false;
                    }
                    SetEntityBooleanAttribute(entityId, attribute, true);
                }
                else if (item is MaterialShaderType)
                {
                    // Material Shader
                    attribute = attributePrefix + MATERIAL_SHADER;
                    if (!SerializedToXRC(entityId, item as MaterialShaderType, attribute))
                    {
                        Debug.LogWarning(nameof(MaterialType) + " Required field could not be added: " + attribute);
                        return false;
                    }
                    SetEntityBooleanAttribute(entityId, attribute, true);
                }
                else if (item is ColorComponentsType)
                {
                    // Color Components
                    attribute = attributePrefix + MATERIAL_RGBA;
                    if (!SerializedToXRC(entityId, item as ColorComponentsType, attribute))
                    {
                        Debug.LogWarning(nameof(MaterialType) + " Required field could not be added: " + attribute);
                        return false;
                    }
                    SetEntityBooleanAttribute(entityId, attribute, true);
                }
                else if (item is ColorPredefinedType type)
                {
                    // Predefined color
                    attribute = attributePrefix + MATERIAL_COLOR;
                    SetEntityStringAttribute(entityId, attribute, type.ToString());
                }
                else
                {
                    // Unknown
                    Debug.LogWarning(nameof(MaterialType) + " Invalid material definition. Unrecognized item.");
                    return false;
                }
            }

            return true;
        }

        private static bool SerializedToXRC(string entityId, ModelFileType serialized, string attributePrefix)
        {
            if (string.IsNullOrEmpty(serialized.Value))
            {
                Debug.LogWarning(nameof(ModelFileType) + " Invalid Model file definition. Value must be defined.");
                return false;
            }

            // Format (Optional but has value)
            SetEntityStringAttribute(entityId, attributePrefix + MODEL_FILE_FORMAT, serialized.format.ToString());

            // Value (required)
            SetEntityStringAttribute(entityId, attributePrefix + MODEL_FILE_VALUE, serialized.Value);

            return true;
        }

        private static bool SerializedToXRC(string entityId, PrimitiveShapeType serialized, string attributePrefix)
        {
            // Shape (Optional but has value)
            SetEntityStringAttribute(entityId, attributePrefix + MODEL_SHAPE_TYPE, serialized.ToString());

            return true;
        }

        private static bool SerializedToXRC(string entityId, ModelType serialized, string attributePrefix)
        {
            string attribute;

            // Material (optional)
            attribute = attributePrefix + MODEL_MATERIAL;
            if (!SerializedToXRC(entityId, serialized.Material, attribute))
            {
                Debug.LogWarning(nameof(MaterialType) + " Required field could not be added: " + attribute);
                return false;
            }
            SetEntityBooleanAttribute(entityId, attribute, true);

            // Determine the model definition
            object item = serialized.Item;
            if (item is AssetType)
            {
                // Asset
                attribute = attributePrefix + MODEL_ASSET;
                if (!SerializedToXRC(entityId, item as AssetType, attribute))
                {
                    Debug.LogWarning(nameof(ModelType) + " Required field could not be added: " + attribute);
                    return false;
                }
                SetEntityBooleanAttribute(entityId, attribute, true);
            }
            else if (item is ModelFileType)
            {
                // Material File
                attribute = attributePrefix + MODEL_FILE;
                if (!SerializedToXRC(entityId, item as ModelFileType, attribute))
                {
                    Debug.LogWarning(nameof(ModelType) + " Required field could not be added: " + attribute);
                    return false;
                }
                SetEntityBooleanAttribute(entityId, attribute, true);
            }
            else if (item is PrimitiveShapeType)
            {
                // Material Shader
                attribute = attributePrefix + MODEL_SHAPE;
                if (!SerializedToXRC(entityId, (PrimitiveShapeType) item, attribute))
                {
                    Debug.LogWarning(nameof(ModelType) + " Required field could not be added: " + attribute);
                    return false;
                }
                SetEntityBooleanAttribute(entityId, attribute, true);
            }
            else
            {
                // Unknown
                Debug.LogWarning(nameof(ModelType) + " Invalid model definition. Unrecognized item.");
                return false;
            }

            return true;
        }

        private static bool SerializedToXRC(string entityId, InteractionSettingsType serialized, string attributePrefix)
        {
            // Enable Interaction (optional but has value)
            SetEntityBooleanAttribute(entityId, attributePrefix + INTERACTABLE_ENABLE_INTERACTION, serialized.EnableInteraction);

            // Enable Usability (optional but has value)
            SetEntityBooleanAttribute(entityId, attributePrefix + INTERACTABLE_ENABLE_USABILITY, serialized.EnableUsability);

            return true;
        }

        private static bool SerializedToXRC(string entityId, TelemetrySettingsType serialized, string attributePrefix)
        {
            // Shade for limits (optional but has value)
            SetEntityBooleanAttribute(entityId, attributePrefix + INTERACTABLE_TELEMETRY_SHADE_FOR_LIMITS, serialized.ShadeForLimitViolations);

            // Telemetry Keys (optional but has value)
            if (serialized.TelemetryKey != null)
            {
                for (int index = 0; index < serialized.TelemetryKey.Length; index++)
                {
                    string attribute = attributePrefix + INTERACTABLE_TELEMETRY_KEYS + "." + index;
                    if (!SetEntityStringAttribute(entityId, attribute, serialized.TelemetryKey[index]))
                    {
                        Debug.LogWarning(nameof(TelemetrySettingsType) + " A problem ocurred adding the telemetry key: " + attribute);
                        return false;
                    }
                    SetEntityBooleanAttribute(entityId, attribute, true);
                }
            }

            return true;
        }

        private static bool SerializedToXRC(string entityId, PhysicsSettingsType serialized, string attributePrefix)
        {
            // Enable Collisions (optional but has a value)
            SetEntityBooleanAttribute(entityId, attributePrefix + PHYSICS_ENABLE_COLLISIONS, serialized.EnableCollisions);

            // Enable Gravity (optional but has a value)
            SetEntityBooleanAttribute(entityId, attributePrefix + PHYSICS_ENABLE_GRAVITY, serialized.EnableGravity);

            return true;
        }
        #endregion Types

        #region Core
        private static bool SerializedToXRC(string entityId, VersionedType serialized, string attributePrefix)
        {
            if (string.IsNullOrEmpty(serialized.version))
            {
                Debug.LogWarning(nameof(VersionedType) + " Invalid definition. Version must be defined.");
                return false;
            }

            // Version
            SetEntityStringAttribute(entityId, attributePrefix + VERSION, serialized.version);

            return true;
        }

        private static bool SerializedToXRC(string entityId, XRType serialized, string attributePrefix)
        {
            // AR Enabled (optional but has a value)
            SetEntityBooleanAttribute(entityId, attributePrefix + XR_AR_ENABLED, serialized.AREnabled);

            // VR Enabled (optional but has a value)
            SetEntityBooleanAttribute(entityId, attributePrefix + XR_VR_ENABLED, serialized.VREnabled);

            // Move up the hierarchy
            return SerializedToXRC(entityId, serialized as VersionedType, attributePrefix);
        }

        private static bool SerializedToXRC(string entityId, IdentifiableType serialized, string attributePrefix)
        {
            if (string.IsNullOrEmpty(serialized.UUID))
            {
                Debug.LogWarning(nameof(IdentifiableType) + " Invalid definition. UUID must be defined.");
                return false;
            }
            if (string.IsNullOrEmpty(serialized.ID))
            {
                Debug.LogWarning(nameof(IdentifiableType) + " Invalid definition. ID must be defined.");
                return false;
            }
            if (string.IsNullOrEmpty(serialized.Name))
            {
                Debug.LogWarning(nameof(IdentifiableType) + " Invalid definition. Name must be defined.");
                return false;
            }

            // UUID
            SetEntityStringAttribute(entityId, attributePrefix + UUID, serialized.UUID);

            // ID
            SetEntityStringAttribute(entityId, attributePrefix + ID, serialized.ID);

            // Name
            SetEntityStringAttribute(entityId, attributePrefix + NAME, serialized.Name);

            // Description
            SetEntityStringAttribute(entityId, attributePrefix + DESCRIPTION, serialized.Description);

            return SerializedToXRC(entityId, serialized as XRType, attributePrefix);
        }

        private static bool SerializedToXRC(string entityId, SceneObjectType serialized, string attributePrefix)
        {
            if (serialized.Transform == null)
            {
                Debug.LogWarning(nameof(SceneObjectType) + " Invalid definition. Transform must be defined.");
                return false;
            }

            // Transform
            if (!SerializedToXRC(entityId, serialized.Transform))
            {
                Debug.LogWarning(nameof(SceneObjectType) + " A problem ocurred adding the transform");
                return false;
            }

            // TODO: Scripts

            // Child Scene Objects
            if (serialized.ChildSceneObjects != null)
            {
                string attribute = attributePrefix + SCENE_OBJECT_CHILDREN;
                if (!SerializedToXRC(entityId, serialized.ChildSceneObjects, attribute))
                {
                    Debug.LogWarning(nameof(SceneObjectType) + " A problem ocurred adding the child scene objects");
                    return false;
                }
                SetEntityBooleanAttribute(entityId, attribute, true);
            }

            // Move up the hierarchy
            return SerializedToXRC(entityId, serialized as IdentifiableType, attributePrefix);
        }

        private static bool SerializedToXRC(string entityId, InteractableSceneObjectType serialized, string attributePrefix)
        {
            // Interactions (optional)
            if (serialized.Interactions != null)
            {
                // Interaction Settings
                string attribute = attributePrefix + INTERACTABLE_INTERACTIONS;
                if (!SerializedToXRC(entityId, serialized.Interactions, attribute))
                {
                    Debug.LogWarning(nameof(InteractableSceneObjectType) + " A problem ocurred adding the object interactions");
                    return false;
                }
                SetEntityBooleanAttribute(entityId, attribute, true);
            }

            // Telemetry Settings (optional)
            if (serialized.Telemetry != null)
            {
                string attribute = attributePrefix + INTERACTABLE_TELEMETRY_SETTINGS;
                if (!SerializedToXRC(entityId, serialized.Telemetry, attribute))
                {
                    Debug.LogWarning(nameof(InteractableSceneObjectType) + " A problem ocurred adding the object telemetry settings");
                    return false;
                }
                SetEntityBooleanAttribute(entityId, attribute, true);
            }

            // Visible (optional but has value)
            SetEntityBooleanAttribute(entityId, attributePrefix + VISIBLE, serialized.Visible);

            // Opacity (optional but has value)
            SetEntityIntegerAttribute(entityId, attributePrefix + OPACITY, serialized.Opacity);

            // Highlight Material (optional)
            if (serialized.HighlightMaterial != null)
            {
                string attribute = attributePrefix + INTERACTABLE_HIGHLIGHT_MATERIAL;
                if (!SerializedToXRC(entityId, serialized.HighlightMaterial, attribute))
                {
                    Debug.LogWarning(nameof(InteractableSceneObjectType) + " A problem ocurred adding the object highlight material");
                    return false;
                }
                SetEntityBooleanAttribute(entityId, attribute, true);
            }

            // Selection Material (optional)
            if (serialized.SelectionMaterial != null)
            {
                string attribute = attributePrefix + INTERACTABLE_SELECTION_MATERIAL;
                if (!SerializedToXRC(entityId, serialized.SelectionMaterial, attribute))
                {
                    Debug.LogWarning(nameof(InteractableSceneObjectType) + " A problem ocurred adding the object selection material");
                    return false;
                }
                SetEntityBooleanAttribute(entityId, attribute, true);
            }

            // Move up the hierarchy
            return SerializedToXRC(entityId, serialized as SceneObjectType, attributePrefix);
        }

        private static bool SerializedToXRC(string entityId, MassSpecificationsType serialized, string attributePrefix)
        {
            MassType serializedMass;

            // Min Mass (optional)
            serializedMass = serialized.Min;
            if (serializedMass != null)
            {
                float mass = 0f;
                SchemaUtil.DeserializeMass(serializedMass, ref mass);
                SetEntityFloatAttribute(entityId, attributePrefix + SPECS_MASS_MIN, mass);
            }

            // Max Mass (optional)
            serializedMass = serialized.Max;
            if (serializedMass != null)
            {
                float mass = 0f;
                SchemaUtil.DeserializeMass(serializedMass, ref mass);
                SetEntityFloatAttribute(entityId, attributePrefix + SPECS_MASS_MAX, mass);
            }

            // Contingency Mass (optional)
            serializedMass = serialized.Contingency;
            if (serializedMass != null)
            {
                float mass = 0f;
                SchemaUtil.DeserializeMass(serializedMass, ref mass);
                SetEntityFloatAttribute(entityId, attributePrefix + SPECS_MASS_CONTINGENCY, mass);
            }

            return true;
        }

        private static bool SerializedToXRC(string entityId, PhysicalSpecificationsType serialized, string attributePrefix)
        {
            string attribute;

            // Part mass (optional)
            if (serialized.Mass != null)
            {
                attribute = attributePrefix + SPECS_MASS;
                if (!SerializedToXRC(entityId, serialized.Mass, attribute))
                {
                    Debug.LogWarning(nameof(PhysicalSpecificationsType) + " A problem ocurred adding the mass specifications");
                    return false;
                }
                SetEntityBooleanAttribute(entityId, attribute, true);
            }

            // Notes (optional but has value)
            attribute = attributePrefix + SPECS_NOTES;
            SetEntityStringAttribute(entityId, attribute, serialized.Notes);

            // Reference (optional but has value)
            attribute = attributePrefix + SPECS_REFERENCE;
            SetEntityStringAttribute(entityId, attribute, serialized.Reference);

            return true;
        }

        private static bool SerializedToXRC(string entityId, PhysicalSceneObjectType serialized, string attributePrefix)
        {
            string attribute;

            // Model (optional)
            if (serialized.Model != null)
            {
                attribute = attributePrefix + MODEL;
                if (!SerializedToXRC(entityId, serialized.Model, attribute))
                {
                    Debug.LogWarning(nameof(PhysicalSceneObjectType) + " A problem ocurred adding the model");
                    return false;
                }
                SetEntityBooleanAttribute(entityId, attribute, true);
            }

            // Physics Settings (optional)
            if (serialized.Physics != null)
            {
                attribute = attributePrefix + PHYSICS_SETTINGS;
                if (!SerializedToXRC(entityId, serialized.Physics, attribute))
                {
                    Debug.LogWarning(nameof(PhysicalSceneObjectType) + " A problem ocurred adding the physics settings");
                    return false;
                }
                SetEntityBooleanAttribute(entityId, attribute, true);
            }

            // Physical Specifications (optional)
            if (serialized.Specifications != null)
            {
                attribute = attributePrefix + SPECS;
                if (!SerializedToXRC(entityId, serialized.Specifications, attribute))
                {
                    Debug.LogWarning(nameof(PhysicalSceneObjectType) + " A problem ocurred adding the physical specifications");
                    return false;
                }
                SetEntityBooleanAttribute(entityId, attribute, true);
            }

            // Randomize Textures (optional but has a value)
            SetEntityBooleanAttribute(entityId, attributePrefix + RANDOMIZE_TEXTURES, serialized.RandomizeTextures);

            // Move up the hierarchy
            return SerializedToXRC(entityId, serialized as InteractableSceneObjectType, attributePrefix);
        }
        #endregion Core

        #region User
        private static bool SerializedToXRC(string entityId, UserComponentType serialized, string attributePrefix)
        {
            if (string.IsNullOrEmpty(serialized.UserId))
            {
                Debug.LogWarning(nameof(UserHeadType) + " Invalid definition. User ID must be defined.");
                return false;
            }

            // User ID
            SetEntityStringAttribute(entityId, attributePrefix + ID, serialized.UserId);

            // Move up the hierarchy
            return SerializedToXRC(entityId, serialized as SceneObjectType, attributePrefix);
        }

        private static bool SerializedToXRC(string entityId, UserHandComponentType serialized, string attributePrefix)
        {
            if (string.IsNullOrEmpty(serialized.HandId))
            {
                Debug.LogWarning(nameof(UserHandComponentType) + " Invalid definition. Hand ID must be defined.");
                return false;
            }

            // Hand ID
            SetEntityStringAttribute(entityId, attributePrefix + USER_HAND_ID, serialized.HandId);

            // Handedness
            string attribute = attributePrefix + USER_HAND_HANDEDNESS;
            SetEntityStringAttribute(entityId, attribute, serialized.Handedness.ToString());

            // Move up the hierarchy
            return SerializedToXRC(entityId, serialized as UserComponentType, attributePrefix);
        }

        private static bool SerializedToXRC(string entityId, UserHeadType serialized, string attributePrefix)
        {
            // Move up the hierarchy
            return SerializedToXRC(entityId, serialized as UserComponentType, attributePrefix);
        }

        private static bool SerializedToXRC(string entityId, UserTorsoType serialized, string attributePrefix)
        {
            // Move up the hierarchy
            return SerializedToXRC(entityId, serialized as UserComponentType, attributePrefix);
        }

        private static bool SerializedToXRC(string entityId, UserHandType serialized, string attributePrefix)
        {
            // Handedness
            string attribute = attributePrefix + USER_HAND_HANDEDNESS;
            SetEntityStringAttribute(entityId, attribute, serialized.Handedness.ToString());

            // Move up the hierarchy
            return SerializedToXRC(entityId, serialized as UserComponentType, attributePrefix);
        }

        private static bool SerializedToXRC(string entityId, UserControllerType serialized, string attributePrefix)
        {
            // Move up the hierarchy
            return SerializedToXRC(entityId, serialized as UserHandComponentType, attributePrefix);
        }

        private static bool SerializedToXRC(string entityId, UserPointerType serialized, string attributePrefix)
        {
            // Move up the hierarchy
            return SerializedToXRC(entityId, serialized as UserHandComponentType, attributePrefix);
        }

        private static bool SerializedToXRC(string entityId, UserType serialized, string attributePrefix)
        {
            // Alias (Optional but has value)
            string attribute = attributePrefix + USER_ALIAS;
            SetEntityStringAttribute(entityId, attribute, serialized.Alias);

            // User Color (Optional but has value)
            object userColor = serialized.Item;
            if (userColor is ColorComponentsType)
            {
                // Color Components
                attribute = attributePrefix + USER_RGBA;
                if (!SerializedToXRC(entityId, userColor as ColorComponentsType, attribute))
                {
                    Debug.LogWarning(nameof(UserType) + " A problem ocurred adding the user RGBA");
                    return false;
                }
                SetEntityBooleanAttribute(entityId, attribute, true);
            }
            else if (userColor is ColorPredefinedType type)
            {
                // Predefined color
                attribute = attributePrefix + USER_COLOR;
                SetEntityStringAttribute(entityId, attribute, type.ToString());
            }
            else
            {
                // Unknown
                Debug.LogWarning(nameof(UserType) + " Invalid color definition. Unrecognized color.");
                return false;
            }

            // Move up the hierarchy
            return SerializedToXRC(entityId, serialized as SceneObjectType, attributePrefix);
        }
        #endregion User

        #region Interface
        private static bool SerializedToXRC(string entityId, InterfaceType serialized, string attributePrefix)
        {
            // Move up the hierarchy
            return SerializedToXRC(entityId, serialized as IdentifiableType, attributePrefix);
        }

        private static bool SerializedToXRC(string entityId, ClientInterfaceType serialized, string attributePrefix)
        {
            if (string.IsNullOrEmpty(serialized.Server))
            {
                Debug.LogWarning(nameof(ClientInterfaceType) + " Required server field is empty.");
                return false;
            }

            // Server (Required)
            string attribute = attributePrefix + CONNECTION_SERVER;
            SetEntityStringAttribute(entityId, attribute, serialized.Server);

            // Port (Required)
            attribute = attributePrefix + CONNECTION_PORT;
            SetEntityIntegerAttribute(entityId, attribute, serialized.Port);

            // Connection Timeout Frequency (optional)
            FrequencyType serializedTimeout = serialized.ConnectionTimeoutFrequency;
            if (serializedTimeout != null)
            {
                float timeout = float.NaN;
                SchemaUtil.DeserializeFrequency(serializedTimeout, ref timeout);
                SetEntityFloatAttribute(entityId, attributePrefix + CONNECTION_TIMEOUT, timeout);
            }

            // Connection Update Frequency (optional)
            FrequencyType serializedUpdate = serialized.ConnectionUpdateFrequency;
            if (serializedUpdate != null)
            {
                float update = float.NaN;
                SchemaUtil.DeserializeFrequency(serializedUpdate, ref update);
                SetEntityFloatAttribute(entityId, attributePrefix + CONNECTION_UPDATE, update);
            }

            // Move up the hierarchy
            return SerializedToXRC(entityId, serialized as InterfaceType, attributePrefix);
        }

        private static bool SerializedToXRC(string entityId, ROSInterfaceType serialized, string attributePrefix)
        {
            if (string.IsNullOrEmpty(serialized.RobotDescription))
            {
                Debug.LogWarning(nameof(ROSInterfaceType) + " Required robot description is empty.");
                return false;
            }

            string attribute;

            // Protocol (Optional but has value)
            attribute = attributePrefix + ROS_PROTOCOL;
            SetEntityStringAttribute(entityId, attribute, serialized.Protocol.ToString());

            // Serializer (Optional but has value)
            attribute = attributePrefix + ROS_SERIALIZER;
            SetEntityStringAttribute(entityId, attribute, serialized.Serializer.ToString());

            // JointStateSubscriberTopic (Optional but has value)
            attribute = attributePrefix + ROS_SUBSCRIBE_TOPIC;
            SetEntityStringAttribute(entityId, attribute, serialized.JointStateSubscriberTopic);

            // JointStatePublisherTopic (Optional but has value)
            attribute = attributePrefix + ROS_PUBLISH_TOPIC;
            SetEntityStringAttribute(entityId, attribute, serialized.JointStatePublisherTopic);

            // Robot Description (required)
            attribute = attributePrefix + ROS_DESCRIPTION;
            SetEntityStringAttribute(entityId, attribute, serialized.RobotDescription);

            // Move up the hierarchy
            return SerializedToXRC(entityId, serialized as ClientInterfaceType, attributePrefix);
        }
        #endregion Interface

        #region Part
        private static bool SerializedToXRC(string entityId, PowerSpecificationsType serialized, string attributePrefix)
        {
            PowerType serializedPower;

            // Min Power (optional)
            serializedPower = serialized.Min;
            if (serializedPower != null)
            {
                float power = 0f;
                SchemaUtil.DeserializePower(serializedPower, ref power);
                SetEntityFloatAttribute(entityId, attributePrefix + PART_POWER_MIN, power);
            }

            // Max Power (optional)
            serializedPower = serialized.Max;
            if (serializedPower != null)
            {
                float power = 0f;
                SchemaUtil.DeserializePower(serializedPower, ref power);
                SetEntityFloatAttribute(entityId, attributePrefix + PART_POWER_MAX, power);
            }

            // Contingency Power (optional)
            serializedPower = serialized.Contingency;
            if (serializedPower != null)
            {
                float power = 0f;
                SchemaUtil.DeserializePower(serializedPower, ref power);
                SetEntityFloatAttribute(entityId, attributePrefix + PART_POWER_CONTINGENCY, power);
            }

            return true;
        }

        private static bool SerializedToXRC(string entityId, PartSpecificationsType serialized, string attributePrefix)
        {
            string attribute;

            // Vendor (required)
            attribute = attributePrefix + PART_VENDOR;
            SetEntityStringAttribute(entityId, attribute, serialized.Vendor);

            // Version (required)
            attribute = attributePrefix + PART_VERSION;
            SetEntityStringAttribute(entityId, attribute, serialized.Version);

            // Part mass (optional)
            if (serialized.Mass1 != null)
            {
                attribute = attributePrefix + SPECS_MASS;
                if (!SerializedToXRC(entityId, serialized.Mass1, attribute))
                {
                    Debug.LogWarning(nameof(PartSpecificationsType) + " A problem ocurred adding the part mass specifications");
                    return false;
                }
                SetEntityBooleanAttribute(entityId, attribute, true);
            }

            // Part power (optional)
            if (serialized.Power != null)
            {
                attribute = attributePrefix + PART_POWER;
                if (!SerializedToXRC(entityId, serialized.Power, attribute))
                {
                    Debug.LogWarning(nameof(PartSpecificationsType) + " A problem ocurred adding the part power specifications");
                    return false;
                }
                SetEntityBooleanAttribute(entityId, attribute, true);
            }

            // Notes (optional but has value)
            attribute = attributePrefix + SPECS_NOTES;
            SetEntityStringAttribute(entityId, attribute, serialized.Notes1);

            // Reference (optional but has value)
            attribute = attributePrefix + SPECS_REFERENCE;
            SetEntityStringAttribute(entityId, attribute, serialized.Reference1);

            return true;
        }

        private static bool SerializedToXRC(string entityId, PartType serialized, string attributePrefix)
        {
            if (serialized.PartSpecifications == null)
            {
                Debug.LogWarning(nameof(PartType) + " Required part specifications are null.");
                return false;
            }

            // Part Specifications (required)
            string attribute = attributePrefix + PART_SPECS;
            if (!SerializedToXRC(entityId, serialized.PartSpecifications, attribute))
            {
                Debug.LogWarning(nameof(PartType) + " A problem ocurred adding the part specifications");
                return false;
            }
            SetEntityBooleanAttribute(entityId, attribute, true);

            // Category (Optional but has value)
            attribute = attributePrefix + PART_CATEGORY;
            SetEntityStringAttribute(entityId, attribute, serialized.Category.ToString());

            // Subsystem (Optional but has value)
            attribute = attributePrefix + PART_SUBSYSTEM;
            SetEntityStringAttribute(entityId, attribute, serialized.Subsystem.ToString());

            // ROS (optional)
            if (serialized.ROSInterface != null)
            {
                attribute = attributePrefix + PART_ROS_INTERFACE;
                if (!SerializedToXRC(entityId, serialized.ROSInterface, attribute))
                {
                    Debug.LogWarning(nameof(PartType) + " A problem ocurred adding the ROS interface");
                    return false;
                }
                SetEntityBooleanAttribute(entityId, attribute, true);
            }

            // Child Parts (optional)
            if (serialized.ChildParts != null)
            {
                attribute = attributePrefix + PART_CHILDREN;
                if (!SerializedToXRC(entityId, serialized.ChildParts, attribute))
                {
                    Debug.LogWarning(nameof(PartType) + " A problem ocurred adding the child parts");
                    return false;
                }
                SetEntityBooleanAttribute(entityId, attribute, true);
            }

            // Move up the hierarchy
            return SerializedToXRC(entityId, serialized as PhysicalSceneObjectType, attributePrefix);
        }

        private static bool SerializedToXRC(string entityId, EnclosureType serialized, string attributePrefix)
        {
            // Move up the hierarchy
            return SerializedToXRC(entityId, serialized as PhysicalSceneObjectType, attributePrefix);
        }
        #endregion Part

        #region Groups
        private static bool SerializedToXRC(string entityId, GroupType serialized, string attributePrefix)
        {
            // Group
            string attribute = attributePrefix + GROUP;
            SetEntityIntegerAttribute(entityId, attribute, serialized.group);

            return SerializedToXRC(entityId, serialized as VersionedType, attributePrefix);
        }

        private static bool SerializedToXRC(string entityId, SceneObjectsType serialized, string attributePrefix)
        {
            // FIXME: Need to build list of scene objects

            return SerializedToXRC(entityId, serialized as GroupType, attributePrefix);
        }

        private static bool SerializedToXRC(string entityId, PartsType serialized, string attributePrefix)
        {
            // Assembly Interaction Settings (optional)
            string attribute = attributePrefix + ASSEMBLY_INTERACTIONS;
            if (!SerializedToXRC(entityId, serialized.AssemblyInteractions, attribute))
            {
                Debug.LogWarning(nameof(PartsType) + " A problem ocurred adding the assembly interactions");
                return false;
            }
            SetEntityBooleanAttribute(entityId, attribute, true);

            // Enclosure (optional)
            if (serialized.Enclosure != null)
            {
                attribute = attributePrefix + PART_ENCLOSURE;
                if (!SerializedToXRC(entityId, serialized.Enclosure, attribute))
                {
                    Debug.LogWarning(nameof(PartsType) + " A problem ocurred adding the part enclosure");
                    return false;
                }
                SetEntityBooleanAttribute(entityId, attribute, true);
            }

            // FIXME: Need to build list of child parts

            return SerializedToXRC(entityId, serialized as GroupType, attributePrefix);
        }
        #endregion Groups

        #region Displays
        private static bool SerializedToXRC(string entityId, DisplayType serialized, string attributePrefix)
        {
            string attribute;

            // Parent ID (optional)
            if (!string.IsNullOrEmpty(serialized.ParentID))
            {
                attribute = attributePrefix + DISPLAY_PARENTID;
                SetEntityStringAttribute(entityId, attribute, serialized.ParentID);
            }

            // Title
            attribute = attributePrefix + DISPLAY_TITLE;
            SetEntityStringAttribute(entityId, attribute, serialized.Title);

            // Width
            attribute = attributePrefix + DISPLAY_WIDTH;
            SetEntityFloatAttribute(entityId, attribute, serialized.Width);

            // Height
            attribute = attributePrefix + DISPLAY_HEIGHT;
            SetEntityFloatAttribute(entityId, attribute, serialized.Height);

            // State (Optional but has value)
            attribute = attributePrefix + DISPLAY_STATE;
            SetEntityStringAttribute(entityId, attribute, serialized.State.ToString());

            // Z-Order
            attribute = attributePrefix + DISPLAY_ZORDER;
            SetEntityIntegerAttribute(entityId, attribute, serialized.Zorder);

            // Move up the hierarchy
            return SerializedToXRC(entityId, serialized as InteractableSceneObjectType, attributePrefix);
        }

        private static bool SerializedToXRC(string entityId, NoteType serialized, string attributePrefix)
        {
            // Content
            string attribute = attributePrefix + NOTE_CONTENT;
            SetEntityStringAttribute(entityId, attribute, serialized.Content);

            // Drawings
            if (serialized.Drawings != null)
            {
                for (int index = 0; index < serialized.Drawings.Length; index++)
                {
                    attribute = attributePrefix + NOTE_DRAWING + "." + index;
                    if (!SerializedToXRC(entityId, serialized.Drawings[index], attribute))
                    {
                        Debug.LogWarning(nameof(NoteType) + " A problem ocurred adding the note drawing: " + attribute);
                        return false;
                    }
                    SetEntityBooleanAttribute(entityId, attribute, true);
                }
            }

            // Move up the hierarchy
            return SerializedToXRC(entityId, serialized as DisplayType, attributePrefix);
        }
        #endregion Displays

        #region Drawings
        private static bool SerializedToXRC(string entityId, PointsType serialized, string attributePrefix)
        {
            if ((serialized.Items == null) || (serialized.Items.Length == 0))
            {
                Debug.LogWarning(nameof(PointsType) + " No points defined.");
                return false;
            }

            // Build the CSV string and encode to a byte array
            string pointsStr = XRCManager.ToCSV(serialized.Items);
            byte[] pointBytes = System.Text.Encoding.UTF8.GetBytes(pointsStr);
            if (pointBytes.Length > RESOURCEBUFSIZE)
            {
                Array.Resize(ref pointBytes, RESOURCEBUFSIZE);
            }
            SetEntityBlobAttribute(entityId, attributePrefix + DRAWING_POINTS, pointBytes);

            return true;
        }

        private static bool SerializedToXRC(string entityId, DrawingType serialized, string attributePrefix)
        {
            if (serialized.Points != null)
            {
                Debug.LogWarning(nameof(DrawingType) + " Drawing points cannot be null.");
                return false;
            }

            string attribute;

            // Material (optional)
            if (serialized.Material != null)
            {
                attribute = attributePrefix + DRAWING_MATERIAL;
                if (!SerializedToXRC(entityId, serialized.Material, attribute))
                {
                    Debug.LogWarning(nameof(DrawingType) + " A problem ocurred adding the drawing material");
                    return false;
                }
                SetEntityBooleanAttribute(entityId, attribute, true);
            }

            // Drawing Color (Optional but has value)
            object drawingColor = serialized.Item;
            if (drawingColor is ColorGradientType)
            {
                attribute = attributePrefix + DRAWING_GRADIENT;
                if (!SerializedToXRC(entityId, drawingColor as ColorGradientType, attribute))
                {
                    Debug.LogWarning(nameof(DrawingType) + " A problem ocurred adding the drawing color gradient");
                    return false;
                }
                SetEntityBooleanAttribute(entityId, attribute, true);
            }
            else if (drawingColor is ColorComponentsType)
            {
                // Color Components
                attribute = attributePrefix + DRAWING_RGBA;
                if (!SerializedToXRC(entityId, drawingColor as ColorComponentsType, attribute))
                {
                    Debug.LogWarning(nameof(UserType) + " A problem ocurred adding the drawing RGBA");
                    return false;
                }
                SetEntityBooleanAttribute(entityId, attribute, true);
            }
            else if (drawingColor is ColorPredefinedType type)
            {
                // Predefined color
                attribute = attributePrefix + DRAWING_COLOR;
                SetEntityStringAttribute(entityId, attribute, type.ToString());
            }

            // Width (optional)
            LengthType serializedWidth = serialized.Width;
            if (serializedWidth != null)
            {
                float width = InteractableDrawingDefaults.WIDTH;
                SchemaUtil.DeserializeLength(serializedWidth, ref width);
                SetEntityFloatAttribute(entityId, attributePrefix + DRAWING_WIDTH, width);
            }

            // Points (Required)
            attribute = attributePrefix + DRAWING_POINTS;
            if (!SerializedToXRC(entityId, serialized.Points, attribute))
            {
                Debug.LogWarning(nameof(DrawingType) + " A problem ocurred adding the drawing points");
                return false;
            }
            SetEntityBooleanAttribute(entityId, attribute, true);

            // Move up the hierarchy
            return SerializedToXRC(entityId, serialized as InteractableSceneObjectType, attributePrefix);
        }

        private static bool SerializedToXRC(string entityId, Drawing2dType serialized, string attributePrefix)
        {
            // Move up the hierarchy
            return SerializedToXRC(entityId, serialized as DrawingType, attributePrefix);
        }

        private static bool SerializedToXRC(string entityId, Drawing3dType serialized, string attributePrefix)
        {
            string attribute;

            // Render Type (Optional but has value)
            attribute = attributePrefix + DRAWING_RENDER_TYPE;
            SetEntityStringAttribute(entityId, attribute, serialized.Type.ToString());

            // Display Measurement (Optional but has value)
            attribute = attributePrefix + DRAWING_DISPLAY_MEASUREMENT;
            SetEntityBooleanAttribute(entityId, attribute, serialized.DisplayMeasurement);

            // Units (Optional but has value)
            attribute = attributePrefix + DRAWING_UNITS;
            SetEntityStringAttribute(entityId, attribute, serialized.Units.ToString());

            // Length Limit (optional)
            LengthType serializedLimit = serialized.LengthLimit;
            if (serializedLimit != null)
            {
                float limit = Interactable3dDrawingDefaults.LIMIT_LENGTH;
                SchemaUtil.DeserializeLength(serializedLimit, ref limit);
                SetEntityFloatAttribute(entityId, attributePrefix + DRAWING_LENGTH_LIMIT, limit);
            }

            // Move up the hierarchy
            return SerializedToXRC(entityId, serialized as DrawingType, attributePrefix);
        }
        #endregion Drawings

        #region Annotations
        private static bool SerializedToXRC(string entityId, AnnotationType serialized, string attributePrefix)
        {
            string attribute;

            // Start delay (Optional but has value)
            float delay = AnnotationDefaults.START_DELAY;
            DurationType serializedDelay = serialized.StartDelay;
            if (serializedDelay != null)
            {
                SchemaUtil.DeserializeDuration(serializedDelay, ref delay);
            }
            attribute = attributePrefix + ANNOTATION_START_DELAY;
            SetEntityFloatAttribute(entityId, attribute, delay);

            // Loop
            attribute = attributePrefix + ANNOTATION_LOOP;
            SetEntityBooleanAttribute(entityId, attribute, serialized.Loop);

            // AttachTo
            attribute = attributePrefix + ANNOTATION_ATTACHTO;
            SetEntityStringAttribute(entityId, attribute, serialized.AttachTo);

            // Move up the hierarchy
            return SerializedToXRC(entityId, serialized as IdentifiableType, attributePrefix);
        }

        private static bool SerializedToXRC(string entityId, TextsType serialized, string attributePrefix)
        {
            if ((serialized.Items == null) || (serialized.Items.Length == 0))
            {
                Debug.LogWarning(nameof(TextsType) + " No texts defined.");
                return false;
            }

            // Text array
            for (int index = 0; index < serialized.Items.Length; index++)
            {
                string attribute = attributePrefix + "." + index;
                SetEntityStringAttribute(entityId, attribute, serialized.Items[index]);
            }

            return true;
        }

        private static bool SerializedToXRC(string entityId, TextAnnotationType serialized, string attributePrefix)
        {
            if (serialized.Texts != null)
            {
                Debug.LogWarning(nameof(TextAnnotationType) + " Texts cannot be null.");
                return false;
            }

            string attribute;

            // Texts (Required)
            attribute = attributePrefix + TEXT_ANNOTATION_TEXT;
            if (!SerializedToXRC(entityId, serialized.Texts, attribute))
            {
                Debug.LogWarning(nameof(TextAnnotationType) + " A problem ocurred adding the texts");
                return false;
            }

            // Text Index (Optional but has value)
            attribute = attributePrefix + TEXT_ANNOTATION_TEXT_INDEX;
            SetEntityIntegerAttribute(entityId, attribute, serialized.TextIndex);

            // Time per text (Optional but has value)
            float timePerText = TextAnnotationDefaults.TIME_PER_TEXT;
            DurationType serializedTimePerText = serialized.TimePerText;
            if (serializedTimePerText != null)
            {
                SchemaUtil.DeserializeDuration(serializedTimePerText, ref timePerText);
            }
            attribute = attributePrefix + TEXT_ANNOTATION_TIME_PER_TEXT;
            SetEntityFloatAttribute(entityId, attribute, timePerText);

            // Move up the hierarchy
            return SerializedToXRC(entityId, serialized as AnnotationType, attributePrefix);
        }

        private static bool SerializedToXRC(string entityId, AudioAnnotationType serialized, string attributePrefix)
        {
            // Move up the hierarchy
            return SerializedToXRC(entityId, serialized as SourceAnnotationType, attributePrefix);
        }

        private static bool SerializedToXRC(string entityId, SourceAnnotationType serialized, string attributePrefix)
        {
            string attribute;

            // Start time (Optional but has a value)
            float startTime = SourceAnnotationDefaults.START_TIME;
            DurationType serializedStartTime = serialized.StartTime;
            if (serializedStartTime != null)
            {
                SchemaUtil.DeserializeDuration(serializedStartTime, ref startTime);
            }
            attribute = attributePrefix + SOURCE_ANNOTATION_START_TIME;
            SetEntityFloatAttribute(entityId, attribute, startTime);

            // Duration (Optional but has a value)
            float duration = SourceAnnotationDefaults.DURATION;
            DurationType serializedDuration = serialized.Duration;
            if (serializedDuration != null)
            {
                SchemaUtil.DeserializeDuration(serializedDuration, ref duration);
            }
            attribute = attributePrefix + SOURCE_ANNOTATION_DURATION;
            SetEntityFloatAttribute(entityId, attribute, duration);

            // Speed (Optional but has a value)
            attribute = attributePrefix + SOURCE_ANNOTATION_SPEED;
            SetEntityFloatAttribute(entityId, attribute, serialized.Speed);

            // FIXME: Source not implemented

            // Move up the hierarchy
            return SerializedToXRC(entityId, serialized as AnnotationType, attributePrefix);
        }
        #endregion Annotation
        #endregion SerializedToXRC

        private static IEntityParameters CreateEntityParameters(IdentifiableType serializedEntity, IIdentifiable parent)
        {
            IEntityParameters entityParameters = null;

            if (serializedEntity == null)
            {
                Debug.LogWarning("Serialized entity is null");
                return entityParameters;
            }

            // Get the parent ID
            string parentId = GetXRCEntityID(parent);

            // Create the entity parameters from the serialized object type
            if (serializedEntity is TextAnnotationType)
            {
                entityParameters = new EntityParameters<TextAnnotationType>(EntityType.entity, serializedEntity as TextAnnotationType, parentId);
            }
            else if (serializedEntity is AudioAnnotationType)
            {
                entityParameters = new EntityParameters<AudioAnnotationType>(EntityType.entity, serializedEntity as AudioAnnotationType, parentId);
            }
            else if (serializedEntity is PartType)
            {
                entityParameters = new EntityParameters<PartType>(EntityType.entity, serializedEntity as PartType, parentId);
            }
            else if (serializedEntity is NoteType)
            {
                entityParameters = new EntityParameters<NoteType>(EntityType.entity, serializedEntity as NoteType, parentId);
            }
            else if (serializedEntity is Drawing2dType)
            {
                entityParameters = new EntityParameters<Drawing2dType>(EntityType.entity, serializedEntity as Drawing2dType, parentId);
            }
            else if (serializedEntity is Drawing3dType)
            {
                entityParameters = new EntityParameters<Drawing3dType>(EntityType.entity, serializedEntity as Drawing3dType, parentId);
            }
            else if (serializedEntity is UserType)
            {
                entityParameters = new UserParameters<UserType>(serializedEntity as UserType);
            }
            else if (serializedEntity is PhysicalSceneObjectType)
            {
                entityParameters = new EntityParameters<PhysicalSceneObjectType>(EntityType.entity, serializedEntity as PhysicalSceneObjectType, parentId);
            }
            else if (serializedEntity is InteractableSceneObjectType)
            {
                entityParameters = new EntityParameters<InteractableSceneObjectType>(EntityType.entity, serializedEntity as InteractableSceneObjectType, parentId);
            }
            else if (serializedEntity is SceneObjectType)
            {
                entityParameters = new EntityParameters<SceneObjectType>(EntityType.entity, serializedEntity as SceneObjectType, parentId);
            }
            else if (serializedEntity is IdentifiableType)
            {
                entityParameters = new EntityParameters<IdentifiableType>(EntityType.entity, serializedEntity);
            }
            else
            {
                Debug.LogWarning("Serialized type unknown: " + serializedEntity?.GetType());
            }

            return entityParameters;
        }

        #region StageEntity
        private static bool StageEntity(IEntityParameters entityParameters)
        {
            if (entityParameters == null)
            {
                Debug.LogWarning("Entity parameters are null");
                return false;
            }

            // Get the ID we are using for the entity
            string entityID = entityParameters.EntityID;
            string category = entityParameters.Category;

            // Initialize the staged entity in XRC. XRC clobbers the staged entity under the same ID.
            bool staged = false;
#if !HOLOLENS_BUILD
            XRCInterface.StagedEntityCreate(entityID);

            // Key parmeters
            SetEntityStringAttribute(entityID, KEY_CATEGORY, category);
            SetEntityStringAttribute(entityID, KEY_PARENTID, entityParameters.ParentID);

            // Mark as staged
            staged = true;

            if (entityParameters != null)
            {
                if (entityParameters.SerializedEntity is TextAnnotationType)
                {
                    staged = SerializedToXRC(entityID, entityParameters.SerializedEntity as TextAnnotationType, category);
                }
                else if (entityParameters.SerializedEntity is AudioAnnotationType)
                {
                    staged = SerializedToXRC(entityID, entityParameters.SerializedEntity as AudioAnnotationType, category);
                }
                else if (entityParameters.SerializedEntity is PartType)
                {
                    staged = SerializedToXRC(entityID, entityParameters.SerializedEntity as PartType, category);
                }
                else if (entityParameters.SerializedEntity is NoteType)
                {
                    staged = SerializedToXRC(entityID, entityParameters.SerializedEntity as NoteType, category);
                }
                else if (entityParameters.SerializedEntity is Drawing2dType)
                {
                    staged = SerializedToXRC(entityID, entityParameters.SerializedEntity as Drawing2dType, category);
                }
                else if (entityParameters.SerializedEntity is Drawing3dType)
                {
                    staged = SerializedToXRC(entityID, entityParameters.SerializedEntity as Drawing3dType, category);
                }
                else if (entityParameters.SerializedEntity is UserType)
                {
                    staged = SerializedToXRC(entityID, entityParameters.SerializedEntity as UserType, category);
                }
                else if (entityParameters.SerializedEntity is UserHeadType)
                {
                    staged = SerializedToXRC(entityID, entityParameters.SerializedEntity as UserHeadType, category);
                }
                else if (entityParameters.SerializedEntity is UserTorsoType)
                {
                    staged = SerializedToXRC(entityID, entityParameters.SerializedEntity as UserTorsoType, category);
                }
                else if (entityParameters.SerializedEntity is UserHandType)
                {
                    staged = SerializedToXRC(entityID, entityParameters.SerializedEntity as UserHandType, category);
                }
                else if (entityParameters.SerializedEntity is UserControllerType)
                {
                    staged = SerializedToXRC(entityID, entityParameters.SerializedEntity as UserControllerType, category);
                }
                else if (entityParameters.SerializedEntity is UserPointerType)
                {
                    staged = SerializedToXRC(entityID, entityParameters.SerializedEntity as UserPointerType, category);
                }
                else if (entityParameters.SerializedEntity is PhysicalSceneObjectType)
                {
                    staged = SerializedToXRC(entityID, entityParameters.SerializedEntity as PhysicalSceneObjectType, category);
                }
                else if (entityParameters.SerializedEntity is InteractableSceneObjectType)
                {
                    staged = SerializedToXRC(entityID, entityParameters.SerializedEntity as InteractableSceneObjectType, category);
                }
                else if (entityParameters.SerializedEntity is SceneObjectType)
                {
                    staged = SerializedToXRC(entityID, entityParameters.SerializedEntity as SceneObjectType, category);
                }
                else if (entityParameters.SerializedEntity is IdentifiableType)
                {
                    staged = SerializedToXRC(entityID, entityParameters.SerializedEntity, category);
                }
                else
                {
                    Debug.LogWarning("Serialized type unknown: " + entityParameters.SerializedEntity?.GetType());
                }
            }
            else
            {
                Debug.LogWarning("Entity parameters are null");
            }
#endif
            return staged;
        }

        public static bool StageEntity(IdentifiableType serializedObject, IIdentifiable parent)
        {
            bool result = false;

            if (XRCInterface.IsStarted)
            {
                IEntityParameters entityParameters = CreateEntityParameters(serializedObject, parent);
                if (entityParameters != null)
                {
                    // Stage the entity
                    result = StageEntity(entityParameters);
                }
                else
                {
                    Debug.LogWarning(nameof(StageEntity) + " Entity parameters could not be staged");
                }
            }
            else
            {
                WarnNoSession("XRCUnity->StageEntity()");
            }

            return result;
        }
        #endregion StageEntity

        /// <summary>
        /// Gets the entity parameters from the current event
        /// </summary>
        /// <returns>The <code>IEntityParameters</code> representing the entity parameters</returns>
        protected static IEntityParameters GetEventEntityParameters() => GetEntityParameters();

        /// <summary>
        /// Gets the entity parameters from the staged entity ID
        /// </summary>
        /// <returns>The <code>IEntityParameters</code> representing the entity parameters</returns>
        protected static IEntityParameters GetStagedEntityParameters(string stagedEntityId) => GetEntityParameters(stagedEntityId);

        public static IdentifiableType CreateSerializedType(string fromSerializableTypeName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            Type serializableType = assembly.GetType(fromSerializableTypeName, true);
            return assembly.CreateInstance(serializableType.ToString(), true) as IdentifiableType;
        }

        /// <summary>
        /// Gets the entity parameters from the supplied staged entity ID or from an event
        /// </summary>
        /// <param name="stagedEntityId">If specified, will obtain the value from the specified staged
        ///     entity ID. Otherwise, will assume it was an event that triggered the call.</param>
        /// <returns>The <code>IEntityParameters</code> representing the entity parameters</returns>
        private static IEntityParameters GetEntityParameters(string stagedEntityId = "")
        {
            // Leave result null if the serialization fails
            IEntityParameters result = null;

            // Key parameter to tell us what object type to deserialize
            GetEntityStringAttribute(KEY_CATEGORY, out string categoryStr, stagedEntityId);
            GetEntityIntegerAttribute(KEY_TYPE, out int type, stagedEntityId);
            GetEntityStringAttribute(KEY_PARENTID, out string parentUUID, stagedEntityId);

            // We need the category to tell us what serialized type we are instantiating
            IdentifiableType serializedType = CreateSerializedType(categoryStr);
            if (serializedType is TextAnnotationType)
            {
                var parameters = new EntityParameters<TextAnnotationType>(XRCManager.EntityTypeFromInt(type), serializedType as TextAnnotationType, parentUUID);
                result = XRCToSerialized(parameters.SerializedEntity, categoryStr, stagedEntityId) ? parameters : null;
            }
            else if (serializedType is AudioAnnotationType)
            {
                var parameters = new EntityParameters<AudioAnnotationType>(XRCManager.EntityTypeFromInt(type), serializedType as AudioAnnotationType, parentUUID);
                result = XRCToSerialized(parameters.SerializedEntity, categoryStr, stagedEntityId) ? parameters : null;
            }
            else if (serializedType is PartType)
            {
                var parameters = new EntityParameters<PartType>(XRCManager.EntityTypeFromInt(type), serializedType as PartType, parentUUID);
                result = XRCToSerialized(parameters.SerializedEntity, categoryStr, stagedEntityId) ? parameters : null;
            }
            else if (serializedType is NoteType)
            {
                var parameters = new EntityParameters<NoteType>(XRCManager.EntityTypeFromInt(type), serializedType as NoteType, parentUUID);
                result = XRCToSerialized(parameters.SerializedEntity, categoryStr, stagedEntityId) ? parameters : null;
            }
            else if (serializedType is Drawing2dType)
            {
                var parameters = new EntityParameters<Drawing2dType>(XRCManager.EntityTypeFromInt(type), serializedType as Drawing2dType, parentUUID);
                result = XRCToSerialized(parameters.SerializedEntity, categoryStr, stagedEntityId) ? parameters : null;
            }
            else if (serializedType is Drawing3dType)
            {
                var parameters = new EntityParameters<Drawing3dType>(XRCManager.EntityTypeFromInt(type), serializedType as Drawing3dType, parentUUID);
                result = XRCToSerialized(parameters.SerializedEntity, categoryStr, stagedEntityId) ? parameters : null;
            }
            else if (serializedType is UserType)
            {
                var parameters = new UserParameters<UserType>(serializedType as UserType);
                result = XRCToSerialized(parameters.SerializedEntity, categoryStr, stagedEntityId) ? parameters : null;
            }
            else if (serializedType is UserHeadType)
            {
                var parameters = new EntityParameters<UserHeadType>(XRCManager.EntityTypeFromInt(type), serializedType as UserHeadType, parentUUID);
                result = XRCToSerialized(parameters.SerializedEntity, categoryStr, stagedEntityId) ? parameters : null;
            }
            else if (serializedType is UserTorsoType)
            {
                var parameters = new EntityParameters<UserTorsoType>(XRCManager.EntityTypeFromInt(type), serializedType as UserTorsoType, parentUUID);
                result = XRCToSerialized(parameters.SerializedEntity, categoryStr, stagedEntityId) ? parameters : null;
            }
            else if (serializedType is UserHandType)
            {
                var parameters = new EntityParameters<UserHandType>(XRCManager.EntityTypeFromInt(type), serializedType as UserHandType, parentUUID);
                result = XRCToSerialized(parameters.SerializedEntity, categoryStr, stagedEntityId) ? parameters : null;
            }
            else if (serializedType is UserControllerType)
            {
                var parameters = new EntityParameters<UserControllerType>(XRCManager.EntityTypeFromInt(type), serializedType as UserControllerType, parentUUID);
                result = XRCToSerialized(parameters.SerializedEntity, categoryStr, stagedEntityId) ? parameters : null;
            }
            else if (serializedType is UserPointerType)
            {
                var parameters = new EntityParameters<UserPointerType>(XRCManager.EntityTypeFromInt(type), serializedType as UserPointerType, parentUUID);
                result = XRCToSerialized(parameters.SerializedEntity, categoryStr, stagedEntityId) ? parameters : null;
            }
            else if (serializedType is PhysicalSceneObjectType)
            {
                var parameters = new EntityParameters<PhysicalSceneObjectType>(XRCManager.EntityTypeFromInt(type), serializedType as PhysicalSceneObjectType, parentUUID);
                result = XRCToSerialized(parameters.SerializedEntity, categoryStr, stagedEntityId) ? parameters : null;
            }
            else if (serializedType is InteractableSceneObjectType)
            {
                var parameters = new EntityParameters<InteractableSceneObjectType>(XRCManager.EntityTypeFromInt(type), serializedType as InteractableSceneObjectType, parentUUID);
                result = XRCToSerialized(parameters.SerializedEntity, categoryStr, stagedEntityId) ? parameters : null;
            }
            else if (serializedType is SceneObjectType)
            {
                var parameters = new EntityParameters<SceneObjectType>(XRCManager.EntityTypeFromInt(type), serializedType as SceneObjectType, parentUUID);
                result = XRCToSerialized(parameters.SerializedEntity, categoryStr, stagedEntityId) ? parameters : null;
            }
            else if (serializedType is IdentifiableType)
            {
                var parameters = new EntityParameters<IdentifiableType>(XRCManager.EntityTypeFromInt(type), serializedType as IdentifiableType, parentUUID);
                result = XRCToSerialized(parameters.SerializedEntity, categoryStr, stagedEntityId) ? parameters : null;
            }
            else
            {
                Debug.LogWarning("Entity parameters could not be created becuase unknown serialed type encountered: " + serializedType);
            }

            return result;
        }

        #region Entity callback handlers
        [MonoPInvokeCallback(typeof(XRCInterface.EntityFunctionCallBack))]
        static void entityCreateEvent(string entityID)
        {
            // Create the event parameters
            IEntityParameters entityParameters = GetEventEntityParameters();

            // Record the create event if the event parameters were serialized successfully
            if (entityParameters != null)
            {
                // Sanity check for matching UUIDs. Hopefully never triggered!
                if (entityParameters.EntityID != entityID)
                {
                    Debug.LogWarning("Entity create event: Event entity ID does not match serialized ID.");
                }

                // Stage the entity with the parameters
                StageEntity(entityParameters);

                // Queue the event
                entityCreatedEventQueue.Enqueue(entityParameters);
                Instance.invocationQueue.Enqueue(Instance.entityCreatedUnityEvent);
            }
            else
            {
                Debug.LogWarning("Entity create event: Serialization of event parameters failed");
            }
        }

        [MonoPInvokeCallback(typeof(XRCInterface.EntityFunctionCallBack))]
        static void entityDestroyEvent(string entityID)
        {
            // Create the event parameters
            IEntityParameters entityParameters = GetEventEntityParameters();

            // Record the delete event if the event parameters were serialized successfully
            if (entityParameters != null)
            {
                // Sanity check for matching UUIDs. Hopefully never triggered!
                if (entityParameters.EntityID != entityID)
                {
                    Debug.LogWarning("Entity delete event: Event entity ID does not match serialized ID.");
                }

                // Delete the staged entity
                XRCInterface.StagedEntityDelete(entityID);

                // Queue the event
                entityDestroyedEventQueue.Enqueue(entityParameters);
                Instance.invocationQueue.Enqueue(Instance.entityDestroyedUnityEvent);
            }
            else
            {
                Debug.LogWarning("Entity delete event: Serialization of event parameters failed");
            }
        }

        [MonoPInvokeCallback(typeof(XRCInterface.EntityFunctionCallBack))]
        static void entityReinitializeEvent(string entityID)
        {
            // Create the event parameters
            IEntityParameters entityParameters = GetEventEntityParameters();

            // Record the create event if the event parameters were serialized successfully
            if (entityParameters != null)
            {
                // Sanity check for matching UUIDs. Hopefully never triggered!
                if (entityParameters.EntityID != entityID)
                {
                    Debug.LogWarning("Entity reinitialize event: Event entity ID does not match serialized ID.");
                }

                // Stage the entity with the parameters
                StageEntity(entityParameters);

                // Queue the event
                entityReinitializedEventQueue.Enqueue(entityParameters);
                Instance.invocationQueue.Enqueue(Instance.entityReinitializedUnityEvent);
            }
            else
            {
                Debug.LogWarning("Entity reinitialize event: Serialization of event parameters failed");
            }
        }

        [MonoPInvokeCallback(typeof(XRCInterface.EntityFunctionCallBack))]
        static void entityUpdateEvent(string entityID)
        {
            // Create the event parameters
            IEntityParameters entityParameters = GetEventEntityParameters();

            // Record the create event if the event parameters were serialized successfully
            if (entityParameters != null)
            {
                // Sanity check for matching UUIDs. Hopefully never triggered!
                if (entityParameters.EntityID != entityID)
                {
                    Debug.LogWarning("Entity update event: Event entity ID does not match serialized ID.");
                }

                // Stage the entity with the parameters
                StageEntity(entityParameters);

                // Queue the event
                entityUpdatedEventQueue.Enqueue(entityParameters);
                Instance.invocationQueue.Enqueue(Instance.entityUpdatedUnityEvent);
            }
            else
            {
                Debug.LogWarning("Entity update event: Serialization of event parameters failed");
            }
        }

        [MonoPInvokeCallback(typeof(XRCInterface.EntityFunctionCallBack))]
        static void entityEditEvent(string entityID)
        {
            // Create the event parameters
            IEntityParameters entityParameters = GetEventEntityParameters();

            // Record the create event if the event parameters were serialized successfully
            if (entityParameters != null)
            {
                // Sanity check for matching UUIDs. Hopefully never triggered!
                if (entityParameters.EntityID != entityID)
                {
                    Debug.LogWarning("Entity edit event: Event entity ID does not match serialized ID.");
                }

                // Stage the entity with the parameters
                StageEntity(entityParameters);

                // Queue the event
                entityEditedEventQueue.Enqueue(entityParameters);
                Instance.invocationQueue.Enqueue(Instance.entityEditedUnityEvent);
            }
            else
            {
                Debug.LogWarning("Entity edit event: Serialization of event parameters failed");
            }
        }
        #endregion

        #region Active session callback handlers.
        [MonoPInvokeCallback(typeof(XRCInterface.ActiveSessionJoinedFunctionCallBack))]
        static void joinedEvent()
        {
            Instance.invocationQueue.Enqueue(Instance.sessionJoinedUnityEvent);
        }

        [MonoPInvokeCallback(typeof(XRCInterface.ActiveSessionParticipantFunctionCallBack))]
        static void participantAddedEvent(string participantID)
        {
            // Create the event parameters
            IEntityParameters entityParameters = GetEventEntityParameters();

            // Record the participant add event if the user parameters were serialized successfully
            if (entityParameters is IUserParameters)
            {
                // Sanity check for matching IDs. Hopefully never triggered!
                if (entityParameters.EntityID != participantID)
                {
                    Debug.LogWarning("Participant add event: participant ID does not match serialized ID.");
                }

                // Stage the entity with the parameters
                StageEntity(entityParameters);

                // Queue the event
                asParticipantAddedEventQueue.Enqueue(entityParameters as IUserParameters);
                Instance.invocationQueue.Enqueue(Instance.sessionParticipantAddedUnityEvent);
            }
            else
            {
                Debug.LogWarning("Participant add event: Serialization of user parameters failed");
            }
        }

        [MonoPInvokeCallback(typeof(XRCInterface.ActiveSessionParticipantFunctionCallBack))]
        static void participantResyncEvent(string participantID)
        {
            // Create the event parameters
            IEntityParameters entityParameters = GetEventEntityParameters();

            // Record the participant add event if the user parameters were serialized successfully
            if (entityParameters is IUserParameters)
            {
                // Sanity check for matching IDs. Hopefully never triggered!
                if (entityParameters.EntityID != participantID)
                {
                    Debug.LogWarning("Participant resynch event: participant ID does not match serialized ID.");
                }

                // Stage the entity with the parameters
                StageEntity(entityParameters);

                // Queue the event
                asParticipantAddedEventQueue.Enqueue(entityParameters as IUserParameters);
                Instance.invocationQueue.Enqueue(Instance.sessionParticipantResyncedUnityEvent);
            }
            else
            {
                Debug.LogWarning("Participant resynch event: Serialization of user parameters failed");
            }
        }

        [MonoPInvokeCallback(typeof(XRCInterface.ActiveSessionParticipantFunctionCallBack))]
        static void participantDeletedEvent(string participantID)
        {
            // Create the event parameters
            IEntityParameters entityParameters = GetEventEntityParameters();

            // Record the delete participant event if the user parameters were serialized successfully
            if (entityParameters is IUserParameters)
            {
                // Sanity check for matching UUIDs. Hopefully never triggered!
                if (entityParameters.EntityID != participantID)
                {
                    Debug.LogWarning("Participant delete event: Paricipant ID does not match serialized ID.");
                }

                // Delete the staged entity
                XRCInterface.StagedEntityDelete(participantID);

                // Queue the event
                asParticipantDeletedEventQueue.Enqueue(entityParameters as IUserParameters);
                Instance.invocationQueue.Enqueue(Instance.sessionParticipantDeletedUnityEvent);
            }
            else
            {
                Debug.LogWarning("Entity delete event: Serialization of event parameters failed");
            }
        }
#endregion

#region Remote session callback handlers.
        [MonoPInvokeCallback(typeof(XRCInterface.RemoteSessionFunctionCallBack))]
        static void remoteSessionAddedEvent(string sessionId)
        {
            rsAddedEventQueue.Enqueue(new RemoteSessionEventParameters(sessionId));
            Instance.invocationQueue.Enqueue(Instance.remoteSessionAddedUnityEvent);
        }

        [MonoPInvokeCallback(typeof(XRCInterface.RemoteSessionFunctionCallBack))]
        static void remoteSessionUpdatedEvent(string sessionId)
        {
            rsUpdatedEventQueue.Enqueue(new RemoteSessionEventParameters(sessionId));
            Instance.invocationQueue.Enqueue(Instance.remoteSessionUpdatedUnityEvent);
        }

        [MonoPInvokeCallback(typeof(XRCInterface.RemoteSessionFunctionCallBack))]
        static void remoteSessionDeletedEvent(string sessionId)
        {
            rsDeletedEventQueue.Enqueue(new RemoteSessionEventParameters(sessionId));
            Instance.invocationQueue.Enqueue(Instance.remoteSessionDeletedUnityEvent);
        }
#endregion

        public static string GetXRCEntityID(IIdentifiable entity)
        {
            return (entity != null) ? entity.uuid.ToString() : "";
        }

        public static string GetXRCEntityID(IdentifiableType serializedEntity)
        {
            return (serializedEntity != null) ? serializedEntity.UUID : "";
        }

        public static bool XRCStarted
        {
            get
            {
                return XRCInterface.IsStarted;
            }
        }

        public void SessAdded()
        {
            Debug.Log("session added");
        }

        public static bool IsSessionActive
        {
            get
            {
                return XRCInterface.IsSessionActive;
            }
        }

        public static bool IsMaster
        {
            get
            {
                return XRCInterface.IsMaster;
            }
        }

        public static void Initialize(string server, int port, ConnectionTypes connType, string miss, string sat, string proj = "XRC", string group = "MRET", string sess = "UNSET")
        {
            if (!XRCInterface.IsStarted)
            {
                // Register for remote session events.
                XRCInterface.AddRemoteSessionEventListeners(remoteSessionAddedEvent, remoteSessionUpdatedEvent, remoteSessionDeletedEvent);

                // Register for active session events.
                XRCInterface.AddActiveSessionEventListeners(joinedEvent, participantAddedEvent, participantResyncEvent, participantDeletedEvent);

                // Register for entity events.
                XRCInterface.AddEntityEventListeners(entityCreateEvent, entityDestroyEvent, entityReinitializeEvent, entityUpdateEvent, entityEditEvent);

                // Configure Debug Level.
                XRCInterface.ConfigureSystem(Instance.logLevel);

                // Configure Session Information.
                XRCInterface.ConfigureSession(proj, group, sess, 120000, 5000, 5000);

                // Configure Events.
                XRCInterface.ConfigureEvents(30000, 250, 500);

                // Configure Message Bus.
                XRCInterface.ConfigureMessageBus(server, port, connType, 1000, 10000, miss, sat);
            }
            else
            {
                WarnAlreadyStarted("XRCUnity->Initialize()");
            }
        }

        public static void Destroy()
        {
            XRCInterface.ShutDown();
            XRCInterface.RemoveEntityEventListeners();
            XRCInterface.RemoveActiveSessionEventListeners();
            XRCInterface.RemoveRemoteSessionEventListeners();
        }

        public static bool ConfigureMessageBus(string server, int port, ConnectionTypes connectionType)
        {
            if (!XRCInterface.IsStarted) return XRCInterface.ConfigureMessageBus(server, port, connectionType, 1000, 10000);

            WarnAlreadyStarted("XRCUnity->ConfigureMessageBus()");

            return false;
        }

        public static bool StartUp()
        {
            if (!XRCInterface.IsStarted) return XRCInterface.StartUp();

            WarnAlreadyStarted("XRCUnity->StartUp()");

            return false;
        }

        public static bool ShutDown()
        {
            if (XRCInterface.IsStarted) return XRCInterface.ShutDown();

            WarnNotStarted("XRCUnity->ShutDown()");

            return false;
        }

        public static bool ConfigureSession(string miss, string sat, string proj, string group, string sessName)
        {
            if (!XRCInterface.IsStarted) return XRCInterface.ConfigureSession(proj, group, sessName, 120000, 5000, 5000);

            WarnAlreadyStarted("XRCUnity->ConfigureSession()");

            return false;
        }

        public static bool StartSession(UserType serializedUser)
        {
            bool result = false;

            if (XRCInterface.IsStarted)
            {
                if (!XRCInterface.IsSessionActive)
                {
                    IEntityParameters entityParameters = CreateEntityParameters(serializedUser, null);
                    if (entityParameters is IUserParameters)
                    {
                        // Start the session
                        result = StartSession(entityParameters as IUserParameters);
                    }
                    else
                    {
                        Debug.LogWarning(nameof(StartSession) + " User parameters could not be created");
                    }
                }
                else
                {
                    WarnInSession("XRCUnity->StartSession()");
                }
            }
            else
            {
                WarnNotStarted("XRCUnity->StartSession()");
            }

            return result;
        }

        public static bool StartSession(IUserParameters userParameters)
        {
            bool result = false;

            if (XRCInterface.IsStarted)
            {
                if (!XRCInterface.IsSessionActive)
                {
                    if (userParameters != null)
                    {
                        // Stage the user
                        if (StageEntity(userParameters))
                        {
                            // Add the user to XRC
                            string userId = userParameters.EntityID;
                            result = XRCInterface.StartSession(userId);
                        }
                        else
                        {
                            Debug.LogWarning(nameof(StartSession) + " User parameters could not be staged");
                        }
                    }
                    else
                    {
                        Debug.LogWarning(nameof(StartSession) + " Supplied user parameters are null");
                    }
                }
                else
                {
                    WarnInSession("XRCUnity->StartSession()");
                }
            }
            else
            {
                WarnNotStarted("XRCUnity->StartSession()");
            }

            return result;
        }

        public static bool EndSession()
        {
            bool result = false;

            if (XRCInterface.IsSessionActive)
            {
                if (IsMaster)
                {
                    result = XRCInterface.EndSession();
                }
                else
                {
                    WarnNotMaster("XRCUnity->EndSession()");
                }
            }
            else
            {
                WarnNoSession("XRCUnity->EndSession()");
            }

            return result;
        }

        public static XRCSessionInfo[] GetRemoteSessions()
        {
            XRCSessionInfo[] result = null;

            if (XRCInterface.IsStarted)
            {
                result = XRCInterface.GetRemoteSessions();
            }
            else
            {
                WarnNotStarted("XRCUnity->GetRemoteSessions()");
            }

            return result;
        }

        public static bool JoinSession(string sessionID, UserType serializedUser)
        {
            bool result = false;

            if (XRCInterface.IsStarted)
            {
                if (!XRCInterface.IsSessionActive)
                {
                    IEntityParameters entityParameters = CreateEntityParameters(serializedUser, null);
                    if (entityParameters is IUserParameters)
                    {
                        // Join the session
                        result = JoinSession(sessionID, entityParameters as IUserParameters);
                    }
                    else
                    {
                        Debug.LogWarning(nameof(JoinSession) + " User parameters could not be created");
                    }
                }
                else
                {
                    WarnInSession("XRCUnity->JoinSession()");
                }
            }
            else
            {
                WarnNotStarted("XRCUnity->JoinSession()");
            }

            return result;
        }

        public static bool JoinSession(string sessionID, IUserParameters userParameters)
        {
            bool result = false;

            if (XRCInterface.IsStarted)
            {
                if (!XRCInterface.IsSessionActive)
                {
                    if (userParameters != null)
                    {
                        // Stage the user
                        if (StageEntity(userParameters))
                        {
                            // Add the user to XRC
                            string userId = userParameters.EntityID;
                            result = XRCInterface.JoinSession(sessionID, userId);
                        }
                        else
                        {
                            Debug.LogWarning(nameof(JoinSession) + " User parameters could not be staged");
                        }
                    }
                    else
                    {
                        Debug.LogWarning(nameof(JoinSession) + " Supplied user parameters are null");
                    }
                }
                else
                {
                    WarnInSession("XRCUnity->JoinSession()");
                }
            }
            else
            {
                WarnNotStarted("XRCUnity->JoinSession()");
            }

            return result;
        }


        public static bool CancelJoinSession()
        {
            bool result = false;

            if (XRCInterface.IsStarted)
            {
                result = XRCInterface.CancelJoinSession();
            }
            else
            {
                WarnNotStarted("XRCUnity->CancelJoinSession()");
            }

            return result;
        }

        public static bool LeaveSession()
        {
            bool result = false;

            if (XRCInterface.IsSessionActive)
            {
                if (!IsMaster)
                {
                    result = XRCInterface.LeaveSession();
                }
                else
                {
                    WarnMaster("XRCUnity->LeaveSession()");
                }
            }
            else
            {
                WarnNoSession("XRCUnity->LeaveSession()");
            }

            return result;
        }

        public static bool SessionEntityExists(IIdentifiable entity)
        {
            bool result = false;

            if (XRCInterface.IsSessionActive)
            {
                // Get the entity ID
                string entityId = GetXRCEntityID(entity);
                if (!string.IsNullOrEmpty(entityId))
                {
                    result = XRCInterface.SessionEntityExists(entityId);
                }
                else
                {
                    Debug.LogWarning(nameof(SessionEntityExists) + " Supplied entity is invalid");
                }
            }
            else
            {
                WarnNoSession("XRCUnity->SessionEntityExists()");
            }

            return result;
        }

        public static long GetSessionEntityCount()
        {
            long result = 0;

            if (XRCInterface.IsSessionActive)
            {
                result = XRCInterface.GetSessionEntityCount();
            }
            else
            {
                WarnNoSession("XRCUnity->GetSessionEntityCount()");
            }

            return result;
        }

        public static IEntityParameters GetSessionEntity(string entityId)
        {
            IEntityParameters result = null;

            if (XRCInterface.IsSessionActive)
            {
                if (XRCInterface.GetSessionEntity(entityId))
                {
                    // Get the staged entity parameters
                    result = GetStagedEntityParameters(entityId);
                }
            }
            else
            {
                WarnNoSession("XRCUnity->GetSessionEntity()");
            }

            return result;
        }

        public static IEntityParameters[] GetAllSessionEntities()
        {
            IEntityParameters[] result = null;

            if (XRCInterface.IsSessionActive)
            {
                // Clear all currently staged entities
                XRCInterface.StagedEntitiesClear();

                // Tell XRC to stage all entities (start with a fresh list)
                XRCInterface.GetSessionEntities();

                // Build a list of entity parameters from the staged entities
                List<IEntityParameters> sessionEntities = new List<IEntityParameters>();
                XRCInterface.GetStagedEntityIds(out string[] entityIds);
                foreach (string entityId in entityIds)
                {
                    // Load the entity parameters from the staged entity
                    IEntityParameters sessionEntity = GetStagedEntityParameters(entityId);
                    if (sessionEntity != null)
                    {
                        sessionEntities.Add(sessionEntity);
                    }
                }

                // Convert the list to the result array
                result = sessionEntities.ToArray();
            }
            else
            {
                WarnNoSession("XRCUnity->GetAllSessionEntities()");
            }

            return result;
        }

        public static bool AddSessionEntity(IdentifiableType serializedObject, IIdentifiable parent)
        {
            bool result = false;

            if (XRCInterface.IsSessionActive)
            {
                IEntityParameters entityParameters = CreateEntityParameters(serializedObject, parent);
                if (entityParameters != null)
                {
                    // Add the session entity
                    result = AddSessionEntity(entityParameters);
                }
                else
                {
                    Debug.LogWarning(nameof(AddSessionEntity) + " Entity parameters could not be created");
                }
            }
            else
            {
                WarnNoSession("XRCUnity->AddSessionEntity()");
            }

            return result;
        }

        public static bool AddSessionEntity(IEntityParameters entityParameters)
        {
            bool result = false;

            if (XRCInterface.IsSessionActive)
            {
                if (entityParameters != null)
                {
                    // Stage the entity
                    if (StageEntity(entityParameters))
                    {
                        // Add the entity to the XRC session
                        string entityId = entityParameters.EntityID;
                        result = XRCInterface.AddSessionEntity(entityId);
                    }
                    else
                    {
                        Debug.LogWarning(nameof(AddSessionEntity) + " Entity parameters could not be staged");
                    }
                }
                else
                {
                    Debug.LogWarning(nameof(AddSessionEntity) + " Supplied entity parameters are null");
                }
            }
            else
            {
                WarnNoSession("XRCUnity->AddSessionEntity()");
            }

            return result;
        }

        public static bool RemoveSessionEntity(IIdentifiable entity)
        {
            bool result = false;

            if (XRCInterface.IsSessionActive)
            {
                // Get the entity ID
                string entityId = GetXRCEntityID(entity);
                if (!string.IsNullOrEmpty(entityId))
                {
                    result = RemoveSessionEntity(entityId);
                }
                else
                {
                    Debug.LogWarning(nameof(RemoveSessionEntity) + " Entity does not exist");
                }
            }
            else
            {
                WarnNoSession("XRCUnity->RemoveSessionEntity()");
            }

            return result;
        }

        public static bool RemoveSessionEntity(string entityId)
        {
            bool result = false;

            if (XRCInterface.IsSessionActive)
            {
                // Remove the session entity
                result = XRCInterface.RemoveSessionEntity(entityId);

                // Clear it from our staged entities
                XRCInterface.StagedEntityDelete(entityId);
            }
            else
            {
                WarnNoSession("XRCUnity->RemoveSessionEntity()");
            }

            return result;
        }

        public static bool UpdateSessionEntityTransform(ISceneObject entity)
        {
            bool result = false;

            if (XRCInterface.IsSessionActive)
            {
                // Get the entity ID
                string entityId = GetXRCEntityID(entity);
                if (!string.IsNullOrEmpty(entityId))
                {
                    // Serialize the transform
                    TransformType serializedTransform = new TransformType();
                    SchemaUtil.SerializeTransform(serializedTransform, entity.gameObject);

                    // Update the entity transform with the serialized transform
                    result = UpdateSessionEntityTransform(entityId, serializedTransform);
                }
                else
                {
                    Debug.LogWarning(nameof(UpdateSessionEntityTransform) + " Entity does not exist");
                }
            }
            else
            {
                WarnNoSession("XRCUnity->UpdateSessionEntityTransform()");
            }

            return result;
        }

        public static bool UpdateSessionEntityTransform(string entityID, TransformType serializedTransform)
        {
            bool result = false;

            if (XRCInterface.IsSessionActive)
            {
                if (SerializedToXRC(entityID, serializedTransform))
                {
                    result = XRCInterface.UpdateSessionEntity(entityID);
                }
            }
            else
            {
                WarnNoSession("XRCUnity->UpdateSessionEntityTransform()");
            }

            return result;
        }

        public static bool UpdateSessionEntityPosition(ISceneObject entity)
        {
            bool result = false;

            if (XRCInterface.IsSessionActive)
            {
                // Get the entity ID
                string entityId = GetXRCEntityID(entity);
                if (!string.IsNullOrEmpty(entityId))
                {
                    // Serialize the position
                    TransformPositionType serializedTransformPosition = new TransformPositionType();
                    SchemaUtil.SerializeTransformPosition(serializedTransformPosition, entity.gameObject);

                    // Update the entity position with the serialized transform position
                    result = UpdateSessionEntityPosition(entityId, serializedTransformPosition);
                }
                else
                {
                    Debug.LogWarning(nameof(UpdateSessionEntityPosition) + " Entity does not exist");
                }
            }
            else
            {
                WarnNoSession("XRCUnity->UpdateSessionEntityPosition()");
            }

            return result;
        }

        public static bool UpdateSessionEntityPosition(string entityID, TransformPositionType serializedPosition)
        {
            bool result = false;

            if (XRCInterface.IsSessionActive)
            {
                if (SerializedToXRC(entityID, serializedPosition))
                {
                    result = XRCInterface.UpdateSessionEntity(entityID);
                }
            }
            else
            {
                WarnNoSession("XRCUnity->UpdateSessionEntityPosition()");
            }

            return result;
        }

        public static bool UpdateSessionEntityScale(ISceneObject entity)
        {
            bool result = false;

            if (XRCInterface.IsSessionActive)
            {
                // Get the entity ID
                string entityId = GetXRCEntityID(entity);
                if (!string.IsNullOrEmpty(entityId))
                {
                    // Serialize the scale
                    TransformScaleType serializedTransformScale = new TransformScaleType();
                    SchemaUtil.SerializeTransformScale(serializedTransformScale, entity.gameObject);

                    // Update the entity position with the serialized transform scale
                    result = UpdateSessionEntityScale(entityId, serializedTransformScale);
                }
                else
                {
                    Debug.LogWarning(nameof(UpdateSessionEntityScale) + " Entity does not exist");
                }
            }
            else
            {
                WarnNoSession("XRCUnity->UpdateSessionEntityScale()");
            }

            return result;
        }

        public static bool UpdateSessionEntityScale(string entityID, TransformScaleType serializedScale)
        {
            bool result = false;

            if (XRCInterface.IsSessionActive)
            {
                if (SerializedToXRC(entityID, serializedScale))
                {
                    result = XRCInterface.UpdateSessionEntity(entityID);
                }
            }
            else
            {
                WarnNoSession("XRCUnity->UpdateSessionEntityScale()");
            }

            return result;
        }

        public static bool UpdateSessionEntityRotation(ISceneObject entity)
        {
            bool result = false;

            if (XRCInterface.IsSessionActive)
            {
                // Get the entity ID
                string entityId = GetXRCEntityID(entity);
                if (!string.IsNullOrEmpty(entityId))
                {
                    // Serialize the scale
                    TransformRotationType serializedTransformRotation = new TransformQRotationType();
                    SchemaUtil.SerializeTransformRotation(serializedTransformRotation, entity.gameObject);

                    // Update the entity position with the serialized transform rotation
                    result = UpdateSessionEntityRotation(entityId, serializedTransformRotation);
                }
                else
                {
                    Debug.LogWarning(nameof(UpdateSessionEntityRotation) + " Entity does not exist");
                }
            }
            else
            {
                WarnNoSession("XRCUnity->UpdateSessionEntityRotation()");
            }

            return result;
        }

        public static bool UpdateSessionEntityRotation(string entityID, TransformRotationType serializedRotation)
        {
            bool result = false;

            if (XRCInterface.IsSessionActive)
            {
                if (SerializedToXRC(entityID, serializedRotation))
                {
                    result = XRCInterface.UpdateSessionEntity(entityID);
                }
            }
            else
            {
                WarnNoSession("XRCUnity->UpdateSessionEntityRotation()");
            }

            return result;
        }

        public static bool UpdateSessionEntityInteractions(IInteractable entity)
        {
            bool result = false;

            if (XRCInterface.IsSessionActive)
            {
                // Get the entity ID
                string entityId = GetXRCEntityID(entity);
                if (!string.IsNullOrEmpty(entityId))
                {
                    // Serialize the interactions
                    InteractionSettingsType serializedInteractions = new InteractionSettingsType();
                    SchemaUtil.SerializeInteractions(serializedInteractions, entity.Usable, entity.Grabbable);

                    // Update the entity interactions with the serialized interactions
                    result = UpdateSessionEntityInteractions(entityId, serializedInteractions);
                }
                else
                {
                    Debug.LogWarning(nameof(UpdateSessionEntityInteractions) + " Entity does not exist");
                }
            }
            else
            {
                WarnNoSession("XRCUnity->UpdateSessionEntityInteractions()");
            }

            return result;
        }

        public static bool UpdateSessionEntityInteractions(string entityId, InteractionSettingsType serializedInteractions)
        {
            bool result = false;

            if (XRCInterface.IsSessionActive)
            {
                // Key parmeters
                XRCInterface.GetEntityStringAttribute(entityId, KEY_CATEGORY, out string category);

                // Update the interactable settings
                string attribute = category + INTERACTABLE_INTERACTIONS;
                if (!SerializedToXRC(entityId, serializedInteractions, attribute))
                {
                    Debug.LogWarning(nameof(InteractableSceneObjectType) + " A problem ocurred updating the object interactions");
                    return false;
                }
                SetEntityBooleanAttribute(entityId, attribute, true);

                // Tell XRC to update
                result = XRCInterface.UpdateSessionEntity(entityId);
            }
            else
            {
                WarnNoSession("XRCUnity->UpdateSessionEntityInteractions()");
            }

            return result;
        }

        public static bool UpdateSessionEntityPhysics(IPhysicalSceneObject entity)
        {
            bool result = false;

            if (XRCInterface.IsSessionActive)
            {
                // Get the entity ID
                string entityId = GetXRCEntityID(entity);
                if (!string.IsNullOrEmpty(entityId))
                {
                    // Serialize the physics settings
                    PhysicsSettingsType serializedPhysics = new PhysicsSettingsType();
                    SchemaUtil.SerializePhysics(serializedPhysics, entity.EnableCollisions, entity.EnableGravity);

                    // Update the entity physics settings with the serialized settings
                    result = UpdateSessionEntityPhysics(entityId, serializedPhysics);
                }
                else
                {
                    Debug.LogWarning(nameof(UpdateSessionEntityPhysics) + " Entity does not exist");
                }
            }
            else
            {
                WarnNoSession("XRCUnity->UpdateSessionEntityPhysics()");
            }

            return result;
        }

        public static bool UpdateSessionEntityPhysics(string entityId, PhysicsSettingsType serializedPhysics)
        {
            bool result = false;

            if (XRCInterface.IsSessionActive)
            {
                // Key parmeters
                XRCInterface.GetEntityStringAttribute(entityId, KEY_CATEGORY, out string category);

                // Update the physics settings
                string attribute = category + PHYSICS_SETTINGS;
                if (!SerializedToXRC(entityId, serializedPhysics, attribute))
                {
                    Debug.LogWarning(nameof(PhysicalSceneObjectType) + " A problem ocurred updating the physics settings");
                    return false;
                }
                SetEntityBooleanAttribute(entityId, attribute, true);

                // Tell XRC to update
                result = XRCInterface.UpdateSessionEntity(entityId);
            }
            else
            {
                WarnNoSession("XRCUnity->UpdateSessionEntityPhysics()");
            }

            return result;
        }

        public static bool UpdateSessionEntity(IdentifiableType serializedObject, IIdentifiable parent)
        {
            bool result = false;

            if (XRCInterface.IsSessionActive)
            {
                IEntityParameters entityParameters = CreateEntityParameters(serializedObject, parent);
                if (entityParameters != null)
                {
                    // Update the session entity
                    result = UpdateSessionEntity(entityParameters);
                }
                else
                {
                    Debug.LogWarning(nameof(UpdateSessionEntity) + " Entity parameters could not be created");
                }
            }
            else
            {
                WarnNoSession("XRCUnity->UpdateSessionEntity()");
            }

            return result;
        }

        public static bool UpdateSessionEntity(IEntityParameters entityParameters)
        {
            bool result = false;

            if (XRCInterface.IsSessionActive)
            {
                if (entityParameters != null)
                {
                    // Stage the entity
                    if (StageEntity(entityParameters))
                    {
                        // Update the entity in the XRC session
                        string entityId = entityParameters.EntityID;
                        result = XRCInterface.UpdateSessionEntity(entityId);
                    }
                    else
                    {
                        Debug.LogWarning(nameof(UpdateSessionEntity) + " Entity parameters could not be staged");
                    }
                }
                else
                {
                    Debug.LogWarning(nameof(UpdateSessionEntity) + " Supplied entity parameters are null");
                }
            }
            else
            {
                WarnNoSession("XRCUnity->AddSessionEntity()");
            }

            return result;
        }

        public static bool UpdateSessionEntityParent(ISceneObject entity, IIdentifiable parent)
        {
            bool result = false;

            if (XRCInterface.IsSessionActive)
            {
                // Get the entity ID
                string entityId = GetXRCEntityID(entity);
                if (!string.IsNullOrEmpty(entityId))
                {
                    // Get the parent ID
                    string parentId = GetXRCEntityID(parent);

                    // Update the entity transform with the serialized transform
                    result = UpdateSessionEntityParent(entityId, parentId);
                }
                else
                {
                    Debug.LogWarning(nameof(UpdateSessionEntityTransform) + " Entity does not exist");
                }
            }
            else
            {
                WarnNoSession("XRCUnity->UpdateSessionEntityTransform()");
            }

            return result;
        }

        public static bool UpdateSessionEntityParent(string entityId, string parentId)
        {
            bool result = false;

            if (XRCInterface.IsSessionActive)
            {
                if (SetEntityStringAttribute(entityId, KEY_PARENTID, parentId))
                {
                    result = XRCInterface.UpdateSessionEntity(entityId);
                }
            }
            else
            {
                WarnNoSession("XRCUnity->UpdateSessionEntityParent()");
            }

            return result;
        }

        /// <seealso cref="MRETUpdateBehaviour.MRETUpdate"/>
        protected override void MRETUpdate()
        {
            base.MRETUpdate();

            int invoked = 0;
            while (invocationQueue.Count > 0 && invoked++ < maxInvokesPerFrame)
            {
                UnityEvent eventToPerform;
                if (!invocationQueue.TryDequeue(out eventToPerform)) return;

                if (eventToPerform != null)
                {
                    eventToPerform.Invoke();
                }
            }
        }

        /// <seealso cref="MRETBehaviour.MRETOnDestroy"/>
        protected override void MRETOnDestroy()
        {
            base.MRETOnDestroy();

            Destroy();
        }

        private static void WarnNotStarted(string location)
        {
            Debug.LogWarning(location + ": XRC Not Started");
        }

        private static void WarnAlreadyStarted(string location)
        {
            Debug.LogWarning(location + ": XRC Already Started");
        }

        private static void WarnMaster(string location)
        {
            Debug.LogWarning(location + ": Session master");
        }
        private static void WarnNotMaster(string location)
        {
            Debug.LogWarning(location + ": Not a session master");
        }

        private static void WarnNoSession(string location)
        {
            Debug.LogWarning(location + ": No Session Active");
        }

        private static void WarnInSession(string location)
        {
            Debug.LogWarning(location + ": Session Already Active");
        }
    }
}