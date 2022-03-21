using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GSFC.ARVR.MRET.Infrastructure.Framework.Part
{
    /// <remarks>
    /// History:
    /// 18 February 2022: Created
    /// </remarks>
    /// <summary>
    /// PartTelemetryPanelController is a class that controls a part
    /// telemetry configuration panel.
    /// Author: Dylan Z. Baker
    /// </summary>
	public class PartTelemetryPanelController : MRETBehaviour
	{
        /// <summary>
        /// Maximum number of points to display on the text.
        /// </summary>
        private static readonly int maxPointsDisplayed = 7;

		/// <seealso cref="MRETBehaviour.ClassName"/>
		public override string ClassName
		{
			get
			{
				return nameof(PartTelemetryPanelController);
			}
		}

        /// <summary>
        /// Text field for outputting the list to.
        /// </summary>
        [Tooltip("Text field for outputting the list to.")]
        public Text listText;

        /// <summary>
        /// Point name input.
        /// </summary>
        [Tooltip("Point name input.")]
        public InputField pointInput;

        /// <summary>
        /// Toggle for limit display.
        /// </summary>
        [Tooltip("Toggle for limit display.")]
        public Toggle displayToggle;

        /// <summary>
        /// The part being controlled.
        /// </summary>
        private InteractablePart part;

        /// <summary>
        /// Saved points to display in the list.
        /// </summary>
        private List<string> savedPoints = new List<string>();

		/// <seealso cref="MRETBehaviour.IntegrityCheck"/>
		protected override IntegrityState IntegrityCheck()
		{
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            return (
                (state == IntegrityState.Failure || listText == null
                || pointInput == null || displayToggle == null)
                    ? IntegrityState.Failure   // Fail is base class fails or anything is null
                    : IntegrityState.Success); // Otherwise, our integrity is valid
		}

		/// <seealso cref="MRETBehaviour.MRETAwake"/>
		protected override void MRETAwake()
		{
			// Take the inherited behavior
			base.MRETAwake();

            part = GetComponentInParent<InteractablePart>();
            if (part == null)
            {
                Debug.LogWarning("[PartTelemetryPanelController] Part not assigned.");
            }
		}
		
        /// <summary>
        /// Adds a point to the part.
        /// </summary>
		public void AddPoint()
        {
            if (string.IsNullOrEmpty(pointInput.text))
            {
                return;
            }

            if (part == null)
            {
                return;
            }

            part.AddDataPoint(pointInput.text);
            AddPointToDisplay(pointInput.text);
            pointInput.text = "";
        }

        /// <summary>
        /// Toggles the limit display setting.
        /// </summary>
        public void ToggleLimitDisplay()
        {
            if (part == null)
            {
                return;
            }

            part.shadeForLimitViolations = displayToggle.isOn;
        }

        /// <summary>
        /// Adds a point to the text display.
        /// </summary>
        /// <param name="point"></param>
        private void AddPointToDisplay(string point)
        {
            if (savedPoints == null)
            {
                savedPoints = new List<string>();
            }

            if (savedPoints.Count >= maxPointsDisplayed)
            {
                savedPoints.RemoveAt(0);
            }

            savedPoints.Add(point);

            listText.text = "";
            foreach (string savedPoint in savedPoints)
            {
                listText.text = savedPoint + "\n" + listText.text;
            }
        }
	}
}