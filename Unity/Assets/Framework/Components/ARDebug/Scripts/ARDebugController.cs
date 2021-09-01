using UnityEngine;
using UnityEngine.UI;

namespace GSFC.ARVR.MRET.Components.ARDebug
{
    /// <remarks>
    /// History:
    /// 12 May 2021: Created
    /// </remarks>
	/// <summary>
	/// ARDebugController
	/// Handles debug messages for the AR HUD.
    /// Author: Dylan Z. Baker
	/// </summary>
	public class ARDebugController : MRETBehaviour
	{
        public Text debugText;

		/// <seealso cref="MRETBehaviour.ClassName"/>
		public override string ClassName
		{
			get
			{
				return nameof(ARDebugController);
			}
		}

        private static ARDebugController instance;

        public static void Log(string debugMessage)
        {
            if (instance == null)
            {
                Debug.LogError("[ARDebugController] No class reference.");
            }
            else
            {
                instance.RecordDebugMessage(debugMessage);
            }
        }

        private void RecordDebugMessage(string debugMessage)
        {
            Debug.Log(debugMessage);
            debugText.text = debugMessage;
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

            instance = this;
		}
		
		/// <seealso cref="MRETBehaviour.MRETStart"/>
		protected override void MRETStart()
		{
			// Take the inherited behavior
			base.MRETStart();

			if (debugText != null)
            {
                debugText.text = "";
            }
		}
	}
}
