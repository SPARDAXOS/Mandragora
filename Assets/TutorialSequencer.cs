using Mandragora;
using System.Collections.Generic;
using UnityEngine;

public class TutorialSequencer : MonoBehaviour {

    public enum Tutorials {
        NONE = 0,
        INTRODUCTION,
        MOVEMENT,
        DASH,
        PICKUP,
        WIN_AND_LOSE_CONDITIONS,
        DIRTY_CREATURES,
        HUNGRY_CREATURES,
        ILL_CREATURES,
        SLEEPY_CREATURES,
        TUTORIAL_COMPLETED
    }


    [SerializeField] List<Tutorials> tutorials = new List<Tutorials>();
    [SerializeField] float introductionTutorialDuration = 3.0f;
    [SerializeField] float winLoseTutorialDuration = 3.0f;
    [SerializeField] float tutorialCompletedDuration = 3.0f;

    public List<Tutorials> queuedTutorials = null;

    private bool initialized = false;
    private bool tutorialRunning = false;

    public float timer = 0.0f;



    private GameObject GUI = null;
    private GameObject introductionTutorial = null;
    private GameObject movementTutorial = null;
    private GameObject dashTutorial = null;
    private GameObject pickupTutorial = null;
    private GameObject winLoseTutorial = null;
    private GameObject dirtyCreaturesTutorial = null;
    private GameObject hungryCreaturesTutorial = null;
    private GameObject illCreaturesTutorial = null;
    private GameObject sleepyCreaturesTutorial = null;

    private GameObject completeTutorial = null;


    private bool player1MovementCheck = false;
    private bool player2MovementCheck = false;

    private bool player1DashCheck = false;
    private bool player2DashCheck = false;

    private bool player1PickupCheck = false;
    private bool player2PickupCheck = false;


    private Player player1Script = null;
    private Player player2Script = null;

    private GameInstance gameInstance = null;
    private SoundManager soundManager = null;
    private Level targetLevel = null;
    public Creature tutorialCreature = null;

    private Tutorials currentTutorial = Tutorials.NONE;


    public void Initialize(SoundManager soundManager) {
        if (initialized) 
            return;

        this.soundManager = soundManager;
        queuedTutorials = tutorials;
        SetupReferences();
        HideAllTutorials();
        initialized = true;
    }
    public void Tick() {
        if (!initialized) {
            Debug.LogError("Attempted to tick invalid entity - TutorialSequencer");
            return;
        }


        if (tutorialRunning)
            UpdateTutorials();
    }
    private void SetupReferences() {
        GUI = transform.Find("GUI").gameObject;
        Utility.Validate(GUI, "Failed to get reference to GUI - TutorialSequencer", Utility.ValidationType.ERROR);

        introductionTutorial = GUI.transform.Find("IntroductionTutorial").gameObject;
        movementTutorial = GUI.transform.Find("MovementTutorial").gameObject;
        dashTutorial = GUI.transform.Find("DashTutorial").gameObject;
        pickupTutorial = GUI.transform.Find("PickupTutorial").gameObject;
        winLoseTutorial = GUI.transform.Find("WinLoseTutorial").gameObject;
        dirtyCreaturesTutorial = GUI.transform.Find("DirtyCreaturesTutorial").gameObject;
        hungryCreaturesTutorial = GUI.transform.Find("HungryCreaturesTutorial").gameObject;
        illCreaturesTutorial = GUI.transform.Find("IllCreaturesTutorial").gameObject;
        sleepyCreaturesTutorial = GUI.transform.Find("SleepyCreaturesTutorial").gameObject;
        completeTutorial = GUI.transform.Find("CompleteTutorial").gameObject;

        Utility.Validate(introductionTutorial, "Failed to get reference to IntroductionTutorial - TutorialSequencer", Utility.ValidationType.ERROR);
        Utility.Validate(movementTutorial, "Failed to get reference to MovementTutorial - TutorialSequencer", Utility.ValidationType.ERROR);
        Utility.Validate(dashTutorial, "Failed to get reference to DashTutorial - TutorialSequencer", Utility.ValidationType.ERROR);
        Utility.Validate(pickupTutorial, "Failed to get reference to PickupTutorial - TutorialSequencer", Utility.ValidationType.ERROR);
        Utility.Validate(winLoseTutorial, "Failed to get reference to WinLoseTutorial - TutorialSequencer", Utility.ValidationType.ERROR);
        Utility.Validate(dirtyCreaturesTutorial, "Failed to get reference to DirtyCreaturesTutorial - TutorialSequencer", Utility.ValidationType.ERROR);
        Utility.Validate(hungryCreaturesTutorial, "Failed to get reference to HungryCreaturesTutorial - TutorialSequencer", Utility.ValidationType.ERROR);
        Utility.Validate(illCreaturesTutorial, "Failed to get reference to IllCreaturesTutorial - TutorialSequencer", Utility.ValidationType.ERROR);
        Utility.Validate(sleepyCreaturesTutorial, "Failed to get reference to SleepyCreaturesTutorial - TutorialSequencer", Utility.ValidationType.ERROR);
        Utility.Validate(completeTutorial, "Failed to get reference to CompleteTutorial - TutorialSequencer", Utility.ValidationType.ERROR);
    }
    public void StartTutorial(GameInstance gameInstance, Level target) {
        if (!target) {
            Debug.LogError("Unable to start tutorial with invalid level reference!");
            return;
        }

        this.gameInstance = gameInstance;
        targetLevel = target;
        player1Script = gameInstance.GetPlayer1Script();
        player2Script = gameInstance.GetPlayer2Script();
        tutorialCreature = targetLevel.StartTutorial();
        tutorialRunning = true;
    }


