﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System.Xml.Serialization;

// 
// This source code was auto-generated by xsd, Version=4.6.1590.0.
// 

namespace GSFC.ARVR.MRET.Common.Schemas
{

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1590.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.example.org/MRET")]
    [System.Xml.Serialization.XmlRootAttribute("Part", Namespace = "http://www.example.org/MRET", IsNullable = false)]
    public partial class PartType
    {

        private string[] vendorField;

        private string[] descriptionField;

        private string assetBundleField;

        private string[] partNameField;

        private UnityTransformType partTransformField;

        private bool[] enableCollisionsField;

        private bool[] enableGravityField;

        private bool[] enableInteractionField;

        private bool[] randomizeTexturesField;

        private float[] minMassField;

        private float[] maxMassField;

        private float[] massContingencyField;

        private PartTypePartType[] partType1Field;

        private string[] partFileNameField;

        private PartsType childPartsField;

        private PartType enclosureField;

        private string notesField;

        private string referenceField;

        private string subsystemField;

        private float idlePowerField;

        private float averagePowerField;

        private float peakPowerField;

        private float powerContingencyField;

        private bool randomizeTextureField;

        private TelemetryTransformsType telemetryTransformsField;

        private string gUIDField;

        private bool aROnlyField;

        private bool vROnlyField;

        private string attachToNameField;

        private UnityTransformType attachToTransformField;

        private bool isOnlyAttachmentField;

        private bool staticAttachmentField;

        private bool nonInteractableField;

        private string nameField;

        private string idField;

