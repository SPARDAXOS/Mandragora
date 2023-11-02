using Mandragora;
using System.Collections.Generic;
using UnityEngine;

public class TutorialsSequencer : MonoBehaviour {

    public enum Tutorials {
        NONE = 0,
        INTRODUCTION,
        MOVEMENT,
        DASH,
        PICKUP,
        THROW,
        WIN_AND_LOSE_CONDITIONS,
        DIRTY_CREATURES,
        HUNGRY_CREATURES,
        ILL_CREATURES,
        SLEEPY_CREATURES,
        DELIVER_CREATURES,
        TUTORIAL_COMPLETED
    }


    [SerializeField] List<Tutorials> tutorials = new List<Tutorials>();
    [SerializeField] float defaultTutorialUpdateDuration = 2.0f;
    [SerializeField] float introductionTutorialDuration = 3.0f;
    [SerializeField] float winLoseTutorialDuration = 3.0f;
    [SerializeField] float tutorialCompletedDuration = 3.0f;

    public List<Tutorials> queuedTutorials = null;

    private bool initialized = false;
    private bool tutorialsRunning = false;
    private bool tutorialsPaused = false;

    public float timer = 0.0f;



    private GameObject GUI = null;
    private GameObject introductionTutorial = null;
    private GameObject movementTutorial = null;
    private GameObject dashTutorial = null;
    private GameObject pickupTutorial = null;
    private GameObject throwingTutorial = null;
    private GameObject winLoseTutorial = null;
    private GameObject dirtyCreaturesTutorial = null;
    private GameObject hungryCreaturesTutorial = null;
    private GameObject illCreaturesTutorial = null;
    private GameObject sleepyCreaturesTutorial = null;
    private GameObject deliveringTutorial = null;

    private GameObject completeTutorial = null;


    private bool player1MovementCheck = false;
    private bool player2MovementCheck = false;
    private bool player1DashCheck = false;
    private bool player2DashCheck = false;
    private bool player1PickupCheck = false;
    private bool player2PickupCheck = false;
    private bool player1ThrowCheck = false;
    private bool player2ThrowCheck = false;

    private Player player1Script = null;
    private Player player2Script = null;

    private GameInstance gameInstance = null;
    private SoundManager soundManager = null;
    private Level targetLevel = null;
    public Creature tutorialCreature = null;

    private Tutorials currentTutorial = Tutorials.NONE;


