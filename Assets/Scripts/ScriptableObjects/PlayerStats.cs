using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "PlayerStats", menuName = "Data/PlayerStats", order = 1)]

public class PlayerStats : ScriptableObject
{
    [Header("Movement")]
    [Range(1.0f, 20.0f)] public float maxSpeed = 1.0f;
    [Range(1.0f, 8.0f)] public float turnRate = 1.0f;
    
    [Header("Acceleration")]
    [Range(1.0f, 200.0f)] public float accelerationRate = 2.0f;
    [Range(1.0f, 200f)] public float decelerationRate = 1.0f;
   

}    
