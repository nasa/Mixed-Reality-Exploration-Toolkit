// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Reflection;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_1;
using UnityEngine;

namespace GOV.NASA.GSFC.XR.MRET.Animation
{
    public class MRETPropertyAnimationDeprecated : MRETBaseAnimationDeprecated
    {
        // AnimationClip managed by this Property Animation.
        private AnimationClip animationClip = new AnimationClip();
        // AnimationCurves managed by this Property Animation.
        private List<MRETAnimationCurve> animationCurves = new List<MRETAnimationCurve>();
        // Keyframes managed by this Property Animation.
        private List<Keyframe> animationKeys = new List<Keyframe>();

        // Properties

        /// <summary>
        /// This property holds the AnimationClip this class uses to play back property change animations.
        /// </summary>
        /// <seealso cref="AnimationClip.SetCurve"/>
        public AnimationClip AnimationClip { get => animationClip; protected set => animationClip = value; }

        //public new string Name { get => animationClip.name; set => animationClip.name = value; }
        //public WrapMode WrapMode { get => animationClip.wrapMode; set => animationClip.wrapMode = value; }
        
        /// <summary>
        /// Constructs a MRETPropertyAnimation to animate properties on a game object. The properties 
        /// to animate are set with the SetCurve method.
        /// </summary>
        public MRETPropertyAnimationDeprecated() : base()
        {
            animationClip.hideFlags = HideFlags.DontSave;
        }

        /// <summary>
        /// Assigns a curve to animate a specific property.
        /// </summary>
        /// <param name="relativePath">Path to the game object this curve applies to.</param>
        /// <param name="type">The class type of the component that is animated such as typeof(Transform).</param>
        /// <param name="propertyName">The name or path to the property being animated such as "localPosition.x".</param>
        /// <param name="curve">The animation curve.</param>
        /// <seealso cref="AnimationClip.SetCurve"/>
        /// <seealso cref="AnimationCurve"/>
        public void SetCurve(string relativePath, Type type, string propertyName, MRETAnimationCurve curve)
        {
            animationClip.SetCurve(relativePath, type, propertyName, curve);
            animationCurves.Add(curve);

            if (animationClip.length > Duration) Duration = animationClip.length;
            animationClip.legacy = true;
        }

        /// <summary>
        /// Clears all curves from the Property Animation.
        /// </summary>
        public void ClearCurves()
        {
            animationClip.ClearCurves();
            animationCurves.Clear();
        }

        /// <summary>
        /// Samples an animation at a given time for any animated property.
        /// </summary>
        /// <param name="go">The animated game object.</param>
        /// <param name="time">The time to sample an animation.</param>
        /// <seealso cref="AnimationClip.SampleAnimation"/>
        protected void SampleAnimation(GameObject go, float time)
        {
            //Debug.Log("[MRETAnimationClip].SampleAnimation: time:" + time);
            animationClip.SampleAnimation(go, time);
        }

        /// <summary>
        /// Overrides inherited updateCurrentTime to update the current value of the property.  
        /// </summary>
        /// <param name="currentTime">the new current time fo this animation</param>
        /// <seealso cref="MRETBaseAnimation.UpdateCurrentTime"/>
        protected override void UpdateCurrentTime(float currentTime)
        {
            base.UpdateCurrentTime(currentTime);
            //Debug.Log("[MRETPropertyAnimation].UpdateCurrentTime: time:" + currentTime);
            animationClip.SampleAnimation(TargetObject, currentTime);
        }

        /// <summary>
        /// Step back to the previous Keyframe. 
        /// </summary>
        public override void StepBack()
        {
            base.StepBack();
            // TODO step to previous key frame
        }

        /// <summary>
        /// Step forward to the next Keyframe.
        /// </summary>
        public virtual void StepForward()
        {
            base.StepForward();
            // TODO step to the next key frame
        }

        #region Serialization

        /// <summary>
        /// Serializes this animation into the given serialized type.
        /// </summary>
        /// <param name="type">serialized type to modify</param>
        public override void SerializeTo(AnimationBaseType type)
        {
            Debug.Log("[MRETPropertyAnimation->SerializeTo] ");
            base.SerializeTo(type);

            AnimationPropertyType animationType = type as AnimationPropertyType;
            if (animationType != null)
            {
                // TODO property specific serialization
            }
        }

        /// <summary>
        /// Returns a serialized version of this animation.
        /// </summary>
        /// <returns>a AnimationPropertyType instance</returns>
        public override AnimationBaseType Serialize()
        {
            Debug.Log("[MRETPropertyAnimation->Serialize] ");
            AnimationPropertyType type = new AnimationPropertyType();
            SerializeTo(type);

            return type;
        }

        /// <summary>
        /// Deserialize information from the given serialized representation into the new animation instance.
        /// </summary>
        /// <param name="serializedAnimation">Serialized representation of an animation</param>
        /// <param name="animation">New animation instance to initialize.</param>
        public static void DeserializeTo(AnimationPropertyType serializedAnimation, MRETPropertyAnimationDeprecated animation)
        {
            Debug.Log("[MRETPropertyAnimation->DeserializeTo] ");
            MRETBaseAnimationDeprecated.DeserializeTo(serializedAnimation, animation);

            // TODO do class specific DeserializeTo
        }

        /// <summary>
        /// Deserialize information into a new animation instance.
        /// </summary>
        /// <param name="serializedAnimation">Serialized representation of an animation</param>
        /// <returns>A new animation instance.</returns>
        public static MRETBaseAnimationDeprecated Deserialize(AnimationPropertyType serializedAnimation)
        {
            Debug.Log("[MRETPropertyAnimation->Deserialize] ");
            MRETPropertyAnimationDeprecated animation = new MRETPropertyAnimationDeprecated();
            DeserializeTo(serializedAnimation, animation);

            return animation;
        }
            
        #endregion Serialization

        #region TestCode

        // Testing reflection in C#
        private void ReflectionTest(object value)
        {
            string packageName = "UnityEngine";
            string namespaceName = "UnityEngine";
            string typeName = "Transform";
            string methodName = "position";

            string typePath = namespaceName + "." + typeName + "," + packageName;

            // Set property via reflection
            Type resolvedType = Type.GetType(typePath);
            //resolvedType = TargetObject.transform.GetType();
            Debug.Log("*** [MRETPropertyAnimation] resolvedType:" + resolvedType);

            // value is Type Vector3 for this call
            object result = resolvedType.InvokeMember(methodName,
                BindingFlags.SetProperty,
                null,
                TargetObject.transform,
                new object[] { value });
        }

        #endregion TestCode
    }
}
