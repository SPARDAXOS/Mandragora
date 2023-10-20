using System;
using UnityEngine;


[Serializable]
public struct EntityEntry {
    public string name;
    public GameObject resource;
}


[CreateAssetMenu(fileName = "ResourcesBundle", menuName = "Data/ResourcesBundle", order = 2)]
public class ResourcesBundle : ScriptableObject {
    public EntityEntry[] entries;
}