    public void Initialize(GameInstance gameInstance, SoundManager soundManager) {
        if (initialized) 
            return;

        this.gameInstance = gameInstance;
        this.soundManager = soundManager;

        SetupReferences();
        HideAllTutorials();
        initialized = true;
    }
    public void Tick() {
        if (!initialized) {
            Debug.LogError("Attempted to tick invalid entity - TutorialSequencer");
            return;
        }


        if (tutorialsRunning && !tutorialsPaused) {
            UpdateTutorials();
            UpdateNextTutorialTimer();
        }
    }
    private void SetupReferences() {
        GUI = transform.Find("GUI").gameObject;
        Utility.Validate(GUI, "Failed to get reference to GUI - TutorialSequencer", Utility.ValidationType.ERROR);

        introductionTutorial = GUI.transform.Find("IntroductionTutorial").gameObject;
        movementTutorial = GUI.transform.Find("MovementTutorial").gameObject;
        dashTutorial = GUI.transform.Find("DashTutorial").gameObject;
        pickupTutorial = GUI.transform.Find("PickupTutorial").gameObject;
        throwingTutorial = GUI.transform.Find("ThrowingTutorial").gameObject;
        winLoseTutorial = GUI.transform.Find("WinLoseTutorial").gameObject;
        dirtyCreaturesTutorial = GUI.transform.Find("DirtyCreaturesTutorial").gameObject;
        hungryCreaturesTutorial = GUI.transform.Find("HungryCreaturesTutorial").gameObject;
        illCreaturesTutorial = GUI.transform.Find("IllCreaturesTutorial").gameObject;
        sleepyCreaturesTutorial = GUI.transform.Find("SleepyCreaturesTutorial").gameObject;
        deliveringTutorial = GUI.transform.Find("DeliveringTutorial").gameObject;
        completeTutorial = GUI.transform.Find("CompleteTutorial").gameObject;

        Utility.Validate(introductionTutorial, "Failed to get reference to IntroductionTutorial - TutorialSequencer", Utility.ValidationType.ERROR);
        Utility.Validate(movementTutorial, "Failed to get reference to MovementTutorial - TutorialSequencer", Utility.ValidationType.ERROR);
        Utility.Validate(dashTutorial, "Failed to get reference to DashTutorial - TutorialSequencer", Utility.ValidationType.ERROR);
        Utility.Validate(pickupTutorial, "Failed to get reference to PickupTutorial - TutorialSequencer", Utility.ValidationType.ERROR);
        Utility.Validate(throwingTutorial, "Failed to get reference to ThrowingTutorial - DeliveringTutorial", Utility.ValidationType.ERROR);
        Utility.Validate(winLoseTutorial, "Failed to get reference to WinLoseTutorial - TutorialSequencer", Utility.ValidationType.ERROR);
        Utility.Validate(dirtyCreaturesTutorial, "Failed to get reference to DirtyCreaturesTutorial - TutorialSequencer", Utility.ValidationType.ERROR);
        Utility.Validate(hungryCreaturesTutorial, "Failed to get reference to HungryCreaturesTutorial - TutorialSequencer", Utility.ValidationType.ERROR);
        Utility.Validate(illCreaturesTutorial, "Failed to get reference to IllCreaturesTutorial - TutorialSequencer", Utility.ValidationType.ERROR);
        Utility.Validate(sleepyCreaturesTutorial, "Failed to get reference to SleepyCreaturesTutorial - TutorialSequencer", Utility.ValidationType.ERROR);
        Utility.Validate(deliveringTutorial, "Failed to get reference to SleepyCreaturesTutorial - DeliveringTutorial", Utility.ValidationType.ERROR);
        Utility.Validate(completeTutorial, "Failed to get reference to CompleteTutorial - TutorialSequencer", Utility.ValidationType.ERROR);
    }
    public void StartTutorials(Level target) {
        if (!target) {
            Debug.LogError("Unable to start tutorial with invalid level reference!");
            return;
        }

        player1Script = gameInstance.GetPlayer1Script();
        player2Script = gameInstance.GetPlayer2Script();

        queuedTutorials = new List<Tutorials>(tutorials);
        targetLevel = target;
        tutorialCreature = targetLevel.StartTutorial();
        tutorialsRunning = true;
        tutorialsPaused = false;

        player1MovementCheck = false;
        player2MovementCheck = false;
        player1DashCheck = false;
        player2DashCheck = false;
        player1PickupCheck = false;
        player2PickupCheck = false;
        player1ThrowCheck = false;
        player2ThrowCheck = false;


        timer = 0.0f;
    }
    public void StopTutorials() {
        if (!tutorialsRunning)
            return;

        player1Script = null;
        player2Script = null;


        queuedTutorials.Clear();
        targetLevel = null;
        tutorialCreature.SetActive(false);
        currentTutorial = Tutorials.NONE;
        timer = 0.0f;

        tutorialsRunning = false;
        tutorialsPaused = false;

        player1MovementCheck = false;
        player2MovementCheck = false;
        player1DashCheck = false;
        player2DashCheck = false;
        player1PickupCheck = false;
        player2PickupCheck = false;
        player1ThrowCheck = false;
        player2ThrowCheck = false;

        HideAllTutorials();
    }
    public void PauseTutorials() {

        HideAllTutorials();
        tutorialsPaused = true;
    }
    public void UnpauseTutorials() {

        ToggleOnCurrentTutorialGUI();
        tutorialsPaused = false;
    }


    private void UpdateTutorials() {
        if (currentTutorial == Tutorials.NONE) {
            queuedTutorials.Remove(0);
            currentTutorial = queuedTutorials[0];
        }

        switch (currentTutorial) {
            case Tutorials.INTRODUCTION:
                UpdateIntroductionTutorial();
                break;
            case Tutorials.MOVEMENT:
                UpdateMovementTutorial();
                break;
            case Tutorials.DASH:
                UpdateDashTutorial();
                break;
            case Tutorials.PICKUP:
                UpdatePickupTutorial();
                break;
            case Tutorials.THROW: 
                UpdateThrowTutorial();
                break;
            case Tutorials.WIN_AND_LOSE_CONDITIONS:
                UpdateWinLoseConditionsTutorial();
                break;
            case Tutorials.DIRTY_CREATURES:
                UpdateDirtyCreaturesTutorial();
                break;
            case Tutorials.HUNGRY_CREATURES:
                UpdateHungryCreaturesTutorial();
                break;
            case Tutorials.ILL_CREATURES:
                UpdateIllCreaturesTutorial();
                break;
            case Tutorials.SLEEPY_CREATURES:
                UpdateSleepyCreaturesTutorial();
                break;
            case Tutorials.DELIVER_CREATURES:
                UpdateDeliverTutorial();
                break;
            case Tutorials.TUTORIAL_COMPLETED:
                UpdateTutorialCompletedMessage();
                break;
        }
    }


