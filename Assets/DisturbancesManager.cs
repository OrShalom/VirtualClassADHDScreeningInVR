using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class DisturbancesManager : MonoBehaviour
{
    public List<AudioSource> Sounds;
    private (float min, float max) epochRange;
    private float epoch;
    private int soundIndex = 0;
    UnityEvent disturbanceEvent;
    bool stop = false;
    public List<int> TimesOfDisturbances;
    IEnumerator AudioCoroutine()
    {
        while (!stop)
        {
            Random.InitState(System.DateTime.Now.Millisecond);
            epoch = Random.Range(epochRange.min, epochRange.max + 1);
            
            Console.WriteLine(epoch);
            if (TimesOfDisturbances.Count > 0) TimesOfDisturbances.Add(TimesOfDisturbances[TimesOfDisturbances.Count - 1] + (int)epoch);
            else TimesOfDisturbances.Add((int)epoch);
            yield return new WaitForSeconds(epoch);
            if (!stop)
            {
                disturbanceEvent.Invoke();
            }
            else if (TimesOfDisturbances.Count > 0)
            {
                TimesOfDisturbances.RemoveAt(TimesOfDisturbances.Count - 1);
            }
        }
    }

    void Start()
    {
        disturbanceEvent = new UnityEvent();
        epoch = 3.0f;
        disturbanceEvent.AddListener(PlayDisturbance);
    }

    private void PlaySound()
    {
        Sounds[1].Play();
        Random.InitState(DateTime.Now.Millisecond);
        soundIndex = Random.Range(0, Sounds.Count);
    }

    internal void PlayDisturbance()
    {
        PlaySound();
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