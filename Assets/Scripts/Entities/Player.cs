using Mandragora;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Windows;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.Image;

public class Player : MonoBehaviour {

    public enum PlayerType {
        NONE = 0,
        PLAYER_1,
        PLAYER_2
    }

    [SerializeField] private PlayerStats stats;

    [Header("Pickup")]
    [SerializeField] private float pickupCheckBoxSize = 1.0f;
    [SerializeField] public Vector3 pickupCheckOffset;
    [SerializeField] private LayerMask pickupMask;
    [SerializeField] private Vector3 pickupBoxColliderCenter;
    [SerializeField] private Vector3 pickupBoxColliderSize;

    [Header("Navigation")]
    [SerializeField] private float pathCheckOffset = 0.1f;

    [Header("CameraShakes")]
    [SerializeField] private CameraShake bounceCameraShake;
    [SerializeField] private CameraShake dashBounceCameraShake;

    [Header("Debugging")]
    [SerializeField] private bool showGroundedCheck = true;
    [SerializeField] private bool showPickupCheck = true;
    [SerializeField] private bool showPathCheck = true;


    private PlayerType playerType = PlayerType.NONE;

    public bool initialized = false;
    private bool isMoving = false;
    private bool isInteractingHeld = false;
    public bool isInteractingTrigger = false;
    private bool isDashingTrigger = false;
    private bool isThrowingTrigger = false;

    public bool isKnockedback = false;
    public bool isGrounded = false;
    private bool isDashing = false;
    private bool isStunned = false;
    public bool isPathBlocked = false;
    public bool isInteractingWithTaskStation = false;


    public float currentSpeed = 0.0f;
    public float dashTimer = 0.0f;
    public float dashCooldownTimer = 0.0f;
    public float stunTimer = 0.0f;

    public Vector3 direction;

    private Vector3 normalBoxColliderSize;
    private Vector3 normalBoxColliderCenter;

    public bool inTaskStationRange = false;
    public TaskStation interactingTaskStation = null;

    private GameObject pickupPoint = null;
    public Creature heldCreature = null;



    private PlayerControlScheme activeControlScheme = null;

    private GameInstance gameInstance = null;
    private SoundManager soundManager = null;
    private MainCamera mainCamera = null;

    private ParticleSystem runDustPS = null;
    private ParticleSystem stunnedPS = null;
    private ParticleSystem impactPS = null;
    private ParticleSystem knockbackTrailPS = null;


    private Rigidbody rigidbodyComp = null;
    private BoxCollider boxColliderComp = null;
    private Animator animatorComp = null;
    private MeshRenderer meshRendererComp = null;
    private Material mainMaterial;

    private PhysicMaterial physicsMaterial = null;

    public void Initialize(PlayerType type, PlayerControlScheme controlScheme, GameInstance gameInstance, SoundManager soundManager, MainCamera mainCamera) {
        if (initialized)
            return;

        playerType = type;
        activeControlScheme = controlScheme;
        this.gameInstance = gameInstance;
        this.soundManager = soundManager;
        this.mainCamera = mainCamera;
        SetupReferences();
        initialized = true;
    }
    private void SetupReferences() {
        pickupPoint = transform.Find("PickupPoint").gameObject;

        runDustPS = transform.Find("RunDustPS").GetComponent<ParticleSystem>();
        Utility.Validate(runDustPS, "Failed to find RunDustPS.", Utility.ValidationType.WARNING);

        stunnedPS = transform.Find("StunnedPS").GetComponent<ParticleSystem>();
        Utility.Validate(stunnedPS, "Failed to find StunnedPS.", Utility.ValidationType.WARNING);

        impactPS = transform.Find("ImpactPS").GetComponent<ParticleSystem>();
        Utility.Validate(impactPS, "Failed to find ImpactPS.", Utility.ValidationType.WARNING);

        knockbackTrailPS = transform.Find("KnockbackTrailPS").GetComponent<ParticleSystem>();
        Utility.Validate(knockbackTrailPS, "Failed to find KnockbackTrailPS.", Utility.ValidationType.WARNING);
        

        rigidbodyComp = GetComponent<Rigidbody>();
        if (!rigidbodyComp)
            GameInstance.Abort("Failed to get Rigidbody component on " + gameObject.name);

        animatorComp = transform.Find("Mesh").GetComponent<Animator>();
        if (!animatorComp)
            GameInstance.Abort("Failed to get Animator component on " + gameObject.name);

        boxColliderComp = GetComponent<BoxCollider>();
        if (!boxColliderComp)
            GameInstance.Abort("Failed to get BoxCollider component on " + gameObject.name);
        physicsMaterial = boxColliderComp.material;

        normalBoxColliderSize = boxColliderComp.size;
        normalBoxColliderCenter = boxColliderComp.center;
    }
    public void SetupStartingState() {

        //More proper stop!
        isKnockedback = false;
        isDashing = false;
        isStunned = false;

        currentSpeed = 0.0f;
        dashTimer = 0.0f;
        dashCooldownTimer = 0.0f;
        stunTimer = 0.0f;

        //Reset interaction state
        isInteractingWithTaskStation = false;
        interactingTaskStation = null;
        inTaskStationRange = false;

        //Reset held creature stuff
        DropHeldCreature(); //This is the only thing that doesnt work
    }


