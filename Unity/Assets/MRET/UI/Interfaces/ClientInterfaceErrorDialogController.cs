// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using GOV.NASA.GSFC.XR.MRET.Interfaces;

namespace GOV.NASA.GSFC.XR.MRET.UI.Interfaces
{
    public class ClientInterfaceErrorDialogController : ThirdPartyInterfaceErrorDialogController<IClientInterface>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(ClientInterfaceErrorDialogController);

        public enum ErrorSourceType
        {
            Client,
            Server,
            None
        }

        [SerializeField]
        [Tooltip("The source of the error. This will control the type of cert that is requested from the user.")]
        public ErrorSourceType source = ErrorSourceType.None;

        /// <summary>
        /// The source of the error. This will control the type of cert that is requested from the user.
        /// </summary>
        public ErrorSourceType Source
        {
            get => source;
            set => source = value;
        }

        public GameObject background;
        public Button clientSourceButton;
        public Button serverSourceButton;

        public void Connect()
        {
            if (thirdPartyInterface != null)
            {
                thirdPartyInterface.Connect();
            }
        }

        protected void RepositionForSource()
        {
            RectTransform rectTransform = background.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                if (thirdPartyInterface.UsesCertificates)
                {
                    rectTransform.offsetMin = new Vector2(rectTransform.offsetMin.x, -105);
                }
                else
                {
                    rectTransform.offsetMin = new Vector2(rectTransform.offsetMin.x, -155);
                }
            }
        }


        protected void EnableSource()
        {
            clientSourceButton.gameObject.SetActive(thirdPartyInterface.UsesCertificates && (source == ErrorSourceType.Client));
            serverSourceButton.gameObject.SetActive(thirdPartyInterface.UsesCertificates && (source == ErrorSourceType.Server));
        }

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            // Take the inherited behavior
            base.MRETStart();

            // Custom initialization (after deserialization)

            // Make sure we have valid properies
            if ((thirdPartyInterface == null) || (background == null) ||
                (serverSourceButton == null) || (clientSourceButton == null))
            {
                LogError("GameObjects not set", nameof(MRETStart));
            }
            else
            {
                // Reposition the buttones based upon the interface and enable the buttons
                RepositionForSource();
                EnableSource();
            }
        }
    }
}