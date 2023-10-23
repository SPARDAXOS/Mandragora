using Mandragora;
using UnityEngine;


public class Player : MonoBehaviour {

    [SerializeField] PlayerStats stats;


    private bool initialized = false;
    private bool isMoving = false;
    private bool isInteractingTrigger = false;
    private bool isInteractingHeld= false;

    public float currentSpeed = 0.0f;
    private Vector3 direction;
    private Vector3 velocity;

    private PlayerControlScheme activeControlScheme = null;

    private SoundManager soundManager = null;


    private ParticleSystem runDustPS = null;
    private TaskStation taskStationInRange = null;

    private Rigidbody rigidbodyComp = null;
    private MeshRenderer meshRendererComp = null;
    private Material mainMaterial;

    public void Initialize(PlayerControlScheme controlScheme, SoundManager soundManager) {
        if (initialized)
            return;

        activeControlScheme = controlScheme;
        this.soundManager = soundManager;

        SetupReferences();
        initialized = true;
    }
    private void SetupReferences() {
        meshRendererComp = GetComponent<MeshRenderer>();
        if (!meshRendererComp)
            GameInstance.Abort("Failed to get MeshRenderer component on " + gameObject.name);
        mainMaterial = meshRendererComp.materials[0];


        runDustPS = transform.Find("RunDustPS").GetComponent<ParticleSystem>();
        Utility.Validate(runDustPS, "Failed to find RunDustPS.", Utility.ValidationType.WARNING);


        rigidbodyComp = GetComponent<Rigidbody>();
        if (!rigidbodyComp)
            GameInstance.Abort("Failed to get Rigidbody component on " + gameObject.name);
    }



    public void Tick() {
        if (!initialized)
            return;


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
    }
    public void DisableInput() {

        activeControlScheme.movement.Disable();
        activeControlScheme.interact.Disable();
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
        if (isInteractingTrigger)
            soundManager.PlaySFX("SFXTest1", transform.position);

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
            currentSpeed += stats.accelerationspeed * Time.deltaTime;
            if (currentSpeed >= stats.maxSpeed)
                currentSpeed = stats.maxSpeed;
        }
    }
    private void Decelerate() {
        if (currentSpeed > 0.0f) {
            currentSpeed -= stats.decelerationspeed * Time.deltaTime;
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





    public bool IsInteractingTrigger() {
        return isInteractingTrigger;
    }
    public bool IsInteractingHeld() {
        return isInteractingHeld;
    }
}
