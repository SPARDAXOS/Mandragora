using System;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    [Tooltip("The time in seconds from the point of satisfaction to dissatisfaction.")]
    [Range(0, 120f)]
    public float timeUntilDissatisfied;
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
    [SerializeField] private Material satisfiedMaterial, dissatisfiedMaterial;

    private bool initialized = false;
    private bool active;
    public CreatureState state;
    private bool isHeld;

    private float accelerationIncrement;
    private float decelerationIncrement;

    private Vector3 targetPosition;
    private Vector3 direction = Vector3.forward;
    private Vector3 targetDirection = Vector3.forward;
    private float speed;
    private bool isMoving = false;
    private Vector3 velocity = Vector3.zero;

    float restTimeElapsed = 0;
    float restDuration = 0;

    private Rigidbody rigidbodyComp = null;
    private Collider col = null;
    private ParticleSystem stinkPS = null;
    private ParticleSystem cryPS = null;
    private ParticleSystem runDustPS = null;
    private Level levelScript;

    public void Initialize(Level level)
    {
        if (initialized) 
            return;

        this.levelScript = level;

        SetupReferences();
        SetupParticleSystems();

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
                SetParticleSystemState(completedTask, false);
            }
        }
    }

    //levelScript.RegisterCreatureMaximumDisatisfied();

    bool TaskListContains(TaskStation.TaskType task)
    {
        return taskList.Contains(task);
        
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

    void CheckRunDust()
    {
        if(isMoving && !runDustPS.isPlaying) 
            runDustPS.Play();
        else if(!isMoving && runDustPS.isPlaying)
            runDustPS.Stop();
    }
    void SetParticleSystemState(TaskStation.TaskType task, bool state)
    {
        switch (task)
        {
            case TaskStation.TaskType.FEEDING:
                {
                    if (state && !cryPS.isPlaying)
                        cryPS.Play();
                    else 
                        cryPS.Stop();
                }
                break;
            case TaskStation.TaskType.BATHING:
                {
                    if (state && !stinkPS.isPlaying)
                        stinkPS.Play();
                    else
                        stinkPS.Stop();
                }
                break;
        }
    }
    void SetupParticleSystems()
    {
        foreach(TaskStation.TaskType task in taskList)
        {
            if(task == TaskStation.TaskType.BATHING)
            {
                stinkPS.Play();
            }
            if (task == TaskStation.TaskType.BATHING)
            {
                cryPS.Play();
            }
        }
    }

    void SetupReferences()
    {
        rigidbodyComp   = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        stinkPS = transform.Find("StinkPS").GetComponent<ParticleSystem>();
        cryPS = transform.Find("CryPS").GetComponent<ParticleSystem>();
        runDustPS = transform.Find("RunDustPS").GetComponent<ParticleSystem>();
    }
    private void UpdateStates()
    {
        if(!isHeld)
            CheckFallState();

        switch (state)
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
        ChooseTarget();
        ChooseInput();
        UpdateDirection();
        UpdateRotation();
        UpdateMovement();
        CheckRunDust();
    }
    private void RestBehavior()
    {
        UpdateMovement();
        Decelerate();
        CheckRunDust();

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
        /*if (transform.position.y < 1f)
        {
            ChangeState(CreatureState.REST);
            return;
        }*/

        UpdateGravity();
    }

    void CheckFallState()
    {
        float yVelocity = rigidbodyComp.velocity.y;
        if (yVelocity < -0.01)
        {
            ChangeState(CreatureState.FALL);
        }
        else if (state == CreatureState.FALL)
            ChangeState(CreatureState.REST);
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
        if (speed > 0f) 
            isMoving = true;
        else 
            isMoving = false;
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
        isHeld = true;
    }
    public void PutDown()
    {
        ChangeState(CreatureState.RUN);
        col.enabled = true;
        isHeld = false;
        rigidbodyComp.velocity = Vector3.zero;
    }

    public void RegisterSatisfied() {
        SetActive(false);
        levelScript.RegisterSatisfiedCreature();
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