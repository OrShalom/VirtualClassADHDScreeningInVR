using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class CoroutineAudio : MonoBehaviour
{
    public AudioSource ButtonAudio;
    public List<AudioSource> sounds;
    private int soundIndex = 0;
    UnityEvent audioEvent;

    IEnumerator AudioCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(3.0f);
            audioEvent.Invoke();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        audioEvent = new UnityEvent();
        audioEvent.AddListener(PlayDisturbance);
        StartCoroutine(AudioCoroutine());
    }

    void PlaySound()
    {
        sounds[soundIndex].Play();
        Random.InitState(System.DateTime.Now.Millisecond);
        soundIndex = Random.Range(0, sounds.Count);
    }


    void PlayDisturbance()
    {
        bool isAudio = Random.value > 0.5;
        if (isAudio) PlaySound();
    }

}