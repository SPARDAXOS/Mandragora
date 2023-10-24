using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct CreatureStats
{
    [Header("Movement")]
    [Tooltip("Units per second")]
    public float maxSpeed;
    public float accelerationTime;
    public float decelerationTime;
    public float gravityScale;
    [Tooltip("Degrees per second.")]
    public float turnRate;

    [Header("Behavior")]
    //List<TaskType> taskList
    [Range(0, 1f)]
    public float shyness;
    public float viewRange;
    public float maxRestDuration;
    [Range(0, 1f)]
    public float restProbability;

    [Header("Variables")]
    [Range(0, 1f)]
    public float satisfaction;
    public bool hungry;
    public bool dirty;
}

[RequireComponent(typeof(Rigidbody))]
public class Creature : MonoBehaviour
{
    public enum CreatureState
    {
        NONE = 0,
        RUN,
        REST,
        HELD,
        FALL
    }

    [SerializeField] private CreatureStats stats;
    [SerializeField] private bool drawAIGizmos;
    public List<TaskStation.TaskType> taskList;

    private bool initialized = false;
    private bool active;
    public CreatureState state;
    private float accelerationIncrement;
    private float decelerationIncrement;

    private Vector3 targetPosition;
    private Vector3 direction = Vector3.forward;
    private Vector3 targetDirection = Vector3.forward;
    private float speed;
    private Vector3 velocity = Vector3.zero;

    float restTimeElapsed = 0;
    float restDuration = 0;

    private Rigidbody rigidbodyComp = null;
    private Collider col = null;
    private ParticleSystem stinkPS = null;
    private ParticleSystem cryPS = null;
    private Level level;

    public void Initialize(Level level)
    {
        if (initialized) 
            return;

        this.level = level;

        SetupReferences();

        state = CreatureState.FALL;

        accelerationIncrement = Time.fixedDeltaTime * stats.maxSpeed / stats.accelerationTime;
        decelerationIncrement = Time.fixedDeltaTime * stats.maxSpeed / stats.decelerationTime;

        direction = RandomDirection();
        FindNewValidTarget();

        initialized = true;
    }
    public void FixedTick()
    {
        UpdateStates();
        UpdateParticles();
    }
    public bool IsSatisfied()
    {
        return taskList.Count == 0;
    }
    public void CompleteTask(TaskStation.TaskType completedTask)
    {
        for (int i = 0; i < taskList.Count; i++)
        {
            TaskStation.TaskType task = taskList[i];
            if (task == completedTask)
            {
                taskList.RemoveAt(i);
                //return;
            }
        }
    }

    public bool DoesRequireTask(TaskStation.TaskType type) {
        foreach(var entry in taskList) {
            if (entry == type)
                return true;
        }
        return false;
    }

    public bool GetActive()
    {
        return active;
    }
    public void SetActive(bool state)
    {
        active = state;
        gameObject.SetActive(active);
    }
    void UpdateParticles()
    {
        if (stats.dirty && !stinkPS.isPlaying)
            stinkPS.Play();
        else if (!stats.dirty)
            stinkPS.Stop();

        if (stats.hungry && !cryPS.isPlaying)
            cryPS.Play();
        else if (!stats.hungry)
            cryPS.Stop();
    }
    void SetupReferences()
    {
        rigidbodyComp = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        stinkPS       = transform.Find("StinkPS").GetComponent<ParticleSystem>();
        cryPS         = transform.Find("CryPS").GetComponent<ParticleSystem>();
    }
    private void UpdateStates()
    {
        switch(state)
        {
            case CreatureState.RUN:
                RunBehavior();
                break;
            case CreatureState.REST:
                RestBehavior();
                break;
            case CreatureState.FALL:
                FallBehavior();
                break;
        }
    }
    private void RunBehavior()
    {
        UpdateDirection();
        UpdateRotation();
        UpdateMovement();
        ChooseInput();
        ChooseTarget();
    }
    private void RestBehavior()
    {
        UpdateMovement();
        Decelerate();

        if (restTimeElapsed == 0)
        {
            restDuration = UnityEngine.Random.value * stats.maxRestDuration + stats.decelerationTime;
        }

        else if(restTimeElapsed > restDuration)
        {
            state = CreatureState.RUN;
            restTimeElapsed = 0;
            FindNewValidTarget();
            return;
        }

        restTimeElapsed += Time.fixedDeltaTime;
    }

