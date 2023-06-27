// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;

namespace GOV.NASA.GSFC.XR.MRET.UI.LoadingIndicator
{
    public class LoadingIndicatorManager : MRETManager<LoadingIndicatorManager>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(LoadingIndicatorManager);

        public Animator loadingAnimator;
        public Text loadingText;
        public Text detailText;

        public void ShowLoadingIndicator(string loadingMessage)
        {
            loadingText.text = loadingMessage;
            detailText.text = "";
            loadingAnimator.gameObject.SetActive(true);
            loadingText.gameObject.SetActive(true);
            detailText.gameObject.SetActive(true);
        }

        public void UpdateDetail(string detailMessage)
        {
            detailText.text = detailMessage;
        }

        public void StopLoadingIndicator()
        {
            loadingAnimator.gameObject.SetActive(false);
            loadingText.gameObject.SetActive(false);
            detailText.gameObject.SetActive(false);
        }
    }
}