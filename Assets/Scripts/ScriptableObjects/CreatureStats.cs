using UnityEngine;

[CreateAssetMenu(fileName = "CreatureStats", menuName = "Data/CreatureStats", order = 4)]
public class CreatureStats : ScriptableObject
{
    [Header("Movement")]
    [Tooltip("Units per second")]
    public float maxSpeed = 10f;
    public float accelerationTime = 0.5f;
    public float decelerationTime = 1f;
    [Tooltip("Degrees per second.")]
    public float turnRate = 360f;

    [Header("Behavior")]
    public float viewRange = 10;
    public float maxRestDuration = 1;
    [Range(0, 1f)]
    public float restProbability = 0.1f;
    [Tooltip("Probability each second of escaping the held state each second.")]
    [Range(0, 1f)]
    public float escapeHeldProbability = 0f;
    [Tooltip("Time in seconds from the initial number of tasks.")]
    [Range(0, 120f)]
    public float timeUntilDissatisfied = 120f;
}
