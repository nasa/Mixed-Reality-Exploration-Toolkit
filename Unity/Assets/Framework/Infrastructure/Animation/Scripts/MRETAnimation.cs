using System;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using UnityEngine;
using GSFC.ARVR.MRET.Common.Schemas;

public class MRETAnimation
{
    public class MRETAnimationClip
    {
        public BaseAction action;
        public BaseAction inverse;
        public DateTime startTime;
        //public float duration;

        //public MRETAnimationClip(BaseAction act, DateTime start, float dur)
        public MRETAnimationClip(BaseAction act, DateTime start, BaseAction inv = null)
        {
            action = act;
            inverse = inv;
            startTime = start;
            //duration = dur;
        }

        public AnimationClipType Serialize()
        {
            try
            {
                return new AnimationClipType()
                {
                    StartTime = startTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds,
                    Action = action.Serialize(),
                    Inverse = inverse.Serialize()
                };
            }
            catch (Exception e)
            {
                return new AnimationClipType()
                {
                    StartTime = startTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds,
                    Action = action.Serialize()
                };
            }
        }
    }

    public string animationName = "default";
    public int numberOfClips
    {
        get
        {
            return sequence.Count;
        }
    }
    public bool loop;
    public bool autoplay;

    private List<MRETAnimationClip> sequence = new List<MRETAnimationClip>();

    public static MRETAnimation LoadFromXML(string filePath)
    {
        // Deserialize Animation File.
        XmlSerializer ser = new XmlSerializer(typeof(AnimationType));
        XmlReader reader = XmlReader.Create(filePath);
        try
        {
            MRETAnimation animToReturn = Deserialize((AnimationType) ser.Deserialize(reader));
            reader.Close();
            return animToReturn;
        }
        catch (Exception e)
        {
            Debug.Log("[UnityProject->LoadFromXML] " + e.ToString());
            reader.Close();
        }
        return null;
    }

    public static void SaveToXML(string filePath, MRETAnimation anim, bool loop = false, bool autoplay = false)
    {
        // Serialize to an Animation File.
        XmlSerializer ser = new XmlSerializer(typeof(AnimationType));
        XmlWriter writer = XmlWriter.Create(filePath);
        try
        {
            ser.Serialize(writer, anim.Serialize(loop, autoplay));
            writer.Close();
        }
        catch (Exception e)
        {
            Debug.Log("[MRETAnimation->SaveToXML] " + e.ToString());
            writer.Close();
        }
    }

    public AnimationType Serialize(bool loop = false, bool autoplay = false)
    {
        AnimationType serialized = new AnimationType();

        serialized.Name = animationName;

        List<AnimationClipType> serializedClips = new List<AnimationClipType>();
        foreach (MRETAnimationClip clip in sequence)
        {
            serializedClips.Add(clip.Serialize());
        }

        serialized.Sequence = serializedClips.ToArray();

        serialized.Loop = loop;
        serialized.Autoplay = autoplay;

        return serialized;
    }

    public static MRETAnimation Deserialize(AnimationType serializedAnimation)
    {
        if (serializedAnimation == null)
        {
            return null;
        }

        if (serializedAnimation.Name == null)
        {
            return null;
        }

        if (serializedAnimation.Name == "")
        {
            return null;
        }

        MRETAnimation deserialized = new MRETAnimation(serializedAnimation.Name);
        if (serializedAnimation.Sequence == null)
        {
            return null;
        }

        foreach (AnimationClipType sClip in serializedAnimation.Sequence)
        {
            deserialized.AddClip(new MRETAnimationClip(BaseAction.Deserialize(sClip.Action),
                new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(sClip.StartTime),
                (sClip.Inverse == null) ? null : BaseAction.Deserialize(sClip.Inverse)));
        }

        deserialized.loop = serializedAnimation.Loop;
        deserialized.autoplay = serializedAnimation.Autoplay;

        return deserialized;
    }

    public MRETAnimation(string nameToGive)
    {
        animationName = nameToGive;
    }