    private void UpdateIntroductionTutorial() {
        if (timer == 0.0f) {
            timer = introductionTutorialDuration;
            HideAllTutorials();
            introductionTutorial.gameObject.SetActive(true);
            return;
        }
    }
    private void UpdateMovementTutorial() {
        if (!movementTutorial.activeInHierarchy) {
            HideAllTutorials();
            movementTutorial.SetActive(true);
        }

        if (player1Script.IsMoving())
            player1MovementCheck = true;
        if (player2Script.IsMoving())
            player2MovementCheck = true;

        if (player1MovementCheck && player2MovementCheck && timer == 0.0f)
            timer = defaultTutorialUpdateDuration;
    }
    private void UpdateDashTutorial() {
        if (!dashTutorial.activeInHierarchy) {
            HideAllTutorials();
            dashTutorial.SetActive(true);
        }

        if (player1Script.IsDashing())
            player1DashCheck = true;
        if (player2Script.IsDashing())
            player2DashCheck = true;

        if (player1DashCheck && player2DashCheck && timer == 0.0f)
            timer = defaultTutorialUpdateDuration;
    }
    private void UpdatePickupTutorial() {
        if (!pickupTutorial.activeInHierarchy) {
            HideAllTutorials();
            pickupTutorial.SetActive(true);
        }

        if (player1Script.GetHeldCreature())
            player1PickupCheck = true;
        if (player2Script.GetHeldCreature())
            player2PickupCheck = true;

        if (player1PickupCheck && player2PickupCheck && timer == 0.0f)
            timer = defaultTutorialUpdateDuration;
    }
    private void UpdateThrowTutorial() {
        if (!throwingTutorial.activeInHierarchy) {
            HideAllTutorials();
            throwingTutorial.SetActive(true);
        }

        if (player1Script.IsThrowing())
            player1ThrowCheck = true;
        if (player2Script.IsThrowing())
            player2ThrowCheck = true;

        if (player1ThrowCheck && player2ThrowCheck && timer == 0.0f)
            timer = defaultTutorialUpdateDuration;
    }
    private void UpdateWinLoseConditionsTutorial() {
        if (!winLoseTutorial.activeInHierarchy) {
            HideAllTutorials();
            winLoseTutorial.SetActive(true);
            timer = winLoseTutorialDuration;
            return;
        }
    }
    private void UpdateDirtyCreaturesTutorial() {
        if (!dirtyCreaturesTutorial.activeInHierarchy) {
            HideAllTutorials();
            dirtyCreaturesTutorial.SetActive(true);
            tutorialCreature.AddTask(TaskStation.TaskType.BATHING);
        }

        if (!tutorialCreature.DoesRequireTask(TaskStation.TaskType.BATHING) && timer == 0.0f)
            timer = defaultTutorialUpdateDuration;
    }
    private void UpdateHungryCreaturesTutorial() {
        if (!hungryCreaturesTutorial.activeInHierarchy) {
            HideAllTutorials();
            hungryCreaturesTutorial.SetActive(true);
            tutorialCreature.AddTask(TaskStation.TaskType.FEEDING);
        }

        if (!tutorialCreature.DoesRequireTask(TaskStation.TaskType.FEEDING) && timer == 0.0f)
            timer = defaultTutorialUpdateDuration;
    }
    private void UpdateIllCreaturesTutorial() {
        if (!illCreaturesTutorial.activeInHierarchy) {
            HideAllTutorials();
            illCreaturesTutorial.SetActive(true);
            tutorialCreature.AddTask(TaskStation.TaskType.HEALING);
        }

        if (!tutorialCreature.DoesRequireTask(TaskStation.TaskType.HEALING) && timer == 0.0f)
            timer = defaultTutorialUpdateDuration;
    }
    private void UpdateSleepyCreaturesTutorial() {
        if (!sleepyCreaturesTutorial.activeInHierarchy) {
            HideAllTutorials();
            sleepyCreaturesTutorial.SetActive(true);
            tutorialCreature.AddTask(TaskStation.TaskType.SLEEPING);
        }

        if (!tutorialCreature.DoesRequireTask(TaskStation.TaskType.SLEEPING) && timer == 0.0f)
            timer = defaultTutorialUpdateDuration;
    }
    private void UpdateDeliverTutorial() {
        if (!deliveringTutorial.activeInHierarchy) {
            HideAllTutorials();
            tutorialCreature.SetTutorialCreature(false);
            deliveringTutorial.SetActive(true);
        }

        if (!tutorialCreature.GetActive() && timer == 0.0f)
            timer = defaultTutorialUpdateDuration;
    }
    private void UpdateTutorialCompletedMessage() {
        if (!completeTutorial.activeInHierarchy) {
            HideAllTutorials();
            completeTutorial.SetActive(true);
            timer = tutorialCompletedDuration;
        }
    }


