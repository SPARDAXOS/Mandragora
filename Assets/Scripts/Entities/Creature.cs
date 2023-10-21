using System;
using UnityEngine;

[Serializable]
public struct CreatureStats
{
    [Header("Movement")]
    [Tooltip("Units per second")]
    public float maxSpeed;
    public float accelerationTime;
    public float decelerationTime;
    [Tooltip("Degrees per second.")]
    public float turnRate;

    [Header("Behavior")]
    [Range(0, 1f)]
    public float shyness;
    public float viewRange;

    [Header("Variables")]
    [Range(0, 1f)]
    public float satisfaction;
}
public enum CreatureState
{
    GROUNDED,
    HELD,
    INACTIVE
}

[RequireComponent(typeof(Rigidbody))]
public class Creature : MonoBehaviour
{
    private bool initialized;

    private Rigidbody rigidbodyComp = null;

    private CreatureState state;
    [SerializeField] private CreatureStats stats;
    private float accelerationIncrement;
    private float decelerationIncrement;

    private Vector3 direction = Vector3.forward;
    private Vector3 targetDirection = Vector3.forward;

    private float speed;
    private Vector3 velocity = Vector3.zero;


    void Start()
    {
        Initialize();
    }

    void FixedUpdate()
    {
        FixedTick();
    }

    public void Initialize()
    {
        if (initialized) 
            return;

        rigidbodyComp = GetComponent<Rigidbody>();
        accelerationIncrement = Time.deltaTime * stats.maxSpeed / stats.accelerationTime;
        decelerationIncrement = Time.deltaTime * stats.maxSpeed / stats.decelerationTime;
        direction = RandomDirection();
        targetDirection = RandomDirection();

        initialized = true;
    }

    public void FixedTick()
    {
        Accelerate();
        UpdateDirection();
        UpdateRotation();
        UpdateMovement();
    }

    private Vector3 RandomDirection()
    {
        Vector2 random2D = (UnityEngine.Random.insideUnitCircle).normalized;
        return new Vector3(random2D.x, 0, random2D.y);
    }

    private void Accelerate()
    {
        if (speed < stats.maxSpeed)
        {
            speed += accelerationIncrement;
            if (speed > stats.maxSpeed)
                speed = stats.maxSpeed;
        }
    }
    private void Decelerate()
    {
        if (speed > 0f)
        {
            speed -= decelerationIncrement;
            if (speed < 0f)
                speed = 0f;
        }
    }

    private void UpdateDirection()
    {
        direction = Vector3.RotateTowards(transform.forward, targetDirection, stats.turnRate * Time.fixedDeltaTime * Mathf.Deg2Rad, 0.0f);
    }
    private void UpdateMovement()
    {
        velocity = transform.forward * speed;
        rigidbodyComp.velocity = velocity;
    }
    private void UpdateRotation()
    {
        transform.forward = direction;
    }
}
