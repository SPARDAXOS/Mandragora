using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEditor;
using UnityEngine;

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
    [SerializeField] private Vector2 randomizeDissatisfactionRate = new Vector2(0.5f, 2f);
    [SerializeField] private Material satisfiedMaterial, dissatisfiedMaterial;
    [SerializeField] private GameObject changeMaterialOn;
    [Range (0, 1f)]
    [SerializeField] private float dissatisfaction = 0;
    [SerializeField] private float scaleWhenDissatisfied = 3;
    [SerializeField] private bool doDissatisfaction;
    [SerializeField] private bool doEscapeHeld = false;
    [SerializeField] private bool drawAIGizmos;
    [SerializeField] private List<TaskStation.TaskType> taskList;


    private bool initialized = false;
    private bool active;
    public CreatureState state;
    private bool isHeld;
    private bool tutorialCreature;
    private float deltaHeldEscapeProbability;

    private float dissatisfactionMultiplier = 1f;
    private float timeIncrement;
    private float initialScale = 1;
    private float accelerationIncrement;
    private float decelerationIncrement;

    private Vector3 targetPosition;
    private bool willRestAtTarget;
    private Vector3 direction = Vector3.forward;
    private Vector3 targetDirection = Vector3.forward;
    private float speed;
    public bool isMoving = false;
    private Vector3 velocity = Vector3.zero;

    private float restTimeElapsed = 0;
    private float restDuration = 0;

    private Rigidbody rigidbodyComp = null;
    private Collider colliderComp = null;
    private Animator animatorComp = null;
    private SkinnedMeshRenderer meshRendererComp = null;
    private Player player = null;
    private ParticleSystem stinkPS = null;
    private ParticleSystem cryPS = null;
    private ParticleSystem runDustPS = null;
    private Level levelScript;

    public void Initialize(Level level)
    {
        if (initialized) 
            return;

        levelScript = level;

        SetupReferences();

        initialScale = transform.localScale.x;
        timeIncrement = (1f / stats.timeUntilDissatisfied);
        CalculateEscapeProbability();

        accelerationIncrement = Time.fixedDeltaTime * stats.maxSpeed / stats.accelerationTime;
        decelerationIncrement = Time.fixedDeltaTime * stats.maxSpeed / stats.decelerationTime;

        initialized = true;
    }
    public void FixedTick()
    {
        if (!initialized)
            return;

        UpdateStates();
    }
    public void Tick()
    {
        if (!initialized)
            return;

        if (doDissatisfaction)
            UpdateSatisfaction();
    }

    /// <summary>
    /// Use this to set how your entity should be at the start of the game.
    /// </summary>
    public void SetupStartState() 
    {
        GetDissatisfactionMultiplier();
        StartDissatisfaction();
        tutorialCreature = false;

        speed = 0;
        direction = RandomDirection();
        state = CreatureState.FALL;
        transform.localScale = initialScale * Vector3.one;
        SetTutorialCreature(false);
        FindNewValidTarget();

        RandomizeTasks();
        SetupParticleSystems();
    }

    void SetupReferences()
    {
        rigidbodyComp   = GetComponent<Rigidbody>();
        colliderComp = GetComponent<Collider>();
        animatorComp = transform.Find("Mesh").GetComponent<Animator>();
        stinkPS = transform.Find("StinkPS").GetComponent<ParticleSystem>();
        cryPS = transform.Find("CryPS").GetComponent<ParticleSystem>();
        runDustPS = transform.Find("RunDustPS").GetComponent<ParticleSystem>();
        meshRendererComp = changeMaterialOn.GetComponent<SkinnedMeshRenderer>();
    }

    public void ClearAllTasks() {
        taskList.Clear();
    }
    public void AddTask(TaskStation.TaskType type) {
        if (taskList.Contains(type))
            return;

        taskList.Add(type);
        SetParticleSystemState(type, true);
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

                transform.position += 1.5f * Vector3.up;
                ApplyImpulse(-player.transform.forward + Vector3.up, 5f);
                player.DropHeldCreature();
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
    private void RandomizeTasks()
    {
        var allTasksArray = Enum.GetValues(typeof(TaskStation.TaskType));
        List<TaskStation.TaskType> availableTasks = new List<TaskStation.TaskType>();
        availableTasks.AddRange((IEnumerable<TaskStation.TaskType>)allTasksArray);

        int amountOfTasks = UnityEngine.Random.Range(1, allTasksArray.Length);

        taskList.Clear();
        while (taskList.Count != amountOfTasks)
        {
            int randomTask = UnityEngine.Random.Range(1, availableTasks.Count);
            AddTask(availableTasks[randomTask]);
            availableTasks.RemoveAt(randomTask);
        }
    }


    public bool IsSatisfied()
    {
        return taskList.Count == 0;
    }
    private void UpdateSatisfaction()
    {
        dissatisfaction += dissatisfactionMultiplier * timeIncrement * Time.deltaTime;


        if (IsSatisfied())
        {
            dissatisfaction = 0;
            doDissatisfaction = false;
        }

        else if (dissatisfaction >= 1f)
            levelScript.RegisterCreatureDesatisfied();

        UpdateMaterials();
        UpdateScale();
    }
    public void StartDissatisfaction()
    {
        dissatisfaction = 0f;
        doDissatisfaction = true;
    }
    public void StopDissatisfaction()
    {
        dissatisfaction = 0f;
        doDissatisfaction = false;
    }
    void UpdateMaterials()
    {
        meshRendererComp.material.Lerp(satisfiedMaterial, dissatisfiedMaterial, dissatisfaction);
    }
    void UpdateScale()
    {
        transform.localScale = (initialScale * (1f + dissatisfaction * dissatisfaction * (scaleWhenDissatisfied - 1f))) * Vector3.one;
    }
    void GetDissatisfactionMultiplier()
    {
        float random = UnityEngine.Random.value * (randomizeDissatisfactionRate.y - randomizeDissatisfactionRate.x) + randomizeDissatisfactionRate.x;
        dissatisfactionMultiplier = random / randomizeDissatisfactionRate.y;
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

    private void CheckRunningAnimation() {
        if (isMoving && !animatorComp.GetBool("isMoving"))
            animatorComp.SetBool("isMoving", true);
        else if (!isMoving && animatorComp.GetBool("isMoving"))
            animatorComp.SetBool("isMoving", false);
    }
    void CheckRunDust()
    {
        if (isMoving && !runDustPS.isPlaying) 
            runDustPS.Play();
        else if(!isMoving && runDustPS.isPlaying)
            runDustPS.Stop();
    }
    void SetupParticleSystems()
    {
        foreach (var entry in taskList)
            SetParticleSystemState(entry, true);
    }
    void SetParticleSystemState(TaskStation.TaskType task, bool state)
    {
        switch (task)
        {
            case TaskStation.TaskType.FEEDING:
                {
                    if (state && !cryPS.isPlaying)
                        cryPS.Play();
                    else if (!state && cryPS.isPlaying)
                        cryPS.Stop();
                }
                break;
            case TaskStation.TaskType.BATHING:
                {
                    if (state && !stinkPS.isPlaying)
                        stinkPS.Play();
                    else if (!state && stinkPS.isPlaying)
                        stinkPS.Stop();
                }
                break;
        }
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
            case CreatureState.HELD:
                HeldBehavior();
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
        CheckRunningAnimation();
    }
    private void RestBehavior()
    {
        UpdateDirection();
        UpdateRotation();
        UpdateMovement();
        Decelerate();
        CheckRunDust();
        CheckRunningAnimation();

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
        //rigidbodyComp.useGravity = true;
        //UpdateGravity();

        // TEMP ANTI-SOFTLOCK
        if(transform.position.y < -5f) transform.position = new Vector3(0f, 5f, 0f);
    }
    void HeldBehavior()
    {
        /*if (!player)
        {
            PutDown();
            return;
        }*/

        //Patched in.
        if (isMoving) {
            animatorComp.SetBool("isMoving", false);
            isMoving = false;
        }


        rigidbodyComp.velocity = Vector3.zero;

        if (!doEscapeHeld || !player || player.GetInTaskStationRange()) 
            return;

        float random = UnityEngine.Random.value;


        if (random < deltaHeldEscapeProbability)
        {
            player.DropHeldCreature();
            ApplyImpulse(Vector3.up + RandomDirection(), 5f);
        }
    }

    void CalculateEscapeProbability()
    {
        deltaHeldEscapeProbability = stats.escapeHeldProbability != 0 ? 1 - Mathf.Exp(Mathf.Log(1 - stats.escapeHeldProbability) / 60) : 0;
    }

    void CheckFallState()
    {
        float threshold = 0.01f;
        float yVelocity = rigidbodyComp.velocity.y;
        if (yVelocity < -threshold)
        {
            ChangeState(CreatureState.FALL);
        }
        else if (yVelocity < threshold && state == CreatureState.FALL)
        {
            ChangeState(CreatureState.REST);
        }
    }

    private Vector3 RandomDirection()
    {
        Vector2 random2D = (UnityEngine.Random.insideUnitCircle).normalized;
        return new Vector3(random2D.x, 0, random2D.y);
    }
    void ChooseTarget()
    {
        bool pathObstructed = CastToTarget(targetPosition, false);

        float sqrDistToTarget = (targetPosition - transform.position).sqrMagnitude;

        float breakDist = 0.5f * speed * stats.decelerationTime;
        float turnRadius = 0.5f * speed / ( 2 * Mathf.PI * (stats.turnRate / 360f));

        if (willRestAtTarget && sqrDistToTarget < breakDist * breakDist)
        {
            ChangeState(CreatureState.REST);
            return;
        }
        else if(sqrDistToTarget < turnRadius * turnRadius)
        {
            FindNewValidTarget();
            return;
        }

        if (pathObstructed)
            FindNewValidTarget(false);
    }
    void FindNewValidTarget(bool interruptable = true)
    {
        int retryLimit = 50;
        int retryIndex = 0;

        while (retryIndex < retryLimit)
        {
            retryIndex++;
            Vector3 candidateTarget = CandidateTarget();
            if (!CastToTarget(candidateTarget))
                break;
        }
        if(interruptable)
            willRestAtTarget = UnityEngine.Random.value <= stats.restProbability;
    }
    bool CastToTarget(Vector3 target, bool assignTarget = true)
    {
        float scale = transform.localScale.x;
        if (!Physics.CheckSphere(target, scale))
            return false;

        Vector3 origin = transform.position;
        Vector3 direction = target - transform.position;
        colliderComp.enabled = false;
        bool output = Physics.SphereCast(new Ray(origin, direction), scale * 0.9f, stats.viewRange);
        colliderComp.enabled = true;
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
        this.player = player;
        ChangeState(CreatureState.HELD);
        colliderComp.enabled = false;
        speed = 0f;
        isHeld = true;
        
        if (runDustPS.isPlaying)
            runDustPS.Stop();
    }
    public void PutDown()
    {
        ChangeState(CreatureState.FALL);
        colliderComp.enabled = true;
        isHeld = false;
        player = null;
    }
    public bool GetHeldState()
    {
        return isHeld;
    }

    public void ApplyImpulse(Vector3 direction, float force) {
        rigidbodyComp.velocity = direction * force;
    }

    public void RegisterSatisfied() {
        SetActive(false);
        levelScript.RegisterSatisfiedCreature();
    }

    public void SetTutorialCreature(bool state)
    {
        tutorialCreature = state;
    }
    public bool GetTutorialCreature() 
    {
        return tutorialCreature;
    }

    private void OnDrawGizmosSelected()
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