    public void Tick() {
        if (!initialized)
            return;


        UpdateStunTimer();
        UpdateDashTimers();
        UpdateHeldCreature();

        UpdateInput();
        UpdateAnimations();
        CheckTaskStatioInteraction();

        CheckGrounded();
        CheckPath();
        CheckMovement();
        CheckMovementPS();
        CheckPause();
        CheckDash();
        CheckPickup();
        CheckThrow();
    }
    public void FixedTick() {
        if (!initialized)
            return;


        UpdateRotation();
        UpdateGravity();


        if (isKnockedback) {
            if (isGrounded && rigidbodyComp.velocity.y < 0.0f) {
                if (knockbackTrailPS.isPlaying)
                    knockbackTrailPS.Stop();
                isKnockedback = false;
            }
        }
        else if (!isDashing)
            UpdateMovement();
    }

    public void EnableInput() {

        activeControlScheme.movement.Enable();
        activeControlScheme.interact.Enable();
        activeControlScheme.dash.Enable();
        activeControlScheme.throwAway.Enable();
        activeControlScheme.pause.Enable();
    }
    public void DisableInput() {

        activeControlScheme.movement.Disable();
        activeControlScheme.interact.Disable();
        activeControlScheme.dash.Disable();
        activeControlScheme.throwAway.Disable();
        activeControlScheme.pause.Disable();
    }
    public void EnableMovement() {
        activeControlScheme.movement.Enable();
        activeControlScheme.dash.Enable();
    }
    public void DisableMovement() {
        activeControlScheme.movement.Disable();
        activeControlScheme.dash.Disable();
    }
    public void EnableTaskStationInputState() {
        activeControlScheme.movement.Disable();
        activeControlScheme.dash.Disable();
        activeControlScheme.throwAway.Disable();
    }
    public void DisableTaskStationInputState() {
        activeControlScheme.movement.Enable();
        activeControlScheme.dash.Enable();
        activeControlScheme.throwAway.Enable();
    }
    public void EnableInteractionInput() {
        activeControlScheme.movement.Enable();
        activeControlScheme.interact.Enable();
        activeControlScheme.dash.Enable();
        activeControlScheme.throwAway.Enable();
    }
    public void DisableInteractionInput() {
        activeControlScheme.movement.Disable();
        activeControlScheme.interact.Disable();
        activeControlScheme.dash.Disable();
        activeControlScheme.throwAway.Disable();
    }



