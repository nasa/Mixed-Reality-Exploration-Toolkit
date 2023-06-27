// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Integrations.GMSEC;
using GOV.NASA.GSFC.XR.MRET.SceneObjects;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Part;

namespace GOV.NASA.GSFC.XR.MRET.Schema.v0_1
{
    public partial class UnityTransformType : IUpgradable<v0_9.TransformType>
    {
        // Always assume scale unless told otherwise
        [XmlIgnore]
        public bool UseSize = false;

        /// <seealso cref="IUpgradable.Upgrade"/>
        object IUpgradable.Upgrade() => Upgrade();

        /// <seealso cref="IUpgradable{T}.Upgrade"/>
        public v0_9.TransformType Upgrade()
        {
            v0_9.TransformType upgraded = new v0_9.TransformType();

            // Position
            upgraded.Position = null;
            if ((Position != null) &&
                !Mathf.Approximately(Position.X, 0f) ||
                !Mathf.Approximately(Position.Y, 0f) ||
                !Mathf.Approximately(Position.Z, 0f))
            {
                upgraded.Position = new v0_9.TransformPositionType();
                upgraded.Position.X = !Mathf.Approximately(Position.X, 0f) ? Position.X : upgraded.Position.X; // v0.9 has defaults
                upgraded.Position.Y = !Mathf.Approximately(Position.Y, 0f) ? Position.Y : upgraded.Position.Y; // v0.9 has defaults
                upgraded.Position.Z = !Mathf.Approximately(Position.Z, 0f) ? Position.Z : upgraded.Position.Z; // v0.9 has defaults
            }

            // Rotation
            upgraded.Item = null;
            if ((Rotation != null) &&
                Mathf.Approximately(Rotation.X, 0f) &&
                Mathf.Approximately(Rotation.Y, 0f) &&
                Mathf.Approximately(Rotation.Z, 0f) &&
                Mathf.Approximately(Rotation.W, 0f))
            {
                // Invalid quaternion definition (0,0,0,0). Change to no rotation.
                Rotation.W = 1f;
            }
            if ((Rotation != null) &&
                !Mathf.Approximately(Rotation.X, 0f) ||
                !Mathf.Approximately(Rotation.Y, 0f) ||
                !Mathf.Approximately(Rotation.Z, 0f) ||
                !Mathf.Approximately(Rotation.W, 1f))
            {
                v0_9.TransformQRotationType qRotation = new v0_9.TransformQRotationType();
                upgraded.Item = qRotation;
                qRotation.X = !Mathf.Approximately(Rotation.X, 0f) ? Rotation.X : qRotation.X; // v0.9 has defaults
                qRotation.Y = !Mathf.Approximately(Rotation.Y, 0f) ? Rotation.Y : qRotation.Y; // v0.9 has defaults
                qRotation.Z = !Mathf.Approximately(Rotation.Z, 0f) ? Rotation.Z : qRotation.Z; // v0.9 has defaults
                qRotation.W = !Mathf.Approximately(Rotation.W, 1f) ? Rotation.W : qRotation.W; // v0.9 has defaults
            }

            // Scale
            upgraded.Item1 = null;
            if (Scale != null)
            {
                // Defaults are different depending on size or scale
                if (UseSize)
                {
                    if (!Mathf.Approximately(Scale.X, 0f) ||
                        !Mathf.Approximately(Scale.Y, 0f) ||
                        !Mathf.Approximately(Scale.Z, 0f))
                    {
                        upgraded.Item1 = new v0_9.TransformSizeType();
                        upgraded.Item1.X = !Mathf.Approximately(Scale.X, 0f) ? Scale.X : upgraded.Item1.X; // v0.9 has defaults
                        upgraded.Item1.Y = !Mathf.Approximately(Scale.Y, 0f) ? Scale.Y : upgraded.Item1.Y; // v0.9 has defaults
                        upgraded.Item1.Z = !Mathf.Approximately(Scale.Z, 0f) ? Scale.Z : upgraded.Item1.Z; // v0.9 has defaults
                    }
                }
                else
                {
                    if (!Mathf.Approximately(Scale.X, 1f) ||
                        !Mathf.Approximately(Scale.Y, 1f) ||
                        !Mathf.Approximately(Scale.Z, 1f))
                    {
                        upgraded.Item1 = new v0_9.TransformScaleType();
                        upgraded.Item1.X = !Mathf.Approximately(Scale.X, 1f) ? Scale.X : upgraded.Item1.X; // v0.9 has defaults
                        upgraded.Item1.Y = !Mathf.Approximately(Scale.Y, 1f) ? Scale.Y : upgraded.Item1.Y; // v0.9 has defaults
                        upgraded.Item1.Z = !Mathf.Approximately(Scale.Z, 1f) ? Scale.Z : upgraded.Item1.Z; // v0.9 has defaults
                    }
                }
            }

            return upgraded;
        }

        public UnityTransformType() : this(null, null, null)
        {

        }

        public UnityTransformType(Vector3Type position, QuaternionType rotation, NonNegativeFloat3Type scale, bool useSize = false)
        {
            UseSize = useSize;
            Position = new Vector3Type()
            {
                X = (position != null) ? position.X : 0f,
                Y = (position != null) ? position.Y : 0f,
                Z = (position != null) ? position.Z : 0f,
            };
            Rotation = new QuaternionType()
            {
                X = (rotation != null) ? rotation.X : 0f,
                Y = (rotation != null) ? rotation.Y : 0f,
                Z = (rotation != null) ? rotation.Z : 0f,
                W = (rotation != null) ? rotation.W : 1f,
            };
            Scale = new NonNegativeFloat3Type()
            {
                X = (scale != null) ? scale.X : (useSize ? default : 1f),
                Y = (scale != null) ? scale.Y : (useSize ? default : 1f),
                Z = (scale != null) ? scale.Z : (useSize ? default : 1f),
            };
        }
    }

    #region Environment
    public partial class VirtualEnvironmentTypeSkybox : IUpgradable<v0_9.MaterialType>
    {
        /// <seealso cref="IUpgradable.Upgrade"/>
        object IUpgradable.Upgrade() => Upgrade();

        /// <seealso cref="IUpgradable{T}.Upgrade"/>
        public v0_9.MaterialType Upgrade()
        {
            v0_9.MaterialType upgraded = new v0_9.MaterialType();

            object upgradedSkybox = null;
            string materialName = MaterialName;
            if (!string.IsNullOrEmpty(materialName))
            {
                // Check if an asset bundle is specified
                if (string.IsNullOrEmpty(AssetBundle) ||
                    (AssetBundle.ToLower() == "none") ||
                    (AssetBundle.ToLower() == "null"))
                {
                    v0_9.MaterialFileType skyBoxMaterial = new v0_9.MaterialFileType();
                    skyBoxMaterial.format = v0_9.MaterialFormatType.Material;
                    skyBoxMaterial.Value = "Skyboxes/" + materialName;
                    upgradedSkybox = skyBoxMaterial;
                }
                else
                {
                    v0_9.AssetType skyBoxAsset = new v0_9.AssetType();
                    skyBoxAsset.AssetBundle = AssetBundle;
                    skyBoxAsset.AssetName = materialName;
                    upgradedSkybox = skyBoxAsset;
                }
            }
            upgraded.Items = new object[1] {upgradedSkybox};

            return upgraded;
        }
    }

    public partial class VirtualEnvironmentTypeClippingPlanes : IUpgradable<v0_9.EnvironmentTypeClippingPlane>
    {
        /// <seealso cref="IUpgradable.Upgrade"/>
        object IUpgradable.Upgrade() => Upgrade();

        /// <seealso cref="IUpgradable{T}.Upgrade"/>
        public v0_9.EnvironmentTypeClippingPlane Upgrade()
        {
            v0_9.EnvironmentTypeClippingPlane upgraded = new v0_9.EnvironmentTypeClippingPlane();

            if (Near != default)
            {
                upgraded.Near = Near;
            }
            if (Far != default)
            {
                upgraded.Far = Far;
            }

            return upgraded;
        }
    }

    public partial class LocomotionSettingsType : IUpgradable<v0_9.LocomotionSettingsType>
    {
        /// <seealso cref="IUpgradable.Upgrade"/>
        object IUpgradable.Upgrade() => Upgrade();

