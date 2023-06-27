// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

#if UNITY_EDITOR
using System;
using System.Diagnostics;

namespace GOV.NASA.GSFC.XR.MRET.Editor
{
    /// <remarks>
    /// History:
    /// 1 January 9999: Created
    /// </remarks>
    ///
    /// <summary>
    /// ExtensionAttribute
    ///
    /// Defines the extension pragma associated with the dependent class
    /// Unity's ConditionalCompilationUtility:
    /// https://github.com/Unity-Technologies/ConditionalCompilationUtility/tree/f364090bbda3728e1662074c969c2b7c3c34199b
    ///
    /// Author: TODO
    /// </summary>
    [Conditional("UNITY_CCU")]
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class MRETExtensionAttribute : Attribute
    {
        public string dependentClass;
        public string define;

        public MRETExtensionAttribute(string dependentClass, string define)
        {
            this.dependentClass = dependentClass;
            this.define = define;
        }
    }
}
#endif