using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "PlayerStats", menuName = "Data/PlayerStats", order = 1)]
public class PlayerStats : ScriptableObject {

    [Range(1.0f, 10.0f)] public float turnRate = 120.0f;
    [Range(1.0f, 999.0f)] public float maxSpeed = 260.0f;
    [Range(1.0f, 999.0f)] public float accelerationRate = 20.0f;
    [Range(1.0f, 999.0f)] public float decelerationRate = 10.0f;

}
