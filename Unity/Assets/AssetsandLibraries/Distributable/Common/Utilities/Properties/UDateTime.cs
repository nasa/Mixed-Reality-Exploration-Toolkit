// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace GOV.NASA.GSFC.XR.Utilities.Properties
{
    /**
     * Allows for DateTime values to be set in the Editor as string fields.
     */
    [Serializable]
    public class UDateTime : ISerializationCallbackReceiver
    {
        public string format = "{0:s}";

        [HideInInspector] public DateTime dateTime;

        // if you don't want to use the PropertyDrawer then remove HideInInspector here
        [HideInInspector] [SerializeField] private string _dateTime;

        public static implicit operator DateTime(UDateTime udt)
        {
            return (udt.dateTime);
        }

        public static implicit operator UDateTime(DateTime dt)
        {
            return new UDateTime() { dateTime = dt };
        }

        /**
         * Convert the internal string back to the datetime
         */
        public void OnAfterDeserialize()
        {
            DateTime.TryParse(_dateTime, out dateTime);
        }

        /**
         * Convert the datetime to our insternal formatted string
         */
        public void OnBeforeSerialize()
        {
            _dateTime = String.Format(format, dateTime);
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(UDateTime))]
    public class UDateTimeDrawer : PropertyDrawer
    {
        /**
         * Called to draw the property inside the given rect
         */
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Remove indenting on child fields, preserving the current value
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Draw the property field - pass GUIContent.none so it is drawn without labels
            EditorGUI.PropertyField(position, property.FindPropertyRelative("_dateTime"), GUIContent.none);

            // Restore the indent level
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
#endif

}
