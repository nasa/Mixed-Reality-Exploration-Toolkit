// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GOV.NASA.GSFC.XR.Utilities.Transforms;
using GOV.NASA.GSFC.XR.XRUI.Keyboard;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.Action;
using GOV.NASA.GSFC.XR.MRET.Collaboration;
using GOV.NASA.GSFC.XR.MRET.Configuration;
using GOV.NASA.GSFC.XR.MRET.Project;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects
{
    /// <remarks>
    /// History:
    /// 10 Sep 2021: Created
    /// </remarks>
	///
	/// <summary>
	/// InteractableDisplay
	///
	/// Provides common functionality for any interactable display
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public class InteractableDisplay<T> : InteractableSceneObject<T>, IInteractableDisplay<T>
        where T : DisplayType, new()
    {
		/// <seealso cref="MRETBehaviour.ClassName"/>
		public override string ClassName => nameof(InteractableDisplay<T>);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private T serializedDisplay;

        public GameObject fullDisplay, minimizedDisplay;
        private string lastRecordedTitle = "";
        public bool titleTextBeingAutoChanged = false;

        #region IInteractable
        /// <seealso cref="IInteractable.Visible"/>
        public override bool Visible
        {
            get
            {
                // Use the state to determine which display we are checking
                return (State == DisplayStateType.Maximized) ?
                    ((fullDisplay != null) && fullDisplay.activeSelf) :
                    ((minimizedDisplay != null) && minimizedDisplay.activeSelf);
            }
            set
            {
                // Use the state to determine which display we are affecting
                if (fullDisplay != null)
                {
                    fullDisplay.SetActive((State == DisplayStateType.Maximized) && value);
                }
                if (minimizedDisplay != null)
                {
                    minimizedDisplay.SetActive((State == DisplayStateType.Minimized) && value);
                }
            }
        }
        #endregion IInteractable

        #region IInteractableDisplay
        /// <seealso cref="IInteractableDisplay.CreateSerializedType"/>
        DisplayType IInteractableDisplay.CreateSerializedType() => CreateSerializedType();

        public VR_InputField titleTextField;
        public string Title
        {
            get
            {
                return (titleTextField != null) ? titleTextField.text : "";
            }
            set
            {
                if (titleTextField != null)
                {
                    titleTextField.text = value;
                }
            }
        }

        /// <seealso cref="IInteractableDisplay.Width"/>
        public float Width => (fullDisplay != null) ?
            TransformUtil.GetSize(fullDisplay).x : InteractableDisplayDefaults.WIDTH;

        /// <seealso cref="IInteractableDisplay.Height"/>
        public float Height => (fullDisplay != null) ?
            TransformUtil.GetSize(fullDisplay).y : InteractableDisplayDefaults.HEIGHT;

        /// <seealso cref="IInteractableDisplay.Zorder"/>
        public DisplayStateType State
        {
            get
            {
                return fullDisplay.activeSelf ? DisplayStateType.Maximized : DisplayStateType.Minimized;
            }
            set
            {
                if (value == DisplayStateType.Maximized)
                {
                    Maximize();
                }
                else
                {
                    Minimize();
                }
            }
        }

        /// <seealso cref="IInteractableDisplay.Zorder"/>
        public int Zorder { get; set; }

        public Color DisplayColor
        {
            get
            {
                return minimizedDisplay.GetComponent<Image>().color;
            }
            set
            {
                minimizedDisplay.GetComponent<Image>().color = value;
                if (titleTextField != null)
                {
                    titleTextField.transform.parent.GetComponent<Image>().color = value;
                }
            }
        }

        /// <seealso cref="IInteractableDisplay.Deserialize(DisplayType, Action{bool, string})"/>
        void IInteractableDisplay.Deserialize(DisplayType serialized, Action<bool, string> onFinished)
        {
            Deserialize(serialized as T, onFinished);
        }

        /// <seealso cref="IInteractableDisplay.Serialize(DisplayType, Action{bool, string})"/>
        void IInteractableDisplay.Serialize(DisplayType serialized, Action<bool, string> onFinished)
        {
            Serialize(serialized as T, onFinished);
        }
        #endregion IInteractableDisplay

        #region MRETUpdateBehaviour
        /// <seealso cref="MRETBehaviour.IntegrityCheck"/>
        protected override IntegrityState IntegrityCheck()
		{
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            return (
                (state == IntegrityState.Failure) ||
                (fullDisplay == null) ||
                (minimizedDisplay == null)
                    ? IntegrityState.Failure   // Fail is base class fails or anything is null
                    : IntegrityState.Success); // Otherwise, our integrity is valid
		}

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            base.MRETStart();

            // Assign the full display if not assigned
            if (fullDisplay == null)
            {
                fullDisplay = gameObject;
            }

            // Setup title change listener
            if (titleTextField != null)
            {
                titleTextField.onValueChanged.AddListener(delegate { CaptureTitleChange(); });
            }
        }

        /// <seealso cref="MRETBehaviour.MRETUpdate"/>
        protected override void MRETUpdate()
        {
            base.MRETUpdate();

            //HandleScaleManagement();
        }
        #endregion MRETUpdateBehaviour

        #region Serializable
        /// <seealso cref="InteractableSceneObject{T}.Deserialize(T, SerializationState)"/>
        protected override IEnumerator Deserialize(T serialized, SerializationState deserializationState)
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
            serializedDisplay = serialized;

            // TODO: Race condition? What happens if parent is not loaded yet?
            if (!string.IsNullOrEmpty(serialized.ParentID))
            {
                IIdentifiable parentObject = MRET.UuidRegistry.GetByID(serialized.ParentID);
                if (parentObject is ISceneObject)
                {
                    transform.SetParent((parentObject as ISceneObject).transform);
                }
            }

            // Deserialize the display
            titleTextBeingAutoChanged = true;
            Title = serializedDisplay.Title;
            titleTextBeingAutoChanged = false;
            State = serializedDisplay.State;

            // Deserialize the width and height
            if ((serializedDisplay.Width != default) &&
                (serializedDisplay.Height != default))
            {
                Vector2 size = new Vector2(serializedDisplay.Width, serializedDisplay.Height);
                // TODO: We should be able to set the size but it needs more thought
                // gameObject.transform.localScale = TransformUtil.GetSizeAsScale(fullDisplay, size);
            }
            // Deserialize the Zorder
            Zorder = serializedDisplay.Zorder;

            // Record the deserialization state as complete
            deserializationState.complete = true;

            yield return null;
        }

        /// <seealso cref="InteractableSceneObject{T}.Serialize(T, SerializationState)"/>
        protected override IEnumerator Serialize(T serialized, SerializationState serializationState)
        {
            StartCoroutine(base.Serialize(serialized, serializationState));

            // Wait for the coroutine to complete
            while (!serializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) yield break;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization
            if (parent != null)
            {
                serialized.ParentID = parent.id;
            }

            // Serialize the display
            serialized.Title = Title;
            serialized.State = State;

            // Serialize the width and height
            serialized.Width = Width;
            serialized.Height = Height;

            // Serialize the Zorder
            serialized.Zorder = Zorder;

            // Save the final serialized reference
            serializedDisplay = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;

            yield return null;
        }
        #endregion Serializable

        /// <seealso cref="Identifiable{T}.CreateSynchronizedObject"/>
        protected override ISynchronized CreateSynchronizedObject()
        {
            return gameObject.AddComponent<SynchronizedDisplay>();
        }

        public void Minimize()
        {
            if (fullDisplay != null) fullDisplay.SetActive(false);
            if (minimizedDisplay != null) minimizedDisplay.SetActive(true);
        }

        public void Maximize()
        {
            if (minimizedDisplay != null) minimizedDisplay.SetActive(false);
            if (fullDisplay != null) fullDisplay.SetActive(true);
        }

        // Catpture the title changes as actions
        private void CaptureTitleChange()
        {
            if (!titleTextBeingAutoChanged)
            {
                ProjectManager.UndoManager.AddAction(
                    new IdentifiableObjectUpdateAction(
                        this,
                        new Dictionary<string, string>()
                        {
                            { nameof(Title), Title },
                        }),
                    new IdentifiableObjectUpdateAction(
                        this,
                        new Dictionary<string, string>()
                        {
                            { nameof(Title), lastRecordedTitle },
                        }));
                lastRecordedTitle = Title;
            }
        }

        #region ScaleManagement
        public float minScale = 0.1f, maxScale = 1f;
        private bool minimizedRescaled = false;

        void HandleScaleManagement()
        {
            if (fullDisplay)
            {
                fullDisplay.transform.localScale =
                    new Vector3(
                        1 / ProjectManager.Project.ScaleMultiplier,
                        1 / ProjectManager.Project.ScaleMultiplier,
                        1 / ProjectManager.Project.ScaleMultiplier);
            }

            if (minimizedDisplay)
            {
                if (ProjectManager.Project.ScaleMultiplier < minScale)
                {
                    minimizedDisplay.transform.localScale =
                        new Vector3(
                            minScale / ProjectManager.Project.ScaleMultiplier,
                            minScale / ProjectManager.Project.ScaleMultiplier,
                            minScale / ProjectManager.Project.ScaleMultiplier);
                    minimizedRescaled = true;
                }
                else if (minimizedRescaled)
                {
                    minimizedDisplay.transform.localScale = new Vector3(1, 1, 1);
                    minimizedRescaled = false;
                }

                if (ProjectManager.Project.ScaleMultiplier > maxScale)
                {
                    minimizedDisplay.transform.localScale =
                        new Vector3(
                            maxScale / ProjectManager.Project.ScaleMultiplier,
                            maxScale / ProjectManager.Project.ScaleMultiplier,
                            maxScale / ProjectManager.Project.ScaleMultiplier);
                }
            }
        }
        #endregion

        #region Material Adjustment
        private Color initialTitleColor;

        protected override void SaveObjectMaterials(bool includeChildInteractables = false)
        {
            base.SaveObjectMaterials(includeChildInteractables);

            // Save the title color
            if (titleTextField != null)
            {
                initialTitleColor = titleTextField.colors.normalColor;
            }
        }

        protected override void RestoreObjectMaterials()
        {
            base.RestoreObjectMaterials();

            // Restore the title color
            if (titleTextField != null)
            {
                ColorBlock newTCB = titleTextField.colors;
                newTCB.normalColor = initialTitleColor;
                titleTextField.colors = newTCB;
            }
        }

        protected override void ReplaceObjectMaterials(Material matToUse, bool includeChildInteractables = false)
        {
            base.ReplaceObjectMaterials(matToUse, includeChildInteractables);

            // Replace the title color
            if (titleTextField != null)
            {
                ColorBlock newTCB = titleTextField.colors;
                newTCB.normalColor = matToUse.color;
                titleTextField.colors = newTCB;
            }
        }
        #endregion // Material Adjustment
    }

    /// <summary>
    /// Used to keep the default values from the schema in sync
    /// </summary>
    public class InteractableDisplayDefaults : InteractableDefaults
    {
        public static readonly float HEIGHT = new DisplayType().Height;
        public static readonly float WIDTH = new DisplayType().Width;
    }
}
