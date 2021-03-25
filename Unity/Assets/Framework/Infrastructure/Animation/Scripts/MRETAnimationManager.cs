// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using GSFC.ARVR.MRET.Common.Schemas;
using System;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
//using UnityEditor.Animations;

namespace GSFC.ARVR.MRET.Infrastructure.Framework.Animation
{
    /// <summary>
    /// The MRETAnimationManager provides an interface to create, interact and start animations. This 
    /// manager maintains a list of animations and a reference to a selected or active animation. The
    /// active animation is the receiver of new recorded actions and events.
    /// </summary>
    public class MRETAnimationManager : MRETBehaviour
    {
        // Fields
        private List<MRETAnimationPlayer> players = new List<MRETAnimationPlayer>();
        private MRETBaseAnimation activeAnimation;
        //private MRETAnimationGroup recordAnimation;
        private float currentRecordTime;
        private int unqueId = 1;
        private MRETAnimationPlayer activePlayer;
        private String defaultName = "temp_";
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
        public override string ClassName
        {
            get
            {
                return nameof(MRETAnimationManager);
            }
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
        }

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            // Take the inherited behavior
            base.MRETStart();
        }

        // Unity calls

        /// <summary>
        /// Repeatedly sends the updateEvent to the active animation if the animation is in a running state.
        /// </summary>
        void Update()
        {
            UpdateTests();
        }

        /// <summary>
        /// Creates a new empty animation group that will be set to active.
        /// </summary>
        /// <returns>a MRETAnimationGroup</returns>
        public MRETAnimationPlayer NewAnimation()
        {
            String unqueName = defaultName + unqueId++;

            while (players.Exists(e => e.Name.Equals(unqueName)))
            {
                unqueName = defaultName + unqueId++;
            }

            MRETAnimationGroup animation = new MRETAnimationGroup() { Name = unqueName };
            AddAnimation(animation);

            return SelectPlayer(animation.Name);
        }

        private MRETAnimationPlayer createPlayer()
        {
            return gameObject.AddComponent<MRETAnimationPlayer>() as MRETAnimationPlayer;
        }

        /// <summary>
        /// Property to get the active animation. 
        /// </summary>
        public MRETBaseAnimation ActiveAnimation
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
        /// <param name="name"></param>
        /// <returns>the selected MRETAnimationPlayer</returns>
        public MRETAnimationPlayer SelectPlayer(string name)
        {
            Debug.Log("[MRETAnimationManager] SelectPlayer: size:" + players.Count);
            Debug.Log("[MRETAnimationManager] SelectPlayer: name:" + name);
            foreach (MRETAnimationPlayer player in players)
            {
                Debug.Log("[MRETAnimationManager] SelectPlayer: player Name:" + player.Name);
                Debug.Log("[MRETAnimationManager] SelectPlayer: player name:" + player.name);
            }

            MRETAnimationPlayer result = players.Find(e => e.Name.Equals(name));

            if (result != null)
            {
                Debug.Log("[MRETAnimationManager] SelectAnimation: animation found:" + result);
                ActivePlayer = result;
            }

            return result;
        }

