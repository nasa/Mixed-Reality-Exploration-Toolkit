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


/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1590.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.example.org/MRET")]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.example.org/MRET", IsNullable = false)]
public partial class AudioRecording
{

    private string audioFileField;

    private float startTimeField;

    private float durationField;

    private float speedField;

    private bool loopField;

    private string attachToField;

    private string versionField;

    /// <remarks/>
    public string AudioFile
    {
        get
        {
            return this.audioFileField;
        }
        set
        {
            this.audioFileField = value;
        }
    }

    /// <remarks/>
    public float StartTime
    {
        get
        {
            return this.startTimeField;
        }
        set
        {
            this.startTimeField = value;
        }
    }

    /// <remarks/>
    public float Duration
    {
        get
        {
            return this.durationField;
        }
        set
        {
            this.durationField = value;
        }
    }

    /// <remarks/>
    public float Speed
    {
        get
        {
            return this.speedField;
        }
        set
        {
            this.speedField = value;
        }
    }

    /// <remarks/>
    public bool Loop
    {
        get
        {
            return this.loopField;
        }
        set
        {
            this.loopField = value;
        }
    }

    /// <remarks/>
    public string AttachTo
    {
        get
        {
            return this.attachToField;
        }
        set
        {
            this.attachToField = value;
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
