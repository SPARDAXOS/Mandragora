using Mandragora;
using UnityEngine;
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
    [SerializeField] private Vector3 pickupCheckOffset;
    [SerializeField] private LayerMask pickupMask;
    [SerializeField] private Vector3 pickupBoxColliderSize;
    [SerializeField] private Vector3 pickupBoxColliderOffset;

    [Header("Navigation")]
    [SerializeField] private float pathCheckOffset = 0.1f;

    [Header("Debugging")]
    [SerializeField] private bool showPickupTrigger = true;
    [SerializeField] private bool showPathCheck = true;


    private PlayerType playerType = PlayerType.NONE;

    private bool initialized = false;
    private bool isMoving = false;
    private bool isInteractingHeld = false;
    private bool isInteractingTrigger = false;
    private bool isDashingTrigger = false;
    private bool isThrowingTrigger = false;

    private bool isKnockedback = false;
    private bool isGrounded = false;
    private bool isDashing = false;
    private bool isStunned = false;

    private bool pathBlocked = false;


    public float currentSpeed = 0.0f;
    public float dashTimer = 0.0f;
    public float dashCooldownTimer = 0.0f;
    public float stunTimer = 0.0f;

    public Vector3 direction;

    private Vector3 normalBoxColliderSize;
    private Vector3 normalBoxColliderOffset;

    private bool inTaskStationRange = false;

    private GameObject pickupPoint = null;

    private PlayerControlScheme activeControlScheme = null;

    private GameInstance gameInstance = null;
    private SoundManager soundManager = null;


    private ParticleSystem runDustPS = null;

    private Creature heldCreature = null;

    private Rigidbody rigidbodyComp = null;
    private BoxCollider boxColliderComp = null;
    private MeshRenderer meshRendererComp = null;
    private Material mainMaterial;

    public void Initialize(PlayerType type, PlayerControlScheme controlScheme, GameInstance gameInstance, SoundManager soundManager) {
        if (initialized)
            return;

        playerType = type;
        activeControlScheme = controlScheme;
        this.gameInstance = gameInstance;
        this.soundManager = soundManager;

        SetupReferences();
        initialized = true;
    }
    private void SetupReferences() {
        //meshRendererComp = GetComponent<MeshRenderer>();
        //if (!meshRendererComp)
        //    GameInstance.Abort("Failed to get MeshRenderer component on " + gameObject.name);
        //mainMaterial = meshRendererComp.materials[0];

        pickupPoint = transform.Find("PickupPoint").gameObject;

        runDustPS = transform.Find("RunDustPS").GetComponent<ParticleSystem>();
        Utility.Validate(runDustPS, "Failed to find RunDustPS.", Utility.ValidationType.WARNING);

        rigidbodyComp = GetComponent<Rigidbody>();
        if (!rigidbodyComp)
            GameInstance.Abort("Failed to get Rigidbody component on " + gameObject.name);

        boxColliderComp = GetComponent<BoxCollider>();
        if (!boxColliderComp)
            GameInstance.Abort("Failed to get BoxCollider component on " + gameObject.name);

        normalBoxColliderSize = boxColliderComp.size;
        normalBoxColliderOffset = boxColliderComp.center;
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
    }


    public void Tick() {
        if (!initialized)
            return;

        rigidbodyComp.useGravity = !stats.customGravity;


        UpdateStunTimer();
        UpdateDashTimers();
        UpdateHeldCreature();

        CheckGrounded();
        CheckInput();
        CheckDash();
        CheckPickup();
        CheckThrow();
        CheckCollidingObject();
    }
    public void FixedTick() {
        if (!initialized)
            return;


        UpdateRotation();

        if (stats.customGravity)
            UpdateGravity();


        if (isKnockedback) {
            if (isGrounded && rigidbodyComp.velocity.y < 0.0f)
                isKnockedback = false;
        }
        else if (!isDashing)
            UpdateMovement();
    }


    public void SetColor(Color color) {
        if (!initialized)
            return;

        //mainMaterial.color = color;
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
            else {
                Vector3 throwDirection = transform.forward;
                throwDirection.x *= Mathf.Cos(Mathf.Deg2Rad * stats.throwHeightAngle);
                throwDirection.y = Mathf.Sin(Mathf.Deg2Rad * stats.throwHeightAngle);
                throwDirection.z *= Mathf.Cos(Mathf.Deg2Rad * stats.throwHeightAngle);
                heldCreature.ApplyImpulse(throwDirection, stats.throwForce);
                DropHeldCreature();
            }
        }
    }
    private void CheckPickup() {
        if (isInteractingTrigger) {
            if (!heldCreature)
                Pickup();
            else if (heldCreature && !inTaskStationRange)
                DropHeldCreature();
        }
    }
    private void CheckInput() {
        if (!activeControlScheme)
            return;

        isInteractingTrigger = activeControlScheme.interact.triggered;
        isDashingTrigger = activeControlScheme.dash.triggered;
        isThrowingTrigger = activeControlScheme.throwAway.triggered;
        isInteractingHeld = activeControlScheme.interact.IsPressed();
        isMoving = activeControlScheme.movement.IsPressed();

        if (activeControlScheme.pause.triggered)
            gameInstance.PauseGame();

        //Break into update func
        if (isMoving && !runDustPS.isPlaying)
            runDustPS.Play();
        else if (!isMoving && runDustPS.isPlaying)
            runDustPS.Stop();

        if (isMoving) {
            Vector2 input = activeControlScheme.movement.ReadValue<Vector2>();
            direction = new Vector3(input.x, 0.0f , input.y);
            Accelerate();
        }
        else if (currentSpeed > 0.0f)
            Decelerate();
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
        Vector3 velocity = direction * currentSpeed * Time.fixedDeltaTime;
        rigidbodyComp.velocity = new Vector3(velocity.x, rigidbodyComp.velocity.y, velocity.z);
    }
    private void UpdateRotation() {
        transform.forward 
            = Vector3.RotateTowards(transform.forward, direction, stats.turnRate * Time.fixedDeltaTime, 0.0f);
    }
    private void UpdateGravity() {
        if (isGrounded)
            return;

        float gravity = stats.gravityScale * Time.fixedDeltaTime;
        rigidbodyComp.velocity += new Vector3(0.0f, -gravity, 0.0f);
    }

    private void CheckGrounded() {
        Vector3 startingPosition = transform.position;
        startingPosition.y += 1;
        isGrounded = Physics.BoxCast(startingPosition, Vector3.one / 2, -transform.up, transform.rotation, 1.0f);
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
                //currentSpeed = stats.maxSpeed * stats.retainedSpeed;
                Debug.LogWarning("Dashing finished!");
            }
        }
    }
    private void UpdateStunTimer() {
        if (stunTimer > 0.0f) {
            stunTimer -= Time.deltaTime;
            if(stunTimer <= 0.0f) {
                //Consider moving this and the turn on to func!
                stunTimer = 0.0f;
                isStunned = false;
                EnableInteractionInput();
                //Disable VFX
            }
        }
        else
            isStunned = false;
    }

    private void Pickup() {
        Vector3 boxcastOrigin = transform.position;
        boxcastOrigin.x += pickupCheckOffset.x * transform.forward.x;
        boxcastOrigin.y += pickupCheckOffset.y * transform.forward.y;
        boxcastOrigin.z += pickupCheckOffset.z * transform.forward.z;
        Vector3 boxExtent = new Vector3(pickupCheckBoxSize / 2.0f, pickupCheckBoxSize / 2.0f, pickupCheckBoxSize / 2.0f);

        var HitResults = Physics.BoxCastAll(boxcastOrigin, boxExtent, transform.forward, transform.rotation, 0.0f, pickupMask.value);
        if (HitResults != null) {
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

                PickupCreature(script);
            }
        }
    }
    private void UpdateHeldCreature() {
        if (!heldCreature)
            return;

        //heldCreature.transform.position = Vector3.Lerp(heldCreature.transform.position, pickupPoint.transform.position, 1.0f);
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
    public PlayerType GetPlayerType() {
        return playerType;
    }


    public bool GetInTaskStationRange() {
        return inTaskStationRange;
    }
    public void SetInTaskStationRange(bool state) {
        inTaskStationRange = state;
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
        boxColliderComp.center = pickupBoxColliderOffset;
    }
    public void DropHeldCreature() {
        if (!heldCreature)
            return;

        //Disable VFX
        heldCreature.PutDown();
        heldCreature = null;
        boxColliderComp.size = normalBoxColliderSize;
        boxColliderComp.center = normalBoxColliderOffset;
    }


    public void ApplyKnockback(Vector3 direction, float force) {
        if (isKnockedback)
            return;

        rigidbodyComp.velocity = Vector3.zero;
        ApplyImpulse(direction, force);
        isKnockedback = true;
    }
    public void ApplyImpulse(Vector3 direction, float force) {
        rigidbodyComp.velocity += direction * force;
    }
    public void ApplyStun(float duration, bool stack = false) {
        if (stack)
            stunTimer += duration;
        else
            stunTimer = duration;


        if (stunTimer > 0.0f) {
            DisableInteractionInput();
            //Start VFX
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

    private void CheckCollidingObject() {
        Vector3 origin = transform.position;
        origin.y += 0.01f;
        origin.x -= pathCheckOffset * transform.forward.x;
        origin.z -= pathCheckOffset * transform.forward.z;

        RaycastHit hit;
        if (Physics.BoxCast(origin, normalBoxColliderSize / 2, transform.forward, out hit, transform.rotation, pathCheckOffset * 2)) {
            Debug.LogWarning("Blocked by " + hit.collider.name);
            pathBlocked = true;





            if (isDashing) {
                StopDashing();
                Vector3 targetDirection = hit.point - transform.position;
                targetDirection.Normalize();
                ApplyDashRetainedSpeed(-targetDirection);

                Debug.LogWarning("Dash Stopped cause touching!!");
            }
        }
        else
            pathBlocked = false;
    }


    private void OnCollisionEnter(Collision collision) {

        if (collision == null)
            return;

        if (collision.collider.CompareTag("Player")) {
            var script = collision.collider.GetComponent<Player>();

            Vector3 knockbackDirection = script.transform.position - transform.position;
            knockbackDirection.Normalize();

            knockbackDirection.x *= Mathf.Cos(Mathf.Deg2Rad * stats.knockbackHeightAngle);
            knockbackDirection.y = Mathf.Sin(Mathf.Deg2Rad * stats.knockbackHeightAngle);
            knockbackDirection.z *= Mathf.Cos(Mathf.Deg2Rad * stats.knockbackHeightAngle);


            if (isDashing) {
                if (heldCreature) {
                    heldCreature.ApplyImpulse(Vector3.up, stats.dashCreatureDropForce);
                    DropHeldCreature();
                }
                StopDashing();
                Vector3 targetDirection = collision.GetContact(0).point - transform.position;
                targetDirection.Normalize();
                ApplyDashRetainedSpeed(-targetDirection); //nOT REALLY NEEDED HERE!

                ApplyStun(stats.stunDuration, false);
                script.ApplyKnockback(knockbackDirection, (stats.knockbackForce / 2) * stats.knockbackMultiplier);
                Debug.LogWarning("Dash Stopped cause hit player!");
            }
            else {
                if (heldCreature) {
                    heldCreature.ApplyImpulse(Vector3.up, stats.knockbackCreatureDropForce);
                    DropHeldCreature();
                }
                script.ApplyKnockback(knockbackDirection, stats.knockbackForce / 2);
            }
        }
    }




    private void OnDrawGizmos() {
        if (showPickupTrigger) {
            Vector3 boxcastOrigin = transform.position;
            boxcastOrigin.x += pickupCheckOffset.x * transform.forward.x;
            boxcastOrigin.y += pickupCheckOffset.y * transform.forward.y;
            boxcastOrigin.z += pickupCheckOffset.z * transform.forward.z;
            Vector3 boxSize = new Vector3(pickupCheckBoxSize, pickupCheckBoxSize, pickupCheckBoxSize);
            Gizmos.color = Color.yellow;
            Gizmos.DrawCube(boxcastOrigin, boxSize);
        }

        if (showPathCheck) {
            Vector3 start = transform.position;
            Vector3 target = transform.position;
      
            start.x -= pathCheckOffset * transform.forward.x;
            start.z -= pathCheckOffset * transform.forward.z;
            target.x += pathCheckOffset * transform.forward.x;
            target.z += pathCheckOffset * transform.forward.z;
        
            Gizmos.color = Color.blue;
            Gizmos.DrawCube(start, normalBoxColliderSize);
            Gizmos.color = Color.red;
            Gizmos.DrawCube(target, normalBoxColliderSize);
        }
    }
}
