using System;
using UnityEngine;



[Serializable]
public struct SoundEntry {
    public string key;
    public AudioClip clip;
    [Range(0.0f, 1.0f)] public float volume;
    [Range(0.0f, 2.0f)] public float maxPitch;
    [Range(0.0f, 2.0f)] public float minPitch;
}

[CreateAssetMenu(fileName = "SoundsBundle", menuName = "Data/SoundsBundle", order = 4)]
public class SoundsBundle : ScriptableObject {
    public SoundEntry[] entries;
}
