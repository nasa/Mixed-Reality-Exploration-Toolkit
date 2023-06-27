// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace GOV.NASA.GSFC.XR.MRET.Extensions.MultimediaNotes
{
    /// <summary>
    /// This file inserts media elements into the multimedia note. The multimedia note
    /// utilizes content fitters so all that needs to be is done insert the instantiated
    /// game object under the content viewport and the content fitters handle the
    /// alignment
    /// </summary>
    public class MultimediaNote : MonoBehaviour
    {

        public GameObject imageContainer;
        public GameObject textContainer;
        public GameObject viewport;
        public GameObject videoContainer;

        //method to insert an image (used a button click event with the gallery)
        public void insertImage(Texture texture)
        {

            imageContainer.GetComponent<RawImage>().texture = texture;
            Instantiate(imageContainer, viewport.transform);
            GameObject instantiatedTextContainer = Instantiate(textContainer, viewport.transform);
            instantiatedTextContainer.transform.GetChild(0).GetComponent<TMPro.TMP_InputField>().text = "";

        }

        //method to insert a video (used a button click event with the gallery)
        public void insertVideo(string videoURL)
        {
            GameObject instantiatedVideoContainer = Instantiate(videoContainer, viewport.transform);
            instantiatedVideoContainer.GetComponent<VideoPlayer>().url = videoURL;
        }

    }
}