    private void UpdateTutorials() {

        //See if you can start from NextTutorial();

        if (currentTutorial == Tutorials.NONE) {
            queuedTutorials.Remove(0);
            currentTutorial = queuedTutorials[0];
        }




        //Remove entry 0 from tutorials whenever one is finished! Its a queue.

        if (currentTutorial == Tutorials.INTRODUCTION) {
            if (timer == 0.0f) {
                timer = introductionTutorialDuration;
                HideAllTutorials();
                introductionTutorial.gameObject.SetActive(true);
                return;
            }
            else {
                timer -= Time.deltaTime;
                if (timer <= 0.0f) {
                    timer = 0.0f;
                    NextTutorial();
                }
            }
        }
        else if (currentTutorial == Tutorials.MOVEMENT) {
            if (!movementTutorial.activeInHierarchy) {
                HideAllTutorials();
                movementTutorial.SetActive(true);
            }

            if (player1Script.IsMoving())
                player1MovementCheck = true;
            if (player2Script.IsMoving())
                player2MovementCheck = true;

            if (player1MovementCheck && player2MovementCheck)
                NextTutorial();
        }
        else if (currentTutorial == Tutorials.DASH) {
            if (!dashTutorial.activeInHierarchy) {
                HideAllTutorials();
                dashTutorial.SetActive(true);
            }

            if (player1Script.IsDashing())
                player1DashCheck = true;
            if (player2Script.IsDashing())
                player2DashCheck = true;

            if (player1DashCheck && player2DashCheck)
                NextTutorial();
        }
        else if (currentTutorial == Tutorials.PICKUP) {
            if (!pickupTutorial.activeInHierarchy) {
                HideAllTutorials();
                pickupTutorial.SetActive(true);
            }

            if (player1Script.GetHeldCreature())
                player1PickupCheck = true;
            if (player2Script.GetHeldCreature())
                player2PickupCheck = true;

            if (player1PickupCheck && player2PickupCheck)
                NextTutorial();
        }
        else if (currentTutorial == Tutorials.WIN_AND_LOSE_CONDITIONS) {
            if (!winLoseTutorial.activeInHierarchy) {
                HideAllTutorials();
                winLoseTutorial.SetActive(true);
                timer = winLoseTutorialDuration;
                return;
            }
            else {
                //Can be broken out!
                timer -= Time.deltaTime;
                if (timer <= 0.0f) {
                    timer = 0.0f;
                    NextTutorial();
                }
            }
        }
        else if (currentTutorial == Tutorials.DIRTY_CREATURES) {
            if (!dirtyCreaturesTutorial.activeInHierarchy) {
                HideAllTutorials();
                dirtyCreaturesTutorial.SetActive(true);
                tutorialCreature.AddTask(TaskStation.TaskType.BATHING);
            }

            if (!tutorialCreature.DoesRequireTask(TaskStation.TaskType.BATHING))
                NextTutorial();
        }
        else if (currentTutorial == Tutorials.HUNGRY_CREATURES) {
            if (!hungryCreaturesTutorial.activeInHierarchy) {
                HideAllTutorials();
                hungryCreaturesTutorial.SetActive(true);
                tutorialCreature.AddTask(TaskStation.TaskType.FEEDING);
            }

            if (!tutorialCreature.DoesRequireTask(TaskStation.TaskType.FEEDING))
                NextTutorial();
        }
        else if (currentTutorial == Tutorials.ILL_CREATURES) {
            if (!illCreaturesTutorial.activeInHierarchy) {
                HideAllTutorials();
                illCreaturesTutorial.SetActive(true);
                tutorialCreature.AddTask(TaskStation.TaskType.HEALING);
            }

            if (!tutorialCreature.DoesRequireTask(TaskStation.TaskType.HEALING))
                NextTutorial();
        }
        else if (currentTutorial == Tutorials.SLEEPY_CREATURES) {
            if (!sleepyCreaturesTutorial.activeInHierarchy) {
                HideAllTutorials();
                sleepyCreaturesTutorial.SetActive(true);
                tutorialCreature.AddTask(TaskStation.TaskType.SLEEPING);
            }

            if (!tutorialCreature.DoesRequireTask(TaskStation.TaskType.SLEEPING))
                NextTutorial();
        }
        else if (currentTutorial == Tutorials.TUTORIAL_COMPLETED) {
            if (!completeTutorial.activeInHierarchy) {
                HideAllTutorials();
                completeTutorial.SetActive(true);
                timer = tutorialCompletedDuration;
                return;
            }
            else {
                //Can be broken out!
                timer -= Time.deltaTime;
                if (timer <= 0.0f) {
                    timer = 0.0f;
                    NextTutorial();
                }
            }
        }
    }








    private void HideAllTutorials() {
        introductionTutorial.SetActive(false);
        movementTutorial.SetActive(false);
        dashTutorial.SetActive(false);
        pickupTutorial.SetActive(false);
        winLoseTutorial.SetActive(false);
        dirtyCreaturesTutorial.SetActive(false);
        hungryCreaturesTutorial.SetActive(false);
        illCreaturesTutorial.SetActive(false);
        sleepyCreaturesTutorial.SetActive(false);
        completeTutorial.SetActive(false);
    }
    private void NextTutorial() {
        HideAllTutorials();

        queuedTutorials.RemoveAt(0);
        if (queuedTutorials.Count == 0) {
            tutorialRunning = false;
            targetLevel.StartLevel();
            Debug.Log("Tutorial is over! List at least is empty!");
            return;
        }
        soundManager.PlaySFX("NextTutorial", gameInstance.GetCameraScript().transform.position); //Make better solution
        currentTutorial = queuedTutorials[0];
    }
    public bool IsTutorialRunning() {
        return tutorialRunning;
    }
}
