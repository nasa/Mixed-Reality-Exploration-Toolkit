// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using GSFC.ARVR.MRET.Common.Schemas;
using UnityEngine;

namespace GSFC.ARVR.MRET.Infrastructure.Framework.Animation
{
    /// <summary>
    /// The MRETActionAnimation class encapsulates a BaseAction and an inverse BaseAction. 
    /// When the animation is started or the currentTime is set to a value > 0 the PerformAction method 
    /// on the Action will be called. If the currentTime is set to 0 the PerformAction will be called on
    /// the Inverse action.
    /// </summary>
    public class MRETActionAnimation : MRETBaseAnimation
    {
        private BaseAction action;
        private BaseAction inverse = null;

        /// <summary>
        /// No argument constructor for MRETActionAnimation.
        /// </summary>
        public MRETActionAnimation()
        {
        }

        /// <summary>
        /// The Action property that references the BaseAction to perform by this animation.
        /// </summary>
        public BaseAction Action
        {
            get => action;
            set
            {
                action = value;
                if (action != null) TargetObject = action.gameObject;
            }
        }

        /// <summary>
        /// The Inverse property that references the BaseAction to undo this animation.
        /// </summary>
        public BaseAction Inverse { get => inverse; set => inverse = value; }

        /// <summary>
        /// Overrides inherited updateCurrentTime to perform the action or inverse action.  
        /// </summary>
        /// <param name="currentTime"></param>
        protected override void UpdateCurrentTime(float currentTime)
        {
            //Debug.Log("[MRETActionAnimation->updateCurrentTime] " + currentTime);
            if (currentTime > 0)
            {
                PerformAction(action);
            }
            else
            {
                PerformAction(inverse);
            }
        }

        // Helper function to perform the action based on type.
        private void PerformAction(BaseAction actionToPerform)
        {
            if (actionToPerform is ProjectAction)
            {
                ((ProjectAction)actionToPerform).PerformAction();
            }
            else if (actionToPerform is RigidbodyAction)
            {
                ((RigidbodyAction)actionToPerform).PerformAction();
            }
            else if (actionToPerform is ViewAction)
            {
                ((ViewAction)actionToPerform).PerformAction();
            }
            else if (actionToPerform is AnnotationAction)
            {
                ((AnnotationAction)actionToPerform).PerformAction();
            }
        }

        #region Serialization

        /// <summary>
        /// Serializes this animation into the given serialized type.
        /// </summary>
        /// <param name="type">serialized type to modify</param>
        public override void SerializeTo(AnimationBaseType type)
        {
            Debug.Log("[MRETActionAnimation->SerializeTo] ");
            base.SerializeTo(type);

            AnimationActionType actionType = type as AnimationActionType;
            if (actionType != null)
            {
                actionType.Action = Action.Serialize();
                try
                {
                    actionType.Inverse = Inverse.Serialize();
                }
                catch
                { 
                    // Inverse is optional so just go on
                }
            }
        }

        /// <summary>
        /// Returns a serialized version of this animation.
        /// </summary>
        /// <returns>an AnimationActionType instance</returns>
        public override AnimationBaseType Serialize()
        {
            Debug.Log("[MRETActionAnimation->Serialize] ");
            AnimationActionType type = new AnimationActionType();
            SerializeTo(type);

            return type;
        }

        /// <summary>
        /// Deserialize information from the given serialized representation into the new animation instance.
        /// </summary>
        /// <param name="serializedAnimation">Serialized representation of an animation</param>
        /// <param name="animation">New animation instance to initialize.</param>
        public static void DeserializeTo(AnimationActionType serializedAnimation, MRETActionAnimation animation)
        {
            Debug.Log("[MRETActionAnimation->DeserializeTo] ");
            MRETBaseAnimation.DeserializeTo(serializedAnimation, animation);

            animation.Action = BaseAction.Deserialize(serializedAnimation.Action);
            if (serializedAnimation.Inverse != null)
                animation.Inverse = BaseAction.Deserialize(serializedAnimation.Inverse);
        }

        /// <summary>
        /// Deserialize information into a new animation instance.
        /// </summary>
        /// <param name="serializedAnimation">Serialized representation of an animation</param>
        /// <returns>A new animation instance.</returns>
        public static MRETBaseAnimation Deserialize(AnimationActionType serializedAnimation)
        {
            Debug.Log("[MRETActionAnimation->Deserialize] ");
            if (serializedAnimation == null)
            {
                return null;
            }

            MRETActionAnimation animation = new MRETActionAnimation();
            DeserializeTo(serializedAnimation, animation);

            return animation;
        }

        #endregion Serialization
    }
}