        /// <seealso cref="IUpgradable{T}.Upgrade"/>
        public v0_9.LocomotionSettingsType Upgrade()
        {
            v0_9.LocomotionSettingsType upgraded = new v0_9.LocomotionSettingsType();

            // Teleport
            upgraded.Teleport = new v0_9.LocomotionSettingsTypeTeleport();
            if (TeleportDistance != default)
            {
                upgraded.Teleport.MaxDistance = TeleportDistance;
                upgraded.Teleport.MaxDistanceSpecified = true;
            }
            else
            {
                upgraded.Teleport.MaxDistanceSpecified = false;
            }

            // Fly
            upgraded.Fly = new v0_9.LocomotionConstraintsType();
            if (FlySpeed != default)
            {
                upgraded.Fly.NormalMultiplier = FlySpeed;
                upgraded.Fly.NormalMultiplierSpecified = true;
            }
            else
            {
                upgraded.Fly.NormalMultiplierSpecified = false;
            }
            upgraded.Fly.FastMultiplierSpecified = false;
            upgraded.Fly.SlowMultiplierSpecified = false;

            // Navigation
            upgraded.Navigation = new v0_9.LocomotionConstraintsType();
            if (TouchpadSpeed != default)
            {
                upgraded.Navigation.NormalMultiplier = TouchpadSpeed;
                upgraded.Navigation.NormalMultiplierSpecified = true;
            }
            else
            {
                upgraded.Navigation.NormalMultiplierSpecified = false;
            }
            upgraded.Navigation.FastMultiplierSpecified = false;
            upgraded.Navigation.SlowMultiplierSpecified = false;

            // Armswing
            upgraded.Armswing = new v0_9.LocomotionConstraintsType();
            if (ArmswingSpeed != default)
            {
                upgraded.Armswing.NormalMultiplier = ArmswingSpeed;
                upgraded.Armswing.NormalMultiplierSpecified = true;
            }
            else
            {
                upgraded.Armswing.NormalMultiplierSpecified = false;
            }
            upgraded.Armswing.FastMultiplierSpecified = false;
            upgraded.Armswing.SlowMultiplierSpecified = false;

            return upgraded;
        }
    }

    public partial class VirtualEnvironmentType : IUpgradable<v0_9.EnvironmentType>
    {
        protected void UpgradeHierachy(v0_9.EnvironmentType toSerialized)
        {
            // VERSION
            toSerialized.version = ProjectFileSchema.SCHEMA_VERSION_v0_9;
        }

        /// <seealso cref="IUpgradable.Upgrade"/>
        object IUpgradable.Upgrade() => Upgrade();

        /// <seealso cref="IUpgradable{T}.Upgrade"/>
        public v0_9.EnvironmentType Upgrade()
        {
            v0_9.EnvironmentType upgraded = new v0_9.EnvironmentType();

            // Upgrade the hierarchy
            UpgradeHierachy(upgraded);

            // Asset (optional)
            upgraded.Asset = null;
            if (!string.IsNullOrEmpty(AssetBundle) && (!string.IsNullOrEmpty(Name)))
            {
                // Create the asset
                upgraded.Asset = new v0_9.AssetType
                {
                    AssetBundle = AssetBundle,
                    AssetName = Name
                };
            }

            // Skybox (required)
            upgraded.SkyBox = (Skybox != null) ? Skybox.Upgrade() : new v0_9.MaterialType();

            // Clipping Plane (optional)
            upgraded.ClippingPlane = (ClippingPlanes != null) ? ClippingPlanes.Upgrade() : null;

            // Locomotion (optional)
            upgraded.LocomotionSettings = (LocomotionSettings != null) ? LocomotionSettings.Upgrade() : null;

            return upgraded;
        }
    }
    #endregion Environment

    #region Content
    #region Interfaces
    public partial class ROSConnectionType : IUpgradable<v0_9.ROSInterfaceType>
    {
        protected void UpgradeHierachy(v0_9.ROSInterfaceType toSerialized)
        {
            // VERSION
            toSerialized.version = ProjectFileSchema.SCHEMA_VERSION_v0_9;

            // IDENTIFIABLE
            {
                // Name is required
                toSerialized.Name = nameof(v0_9.ROSInterfaceType).Replace("Type", "");

                // Set the ID
                toSerialized.ID = ProjectType.NameToId(toSerialized.Name);
            }

            // CLIENT INTERFACE
            {
                // Server/Port
                toSerialized.Server = null;
                toSerialized.Port = 0;
                int idx = ROSBridgeServerURL.LastIndexOf(':');
                if (idx != -1)
                {
                    toSerialized.Server = ROSBridgeServerURL.Substring(0, idx);
                    if (int.TryParse(ROSBridgeServerURL.Substring(idx + 1), out int port))
                    {
                        toSerialized.Port = port;
                    }
                }
                else
                {
                    toSerialized.Server = ROSBridgeServerURL;
                }

                // Timeout
                if (Timeout != default)
                {
                    toSerialized.ConnectionTimeoutFrequency = new v0_9.FrequencyType();
                    v0_9.SchemaUtil.SerializeFrequency(Timeout, toSerialized.ConnectionTimeoutFrequency);
                }
            }
        }

        /// <seealso cref="IUpgradable.Upgrade"/>
        object IUpgradable.Upgrade() => Upgrade();

        /// <seealso cref="IUpgradable{T}.Upgrade"/>
        public v0_9.ROSInterfaceType Upgrade()
        {
            v0_9.ROSInterfaceType upgraded = new v0_9.ROSInterfaceType();

            // Upgrade the hierarchy
            UpgradeHierachy(upgraded);

            // ROS Protocol (assumes list is the same in 0.9)
            if (Enum.TryParse(Protocol.ToString(), out v0_9.ROSProtocolType protocol))
            {
                upgraded.Protocol = protocol;
            }

            // ROS Serializer (assumes list is the same in 0.9)
            if (Enum.TryParse(Serializer.ToString(), out v0_9.ROSSerializerType serializer))
            {
                upgraded.Serializer = serializer;
            }

            // Subscriber topic
            if (!string.IsNullOrEmpty(JointStateSubscriberTopic))
            {
                upgraded.JointStateSubscriberTopic = JointStateSubscriberTopic;
            }

            // Publisher topic
            if (!string.IsNullOrEmpty(JointStatePublisherTopic))
            {
                upgraded.JointStatePublisherTopic = JointStatePublisherTopic;
            }

            // Robot description (required)
            upgraded.RobotDescription = UrdfPath;

            return upgraded;
        }
    }

    #region IoT
    public partial class IoTThingPayloadValueThresholdType : IUpgradable<v0_9.IoTThingPayloadValueThresholdType>
    {
        /// <seealso cref="IUpgradable.Upgrade"/>
        object IUpgradable.Upgrade() => Upgrade();

        /// <seealso cref="IUpgradable{T}.Upgrade"/>
        public v0_9.IoTThingPayloadValueThresholdType Upgrade()
        {
            v0_9.IoTThingPayloadValueThresholdType upgraded = new v0_9.IoTThingPayloadValueThresholdType();

            // Low
            upgraded.LowSpecified = (Low != default);
            upgraded.Low = Low;

            // High
            upgraded.HighSpecified = (High != default);
            upgraded.High = High;

            // Default Color (assumes list is the same in 0.9)
            upgraded.Item = v0_9.ColorPredefinedType.Black;
            if (Enum.TryParse(Color.ToString(), out v0_9.ColorPredefinedType defaultColor))
            {
                upgraded.Item = defaultColor;
            }

            return upgraded;
        }
    }

    public partial class IoTThingPayloadValueThresholdsType : IUpgradable<v0_9.IoTThingPayloadValueThresholdsType>
    {
        /// <seealso cref="IUpgradable.Upgrade"/>
        object IUpgradable.Upgrade() => Upgrade();

        /// <seealso cref="IUpgradable{T}.Upgrade"/>
        public v0_9.IoTThingPayloadValueThresholdsType Upgrade()
        {
            v0_9.IoTThingPayloadValueThresholdsType upgraded = new v0_9.IoTThingPayloadValueThresholdsType();

            // Default Color (assumes list is the same in 0.9)
            upgraded.Item = v0_9.ColorPredefinedType.Black;
            if (Enum.TryParse(Color.ToString(), out v0_9.ColorPredefinedType defaultColor))
            {
                upgraded.Item = defaultColor;
            }

            // Thresholds
            if ((Threshold != null) && (Threshold.Length > 0))
            {
                // Build our list of upgraded thresholds
                List<v0_9.IoTThingPayloadValueThresholdType> upgradedThresholds = new List<v0_9.IoTThingPayloadValueThresholdType>();
                foreach (IoTThingPayloadValueThresholdType threshold in Threshold)
                {
                    upgradedThresholds.Add(threshold.Upgrade());
                }

                // Save out the upgraded threshold
                upgraded.Threshold = upgradedThresholds.ToArray();
            }

            return upgraded;
        }
    }

    public partial class IoTThingPayloadValueType : IUpgradable<v0_9.IoTThingPayloadValueType>
    {
        /// <seealso cref="IUpgradable.Upgrade"/>
        object IUpgradable.Upgrade() => Upgrade();

