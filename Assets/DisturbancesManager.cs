using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class DisturbancesManager : MonoBehaviour
{
    public List<AudioSource> Sounds;
    public List<Animator> Animations;
    private (float min, float max) epochRange;
    private float epoch;
    private int soundIndex = 0;
    private int animationIndex = 0;
    bool stop = false;
    public List<int> TimesOfDisturbances;

    IEnumerator AudioCoroutine()
    {
        while (!stop)
        {
            Random.InitState(System.DateTime.Now.Millisecond);
            epoch = Random.Range(epochRange.min, epochRange.max + 1);

            //Console.WriteLine(epoch);
            if (TimesOfDisturbances.Count > 0)
            {
                TimesOfDisturbances.Add(TimesOfDisturbances[TimesOfDisturbances.Count - 1] + (int)epoch);
            }
            else TimesOfDisturbances.Add((int)epoch);
            yield return new WaitForSecondsRealtime(epoch-1);
            if (!stop)
            {
                var disturbanceLength = PlayDisturbance();
                for (int i = 1; i < disturbanceLength; i++)
                {
                    TimesOfDisturbances.Add(TimesOfDisturbances[TimesOfDisturbances.Count - 1] + 1);
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
        bool isSound = false;// Random.Range(0, 2) == 0;
        if (isSound) return PlaySound();
        else return PlayAnimation();
    }

    private int PlayAnimation()
    {
        Random.InitState(DateTime.Now.Millisecond);
        animationIndex = 2;// Random.Range(0, Animations.Count);
        Animations[animationIndex].Play("StartAnimate");
        return (int)Math.Max(1, Math.Ceiling(Animations[animationIndex].GetCurrentAnimatorStateInfo(0).length));
    }

    internal void StartDisturbances(float minEpoch, float maxEpoch = -1f)
    {
        epochRange.min = minEpoch;
        epochRange.max = maxEpoch != -1 ? maxEpoch : minEpoch;
        stop = false;
        TimesOfDisturbances = new List<int>();
        StartCoroutine(AudioCoroutine());
    }

    internal void Stop()
    {
        stop = true;
    }
}