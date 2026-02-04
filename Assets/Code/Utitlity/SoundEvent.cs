using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;

[CreateAssetMenu(fileName = "sound", menuName = "Audio/Sound Event", order = 1)]
public class SoundEvent : ScriptableObject
{
    [Header("Volume")]
    public float Volume = 1;
    public bool RandomVolume;
    public float VolumeMax = 1;

    [Header("Pitch")]
    public float Pitch = 1;
    public bool RandomPitch;
    public float PitchMax = 1;

    [Header("Other Bits")]
    public float SpacialBlend = 1;

    [Header("Sounds")]
    public List<AudioClip> Sounds;

    public void Apply(AudioSource source, float volumeMult = 1, float pitchMult = 1)
    {
        var rnd = new System.Random();

        source.clip = Sounds[rnd.Next(0, Sounds.Count - 1)];

        source.volume = Volume;
        if (RandomVolume)
            source.volume += UnityEngine.Random.Range(0, Volume - VolumeMax);
        source.volume *= volumeMult;

        source.pitch = Pitch;
        if (RandomPitch)
            source.pitch += UnityEngine.Random.Range(0, Pitch - PitchMax);
        source.pitch *= pitchMult;

        source.spatialBlend = SpacialBlend;
    }

    public void Play(Vector3 position = default, float volumeMult = 1, float pitchMult = 1, bool forcePlay = false)
    {
        SoundSystem.PlaySound(this, position, volumeMult, pitchMult, forcePlay);
    }
}