        /// <seealso cref="IUpgradable{T}.Upgrade"/>
        public v0_9.IoTThingPayloadValueType Upgrade()
        {
            v0_9.IoTThingPayloadValueType upgraded = new v0_9.IoTThingPayloadValueType();

            // Value type (assumes list is the same in 0.9)
            if (Enum.TryParse(Type.ToString(), out v0_9.IoTThingPayloadValueTypeType valueType))
            {
                upgraded.Type = valueType;
            }

            // Thresholds
            if (Thresholds != null)
            {
                upgraded.Thresholds = Thresholds.Upgrade();
            }

            return upgraded;
        }
    }

    public partial class IoTThingPayloadPairType : IUpgradable<v0_9.IoTThingPayloadPairType>
    {
        /// <seealso cref="IUpgradable.Upgrade"/>
        object IUpgradable.Upgrade() => Upgrade();

        /// <seealso cref="IUpgradable{T}.Upgrade"/>
        public v0_9.IoTThingPayloadPairType Upgrade()
        {
            v0_9.IoTThingPayloadPairType upgraded = new v0_9.IoTThingPayloadPairType();

            // Key/Value
            upgraded.Key = Key;
            if (Value != null)
            {
                upgraded.Value = Value.Upgrade();
            }

            return upgraded;
        }
    }

    public partial class IoTThingPayloadPairsType : IUpgradable<v0_9.IoTThingPayloadPairsType>
    {
        /// <seealso cref="IUpgradable.Upgrade"/>
        object IUpgradable.Upgrade() => Upgrade();

        /// <seealso cref="IUpgradable{T}.Upgrade"/>
        public v0_9.IoTThingPayloadPairsType Upgrade()
        {
            v0_9.IoTThingPayloadPairsType upgraded = new v0_9.IoTThingPayloadPairsType();

            // Pairs
            if ((Pair != null) && (Pair.Length > 0))
            {
                // Build our list of upgraded pairs
                List<v0_9.IoTThingPayloadPairType> upgradedPairs = new List<v0_9.IoTThingPayloadPairType>();
                foreach (IoTThingPayloadPairType pair in Pair)
                {
                    upgradedPairs.Add(pair.Upgrade());
                }

                // Save out the upgraded pair
                upgraded.Pair = upgradedPairs.ToArray();
            }

            return upgraded;
        }
    }

    public partial class IoTThingPayloadType : IUpgradable<v0_9.IoTThingPayloadType>
    {
        /// <seealso cref="IUpgradable.Upgrade"/>
        object IUpgradable.Upgrade() => Upgrade();

        /// <seealso cref="IUpgradable{T}.Upgrade"/>
        public v0_9.IoTThingPayloadType Upgrade()
        {
            v0_9.IoTThingPayloadType upgraded = new v0_9.IoTThingPayloadType();

            if (Item is IoTThingPayloadValueType)
            {
                upgraded.Item = (Item as IoTThingPayloadValueType).Upgrade();
            }
            else if (Item is IoTThingPayloadPairsType)
            {
                upgraded.Item = (Item as IoTThingPayloadPairsType).Upgrade();
            }

            return upgraded;
        }
    }

    public partial class IoTThingType : IUpgradable<v0_9.IoTThingType>
    {
        protected void UpgradeHierachy(v0_9.IoTThingType toSerialized)
        {
            // VERSION
            toSerialized.version = ProjectFileSchema.SCHEMA_VERSION_v0_9;

            // IDENTIFIABLE
            {
                // Name is required
                toSerialized.Name = !string.IsNullOrEmpty(Name) ? Name : nameof(v0_9.IoTThingType).Replace("Type", "");

                // Set the ID
                toSerialized.ID = (ID != null) ? ID : ProjectType.NameToId(toSerialized.Name);

                // Set the Desription
                toSerialized.Description = Description;
            }
        }

        /// <seealso cref="IUpgradable.Upgrade"/>
        object IUpgradable.Upgrade() => Upgrade();

        /// <seealso cref="IUpgradable{T}.Upgrade"/>
        public v0_9.IoTThingType Upgrade()
        {
            v0_9.IoTThingType upgraded = new v0_9.IoTThingType();

            // Upgrade the hierarchy
            UpgradeHierachy(upgraded);

            // Payload
            if (Payload != null)
            {
                // Upgrade the payload
                upgraded.Payload = Payload.Upgrade();
            }

            return upgraded;
        }
    }

    public partial class IoTThingsType : IUpgradable<v0_9.IoTThingType[]>
    {
        /// <seealso cref="IUpgradable.Upgrade"/>
        object IUpgradable.Upgrade() => Upgrade();

        /// <seealso cref="IUpgradable{T}.Upgrade"/>
        public v0_9.IoTThingType[] Upgrade()
        {
            v0_9.IoTThingType[] upgraded = null;
            if (Thing != null)
            {
                upgraded = new v0_9.IoTThingType[Thing.Length];
                for (int i=0; i<Thing.Length; i++)
                {
                    upgraded[i] = Thing[i].Upgrade();
                }
            }

            return upgraded;
        }
    }

    public partial class IoTTopicType : IUpgradable<v0_9.IoTTopicType>
    {
        /// <seealso cref="IUpgradable.Upgrade"/>
        object IUpgradable.Upgrade() => Upgrade();

        /// <seealso cref="IUpgradable{T}.Upgrade"/>
        public v0_9.IoTTopicType Upgrade()
        {
            v0_9.IoTTopicType upgraded = new v0_9.IoTTopicType();

            upgraded.ThingID = !string.IsNullOrEmpty(ThingID) ? ThingID : ProjectType.NameToId(ThingID);
            upgraded.pattern = pattern;

            return upgraded;
        }
    }

    public partial class IoTConnectionType : IUpgradable<v0_9.IoTInterfaceType>
    {
        protected void UpgradeHierachy(v0_9.IoTInterfaceType toSerialized)
        {
            // VERSION
            toSerialized.version = ProjectFileSchema.SCHEMA_VERSION_v0_9;

            // IDENTIFIABLE
            {
                // Name is required
                toSerialized.Name = !string.IsNullOrEmpty(Name) ? Name : nameof(v0_9.IoTInterfaceType).Replace("Type", "");

                // Set the ID
                toSerialized.ID = ProjectType.NameToId(toSerialized.Name);

                // Set the Desription
                toSerialized.Description = Description;
            }

            // CLIENT INTERFACE
            {
                // Server/Port
                toSerialized.Server = Server;
                toSerialized.Port = Port;
            }
        }

        /// <seealso cref="IUpgradable.Upgrade"/>
        object IUpgradable.Upgrade() => Upgrade();

        /// <seealso cref="IUpgradable{T}.Upgrade"/>
        public v0_9.IoTInterfaceType Upgrade()
        {
            v0_9.IoTInterfaceType upgraded = new v0_9.IoTInterfaceType();

            // Upgrade the hierarchy
            UpgradeHierachy(upgraded);

            // Prepare the certificates structure
            upgraded.Certificates = new v0_9.ClientCertificatesType();

            // Server Certificate
            upgraded.Certificates.Item = Item;

            // Server Certificate element (assumes list is the same in 0.9)
            if (Enum.TryParse(ItemElementName.ToString(), out v0_9.ItemChoiceType itemElementName))
            {
                upgraded.Certificates.ItemElementName = itemElementName;
            }

            // Client Certificate
            upgraded.Certificates.Item1 = Item1;

            // Client Certificate element (assumes list is the same in 0.9)
            if (Enum.TryParse(Item1ElementName.ToString(), out v0_9.Item1ChoiceType item1ElementName))
            {
                upgraded.Certificates.Item1ElementName = item1ElementName;
            }

            // Topics
            upgraded.Topics = null;
            if ((Topics != null) && (Topics.Length > 0))
            {
                IoTTopicType defaultTopic = new IoTTopicType();
                if ((!string.IsNullOrEmpty(Topics[0].ThingID) || (Topics[0].pattern != defaultTopic.pattern)))
                {
                    List<v0_9.IoTTopicType> upgradedTopics = new List<v0_9.IoTTopicType>();
                    foreach (IoTTopicType serializedTopic in Topics)
                    {
                        // Add the upgraded topic to the list
                        upgradedTopics.Add(serializedTopic.Upgrade());
                    }

                    // Store the upgraded topics
                    upgraded.Topics = upgradedTopics.ToArray();
                }
            }

            return upgraded;
        }
    }

    public partial class IoTConnectionsType : IUpgradable<v0_9.IoTInterfaceType[]>
    {
        /// <seealso cref="IUpgradable.Upgrade"/>
        object IUpgradable.Upgrade() => Upgrade();

