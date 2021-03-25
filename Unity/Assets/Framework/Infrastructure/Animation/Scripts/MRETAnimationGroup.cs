// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using UnityEngine;
using GSFC.ARVR.MRET.Common.Schemas;

namespace GSFC.ARVR.MRET.Infrastructure.Framework.Animation
{
    /// <summary>
    /// The MRETAnimationGroup class is a base class for groups of animations. An animation 
    /// group is a container for animations (subclasses of MRETBaseAnimation). The default implementation
    /// runs animations based on the Order property (Parallel or Sequential) and the animation group 
    /// finishes when all the animations have finished. If Sequential, the animations are played in the 
    /// order they are added to the group (using AddAnimation() or InsertAnimation()).
    /// </summary>
    public class MRETAnimationGroup : MRETBaseAnimation
    {
        // Types

        /// <summary>
        /// The order in which animations in the groupd should be run. If Parallel all animations will 
        /// be run simultaniously. If Sequential they will be run in the order they were added. 
        /// Sequential is the default.
        /// </summary>
        public enum GroupOrder
        {
            Sequential,
            Parallel
        }

        // Properties
        private GroupOrder order = GroupOrder.Sequential;

        /// <summary>
        /// Sets the order that the animations in the group will be run. Sequential is the default.
        /// </summary>
        public GroupOrder Order
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
        private List<MRETBaseAnimation> sequence = new List<MRETBaseAnimation>();
        // Structure to cache animation time offsets for performance reasons.
        private float[] animationTimeOffsets = { 0 };


        /// <summary>
        /// Adds animation to this group. 
        /// </summary>
        /// <param name="animation"></param>
        public void AddAnimation(MRETBaseAnimation animation)
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
        public MRETBaseAnimation AnimationAt(int index)
        {
            MRETBaseAnimation result = null;

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
        public MRETBaseAnimation CurrentAnimation()
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
            foreach (MRETBaseAnimation animation in sequence)
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
        public int IndexOfAnimation(MRETBaseAnimation animation)
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
        public void InsertAnimation(int index, MRETBaseAnimation animation)
        {
            sequence.Insert(index, animation);
            animation.Group = this;
            UpdateDuration();
        }

        /// <summary>
        /// Removes animation from this group.
        /// </summary>
        /// <param name="animation"></param>
        public void RemoveAnimation(MRETBaseAnimation animation)
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
        public MRETBaseAnimation RemoveAnimationAt(int index)
        {
            MRETBaseAnimation animation = sequence[index];
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
                case GroupOrder.Sequential:
                    ApplyTime(currentTime);
                    break;
                case GroupOrder.Parallel:
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
            if (Order == GroupOrder.Sequential)
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
            if (Order == GroupOrder.Sequential)
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
                case GroupOrder.Sequential:
                    for (int i = 0; i < AnimationCount(); i++)
                    {
                        maxValue += AnimationAt(i).Duration;
                    }
                    break;
                case GroupOrder.Parallel:
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

            MRETBaseAnimation animation = AnimationAt(targetIndex);
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

        #region Serialization

        /// <summary>
        /// Serializes this animation into the given serialized type.
        /// </summary>
        /// <param name="type">serialized type to modify</param>
        public override void SerializeTo(AnimationBaseType type)
        {
            Debug.Log("[MRETAnimationGroup->SerializeTo] ");
            base.SerializeTo(type);

            AnimationGroupType animationType = type as AnimationGroupType;
            if (animationType != null)
            {
                animationType.Order = Order.ToString();
                List<AnimationBaseType> serializedBaseTypes = new List<AnimationBaseType>();
                foreach (MRETBaseAnimation animation in sequence)
                {
                    serializedBaseTypes.Add(animation.Serialize());
                }

                animationType.Animation = serializedBaseTypes.ToArray();
            }
        }

        /// <summary>
        /// Returns a serializable version of this animation.
        /// </summary>
        /// <returns>a AnimationGroupType instance</returns>
        public override AnimationBaseType Serialize()
        {
            Debug.Log("[MRETAnimationGroup->Serialize] ");
            AnimationGroupType type = new AnimationGroupType();
            SerializeTo(type);

            return type;
        }

        /// <summary>
        /// Deserialize information from the given serialized representation into the new animation instance.
        /// </summary>
        /// <param name="serializedAnimation">Serialized representation of an animation</param>
        /// <param name="group">New animation instance to initialize.</param>
        public static void DeserializeTo(AnimationGroupType serializedAnimation, MRETAnimationGroup group)
        {
            Debug.Log("[MRETAnimationGroup->DeserializeTo] ");
            MRETBaseAnimation.DeserializeTo(serializedAnimation, group);
            group.Order = (GroupOrder)Enum.Parse(typeof(GroupOrder), serializedAnimation.Order);

            foreach (AnimationBaseType animationType in serializedAnimation.Animation)
            {
                MRETBaseAnimation animation = null;

                // Check the type and create the correct type of animation
                if (animationType.GetType() == typeof(AnimationGroupType))
                {
                    animation = MRETAnimationGroup.Deserialize((AnimationGroupType)animationType);
                }
                else if (animationType.GetType() == typeof(AnimationActionType))
                {
                    animation = MRETActionAnimation.Deserialize((AnimationActionType)animationType);
                }
                else if (animationType.GetType() == typeof(AnimationPropertyType))
                {
                    animation = MRETPropertyAnimation.Deserialize((AnimationPropertyType)animationType);
                }

                // Add the animation to the group
                if (animation != null)
                {
                    group.AddAnimation(animation);
                }
            }
        }

        /// <summary>
        /// Deserialize information into a new animation instance.
        /// </summary>
        /// <param name="serializedAnimation">Serialized representation of an animation</param>
        /// <returns>A new animation instance.</returns>
        public static MRETAnimationGroup Deserialize(AnimationGroupType serializedAnimation)
        {
            Debug.Log("[MRETAnimationGroup->Deserialize] ");
            MRETAnimationGroup deserialized = new MRETAnimationGroup();
            DeserializeTo(serializedAnimation, deserialized);

            return deserialized;
        }

        #endregion Serialization
    }
}