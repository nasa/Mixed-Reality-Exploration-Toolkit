// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.Configuration;

namespace GOV.NASA.GSFC.XR.MRET.Animation
{
    /// <summary>
    /// The MRETAnimationGroup class is a base class for groups of animations. An animation 
    /// group is a container for animations (subclasses of MRETBaseAnimation). The default implementation
    /// runs animations based on the Order property (Parallel or Sequential) and the animation group 
    /// finishes when all the animations have finished. If Sequential, the animations are played in the 
    /// order they are added to the group (using AddAnimation() or InsertAnimation()).
    /// </summary>
    public class MRETAnimationGroup : MRETBaseAnimation<ActionSequenceGroupType>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(MRETAnimationGroup);

        public readonly ActionSequenceProcessingOrderType DEFAULT_PROCESSING_ORDER = new ActionSequenceGroupType().ProcessingOrder;

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private ActionSequenceGroupType serializedGroup;

        // Types

        /// <summary>
        /// The order in which animations in the groupd should be run. If Parallel all animations will 
        /// be run simultaniously. If Sequential they will be run in the order they were added. 
        /// Sequential is the default.
        /// </summary>
        private ActionSequenceProcessingOrderType order;

        /// <summary>
        /// Sets the order that the animations in the group will be run. Sequential is the default.
        /// </summary>
        public ActionSequenceProcessingOrderType Order
        {
            get => order;
            set
            {
                if (value != order)
                {
                    order = value;
                    UpdateDuration();
                }
            }
        }

        // Fields

        // Animations managed by this group.
        private List<IActionSequence> sequence = new List<IActionSequence>();
        // Structure to cache animation time offsets for performance reasons.
        private float[] animationTimeOffsets = { 0 };

        #region MRETUpdateBehaviour
        /// <seealso cref="MRETBehaviour.IntegrityCheck"/>
        protected override IntegrityState IntegrityCheck()
        {
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            return (
                (state == IntegrityState.Failure) // TODO: || (MyRequiredRef == null)

                    ? IntegrityState.Failure   // Fail is base class fails or anything is null
                    : IntegrityState.Success); // Otherwise, our integrity is valid
        }

        /// <seealso cref="MRETBehaviour.MRETAwake"/>
        protected override void MRETAwake()
        {
            base.MRETAwake();

            // Set the defaults
            Order = DEFAULT_PROCESSING_ORDER;
        }
        #endregion MRETUpdateBehaviour

        #region Serializable
        /// <summary>
        /// Asynchronously Deserializes the supplied seriqlized animation serializable animation
        /// into the supplied serializable animation and updates the supplied state with the result.
        /// </summary>
        /// <param name="serializableAnimation">The serializable <code>IActionSequence</code> animation</param>
        /// <param name="serializedAnimation">The serialized <code>ActionSequenceBaseType</code> animation</param>
        /// <param name="animationDeserializationState">The <code>SerializationState</code> to write the state.</param>
        /// 
        /// <see cref="IActionSequence"/>
        /// <see cref="ActionSequenceBaseType"/>
        /// <see cref="SerializationState"/>
        /// 
        protected virtual IEnumerator DeserializeAnimation(IActionSequence serializableAnimation, ActionSequenceBaseType serializedAnimation, SerializationState animationDeserializationState)
        {
            void DeserializeAnimationAction(bool deserialized, string message)
            {
                // Update the serialization state
                if (!deserialized)
                {
                    animationDeserializationState.Error(message);
                }

                // Mark as complete
                animationDeserializationState.complete = true;
            };

            // Deserialize the animation
            serializableAnimation.Deserialize(serializedAnimation, DeserializeAnimationAction);

            // If the deserialization failed, abort
            if (animationDeserializationState.IsError) yield break;

            // Add the animation to the group
            AddAnimation(serializableAnimation);

            yield return null;
        }

