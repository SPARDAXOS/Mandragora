using Mandragora;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Player;

public class TaskStation : MonoBehaviour {
    public enum TaskType {
        NONE = 0,
        BATHING,
        FEEDING,
        HEALING,
        SLEEPING
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
    [Range(1.0f, 20.0f)][SerializeField] private float mashIncreaseRate = 5.0f;

    [Space(10)]
    [Header("Hold")]
    [Range(0.01f, 1.0f)][SerializeField] private float holdIncreaseRate = 0.5f;

    [Space(10)]
    [Header("QTE")]
    [SerializeField] private KeyCode[] player1PossibleQTEKeys;
    [SerializeField] private KeyCode[] player2PossibleQTEKeys;
    [Range(1, 10)][SerializeField] private uint QTECount = 5;

    [Space(10)]
    [Header("Settings")]
    [SerializeField] private bool customHeldSpot = false;
    [SerializeField] private bool persistentParticles = true;
    [SerializeField] private bool interactParticles = true;
    [SerializeField] private bool sfxInterruptable = false;
    [SerializeField] private bool endOnlySFX = false;
    [SerializeField] private bool endOnlyVFX = false;

    private Transform customHeldSpotTransform = null;



    private bool initialized = false;

    private PlayerType playerType = PlayerType.NONE;

    private MainCamera mainCamera = null;
    private SoundManager soundManager = null;
    public Player targetPlayer = null;
    public List<bool> playersInRange = new List<bool>();

    public bool interactionOngoing = false;

    public string lastInputedKey = null;
    public KeyCode currentTargetQTE = KeyCode.None;
    private uint currentQTECount = 0;

    private bool QTEBarTrigger = false;




    private GameObject GUI = null;
    private GameObject interactionIndicator = null;
    private GameObject QTEIndicator = null;
    private GameObject normalBarFrame = null;
    private GameObject QTEBarFrame = null;

    private ParticleSystem bathBubblePS = null;
    private ParticleSystem foodCrumbsPS = null;
    private ParticleSystem sleepingPS = null;
    private ParticleSystem alchemyPS = null;

    private TextMeshProUGUI QTEIndicatorText = null;
    private Image normalBar = null;

    private Animation QTEBarAnimationComp = null;