        private string versionField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Vendor")]
        public string[] Vendor
        {
            get
            {
                return this.vendorField;
            }
            set
            {
                this.vendorField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Description")]
        public string[] Description
        {
            get
            {
                return this.descriptionField;
            }
            set
            {
                this.descriptionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("AssetBundle")]
        public string AssetBundle
        {
            get
            {
                return this.assetBundleField;
            }
            set
            {
                this.assetBundleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("PartName")]
        public string[] PartName
        {
            get
            {
                return this.partNameField;
            }
            set
            {
                this.partNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("PartTransform")]
        public UnityTransformType PartTransform
        {
            get
            {
                return this.partTransformField;
            }
            set
            {
                this.partTransformField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("EnableCollisions")]
        public bool[] EnableCollisions
        {
            get
            {
                return this.enableCollisionsField;
            }
            set
            {
                this.enableCollisionsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("EnableGravity")]
        public bool[] EnableGravity
        {
            get
            {
                return this.enableGravityField;
            }
            set
            {
                this.enableGravityField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("EnableInteraction")]
        public bool[] EnableInteraction
        {
            get
            {
                return this.enableInteractionField;
            }
            set
            {
                this.enableInteractionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("RandomizeTextures")]
        public bool[] RandomizeTextures
        {
            get
            {
                return this.randomizeTexturesField;
            }
            set
            {
                this.randomizeTexturesField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("MinMass")]
        public float[] MinMass
        {
            get
            {
                return this.minMassField;
            }
            set
            {
                this.minMassField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("MaxMass")]
        public float[] MaxMass
        {
            get
            {
                return this.maxMassField;
            }
            set
            {
                this.maxMassField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("MassContingency")]
        public float[] MassContingency
        {
            get
            {
                return this.massContingencyField;
            }
            set
            {
                this.massContingencyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("PartType")]
        public PartTypePartType[] PartType1
        {
            get
            {
                return this.partType1Field;
            }
            set
            {
                this.partType1Field = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("PartFileName")]
        public string[] PartFileName
        {
            get
            {
                return this.partFileNameField;
            }
            set
            {
                this.partFileNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ChildParts")]
        public PartsType ChildParts
        {
            get
            {
                return this.childPartsField;
            }
            set
            {
                this.childPartsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Enclosure")]
        public PartType Enclosure
        {
            get
            {
                return this.enclosureField;
            }
            set
            {
                this.enclosureField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Notes")]
        public string Notes
        {
            get
            {
                return this.notesField;
            }
            set
            {
                this.notesField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Reference")]
        public string Reference
        {
            get
            {
                return this.referenceField;
            }
            set
            {
                this.referenceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Subsystem")]
        public string Subsystem
        {
            get
            {
                return this.subsystemField;
            }
            set
            {
                this.subsystemField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("IdlePower")]
        public float IdlePower
        {
            get
            {
                return this.idlePowerField;
            }
            set
            {
                this.idlePowerField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("AveragePower")]
        public float AveragePower
        {
            get
            {
                return this.averagePowerField;
            }
            set
            {
                this.averagePowerField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("PeakPower")]
        public float PeakPower
        {
            get
            {
                return this.peakPowerField;
            }
            set
            {
                this.peakPowerField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("PowerContingency")]
        public float PowerContingency
        {
            get
            {
                return this.powerContingencyField;
            }
            set
            {
                this.powerContingencyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("RandomizeTexture")]
        public bool RandomizeTexture
        {
            get
            {
                return this.randomizeTextureField;
            }
            set
            {
                this.randomizeTextureField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("TelemetryTransforms")]
        public TelemetryTransformsType TelemetryTransforms
        {
            get
            {
                return this.telemetryTransformsField;
            }
            set
            {
                this.telemetryTransformsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("GUID")]
        public string GUID
        {
            get
            {
                return this.gUIDField;
            }
            set
            {
                this.gUIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("AR-Only")]
        public bool AROnly
        {
            get
            {
                return this.aROnlyField;
            }
            set
            {
                this.aROnlyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("VR-Only")]
        public bool VROnly
        {
            get
            {
                return this.vROnlyField;
            }
            set
            {
                this.vROnlyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("AttachToName")]
        public string AttachToName
        {
            get
            {
                return this.attachToNameField;
            }
            set
            {
                this.attachToNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("AttachToTransform")]
        public UnityTransformType AttachToTransform
        {
            get
            {
                return this.attachToTransformField;
            }
            set
            {
                this.attachToTransformField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("IsOnlyAttachment")]
        public bool IsOnlyAttachment
        {
            get
            {
                return this.isOnlyAttachmentField;
            }
            set
            {
                this.isOnlyAttachmentField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("StaticAttachment")]
        public bool StaticAttachment
        {
            get
            {
                return this.staticAttachmentField;
            }
            set
            {
                this.staticAttachmentField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("NonInteractable")]
        public bool NonInteractable
        {
            get
            {
                return this.nonInteractableField;
            }
            set
            {
                this.nonInteractableField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "NCName")]
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string ID
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Version
        {
            get
            {
                return this.versionField;
            }
            set
            {
                this.versionField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1590.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.example.org/MRET")]
    public partial class UnityTransformType
    {

        private Vector3Type positionField;

        private QuaternionType rotationField;

        private Vector3Type scaleField;

        /// <remarks/>
        public Vector3Type Position
        {
            get
            {
                return this.positionField;
            }
            set
            {
                this.positionField = value;
            }
        }

        /// <remarks/>
        public QuaternionType Rotation
        {
            get
            {
                return this.rotationField;
            }
            set
            {
                this.rotationField = value;
            }
        }

        /// <remarks/>
        public Vector3Type Scale
        {
            get
            {
                return this.scaleField;
            }
            set
            {
                this.scaleField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1590.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.example.org/MRET")]
    public partial class Vector3Type
    {

        private float xField;

        private float yField;

        private float zField;

        /// <remarks/>
        public float X
        {
            get
            {
                return this.xField;
            }
            set
            {
                this.xField = value;
            }
        }

        /// <remarks/>
        public float Y
        {
            get
            {
                return this.yField;
            }
            set
            {
                this.yField = value;
            }
        }

        /// <remarks/>
        public float Z
        {
            get
            {
                return this.zField;
            }
            set
            {
                this.zField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1590.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.example.org/MRET")]
    public partial class NonNegativeFloat3Type : Vector3Type
    {
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1590.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.example.org/MRET")]
    public partial class Percent3Type : Vector3Type
    {
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1590.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.example.org/MRET")]
    public partial class Degree3Type : Vector3Type
    {
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1590.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.example.org/MRET")]
    public partial class AssetType
    {
        private string nameField;

        private string assetBundleField;

        /// <remarks/>
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        public string AssetBundle
        {
            get
            {
                return this.assetBundleField;
            }
            set
            {
                this.assetBundleField = value;
            }
        }

    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1590.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.example.org/MRET")]
    public enum DistributionFunctionType
    {
        /// <remarks/>
        Linear,

        /// <remarks/>
        Normal,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1590.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.example.org/MRET")]
    public partial class TelemetryTransformPointType
    {

        private string nameField;

        private bool useValueField;

        private bool invertField;

        private float offsetField;

        /// <remarks/>
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        public bool UseValue
        {
            get
            {
                return this.useValueField;
            }
            set
            {
                this.useValueField = value;
            }
        }

        /// <remarks/>
        public bool Invert
        {
            get
            {
                return this.invertField;
            }
            set
            {
                this.invertField = value;
            }
        }

        /// <remarks/>
        public float Offset
        {
            get
            {
                return this.offsetField;
            }
            set
            {
                this.offsetField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1590.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.example.org/MRET")]
    public partial class TelemetryTransformType
    {

        private TelemetryTransformAttributeType controlledAttributeField;

        private TelemetryTransformPointType xPointInfoField;

        private TelemetryTransformPointType yPointInfoField;

        private TelemetryTransformPointType zPointInfoField;

        private TelemetryTransformPointType wPointInfoField;

        private bool radiansField;

        private bool quaternionsField;

        private int frequencyField;

        /// <remarks/>
        public TelemetryTransformAttributeType ControlledAttribute
        {
            get
            {
                return this.controlledAttributeField;
            }
            set
            {
                this.controlledAttributeField = value;
            }
        }

        /// <remarks/>
        public TelemetryTransformPointType XPointInfo
        {
            get
            {
                return this.xPointInfoField;
            }
            set
            {
                this.xPointInfoField = value;
            }
        }

        /// <remarks/>
        public TelemetryTransformPointType YPointInfo
        {
            get
            {
                return this.yPointInfoField;
            }
            set
            {
                this.yPointInfoField = value;
            }
        }

        /// <remarks/>
        public TelemetryTransformPointType ZPointInfo
        {
            get
            {
                return this.zPointInfoField;
            }
            set
            {
                this.zPointInfoField = value;
            }
        }

        /// <remarks/>
        public TelemetryTransformPointType WPointInfo
        {
            get
            {
                return this.wPointInfoField;
            }
            set
            {
                this.wPointInfoField = value;
            }
        }

        /// <remarks/>
        public bool Radians
        {
            get
            {
                return this.radiansField;
            }
            set
            {
                this.radiansField = value;
            }
        }

        /// <remarks/>
        public bool Quaternions
        {
            get
            {
                return this.quaternionsField;
            }
            set
            {
                this.quaternionsField = value;
            }
        }

        /// <remarks/>
        public int Frequency
        {
            get
            {
                return this.frequencyField;
            }
            set
            {
                this.frequencyField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1590.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.example.org/MRET")]
    public enum TelemetryTransformAttributeType
    {

        /// <remarks/>
        None,

        /// <remarks/>
        GlobalPosition,

        /// <remarks/>
        LocalPosition,

        /// <remarks/>
        GlobalRotation,

        /// <remarks/>
        LocalRotation,

        /// <remarks/>
        Scale,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1590.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.example.org/MRET")]
    public partial class TelemetryTransformsType
    {

        private TelemetryTransformType[] telemetryTransformField;

        private int[] idField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("TelemetryTransform")]
        public TelemetryTransformType[] TelemetryTransform
        {
            get
            {
                return this.telemetryTransformField;
            }
            set
            {
                this.telemetryTransformField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ID")]
        public int[] ID
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1590.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.example.org/MRET")]
    public partial class PartsType
    {

        private PartType[] partsField;

        private int[] idField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Parts")]
        public PartType[] Parts
        {
            get
            {
                return this.partsField;
            }
            set
            {
                this.partsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ID")]
        public int[] ID
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1590.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.example.org/MRET")]
    public partial class QuaternionType
    {

        private float xField;

        private float yField;

        private float zField;

        private float wField;

        /// <remarks/>
        public float X
        {
            get
            {
                return this.xField;
            }
            set
            {
                this.xField = value;
            }
        }

        /// <remarks/>
        public float Y
        {
            get
            {
                return this.yField;
            }
            set
            {
                this.yField = value;
            }
        }

        /// <remarks/>
        public float Z
        {
            get
            {
                return this.zField;
            }
            set
            {
                this.zField = value;
            }
        }

        /// <remarks/>
        public float W
        {
            get
            {
                return this.wField;
            }
            set
            {
                this.wField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1590.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.example.org/MRET")]
    public enum PartTypePartType
    {

        /// <remarks/>
        CPU,

        /// <remarks/>
        Memory,

        /// <remarks/>
        FPGA,

        /// <remarks/>
        Processor,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("Processor Board")]
        ProcessorBoard,

        /// <remarks/>
        Microcontroller,

        /// <remarks/>
        Aerodynamic,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("Earth Tracker")]
        EarthTracker,

        /// <remarks/>
        Gyro,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("Reaction Wheel")]
        ReactionWheel,

        /// <remarks/>
        Magnetorquer,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("Star Tracker")]
        StarTracker,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("Sun Sensor")]
        SunSensor,

        /// <remarks/>
        SSDR,

        /// <remarks/>
        Antenna,

        /// <remarks/>
        Modem,

        /// <remarks/>
        Radio,

        /// <remarks/>
        Transceiver,

        /// <remarks/>
        Transmitter,

        /// <remarks/>
        Transponder,

        /// <remarks/>
        GPS,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("De-Orbit")]
        DeOrbit,

        /// <remarks/>
        Battery,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("Power Distribution Board")]
        PowerDistributionBoard,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("Solar Cell")]
        SolarCell,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("Solar Panel")]
        SolarPanel,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("Cold Gas")]
        ColdGas,

        /// <remarks/>
        Electric,

        /// <remarks/>
        Liquid,

        /// <remarks/>
        Bus,

        /// <remarks/>
        Chassis,

        /// <remarks/>
        Gymbal,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("Mechanical Arm")]
        MechanicalArm,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("Phase Change")]
        PhaseChange,

        /// <remarks/>
        GSE,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("Test Fixture")]
        TestFixture,

        /// <remarks/>
        Instrument,
    }
}