    private void CheckKnockback(Player target, Vector3 contactPoint) {

        Vector3 knockbackDirection = target.transform.position - transform.position;
        knockbackDirection.Normalize();

        if (isDashing) {
            if (heldCreature) {
                heldCreature.ApplyImpulse(Vector3.up, stats.dashCreatureDropForce);
                DropHeldCreature();
            }
            StopDashing();
            Vector3 targetDirection = contactPoint - transform.position;
            targetDirection.Normalize();
            //ApplyDashRetainedSpeed(-targetDirection); //nOT REALLY NEEDED HERE!

            ApplyStun(stats.stunDuration, false);

            knockbackDirection.x *= Mathf.Cos(Mathf.Deg2Rad * stats.dashKnockbackHeightAngle);
            knockbackDirection.y = Mathf.Sin(Mathf.Deg2Rad * stats.dashKnockbackHeightAngle);
            knockbackDirection.z *= Mathf.Cos(Mathf.Deg2Rad * stats.dashKnockbackHeightAngle);
            target.ApplyKnockback(knockbackDirection, stats.dashKnockbackForce); //Push other by full force
            Debug.Log("Dash Knockback - " + gameObject.name);
        }
        else {
            if (heldCreature) {
                heldCreature.ApplyImpulse(Vector3.up, stats.knockbackCreatureDropForce);
                DropHeldCreature();
            }
            knockbackDirection.x *= Mathf.Cos(Mathf.Deg2Rad * stats.knockbackHeightAngle);
            knockbackDirection.y = Mathf.Sin(Mathf.Deg2Rad * stats.knockbackHeightAngle);
            knockbackDirection.z *= Mathf.Cos(Mathf.Deg2Rad * stats.knockbackHeightAngle);
            target.ApplyKnockback(knockbackDirection, stats.knockbackForce);
            Debug.Log("Normal Knockback - " + gameObject.name);
        }
    }
    private void CheckDash() {
        if (isStunned || !isGrounded || dashCooldownTimer > 0.0f)
            return;

        if (isDashingTrigger && !isDashing) {
            isDashing = true;
            rigidbodyComp.velocity = transform.forward * stats.dashSpeed;
            dashTimer = stats.dashLength;
            direction = transform.forward;
        }
    }
    private void CheckThrow() {
        if (isThrowingTrigger) {
            if (!heldCreature)
                return;
            else if (!isInteractingWithTaskStation){
                Vector3 throwDirection = transform.forward;
                throwDirection.x *= Mathf.Cos(Mathf.Deg2Rad * stats.throwHeightAngle);
                throwDirection.y = Mathf.Sin(Mathf.Deg2Rad * stats.throwHeightAngle);
                throwDirection.z *= Mathf.Cos(Mathf.Deg2Rad * stats.throwHeightAngle);
                soundManager.PlaySFX("CreatureThrow", transform.position);
                heldCreature.ApplyImpulse(throwDirection, stats.throwForce);
                DropHeldCreature();
            }
        }
    }
    private void CheckPickup() {
        //Here
        if (isInteractingTrigger) {
            if (!heldCreature)
                Pickup();
            else if (heldCreature && !inTaskStationRange) {
                Debug.Log("Drop!");
                DropHeldCreature();
            }
        }
    }

    private void CheckGrounded() {
        Vector3 startingPosition = transform.position;

        Vector3 size;
        float offset = 0.1f;
        if (heldCreature) {
            startingPosition += pickupBoxColliderCenter;
            size = pickupBoxColliderSize;
        }
        else {
            startingPosition += normalBoxColliderCenter;
            size = normalBoxColliderSize;
        }


        startingPosition.y += offset;
        bool results = Physics.BoxCast(startingPosition, size / 2, -transform.up, transform.rotation, offset * 2);
        if (!isGrounded && results)
            soundManager.PlaySFX("Landing", transform.position);
        isGrounded = results;
    }
    private void CheckPath() {
        Vector3 origin = transform.position;
        if (heldCreature)
            origin += transform.rotation * pickupBoxColliderCenter;
        else
            origin += transform.rotation * normalBoxColliderCenter;

        origin.y += 0.01f;
        origin.x -= pathCheckOffset * transform.forward.x;
        origin.z -= pathCheckOffset * transform.forward.z;

        Vector3 halfExtent;
        if (heldCreature)
            halfExtent = pickupBoxColliderSize / 2;
        else
            halfExtent = normalBoxColliderSize / 2;

        RaycastHit hit;
        if (Physics.BoxCast(origin, halfExtent, transform.forward, out hit, transform.rotation, pathCheckOffset * 2)) {
            if (hit.collider.CompareTag("Player") || hit.collider.CompareTag("Creature")) {
                isPathBlocked = false;
                return;
            }
            else {
                if (isDashing) {
                    Debug.LogWarning("I dashed into " + hit.collider.name);
                    StopDashing();
                    Vector3 targetDirection = hit.point - transform.position;
                    //targetDirection.Normalize();
                    //ApplyDashRetainedSpeed(-targetDirection);
                    Debug.Log("Dash Stopped cause touching!!");


                    Vector3 bounceDirection = -transform.forward;
                    bounceDirection.x *= Mathf.Cos(Mathf.Deg2Rad * stats.objectBounceOffAngle);
                    bounceDirection.y = Mathf.Sin(Mathf.Deg2Rad * stats.objectBounceOffAngle);
                    bounceDirection.z *= Mathf.Cos(Mathf.Deg2Rad * stats.objectBounceOffAngle);
                    ApplyKnockback(bounceDirection, stats.objectBouceOffForce);
                    mainCamera.ShakeFor(bounceCameraShake);
                    soundManager.PlaySFX("BounceOffObject", transform.position);
                    if (!impactPS.isPlaying)
                        impactPS.Play();
                }
                isPathBlocked = true;
            }
        }
        else
            isPathBlocked = false;
    }
    private void CheckReverse(Vector2 newDirection) {
        Vector2 currentDirection = new Vector2(direction.x, direction.z);
        float dot = Vector2.Dot(currentDirection, newDirection);
        if (dot == -1) {
            currentSpeed *= stats.reverseRetainedSpeed;

        }
    }