        /// <summary>
        /// Records the given BaseAction to the current animation as a MRETActionAnimation.
        /// </summary>
        /// <param name="actionToRecord"></param>
        /// <param name="inverseAction"></param>
        public void RecordAction(BaseAction actionToRecord, BaseAction inverseAction)
        {
            Debug.Log("[MRETAnimationManager] RecordAction:" + actionToRecord);
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
            MRETActionAnimation clip = new MRETActionAnimation();
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
                Debug.Log("[MRETAnimationManager] AddPlayer: player exists:" + player.Name);
                added = false;
            }
            else if (players.Exists(e => e.Name.Equals(player.Name)))
            {
                Debug.Log("[MRETAnimationManager] AddPlayer: player with name already exists:" + player.Name);
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
        public bool AddAnimation(MRETBaseAnimation animation)
        {
            bool added = true;

            MRETAnimationPlayer player = createPlayer();
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
        public void AddSelectAnimation(MRETBaseAnimation animation)
        {
            Debug.Log("[MRETAnimationManager] AddSelectAnimation:" + animation.Name);
            if (AddAnimation(animation))
            {
                ActivePlayer = SelectPlayer(animation.Name);
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
        /// Returns an Animation from an XML file.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>a MRETBaseAnimation</returns>
        public MRETBaseAnimation LoadFromXML(string filePath)
        {
            Debug.Log("[MRETAnimationManager->LoadFromXML] " + filePath);
            // Deserialize Animation File.
            MRETBaseAnimation animToReturn = null;
            XmlReader reader = XmlReader.Create(filePath);
            XmlSerializer ser = new XmlSerializer(typeof(AnimationType));

            try
            {
                AnimationType type = (AnimationType)ser.Deserialize(reader);

                animToReturn = DeserializeAnimation(type);
                reader.Close();
            }
            catch (Exception e)
            {
                Debug.Log("[MRETAnimationManager->LoadFromXML] " + e.ToString());
                reader.Close();
            }

            return animToReturn;
        }

        /// <summary>
        /// Serializes an Animation to the given file.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="anim"></param>
        public void SaveToXML(string filePath, MRETBaseAnimation anim)
        {
            Debug.Log("[MRETAnimationManager->SaveToXML] " + anim.GetType().ToString());

            // Serialize to an Animation File.
            XmlWriter writer = XmlWriter.Create(filePath);

            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(AnimationType));
                AnimationType type = new AnimationType
                {
                    Item = anim.Serialize()
                };

                ser.Serialize(writer, type);
                writer.Close();
            }
            catch (Exception e)
            {
                Debug.Log("[MRETAnimationManager->SaveToXML] " + e.ToString());
                writer.Close();
            }
        }

        /// <summary>
        /// Serializes the currently active animation.
        /// </summary>
        /// <returns>The serialized AnimationType of the animation</returns>
        internal AnimationType SerializeAnimation()
        {
            MRETBaseAnimation anim = ActiveAnimation;
            AnimationType type = new AnimationType
            {
                Item = anim.Serialize()
            };

            return type;
        }

        /// <summary>
        /// Deserializes the given animation.
        /// </summary>
        /// <param name="type"></param>
        /// <returns>An instance of the animation</returns>
        internal MRETBaseAnimation DeserializeAnimation(AnimationType type)
        {
            MRETBaseAnimation animToReturn = null;
            AnimationBaseType animType = type.Item;

            if (animType.GetType() == typeof(AnimationGroupType))
            {
                animToReturn = MRETAnimationGroup.Deserialize((AnimationGroupType)animType);
            }
            else if (animType.GetType() == typeof(AnimationActionType))
            {
                animToReturn = MRETActionAnimation.Deserialize((AnimationActionType)animType);
            }
            else if (animType.GetType() == typeof(AnimationPropertyType))
            {
                animToReturn = MRETPropertyAnimation.Deserialize((AnimationPropertyType)animType);
            }

            return animToReturn;
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
        private int nameIncrement = 0;
        AnimationCurve anim;
        Keyframe[] ks;
        float startTime;

        internal void UpdateTests()
        {
            if (runTest && testObject != null)
            {
                Debug.Log("[MRETAnimationManager] Run Test:" + runTest);
                runTest = false;
                //TestBuiltIn2(tempObject);
                startTime = UnityEngine.Time.time;
                TestAnimation(testObject);
                //TestAnimation2(testObject);
            }

            if (readTest)
            {
                Debug.Log("[MRETAnimationManager] Read Test:" + readTest);
                readTest = false;
                TestDeserialization();
            }

            if (writeTest)
            {
                Debug.Log("[MRETAnimationManager] write Test:" + writeTest);
                writeTest = false;
                TestSerialization();
            }
        }

        // Method for testing reading in a xml representation of an animation.
        internal void TestDeserialization()
        {
            MRETBaseAnimation animation = LoadFromXML(filePath + readFileName);
            if (animation != null) AddSelectAnimation(animation);
        }

        // Method for testing the serialization of an animation.
        internal void TestSerialization()
        {
            //MRETBaseAnimation animation = activePlayer.MRETAnimation;
            MRETAnimationGroup animGroup = new MRETAnimationGroup();
            animGroup.Name = "Test" + nameIncrement.ToString();
            nameIncrement++;

            MRETActionAnimation animation = new MRETActionAnimation();// activePlayer.MRETAnimation;
            GameObject obj = Instantiate(testObject, new Vector3(3.8f, 0.4f, -2), Quaternion.identity) as GameObject;

            // string partName, Vector3 pos, Quaternion rot, string guid = null
            animation.Action = ProjectAction.MoveObjectAction(obj.name, new Vector3(3.8f, 1.0f, -2), Quaternion.identity);
            animation.Inverse = ProjectAction.MoveObjectAction(obj.name, new Vector3(3.8f, 0.4f, -2), Quaternion.identity);
            animation.TargetObject = obj;
            animation.Duration = 1f;
            animation.Name = obj.name;

            animGroup.AddAnimation(animation);

            animation = new MRETActionAnimation();// activePlayer.MRETAnimation;

            // string partName, Vector3 pos, Quaternion rot, string guid = null
            animation.Action = ProjectAction.MoveObjectAction(obj.name, new Vector3(3.8f, 1.0f, -2), Quaternion.identity);
            animation.Inverse = ProjectAction.MoveObjectAction(obj.name, new Vector3(3.8f, 0.4f, -2), Quaternion.identity);
            animation.TargetObject = obj;
            animation.Duration = 1f;
            animation.Name = obj.name + nameIncrement.ToString();

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
        internal MRETBaseAnimation TestAnimation(GameObject testTarget)
        {
            GameObject obj = Instantiate(testTarget, new Vector3(3.8f, 0.4f, -2), Quaternion.identity) as GameObject;
            //obj.name = animation.Name;
            obj.SetActive(true);

            // Create recorder and record the script GameObject.
            //m_Recorder = new GameObjectRecorder(obj);
            // Bind all the Transforms on the GameObject and all its children.
            //m_Recorder.BindComponentsOfType<Transform>(gameObject, true);

            MRETPropertyAnimation animation = new MRETPropertyAnimation();
            animation.TargetObject = obj;
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
            animation.Name = obj.name;

            AddSelectAnimation(animation);
            //MRETPropertyAnimation anim = gameObject.AddComponent<MRETPropertyAnimation>() as MRETPropertyAnimation;

            return animation;
        }

        // Method for testing specific animation functions only.
        internal MRETBaseAnimation TestAnimation2(GameObject testTarget)
        {
            GameObject obj = Instantiate(testTarget, new Vector3(3.8f, 2f, -2), Quaternion.identity) as GameObject;
            obj.name = "Ballon 2";

            obj.SetActive(true);

            MRETPropertyAnimation animation = new MRETPropertyAnimation();
            animation.TargetObject = obj;
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
            animation.Name = obj.name;

            AddSelectAnimation(animation);
            //MRETPropertyAnimation anim = gameObject.AddComponent<MRETPropertyAnimation>() as MRETPropertyAnimation;

            return animation;
        }
        #endregion TestCode
    }
}