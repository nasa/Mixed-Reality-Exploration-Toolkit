// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Xml;
using System.Xml.Serialization;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_1;
using UnityEngine;

namespace GOV.NASA.GSFC.XR.MRET.Animation
{
    // Types

    /// <summary>
    /// Animation direction can either be forward from time 0 to duration or backward from 
    /// duration to 0.
    /// </summary>
    public enum Direction
    {
        Forward,
        Backward
    }

    /// <summary>
    /// The MRETBaseAnimation class is an abstract base class for all animation types. 
    /// 
    /// The progress of an animation is given by its current time, which is 
    /// measured in seconds from the start of the animation (0) to its end (Duration). The progress 
    /// of the animation can be set directly by setting the CurrentTime property or by periodically 
    /// calling UpdateTimeEvent(). The StepForward, StepBack, JumpToEnd, and Rewind methods will also 
    /// change the current time of the animation.
    /// 
    /// MRETBaseAnimation provides virtual functions used by subclasses to track the progress of the 
    /// animation: UpdateDirection() and UpdateCurrentTime(). The animation framework calls 
    /// UpdateCurrentTime() when current time has changed. 
    /// </summary>
    public abstract class MRETBaseAnimationDeprecated
    {
        // Fields
        private float pingPongDirection = 1f;
        private float animationDirection = 1f;

        // Properties
        private float currentTime = 0f;
        private float duration = 1.0f;
        private String name = String.Empty;
        private bool autoplay = false;
        private MRETAnimationGroupDeprecated group = null;
        private GameObject targetObject = null;
        private Direction direction = Direction.Forward;
        private WrapMode wrapMode = WrapMode.Default;

        // Event publishing

        // Property get and set methods.

        /// <summary>
        /// Target object that will be animated.
        /// </summary>
        public GameObject TargetObject { get => targetObject; set => targetObject = value; }

        /// <summary>
        /// This property holds the duration of the animation.
        /// </summary>
        public float Duration { get => duration; set => duration = value; }

         /// <summary>
        /// This property holds the current time and progress of the animation. You can change the 
        /// current time by setting the CurrentTime property, or you can call start() and let the 
        /// animation manager update the time automatically as the animation progresses.
        /// 
        /// The animation's current time starts at 0, and ends at Duration.
        /// </summary>        
        public float CurrentTime
        {
            get => currentTime;
            set
            {
                // Current time is limited to: 0 <= time
                value = (value < 0) ? 0 : value;

                // Handle time change
                currentTime = value;

                UpdateCurrentTime(currentTime);
            }
        }

        /// <summary>
        /// The name property of this animation.
        /// </summary>
        public string Name
        {
            get => name;
            set
            {
                if (value == null)
                {
                    name = string.Empty;
                    //throw new ArgumentException("Name must not be blank");
                }
                else
                {
                    name = value;
                }
            }
        }

        /// <summary>
        /// If this animation is part of a MRETAnimationGroup, this function returns a reference 
        /// to the group; otherwise, it returns null.
        /// </summary>
        public MRETAnimationGroupDeprecated Group { get => group; set => group = value; }


        /// <summary>
        /// Sets the wrap mode used in the animation.
        /// </summary>
        public WrapMode WrapMode
        {
            get => wrapMode;
            set
            {
                wrapMode = value;
                pingPongDirection = 1f;
            }
        }

        /// <summary>
        /// This property holds the direction of the animation when it is receiving UpdateTimeEvents. 
        /// This direction indicates whether the time moves from 0 towards the animation duration, or 
        /// from the value of the duration towards 0.
        /// By default, this property is set to Forward.
        /// </summary>
        public Direction Direction
        {
            get => direction;
            set
            {
                direction = value;
                if (value == Direction.Forward)
                {
                    animationDirection = 1f;
                }
                else
                {
                    animationDirection = -1f;
                }
            }
        }

        /// <summary>
        /// Should the animation be played automatically when loaded.
        /// </summary>
        public bool Autoplay { get => autoplay; set => autoplay = value; }

        /// <summary>
        /// Constructs the MRETBaseAnimation base class.
        /// </summary>
        public MRETBaseAnimationDeprecated() { }

        /// <summary>
        /// Step back a meaningful amount of time by offsetting currentTime by some amount. Subclasses 
        /// should override to define what offset should be used. This implementation uses 
        /// offset = Duration / 10.
        /// </summary>
        public virtual void StepBack()
        {
            float stepsize = Duration / 10.0f;

            float newtime = currentTime - stepsize;
            if (newtime >= 0)
            {
                CurrentTime = newtime;
            }
            else
            {
                CurrentTime = 0;
            }
        }

        /// <summary>
        /// Step forward a meaningful amount of time by offsetting currentTime by some amount. Subclasses 
        /// should override to define what offset should be used. This implementation uses 
        /// offset = Duration / 10.
        /// </summary>
        public virtual void StepForward()
        {
            float stepsize = Duration / 10.0f;

            float newtime = currentTime + stepsize;
            if (newtime > Duration)
            {
                CurrentTime = Duration;
            }
            else
            {
                CurrentTime = newtime;
            }
        }