        /// <seealso cref="IUpgradable{T}.Upgrade"/>
        public v0_9.IoTInterfaceType[] Upgrade()
        {
            v0_9.IoTInterfaceType[] upgraded = null;
            if (IoTConnection != null)
            {
                upgraded = new v0_9.IoTInterfaceType[IoTConnection.Length];
                for (int i = 0; i < IoTConnection.Length; i++)
                {
                    upgraded[i] = IoTConnection[i].Upgrade();
                }
            }

            return upgraded;
        }
    }
    #endregion IoT

    #region GMSEC
    public partial class GMSECSourceType : IUpgradable<v0_9.GMSECInterfaceType>
    {
        protected void UpgradeHierachy(v0_9.GMSECInterfaceType toSerialized)
        {
            // VERSION
            toSerialized.version = ProjectFileSchema.SCHEMA_VERSION_v0_9;

            // XR
            {
                toSerialized.AREnabled = !AROnly;
                toSerialized.VREnabled = !VROnly;
            }

            // IDENTIFIABLE
            {
                // Name is required
                toSerialized.Name = nameof(v0_9.GMSECInterfaceType).Replace("Type", "");

                // Set the ID
                toSerialized.ID = ProjectType.NameToId(toSerialized.Name);
            }

            // CLIENT INTERFACE
            {
                // Server/Port
                toSerialized.Server = Server;
                toSerialized.Port = GMSECBusToDataManager.DEFAULT_PORT;

                // Connection Update Frequency
                // In 0.1, the read frequency was really the number of messages to skip, but in 0.9 we throttle on framerate.
                // We will assume that if not specified, max framerate is desired. Otherwise, just leave the value because
                // it doesn't necessarily map to framerate, but they clearly wanted a slower frequency
                int readFrequency = (ReadFrequency != default) ? ReadFrequency : int.MaxValue;

                // Convert the frequency
                toSerialized.ConnectionUpdateFrequency = new v0_9.FrequencyType();
                v0_9.SchemaUtil.SerializeFrequency(readFrequency, toSerialized.ConnectionUpdateFrequency);
            }
        }

        /// <seealso cref="IUpgradable.Upgrade"/>
        object IUpgradable.Upgrade() => Upgrade();

        /// <seealso cref="IUpgradable{T}.Upgrade"/>
        public v0_9.GMSECInterfaceType Upgrade()
        {
            v0_9.GMSECInterfaceType upgraded = new v0_9.GMSECInterfaceType();

            // Upgrade the hierarchy
            UpgradeHierachy(upgraded);

            // Connection type
            upgraded.Connection = v0_9.GMSECConnectionType.bolt;
            if (!string.IsNullOrEmpty(ConnectionType))
            {
                // String mapping
                switch (ConnectionType.Trim().ToLower())
                {
                    case "gmsec_activemq383":
                        upgraded.Connection = v0_9.GMSECConnectionType.amq383;
                        break;

                    case "gmsec_activemq384":
                        upgraded.Connection = v0_9.GMSECConnectionType.amq384;
                        break;

                    case "gmsec_mb":
                        upgraded.Connection = v0_9.GMSECConnectionType.mb;
                        break;

                    case "gmsec_websphere71":
                        upgraded.Connection = v0_9.GMSECConnectionType.ws71;
                        break;

                    case "gmsec_websphere75":
                        upgraded.Connection = v0_9.GMSECConnectionType.ws75;
                        break;

                    case "gmsec_websphere80":
                        upgraded.Connection = v0_9.GMSECConnectionType.ws80;
                        break;

                    case "gmsec_bolt":
                    default:
                        upgraded.Connection = v0_9.GMSECConnectionType.bolt;
                        break;
                }
            }

            // Subject
            upgraded.Subject = Subject;

            return upgraded;
        }
    }

    public partial class GMSECSourcesType : IUpgradable<v0_9.GMSECInterfaceType[]>
    {
        /// <seealso cref="IUpgradable.Upgrade"/>
        object IUpgradable.Upgrade() => Upgrade();

        /// <seealso cref="IUpgradable{T}.Upgrade"/>
        public v0_9.GMSECInterfaceType[] Upgrade()
        {
            v0_9.GMSECInterfaceType[] upgraded = null;
            if (GMSECSources != null)
            {
                upgraded = new v0_9.GMSECInterfaceType[GMSECSources.Length];
                for (int i = 0; i < GMSECSources.Length; i++)
                {
                    upgraded[i] = GMSECSources[i].Upgrade();
                }
            }

            return upgraded;
        }
    }
    #endregion GMSEC
    #endregion Interfaces

    #region Parts
    public partial class PartType : IUpgradable<v0_9.PartType>
    {
        // Part files always assume size unless told otherwise
        [XmlIgnore]
        public bool IsEnclosure = false;

        // Parts always assume part specifications unless told otherwise
        [XmlIgnore]
        public bool UsePartSpecs = true;

        protected void UpgradeHierachy(v0_9.PhysicalSceneObjectType toSerialized)
        {
            // VERSION
            toSerialized.version = PartFileSchema.SCHEMA_VERSION_v0_9;

            // XR
            {
                toSerialized.AREnabled = !AROnly;
                toSerialized.VREnabled = !VROnly;
            }

            // IDENTIFIABLE
            {
                toSerialized.UUID = GUID;

                // Name (required)
                toSerialized.Name = !string.IsNullOrEmpty(Name) ? Name : PartName;

                // Generate the ID from the name
                toSerialized.ID = ProjectType.NameToId(toSerialized.Name);

                // Description
                toSerialized.Description = Description;
            }

            // SCENE OBJECT
            {
                // Determine if we are using size or scale
                // For v0.1, no assetbundle = scale
                bool useSize = !IsEnclosure &&
                    (!string.IsNullOrEmpty(AssetBundle) &&
                    (AssetBundle.ToLower() != "none") &&
                    (AssetBundle.ToLower() != "null"));

                // Transform
                Vector3Type position = PartTransform?.Position;
                QuaternionType rotation = PartTransform?.Rotation;
                NonNegativeFloat3Type scale = PartTransform?.Scale;
                UnityTransformType partTransform = new UnityTransformType(position, rotation, scale, useSize);
                toSerialized.Transform = partTransform.Upgrade();
            }

            // INTERACTABLE
            {
                toSerialized.Interactions = null;
                if ((EnableInteraction != InteractableDefaults.INTERACTABLE) ||
                    (NonInteractable == InteractableDefaults.USABLE)) // <= Opposite
                {
                    // Interactions
                    toSerialized.Interactions = new v0_9.InteractionSettingsType();
                    toSerialized.Interactions.EnableInteraction = EnableInteraction;
                    toSerialized.Interactions.EnableUsability = !NonInteractable;
                }

                // Telemetry is defined by the ID containing the telemetry key in v0.1
                toSerialized.Telemetry = null;
                if (!string.IsNullOrEmpty(ID) && (ID.Contains("/")))
                {
                    // Telemetry settings
                    toSerialized.Telemetry = new v0_9.TelemetrySettingsType();
                    toSerialized.Telemetry.ShadeForLimitViolations = true;
                    toSerialized.Telemetry.TelemetryKey = new string[] { ID };
                }

            }

            // PHYSICAL OBJECT
            {
                // Model
                toSerialized.Model = null;
                if (!string.IsNullOrEmpty(AssetBundle) &&
                    (AssetBundle.ToLower() != "none") &&
                    (AssetBundle.ToLower() != "null"))
                {
                    toSerialized.Model = new v0_9.ModelType();
                    if (AssetBundle.ToLower() == "gltf")
                    {
                        v0_9.ModelFileType modelFile = new v0_9.ModelFileType();
                        modelFile.format = v0_9.ModelFormatType.GLTF;
                        modelFile.Value = (PartName == null) ? Name : PartName;
                        toSerialized.Model.Item = modelFile;
                    }
                    else
                    {
                        v0_9.AssetType modelAsset = new v0_9.AssetType();
                        modelAsset.AssetBundle = AssetBundle;
                        modelAsset.AssetName = (PartName == null) ? Name : PartName;
                        toSerialized.Model.Item = modelAsset;
                    }
                }

                // Physics
                toSerialized.Physics = null;
                if ((EnableCollisions != PhysicalSceneObjectDefaults.COLLISIONS_ENABLED) ||
                    (EnableGravity != PhysicalSceneObjectDefaults.GRAVITY_ENABLED))
                {
                    toSerialized.Physics = new v0_9.PhysicsSettingsType();
                    toSerialized.Physics.EnableCollisions = EnableCollisions;
                    toSerialized.Physics.EnableGravity = EnableGravity;
                    // TODO: Do we defer to the vendor specs for mass? If so, which mass value?
                }

                // Physical Specifications (optional)
                toSerialized.Specifications = null;
                if (!UsePartSpecs)
                {
                    // Specifications
                    if (!Mathf.Approximately(MinMass, PhysicalSceneObjectDefaults.MASS_MIN) ||
                        !Mathf.Approximately(MaxMass, PhysicalSceneObjectDefaults.MASS_MAX) ||
                        !Mathf.Approximately(MassContingency, PhysicalSceneObjectDefaults.MASS_CONTINGENCY) ||
                        !string.IsNullOrEmpty(Notes) || !string.IsNullOrEmpty(Reference))
                    {
                        // Make sure we have a structure for upgrading
                        toSerialized.Specifications = (toSerialized.Specifications == null)
                            ? new v0_9.PhysicalSpecificationsType()
                            : toSerialized.Specifications;

                        // Mass
                        toSerialized.Specifications.Mass = new v0_9.MassSpecificationsType();
                        if (!Mathf.Approximately(MinMass, PhysicalSceneObjectDefaults.MASS_MIN))
                        {
                            toSerialized.Specifications.Mass.Min = new v0_9.MassType();
                            v0_9.SchemaUtil.SerializeMass(MinMass, toSerialized.Specifications.Mass.Min);
                        }
                        if (!Mathf.Approximately(MaxMass, PhysicalSceneObjectDefaults.MASS_MAX))
                        {
                            toSerialized.Specifications.Mass.Max = new v0_9.MassType();
                            v0_9.SchemaUtil.SerializeMass(MaxMass, toSerialized.Specifications.Mass.Max);
                        }
                        if (!Mathf.Approximately(MassContingency, PhysicalSceneObjectDefaults.MASS_CONTINGENCY))
                        {
                            toSerialized.Specifications.Mass.Contingency = new v0_9.MassType();
                            v0_9.SchemaUtil.SerializeMass(MassContingency, toSerialized.Specifications.Mass.Contingency);
                        }

                        // Details
                        toSerialized.Specifications.Notes = !string.IsNullOrEmpty(Notes) ? Notes : PhysicalSceneObjectDefaults.NOTES;
                        toSerialized.Specifications.Reference = !string.IsNullOrEmpty(Reference) ? Reference : PhysicalSceneObjectDefaults.REFERENCE;
                    }
                }

                // Textures
                toSerialized.RandomizeTextures = RandomizeTexture;
            }
        }

