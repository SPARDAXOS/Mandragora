using Mandragora;
using TMPro;
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
        QTE_BAR
    }

    [Header("Types")]
    [SerializeField] private TaskType taskType = TaskType.NONE;
    [SerializeField] private ActionType actionType = ActionType.MASH;

    [Space(10)]
    [Header("Mash")]
    [Range(1.0f, 10.0f)][SerializeField] private float mashIncreaseRate = 5.0f;

    [Space(10)]
    [Header("Hold")]
    [Range(0.01f, 1.0f)][SerializeField] private float holdIncreaseRate = 0.5f;

    [Space(10)]
    [Header("QTE")]
    [SerializeField] private KeyCode[] possibleQTEKeys;
    [Range(1, 10)][SerializeField] private uint QTECount = 5;



    private bool initialized = false;

    private MainCamera cameraScript = null;
    private Player targetPlayer = null;
    private bool playerInRange = false;

    private bool interactionOngoing = false;

    private string lastInputedKey = null;
    private KeyCode currentTargetQTE = KeyCode.None;
    private uint currentQTECount = 0;

    private bool QTEBarTrigger = false;




    private GameObject GUI = null;
    private GameObject interactionIndicator = null;
    private GameObject QTEIndicator = null;
    private GameObject normalBarFrame = null;
    private GameObject QTEBarFrame = null;

    private ParticleSystem sparklePS = null;
    private ParticleSystem bathBubblePS = null;

    private TextMeshProUGUI QTEIndicatorText = null;
    private Image normalBar = null;

    private Animation QTEBarAnimationComp = null;




    public void Initialize(MainCamera camera) {
        if (initialized)
            return;

        cameraScript = camera;
        SetupReferences();
        initialized = true;
    }
    public void Tick() {
        if (!initialized)
            return;

        if (playerInRange)
            UpdateIndicatorsRotation();

        if (interactionOngoing)
            UpdateInteraction();
        else if (playerInRange && targetPlayer)
            CheckInput();
    }
    private void SetupReferences() {
        GUI = transform.Find("GUI").gameObject;
        if (!Utility.Validate(GUI, "Failed to get reference to GUI - " + gameObject.name, Utility.ValidationType.ERROR))
            return;

        //Interaction Indicator
        interactionIndicator = GUI.transform.Find("InteractionIndicator").gameObject;
        Utility.Validate(interactionIndicator, "Failed to get reference to InteractionIndicator - " + gameObject.name, Utility.ValidationType.ERROR);
        interactionIndicator.SetActive(false);

        //QTE Indicator
        QTEIndicator = GUI.transform.Find("QTEIndicator").gameObject;
        Utility.Validate(QTEIndicator, "Failed to get reference to QTEIndicator - " + gameObject.name, Utility.ValidationType.ERROR);
        QTEIndicatorText = QTEIndicator.GetComponent<TextMeshProUGUI>();
        Utility.Validate(QTEIndicatorText, "Failed to get component TextMeshProUGUI in QTEIndicatorText - " + gameObject.name, Utility.ValidationType.ERROR);
        QTEIndicator.SetActive(false);

        //Normal Bar
        normalBarFrame = GUI.transform.Find("NormalBarFrame").gameObject;
        Utility.Validate(normalBarFrame, "Failed to get reference to NormalBarFrame - " + gameObject.name, Utility.ValidationType.ERROR);
        normalBar = normalBarFrame.transform.Find("NormalBarFill").GetComponent<Image>();
        Utility.Validate(normalBar, "Failed to get reference to NormalBarFill - " + gameObject.name, Utility.ValidationType.ERROR);
        normalBarFrame.SetActive(false);

        //QTE Bar
        QTEBarFrame = GUI.transform.Find("QTEBarFrame").gameObject;
        Utility.Validate(QTEBarFrame, "Failed to get reference to QTEBarFrame - " + gameObject.name, Utility.ValidationType.ERROR);
        QTEBarAnimationComp = QTEBarFrame.GetComponent<Animation>();
        Utility.Validate(QTEBarAnimationComp, "Failed to get component Animation in QTEBarAnimationComp - " + gameObject.name, Utility.ValidationType.ERROR);
        QTEBarFrame.SetActive(false);

        //Sparkle PS
        var SparklePSTransform = transform.Find("SparklePS");
        Utility.Validate(SparklePSTransform, "Failed to get reference to SparklePS - " + gameObject.name, Utility.ValidationType.ERROR);
        sparklePS = SparklePSTransform.GetComponent<ParticleSystem>();

        //BathBubble PS
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
        else if (actionType == ActionType.QTE_BAR)
            UpdateTimedClickInteraction();
    }
    private void UpdateIndicatorsRotation() {
        Quaternion RotationTowardsCamera = Quaternion.LookRotation(cameraScript.transform.forward, cameraScript.transform.up);
        interactionIndicator.transform.rotation = RotationTowardsCamera;
        normalBarFrame.transform.rotation       = RotationTowardsCamera;
        QTEIndicator.transform.rotation         = RotationTowardsCamera;
        QTEBarFrame.transform.rotation          = RotationTowardsCamera;
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
        lastInputedKey = Input.inputString;
        if (lastInputedKey == currentTargetQTE.ToString().ToLower()) {
            currentQTECount++;
            normalBar.fillAmount += 1.0f / QTECount;
            if (currentQTECount == QTECount) {
                normalBar.fillAmount = 1.0f;
                currentQTECount = 0;
                CompleteInteraction();
            }
            else
                UpdateQTE();
        }
    }
    private void UpdateTimedClickInteraction() {
        if (QTEBarTrigger && targetPlayer.IsInteractingTrigger())
            CompleteInteraction();
    }

    private void CompleteInteraction() {
        //Consider breaking these out into reusable func
        interactionOngoing = false;
        DisableInteraction();
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
        EnableInteraction();
        EnableParticleSystem();
        targetPlayer.DisableMovement();
        interactionIndicator.SetActive(false);
    }


    private void EnableParticleSystem() {
        if (taskType == TaskType.BATHING)
            bathBubblePS.Play();


    }
    private void EnableInteraction() {
        if (actionType == ActionType.MASH || actionType == ActionType.HOLD) {
            normalBarFrame.SetActive(true);
            normalBar.fillAmount = 0.0f;
        }
        else if (actionType == ActionType.QTE) {
            normalBarFrame.SetActive(true);
            QTEIndicator.SetActive(true);
            normalBar.fillAmount = 0.0f;
            UpdateQTE();
        }
        else if (actionType == ActionType.QTE_BAR) {
            QTEBarFrame.SetActive(true);
            QTEBarAnimationComp.Play();
            QTEBarTrigger = false;
        }
    }
    private void DisableInteraction() {
        if (actionType == ActionType.MASH || actionType == ActionType.HOLD)
            normalBarFrame.SetActive(false);
        else if (actionType == ActionType.QTE) {
            normalBarFrame.SetActive(false);
            QTEIndicator.SetActive(false);
        }
        else if(actionType == ActionType.QTE_BAR) {
            QTEBarFrame.SetActive(false);
            QTEBarAnimationComp.Stop();
        }
    }
    private void DisableParticleSystem() {
        if (taskType == TaskType.BATHING)
            bathBubblePS.Stop();

    }


    private void UpdateQTE() {
        KeyCode NewQTE = currentTargetQTE;
        while(NewQTE == currentTargetQTE) {
            int rand = UnityEngine.Random.Range(0, possibleQTEKeys.Length);
            NewQTE = possibleQTEKeys[rand];
        }

        currentTargetQTE = NewQTE;
        QTEIndicatorText.text = currentTargetQTE.ToString();
    }
    public void ToggleQTEBarTrigger() {
        QTEBarTrigger ^= true;
    }


    //VERY BUGGY
    //NOTES:
    //Gets a bit finicky if a player leaves while another is already in.
    //-Also when both players are in but the second one in tries to interact!
    //TODO: Rework this to at the very least the first to click gets to start it!
    private void OnTriggerEnter(Collider other) {
        if (!targetPlayer && other.CompareTag("Player")) {
            targetPlayer = other.GetComponent<Player>();
            targetPlayer.SetInTaskStationRange(true);
            playerInRange = true;
            interactionIndicator.SetActive(true);
        }
    }
    private void OnTriggerExit(Collider other) {
        if (targetPlayer && other.CompareTag("Player")) {
            var script = other.GetComponent<Player>();
            if (targetPlayer == script) {
                if (interactionOngoing) {
                    interactionOngoing = false;
                    DisableInteraction();
                    DisableParticleSystem();
                    targetPlayer.EnableMovement();
                }

                targetPlayer.SetInTaskStationRange(false);
                targetPlayer = null;
                playerInRange = false;
                interactionIndicator.SetActive(false);
            }
        }
    }
}
