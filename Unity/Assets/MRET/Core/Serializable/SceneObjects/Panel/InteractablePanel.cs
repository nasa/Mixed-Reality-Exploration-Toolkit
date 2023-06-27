// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.Configuration;
using GOV.NASA.GSFC.XR.MRET.HUD;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Feed;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Panel
{
    /// <remarks>
    /// History:
    /// 30 Sep 2021: Created
    /// </remarks>
	///
	/// <summary>
	/// InteractablePanel
	///
	/// Defines a display associated with a feed source
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public class InteractablePanel : InteractableDisplay<PanelType>, IInteractablePanel<PanelType>
    {
		/// <seealso cref="MRETBehaviour.ClassName"/>
		public override string ClassName => nameof(InteractablePanel);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private PanelType serializedPanel;

        // TODO: Access should move to MRET or the ProjectManager
        private HudManager hudManager;

        #region IInteractablePanel
        /// <seealso cref="IInteractablePanel.feedSource"/>
        public FeedSource feedSource { get; private set; }
        #endregion IInteractablePanel

        #region MRETUpdateBehaviour
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
            base.MRETStart();

            // Obtain the HudManager reference
            if (hudManager == null)
            {
                hudManager = MRET.HudManager;
            }
        }
        #endregion MRETUpdateBehaviour

        #region Serializable
        /// <seealso cref="InteractableDisplay{T}.Deserialize(T, SerializationState)"/>
        protected override IEnumerator Deserialize(PanelType serialized, SerializationState deserializationState)
        {
            StartCoroutine(base.Deserialize(serialized, deserializationState));

            // Wait for the coroutine to complete
            while (!deserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (deserializationState.IsError) yield break;

            // Start with an incomplete indicator
            deserializationState.Clear();

            // Process this object specific deserialization

            // Save the serialized reference
            serializedPanel = serialized;

            // Deserialize the panel
            if (serializedPanel.Item is FeedType)
            {
                FeedType serializedFeed = serializedPanel.Item as FeedType;

                // Make sure we have a Feed
                feedSource = gameObject.GetComponent<FeedSource>();
                if (feedSource == null)
                {
                    feedSource = gameObject.AddComponent<FeedSource>();
                }

                // Deserialize the feed
                SerializationState feedDeserializationState = new SerializationState();
                StartCoroutine(feedSource.DeserializeWithLogging(serializedFeed, feedDeserializationState));

                // Wait for the coroutine to complete
                while (!feedDeserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // Record the state
                deserializationState.Update(feedDeserializationState);

                // If the parent failed, there's no point in continuing
                if (deserializationState.IsError) yield break;
            }
            else if (serializedPanel.Item is string)
            {
                string feedId = serializedPanel.Item as string;

                // Destroy our internal feed if we have one
                feedSource = gameObject.GetComponent<FeedSource>();
                if (feedSource == null)
                {
                    Destroy(feedSource);
                }

                // Feed is an ID reference
                feedSource = hudManager.FindFeed(feedId);
                if (feedSource == null)
                {
                    // Record the error
                    deserializationState.Error("Feed ID reference was invalid");

                    // Abort
                    yield break;
                }
            }

            // Record the deserialization state as complete
            deserializationState.complete = true;

            yield return null;
        }

        /// <seealso cref="InteractableDisplay{T}.Serialize(T, SerializationState)"/>
        protected override IEnumerator Serialize(PanelType serialized, SerializationState serializationState)
        {
            StartCoroutine(base.Serialize(serialized, serializationState));

            // Wait for the coroutine to complete
            while (!serializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) yield break;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization

            // Serialize the panel
            FeedSource internalFeedSource = gameObject.GetComponent<FeedSource>();
            if (internalFeedSource == null)
            {
                // This is a global feed reference
                if (feedSource != null)
                {
                    serialized.Item = feedSource.id;
                }
                else
                {
                    // Error condition
                    serializationState.Error("Panel feed source is not properly initialized");
                    yield break;
                }
            }
            else
            {
                // This is an internal feed source
                var serializedFeed = feedSource.CreateSerializedType();

                // Serialize the feed
                SerializationState feedSerializationState = new SerializationState();
                StartCoroutine(feedSource.SerializeWithLogging(serializedFeed, feedSerializationState));

                // Wait for the coroutine to complete
                while (!feedSerializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

                // Record the state
                serializationState.Update(feedSerializationState);

                // If the parent failed, there's no point in continuing
                if (serializationState.IsError) yield break;

                // Store the serialized feed
                serialized.Item = serializedFeed;
            }

            // Save the final serialized reference
            serializedPanel = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;

            yield return null;
        }
        #endregion Serializable

    }
}
