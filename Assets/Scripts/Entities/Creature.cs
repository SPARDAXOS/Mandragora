using System;
using System.Collections.Generic;
using UnityEngine;



[RequireComponent(typeof(Rigidbody))]
public class Creature : MonoBehaviour
{
    public enum CreatureType {
        NONE = 0,
        ONION,
        MUSHROOM
    }
    public enum CreatureState
    {
        NONE = 0,
        RUN,
        REST,
        HELD,
        FALL
    }

    [SerializeField] private CreatureType creatureType = CreatureType.NONE;
    [SerializeField] private CreatureStats stats;
    [SerializeField] private Vector2 randomizeDissatisfactionRate = new Vector2(0.5f, 2f);


    [Space(10)]
    [Range (0, 1f)] [SerializeField] private float dissatisfaction = 0;
    [SerializeField] private float scaleWhenDissatisfied = 3;
    [SerializeField] private bool doDissatisfaction;
    [SerializeField] private bool doEscapeHeld = false;
    [SerializeField] private bool drawAIGizmos;
    [Range(1, 4)][SerializeField] private int maxAmountOfTasks = 2;
    [Range(0, 3)][SerializeField] private int minAmountOfTasks = 2;



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

    public List<TaskStation.TaskType> taskList;

    private float restTimeElapsed = 0;
    private float restDuration = 0;

    private Rigidbody rigidbodyComp = null;
    private SphereCollider colliderComp = null;
    private Animator animatorComp = null;

    //In case of Onion ;(
    public Material[] onionMaterials = null;
    //In case of Mushroom ;(
    public Material[] mushroomMaterials = new Material[5];

    private Player player = null;
    private ParticleSystem stinkPS = null;
    private ParticleSystem cryPS = null;
    private ParticleSystem runDustPS = null;
    private ParticleSystem stressedPS = null;
    private ParticleSystem sickPS = null;
    private ParticleSystem heldPS = null;
    private Level levelScript;

    public void Initialize(Level level)
    {
        if (initialized) 
            return;

        if (creatureType == CreatureType.NONE) {
            Debug.LogError("Creature " + gameObject.name + " has CreatureType set to NONE!");
            return;
        }

        levelScript = level;

        if (minAmountOfTasks > maxAmountOfTasks) {
            Debug.LogWarning("minAmountOfTasks should not be higher than maxAmountOfTasks - Value will be swapped!");
            int temp = minAmountOfTasks;
            minAmountOfTasks = maxAmountOfTasks;
            maxAmountOfTasks = temp;
        }


        
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
        rigidbodyComp.velocity = Vector3.zero;
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
        colliderComp = GetComponent<SphereCollider>();
        animatorComp = transform.Find("Mesh").GetComponent<Animator>();
        stinkPS = transform.Find("StinkPS").GetComponent<ParticleSystem>();
        cryPS = transform.Find("CryPS").GetComponent<ParticleSystem>();
        runDustPS = transform.Find("RunDustPS").GetComponent<ParticleSystem>();
        stressedPS = transform.Find("StressedPS").GetComponent<ParticleSystem>();
        sickPS = transform.Find("SickPS").GetComponent<ParticleSystem>();
        heldPS = transform.Find("HeldPS").GetComponent<ParticleSystem>();
        heldPS.Stop();

        SetupMeshRenderers();
    }

    void SetupMeshRenderers() {
        var mesh = transform.Find("Mesh");
        switch (creatureType) {
            case CreatureType.ONION: {
                var skinnedMeshRenderers = mesh.GetComponentsInChildren<SkinnedMeshRenderer>();
                    onionMaterials = new Material[skinnedMeshRenderers.Length];
                    for (int i = 0; i < onionMaterials.Length; i++)
                        onionMaterials[i] = skinnedMeshRenderers[i].material;
                }
                break;
            case CreatureType.MUSHROOM:
                mushroomMaterials = mesh.GetComponentInChildren<SkinnedMeshRenderer>().materials;
                break;
        }
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
                Debug.Log("CompleteTask");
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

        int amountOfTasks = UnityEngine.Random.Range(minAmountOfTasks, maxAmountOfTasks + 1);

        taskList.Clear();
        while (taskList.Count != amountOfTasks)
        {
            int randomTask = UnityEngine.Random.Range(1, availableTasks.Count);

            if (taskList.Contains(availableTasks[randomTask]))
                continue;

            taskList.Add(availableTasks[randomTask]);
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
    void UpdateMaterials() {
        switch (creatureType) {
            case CreatureType.ONION: {
                foreach (var material in onionMaterials)
                        material.SetFloat("_Dissatisfaction", dissatisfaction);
                }
                break;
            case CreatureType.MUSHROOM: {
                    mushroomMaterials[0].SetFloat("_Dissatisfaction", dissatisfaction);
                    mushroomMaterials[1].SetFloat("_Dissatisfaction", dissatisfaction);
                    mushroomMaterials[2].SetFloat("_Dissatisfaction", dissatisfaction);
                    mushroomMaterials[3].SetFloat("_Dissatisfaction", dissatisfaction);
                    mushroomMaterials[4].SetFloat("_Dissatisfaction", dissatisfaction);
                }
                break;
        }
    }
    void UpdateScale()
    {
        transform.localScale = (initialScale + initialScale * (dissatisfaction * dissatisfaction * (scaleWhenDissatisfied - 1f))) * Vector3.one;
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
        if (taskList.Count == 0) {
            SetParticleSystemState(TaskStation.TaskType.BATHING, false);
            SetParticleSystemState(TaskStation.TaskType.SLEEPING, false);
            SetParticleSystemState(TaskStation.TaskType.FEEDING, false);
            SetParticleSystemState(TaskStation.TaskType.HEALING, false);
            return;
        }

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
            case TaskStation.TaskType.HEALING: {
                    if (state && !sickPS.isPlaying)
                        sickPS.Play();
                    else if (!state && sickPS.isPlaying)
                        sickPS.Stop();
                }
                break;
            case TaskStation.TaskType.SLEEPING: {
                    if (state && !stressedPS.isPlaying)
                        stressedPS.Play();
                    else if (!state && stressedPS.isPlaying)
                        stressedPS.Stop();
                }
                break;
        }
    }



    private void UpdateStates()
    {
        if (!isHeld)
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

        Vector3 origin = transform.position + colliderComp.center * transform.localScale.x;
        Vector3 direction = target - transform.position;
        direction.y = 0f;
        float radius = colliderComp.radius * transform.localScale.x;
        bool output = Physics.SphereCast(new Ray(origin, direction), radius * 0.9f, stats.viewRange);
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

        if (!heldPS.isEmitting)
            heldPS.Play();

        Debug.Log("OnPick : " + heldPS.isPlaying);
    }
    public void PutDown()
    {
        ChangeState(CreatureState.FALL);
        colliderComp.enabled = true;
        isHeld = false;
        player = null;

        if (heldPS.isEmitting)
            heldPS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
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
        if (state) {
            taskList.Clear();
            SetupParticleSystems();
        }

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
            Gizmos.color = Color.red;
            Vector3 origin = transform.position + colliderComp.center * transform.localScale.x;
            float radius = colliderComp.radius * transform.localScale.x;
            Gizmos.DrawWireSphere(origin, radius * 0.9f);
        }
    }
}