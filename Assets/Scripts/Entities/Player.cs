using Mandragora;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

public class Player : MonoBehaviour {

    [SerializeField] private PlayerStats stats;
    [SerializeField] private float pickupCheckBoxSize = 1.0f;
    [SerializeField] private float pickupCheckOffset = 0.5f;
    [SerializeField] private LayerMask pickupMask;

    [Header("Debugging")]
    [SerializeField] private bool showPickupTrigger = true;



    private bool initialized = false;
    private bool isMoving = false;
    private bool isInteractingTrigger = false;
    private bool isInteractingHeld = false;

    private float currentSpeed = 0.0f;
    private Vector3 direction;
    private Vector3 velocity;

    private GameObject pickupPoint = null;

    private PlayerControlScheme activeControlScheme = null;

    private GameInstance gameInstance = null;
    private SoundManager soundManager = null;


    private ParticleSystem runDustPS = null;

    private Creature heldCreature = null;

    private Rigidbody rigidbodyComp = null;
    private MeshRenderer meshRendererComp = null;
    private Material mainMaterial;

    public void Initialize(PlayerControlScheme controlScheme, GameInstance gameInstance, SoundManager soundManager) {
        if (initialized)
            return;

        activeControlScheme = controlScheme;
        this.gameInstance = gameInstance;
        this.soundManager = soundManager;

        SetupReferences();
        initialized = true;
    }
    private void SetupReferences() {
        meshRendererComp = GetComponent<MeshRenderer>();
        if (!meshRendererComp)
            GameInstance.Abort("Failed to get MeshRenderer component on " + gameObject.name);
        mainMaterial = meshRendererComp.materials[0];

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

        if (heldCreature) {
            heldCreature.transform.position = pickupPoint.transform.position;

        }
        
        CheckInput();
    }
    public void FixedTick() {
        if (!initialized)
            return;

        UpdateMovement();
        UpdateRotation();
    }


    public void SetColor(Color color) {
        if (!initialized)
            return;

        mainMaterial.color = color;
    }
    public void EnableInput() {

        activeControlScheme.movement.Enable();
        activeControlScheme.interact.Enable();
        activeControlScheme.pause.Enable();
    }
    public void DisableInput() {

        activeControlScheme.movement.Disable();
        activeControlScheme.interact.Disable();
        activeControlScheme.pause.Disable();
    }
    public void EnableMovement() {
        activeControlScheme.movement.Enable();
    }
    public void DisableMovement() {
        activeControlScheme.movement.Disable();
    }


    private void CheckInput() {
        if (!activeControlScheme)
            return;

        isInteractingTrigger = activeControlScheme.interact.triggered;
        isInteractingHeld = activeControlScheme.interact.IsPressed();
        isMoving = activeControlScheme.movement.IsPressed();
        



        //Testing
        if (isInteractingTrigger) {
            if (!heldCreature)
                Pickup();
            else if (heldCreature) {
               
                heldCreature.PutDown();
                heldCreature = null;
            }
        }

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
        velocity = direction * currentSpeed * Time.fixedDeltaTime;
        rigidbodyComp.velocity = velocity;
    }
    private void UpdateRotation() {
        transform.forward 
            = Vector3.RotateTowards(transform.forward, direction, stats.turnRate * Time.fixedDeltaTime, 0.0f);
    }



    private void Pickup() {
        Vector3 boxcastOrigin = transform.position + transform.forward * pickupCheckOffset;
        Vector3 boxExtent = new Vector3(pickupCheckBoxSize / 2.0f, pickupCheckBoxSize / 2.0f, pickupCheckBoxSize / 2.0f);

        var HitResults = Physics.BoxCastAll(boxcastOrigin, boxExtent, transform.forward, transform.rotation, 0.0f, pickupMask.value);
        if (HitResults != null) {
            foreach (var entry in HitResults) {
                var script = entry.collider.GetComponent<Creature>();
                if (!script) {
                    Debug.LogError("Attempted to pickup invalid Creature that did not own creature script!");
                    continue;
                }

                heldCreature = script;
                heldCreature.PickUp(this);
            }
        }
    }

    public bool IsInteractingTrigger() {
        return isInteractingTrigger;
    }
    public bool IsInteractingHeld() {
        return isInteractingHeld;
    }

    private void OnDrawGizmos() {
        if (showPickupTrigger) {
            Vector3 boxcastOrigin = transform.position + transform.forward * pickupCheckOffset;
            Vector3 boxSize = new Vector3(pickupCheckBoxSize, pickupCheckBoxSize, pickupCheckBoxSize);
            Gizmos.color = Color.yellow;
            Gizmos.DrawCube(boxcastOrigin, boxSize);
        }
    }
}
