// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using GOV.NASA.GSFC.XR.CrossPlatformInputSystem;
using GOV.NASA.GSFC.XR.MRET.Integrations.Matlab.IK;
using GOV.NASA.GSFC.XR.MRET.Extensions.Ros.IK;

namespace GOV.NASA.GSFC.XR.MRET.IK
{
    /// <remarks>
    /// History:
    /// 24 February 2021: Created
    /// </remarks>
	///
	/// <summary>
	/// IKManager
	///
	/// Handles IK modes for all of MRET.
	///
    /// Author: Dylan Z. Baker
	/// </summary>
	/// 
	public class IKManager : MRETManager<IKManager>
    {
        /// <summary>
        /// Key for IK parameter.
        /// </summary>
        public static readonly string ikKey = "MRET.INTERNAL.TOOLS.IK";

        /// <summary>
        /// An enumeration for IK modes.
        /// </summary>
        public enum IKMode { None, Basic, Matlab }

        /// <summary>
        /// The current IK mode.
        /// </summary>
        public IKMode currentMode
        {
            get
            {
                return (IKMode) MRET.DataManager.FindPoint(ikKey);
            }
            private set
            {
                MRET.DataManager.SaveValue(ikKey, value);
            }
        }

		/// <seealso cref="MRETBehaviour.ClassName"/>
		public override string ClassName => nameof(IKManager);

        /// <seealso cref="MRETManager{M}.Initialize"/>
        public override void Initialize()
        {
            // Take the inherited behavior
            base.Initialize();

            MRET.DataManager.SaveValue(ikKey, IKMode.None);
        }

        /// <summary>
        /// Set the IK mode.
        /// </summary>
        /// <param name="modeToSet">The mode to set.</param>
        public void SetIKMode(IKMode modeToSet)
        {
            foreach (InputHand hand in MRET.InputRig.hands)
            {
                IKInteractionManager ikManager = hand.GetComponent<IKInteractionManager>();
                if (ikManager != null)
                {
                    ikManager.enabled = modeToSet == IKMode.Basic;
                }

                MatlabIKInteractionManager mIKManager = hand.GetComponent<MatlabIKInteractionManager>();
                if (mIKManager != null)
                {
                    mIKManager.enabled = modeToSet == IKMode.Matlab;
                }
            }

            currentMode = modeToSet;
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
	}
}