    private void UpdateNextTutorialTimer() {
        if (timer > 0.0f) {
            timer -= Time.deltaTime;
            if (timer <= 0.0f) {
                timer = 0.0f;
                NextTutorial();
            }
        }
    }

    private void ToggleOnCurrentTutorialGUI() {
        switch (currentTutorial) {
            case Tutorials.INTRODUCTION:
                introductionTutorial.SetActive(true);
                break;
            case Tutorials.MOVEMENT:
                movementTutorial.SetActive(true);
                break;
            case Tutorials.DASH:
                dashTutorial.SetActive(true);
                break;
            case Tutorials.PICKUP:
                pickupTutorial.SetActive(true);
                break;
            case Tutorials.THROW:
                throwingTutorial.SetActive(true);
                break;
            case Tutorials.WIN_AND_LOSE_CONDITIONS:
                winLoseTutorial.SetActive(true);
                break;
            case Tutorials.DIRTY_CREATURES:
                dirtyCreaturesTutorial.SetActive(true);
                break;
            case Tutorials.HUNGRY_CREATURES:
                hungryCreaturesTutorial.SetActive(true);
                break;
            case Tutorials.ILL_CREATURES:
                illCreaturesTutorial.SetActive(true);
                break;
            case Tutorials.SLEEPY_CREATURES:
                sleepyCreaturesTutorial.SetActive(true);
                break;
            case Tutorials.DELIVER_CREATURES:
                deliveringTutorial.SetActive(true);
                break;
            case Tutorials.TUTORIAL_COMPLETED:
                completeTutorial.SetActive(true);
                break;
        }
    }
    private void HideAllTutorials() {
        introductionTutorial.SetActive(false);
        movementTutorial.SetActive(false);
        dashTutorial.SetActive(false);
        pickupTutorial.SetActive(false);
        throwingTutorial.SetActive(false);
        winLoseTutorial.SetActive(false);
        dirtyCreaturesTutorial.SetActive(false);
        hungryCreaturesTutorial.SetActive(false);
        illCreaturesTutorial.SetActive(false);
        sleepyCreaturesTutorial.SetActive(false);
        deliveringTutorial.SetActive(false);
        completeTutorial.SetActive(false);
    }
    private void NextTutorial() {
        HideAllTutorials();

        queuedTutorials.RemoveAt(0);
        if (queuedTutorials.Count == 0) {
            currentTutorial = Tutorials.NONE;
            tutorialsRunning = false;
            tutorialsPaused = false;

            player1MovementCheck = false;
            player2MovementCheck = false;
            player1DashCheck = false;
            player2DashCheck = false;
            player1PickupCheck = false;
            player2PickupCheck = false;
            player1ThrowCheck = false;
            player2ThrowCheck = false;

            gameInstance.StartLevelCountdown();
            return;
        }
        soundManager.PlaySFX("NextTutorial", Vector3.zero, true);
        currentTutorial = queuedTutorials[0];
    }
    public bool IsTutorialRunning() {
        return tutorialsRunning;
    }
}
