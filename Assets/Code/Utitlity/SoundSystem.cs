using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;

public class SoundSystem : MonoBehaviour
{
    public int Sounds = 50;

    public List<AudioSource> Sources = new();

    public static SoundSystem Instance;

    public static int CurrentIndex = 0;

    void Start()
    {
        CurrentIndex = 0;
        Instance = this;

        for (int i = 0; i < Sounds; i++)
        {
            var gameObject = new GameObject("AudioSource");
            gameObject.transform.SetParent(transform);
            Sources.Add(gameObject.AddComponent<AudioSource>());
        }
    }

    public static AudioSource GetSource()
    {
        if (CurrentIndex >= Instance.Sources.Count)
            CurrentIndex = 0;

        var source = Instance.Sources[CurrentIndex];
        CurrentIndex++;

        return source;
    }

    public static void PlaySound(SoundEvent soundEvent, Vector3 position = default, float volumeMult = 1, float pitchMult = 1, bool forcePlay = false)
    {
        var source = GetSource();

        if (source.isPlaying && !forcePlay)
            return;

        source.Stop();
        soundEvent.Apply(source, volumeMult, pitchMult);
        source.transform.position = position;
        source.Play();
    }
}