    void FallBehavior()
    {
        //Debug.Log(rigidbodyComp.velocity.y);
        if (transform.position.y < 1f)
        {
            
            ChangeState(CreatureState.REST);
            return;
        }

        UpdateGravity();
    }

    private Vector3 RandomDirection()
    {
        Vector2 random2D = (UnityEngine.Random.insideUnitCircle).normalized;
        return new Vector3(random2D.x, 0, random2D.y);
    }
    void ChooseTarget()
    {
        bool pathObstructed = CastToTarget(targetPosition, false);

        float breakDist = speed * stats.decelerationTime;
        bool isClose = (targetPosition - transform.position).sqrMagnitude < breakDist * breakDist;
        bool willRest = UnityEngine.Random.value <= stats.restProbability;

        if (isClose)
        {
            if (willRest)
                ChangeState(CreatureState.REST);
            else
                FindNewValidTarget();
            return;
        }

        if (pathObstructed)
            FindNewValidTarget();
    }
    void FindNewValidTarget()
    {
        int retryLimit = 50;
        int retryIndex = 0;

        while (retryIndex < retryLimit)
        {
            retryIndex++;
            Vector3 candidateTarget = CandidateTarget();
            if (!CastToTarget(candidateTarget, true))
                break;
        }
    }
    bool CastToTarget(Vector3 target, bool assignTarget)
    {
        Vector3 origin = transform.position;
        Vector3 direction = target - transform.position;
        col.enabled = false;
        bool output = Physics.SphereCast(new Ray(origin, direction), 0.25f, stats.viewRange);
        col.enabled = true;
        if (!output && assignTarget) targetPosition = target;
        return output;
    }
    Vector3 CandidateTarget()
    {
        Vector3 candidate = transform.position.xz() + stats.viewRange * UnityEngine.Random.insideUnitCircle;
        candidate = candidate.x0y();
        return candidate + transform.position.y * Vector3.up;
    }
    private void ChooseInput()
    {
        float dirDot = Vector3.Dot(direction, targetDirection);
        if(dirDot > -0.5f)
            Accelerate();
        else 
            Decelerate();
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

    private void UpdateGravity() {
        rigidbodyComp.velocity += new Vector3(0.0f, -stats.gravityScale * Time.fixedDeltaTime, 0.0f);
    }
    private void UpdateDirection()
    {
        targetDirection = (targetPosition - transform.position).normalized;

        direction = Vector3.RotateTowards(direction, targetDirection, stats.turnRate * Time.fixedDeltaTime * Mathf.Deg2Rad, 0.0f);
        direction.y = 0;
        direction = direction.normalized;
    }
    private void UpdateMovement()
    {
        velocity = speed * direction;
        rigidbodyComp.velocity = velocity;
    }
    private void UpdateRotation()
    {
        transform.forward = direction;
    }
    private void ChangeState(CreatureState state)
    {
        this.state = state;
    }
    public void PickUp(Player player)
    {
        ChangeState(CreatureState.HELD);
        col.enabled = false;
    }
    public void PutDown()
    {
        ChangeState(CreatureState.RUN);
        col.enabled = true;
    }
    private void OnCollisionEnter(Collision collision)
    {
        //direction *= -1f;
        //speed *= 0.5f;
    }
    private void OnDrawGizmos()
    {
        if (drawAIGizmos)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, stats.viewRange);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(targetPosition, 0.5f);
        }
    }
}