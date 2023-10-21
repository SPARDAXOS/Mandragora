using UnityEngine;


public class Player : MonoBehaviour {

    [SerializeField] PlayerStats stats;


    private bool initialized = false;
    private bool isMoving = false;

    public float currentSpeed = 0.0f;
    private Vector3 direction;
    private Vector3 velocity;

    private PlayerControlScheme activeControlScheme = null;

    private SoundManager soundManager = null;

    private Rigidbody rigidbodyComp = null;
    private MeshRenderer meshRendererComp = null;
    private Material mainMaterial;

    public void Initialize(PlayerControlScheme controlScheme, SoundManager soundManager) {
        if (initialized)
            return;

        activeControlScheme = controlScheme;
        this.soundManager = soundManager;

        BindInputCallbacks();
        SetupReferences();
        initialized = true;
    }
    private void SetupReferences() {
        meshRendererComp = GetComponent<MeshRenderer>();
        if (!meshRendererComp)
            GameInstance.Abort("Failed to get MeshRenderer component on " + gameObject.name);
        mainMaterial = meshRendererComp.materials[0];

        rigidbodyComp = GetComponent<Rigidbody>();
        if (!rigidbodyComp)
            GameInstance.Abort("Failed to get Rigidbody component on " + gameObject.name);
    }
    private void BindInputCallbacks() {
        activeControlScheme.interact.performed += InteractInputCallback;
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
    private void OnDestroy() {
        //UNBIND Callbacks from inputactions.
    }

    public void SetColor(Color color) {
        if (!initialized)
            return;

        mainMaterial.color = color;
    }
    public void EnableInput() {
        if (!activeControlScheme)
            return;

        activeControlScheme.movement.Enable();
        activeControlScheme.interact.Enable();
    }
    public void DisableInput() {
        if (!activeControlScheme)
            return;

        activeControlScheme.movement.Disable();
        activeControlScheme.interact.Disable();
    }
    private void CheckInput() {
        if (!activeControlScheme)
            return;

        isMoving = activeControlScheme.movement.IsPressed();

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



    private void InteractInputCallback(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        //Raycast to check for pickups!

        //For testing
        soundManager.PlaySFX("SFXTest1", transform.position);
    }
}
