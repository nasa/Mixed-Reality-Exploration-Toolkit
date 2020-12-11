using UnityEngine;
using UnityEngine.UI;

public class AnimationMenuController : MonoBehaviour
{
    public MRETAnimation activeAnimation
    {
        get
        {
            return currentAnimation;
        }
    }

    public Text titleText;
    public Toggle loopToggle, autoplayToggle;
    public Slider playbackSlider;
    
    private MRETAnimation currentAnimation;
    private AnimationManager animationManager;
    private bool isRecording, isPlayingBack, isBeingDragged = false, animationSet = false, autoplayInitialized = false;
    private int sliderDelayTimer = 0;
    private System.DateTime pauseStart, pauseEnd;

	void Start()
    {
        animationManager = FindObjectOfType<AnimationManager>();

        if (!animationSet)
        {
            SetAnimation(new MRETAnimation("default_animation"));
        }

        if (autoplayToggle.isOn)
        {
            StartPlayingAt(0);
        }
    }

    void Update()
    {
        HandlePlayback();

        if (!isPlayingBack)
        {
            if (autoplayToggle.isOn && !autoplayInitialized)
            {
                isPlayingBack = true;
                StartPlayingAt(0);
                autoplayInitialized = true;
            }
        }
    }
	
    public void SetAnimation(MRETAnimation animationToSet)
    {
        animationSet = true;
        currentAnimation = animationToSet;
        isRecording = isPlayingBack = false;
        titleText.text = animationToSet.animationName;
    }

	public void ToggleRecording()
    {
        if (isRecording)
        {
            StopRecording();
        }
        else
        {
            StartRecording();
        }
    }

    public void TogglePlayback()
    {
        if (isPlayingBack)
        {
            pauseStart = System.DateTime.Now;
            StopPlaying();
        }
        else
        {
            isBeingDragged = false;
            pauseEnd = System.DateTime.Now;
            StartPlayingAt(currentPlaybackIndex);
        }
    }

    public void StartRecording()
    {
        StopPlaying();

        animationManager.RecordTo(currentAnimation);
        isRecording = true;
    }

    public void StopRecording()
    {
        animationManager.StopRecordingTo(currentAnimation);
        isRecording = false;
    }

    public void StartPlayingAt(int index)
    {
        StopRecording();

        if (pauseStart != null && pauseEnd != null)
        {
            playbackIterationStart += pauseEnd - pauseStart;
        }
        
        if (playbackSequence == null || playbackSequence.Length == 0)
        {
            playbackSequence
            = currentAnimation.GetSequenceStartingAtIndex(index);
            currentPlaybackIndex = 0;
        }
        else
        {
            currentPlaybackIndex = index;
        }

        isPlayingBack = true;
    }

    public void StartPlayingAt(System.DateTime timeToStart)
    {
        StopRecording();

        if (pauseStart != null && pauseEnd != null)
        {
            playbackIterationStart += pauseEnd - pauseStart;
        }

        if (playbackSequence == null || playbackSequence.Length == 0)
        {
            playbackSequence
            = currentAnimation.GetSequenceStartingAtIndex(currentAnimation.GetIndexOfStartTime(timeToStart));
            currentPlaybackIndex = 0;
        }
        else
        {
            int oldPlaybackIndex = (currentPlaybackIndex > playbackSequence.Length)
                ? playbackSequence.Length - 1 : currentPlaybackIndex;
            currentPlaybackIndex = currentAnimation.GetIndexOfStartTime(timeToStart);
            playbackIterationStart = System.DateTime.Now -
                (timeToStart - playbackSequence[0].startTime);

            // Update states.
            if (oldPlaybackIndex > currentPlaybackIndex)
            {
                UndoAnimation();
                for (int i = 0; i <= currentPlaybackIndex; i++)
                {
                    PerformAction(playbackSequence[i].action);
                }
            }
            else
            {
                for (int i = oldPlaybackIndex; i <= currentPlaybackIndex; i++)
                {
                    PerformAction(playbackSequence[i].action);
                }
            }
        }
        
        isPlayingBack = true;
    }

    public void StopPlaying()
    {
        isPlayingBack = false;
    }

    public void StartDragging()
    {
        isBeingDragged = true;
    }

    public void StopDragging()
    {
        isBeingDragged = false;
    }

    public void SliderValueChanged()
    {
        if (isBeingDragged)
        {
            if (sliderDelayTimer++ < 16)
            {
                return;
            }

            if (playbackSequence != null && playbackSequence.Length > 0)
            {
                System.DateTime startofEpoch = new System.DateTime(1970, 1, 1, 0, 0, 0);
                System.TimeSpan diffTime = playbackSequence[playbackSequence.Length - 1].startTime
                    - playbackSequence[0].startTime;
                System.TimeSpan startTimeEpoch = playbackSequence[0].startTime - startofEpoch;
                System.DateTime timeToStart = startofEpoch.AddMilliseconds(
                    playbackSlider.value * diffTime.TotalMilliseconds) + startTimeEpoch;
                StopPlaying();
                StartPlayingAt(timeToStart);
            }

            sliderDelayTimer = 0;
        }
    }

#region Playback
    private int currentPlaybackIndex = 0;
    private MRETAnimation.MRETAnimationClip[] playbackSequence;
    private System.DateTime playbackIterationStart;
    void HandlePlayback()
    {
        if (isPlayingBack && currentPlaybackIndex == 0)
        {
            UndoAnimation();
            playbackIterationStart = System.DateTime.Now;
        }

        if (isPlayingBack && playbackSequence != null)
        {
            if (!isBeingDragged && playbackSequence.Length > 0)
            {
                if (currentPlaybackIndex >= playbackSequence.Length)
                {
                    if (loopToggle.isOn)
                    {
                        currentPlaybackIndex = 0;
                    }
                    else
                    {
                        isPlayingBack = false;
                    }
                }
                else
                {
                    if (System.DateTime.Now - playbackIterationStart >=
                        playbackSequence[currentPlaybackIndex].startTime - playbackSequence[0].startTime)
                    {
                        PerformAction(playbackSequence[currentPlaybackIndex++].action);
                    }
                }
            }

            if (!isBeingDragged && playbackSequence.Length > 0)
            {
                //Debug.Log("setting slider");
                playbackSlider.value = (float) ((System.DateTime.Now - playbackIterationStart).TotalMilliseconds /
                    (playbackSequence[playbackSequence.Length - 1].startTime - playbackSequence[0].startTime).TotalMilliseconds);
            }
        }
    }

    void UndoAnimation()
    {
        MRETAnimation.MRETAnimationClip[] clips = currentAnimation.GetSequenceStartingAtIndex(0);
        for (int i = clips.Length - 1; i >= 0; i--)
        {
            PerformAction(clips[i].inverse);
        }
    }

    void PerformAction(BaseAction actionToPerform)
    {
        if (actionToPerform is ProjectAction)
        {
            ((ProjectAction) actionToPerform).PerformAction();
        }
        else if (actionToPerform is RigidbodyAction)
        {
            ((RigidbodyAction) actionToPerform).PerformAction();
        }
        else if (actionToPerform is ViewAction)
        {
            ((ViewAction) actionToPerform).PerformAction();
        }
        else if (actionToPerform is AnnotationAction)
        {
            ((AnnotationAction) actionToPerform).PerformAction();
        }
    }
#endregion
}