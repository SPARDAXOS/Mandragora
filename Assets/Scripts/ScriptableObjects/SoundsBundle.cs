using System;
using UnityEngine;



[Serializable]
public struct SoundEntry {
    public string key;
    public AudioClip clip;
}

[CreateAssetMenu(fileName = "ResourcesBundle", menuName = "Data/ResourcesBundle", order = 2)]
public class SoundsBundle : ScriptableObject {
    public SoundEntry[] entries;
}