        /// <seealso cref="IUpgradable.Upgrade"/>
        object IUpgradable.Upgrade() => Upgrade();

        /// <seealso cref="IUpgradable{T}.Upgrade"/>
        public v0_9.PartType Upgrade()
        {
            v0_9.PartType upgraded = new v0_9.PartType();

            // Upgrade the hierarchy
            UpgradeHierachy(upgraded);

            // Vendor Specifications (required)
            {
                upgraded.PartSpecifications = new v0_9.PartSpecificationsType();
                upgraded.PartSpecifications.Vendor = !string.IsNullOrEmpty(Vendor) ? Vendor : "Unknown";
                upgraded.PartSpecifications.Version = !string.IsNullOrEmpty(Version) ? Version : "Unknown";

                // Mass
                if (!Mathf.Approximately(MinMass, InteractablePartDefaults.MASS_MIN) ||
                    !Mathf.Approximately(MaxMass, InteractablePartDefaults.MASS_MAX) ||
                    !Mathf.Approximately(MassContingency, InteractablePartDefaults.MASS_CONTINGENCY))
                {
                    upgraded.PartSpecifications.Mass1 = new v0_9.MassSpecificationsType();
                    if (!Mathf.Approximately(MinMass, InteractablePartDefaults.MASS_MIN))
                    {
                        upgraded.PartSpecifications.Mass1.Min = new v0_9.MassType();
                        v0_9.SchemaUtil.SerializeMass(MinMass, upgraded.PartSpecifications.Mass1.Min);
                    }
                    if (!Mathf.Approximately(MaxMass, InteractablePartDefaults.MASS_MAX))
                    {
                        upgraded.PartSpecifications.Mass1.Max = new v0_9.MassType();
                        v0_9.SchemaUtil.SerializeMass(MaxMass, upgraded.PartSpecifications.Mass1.Max);
                    }
                    if (!Mathf.Approximately(MassContingency, InteractablePartDefaults.MASS_CONTINGENCY))
                    {
                        upgraded.PartSpecifications.Mass1.Contingency = new v0_9.MassType();
                        v0_9.SchemaUtil.SerializeMass(MassContingency, upgraded.PartSpecifications.Mass1.Contingency);
                    }
                }

                // Power
                if (!Mathf.Approximately(IdlePower, InteractablePartDefaults.POWER_IDLE) ||
                    !Mathf.Approximately(PeakPower, InteractablePartDefaults.POWER_MAX) ||
                    !Mathf.Approximately(AveragePower, InteractablePartDefaults.POWER_AVERAGE) ||
                    !Mathf.Approximately(PowerContingency, InteractablePartDefaults.POWER_CONTINGENCY))
                {
                    upgraded.PartSpecifications.Power = new v0_9.PowerSpecificationsType();
                    if (!Mathf.Approximately(IdlePower, InteractablePartDefaults.POWER_IDLE))
                    {
                        upgraded.PartSpecifications.Power.Idle = new v0_9.PowerType();
                        v0_9.SchemaUtil.SerializePower(IdlePower, upgraded.PartSpecifications.Power.Idle);
                    }
                    if (!Mathf.Approximately(PeakPower, InteractablePartDefaults.POWER_MAX))
                    {
                        upgraded.PartSpecifications.Power.Max = new v0_9.PowerType();
                        v0_9.SchemaUtil.SerializePower(PeakPower, upgraded.PartSpecifications.Power.Max);
                    }
                    if (!Mathf.Approximately(AveragePower, InteractablePartDefaults.POWER_AVERAGE))
                    {
                        upgraded.PartSpecifications.Power.Average = new v0_9.PowerType();
                        v0_9.SchemaUtil.SerializePower(AveragePower, upgraded.PartSpecifications.Power.Average);
                    }
                    if (!Mathf.Approximately(PowerContingency, InteractablePartDefaults.POWER_CONTINGENCY))
                    {
                        upgraded.PartSpecifications.Power.Contingency = new v0_9.PowerType();
                        v0_9.SchemaUtil.SerializePower(PowerContingency, upgraded.PartSpecifications.Power.Contingency);
                    }
                }

                // Details
                upgraded.PartSpecifications.Notes1 = !string.IsNullOrEmpty(Notes) ? Notes : InteractablePartDefaults.NOTES;
                upgraded.PartSpecifications.Reference1 = !string.IsNullOrEmpty(Reference) ? Reference : InteractablePartDefaults.REFERENCE;
            }

            // Category (assumes list is the same in 0.9)
            if (Enum.TryParse(PartType1.ToString(), out v0_9.PartCategoryType category))
            {
                upgraded.Category = category;
            }

            // Subsystem (Freeform string, so just try to parse and pray)
            if (!string.IsNullOrEmpty(Subsystem))
            {
                if (Enum.TryParse(Subsystem.ToString(), out v0_9.PartSubsystemType subsystem))
                {
                    upgraded.Subsystem = subsystem;
                }
            }

            // Child parts
            if (ChildParts != null)
            {
                List<v0_9.PartType> childParts = new List<v0_9.PartType>();
                if (ChildParts.Parts != null)
                {
                    foreach (PartType childPart in ChildParts.Parts)
                    {
                        childParts.Add(childPart.Upgrade());
                    }
                }

                // Make sure we have at least one child part
                if (childParts.Count > 0)
                {
                    upgraded.ChildParts = new v0_9.PartsType();
                    upgraded.ChildParts.Items = childParts.ToArray();

                    // Process the enclosure
                    if (Enclosure != null)
                    {
                        upgraded.ChildParts.Enclosure = new v0_9.EnclosureType();

                        // Upgrade the hierarchy
                        Enclosure.IsEnclosure = true;
                        Enclosure.UsePartSpecs = false; // Enclosures are not parts, so just use physical specs
                        Enclosure.UpgradeHierachy(upgraded.ChildParts.Enclosure);
                    }
                }
            }

            // ROS
            if (ROSConnection != null)
            {
                upgraded.ROSInterface = ROSConnection.Upgrade();
            }

            // TODO:
            // PartFileName
            // TelemetryTransforms
            // AttachToTransform
            // IsOnlyAttachment
            // StaticAttachment
            // AttachToName
            if (!string.IsNullOrEmpty(AttachToName))
            {
                // TODO: What is this used for in MRET?
            }

            return upgraded;
        }
    }

    public partial class PartsType : IUpgradable<v0_9.PartsType>
    {
        // The default identifier for the group
        [XmlIgnore]
        public int groupId = 0;

