using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "PlayerStats", menuName = "Data/PlayerStats", order = 1)]

public class PlayerStats : ScriptableObject
{
    [Header("Movement")]
    [Range(1.0f, 1000.0f)] public float maxSpeed = 1.0f;
    [Range(1.0f, 12.0f)] public float turnRate = 1.0f;
    [Range(1.0f, 1000.0f)] public float accelerationRate = 2.0f;
    [Range(1.0f, 1000.0f)] public float decelerationRate = 1.0f;

    [Header("Knockback")]
    public bool customGravity = false;
    [Range(0.0f, 100.0f)] public float gravityScale = 1.0f;
    [Range(0.0f, 100.0f)] public float knockbackForce = 20.0f;
    [Range(0.0f, 90.0f)] public float knockbackHeightAngle = 1.0f;
    [Range(0.0f, 100.0f)] public float knockbackCreatureDropForce = 5.0f;

    [Header("Dash")]
    [Range(1.0f, 200.0f)] public float dashSpeed = 10.0f;
    [Tooltip("Length of the dash in seconds")][Range(0.1f, 10.0f)] public float dashLength = 1.0f;
    [Range(0.0f, 2.0f)] public float dashCooldown = 1.0f;
    [Range(0.0f, 1.0f)] public float retainedSpeed = 1.0f;
    [Range(1.0f, 2.0f)] public float knockbackMultiplier = 1.5f;
    [Range(0.0f, 100.0f)] public float dashCreatureDropForce = 5.0f;
    [Range(1.0f, 10.0f)] public float stunDuration = 3.5f;

    [Header("Throw")]
    [Range(0.0f, 100.0f)] public float throwForce = 20.0f;
    [Range(0.0f, 90.0f)] public float throwHeightAngle = 1.0f;


}    
