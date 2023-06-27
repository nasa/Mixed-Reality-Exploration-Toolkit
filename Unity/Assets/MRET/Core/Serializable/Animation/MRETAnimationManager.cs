// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
//using UnityEditor.Animations;
using GOV.NASA.GSFC.XR.MRET.Action;
using GOV.NASA.GSFC.XR.MRET.SceneObjects;
using GOV.NASA.GSFC.XR.MRET.Schema;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.Animation
{
    /// <summary>
    /// The MRETAnimationManager provides an interface to create, interact and start animations. This 
    /// manager maintains a list of animations and a reference to a selected or active animation. The
    /// active animation is the receiver of new recorded actions and events.
    /// </summary>
    public class MRETAnimationManager : MRETUpdateManager<MRETAnimationManager>
    {
        public GameObject animationPanelPrefab;

        // Fields
        private List<MRETAnimationPlayer> players = new List<MRETAnimationPlayer>();
        private IActionSequence activeAnimation;
        //private MRETAnimationGroup recordAnimation;
        private float currentRecordTime;
        private int unqueId = 1;
        private MRETAnimationPlayer activePlayer;
        private string defaultName = "temp_";
        private float previousSystemTime;
        private bool recordTimeInitialized;


        // Event publishing

        public delegate void ActivePlayerChangeDelegate();
        public static event ActivePlayerChangeDelegate ActivePlayerChangeEvent;
        public delegate void PlayerListChangeDelegate();
        public static event PlayerListChangeDelegate PlayerListChangeEvent;

        public bool IsRecording { get; private set; }

        //public UnityEngine.Animation unitAnimation;
        public Boolean runTest = false;

        // Overridden MRETBehaviour methods

        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(MRETAnimationManager);

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

        /// <seealso cref="MRETUpdateSingleton{T}.Initialize"/>
        public override void Initialize()
        {
            // Take the inherited behavior
            base.Initialize();

            // TODO: Custom initialization (before deserialization)

        }

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            // Take the inherited behavior
            base.MRETStart();
        }

        /// <seealso cref="MRETUpdateBehaviour.MRETUpdate"/>
        protected override void MRETUpdate()
        {
            // Take the inherited behavior
            base.MRETUpdate();

            UpdateTests();
        }

        // Unity calls

        /// <summary>
        /// Creates a new empty animation group that will be set to active.
        /// </summary>
        /// <returns>a MRETAnimationGroup</returns>
        public MRETAnimationPlayer NewAnimation()
        {
            MRETAnimationGroup animation = gameObject.AddComponent<MRETAnimationGroup>();
            AddAnimation(animation);

            return SelectPlayer(animation.name);
        }

        private MRETAnimationPlayer CreatePlayer()
        {
            return gameObject.AddComponent<MRETAnimationPlayer>();
        }

        /// <summary>
        /// Property to get the active animation. 
        /// </summary>
        public IActionSequence ActiveAnimation
        {
            get { return ActivePlayer.MRETAnimation; }
        }

        /// <summary>
        /// Property to get and set the active animation player. Setting the active player will publish an
        /// ActivePlayerChangeEvent to all listeners.
        /// </summary>
        public MRETAnimationPlayer ActivePlayer
        {
            get { return activePlayer; }

            private set
            {
                activePlayer = value;
                ActivePlayerChangeEvent?.Invoke();
            }
        }

        /// <summary>
        /// Returns a read only list of animation players managed by this manager.
        /// </summary>
        public ReadOnlyCollection<MRETAnimationPlayer> Players => players.AsReadOnly();

        /// <summary>
        /// Selects the animation player with the given name to be the active player.
        /// </summary>
        /// <param name="animationName">The name of the animation</param>
        /// <returns>the selected MRETAnimationPlayer</returns>
        public MRETAnimationPlayer SelectPlayer(string animationName)
        {
            Log("SelectPlayer: Number of existing players:" + players.Count, nameof(SelectPlayer));

            // Locate the player that contains the reference to the supplied animation
            MRETAnimationPlayer result = players.Find(e => e.MRETAnimation.name.Equals(animationName));

            if (result != null)
            {
                Log("Animation found: " + result, nameof(SelectPlayer));
                ActivePlayer = result;
            }

            return result;
        }

        /// <summary>
        /// Records the given BaseAction to the current animation as a MRETActionAnimation.
        /// </summary>
        /// <param name="actionToRecord"></param>
        /// <param name="inverseAction"></param>
        public void RecordAction(IAction actionToRecord, IAction inverseAction)
        {
            Log("Recording action", nameof(RecordAction));
            if (!IsRecording)// || actionToRecord == null)
            {
                return;
            }

            // Initialize record time variable if not set
            if (!recordTimeInitialized)
            {
                recordTimeInitialized = true;
                currentRecordTime = UnityEngine.Time.time;
            }

            float timeOffset = UnityEngine.Time.time - currentRecordTime; // + activeAnimation.CurrentTime;
            currentRecordTime = UnityEngine.Time.time;
            //timeOffset = timeOffset + recordAnimation.CurrentPlayPosition;
            MRETActionAnimation clip = gameObject.AddComponent<MRETActionAnimation>();
            clip.Duration = timeOffset;
            clip.Action = actionToRecord;
            clip.Inverse = inverseAction;

            if (ActiveAnimation is MRETAnimationGroup)
            {
                ((MRETAnimationGroup)ActiveAnimation).AddAnimation(clip);
            }
            else
            {
                AddAnimation(clip);
            }
        }

        // Utility filter method for adding a player to the list.
        private bool AddPlayer(MRETAnimationPlayer player)
        {
            bool added = true;

            // Don't add duplicates either by reference or name
            if (players.Exists(e => e.Equals(player)))
            {
                Log("Player exists: " + player.id, nameof(AddPlayer));
                added = false;
            }
            else if (players.Exists(e => e.id.Equals(player.id)))
            {
                Log("Player with id already exists: " + player.id, nameof(AddPlayer));
                added = false;
            }
            else
            {
                players.Add(player);
                PlayerListChangeEvent?.Invoke();
            }

            return added;
        }

        /// <summary>
        /// Adds the given animation to the end of the active animation.
        /// </summary>
        /// <param name="animation"></param>
        /// <returns></returns>
        public bool AddAnimation(IActionSequence animation)
        {
            bool added = true;

            MRETAnimationPlayer player = CreatePlayer();
            player.MRETAnimation = animation;
            added = AddPlayer(player);

            if (added)
            {
                // TODO delete player
            }

            return added;
        }

        /// <summary>
        /// Adds and selects the given animation.
        /// </summary>
        /// <param name="animation"></param>
        public void AddSelectAnimation(IActionSequence animation)
        {
            Log("Adding animation: " + animation.name, nameof(AddSelectAnimation));
            if (AddAnimation(animation))
            {
                ActivePlayer = SelectPlayer(animation.name);
            }
        }

        /// <summary>
        /// Sets the Record flag so that future actions will be recorded.
        /// </summary>
        public void Record()
        {
            if (!IsRecording)
            {
                IsRecording = true;
                recordTimeInitialized = false;
            }
        }

        /// <summary>
        /// Stops the recording of actions to an animation.
        /// </summary>
        public void StopRecord()
        {
            if (IsRecording)
            {
                IsRecording = false;
            }
        }

        #region Serialization
        /// <summary>
        /// Loads an Animation from an XML file.
        /// </summary>
        /// <param name="filePath"></param>
        public void LoadFromXML(string filePath)
        {
            try
            {
                // Deserialize the file into our deserialized type class
                ActionSequenceType serializedActionSequence = AnimationFileSchema.FromXML(filePath) as ActionSequenceType;
                ActionSequenceBaseType serializedAnimation = serializedActionSequence.Item;

                // Create the onloaded action 
                Action<IActionSequence> OnActionSequenceLoadedAction = (IActionSequence loadedActionSequence) =>
                {
                    if (loadedActionSequence != null)
                    {
                        // Add the animation
                        AddSelectAnimation(loadedActionSequence);
                    }
                    else
                    {
                        // Error condition
                        LogError("A problem occurred attempting to instantiate the animation", nameof(LoadFromXML));
                    }
                };

                // Instantiate the animation
                MRET.ProjectManager.InstantiateObject(serializedAnimation,
                    null, MRET.ProjectManager.animationPanelsContainer.transform, OnActionSequenceLoadedAction);
            }
            catch (Exception e)
            {
                LogWarning("A problem was encountered loading the XML file: " + e.ToString(), nameof(LoadFromXML));
            }
        }

        /// <summary>
        /// Serializes an animation as XML to the supplied file.
        /// </summary>
        /// <param name="filePath">The XML file path</param>
        /// <param name="anim">The <code>IActionSequence</code> to serialize to XML</param>
        public bool SaveToXML(string filePath, IActionSequence anim)
        {
            bool result = false;

            Log("Saving animation to XML: " + anim.GetType().ToString(), nameof(SaveToXML));

            try
            {
                // Setup our serialized action which will write the result to a file if successful
                ActionSequenceType serializedAnimation = new ActionSequenceType();
                Action<bool, string> OnSerializedAction = (bool serialized, string message) =>
                {
                    if (serialized)
                    {
                        // Write out the animation XML file
                        AnimationFileSchema.ToXML(filePath, serializedAnimation);
                        result = true;
                    }
                    else
                    {
                        string logMessage = "A problem occurred serializing the animation";
                        if (!string.IsNullOrEmpty(message))
                        {
                            logMessage += ": " + message;
                        }
                        LogWarning(logMessage, nameof(SaveToXML));
                    }
                };

                // Serialize the animation
                anim.Serialize(serializedAnimation.Item, OnSerializedAction);
            }
            catch (Exception e)
            {
                LogError(e.ToString());
            }

            return result;
        }

        /// <summary>
        /// Serializes the currently active animation.
        /// </summary>
        /// <returns>The serialized AnimationType of the animation</returns>
        internal ActionSequenceType SerializeAnimation()
        {
            IActionSequence anim = ActiveAnimation;
            ActionSequenceType type = new ActionSequenceType();
            anim.Serialize(type.Item);

            return type;
        }

        /// <summary>
        /// Deserializes the given animation.
        /// </summary>
        /// <param name="type"></param>
        /// <returns>An instance of the animation</returns>
        internal IActionSequence DeserializeAnimation(ActionSequenceType serializedActionSequence)
        {
            IActionSequence animation = null;
            ActionSequenceBaseType serializedAnimation = serializedActionSequence.Item;

            // Create the serializable implementation for this animation type
            if (serializedAnimation is ActionSequenceGroupType)
            {
                // Animation group
                animation = gameObject.AddComponent<MRETAnimationGroup>();
            }
            else if (serializedAnimation is ActionSequenceFrameType)
            {
                // Animation frame
                animation = gameObject.AddComponent<MRETActionAnimation>();
            }
            else if (serializedAnimation is ActionSequencePropertyType)
            {
                // Animation property
                animation = gameObject.AddComponent<MRETPropertyAnimation>();
            }
            else
            {
                // Unknown animation type. Something's wrong
                LogWarning("Unknown animation type: " + nameof(serializedAnimation), nameof(DeserializeAnimation));
                return animation;
            }

            void DeserializeAnimationAction(bool deserialized, string message)
            {
                // Check the serialization state
                if (!deserialized)
                {
                    // A problem was encountered

                    string logMessage = "An issue occurred while deserializing the animation";
                    if (!string .IsNullOrEmpty(message))
                    {
                        logMessage += ": " + message;
                    }
                    LogWarning(logMessage, nameof(DeserializeAnimationAction));

                    // Delete the animation component we just created
                    foreach (Component childAnim in GetComponents(animation.GetType()))
                    {
                        if (childAnim.Equals(animation))
                        {
                            Destroy(childAnim);
                            animation = null;
                        }
                    }
                }
            };

            // Perform the animation deserialization
            animation.Deserialize(serializedAnimation, DeserializeAnimationAction);

            return animation;
        }
        #endregion Serialization

        #region TestCode

        // Test only
        public GameObject testObject;
        public string filePath = "..\\";
        //public string filePath = "C:\\Users\\tjames1\\Documents\\Projects\\VR\\MRET\\MRET_Core\\Unity\\Assets\\";
        public bool readTest = false;
        public string readFileName = "Numbers123456.manim";
        public bool writeTest = false;
        public string writeFileName = "Test123456.manim";
        AnimationCurve anim;
        Keyframe[] ks;
        float startTime;

        internal void UpdateTests()
        {
            if (runTest && testObject != null)
            {
                Debug.Log("[" + ClassName + "] Run Test:" + runTest);
                runTest = false;
                //TestBuiltIn2(tempObject);
                startTime = UnityEngine.Time.time;
                TestAnimation(testObject);
                //TestAnimation2(testObject);
            }

            if (readTest)
            {
                Debug.Log("[" + ClassName + "] Read Test:" + readTest);
                readTest = false;
                TestDeserialization();
            }

            if (writeTest)
            {
                Debug.Log("[" + ClassName + "] write Test:" + writeTest);
                writeTest = false;
                TestSerialization();
            }
        }

        // Method for testing reading in a xml representation of an animation.
        internal void TestDeserialization()
        {
            LoadFromXML(filePath + readFileName);
        }

        // Method for testing the serialization of an animation.
        internal void TestSerialization()
        {
            GameObject obj = Instantiate(testObject, new Vector3(3.8f, 0.4f, -2), Quaternion.identity) as GameObject;

            //MRETBaseAnimation animation = activePlayer.MRETAnimation;
            MRETAnimationGroup animGroup = obj.AddComponent<MRETAnimationGroup>();
            MRETActionAnimation animation = obj.AddComponent<MRETActionAnimation>();// activePlayer.MRETAnimation;
            ISceneObject sceneObj = obj.AddComponent<SceneObject>();

            // string partName, Vector3 pos, Quaternion rot, string guid = null
            animation.Inverse = new SceneObjectTransformAction(sceneObj);
            animation.Action = new SceneObjectTransformAction(sceneObj, new Vector3(3.8f, 1.0f, -2));
            animation.Duration = 1f;

            animGroup.AddAnimation(animation);

            animation = obj.AddComponent<MRETActionAnimation>();// activePlayer.MRETAnimation;

            // string partName, Vector3 pos, Quaternion rot, string guid = null
            animation.Action = new SceneObjectTransformAction(sceneObj, new Vector3(3.8f, 1.0f, -2));
            animation.Inverse = new SceneObjectTransformAction(sceneObj, new Vector3(3.8f, 0.4f, -2));
            animation.Duration = 1f;

            animGroup.AddAnimation(animation);

            if (animation != null) SaveToXML(filePath + writeFileName, animGroup);
            AddSelectAnimation(animGroup);
        }

        private MRETAnimationClip clip;
        UnityEngine.Animation testAnimationObj;
        private string relativePath = "";

        // Method for testing specific builtin animation functions only.
        internal void TestBuiltIn2(GameObject testTarget)
        {
            //testAnimationObj = UnityEngine.Animation.Instantiate(unitAnimation) as UnityEngine.Animation;
            //testAnimationObj = GetComponent<UnityEngine.Animation>();
            //testAnimationObj = testTarget.AddComponent<UnityEngine.Animation>();

            //GameObject obj = Instantiate(testTarget, new Vector3(4.2f, 1.75f, -2), Quaternion.identity) as GameObject;
            //tempObject = obj;
            // create a new AnimationClip
            clip = new MRETAnimationClip();
            //clip.legacy = true;

            //animation.setKeyFrameAt(0.33f, new Vector3(5, 2, 0));
            //animation.setKeyFrameAt(0.66f, new Vector3(2.25f, 2, 0));
            //animation.setKeyFrameAt(0.9f, new Vector3(5, 2, 0));

            // create a curve to move the GameObject and assign to the clip
            MRETAnimationCurve curve = new MRETAnimationCurve();
            curve.AddKey(new Keyframe(0.0f, 4.2f));
            curve.AddKey(new Keyframe(1.5f, 5f));
            curve.AddKey(new Keyframe(3.0f, 2.25f));
            curve.AddKey(new Keyframe(4.0f, 5f));
            curve.AddKey(new Keyframe(5.0f, 4.2f));

            clip.SetCurve(relativePath, typeof(Transform), "localPosition.x", curve);

            curve = new MRETAnimationCurve();
            curve.AddKey(new Keyframe(0.0f, 1.75f));
            curve.AddKey(new Keyframe(1.5f, 2f));
            curve.AddKey(new Keyframe(3.0f, 2f));
            curve.AddKey(new Keyframe(4.0f, 2f));
            curve.AddKey(new Keyframe(5.0f, 1.75f));

            clip.SetCurve(relativePath, typeof(Transform), "localPosition.y", curve);

            curve = new MRETAnimationCurve();
            curve.AddKey(new Keyframe(0.0f, -2));
            curve.AddKey(new Keyframe(1.5f, 0));
            curve.AddKey(new Keyframe(3.0f, 0));
            curve.AddKey(new Keyframe(4.0f, 0));
            curve.AddKey(new Keyframe(5.0f, -2));

            clip.SetCurve(relativePath, typeof(Transform), "localPosition.z", curve);

            // update the clip to a change the red color
            curve = MRETAnimationCurve.Linear(0.0f, 1.0f, 5.0f, 0.0f);
            clip.SetCurve(relativePath, typeof(Material), "_Color.r", curve);
            clip.Name = "Ballon";

            //testAnimationObj.AddClip(clip.AnimationClip, clip.Name);
            //testAnimationObj.clip = clip.AnimationClip;

            //AnimationClip animClip = new AnimationClip();
            //AnimationState animState = new AnimationState();
            ////Animator animator = GetComponent<Animator>();
            //Animator animator = obj.AddComponent(typeof(Animator)) as Animator;
            //var controller = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath("Assets/StateMachineTransitions.controller");
            //animator.runtimeAnimatorController = controller;
            //GetComponent<UnityEngine.Animation>().AddClip(animClip, "test Clip");
            //Debug.Log("[MRETAnimationManager->Start clip] " + animClip.ToString());
            //Debug.Log("[MRETAnimationManager->Start state] " + animState.ToString());
            //Debug.Log("[MRETAnimationManager->Start anim] " + GetComponent<UnityEngine.Animation>());
        }

        // Method for testing specific builtin animation functions only.
        internal void TestBuiltIn(GameObject testTarget)
        {
            GameObject obj = Instantiate(testTarget, new Vector3(4.2f, 1.75f, -2), Quaternion.identity) as GameObject;

            //AnimationClip animClip = new AnimationClip();
            //AnimationState animState = new AnimationState();
            ////Animator animator = GetComponent<Animator>();
            //Animator animator = obj.AddComponent(typeof(Animator)) as Animator;
            //var controller = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath("Assets/StateMachineTransitions.controller");
            //animator.runtimeAnimatorController = controller;
            //GetComponent<UnityEngine.Animation>().AddClip(animClip, "test Clip");
            //Debug.Log("[MRETAnimationManager->Start clip] " + animClip.ToString());
            //Debug.Log("[MRETAnimationManager->Start state] " + animState.ToString());
            //Debug.Log("[MRETAnimationManager->Start anim] " + GetComponent<UnityEngine.Animation>());
        }

        //private GameObjectRecorder m_Recorder;

        // Method for testing specific animation functions only.
        internal IActionSequence TestAnimation(GameObject testTarget)
        {
            GameObject obj = Instantiate(testTarget, new Vector3(3.8f, 0.4f, -2), Quaternion.identity) as GameObject;
            //obj.name = animation.Name;
            obj.SetActive(true);

            // Create recorder and record the script GameObject.
            //m_Recorder = new GameObjectRecorder(obj);
            // Bind all the Transforms on the GameObject and all its children.
            //m_Recorder.BindComponentsOfType<Transform>(gameObject, true);

            MRETPropertyAnimation animation = obj.AddComponent<MRETPropertyAnimation>();
            // create a curve to move the GameObject and assign to the clip
            MRETAnimationCurve curve = new MRETAnimationCurve();
            curve.AddKey(new Keyframe(0.0f, 3.8f));
            curve.AddKey(new Keyframe(1.5f, 5f));
            curve.AddKey(new Keyframe(3.0f, 2.25f));
            curve.AddKey(new Keyframe(5.0f, 2.8f));

            animation.SetCurve(relativePath, typeof(Transform), "localPosition.x", curve);

            curve = new MRETAnimationCurve();
            curve.AddKey(new Keyframe(0.0f, 0.4f));
            curve.AddKey(new Keyframe(1.5f, 1.5f));
            curve.AddKey(new Keyframe(3.0f, 1.5f));
            curve.AddKey(new Keyframe(5.0f, 0.5f));

            animation.SetCurve(relativePath, typeof(Transform), "localPosition.y", curve);

            curve = new MRETAnimationCurve();
            curve.AddKey(new Keyframe(0.0f, -2));
            curve.AddKey(new Keyframe(1.5f, 0));
            curve.AddKey(new Keyframe(3.0f, 0));
            curve.AddKey(new Keyframe(5.0f, -2));

            animation.SetCurve(relativePath, typeof(Transform), "localPosition.z", curve);

            // update the clip to a change the red color
            curve = MRETAnimationCurve.Linear(0.0f, 1.0f, 5.0f, 0.0f);
            animation.SetCurve(relativePath, typeof(Material), "_Color.r", curve);

            AddSelectAnimation(animation);
            //MRETPropertyAnimation anim = gameObject.AddComponent<MRETPropertyAnimation>() as MRETPropertyAnimation;

            return animation;
        }

        // Method for testing specific animation functions only.
        internal IActionSequence TestAnimation2(GameObject testTarget)
        {
            GameObject obj = Instantiate(testTarget, new Vector3(3.8f, 2f, -2), Quaternion.identity) as GameObject;
            obj.name = "Ballon 2";

            obj.SetActive(true);

            MRETPropertyAnimation animation = obj.AddComponent<MRETPropertyAnimation>();
            // create a curve to move the GameObject and assign to the clip
            MRETAnimationCurve curve = new MRETAnimationCurve();
            curve.AddKey(new Keyframe(0.0f, 3.8f));
            curve.AddKey(new Keyframe(1.5f, 5f));
            curve.AddKey(new Keyframe(3.0f, 2.25f));
            curve.AddKey(new Keyframe(4.0f, 4f));
            curve.AddKey(new Keyframe(5.0f, 3.3f));

            animation.SetCurve(relativePath, typeof(Transform), "localPosition.x", curve);

            curve = new MRETAnimationCurve();
            curve.AddKey(new Keyframe(0.0f, 2f));
            curve.AddKey(new Keyframe(1.5f, 1f));
            curve.AddKey(new Keyframe(3.0f, 1.5f));
            curve.AddKey(new Keyframe(4.0f, 1f));
            curve.AddKey(new Keyframe(5.0f, 2f));

            animation.SetCurve(relativePath, typeof(Transform), "localPosition.y", curve);

            curve = new MRETAnimationCurve();
            curve.AddKey(new Keyframe(0.0f, -2));
            curve.AddKey(new Keyframe(1.5f, 0));
            curve.AddKey(new Keyframe(3.0f, 0));
            curve.AddKey(new Keyframe(4.0f, 0));
            curve.AddKey(new Keyframe(5.0f, -2));

            animation.SetCurve(relativePath, typeof(Transform), "localPosition.z", curve);

            // update the clip to a change the red color
            curve = MRETAnimationCurve.Linear(0.0f, 1.0f, 5.0f, 0.0f);
            animation.SetCurve(relativePath, typeof(Material), "_Color.r", curve);

            AddSelectAnimation(animation);
            //MRETPropertyAnimation anim = gameObject.AddComponent<MRETPropertyAnimation>() as MRETPropertyAnimation;

            return animation;
        }
        #endregion TestCode
    }
}