	public void InitializeSequence()
    {
        sequence = new List<MRETAnimationClip>();
    }

    public void AddClip(MRETAnimationClip clipToAdd)
    {
        if (sequence == null)
        {
            // Sequence structure needs to be initialized.
            InitializeSequence();
        }

        if (sequence.Count == 0)
        {
            // This will be the first element in the sequence, so just put it there.
            sequence.Add(clipToAdd);
        }
        else
        {
            if (clipToAdd.startTime < sequence[0].startTime)
            {
                // Update this clip's duration.
                //clipToAdd.duration = (sequence[0].startTime
                //    - clipToAdd.startTime).Milliseconds * 1000f;

                // Truncate any clips that this overlaps with.
                //TruncateOverlappingClips(clipToAdd);

                // Place clip at beginning.
                sequence.Insert(0, clipToAdd);

            }
            else if (clipToAdd.startTime > sequence[sequence.Count - 1].startTime)
                //.AddSeconds(sequence[sequence.Count - 1].duration))
            {
                // Update previous clip's duration.
                //sequence[sequence.Count - 1].duration = (clipToAdd.startTime
                //    - sequence[sequence.Count - 1].startTime).Milliseconds * 1000f;

                // Place clip at end.
                sequence.Add(clipToAdd);
            }
            else
            {
                // Find correct index.
                int indexToPlaceAt = FindIndexForTime(clipToAdd);
                int previousIndex = indexToPlaceAt - 1;
                int nextIndex = indexToPlaceAt + 1;

                // Update previous clip's duration.
                //sequence[previousIndex].duration = (clipToAdd.startTime
                //    - sequence[previousIndex].startTime).Milliseconds * 1000f;

                // Set this clip's duration.
                //clipToAdd.duration = (sequence[nextIndex].startTime
                //    - clipToAdd.startTime).Milliseconds * 1000f;

                // Truncate any clips that this overlaps with.
                //TruncateOverlappingClips(clipToAdd);

                // Place clip in between the two clips it belongs.
                sequence.Insert(indexToPlaceAt, clipToAdd);
            }
        }
    }

    public void AddClip(BaseAction actionToAdd, DateTime startTime, BaseAction inverseAction = null)
    {
        AddClip(new MRETAnimationClip(actionToAdd, startTime, inverseAction));
    }

    public int GetIndexOfStartTime(DateTime startTime)
    {
        for (int i = 0; i < sequence.Count; i++)
        {
            if (sequence[i].startTime > startTime)
            {
                return i;
            }
        }

        return 0;
    }

    public MRETAnimationClip[] GetSequenceStartingAtIndex(int indexToStartAt)
    {
        MRETAnimationClip[] returnArray = new MRETAnimationClip[sequence.Count - indexToStartAt];
        sequence.CopyTo(returnArray, indexToStartAt);
        return returnArray;
    }

    private int FindIndexForTime(MRETAnimationClip clipToAdd)
    {
        for (int i = 0; i < sequence.Count; i++)
        {
            if (sequence[i].startTime > clipToAdd.startTime)
            {
                return i;
            }
        }

        return sequence.Count;
    }

    /*private void TruncateOverlappingClips(MRETAnimationClip clip)
    {
        for (int i = 0; i < sequence.Count; i++)
        {
            // If this clip starts after the one of interest.
            if (sequence[i].startTime > clip.startTime)
            {
                // Check if the start time of the clip is before the
                // end time of the one of interest.
                if (clip.startTime.AddSeconds(clip.duration) > sequence[i].startTime)
                {
                    if (clip.startTime.AddSeconds(clip.duration) >
                        sequence[i].startTime.AddSeconds(sequence[i].duration))
                    {
                        // If this clip is completely covered by the one of interest, delete it.
                        sequence.RemoveAt(i--);
                    }
                    else
                    {
                        // Otherwise, move the start time to after the other clip's end time.
                        sequence[i].startTime = clip.startTime.AddSeconds(clip.duration + 0.001f);
                    }
                }
            }
        }
    }*/
}