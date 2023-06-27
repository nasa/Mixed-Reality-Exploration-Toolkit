// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GOV.NASA.GSFC.XR.CrossPlatformInputSystem
{
    public enum MotionConstraint
    {
        Normal,
        Slow,
        Fast
    }

    public enum GravityConstraint
    {
        Allowed,
        Required,
        Prohibited
    }

    /// <remarks>
    /// History:
    /// 8 march 2021: Created
    /// </remarks>
    ///
    /// <summary>
    /// IInputRigLocomotionControl
    ///
    /// Defines the input ig locomotion control interface
    ///
    /// Author: Jeffrey C. Hosler
    /// </summary>
    /// 
    public interface IInputRigLocomotionControl
    {
        /// <summary>
        /// Gets the active state of the control
        /// </summary>
        /// 
        /// <returns>The active state of the control</returns>
        bool GetControlActive();

        /// <summary>
        /// Sets the control active state
        /// </summary>
        /// 
        /// <param name="value">The new control active state</param>
        void SetControlActive(bool value);

        /// <summary>
        /// Gets the current motion constraint for this controller
        /// </summary>
        /// 
        /// <returns>The motion constraint of the controller</returns>
        /// <seealso cref="MotionConstraint"/>
        MotionConstraint GetMotionConstraint();

        /// <summary>
        /// Gets the current motion step multiplier that exaggerates (values > 1) or limits movement (values < 1)
        /// </summary>
        /// 
        /// <returns>The current motion multiplier of the controller</returns>
        float GetMotionMultiplier();

        /// <summary>
        /// Gets the motion step multiplier for the supplied motion constraint representing the value that
        /// exaggerates (values > 1) or limits movement (values < 1)
        /// </summary>
        /// 
        /// <returns>The motion multiplier of the controller</returns>
        /// <seealso cref="MotionConstraint"/>
        float GetMotionConstraintMultiplier(MotionConstraint constraint);

        /// <summary>
        /// Sets the motion step multiplier for the supplied motion constraint used to exaggerate
        /// (values > 1) or limit movement (values < 1)
        /// </summary>
        /// 
        /// <param name="constraint">The motion constraint multiplier being set</param>
        /// <param name="value">The new motion multiplier for the supplied constraint</param>
        /// <seealso cref="MotionConstraint"/>
        void SetMotionConstraintMultiplier(MotionConstraint constraint, float value);

        /// <summary>
        /// Gets the gravity constraint for this controller
        /// </summary>
        /// 
        /// <returns>The gravity constraint of the controller</returns>
        /// <seealso cref="GravityConstraint"/>
        GravityConstraint GetGravityConstraint();

        /// <summary>
        /// Sets the gravity constraint for this controller
        /// </summary>
        /// 
        /// <param name="value">The gravity constraint of the controller</param>
        /// <seealso cref="GravityConstraint"/>
        void SetGravityConstraint(GravityConstraint value);

        /// <summary>
        /// Sets active state of the supplied hand
        /// </summary>
        /// 
        /// <param name="hand">The hand associated with the changing active state</param>
        /// <param name="value">The new active state</param>
        void SetHandActiveState(InputHand hand, bool value);

    }

#if UNITY_EDITOR
    /// <summary>
    /// Attribute that require implementation of the provided interface.
    /// </summary>
    public class RequireInterfaceAttribute : PropertyAttribute
    {
        // Interface type.
        public System.Type requiredType { get; private set; }

        /// <summary>
        /// Requiring implementation of the <see cref="T:RequireInterfaceAttribute"/> interface.
        /// </summary>
        /// <param name="type">Interface type.</param>
        public RequireInterfaceAttribute(System.Type type)
        {
            this.requiredType = type;
        }
    }

    /// <summary>
    /// Drawer for the RequireInterface attribute.
    /// </summary>
    [CustomPropertyDrawer(typeof(RequireInterfaceAttribute))]
    public class RequireInterfaceDrawer : PropertyDrawer
    {
        /// <summary>
        /// Overrides GUI drawing for the attribute.
        /// </summary>
        /// <param name="position">Position.</param>
        /// <param name="property">Property.</param>
        /// <param name="label">Label.</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Check if this is reference type property.
            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                // Get attribute parameters.
                var requiredAttribute = this.attribute as RequireInterfaceAttribute;

                // Begin drawing property field.
                EditorGUI.BeginProperty(position, label, property);

                if (property.objectReferenceValue && requiredAttribute.requiredType.IsAssignableFrom(property.objectReferenceValue.GetType()))
                {
//                    Debug.Log("A) ASSIGNABLE!");
                }

                // Draw property field.
//                Debug.Log("1) Object reference value: " + property.objectReferenceValue + "; Required type: " + requiredAttribute.requiredType);
                property.objectReferenceValue = EditorGUI.ObjectField(position, label, property.objectReferenceValue, requiredAttribute.requiredType, true);
//                Debug.Log("2) Object reference value: " + property.objectReferenceValue + "; Required type: " + requiredAttribute.requiredType);

                // Finish drawing property field.
                EditorGUI.EndProperty();
            }
            else
            {
//                Debug.Log("3) Object reference value: " + property.objectReferenceValue + " Error");
                // If field is not reference, show error message.
                // Save previous color and change GUI to red.
                var previousColor = GUI.color;
                GUI.color = Color.red;

                // Display label with error message.
                EditorGUI.LabelField(position, label, new GUIContent("Property is not a reference type"));

                // Revert color change.
                GUI.color = previousColor;
            }
        }
    }

    [CustomEditor(typeof(RequireInterfaceAttribute))]
    public class RequireInterfaceEditor : Editor
    {
        private SerializedObject obj;

        public void OnEnable()
        {
            Debug.Log("ENABLED");
            obj = new SerializedObject(target);
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();
            DropAreaGUI();
        }

        public void DropAreaGUI()
        {
            Event evt = Event.current;

            Rect drop_area = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));

            GUI.Box(drop_area, "Add Trigger");

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:

                    Debug.Log("EVENT DETECTED");

                    if (!drop_area.Contains(evt.mousePosition))
                        return;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        Debug.Log("ACCEPTING DRAG");
                        DragAndDrop.AcceptDrag();
                        foreach (Object dragged_object in DragAndDrop.objectReferences)
                        {
                            // Do On Drag Stuff here
                        }
                    }

                    break;

            }

        }

    }
#endif

}