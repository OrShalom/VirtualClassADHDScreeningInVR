using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class DisturbancesManager : MonoBehaviour
{
    List<AudioSource> Sounds;
    public List<Animator> Animations;
    private (float min, float max) epochRange;
    private float epoch;
    private int soundIndex = 0;
    private int animationIndex = 0;
    bool stop = false;
    [NonSerialized]
    public List<List<int>> TimesOfDisturbances;
    [NonSerialized]
    public List<string> disturbancesTypes;

    private void Start()
    {
        Sounds = new List<AudioSource>(GetComponents<AudioSource>());
    }

    IEnumerator DisturbancesCoroutine()
    {
        while (!stop)
        {
            Random.InitState(System.DateTime.Now.Millisecond);
            epoch = Random.Range(epochRange.min, epochRange.max + 1);

            if (TimesOfDisturbances.Count > 0)
            {
                int lastDisturbanceTime = TimesOfDisturbances.Last().Last();// The last time of the last disturbance.

                TimesOfDisturbances.Add(new List<int>() { lastDisturbanceTime + (int)epoch });
            }
            else TimesOfDisturbances.Add(new List<int>() { (int)epoch });
            yield return new WaitForSecondsRealtime(epoch - 1);
            if (!stop)
            {
                var disturbanceLength = PlayDisturbance();
                List<int> currentDisturbTimes = TimesOfDisturbances.Last();
                int firstTimeOfDist = currentDisturbTimes.Last();
                for (int i = 1; i < disturbanceLength; i++)
                {
                    currentDisturbTimes.Add(firstTimeOfDist + i);
                }
            }
            else if (TimesOfDisturbances.Count > 0)
            {
                TimesOfDisturbances.RemoveAt(TimesOfDisturbances.Count - 1);
            }
        }
    }

    private int PlaySound()
    {
        Random.InitState(DateTime.Now.Millisecond);
        soundIndex = Random.Range(0, Sounds.Count);
        Sounds[soundIndex].Play();
        return (int)Math.Max(1, Math.Ceiling(Sounds[soundIndex].clip.length));
    }

    internal int PlayDisturbance()
    {
        bool isSound = Random.Range(0, 2) == 0;
        if (isSound)
        {
            disturbancesTypes.Add("Audio");
            return PlaySound();
        }
        else
        {
            disturbancesTypes.Add("Visual");
            return PlayAnimation();
        }
    }

    private int PlayAnimation()
    {
        Random.InitState(DateTime.Now.Millisecond);
        animationIndex = Random.Range(0, Animations.Count);
        Animations[animationIndex].Play("StartAnimate");
        return (int)Math.Max(1, Math.Ceiling(Animations[animationIndex].GetCurrentAnimatorStateInfo(0).length));
    }

    internal void StartDisturbances(float minEpoch, float maxEpoch = -1f)
    {
        epochRange.min = minEpoch;
        epochRange.max = maxEpoch != -1 ? maxEpoch : minEpoch;
        stop = false;
        TimesOfDisturbances = new List<List<int>>();
        disturbancesTypes = new List<string>();
        StartCoroutine(DisturbancesCoroutine());
    }

    internal void Stop()
    {
        stop = true;
    }
}