    public void SetInteractingWithTaskStationState(TaskStation target, bool state) {
        if (!target) {
            Debug.LogError("Null taskstation sent to SetInteractingWithTaskStationState - Should always send valid ref here for check");
            return;
        }

        if (!state && isInteractingWithTaskStation) {
            if (interactingTaskStation == target) { 
                isInteractingWithTaskStation = false;
                return;
            }
        }
        else if (state && !isInteractingWithTaskStation) {
            isInteractingWithTaskStation = true;
            return;
        }

        Debug.LogError("ERROR! This is not meant to be reached!");
    }


    public void SetInTaskStationRange(TaskStation target, bool state) {

        interactingTaskStation = target;
        inTaskStationRange = state;
    }
    public bool GetInTaskStationRange() { //(???
        return inTaskStationRange;
    }
    private void CheckTaskStatioInteraction() {
        if (!inTaskStationRange || !interactingTaskStation || isInteractingWithTaskStation)
            return;

        if (!interactingTaskStation.IsInteractionOngoing() && isInteractingTrigger) {
            interactingTaskStation.Interact(this);
            Debug.Log("I interacted! should be true - " + isInteractingWithTaskStation);
        }
    }



    private void CheckPause() {
        if (activeControlScheme.pause.triggered)
            gameInstance.PauseGame();
    }
    private void CheckMovement() {
        if (isMoving) {
            Vector2 input = activeControlScheme.movement.ReadValue<Vector2>();
            CheckReverse(input);

            direction = new Vector3(input.x, 0.0f, input.y);
            if (!isPathBlocked)
                Accelerate();
        }
        else if (currentSpeed > 0.0f)
            Decelerate();
    }
    private void CheckMovementPS() {
        if (isMoving && !runDustPS.isPlaying && isGrounded) //Test this
            runDustPS.Play();
        else if (!isMoving && runDustPS.isPlaying)
            runDustPS.Stop();
    }
    private void UpdateInput() {
        if (!activeControlScheme)
            return;

        isInteractingTrigger = activeControlScheme.interact.triggered;
        isDashingTrigger = activeControlScheme.dash.triggered;
        isThrowingTrigger = activeControlScheme.throwAway.triggered;
        isInteractingHeld = activeControlScheme.interact.IsPressed();
        isMoving = activeControlScheme.movement.IsPressed();
    }