        protected void UpgradeHierachy(v0_9.PartsType toSerialized)
        {
            // VERSION
            toSerialized.version = PartFileSchema.SCHEMA_VERSION_v0_9;

            // GROUP
            toSerialized.group = groupId;
        }

        /// <seealso cref="IUpgradable.Upgrade"/>
        object IUpgradable.Upgrade() => Upgrade();

        /// <seealso cref="IUpgradable{T}.Upgrade"/>
        public v0_9.PartsType Upgrade()
        {
            v0_9.PartsType upgraded = new v0_9.PartsType();

            // Upgrade the hierarchy
            UpgradeHierachy(upgraded);

            List<v0_9.PartType> parts = new List<v0_9.PartType>();
            if (Parts != null)
            {
                foreach (PartType part in Parts)
                {
                    parts.Add(part.Upgrade());
                }
            }

            // There must be at least one part
            if (parts.Count > 0)
            {
                upgraded.Items = parts.ToArray();
            }
            else
            {
                upgraded = null;
            }

            return upgraded;
        }
    }
    #endregion Parts

    #region Drawings
    public partial class DrawingType : IUpgradable<v0_9.Drawing3dType>
    {
        protected void UpgradeHierachy(v0_9.DrawingType toSerialized)
        {
            // VERSION
            toSerialized.version = ProjectFileSchema.SCHEMA_VERSION_v0_9;

            // IDENTIFIABLE
            {
                toSerialized.UUID = GUID;

                // Name (required)
                toSerialized.Name = !string.IsNullOrEmpty(Name) ? Name : nameof(v0_9.Drawing3dType).Replace("Type", "");

                // Generate the ID from the name
                toSerialized.ID = ProjectType.NameToId(toSerialized.Name);
            }

            // DRAWING
            {
                if (Width != default)
                {
                    toSerialized.Width = new v0_9.LengthType();
                    v0_9.SchemaUtil.SerializeLength(Width, toSerialized.Width);
                }

                if (Points != null)
                {
                    toSerialized.Points = new v0_9.PointsType();
                    Vector3[] points = SchemaUtil.DeserializeVector3Array(Points);
                    v0_9.SchemaUtil.SerializePoints(points, toSerialized.Points);
                }
            }
        }

        /// <seealso cref="IUpgradable.Upgrade"/>
        object IUpgradable.Upgrade() => Upgrade();

        /// <seealso cref="IUpgradable{T}.Upgrade"/>
        public v0_9.Drawing3dType Upgrade()
        {
            v0_9.Drawing3dType upgraded = new v0_9.Drawing3dType();

            // Upgrade the hierarchy
            UpgradeHierachy(upgraded);

            // Render type
            if (!string.IsNullOrEmpty(RenderType))
            {
                upgraded.Type = (RenderType.ToLower() == "drawing" || RenderType == "measurement") ?
                    v0_9.DrawingRender3dType.Basic :
                    v0_9.DrawingRender3dType.Volumetric;
            }

            // Units
            switch (DesiredUnits)
            {
                case LineDrawingUnitsType.centimeters:
                    upgraded.Units = v0_9.LengthUnitType.Centimeter;
                    break;

                case LineDrawingUnitsType.millimeters:
                    upgraded.Units = v0_9.LengthUnitType.Millimeter;
                    break;

                case LineDrawingUnitsType.yards:
                    upgraded.Units = v0_9.LengthUnitType.Yard;
                    break;

                case LineDrawingUnitsType.feet:
                    upgraded.Units = v0_9.LengthUnitType.Foot;
                    break;

                case LineDrawingUnitsType.inches:
                    upgraded.Units = v0_9.LengthUnitType.Inch;
                    break;

                case LineDrawingUnitsType.meters:
                default:
                    upgraded.Units = v0_9.LengthUnitType.Meter;
                    break;
            }

            return upgraded;
        }
    }

    public partial class DrawingsType : IUpgradable<v0_9.Drawing3dType[]>
    {
        /// <seealso cref="IUpgradable.Upgrade"/>
        object IUpgradable.Upgrade() => Upgrade();

        /// <seealso cref="IUpgradable{T}.Upgrade"/>
        public v0_9.Drawing3dType[] Upgrade()
        {
            v0_9.Drawing3dType[] upgraded = null;

            List<v0_9.Drawing3dType> upgradedDrawings = new List<v0_9.Drawing3dType>();
            if (Drawings != null)
            {
                foreach (DrawingType drawing in Drawings)
                {
                    upgradedDrawings.Add(drawing.Upgrade());
                }
            }

            // There must be at least one drawing
            if (upgradedDrawings.Count > 0)
            {
                upgraded = upgradedDrawings.ToArray();
            }

            return upgraded;
        }
    }
    #endregion Drawings

    #region Notes
    public partial class NoteDrawingType : IUpgradable<v0_9.Drawing2dType>
    {
        protected void UpgradeHierachy(v0_9.Drawing2dType toSerialized)
        {
            // VERSION
            toSerialized.version = ProjectFileSchema.SCHEMA_VERSION_v0_9;

            // IDENTIFIABLE
            {
                toSerialized.UUID = GUID;

                // Name (required)
                toSerialized.Name = nameof(v0_9.Drawing2dType).Replace("Type", "");

                // Generate the ID from the name
                toSerialized.ID = ProjectType.NameToId(toSerialized.Name);
            }

            // DRAWING
            {
                if (Points != null)
                {
                    toSerialized.Points = new v0_9.PointsType();
                    Vector3[] points = SchemaUtil.DeserializeVector3Array(Points);
                    v0_9.SchemaUtil.SerializePoints(points, toSerialized.Points);
                }
            }
        }

        /// <seealso cref="IUpgradable.Upgrade"/>
        object IUpgradable.Upgrade() => Upgrade();

        /// <seealso cref="IUpgradable{T}.Upgrade"/>
        public v0_9.Drawing2dType Upgrade()
        {
            v0_9.Drawing2dType upgraded = new v0_9.Drawing2dType();

            // Upgrade the hierarchy
            UpgradeHierachy(upgraded);

            return upgraded;
        }
    }

    public partial class NoteDrawingsType : IUpgradable<v0_9.Drawing2dType[]>
    {
        /// <seealso cref="IUpgradable.Upgrade"/>
        object IUpgradable.Upgrade() => Upgrade();

        /// <seealso cref="IUpgradable{T}.Upgrade"/>
        public v0_9.Drawing2dType[] Upgrade()
        {
            v0_9.Drawing2dType[] upgraded = null;

            List<v0_9.Drawing2dType> upgradedDrawings = new List<v0_9.Drawing2dType>();
            if (NoteDrawings != null)
            {
                foreach (NoteDrawingType drawing in NoteDrawings)
                {
                    upgradedDrawings.Add(drawing.Upgrade());
                }
            }

            // There must be at least one drawing
            if (upgradedDrawings.Count > 0)
            {
                upgraded = upgradedDrawings.ToArray();
            }

            return upgraded;
        }
    }

    public partial class NoteType : IUpgradable<v0_9.NoteType>
    {
        protected void UpgradeHierachy(v0_9.NoteType toSerialized)
        {
            // VERSION
            toSerialized.version = ProjectFileSchema.SCHEMA_VERSION_v0_9;

            // XR
            {
                toSerialized.AREnabled = !AROnly;
                toSerialized.VREnabled = !VROnly;
            }

            // IDENTIFIABLE
            {
                toSerialized.UUID = GUID;

                // Name is required
                toSerialized.Name = nameof(v0_9.NoteType).Replace("Type", "");

                // Set the ID
                toSerialized.ID = ProjectType.NameToId(toSerialized.Name);
            }

            // SCENE OBJECT
            {
                // Transform
                Vector3Type position = Transform?.Position;
                QuaternionType rotation = Transform?.Rotation;
                NonNegativeFloat3Type scale = Transform?.Scale;
                UnityTransformType noteTransform = new UnityTransformType(position, rotation, scale);
                toSerialized.Transform = noteTransform.Upgrade();
            }

            // DISPLAY
            {
                // Parent
                if (!string.IsNullOrEmpty(ParentName))
                {
                    toSerialized.ParentID = ProjectType.NameToId(ParentName);
                }

                // Title
                if (!string.IsNullOrEmpty(Title))
                {
                    toSerialized.Title = Title;
                }

                // Width and Height (required)
                toSerialized.Width = 1;
                toSerialized.Height = 1;

                // Display state
                switch (State)
                {
                    case NoteTypeState.Maximized:
                        toSerialized.State = v0_9.DisplayStateType.Maximized;
                        break;

                    case NoteTypeState.Minimized:
                    default:
                        toSerialized.State = v0_9.DisplayStateType.Minimized;
                        break;
                }

                // Zorder (required)
                toSerialized.Zorder = 0;
            }
        }

