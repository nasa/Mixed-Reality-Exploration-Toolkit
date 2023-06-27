// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.Configuration;

namespace GOV.NASA.GSFC.XR.MRET.Animation
{
    public class MRETPropertyAnimation : MRETBaseAnimation<ActionSequencePropertyType>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(MRETPropertyAnimation);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private ActionSequencePropertyType serializedProperty;

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
            animationClip.SampleAnimation(gameObject, currentTime);
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
        public override void StepForward()
        {
            base.StepForward();
            // TODO step to the next key frame
        }

        #region MRETUpdateBehaviour
        protected override void MRETAwake()
        {
            base.MRETAwake();

            animationClip.hideFlags = HideFlags.DontSave;
        }
        #endregion MRETUpdateBehaviour

        #region Serializable
        /// <seealso cref="MRETBaseAnimation{T}.Deserialize(T, SerializationState)"/>
        protected override IEnumerator Deserialize(ActionSequencePropertyType serialized, SerializationState deserializationState)
        {
            StartCoroutine(base.Deserialize(serialized, deserializationState));

            // Wait for the coroutine to complete
            while (!deserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (deserializationState.IsError) yield break;

            // Start with an incomplete indicator
            deserializationState.Clear();

            // Save the serialized reference
            serializedProperty = serialized;

            // Process this object specific deserialization

            // Record the deserialization state as complete
            deserializationState.complete = true;

            yield return null;
        }

        /// <seealso cref="MRETBaseAnimation{T}.Serialize(T, SerializationState)"/>
        protected override IEnumerator Serialize(ActionSequencePropertyType serialized, SerializationState serializationState)
        {
            StartCoroutine(base.Serialize(serialized, serializationState));

            // Wait for the coroutine to complete
            while (!serializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) yield break;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Save the final serialized reference
            serializedProperty = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;

            yield return null;
        }
        #endregion Serializable

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
                gameObject.transform,
                new object[] { value });
        }

        #endregion TestCode
    }
}
