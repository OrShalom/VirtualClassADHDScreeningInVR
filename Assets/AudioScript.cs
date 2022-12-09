using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioScript : MonoBehaviour
{
    public AudioSource ButtonAudio;

    public void OnButtonPress()
    {
        ButtonAudio.Play();
    }
}
