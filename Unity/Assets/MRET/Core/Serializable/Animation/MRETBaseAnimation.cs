// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.Configuration;

namespace GOV.NASA.GSFC.XR.MRET.Animation
{
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
    public abstract class MRETBaseAnimation<T> : Identifiable<T>, IActionSequence
        where T : ActionSequenceBaseType, new()
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(MRETBaseAnimation<T>);

        // Assign these constants based upon the schema in case it changes
        public readonly bool DEFAULT_AUTOPLAY = new ActionSequenceBaseType().Autoplay;
        public readonly ActionSequenceDirectionType DEFAULT_DIRECTION = new ActionSequenceBaseType().Direction;
        public readonly float DEFAULT_DURATION = new ActionSequenceBaseType().Duration;
        public readonly ActionSequenceWrapModeType DEFAULT_WRAPMODE = new ActionSequenceBaseType().WrapMode;

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private ActionSequenceBaseType serializedActionSequence;

        // Fields
        private float pingPongDirection = 1f;
        private float animationDirection = 1f;

        // Properties
        private float currentTime = 0f;
        private MRETAnimationGroup group = null;

        #region IActionSequence
        /// <seealso cref="IActionSequence.Duration"/>
        public float Duration { get; set; }

        /// <seealso cref="IActionSequence.WrapMode"/>
        public ActionSequenceWrapModeType WrapMode
        {
            get
            {
                return WrapMode;
            }
            set
            {
                WrapMode = value;
                pingPongDirection = 1f;
            }
        }

        /// <summary>
        /// This property holds the direction of the animation when it is receiving UpdateTimeEvents. 
        /// This direction indicates whether the time moves from 0 towards the animation duration, or 
        /// from the value of the duration towards 0.
        /// By default, this property is set to Forward.
        /// </summary>
        /// <seealso cref="IActionSequence.Direction"/>
        public ActionSequenceDirectionType Direction
        {
            get
            {
                return Direction;
            }
            set
            {
                Direction = value;
                if (value == ActionSequenceDirectionType.Forward)
                {
                    animationDirection = 1f;
                }
                else
                {
                    animationDirection = -1f;
                }
            }
        }

        /// <seealso cref="IActionSequence.Autoplay"/>
        public bool Autoplay { get; protected set; }

        /// <summary>
        /// This property holds the current time and progress of the animation. You can change the 
        /// current time by setting the CurrentTime property, or you can call start() and let the 
        /// animation manager update the time automatically as the animation progresses.
        /// 
        /// The animation's current time starts at 0, and ends at Duration.
        /// </summary>        
        /// <seealso cref="IActionSequence.CurrentTime"/>
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
        /// If this animation is part of a <code>IActionSequenceGroup</code>, this function returns
        /// a reference to the group; otherwise, it returns null.
        /// </summary>
        public MRETAnimationGroup Group { get => group; set => group = value; }

        /// <summary>
        /// Called from the MRETAnimationManager periodically to update the animation's CurrentTime 
        /// property. The time parameter will be the time elapsed since the last time this method was 
        /// called.
        /// </summary>
        /// <param name="timeDelta">elapsed time in seconds</param>
        /// <see cref="IActionSequence.UpdateTimeEvent(float)"/>
        public virtual void UpdateTimeEvent(float timeDelta)
        {
            float newCurrentTime = currentTime + (timeDelta * pingPongDirection * animationDirection);

            // Update based on wrap mode
            switch (WrapMode)
            {
                case ActionSequenceWrapModeType.Once:
                    if (newCurrentTime > Duration)
                    {
                        newCurrentTime = 0;
                    }
                    else if (newCurrentTime < 0)
                    {
                        newCurrentTime = Duration;
                    }
                    break;
                case ActionSequenceWrapModeType.Loop:
                    if (newCurrentTime > Duration)
                    {
                        newCurrentTime = newCurrentTime % Duration;
                    }
                    else if (newCurrentTime < 0)
                    {
                        newCurrentTime = Duration - (newCurrentTime % Duration); ;
                    }
                    break;
                case ActionSequenceWrapModeType.PingPong:
                    if (newCurrentTime > Duration)
                    {
                        pingPongDirection = -pingPongDirection;
                        newCurrentTime = Duration - (newCurrentTime % Duration);
                    }
                    else if (newCurrentTime < 0)
                    {
                        pingPongDirection = -pingPongDirection;
                        newCurrentTime = 0 - (newCurrentTime % Duration);
                    }
                    break;
                case ActionSequenceWrapModeType.ClampForever:
                case ActionSequenceWrapModeType.Default:
                default:
                    if (newCurrentTime > Duration)
                    {
                        newCurrentTime = Duration;
                    }
                    else if (newCurrentTime < 0)
                    {
                        newCurrentTime = 0;
                    }
                    break;
            }

            CurrentTime = newCurrentTime;
        }

