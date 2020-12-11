using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    private List<MRETAnimation> recordingAnimations;

    void Start()
    {
        recordingAnimations = new List<MRETAnimation>();
    }

    public void RecordTo(MRETAnimation animationToAdd)
    {
        // Avoid duplicates.
        foreach (MRETAnimation anim in recordingAnimations)
        {
            if (anim == animationToAdd) return;
        }

        recordingAnimations.Add(animationToAdd);
    }

    public void StopRecordingTo(MRETAnimation animationToStop)
    {
        recordingAnimations.Remove(animationToStop);
    }

	public void RecordAction(BaseAction actionToRecord, BaseAction inverseAction = null)
    {
        MRETAnimation.MRETAnimationClip clip
            = new MRETAnimation.MRETAnimationClip(actionToRecord, System.DateTime.Now, inverseAction);
        
        foreach (MRETAnimation anim in recordingAnimations)
        {
            anim.AddClip(clip);
        }
    }
}