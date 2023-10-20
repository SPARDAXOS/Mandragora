using System;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using static UnityEngine.UI.Image;

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
    RUNNING,
    WAITING,
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

    private Vector3 targetPosition;
    private Vector3 direction = Vector3.forward;
    private Vector3 targetDirection = Vector3.forward;

    private float speed;
    private Vector3 velocity = Vector3.zero;

    [SerializeField] LayerMask ignoreLayer;


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
        if(initialized) 
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
        FindValidTarget();
        UpdateDirection();
        ChooseInput();
        UpdateRotation();
        UpdateMovement();
    }

    private Vector3 RandomDirection()
    {
        Vector2 random2D = (UnityEngine.Random.insideUnitCircle).normalized;
        return new Vector3(random2D.x, 0, random2D.y);
    }

    void FindValidTarget()
    {
        bool proximityCondition = (targetPosition - transform.position).sqrMagnitude > 1;
        bool clearPathCondition = !CastToTarget(ref targetPosition);

        if (proximityCondition || (proximityCondition && clearPathCondition))
            return;

        int retryLimit = 50;
        int retryIndex = 0;

        while (retryIndex < retryLimit)
        {
            retryIndex++;

            if (!CastToTarget(ref targetPosition))
                break;
        }
        Debug.Log(retryIndex);
    }

    bool CastToTarget(ref Vector3 target)
    {
        Vector3 candidateTarget = CandidateTarget();
        Vector3 origin = transform.position;
        Vector3 direction = candidateTarget - transform.position;
        bool output = Physics.Raycast(origin, direction, stats.viewRange, ~ignoreLayer);
        if (output) target = candidateTarget;
        return output;
    }

    Vector3 CandidateTarget()
    {
        Vector3 candidate = stats.viewRange * UnityEngine.Random.insideUnitCircle;
        candidate = new Vector3(candidate.x, 0, candidate.y);
        return candidate + transform.position.y * Vector3.up;
    }

    private void ChooseInput()
    {
        float dirDot = Vector3.Dot(direction, targetDirection);
        if(dirDot > 0.5f)
            Accelerate();
        else Decelerate();
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
        targetDirection = (targetPosition - transform.position).normalized;
        direction = Vector3.RotateTowards(transform.forward, targetDirection, stats.turnRate * Time.fixedDeltaTime * Mathf.Deg2Rad, 0.0f);
        direction.y = 0;
        Vector3.Normalize(direction);
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

    public void ChangeState(CreatureState state)
    {
        this.state = state;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stats.viewRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(targetPosition, 0.5f);
    }
}
