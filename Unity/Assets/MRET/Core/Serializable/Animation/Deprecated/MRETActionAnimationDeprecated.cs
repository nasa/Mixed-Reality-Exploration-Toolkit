// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using GOV.NASA.GSFC.XR.MRET.Action;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_1;
using UnityEngine;

namespace GOV.NASA.GSFC.XR.MRET.Animation
{
    /// <summary>
    /// The MRETActionAnimation class encapsulates a BaseAction and an inverse BaseAction. 
    /// When the animation is started or the currentTime is set to a value > 0 the PerformAction method 
    /// on the Action will be called. If the currentTime is set to 0 the PerformAction will be called on
    /// the Inverse action.
    /// </summary>
    public class MRETActionAnimationDeprecated : MRETBaseAnimationDeprecated
    {
        private BaseActionDeprecated action;
        private BaseActionDeprecated inverse = null;

        /// <summary>
        /// No argument constructor for MRETActionAnimation.
        /// </summary>
        public MRETActionAnimationDeprecated()
        {
        }

        /// <summary>
        /// The Action property that references the BaseAction to perform by this animation.
        /// </summary>
        public BaseActionDeprecated Action
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
        public BaseActionDeprecated Inverse { get => inverse; set => inverse = value; }

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
        private void PerformAction(BaseActionDeprecated actionToPerform)
        {
            if (actionToPerform is ProjectActionDeprecated)
            {
                ((ProjectActionDeprecated)actionToPerform).PerformAction();
            }
            else if (actionToPerform is RigidbodyActionDeprecated)
            {
                ((RigidbodyActionDeprecated)actionToPerform).PerformAction();
            }
            else if (actionToPerform is ViewActionDeprecated)
            {
                ((ViewActionDeprecated)actionToPerform).PerformAction();
            }
            else if (actionToPerform is AnnotationActionDeprecated)
            {
                ((AnnotationActionDeprecated)actionToPerform).PerformAction();
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
        public static void DeserializeTo(AnimationActionType serializedAnimation, MRETActionAnimationDeprecated animation)
        {
            Debug.Log("[MRETActionAnimation->DeserializeTo] ");
            MRETBaseAnimationDeprecated.DeserializeTo(serializedAnimation, animation);

            animation.Action = BaseActionDeprecated.Deserialize(serializedAnimation.Action);
            if (serializedAnimation.Inverse != null)
                animation.Inverse = BaseActionDeprecated.Deserialize(serializedAnimation.Inverse);
        }

        /// <summary>
        /// Deserialize information into a new animation instance.
        /// </summary>
        /// <param name="serializedAnimation">Serialized representation of an animation</param>
        /// <returns>A new animation instance.</returns>
        public static MRETBaseAnimationDeprecated Deserialize(AnimationActionType serializedAnimation)
        {
            Debug.Log("[MRETActionAnimation->Deserialize] ");
            if (serializedAnimation == null)
            {
                return null;
            }

            MRETActionAnimationDeprecated animation = new MRETActionAnimationDeprecated();
            DeserializeTo(serializedAnimation, animation);

            return animation;
        }

        #endregion Serialization
    }
}