    private void Accelerate() {
        if (currentSpeed < stats.maxSpeed) {
            currentSpeed += stats.accelerationRate * Time.deltaTime;
            if (currentSpeed >= stats.maxSpeed)
                currentSpeed = stats.maxSpeed;
        }
    }
    private void Decelerate() {
        if (currentSpeed > 0.0f) {
            currentSpeed -= stats.decelerationRate * Time.deltaTime;
            if (currentSpeed < 0.0f)
                currentSpeed = 0.0f;
        }
    }
    private void UpdateMovement() {
        //WORKS BUT QUESTIONABLE!
        if (isPathBlocked)
            currentSpeed *= stats.speedRetainedOnHit;

        Vector3 velocity = direction * currentSpeed * Time.fixedDeltaTime;
        rigidbodyComp.velocity = new Vector3(velocity.x, rigidbodyComp.velocity.y, velocity.z);
    }
    private void UpdateRotation() {
        transform.forward 
            = Vector3.RotateTowards(transform.forward, direction, stats.turnRate * Time.fixedDeltaTime, 0.0f);
    }
    private void UpdateGravity() {
        rigidbodyComp.useGravity = !stats.customGravity;
        if (!stats.customGravity)
            physicsMaterial.bounciness = stats.normalGravityBounciness;
        else
            physicsMaterial.bounciness = stats.customGravityBounciness;

        if (isGrounded || !stats.customGravity)
            return;
        
        float gravity = stats.gravityScale * Time.fixedDeltaTime;
        rigidbodyComp.velocity += new Vector3(0.0f, -gravity, 0.0f);
    }



