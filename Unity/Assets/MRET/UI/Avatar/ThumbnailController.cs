// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using GOV.NASA.GSFC.XR.XRUI.ControllerMenu;
using GOV.NASA.GSFC.XR.MRET.Avatar;

namespace GOV.NASA.GSFC.XR.MRET.UI.Avatar
{
    /// <remarks>
    /// History:
    /// 1 January 9999: Created
    /// </remarks>
	///
	/// <summary>
	/// ThumbnailController
	///
	/// TODO: Describe this class here...
	///
    /// Author: TODO
	/// </summary>
	/// 
	public class ThumbnailController : MRETBehaviour
	{
		/// <seealso cref="MRETBehaviour.ClassName"/>
		public override string ClassName
		{
			get
			{
				return nameof(ThumbnailController);
			}
		}

        public ControllerMenuPanel thumbnailPanel;

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
		protected override void MRETStart()
		{
			// Take the inherited behavior
			base.MRETStart();

            AvatarManager avatarManager = MRET.AvatarManager;
            thumbnailPanel.panelName = "AVATAR APPEARANCE";
            thumbnailPanel.Initialize(false, false, false);
            //thumbnailPanel.AddLabel("Avatar Selection:");
            for (int i=0; i<MRET.AvatarManager.avatars.Count; i++)
            {
                XR.MRET.Avatar.Avatar avatar = MRET.AvatarManager.avatars[i];
                Button button = thumbnailPanel.AddButton(avatar.name, avatar.thumbnail, null, new Vector2(75, 75), ControllerMenuPanel.ButtonSize.Small);
                int delegateIndex = i;
                button.onClick.AddListener(delegate { UpdateAvatar(delegateIndex); });
            }
        }

        public void UpdateAvatar(int index)
        {
            MRET.AvatarManager.SetActiveAvatar(index);
        }
    }
}
