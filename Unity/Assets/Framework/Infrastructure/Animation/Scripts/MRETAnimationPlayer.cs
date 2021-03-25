// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GSFC.ARVR.MRET.Infrastructure.Framework.Animation
{
    /// <summary>
    /// The state of the animation. Stopped is the default initial state when the animation is not 
    /// running. The paused state temporarily suspends the animation until the resume method is called. 
    /// </summary>
    public enum State
    {
        Stopped,
        Paused,
        Running
    }

    /// <remarks>
    /// History:
    /// 1 January 9999: Created
    /// </remarks>
	///
	/// <summary>
	/// MRETAnimation
	///
    /// This class is responsible for sending an update time event to an animation at regular 
    /// intervals.
    /// 
    /// At any point an animation is in one of three states: Running, Stopped, or Paused--as defined by the 
    /// State property. The current state can be changed by calling start(), stop(), pause(), or 
    /// resume(). The animation will always reset its current time when it is started. If paused, 
    /// it will continue with the same current time when resumed. When an animation is stopped, 
    /// it cannot be resumed, but will keep its current time (until started again). This 
    /// class will send a stateChanged() to listeners whenever its state changes.
	///
    /// Author: T. Ames
	/// </summary>
	/// 
    public class MRETAnimationPlayer : MRETBehaviour
    {
        // Event publishing

        /// <summary>
        /// The state of the animation has changed.
        /// </summary>
        public delegate void StateChangeDelegate();
        public static event StateChangeDelegate StateChangeEvent;
        /// <summary>
        /// The animation has stopped and has reached the end.
        /// </summary>
        public delegate void AnimationFinishedDelegate();
        public static event AnimationFinishedDelegate AnimationFinishedEvent;

        // Unity Editor accessable properties
        public bool unityEditorEnabled = false;
        public string name;
        public GameObject target;
        public float duration;
        public Direction direction;
        public WrapMode wrapMode;
        public State state = State.Stopped;

        // Properties
        private MRETBaseAnimation baseAnimation;
        private float previousStartTime;
        private float pauseDurationTime;
        private State _state = State.Stopped;

        // Fields
        private bool editorValuesChanged = false;


        /// <seealso cref="MRETBaseAnimation.Duration"/>
        public float Duration
        {
            get => baseAnimation.Duration;
            set
            {
                baseAnimation.Duration = value;
                // Update Unity Editor field
                duration = value;
            }
        }

        /// <summary>
        /// This property holds the direction of the animation when it is in the running state. 
        /// This direction indicates whether the time moves from 0 towards the animation duration, or 
        /// from the value of the duration towards 0.
        /// By default, this property is set to Forward.
        /// </summary>
        public Direction Direction
        {
            get => baseAnimation.Direction;
            set
            {
                baseAnimation.Direction = value;
                // Update Unity Editor field
                direction = value;
            }
        }

        /// <seealso cref="MRETBaseAnimation.Name"/>
        public string Name
        {
            get => baseAnimation.Name;
            set
            {
                baseAnimation.Name = value;
                // Update Unity Editor field
                name = value;
            }
        }

        /// <summary>
        /// State of the animation. Calls UpdateState for subclasses to handle state changes. 
        /// Sends a StateChangeEvent to listeners.
        /// </summary>
        public State State
        {
            get => _state;
            protected set
            {
                State oldState = _state;
                Debug.Log("[MRETAnimationPlayer] State: " + value);

                if (value != oldState)
                {
                    // A state change from stopped to paused is not valid.
                    if (oldState == State.Stopped && value == State.Paused) return;

                    _state = value;
                    UpdateState(_state, oldState);

                    // Publish event to any listeners
                    StateChangeEvent?.Invoke();
                }
            }
        }

       /// <summary>
        /// Determines how time is treated outside of the time range of an animation.
        /// </summary>
        public WrapMode WrapMode
        {
            get => baseAnimation.WrapMode;
            set
            {
                baseAnimation.WrapMode = value;
                // Update Unity Editor field
                wrapMode = value;
            }
        }

        /// <summary>
        /// MRET Animation managed by this Animation Player.
        /// </summary>
        public MRETBaseAnimation MRETAnimation
        {
            get => baseAnimation;
            set
            {
                baseAnimation = value;
                Name = baseAnimation.Name;
                target = baseAnimation.TargetObject;
                duration = baseAnimation.Duration;
                wrapMode = baseAnimation.WrapMode;
                direction = baseAnimation.Direction;
                //MRETAnimationManager.ActiveAnimationChangeEvent += UpdateSelectedAnimation;
            }
        }

        /// <summary>
        /// This property holds the current time and progress of the animation. You can change the 
        /// current time by setting the CurrentTime property, or you can call start() and let this 
        /// animation player to update the time automatically as the animation progresses.
        /// 
        /// The animation's current time starts at 0, and ends at Duration.
        /// </summary>        
        /// <seealso cref="MRETBaseAnimation.CurrentTime"/>
        public float CurrentTime
        {
            get => baseAnimation.CurrentTime;
            set
            {
                baseAnimation.CurrentTime = value;
            }
        }

        // Overridden MRETBehaviour methods

        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName
        {
            get
            {
                return nameof(MRETAnimationPlayer);
            }
        }

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
            // Take the inherited behavior
            base.MRETAwake();

            // TODO: Custom initialization (before deserialization)

        }

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            // Take the inherited behavior
            base.MRETStart();

            Debug.Log("[MRETAnimationPlayer] Start");
            previousStartTime = UnityEngine.Time.time; // DateTime.Now.Ticks;
        }

        // Unity calls

        // Called when there is a change in the Editor accessable properties.
        private void OnValidate()
        {
            Debug.Log("[MRETAnimationPlayer] OnValidate()");
            if (baseAnimation != null)
            {
                // TODO verify the values are valid
                editorValuesChanged = true;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (editorValuesChanged && unityEditorEnabled)
            {
                UpdateAnimationProperties();
                editorValuesChanged = false;
            }

            UpdateAnimation();
        }

        /// <summary>
        /// Sends the update event to the animation if it is in a Started state.
        /// </summary>
        protected void UpdateAnimation()
        {
            if (baseAnimation != null)
            {
                if (State == State.Running)
                {
                    // TODO the resolution of the System clock appears to be 100ms. Find a better clock.
                    //float currentSystemTime = UnityEngine.Time.time;
                    //float timeDelta = currentSystemTime - previousStartTime;
                    float timeDelta = UnityEngine.Time.deltaTime;

                    baseAnimation.UpdateTimeEvent(timeDelta);

                    // Update animation state
                    switch (baseAnimation.WrapMode)
                    {
                        case WrapMode.Once:
                            if (baseAnimation.CurrentTime <= 0 && Direction == Direction.Forward)
                            {
                                State = State.Stopped;

                                // Publish event to any listeners
                                AnimationFinishedEvent?.Invoke();
                            }
                            else if (baseAnimation.CurrentTime >= baseAnimation.Duration 
                                && Direction == Direction.Backward)
                            {
                                State = State.Stopped;

                                // Publish event to any listeners
                                AnimationFinishedEvent?.Invoke();
                            }

                            break;
                        case WrapMode.Loop:
                            // Animation never stops in this mode
                           break;
                        case WrapMode.PingPong:
                            // Animation never stops in this mode
                            break;
                        case WrapMode.ClampForever:
                            // Animation never stops in this mode
                            break;
                        default:
                            if (baseAnimation.CurrentTime >= baseAnimation.Duration)
                            {
                                State = State.Stopped;

                                // Publish event to any listeners
                                AnimationFinishedEvent?.Invoke();
                            }
                            else if (baseAnimation.CurrentTime <= 0)
                            {
                                State = State.Stopped;

                                // Publish event to any listeners
                                AnimationFinishedEvent?.Invoke();
                            }
                            break;
                    }

                    //Debug.Log("[MRETAnimationPlayer] UpdateAnimation timeDelta: " + timeDelta + " => " + baseAnimation.CurrentTime);
                    //Debug.Log("[MRETAnimationPlayer] UpdateAnimation duration: " + baseAnimation.Duration);
                    //Debug.Log("[MRETAnimationPlayer] UpdateAnimation direction: " + Direction);
                }
            }
        }

        protected void UpdateAnimationProperties()
        {
            Name = name;
            baseAnimation.TargetObject = target;
            State = state;
            baseAnimation.Duration = duration;
            baseAnimation.Direction = direction;
            baseAnimation.WrapMode = wrapMode;
        }

        /// <summary>
        /// This function is called when the state of the animation is 
        /// changed from oldState to newState.
        /// </summary>
        /// <param name="newState"></param>
        /// <param name="oldState"></param>
        protected virtual void UpdateState(State newState, State oldState)
        {  
            if (oldState == State.Stopped)
            {
                switch (newState)
                {
                    case State.Running:
                        if (Direction == Direction.Forward)
                        {
                            baseAnimation.CurrentTime = 0;
                        }
                        else
                        {
                            baseAnimation.CurrentTime = baseAnimation.Duration;
                        }
                        break;
                    default:
                        break;
                }
            }
            // Update Unity Editor field
            state = newState;
        }

        /// <summary>
        /// Stops the animation. When the animation is stopped, it sends the StateChangedEvent, and 
        /// state() returns Stopped. The current time is not changed.
        /// </summary>
        public void StopAnimation()
        {
            State = State.Stopped;
        }

        /// <summary>
        /// Pauses the animation. When the animation is paused, state() returns Paused. The value of 
        /// currentTime will remain unchanged until resume() or start() is called. If you want to 
        /// continue from the current time, call resume().
        /// </summary>
        public void PauseAnimation()
        {
            if (State == State.Running)
            {
                State = State.Paused;
            }
        }

        /// <summary>
        /// Resumes the animation after it was paused. The currenttime is not changed.
        /// </summary>
        public void ResumeAnimation()
        {
            if (State == State.Paused)
            {
                State = State.Running;
            }
        }

        /// <summary>
        /// Starts the animation. When the animation starts, the StateChangedEvent is sent, and 
        /// state() returns Running. The animation will run, periodically calling updateCurrentTime() as 
        /// the animation progresses.
        /// </summary>
        public void StartAnimation()
        {
            State = State.Running;
        }

        /// <summary>
        /// Jump to the frame at time.
        /// </summary>
        public void JumpToTime(float time)
        {
            baseAnimation.UpdateTimeEvent(time);
        }

        /// <summary>
        /// Step back a meaningful amount of time by offsetting currentTime by some amount. Subclasses 
        /// should override to define what offset should be used. This implementation uses 
        /// offset = Duration / 10.
        /// </summary>
        public void StepBack()
        {
            baseAnimation.StepBack();
        }

        /// <summary>
        /// Step forward a meaningful amount of time by offsetting currentTime by some amount. Subclasses 
        /// should override to define what offset should be used. This implementation uses 
        /// offset = Duration / 10.
        /// </summary>
        public void StepForward()
        {
            baseAnimation.StepForward();
        }

        /// <summary>
        /// Jumps to the end of the animation CurrentTime = TotalDuration.
        /// </summary>
        public void JumpToEnd()
        {
            baseAnimation.JumpToEnd();
        }

        /// <summary>
        /// Rewinds to the start of the animation CurrentTime = 0.
        /// </summary>
        public virtual void Rewind()
        {
            baseAnimation.Rewind();
        }
    }
}
