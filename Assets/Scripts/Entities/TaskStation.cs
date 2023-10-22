using Mandragora;
using UnityEngine;
using UnityEngine.UI;

public class TaskStation : MonoBehaviour {
    public enum TaskType {
        NONE = 0,
        BATHING,
        FEEDING
    }
    public enum ActionType {
        MASH,
        HOLD,
        QTE,
        TIMED_CLICK
    }

    [Header("Types")]
    [SerializeField] private TaskType taskType = TaskType.NONE;
    [SerializeField] private ActionType actionType = ActionType.MASH;

    [Space(10)]
    [Header("Interactions")]
    [Range(1.0f, 10.0f)][SerializeField] private float mashIncreaseRate = 5.0f;
    [Range(0.01f, 1.0f)][SerializeField] private float holdIncreaseRate = 0.5f;


    private bool initialized = false;

    private CameraMovement cameraScript = null;
    public Player targetPlayer = null;
    public bool playerInRange = false;

    public bool interactionOngoing = false;

    private GameObject GUI = null;
    private GameObject interactionIndicator = null;
    private GameObject normalBarFrame = null;

    private ParticleSystem sparklePS = null;
    private ParticleSystem bathBubblePS = null;


    private UnityEngine.UI.Image normalBar = null;





    public void Initialize(CameraMovement camera) {
        if (initialized)
            return;

        cameraScript = camera;
        SetupReferences();
        initialized = true;
    }
    public void Tick() {
        if (!initialized)
            return;

        if (interactionOngoing) {
            UpdateIndicatorsRotation();
            UpdateInteraction();
        }
        else if (playerInRange && targetPlayer)
            CheckInput();
    }
    private void SetupReferences() {
        GUI = transform.Find("GUI").gameObject;
        if (!Utility.Validate(GUI, "Failed to get reference to GUI - " + gameObject.name, Utility.ValidationType.ERROR))
            return;

        interactionIndicator = GUI.transform.Find("InteractionIndicator").gameObject;
        Utility.Validate(interactionIndicator, "Failed to get reference to InteractionIndicator - " + gameObject.name, Utility.ValidationType.ERROR);
        interactionIndicator.SetActive(false);

        //Bars
        normalBarFrame = GUI.transform.Find("NormalBarFrame").gameObject;
        Utility.Validate(normalBarFrame, "Failed to get reference to NormalBarFrame - " + gameObject.name, Utility.ValidationType.ERROR);
        normalBar = normalBarFrame.transform.Find("NormalBarFill").GetComponent<Image>();
        Utility.Validate(normalBar, "Failed to get reference to NormalBarFill - " + gameObject.name, Utility.ValidationType.ERROR);

        normalBarFrame.SetActive(false);


        //Particle Systems
        var SparklePSTransform = transform.Find("SparklePS");
        Utility.Validate(SparklePSTransform, "Failed to get reference to SparklePS - " + gameObject.name, Utility.ValidationType.ERROR);
        sparklePS = SparklePSTransform.GetComponent<ParticleSystem>();


        var BathBubblePSTransform = transform.Find("BathBubblePS");
        Utility.Validate(BathBubblePSTransform, "Failed to get reference to BathBubblePS - " + gameObject.name, Utility.ValidationType.ERROR);
        bathBubblePS = BathBubblePSTransform.GetComponent<ParticleSystem>();

    }

    private void UpdateInteraction() {
        if (actionType == ActionType.MASH)
            UpdateMashInteraction();
        else if (actionType == ActionType.HOLD)
            UpdateHoldInteraction();
        else if (actionType == ActionType.QTE)
            UpdateQTEInteraction();
        else if (actionType == ActionType.TIMED_CLICK)
            UpdateTimedClickInteraction();
    }
    private void UpdateIndicatorsRotation() {
        normalBarFrame.transform.rotation = Quaternion.LookRotation(cameraScript.transform.forward, cameraScript.transform.up);
    }


    private void UpdateMashInteraction() {
        if (targetPlayer.IsInteractingTrigger()) {
            normalBar.fillAmount += mashIncreaseRate * Time.deltaTime;
            if (normalBar.fillAmount >= 1.0f) {
                normalBar.fillAmount = 1.0f;
                CompleteInteraction();
            }
        }
    }
    private void UpdateHoldInteraction() {
        if (targetPlayer.IsInteractingHeld()) {
            normalBar.fillAmount += holdIncreaseRate * Time.deltaTime;
            if (normalBar.fillAmount >= 1.0f) {
                normalBar.fillAmount = 1.0f;
                CompleteInteraction();
            }
        }
    }
    private void UpdateQTEInteraction() {

    }
    private void UpdateTimedClickInteraction() {
    }

    private void CompleteInteraction() {
        interactionOngoing = false;
        DisableInteractionBar();
        DisableParticleSystem();
        sparklePS.Play();
        //-Gets picked up creature and toggles off TaskType!
        targetPlayer.EnableMovement();
    }


    private void CheckInput() {
        //And ONLY if current held creature requires the TaskType that this station provides!
        if (targetPlayer.IsInteractingTrigger())
            Intearct();
    }
    private void Intearct() {
        interactionOngoing = true;
        EnableInteractionBar();
        EnableParticleSystem();
        targetPlayer.DisableMovement();
        interactionIndicator.SetActive(false);
    }


    private void EnableParticleSystem() {
        if (taskType == TaskType.BATHING)
            bathBubblePS.Play();


    }
    private void EnableInteractionBar() {
        if (actionType == ActionType.MASH || actionType == ActionType.HOLD) {
            normalBarFrame.SetActive(true);
            normalBar.fillAmount = 0.0f;
        }
        //else if (actionType == ActionType.QTE)
        //    UpdateQTEInteraction();
        //else if (actionType == ActionType.TIMED_CLICK)
        //    UpdateTimedClickInteraction();
    }
    private void DisableInteractionBar() {
        if (actionType == ActionType.MASH || actionType == ActionType.HOLD)
            normalBarFrame.SetActive(false);
        
    }
    private void DisableParticleSystem() {
        if (taskType == TaskType.BATHING)
            bathBubblePS.Stop();

    }


    //NOTES:
    //Gets a bit finicky if a player leaves while another is already in.
    //-Also when both players are in but the second one in tries to interact!
    //TODO: Rework this to at the very least the first to click gets to start it!
    private void OnTriggerEnter(Collider other) {
        if (!targetPlayer && other.CompareTag("Player")) {
            targetPlayer = other.GetComponent<Player>();
            playerInRange = true;
            interactionIndicator.SetActive(true);
        }
    }
    private void OnTriggerExit(Collider other) {
        if (targetPlayer && other.CompareTag("Player")) {
            var script = other.GetComponent<Player>();
            if (targetPlayer == script) {
                targetPlayer = null;
                playerInRange = false;
                interactionIndicator.SetActive(false);
            }
        }
    }
}