        /// <summary>
        /// Step back a meaningful amount of time by offsetting currentTime by some amount. Subclasses 
        /// should override to define what offset should be used. This implementation uses 
        /// offset = Duration / 10.
        /// </summary>
        /// <see cref="IActionSequence.StepBack"/>
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
        /// <see cref="IActionSequence.StepForward"/>
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
        /// <see cref="IActionSequence.JumpToEnd"/>
        public virtual void JumpToEnd()
        {
            CurrentTime = Duration;
        }

        /// <summary>
        /// Rewinds to the start of the animation CurrentTime = 0.
        /// </summary>
        /// <see cref="IActionSequence.Rewind"/>
        public virtual void Rewind()
        {
            CurrentTime = 0;
        }
        #endregion IActionSequence

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

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETAwake()
        {
            base.MRETAwake();

            // Set the defaults
            Autoplay = false;
            Direction = DEFAULT_DIRECTION;
            Duration = DEFAULT_DURATION;
            WrapMode = DEFAULT_WRAPMODE;
        }
        #endregion MRETUpdateBehaviour

        #region Serializable
        /// <seealso cref="Identifiable{T}.Deserialize(T, SerializationState)"/>
        protected override IEnumerator Deserialize(T serialized, SerializationState deserializationState)
        {
            StartCoroutine(base.Deserialize(serialized, deserializationState));

            // Wait for the coroutine to complete
            while (!deserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (deserializationState.IsError) yield break;

            // Start with an incomplete indicator
            deserializationState.Clear();

            // Save the serialized reference
            serializedActionSequence = serialized;

            // Process this object specific deserialization
            WrapMode = serialized.WrapMode;
            Direction = serialized.Direction;
            Duration = serialized.Duration;
            Autoplay = serialized.Autoplay;

            // Record the deserialization state as complete
            deserializationState.complete = true;

            yield return null;
        }

        /// <seealso cref="Identifiable{T}.Serialize(T, SerializationState)"/>
        protected override IEnumerator Serialize(T serialized, SerializationState serializationState)
        {
            StartCoroutine(base.Serialize(serialized, serializationState));

            // Wait for the coroutine to complete
            while (!serializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) yield break;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization
            if (Duration != DEFAULT_DURATION)
            {
                serialized.Duration = Duration;
            }
            if (Autoplay != DEFAULT_AUTOPLAY)
            {
                serialized.Autoplay = Autoplay;
            }
            if (Direction != DEFAULT_DIRECTION)
            {
                serialized.Direction = Direction;
            }
            if (WrapMode != DEFAULT_WRAPMODE)
            {
                serialized.WrapMode = WrapMode;
            }

            // Save the final serialized reference
            serializedActionSequence = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;

            yield return null;
        }
        #endregion Serializable

        /// <summary>
        /// This virtual method is called every time the animation's currentTime changes.
        /// </summary>
        /// <param name="currentTime"></param>
        protected virtual void UpdateCurrentTime(float currentTime) { }

    }
}