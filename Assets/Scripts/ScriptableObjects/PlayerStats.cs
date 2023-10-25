using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "PlayerStats", menuName = "Data/PlayerStats", order = 1)]

public class PlayerStats : ScriptableObject
{
    [Header("Movement")]
    [Range(1.0f, 1000.0f)] public float maxSpeed = 1.0f;
    [Range(1.0f, 12.0f)] public float turnRate = 1.0f;
    
    [Header("Acceleration")]
    [Range(1.0f, 1000.0f)] public float accelerationRate = 2.0f;
    [Range(1.0f, 1000.0f)] public float decelerationRate = 1.0f;

    [Header("Effects")]
    public bool customGravity = false;
    [Range(0.0f, 100.0f)] public float gravityScale = 1.0f;
    [Range(0.0f, 100.0f)] public float knockbackForce = 20.0f;
    [Range(0.0f, 90.0f)] public float knockbackHeightAngle = 1.0f;


}    