        /// <seealso cref="MRETBaseAnimation{T}.Deserialize(T, SerializationState)"/>
        /// <see cref="GroupType"/>
        protected override IEnumerator Deserialize(ActionSequenceGroupType serialized, SerializationState deserializationState)
        {
            StartCoroutine(base.Deserialize(serialized, deserializationState));

            // Wait for the coroutine to complete
            while (!deserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (deserializationState.IsError) yield break;

            // Start with an incomplete indicator
            deserializationState.Clear();

            // Process this object specific deserialization

            // Deserialize the processing order
            Order = serialized.ProcessingOrder;

            // Deserialize the animations
            foreach (ActionSequenceType serializedActionSequence in serialized.ActionSequence)
            {
                IActionSequence animation = null;
                ActionSequenceBaseType serializedAnimation = serializedActionSequence.Item;

                // Create the serializable implementation for this animation type
                if (serializedAnimation is ActionSequenceGroupType)
                {
                    // Animation group
                    animation = gameObject.AddComponent<MRETAnimationGroup>();
                }
                else if (serializedAnimation is ActionSequenceFrameType)
                {
                    // Animation frame
                    animation = gameObject.AddComponent<MRETActionAnimation>();
                }
                else if (serializedAnimation is ActionSequencePropertyType)
                {
                    // Animation property
                    animation = gameObject.AddComponent<MRETPropertyAnimation>();
                }
                else
                {
                    // Error condition
                    deserializationState.Error("Unknown animation type: " + nameof(serializedAnimation));

                    // Unknown animation type. Something's wrong
                    yield break;
                }

                // Perform the animation deserialization
                SerializationState animationDeserializationState = new SerializationState();
                StartCoroutine(DeserializeAnimation(animation, serializedAnimation, animationDeserializationState));

                // Wait for the coroutine to complete
                while (!animationDeserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // Record the deserialization state
                deserializationState.Update(animationDeserializationState);

                // If the animation deserialization failed, there's no point in continuing. Something's wrong
                if (deserializationState.IsError) yield break;
            }

            // Save the serialized reference
            serializedGroup = serialized;

            // Record the deserialization state as complete
            deserializationState.complete = true;

            yield return null;
        }

        /// <summary>
        /// Asynchronously serializes the supplied serializable animation into the supplied serialized
        /// animation and updates the supplied state with the result.
        /// </summary>
        /// <param name="serializableAnimation">The serializable <code>IActionSequence</code> animation</param>
        /// <param name="serializedAnimation">The serialized <code>ActionSequenceBaseType</code> animation to populate with the result.</param>
        /// <param name="animationSerializationState">The <code>SerializationState</code> to write the state.</param>
        /// 
        /// <see cref="IActionSequence"/>
        /// <see cref="ActionSequenceBaseType"/>
        /// <see cref="SerializationState"/>
        /// 
        protected virtual IEnumerator SerializeAnimation(IActionSequence serializableAnimation, ActionSequenceBaseType serializedAnimation, SerializationState animationSerializationState)
        {
            void SerializeAnimationAction(bool deserialized, string message)
            {
                // Update the serialization state
                if (!deserialized)
                {
                    animationSerializationState.Error(message);
                }

                // Mark as complete
                animationSerializationState.complete = true;
            };

            // Serialize the animation
            serializableAnimation.Serialize(serializedAnimation, SerializeAnimationAction);

            // If the serialization failed, abort
            if (animationSerializationState.IsError) yield break;

            yield return null;
        }

        /// <seealso cref="MRETBaseAnimation{T}.Serialize(T, SerializationState)"/>
        /// <see cref="ActionSequenceGroupType"/>
        protected override IEnumerator Serialize(ActionSequenceGroupType serialized, SerializationState serializationState)
        {
            StartCoroutine(base.Serialize(serialized, serializationState));

            // Wait for the coroutine to complete
            while (!serializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) yield break;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Serialize out the processing order if different than the default setting (optional)
            if (Order != DEFAULT_PROCESSING_ORDER)
            {
                serialized.ProcessingOrder = Order;
            }

            // Serialize out the animations in the group (optional)
            serialized.ActionSequence = null;
            List<ActionSequenceType> serializedAnimations = new List<ActionSequenceType>();
            foreach (IActionSequence animation in sequence)
            {
                ActionSequenceBaseType serializedAnimation = null;

                if (animation is MRETAnimationGroup)
                {
                    // Animation group
                    serializedAnimation = new ActionSequenceGroupType();
                }
                else if (animation is MRETActionAnimation)
                {
                    // Animation frame
                    serializedAnimation = new ActionSequenceFrameType();
                }
                else if (animation is MRETPropertyAnimation)
                {
                    // Animation property
                    serializedAnimation = new ActionSequencePropertyType();
                }
                else
                {
                    // Error condition
                    serializationState.Error("Unknown animation instance: " + nameof(animation));

                    // Unknown animation type. Something's wrong
                    yield break;
                }

                // Perform the animation serialization
                SerializationState animationSerializationState = new SerializationState();
                StartCoroutine(SerializeAnimation(animation, serializedAnimation, animationSerializationState));

                // Wait for the coroutine to complete
                while (!animationSerializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // Record the deserialization state
                serializationState.Update(animationSerializationState);

                // If the animation serialization failed, there's no point in continuing. Something's wrong
                if (serializationState.IsError) yield break;

                // We need to create the serialized action sequence type for the list
                ActionSequenceType serializedActionSequence = new ActionSequenceType();
                serializedActionSequence.Item = serializedAnimation;
                serializedAnimations.Add(serializedActionSequence);
            }

            // Serialize out the action sequence
            serialized.ActionSequence = serializedAnimations.ToArray();

            // Save the final serialized reference
            serializedGroup = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;

            yield return null;
        }
        #endregion Serializable

        /// <summary>
        /// Adds animation to this group. 
        /// </summary>
        /// <param name="animation"></param>
        public void AddAnimation(IActionSequence animation)
        {
            sequence.Add(animation);
            animation.Group = this;
            UpdateDuration();
        }

        /// <summary>
        /// Returns a reference to the animation at index in this group. 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public IActionSequence AnimationAt(int index)
        {
            IActionSequence result = null;

            if (index < sequence.Count)
            {
                result = sequence[index];
            }
            return result;
        }

        /// <summary>
        /// Returns the animation at the current time.
        /// </summary>
        /// <returns>a MRETBaseAnimation</returns>
        public IActionSequence CurrentAnimation()
        {
            int currentIndex = GetIndexContainingTime(CurrentTime);
            return AnimationAt(currentIndex);
        }

        /// <summary>
        /// Returns the number of animations managed by this group.
        /// </summary>
        /// <returns></returns>
        public int AnimationCount()
        {
            return sequence.Count;
        }

        /// <summary>
        /// Removes all animations in this animation group, and resets the current time to 0.
        /// </summary>
        public void Clear()
        {
            foreach (IActionSequence animation in sequence)
            {
                animation.Group = null;
            }
            sequence.Clear();
            CurrentTime = 0;
            Duration = 0;
        }

        /// <summary>
        /// Returns the index of animation. 
        /// </summary>
        /// <param name="animation"></param>
        /// <returns></returns>
        public int IndexOfAnimation(IActionSequence animation)
        {
            return sequence.FindIndex(animation.Equals);
        }

        /// <summary>
        /// Inserts animation into this animation group at index. If index is 0 the animation is 
        /// inserted at the beginning. If index is animationCount(), the animation is inserted at the 
        /// end.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="animation"></param>
        public void InsertAnimation(int index, IActionSequence animation)
        {
            sequence.Insert(index, animation);
            animation.Group = this;
            UpdateDuration();
        }

        /// <summary>
        /// Removes animation from this group.
        /// </summary>
        /// <param name="animation"></param>
        public void RemoveAnimation(IActionSequence animation)
        {
            sequence.Remove(animation);
            animation.Group = null;
            UpdateDuration();
        }

        /// <summary>
        /// Returns the animation at index and removes it from the animation group.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public IActionSequence RemoveAnimationAt(int index)
        {
            IActionSequence animation = sequence[index];
            sequence.RemoveAt(index);
            animation.Group = null;
            UpdateDuration();

            return animation;
        }

        /// <summary>
        /// Overrides inherited UpdateCurrentTime to update all animations in the group. The animations are
        /// updated based on this animation groups order..
        /// </summary>
        /// <param name="currentTime"></param>
        protected override void UpdateCurrentTime(float currentTime)
        {
            switch (Order)
            {
                case ActionSequenceProcessingOrderType.Sequential:
                    ApplyTime(currentTime);
                    break;
                case ActionSequenceProcessingOrderType.Parallel:
                    for (int i = 0; i < AnimationCount(); i++)
                    {
                        AnimationAt(i).CurrentTime = currentTime;
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Step back to the previous animation in the sequence or the default step.
        /// </summary>
        public override void StepBack()
        {
            if (Order == ActionSequenceProcessingOrderType.Sequential)
                StepBackSequential();
            else
                base.StepBack();
        }

        // Performs StepBack for a Sequential group
        private void StepBackSequential()
        {
            int currentIndex = GetIndexContainingTime(CurrentTime);
            float newTime = 0;

            if (CurrentTime > animationTimeOffsets[currentIndex])
            {
                newTime = animationTimeOffsets[currentIndex];
            }
            else if (currentIndex > 0)
            {
                newTime = animationTimeOffsets[currentIndex - 1];
            }

            CurrentTime = newTime;
        }

        /// <summary>
        /// Step forward to the next animation in the sequence or the default step.
        /// </summary>
        public override void StepForward()
        {
            if (Order == ActionSequenceProcessingOrderType.Sequential)
                StepForwardSequential();
            else
                base.StepForward();
        }

        // Performs StepForward for a Sequential group
        private void StepForwardSequential()
        {
            float newTime = CurrentTime;

            int currentIndex = GetIndexContainingTime(newTime);

            if (currentIndex < animationTimeOffsets.Length - 1)
            {
                newTime = animationTimeOffsets[currentIndex + 1];
            }
            else
            {
                newTime = Duration;
            }

            CurrentTime = newTime;
        }

        /// <summary>
        /// Called after an animation is added or removed from the group. This default implementation 
        /// sets duration to the longest animation in the group for a parallel group or the total duration
        /// for a sequential group. Subclasses should 
        /// override to update the value of duration if a different behavior is needed.
        /// </summary>
        protected virtual void UpdateDuration() 
        {
            float maxValue = 0;

            switch (Order)
            {
                case ActionSequenceProcessingOrderType.Sequential:
                    for (int i = 0; i < AnimationCount(); i++)
                    {
                        maxValue += AnimationAt(i).Duration;
                    }
                    break;
                case ActionSequenceProcessingOrderType.Parallel:
                    for (int i = 0; i < AnimationCount(); i++)
                    {
                        maxValue = System.Math.Max(maxValue, AnimationAt(i).Duration);
                    }
                    break;
                default:
                    maxValue = 1;
                    break;
            }

            Duration = maxValue;
            animationTimeOffsets = UpdatedTimeOffsets();
        }

        // Helper method to apply the time update over the sequence in the group.
        private void ApplyTime(float loopTime)
        {
            int targetIndex = GetIndexContainingTime(loopTime);
            float timeDelta = loopTime - animationTimeOffsets[targetIndex];

            for (int i = 0; i < targetIndex; i++)
            {
                AnimationAt(i).CurrentTime = AnimationAt(i).Duration;
            }

            IActionSequence animation = AnimationAt(targetIndex);
            if (animation != null) AnimationAt(targetIndex).CurrentTime = timeDelta;

            for (int i = targetIndex + 1; i < AnimationCount(); i++)
            {
                AnimationAt(i).CurrentTime = 0;
            }
        }

        // Helper method to find the index of the animation in the group that contains the given time.
        private int GetIndexContainingTime(float time)
        {
            float[] offsets = animationTimeOffsets;
            int index = 0;

            for (int i = 0; i < offsets.Length; i++)
            {
                if (time >= offsets[i])
                {
                    index = i;
                }
            }

            return index;
        }

        // Helper method to update the cached time offsets for the sequence. Called when an animation
        // is added or removed from the group.
        private float[] UpdatedTimeOffsets()
        {
            float[] offsets = new float[AnimationCount()];
            float startOffset = 0;

            for (int i = 0; i < offsets.Length; i++)
            {
                offsets[i] = startOffset;
                startOffset += AnimationAt(i).Duration;
            }
            return offsets;
        }

    }
}