// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

namespace GOV.NASA.GSFC.XR.MRET
{
    /// <remarks>
    /// History:
    /// 4 Apr 2023: Created (J. Hosler)
    /// </remarks>
	///
	/// <summary>
	/// MRETSingleton
	///
	/// Provides an abstract implementation of a singleton class that
    /// DO NOT require update behavior.
	///
    /// Author: Jeffrey Hosler
	/// </summary>
    /// 
    /// <seealso cref="MRETUpdateSingleton{M}"/>
	/// 
	public abstract class MRETSingleton<T> : MRETBehaviour, ISingleton<T>
        where T : ISingleton
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(MRETSingleton<T>);

        /// <summary>
        /// The instance reference for this singleton
        /// </summary>
        public static T Instance { get; private set; }

        /// <seealso cref="MRETBehaviour.MRETAwake"/>
        protected sealed override void MRETAwake()
        {
            // Make sure the singleton pattern holds for this class
            if ((Instance != null) && (Instance as UnityEngine.Object) != this)
            {
                // We can only have one instance
                Destroy(this);
            }
            else
            {
                // Make sure we've been initialized
                if (Instance == null)
                {
                    // Initialize
                    Initialize();
                }

                // Take the inherited behavior
                base.MRETAwake();
            }
        }

        /// <seealso cref="ISingleton.Initialize"/>
        public virtual void Initialize()
        {
            // Assign the instance
            Instance = (T)(this as ISingleton);
        }

    }

    /// <remarks>
    /// History:
    /// 4 Apr 2023: Created (J. Hosler)
    /// </remarks>
	///
	/// <summary>
	/// MRETSingleton
	///
	/// Provides an abstract implementation of a singleton class that
    /// requires update behavior.
	///
    /// Author: Jeffrey Hosler
	/// </summary>
    /// 
    /// <seealso cref="MRETSingleton{M}"/>
	/// 
	public abstract class MRETUpdateSingleton<T> : MRETUpdateBehaviour, ISingleton<T>
        where T : ISingleton
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(MRETSingleton<T>);

        /// <summary>
        /// The instance reference for this singleton
        /// </summary>
        public static T Instance { get; private set; }

        /// <seealso cref="MRETBehaviour.MRETAwake"/>
        protected sealed override void MRETAwake()
        {
            // Make sure the singleton pattern holds for this class
            if ((Instance != null) && (Instance as UnityEngine.Object) != this)
            {
                // We can only have one instance
                Destroy(this);
            }
            else
            {
                // Make sure we've been initialized
                if (Instance == null)
                {
                    // Initialize
                    Initialize();
                }

                // Take the inherited behavior
                base.MRETAwake();
            }
        }

        /// <seealso cref="ISingleton.Initialize"/>
        public virtual void Initialize()
        {
            // Assign the instance
            Instance = (T)(this as ISingleton);
        }

    }
}
