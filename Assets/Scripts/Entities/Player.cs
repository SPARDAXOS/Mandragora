using Mandragora;
using UnityEngine;

public class Player : MonoBehaviour {

    public enum PlayerType {
        NONE = 0,
        PLAYER_1,
        PLAYER_2
    }

    [SerializeField] private PlayerStats stats;
    [SerializeField] private float pickupCheckBoxSize = 1.0f;
    [SerializeField] private Vector3 pickupCheckOffset;
    [SerializeField] private LayerMask pickupMask;

    [Header("Debugging")]
    [SerializeField] private bool showPickupTrigger = true;


    private PlayerType playerType = PlayerType.NONE;

    private bool initialized = false;
    private bool isMoving = false;
    private bool isInteractingTrigger = false;
    private bool isDashingTrigger = false;
    private bool isInteractingHeld = false;

    private bool isKnockedback = false;
    public bool isGrounded = false;
    public bool isDashing = false;
    public bool isStunned = false;
    private bool isThrowingTrigger = false;


    public float currentSpeed = 0.0f;
    public float dashTimer = 0.0f;
    public float stunTimer = 0.0f;

    private Vector3 direction;

    private bool inTaskStationRange = false;

    private GameObject pickupPoint = null;

    private PlayerControlScheme activeControlScheme = null;

    private GameInstance gameInstance = null;
    private SoundManager soundManager = null;


    private ParticleSystem runDustPS = null;

    private Creature heldCreature = null;

    private Rigidbody rigidbodyComp = null;
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
    }



    public void Tick() {
        if (!initialized)
            return;

        rigidbodyComp.useGravity = !stats.customGravity;


        UpdateStunTimer();
        UpdateDashTimer();
        UpdateHeldCreature();

        CheckGrounded();
        CheckInput();
        CheckDash();
        CheckPickup();
        CheckThrow();
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
                throwDirection.y = Mathf.Sin(Mathf.Deg2Rad * stats.throwHeightAngle);
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

    private void UpdateDashTimer() {
        if (dashTimer > 0.0f) {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0.0f) {
                //StopDashing(-transform.forward);
                //
                dashTimer = 0.0f;
                isDashing = false;
                currentSpeed = stats.maxSpeed * stats.retainedSpeed;
                Debug.Log("Dashing finished!");
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
                    Debug.Log("Dash interrupted by pickup!");
                    StopDashing(transform.forward);
                }

                heldCreature = script;
                heldCreature.PickUp(this);
            }
        }
    }
    private void UpdateHeldCreature() {
        if (!heldCreature)
            return;

        //heldCreature.transform.position = Vector3.Lerp(heldCreature.transform.position, pickupPoint.transform.position, 0.01f);
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

    public void SetInTaskStationRange(bool state) {
        inTaskStationRange = state;
    }

    public Creature GetHeldCreature() {
        return heldCreature;
    }
    public void DropHeldCreature() {
        if (!heldCreature)
            return;

        //Disable VFX
        heldCreature.PutDown();
        heldCreature = null;
    }


    public void ApplyKnockback(Vector3 direction, float force) {
        if (isKnockedback)
            return;

        rigidbodyComp.velocity = Vector3.zero;

        //rigidbodyComp.velocity = direction * force;
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
    private void StopDashing(Vector3 retainedSpeedDirection) {
        isDashing = false;
        dashTimer = 0.0f;

        Vector3 directionToHit = retainedSpeedDirection - transform.position;
        directionToHit.y = 0.0f;
        direction = -directionToHit;
        currentSpeed = stats.maxSpeed * stats.retainedSpeed;
    }


    private void OnCollisionStay(Collision collision) {
        if (collision == null)
            return;

        if (collision.collider.CompareTag("Floor"))
            return;

        //Move stop dashing to func
        if (isDashing) {
            StopDashing(collision.GetContact(0).point);

            Debug.Log("Adjusted direction while dash hit! - stay");
        }
    }
    private void OnCollisionEnter(Collision collision) {

        if (collision == null)
            return;



        if (collision.collider.CompareTag("Player")) {
            var script = collision.collider.GetComponent<Player>();

            Vector3 knockbackDirection = script.transform.position - transform.position;
            knockbackDirection.Normalize();
            knockbackDirection.y = Mathf.Sin(Mathf.Deg2Rad * stats.knockbackHeightAngle);
            if (isDashing) {
                script.ApplyKnockback(knockbackDirection, (stats.knockbackForce / 2) * stats.knockbackMultiplier);
                ApplyStun(stats.stunDuration, false);
                Debug.Log("Stunned!");
            }
            else
                script.ApplyKnockback(knockbackDirection, stats.knockbackForce / 2);

            if (heldCreature) {
                heldCreature.PutDown();
                heldCreature = null;
            }
        }

        if (isDashing) {
            StopDashing(collision.GetContact(0).point);

            Debug.Log("Adjusted direction while dash hit!");
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

        Gizmos.color = Color.yellow;
        Gizmos.DrawCube(transform.position, Vector3.one);
    }
}