    private void UpdateDashTimers() {
        if (dashCooldownTimer > 0.0f) {
            dashCooldownTimer -= Time.deltaTime;
            if (dashCooldownTimer < 0.0f)
                dashCooldownTimer = 0.0f;
        }

        if (dashTimer > 0.0f) {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0.0f) {
                StopDashing();
                ApplyDashRetainedSpeed(transform.forward);
                Debug.Log("Dashing finished!");
            }
        }
    }
    private void UpdateStunTimer() {
        if (stunTimer > 0.0f) {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0.0f) {
                //Consider moving this and the turn on to func!
                stunTimer = 0.0f;
                isStunned = false;
                EnableInteractionInput();
                if (stunnedPS.isPlaying)
                    stunnedPS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
        else
            isStunned = false;
    }

    private void UpdateAnimations() {
        if (isMoving && isGrounded)
            animatorComp.SetBool("isMoving", true);
        else
            animatorComp.SetBool("isMoving", false);
    }

    private void Pickup() {
        Vector3 boxcastOrigin = transform.position;
        boxcastOrigin += transform.rotation * pickupCheckOffset;
        Vector3 boxExtent = new Vector3(pickupCheckBoxSize / 2.0f, pickupCheckBoxSize / 2.0f, pickupCheckBoxSize / 2.0f);

        var HitResults = Physics.BoxCastAll(boxcastOrigin, boxExtent, transform.forward, transform.rotation, 0.0f, pickupMask.value);
        if (HitResults != null)
            foreach (var entry in HitResults) {
                var script = entry.collider.GetComponent<Creature>();
                if (!script) {
                    Debug.LogError("Attempted to pickup invalid Creature that did not own creature script!");
                    continue;
                }

                if (isDashing) {
                    Debug.LogWarning("Dash interrupted by pickup!");
                    StopDashing();
                    //Weird!
                    ApplyDashRetainedSpeed(transform.forward);
                }
                if (!script.GetHeldState()) {
                    PickupCreature(script);
                    Debug.Log("Called pickup on " + script.name);
                    return;
                }
            }
    }
    private void UpdateHeldCreature() {
        if (!heldCreature)
            return;

        if (isInteractingWithTaskStation) {
            if (interactingTaskStation.IsUsingCustomHeldSpot()) {
                Transform heldSpot = interactingTaskStation.GetCustomHeldSpot();
                heldCreature.transform.position = heldSpot.transform.position;
                heldCreature.transform.rotation = heldSpot.transform.rotation;
                return;
            }
        }
        Debug.Log("Player side");
        heldCreature.transform.position = pickupPoint.transform.position;
    }




    public bool IsInteractingTrigger() {
        return isInteractingTrigger;
    }
    public bool IsInteractingHeld() {
        return isInteractingHeld;
    }
    public bool IsMoving() {
        return isMoving;
    }
    public bool IsDashing() {
        return isDashing;
    }
    public bool IsThrowing() {
        return isThrowingTrigger;
    }
    public GameObject GetPickupPoint() {
        return pickupPoint;
    }


    public PlayerType GetPlayerType() {
        return playerType;
    }
    public Creature GetHeldCreature() {
        return heldCreature;
    }
    public void PickupCreature(Creature target) {
        if (!target)
            return;

        heldCreature = target;
        heldCreature.PickUp(this);
        boxColliderComp.size = pickupBoxColliderSize;
        boxColliderComp.center = pickupBoxColliderCenter;
    }
    public void DropHeldCreature() {
        if (!heldCreature)
            return;
        
        //Disable VFX
        heldCreature.PutDown();
        heldCreature = null;
        boxColliderComp.size = normalBoxColliderSize;
        boxColliderComp.center = normalBoxColliderCenter;
    }


    public void ApplyKnockback(Vector3 direction, float force) {
        if (isKnockedback)
            return;

        rigidbodyComp.velocity = Vector3.zero;
        rigidbodyComp.velocity += direction * force;
        isKnockedback = true;
        //if (!knockbackTrailPS.isPlaying)
            knockbackTrailPS.Play();
    }
    public void ApplyStun(float duration, bool stack = false) {
        if (stack)
            stunTimer += duration;
        else
            stunTimer = duration;


        if (stunTimer > 0.0f) {
            DisableInteractionInput();

            if (!stunnedPS.isPlaying)
                stunnedPS.Play();
            isStunned = true;
        }
    }
    private void StopDashing() {
        isDashing = false;
        dashTimer = 0.0f;
        dashCooldownTimer = stats.dashCooldown;
    }
    private void ApplyDashRetainedSpeed(Vector3 target) {
        target.y = 0.0f; //Just in case
        direction = target;
        currentSpeed = stats.maxSpeed * stats.retainedSpeed;
    }




    private void OnCollisionEnter(Collision collision) {
        if (collision == null)
            return;

        if (collision.collider.CompareTag("Player")) {
            var script = collision.collider.GetComponent<Player>();
            if (script) {
                soundManager.PlaySFX("PlayerBounce", transform.position);
                CheckKnockback(script, collision.GetContact(0).point);
                if (!impactPS.isPlaying)
                    impactPS.Play();
            }
        }
    }




    private void OnDrawGizmos() {


        if (showGroundedCheck) {
            Vector3 startingPosition = transform.position;
            Vector3 endPosition = transform.position;
            //startingPosition.y += 1;

            Vector3 size;
            float offset;
            if (heldCreature) {
                size = pickupBoxColliderSize;
                offset = pickupBoxColliderCenter.y;
            }
            else {
                size = normalBoxColliderSize;
                offset = normalBoxColliderCenter.y;
            }

            startingPosition.y += offset;
            endPosition.y += offset - 0.1f;

            Color startColor = Color.blue;
            startColor.a = 0.5f;
            Color endColor = Color.red;
            endColor.a = 0.5f;
            Gizmos.color = startColor;
            Gizmos.DrawCube(startingPosition, size);
            Gizmos.color = endColor;
            Gizmos.DrawCube(endPosition, size);
        }
        if (showPickupCheck) {
            Vector3 boxcastOrigin = transform.position;
            boxcastOrigin += transform.rotation * pickupCheckOffset;
            Vector3 boxSize = new Vector3(pickupCheckBoxSize, pickupCheckBoxSize, pickupCheckBoxSize);
            Gizmos.color = Color.yellow;
            Gizmos.DrawCube(boxcastOrigin, boxSize);
        }
        if (showPathCheck) {
            Vector3 start = transform.position;
            Vector3 target = transform.position;
            if (heldCreature) {
                start += transform.rotation * pickupBoxColliderCenter;
                target += transform.rotation * pickupBoxColliderCenter;
            }
            else {
                start += transform.rotation * normalBoxColliderCenter;
                target += transform.rotation * normalBoxColliderCenter;
            }

            start.x -= pathCheckOffset * transform.forward.x;
            start.z -= pathCheckOffset * transform.forward.z;
            target.x += pathCheckOffset * transform.forward.x;
            target.z += pathCheckOffset * transform.forward.z;

            Vector3 Extent;
            if (heldCreature)
                Extent = pickupBoxColliderSize;
            else
                Extent = normalBoxColliderSize;

            Color startColor = Color.blue;
            startColor.a = 0.5f;
            Color targetColor = Color.red;
            targetColor.a = 0.5f;
            Gizmos.color = startColor;
            Gizmos.DrawCube(start, Extent);
            Gizmos.color = targetColor;
            Gizmos.DrawCube(target, Extent);
        }
    }
}