        /// <seealso cref="IUpgradable.Upgrade"/>
        object IUpgradable.Upgrade() => Upgrade();

        /// <seealso cref="IUpgradable{T}.Upgrade"/>
        public v0_9.NoteType Upgrade()
        {
            v0_9.NoteType upgraded = new v0_9.NoteType();

            // Upgrade the hierarchy
            UpgradeHierachy(upgraded);

            // Content (required)
            upgraded.Content = !string.IsNullOrEmpty(Details) ? Details : "";

            // Drawings (optional)
            if (Drawings != null)
            {
                List<v0_9.Drawing2dType> upgradedDrawings = new List<v0_9.Drawing2dType>();
                if (Drawings.NoteDrawings != null)
                {
                    foreach (NoteDrawingType drawing in Drawings.NoteDrawings)
                    {
                        upgradedDrawings.Add(drawing.Upgrade());
                    }
                }

                // There must be at least one drawing
                if (upgradedDrawings.Count > 0)
                {
                    upgraded.Drawings = upgradedDrawings.ToArray();
                }
            }

            return upgraded;
        }
    }

    public partial class NotesType : IUpgradable<v0_9.NotesType>
    {
        public int groupId = 0;

        protected void UpgradeHierachy(v0_9.NotesType toSerialized)
        {
            // VERSION
            toSerialized.version = ProjectFileSchema.SCHEMA_VERSION_v0_9;

            // GROUP
            toSerialized.group = groupId;
        }

        /// <seealso cref="IUpgradable.Upgrade"/>
        object IUpgradable.Upgrade() => Upgrade();

        /// <seealso cref="IUpgradable{T}.Upgrade"/>
        public v0_9.NotesType Upgrade()
        {
            v0_9.NotesType upgraded = new v0_9.NotesType();

            // Upgrade the hierarchy
            UpgradeHierachy(upgraded);

            List<v0_9.NoteType> notes = new List<v0_9.NoteType>();
            if (Notes != null)
            {
                foreach (NoteType note in Notes)
                {
                    notes.Add(note.Upgrade());
                }
            }

            // There must be at least one note
            if (notes.Count > 0)
            {
                upgraded.Items = notes.ToArray();
            }
            else
            {
                upgraded = null;
            }

            return upgraded;
        }
    }
    #endregion Notes

    #region PointCloud
    public partial class StaticPointCloudType : IUpgradable<v0_9.PointCloudType>
    {
        protected void UpgradeHierachy(v0_9.PointCloudType toSerialized)
        {
            // VERSION
            toSerialized.version = ProjectFileSchema.SCHEMA_VERSION_v0_9;

            // XR
            {
                // Take the defaults
            }

            // IDENTIFIABLE
            {
                // Name is required
                toSerialized.Name = !string.IsNullOrEmpty(Name) ? Name : nameof(v0_9.NoteType).Replace("Type", "");

                // Set the ID
                toSerialized.ID = ProjectType.NameToId(toSerialized.Name);
            }

            // SCENE OBJECT
            {
                // Transform
                Vector3Type position = Position;
                QuaternionType rotation = Rotation;
                NonNegativeFloat3Type scale = Scale;
                UnityTransformType pointCloudTransform = new UnityTransformType(position, rotation, scale);
                toSerialized.Transform = pointCloudTransform.Upgrade();
            }

        }

        /// <seealso cref="IUpgradable.Upgrade"/>
        object IUpgradable.Upgrade() => Upgrade();

        /// <seealso cref="IUpgradable{T}.Upgrade"/>
        public v0_9.PointCloudType Upgrade()
        {
            v0_9.PointCloudType upgraded = new v0_9.PointCloudType();

            // Upgrade the hierarchy
            UpgradeHierachy(upgraded);

            // LOD (required)
            upgraded.LODLevel = LODIndex;

            // Source
            upgraded.Source = new v0_9.PointCloudSourceType();
            upgraded.Source.Item = new v0_9.PointCloudFileType
            {
                format = v0_9.PointCloudFormatType.Binary,
                Value = Path
            };

            return upgraded;
        }
    }

    public partial class StaticPointCloudsType : IUpgradable<v0_9.PointCloudsType>
    {
        public int groupId = 0;

        protected void UpgradeHierachy(v0_9.PointCloudsType toSerialized)
        {
            // VERSION
            toSerialized.version = ProjectFileSchema.SCHEMA_VERSION_v0_9;

            // GROUP
            toSerialized.group = groupId;
        }

        /// <seealso cref="IUpgradable.Upgrade"/>
        object IUpgradable.Upgrade() => Upgrade();

        /// <seealso cref="IUpgradable{T}.Upgrade"/>
        public v0_9.PointCloudsType Upgrade()
        {
            v0_9.PointCloudsType upgraded = new v0_9.PointCloudsType();

            // Upgrade the hierarchy
            UpgradeHierachy(upgraded);

            List<v0_9.PointCloudType> pointClouds = new List<v0_9.PointCloudType>();
            if (StaticPointClouds != null)
            {
                foreach (StaticPointCloudType pointCloud in StaticPointClouds)
                {
                    pointClouds.Add(pointCloud.Upgrade());
                }
            }

            // There must be at least one point cloud
            if (pointClouds.Count > 0)
            {
                upgraded.Items = pointClouds.ToArray();
            }
            else
            {
                upgraded = null;
            }

            return upgraded;
        }
    }
    #endregion PointCloud

    #region Time
    public partial class TimeType : IUpgradable<v0_9.TimeType>
    {
        /// <seealso cref="IUpgradable.Upgrade"/>
        object IUpgradable.Upgrade() => Upgrade();

        /// <seealso cref="IUpgradable{T}.Upgrade"/>
        public v0_9.TimeType Upgrade()
        {
            v0_9.TimeType upgraded = new v0_9.TimeType();

            upgraded.Items = new object[Items.Length];
            upgraded.ItemsElementName = new v0_9.ItemsChoiceType3[ItemsElementName.Length];

            // Transfer over each element
            for (int i = 0; i < Items.Length; i++)
            {
                try
                {
                    ItemsChoiceType itemsChoice = ItemsElementName[i];
                    switch (itemsChoice)
                    {
                        case ItemsChoiceType.TimeString:
                            TimeStringType timeString = (TimeStringType)Items[i];
                            v0_9.TimeStringType upgradedTimeString = new v0_9.TimeStringType();

                            // Copy over the fields
                            upgradedTimeString.format = timeString.format;
                            upgradedTimeString.Value = timeString.Value;

                            // Assign the new time string
                            upgraded.ItemsElementName[i] = v0_9.ItemsChoiceType3.TimeString;
                            upgraded.Items[i] = upgradedTimeString;
                            break;
                        case ItemsChoiceType.Year:
                            // Assign the year
                            upgraded.ItemsElementName[i] = v0_9.ItemsChoiceType3.Year;
                            upgraded.Items[i] = (int)Items[i];
                            break;
                        case ItemsChoiceType.Month:
                            // Assign the month
                            upgraded.ItemsElementName[i] = v0_9.ItemsChoiceType3.Month;
                            upgraded.Items[i] = (int)Items[i];
                            break;
                        case ItemsChoiceType.Day:
                            // Assign the day
                            upgraded.ItemsElementName[i] = v0_9.ItemsChoiceType3.Day;
                            upgraded.Items[i] = (int)Items[i];
                            break;
                        case ItemsChoiceType.Hour:
                            // Assign the hour
                            upgraded.ItemsElementName[i] = v0_9.ItemsChoiceType3.Hour;
                            upgraded.Items[i] = (int)Items[i];
                            break;
                        case ItemsChoiceType.Minute:
                            // Assign the minute
                            upgraded.ItemsElementName[i] = v0_9.ItemsChoiceType3.Minute;
                            upgraded.Items[i] = (int)Items[i];
                            break;
                        case ItemsChoiceType.Second:
                            // Assign the second
                            upgraded.ItemsElementName[i] = v0_9.ItemsChoiceType3.Second;
                            upgraded.Items[i] = (int)Items[i];
                            break;
                        case ItemsChoiceType.Millisecond:
                            // Assign the millisecond
                            upgraded.ItemsElementName[i] = v0_9.ItemsChoiceType3.Millisecond;
                            upgraded.Items[i] = (int)Items[i];
                            break;
                        case ItemsChoiceType.TimeZone:
                            // Assign the timezone
                            upgraded.ItemsElementName[i] = v0_9.ItemsChoiceType3.TimeZone;
                            upgraded.Items[i] = (string)Items[i];
                            break;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning("There was an issue during the upgrade of the time: " + e.Message);
                }
            }

            return upgraded;
        }
    }