        /// <summary>
        /// Jumps to the end of the animation CurrentTime = TotalDuration.
        /// </summary>
        public virtual void JumpToEnd()
        {
            CurrentTime = Duration;
        }

        /// <summary>
        /// Rewinds to the start of the animation CurrentTime = 0.
        /// </summary>
        public virtual void Rewind()
        {
            CurrentTime = 0;
        }

        /// <summary>
        /// This virtual method is called every time the animation's currentTime changes.
        /// </summary>
        /// <param name="currentTime"></param>
        protected virtual void UpdateCurrentTime(float currentTime) { }

        /// <summary>
        /// Called from the MRETAnimationManager periodically to update the animation's CurrentTime 
        /// property. The time parameter will be the time elapsed since the last time this method was 
        /// called.
        /// </summary>
        /// <param name="timeDelta">elapsed time in seconds</param>
        /// <returns></returns>
        public virtual void UpdateTimeEvent(float timeDelta)
        {
            float newCurrentTime = currentTime + (timeDelta * pingPongDirection * animationDirection);

            // Update based on wrap mode
            switch (WrapMode)
            {
                case WrapMode.Once:
                    if (newCurrentTime > duration)
                    {
                        newCurrentTime = 0;
                    }
                    else if (newCurrentTime < 0)
                    {
                        newCurrentTime = duration;
                    }
                    break;
                case WrapMode.Loop:
                    if (newCurrentTime > duration)
                    {
                        newCurrentTime = newCurrentTime % duration;
                    }
                    else if (newCurrentTime < 0)
                    {
                        newCurrentTime = duration - (newCurrentTime % duration); ;
                    }
                    break;
                case WrapMode.PingPong:
                    if (newCurrentTime > duration)
                    {
                        pingPongDirection = -pingPongDirection;
                        newCurrentTime = duration - (newCurrentTime % duration);
                    }
                    else if (newCurrentTime < 0)
                    {
                        pingPongDirection = -pingPongDirection;
                        newCurrentTime = 0 - (newCurrentTime % duration);
                    }
                    break;
                default:
                    if (newCurrentTime > duration)
                    {
                        newCurrentTime = duration;
                    }
                    else if (newCurrentTime < 0)
                    {
                        newCurrentTime = 0;
                    }
                    break;
            }

            CurrentTime = newCurrentTime;
        }

        #region Serialization

        /// <summary>
        /// Serializes this animation into the given serialized type.
        /// </summary>
        /// <param name="type">serialized type to modify</param>
        public virtual void SerializeTo(AnimationBaseType type)
        {
            Debug.Log("[MRETBaseAnimation->SerializeTo] ");
            type.Name = Name;
            type.Duration = Duration;
            type.Autoplay = Autoplay;
            type.Target = TargetObject?.GetInstanceID().ToString();
            type.Direction = Direction.ToString();
            type.WrapMode = WrapMode.ToString();
        }

        /// <summary>
        /// Returns a serialized version of this animation.
        /// </summary>
        /// <returns>an AnimationActionType instance</returns>
        public abstract AnimationBaseType Serialize();

        /// <summary>
        /// Deserialize information from the given serialized representation into the new animation instance.
        /// </summary>
        /// <param name="serializedAnimation">Serialized representation of an animation</param>
        /// <param name="animation">New animation instance to initialize.</param>
        public static void DeserializeTo(AnimationBaseType serializedAnimation, MRETBaseAnimationDeprecated animation)
        {
            Debug.Log("[MRETBaseAnimation->DeserializeTo] ");
            animation.Name = serializedAnimation.Name;
            animation.WrapMode = (WrapMode)Enum.Parse(typeof(WrapMode), serializedAnimation.WrapMode);
            animation.Direction = (Direction)Enum.Parse(typeof(Direction), serializedAnimation.Direction);
            animation.Duration = serializedAnimation.Duration;
            animation.Autoplay = serializedAnimation.Autoplay;
        }

        //public static MRETBaseAnimation Deserialize(AnimationBaseType serializedAnimation)
        //{
        //    Debug.Log("[MRETBaseAnimation->Deserialize] ");
        //    if (serializedAnimation == null)
        //    {
        //        return null;
        //    }


        //    //if (serializedAnimation.Name == null)
        //    //{
        //    //    return null;
        //    //}

        //    //if (serializedAnimation.Name == "")
        //    //{
        //    //    return null;
        //    //}

        //    MRETAnimationGroup deserialized = new MRETSequentialAnimationGroup() { };// Name = serializedAnimation.Name };
        //    //if (serializedAnimation.Sequence == null)
        //    //{
        //    //    return null;
        //    //}

        //    //foreach (AnimationClipType sClip in serializedAnimation.Sequence)
        //    //{
        //    //    MRETActionAnimation clip = new MRETActionAnimation();
        //    //    clip.Action = BaseAction.Deserialize(sClip.Action);
        //    //    clip.Inverse = (sClip.Inverse == null) ? null : BaseAction.Deserialize(sClip.Inverse);
        //    //    deserialized.AddAnimation(clip);
        //    //}

        //    return deserialized;
        //}

        #endregion Serialization

    }
}