    void OnGUI() {
        if (!interactionOngoing)
            return;

        Event eventRef = Event.current;
        if (eventRef.isKey)
            lastInputedKey = eventRef.keyCode.ToString().ToLower();
    }
    public void Initialize(MainCamera camera, SoundManager soundManager) {
        if (initialized)
            return;

        this.soundManager = soundManager;
        mainCamera = camera;
        SetupReferences();
        if (persistentParticles)
            EnableParticleSystem();

        initialized = true;
    }
    public void Tick() {
        if (!initialized)
            return;

        UpdateIndicator();
        if (interactionOngoing)
            UpdateInteraction();
    }
    public void ResetState() {
        playersInRange.Clear();
        interactionOngoing = false;
        playerType = PlayerType.NONE;
        DisableInteractionGUI();

        if (!persistentParticles)
            DisableParticleSystem();

        targetPlayer = null;

        //Mash/Hold
        normalBar.fillAmount = 0.0f;

        //QTE
        lastInputedKey = null;
        currentTargetQTE = KeyCode.None;
        currentQTECount = 0;

        //Timed QTE
        QTEBarTrigger = false;
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
        //var SparklePSTransform = transform.Find("SparklePS");
        //Utility.Validate(SparklePSTransform, "Failed to get reference to SparklePS - " + gameObject.name, Utility.ValidationType.ERROR);
        //sparklePS = SparklePSTransform.GetComponent<ParticleSystem>();

        customHeldSpotTransform = transform.Find("HeldSpot");
        Utility.Validate(customHeldSpotTransform, "Failed to get reference to HeldSpot - " + gameObject.name, Utility.ValidationType.ERROR);



        //BathBubble PS
        var BathBubblePSTransform = transform.Find("BathBubblePS");
        Utility.Validate(BathBubblePSTransform, "Failed to get reference to BathBubblePS - " + gameObject.name, Utility.ValidationType.ERROR);
        bathBubblePS = BathBubblePSTransform.GetComponent<ParticleSystem>();

        //FoodCrumbs PS
        var FoodCrumbsTransform = transform.Find("FoodCrumbsPS");
        Utility.Validate(FoodCrumbsTransform, "Failed to get reference to FoodCrumbsPS - " + gameObject.name, Utility.ValidationType.ERROR);
        foodCrumbsPS = FoodCrumbsTransform.GetComponent<ParticleSystem>();

        //Sleepy
        var SleepPSTransform = transform.Find("SleepPS");
        Utility.Validate(SleepPSTransform, "Failed to get reference to SleepPS - " + gameObject.name, Utility.ValidationType.ERROR);
        sleepingPS = SleepPSTransform.GetComponent<ParticleSystem>();

        //Alchemy
        var AlchemyPSTransform = transform.Find("AlchemyPS");
        Utility.Validate(AlchemyPSTransform, "Failed to get reference to AlchemyPS - " + gameObject.name, Utility.ValidationType.ERROR);
        alchemyPS = AlchemyPSTransform.GetComponent<ParticleSystem>();
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
    private void UpdateIndicator() {
        if (playersInRange.Count > 0) {
            if (!interactionOngoing)
                interactionIndicator.SetActive(true);
            else
                interactionIndicator.SetActive(false);

            UpdateIndicatorsRotation();
        }
        else
            interactionIndicator.SetActive(false);
    }
    private void UpdateIndicatorsRotation() {
        Quaternion RotationTowardsCamera = Quaternion.LookRotation(mainCamera.transform.forward, mainCamera.transform.up);
        interactionIndicator.transform.rotation = RotationTowardsCamera;
        normalBarFrame.transform.rotation       = RotationTowardsCamera;
        QTEIndicator.transform.rotation         = RotationTowardsCamera;
        QTEBarFrame.transform.rotation          = RotationTowardsCamera;
    }





    private void UpdateMashInteraction() {
        if (targetPlayer.IsInteractingTrigger()) {
            if (!endOnlySFX)
                PlaySFX();
            if(!persistentParticles && interactParticles)
                EnableParticleSystem();
            normalBar.fillAmount += mashIncreaseRate * Time.deltaTime;
            if (normalBar.fillAmount >= 1.0f) {
                normalBar.fillAmount = 1.0f;
                CompleteInteraction();
            }
        }
    }
    private void UpdateHoldInteraction() {
        if (targetPlayer.IsInteractingHeld()) {
            if (!endOnlySFX)
                PlaySFX();
            if (!persistentParticles && interactParticles)
                EnableParticleSystem();
            normalBar.fillAmount += holdIncreaseRate * Time.deltaTime;
            if (normalBar.fillAmount >= 1.0f) {
                normalBar.fillAmount = 1.0f;
                CompleteInteraction();
            }
        }
    }
    private void UpdateQTEInteraction() {
        if (lastInputedKey == currentTargetQTE.ToString().ToLower()) {
            currentQTECount++;
            normalBar.fillAmount += 1.0f / QTECount;
            if (!endOnlySFX)
                PlaySFX();
            if (!persistentParticles && interactParticles)
                EnableParticleSystem();
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
        if (QTEBarTrigger && targetPlayer.IsInteractingTrigger()) {
            if (!endOnlySFX)
                PlaySFX();
            if (!persistentParticles && interactParticles)
                EnableParticleSystem();
            CompleteInteraction();
        }
    }

    private void PlaySFX() {
        if (taskType == TaskType.NONE)
            return;

        if (taskType == TaskType.BATHING) {
            if (!sfxInterruptable)
                soundManager.PlaySFX("CreatureBath", transform.position, false, false, gameObject);
            else
                soundManager.PlaySFX("CreatureBath", transform.position);
        }
        else if (taskType == TaskType.FEEDING) {
            if (!sfxInterruptable)
                soundManager.PlaySFX("CreatureEat", transform.position, false, false, gameObject);
            else
                soundManager.PlaySFX("CreatureEat", transform.position);
        }
        else if (taskType == TaskType.HEALING) {
            if (!sfxInterruptable)
                soundManager.PlaySFX("CreatureHeal", transform.position, false, false, gameObject);
            else
                soundManager.PlaySFX("CreatureHeal", transform.position);
        }
        else if (taskType == TaskType.SLEEPING) {
            if (!sfxInterruptable)
                soundManager.PlaySFX("CreatureSleep", transform.position, false, false, gameObject);
            else
                soundManager.PlaySFX("CreatureSleep", transform.position);
        }
    }

    private void CompleteInteraction() {
        var heldCreature = targetPlayer.GetHeldCreature();
        if (customHeldSpot) {
            heldCreature.transform.rotation = Quaternion.identity;
        }
        if (endOnlySFX)
            PlaySFX();
        if (endOnlyVFX)
            EnableParticleSystem();

        heldCreature.CompleteTask(taskType);
        DisableInteractionState();
    }


    public bool IsInteractionOngoing() {
        return interactionOngoing;
    }
    public bool IsUsingCustomHeldSpot() {
        return customHeldSpot;
    }
    public Transform GetCustomHeldSpot() {
        return customHeldSpotTransform;
    }


    public bool Interact(Player user) {
        if (interactionOngoing)
            return false;

        Creature creature = user.GetHeldCreature();
        if (!creature)
            return false;
        if (!creature.DoesRequireTask(taskType))
            return false;

        Debug.Log("Interaction started! ");

        targetPlayer = user;
        EnableInteractionState();
        return true;
    }
    private void EnableInteractionState() {
        interactionOngoing = true;
        playerType = targetPlayer.GetPlayerType();
        EnableInteractionGUI();

        if (!persistentParticles || endOnlyVFX)
            EnableParticleSystem();

        targetPlayer.SetInteractingWithTaskStationState(this, true);
        targetPlayer.EnableTaskStationInputState();
        interactionIndicator.SetActive(false);
    }
    private void DisableInteractionState() {
        interactionOngoing = false;
        playerType = PlayerType.NONE;
        DisableInteractionGUI();

        if (!persistentParticles)
            DisableParticleSystem();

        targetPlayer.SetInteractingWithTaskStationState(this, false);
        targetPlayer.DisableTaskStationInputState();
        targetPlayer = null;
    }


    private void EnableParticleSystem() {
        if (taskType == TaskType.BATHING)
            bathBubblePS.Play();

        if (taskType == TaskType.FEEDING)
            foodCrumbsPS.Play();
        
        if (taskType == TaskType.HEALING)
            alchemyPS.Play();

        if (taskType == TaskType.SLEEPING)
            sleepingPS.Play();
    }
    private void DisableParticleSystem() {
        if (taskType == TaskType.BATHING)
            bathBubblePS.Stop();

        if (taskType == TaskType.FEEDING)
            foodCrumbsPS.Stop();

        if (taskType == TaskType.HEALING)
            alchemyPS.Stop();

        if (taskType == TaskType.SLEEPING)
            sleepingPS.Stop();
    }

    private void EnableInteractionGUI() {
        if (actionType == ActionType.MASH || actionType == ActionType.HOLD) {
            normalBarFrame.SetActive(true);
            normalBar.fillAmount = 0.0f;
        }
        else if (actionType == ActionType.QTE) {
            normalBarFrame.SetActive(true);
            QTEIndicator.SetActive(true);
            normalBar.fillAmount = 0.0f;
            currentQTECount = 0;
            lastInputedKey = null;
            UpdateQTE();
        }
        else if (actionType == ActionType.QTE_BAR) {
            QTEBarFrame.SetActive(true);
            QTEBarAnimationComp.Play();
            QTEBarTrigger = false;
        }
    }
    private void DisableInteractionGUI() {
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



    private void UpdateQTE() {
        KeyCode NewQTE = currentTargetQTE;
        int rand = 0;
        while (NewQTE == currentTargetQTE) {
            if (playerType == PlayerType.PLAYER_1) {
                rand = UnityEngine.Random.Range(0, player1PossibleQTEKeys.Length);
                NewQTE = player1PossibleQTEKeys[rand];
            }
            else if (playerType == PlayerType.PLAYER_2) {
                rand = UnityEngine.Random.Range(0, player2PossibleQTEKeys.Length);
                NewQTE = player2PossibleQTEKeys[rand];
            }
            else {
                Debug.LogError("Infinite loop detected at UpdateQTE() - TaskStation");
                return;
            }
        }

        currentTargetQTE = NewQTE;
        if (currentTargetQTE == KeyCode.LeftArrow)
            QTEIndicatorText.text = "\u2190";
        else if (currentTargetQTE == KeyCode.RightArrow)
            QTEIndicatorText.text = "\u2192";
        else if (currentTargetQTE == KeyCode.UpArrow)
            QTEIndicatorText.text = "\u2191";
        else if (currentTargetQTE == KeyCode.DownArrow)
            QTEIndicatorText.text = "\u2193";
        else
            QTEIndicatorText.text = currentTargetQTE.ToString();
    }
    public void ToggleQTEBarTrigger() {
        QTEBarTrigger ^= true;
    }




    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            var script = other.GetComponent<Player>();
            script.SetInTaskStationRange(this, true);
            playersInRange.Add(true);
        }
    }
    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player")) {
            var script = other.GetComponent<Player>();
            if (targetPlayer == script)
                DisableInteractionState();

            script.SetInTaskStationRange(null, false);
            if (playersInRange.Count > 0)
                playersInRange.RemoveAt(0);
        }
    }
}