    public partial class TimeSimulationType : IUpgradable<v0_9.TimeSimulationType>
    {
        protected void UpgradeHierachy(v0_9.TimeSimulationType toSerialized)
        {
            // VERSION
            toSerialized.version = ProjectFileSchema.SCHEMA_VERSION_v0_9;
        }

        /// <seealso cref="IUpgradable.Upgrade"/>
        object IUpgradable.Upgrade() => Upgrade();

        /// <seealso cref="IUpgradable{T}.Upgrade"/>
        public v0_9.TimeSimulationType Upgrade()
        {
            v0_9.TimeSimulationType upgraded = new v0_9.TimeSimulationType();

            // Upgrade the hierarchy
            UpgradeHierachy(upgraded);

            // Name/Description
            upgraded.Name = Name;
            upgraded.Description = Description;

            // Start Time
            upgraded.StartTime = null;
            if ((StartTime != null) && (StartTime.Items != null) && (StartTime.ItemsElementName != null) &&
                (StartTime.Items.Length == StartTime.ItemsElementName.Length))
            {
                upgraded.StartTime = StartTime.Upgrade();
            }

            // End Time
            upgraded.EndTime = null;

            // Update Rate
            upgraded.UpdateRate = UpdateRate;

            // State
            upgraded.Paused = Paused;

            return upgraded;
        }
    }
    #endregion Time
    #endregion Content

    public partial class ProjectType : IUpgradable<v0_9.ProjectType>
    {
        /// <summary>
        /// Convenience function that converts a name to a v0.9 id recommended format
        /// </summary>
        /// <param name="name">The name t use as a bassis for the resultant id</param>
        /// <returns>A v0.9 recommended format id</returns>
        public static string NameToId(string name)
        {
            return MRET.UuidRegistry.CreateUniqueIDFromName(name);
        }

        protected void UpgradeHierachy(v0_9.ProjectType toSerialized)
        {
            // VERSION
            toSerialized.version = ProjectFileSchema.SCHEMA_VERSION_v0_9;
        }

        /// <seealso cref="IUpgradable.Upgrade"/>
        object IUpgradable.Upgrade() => Upgrade();

        /// <seealso cref="IUpgradable{T}.Upgrade"/>
        public v0_9.ProjectType Upgrade()
        {
            v0_9.ProjectType upgraded = new v0_9.ProjectType();

            // Upgrade the hierarchy
            UpgradeHierachy(upgraded);

            // Create an interface container for interface lists
            List<v0_9.InterfaceType> upgradedInterfaces = new List<v0_9.InterfaceType>();

            // Perform the upgrades
            VirtualEnvironmentType environment = null;
            for (int i = 0; i < Items.Length; i++)
            {
                switch (ItemsElementName[i])
                {
                    case ItemsChoiceType3.Description:
                        upgraded.Description = Items[i] as string;
                        break;

                    case ItemsChoiceType3.Environment:
                        environment = Items[i] as VirtualEnvironmentType;
                        upgraded.Environment = environment.Upgrade();
                        break;

                    case ItemsChoiceType3.AnimationPanels:
                        // TODO:
                        break;

                    case ItemsChoiceType3.CurrentView:
                        // TODO:
                        break;

                    case ItemsChoiceType3.Parts:
                        // Upgrade, but it could return null if no parts specified
                        PartsType v0_1parts = (Items[i] as PartsType);

                        // Perform the upgrade
                        v0_9.PartsType parts = v0_1parts.Upgrade();
                        if (parts != null)
                        {
                            // Make sure we have a content container defined
                            if (upgraded.Content == null)
                            {
                                upgraded.Content = new v0_9.ContentType();
                            }
                            upgraded.Content.Parts = parts;
                        }
                        break;

                    case ItemsChoiceType3.Drawings:
                        // Upgrade, but it could return null if no drawings specified
                        v0_9.Drawing3dType[] drawings = (Items[i] as DrawingsType).Upgrade();
                        if ((drawings != null) && (drawings.Length > 0))
                        {
                            // Make sure we have a content container defined
                            if (upgraded.Content == null)
                            {
                                upgraded.Content = new v0_9.ContentType();
                            }
                            upgraded.Content.Drawings = drawings;
                        }
                        break;

                    case ItemsChoiceType3.GMSECSources:
                        // Upgrade, but it could return null if no GMSEC sources specified
                        v0_9.GMSECInterfaceType[] gmsecInterfaces = (Items[i] as GMSECSourcesType).Upgrade();
                        if (gmsecInterfaces != null)
                        {
                            // Add the GMSEC interfaces to our interface list
                            foreach (v0_9.GMSECInterfaceType gmsecInterface in gmsecInterfaces)
                            {
                                upgradedInterfaces.Add(gmsecInterface);
                            }
                        }
                        break;

                    case ItemsChoiceType3.MatlabConnection:
                        // TODO:
                        break;
                    case ItemsChoiceType3.IoTThings:
                        // Upgrade, but it could return null if no IoTThings specified
                        v0_9.IoTThingType[] iotThings = (Items[i] as IoTThingsType).Upgrade();
                        if ((iotThings != null) && (iotThings.Length > 0))
                        {
                            // Make sure we have a content container defined
                            if (upgraded.Content == null)
                            {
                                upgraded.Content = new v0_9.ContentType();
                            }
                            upgraded.Content.IoTThings = iotThings;
                        }
                        break;
                    case ItemsChoiceType3.IoTConnections:
                        // Upgrade, but it could return null if no IoT connections specified
                        v0_9.IoTInterfaceType[] iotConnections = (Items[i] as IoTConnectionsType).Upgrade();
                        if (iotConnections != null)
                        {
                            // Add the IoT interfaces to our interface list
                            foreach (v0_9.IoTInterfaceType iotConnection in iotConnections)
                            {
                                upgradedInterfaces.Add(iotConnection);
                            }
                        }
                        break;
                    case ItemsChoiceType3.Notes:
                        v0_9.NotesType notes = (Items[i] as NotesType).Upgrade();
                        if (notes != null)
                        {
                            // Make sure we have a content container defined
                            if (upgraded.Content == null)
                            {
                                upgraded.Content = new v0_9.ContentType();
                            }

                            // Make sure we have a displays container defined
                            if (upgraded.Content.Displays == null)
                            {
                                upgraded.Content.Displays = new v0_9.DisplaysType();
                            }

                            // Make sure we have a notes container defined
                            if (upgraded.Content.Displays.Notes == null)
                            {
                                upgraded.Content.Displays.Notes = new v0_9.NotesType();
                            }

                            upgraded.Content.Displays.Notes = notes;
                        }
                        break;

                    case ItemsChoiceType3.PointClouds:
                        v0_9.PointCloudsType pointClouds = (Items[i] as StaticPointCloudsType).Upgrade();
                        if (pointClouds != null)
                        {
                            // Make sure we have a content container defined
                            if (upgraded.Content == null)
                            {
                                upgraded.Content = new v0_9.ContentType();
                            }

                            // Make sure we have a point clouds container defined
                            if (upgraded.Content.PointClouds == null)
                            {
                                upgraded.Content.PointClouds = new v0_9.PointCloudsType();
                            }

                            upgraded.Content.PointClouds = pointClouds;
                        }
                        break;

                    case ItemsChoiceType3.ObjectGenerators:
                        // TODO:
                        break;

                    case ItemsChoiceType3.VDEConnection:
                        // TODO:
                        break;

                    default:
                        Debug.LogWarning("Unrecognized schema type: " + ItemsElementName[i]);
                        break;
                }
            }

            // Make sure required project fields are initialized

            // Environment (required)
            if (upgraded.Environment == null)
            {
                if (environment == null)
                {
                    environment = new VirtualEnvironmentType();
                }
                upgraded.Environment = environment.Upgrade();
            }

            // UserTransform (optional). New in v0.9. Was in environment in v0.1.
            if ((environment != null) && (environment.DefaultUserTransform != null))
            {
                Vector3Type position = environment.DefaultUserTransform?.Position;
                QuaternionType rotation = environment.DefaultUserTransform?.Rotation;
                NonNegativeFloat3Type scale = environment.DefaultUserTransform?.Scale;
                UnityTransformType userTransform = new UnityTransformType(position, rotation, scale);
                upgraded.UserTransform = userTransform.Upgrade();
            }

            // Interfaces
            if (upgradedInterfaces.Count > 0)
            {
                upgraded.Interfaces = upgradedInterfaces.ToArray();
            }

            // Description (optional) may be in the environment, so let's check
            if (string.IsNullOrEmpty(upgraded.Description) && !string.IsNullOrEmpty(environment.Description))
            {
                upgraded.Description = environment.Description;
            }

            return upgraded;
        }
    }
}
