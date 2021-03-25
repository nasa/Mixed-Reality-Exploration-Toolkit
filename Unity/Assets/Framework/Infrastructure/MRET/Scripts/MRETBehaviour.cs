// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;

namespace GSFC.ARVR.MRET
{
    public enum IntegrityState
    {
        Failure = 0,
        Success = 1
    };

    /// <remarks>
    /// History:
    /// 2 November 2020: Created
    /// </remarks>
    ///
    /// <summary>
    /// MRETBehaviour
    /// 
    /// The base class for all MRET infrastructure/components.This class provides common
    /// functionality for all MRET scripts, including integrity checks. This class does not
    /// handle periodic updating.<br>
    ///
    /// Author: Jeffrey C. Hosler
    /// </summary>
    /// 
    public abstract class MRETBehaviour : MonoBehaviour
    {
        /// <summary>
        /// The name of this class. Subclasses should override this accessor for logging purposes.
        /// 
        /// Example:
        /// <code>
        ///     public override string ClassName
        ///     {
        ///         get
        ///         {
        ///             return nameof(MyMRETBehaviourSubclass);
        ///         }
        ///     }
        /// </code>
        /// </summary>
        public abstract string ClassName
        {
            get;
        }

        /// <value>Indicates that the integrity of this class is valid</value>
        public IntegrityState IntegrityValid
        {
            get
            {
                return IntegrityCheck();
            }
        }

        /// <summary>
        /// Called to check the integrity of this class. Any required state of this class, including
        /// property settings should be checked in this function. For each invalid state that is encountered,
        /// an error message <c>Debug.LogError()</c> should be logged with the appropriate description of
        /// the issue and the appropriate return value should be returned.
        /// </summary>
        /// 
        /// <returns>A <code>IntegrityState</code> value indicating integrity state of this component</returns>
        /// 
        /// <example>
        /// <c>
        ///     IntegrityState state = IntegrityState.Success;
        ///     
        ///     if (myProperty1 == NULL)
        ///     {
        ///         Debug.LogError(NAME, "Required property is NULL: " + myProperty1);
        ///         state = IntegrityState.Failure;
        ///     }
        ///     if (myProperty2 == NULL)
        ///     {
        ///         Debug.LogError(NAME, "Required property is NULL: " + myProperty2);
        ///         state = IntegrityState.Failure;
        ///     }
        ///     
        ///     return state;
        /// </c>
        /// </example>
        /// 
        protected virtual IntegrityState IntegrityCheck()
        {
            return IntegrityState.Success;
        }

        /// <summary>
        /// Called by the <code>Awake</code> method as part of the MRETBehaviour initialization process.
        /// This method is available for subclasses to perform a one-time self-initialisation (e.g. creating
        /// component references and initialising variables) before another script attempts to access
        /// the values. Guaranteed to be called only once and before any <code>MRETStart</code> methods are
        /// called. This method is called even is the attached component is disabled.<br>
        /// </summary>
        /// 
        /// <see cref="MRETStart"/>
        /// 
        protected virtual void MRETAwake()
        {
#if MRET_DEBUG
            Debug.Log("[" + ClassName + "->" + nameof(MRETAwake) + "; " + name + "] executing...");
#endif
        }

        /// <summary>
        /// Called by Unity as part of the MonoBehavior instantiation process to perform a one-time
        /// self-initialization (e.g. creating component references and initialising variables) before
        /// another script attempts to access the values. Guaranteed to be called only once and before
        /// any <code>Start</code> methods are called and only after all game objects are created.
        /// This method is called even is the attached component is disabled.<br>
        /// 
        /// For MRET, this method performs some common self-initialization and then calls <code>MRETAwake</code>
        /// for subclasses to perform their own self-initialization procedures. As a result, this method
        /// is thereby defined to discourage subclasses from overriding the behavior. Subclasses should
        /// override the MRETAwake method to perform self-initialization procedures.<br>
        /// </summary>
        /// 
        /// <see cref="MRETAwake"/>
        /// 
        protected void Awake()
        {
            // Perform the MRET awake
            MRETAwake();
        }

        /// <summary>
        /// Called by the <code>Start</code> method as part of the MRETBehaviour initialization process.
        /// This method can be used to create references to other game objects and their components and is
        /// guaranteed to be called only once, before any <code>MRETUpdate</code> methods are called and only
        /// after all <code>MRETAwake</code> methods are called. This method not called if the attached
        /// component is disabled.<br>
        /// </summary>
        /// 
        /// <see cref="MRETAwake"/>
        /// 
        protected virtual void MRETStart()
        {
#if MRET_DEBUG
            Debug.Log("[" + ClassName + "->" + nameof(MRETStart) + "; " + name + "] executing...");
#endif
        }

        /// <summary>
        /// Called by Unity as part of the MonoBehavior initialization process. This method can be used to
        /// create references to other game objects and their components and is guaranteed to be called only
        /// once, before any <code>Update</code> methods are called and only after all <code>Awake</code>
        /// methods are called. This method not called if the attached component is disabled.<br>
        /// 
        /// For MRET, this method performs some common game object and component references and then calls
        /// <code>MRETStart</code> for subclasses to perform their own references. As a result, this method
        /// is thereby defined to discourage subclasses from overriding the behavior. Subclasses should
        /// override the <code>MRETStart</code> method to perform self-initialization procedures.<br>
        /// </summary>
        /// 
        /// <see cref="MRETStart"/>
        /// 
        protected void Start()
        {
            // Perform the MRET start
            MRETStart();
        }
    }

}