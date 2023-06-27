// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.Animation
{
    /// <remarks>
    /// History:
    /// 12 Oct 2021: Created
    /// </remarks>
	///
	/// <summary>
	/// IActionSequence
	///
	/// Describes an action sequence in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IActionSequence : IIdentifiable
    {
        /// <summary>
        /// Indicates if this action sequence will be played automatically when loaded.<br>
        /// </summary>
        public bool Autoplay { get; }

        /// <summary>
        /// Indicates the direction of this action sequence.<br>
        /// </summary>
        public ActionSequenceDirectionType Direction { get; set; }

        /// <summary>
        /// Indicates the duration in seconds for this action sequence.<br>
        /// </summary>
        public float Duration { get; set; }

        /// <summary>
        /// Indicates the wrap mode of this action sequence.<br>
        /// </summary>
        public ActionSequenceWrapModeType WrapMode { get; set; }

        /// <summary>
        /// The current time and progress of the action sequence.
        /// 
        /// The action sequence's current time starts at 0, and ends at Duration.
        /// </summary>        
        public float CurrentTime { get; set; }

        /// <summary>
        /// If this action sequence is part of a group, this function should return
        /// a reference to the group; otherwise, return null.
        /// </summary>
        public MRETAnimationGroup Group { get; set; }

        /// <summary>
        /// Called to update the action sequence's timer.
        /// </summary>
        /// <param name="timeDelta">Elapsed time in seconds</param>
        public void UpdateTimeEvent(float timeDelta);

        /// <summary>
        /// Steps back a meaningful amount of time in the action sequence.
        /// </summary>
        public void StepBack();

        /// <summary>
        /// Step forward a meaningful amount of time in the action sequence.
        /// </summary>
        public void StepForward();

        /// <summary>
        /// Jumps to the end of the action sequence.
        /// </summary>
        public void JumpToEnd();

        /// <summary>
        /// Rewinds to the start of the action sequence.
        /// </summary>
        public void Rewind();
    }
}
