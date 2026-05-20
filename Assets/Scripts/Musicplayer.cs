using JetBrains.Annotations;
using UnityEngine;

    //Plays intro followed by loop when intro is done
public class Musicplayer : MonoBehaviour
{
    public AudioSource introsource, loopsource;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        introsource.Play();
        loopsource.PlayScheduled(AudioSettings.dspTime + introsource.clip.length);
    }
}
