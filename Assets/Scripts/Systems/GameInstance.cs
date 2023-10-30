using UnityEngine;
using Mandragora;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UIElements;
using Unity.VisualScripting;

namespace Initialization {
    public class Initialization {

        [RuntimeInitializeOnLoadMethod]
        public static void InitilaizeGame() {
            var resource = Resources.Load<GameObject>("GameInstance");
            var gameInstance = GameObject.Instantiate(resource);
            var Comp = gameInstance.GetComponent<GameInstance>();
            Comp.Initialize();
        }
    }
}

public class GameInstance : MonoBehaviour {
    public enum GameState {
        NONE = 0,
        MAIN_MENU,
        SETTINGS_MENU,
        CUSTOMIZATION_MENU,
        WIN_MENU,
        LOSE_MENU,
        CREDITS_MENU,
        PLAYING,
        PAUSE_MENU
    }
    public enum GameResults {
        WIN,
        LOSE
    }


    [SerializeField] private ResourcesBundle entitiesBundle;
    [SerializeField] private ResourcesBundle levelsBundle;
    [SerializeField] private GameSettings gameSettings;
    [SerializeField] private PlayerControlScheme player1ControlScheme;
    [SerializeField] private PlayerControlScheme player2ControlScheme;
    [SerializeField] private bool playTutorials = false;


    public GameState currentGameState = GameState.NONE;
    private Dictionary<string, GameObject> entitiesResources;
    private Dictionary<string, GameObject> levelsResources;


    private bool gameStarted = false;
    private bool gamePaused = false;


    private GameObject currentLevel;
    private Level currentLevelScript;

    private GameObject player1 = null;
    private GameObject player2 = null;

    private GameObject eventSystem = null;
    private GameObject mainCamera = null;
    private GameObject soundManager = null;
    private GameObject tutorialSequencer = null;
    private GameObject mainMenu = null;
    private GameObject settingsMenu = null;
    private GameObject pauseMenu = null;
    private GameObject winMenu = null;
    private GameObject loseMenu = null;
    private GameObject creditsMenu = null;

    private Player player1Script = null;
    private Player player2Script = null;
    private MainCamera mainCameraScript = null;
    private SoundManager soundManagerScript = null;
    private TutorialSequencer tutorialSequencerScript = null;
    private MainMenu mainMenuScript = null;
    private SettingsMenu settingsMenuScript = null;
    private PauseMenu pauseMenuScript = null;
    private LoseMenu loseMenuScript = null;
    private UIWinMenu winMenuScript = null;
    private CreditsMenu creditsMenuScript = null;



    void Update() {
        //Updated regardless?
        if (soundManagerScript)
            soundManagerScript.Tick();

        switch (currentGameState) {
            case GameState.NONE: 
                break;
            case GameState.PLAYING:
                UpdatePlayingState();
                break;
        }
    }
    private void FixedUpdate() {
        switch (currentGameState) {
            case GameState.NONE:
                break;
            case GameState.PLAYING:
                UpdateFixedPlayingState();
                break;
        }
    }
    private void Awake() {
        DontDestroyOnLoad(gameObject);
    }
    public void Initialize() {
        LoadResources();
        CreateEntities();
        SetGameState(GameState.MAIN_MENU);
    }
    public static void Abort(string message = null) {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
        if (message == null)
            Debug.LogError(message);
#else
        Application.Quit();
#endif
    }
    private void LoadResources() {
        if (!Utility.Validate(entitiesBundle, null, Utility.ValidationType.ERROR))
            Abort("No entities bundle was not set!");
        if (!Utility.Validate(levelsBundle, null, Utility.ValidationType.ERROR))
            Abort("No levels bundle was not set!");

        entitiesResources = new Dictionary<string, GameObject>(entitiesBundle.entries.Length);
        levelsResources = new Dictionary<string, GameObject>(levelsBundle.entries.Length);

        foreach (var entry in entitiesBundle.entries)
            entitiesResources.Add(entry.name, entry.resource);

        foreach (var entry in levelsBundle.entries)
            levelsResources.Add(entry.name, entry.resource);
    }
    private void CreateEntities() {
        if (!entitiesResources["SoundManager"])
            Abort("Failed to find SoundManager resource");
        else {
            soundManager = Instantiate(entitiesResources["SoundManager"]);
            soundManagerScript = soundManager.GetComponent<SoundManager>();
            soundManagerScript.Initialize();
        }

        if (!entitiesResources["TutorialSequencer"])
            Abort("Failed to find TutorialSequencer resource");
        else {
            tutorialSequencer = Instantiate(entitiesResources["TutorialSequencer"]);
            tutorialSequencerScript = tutorialSequencer.GetComponent<TutorialSequencer>();
            tutorialSequencerScript.Initialize(this, soundManagerScript);
        }

        
        if (!entitiesResources["MainMenu"])
            Abort("Failed to find MainMenu resource");
        else {
            mainMenu = Instantiate(entitiesResources["MainMenu"]);
            mainMenuScript = mainMenu.GetComponent<MainMenu>();
            mainMenuScript.Initialize(this, soundManagerScript);
            mainMenu.SetActive(false);
        }

        if (!entitiesResources["SettingsMenu"])
            Abort("Failed to find SettingsMenu resource");
        else {
            settingsMenu = Instantiate(entitiesResources["SettingsMenu"]);
            settingsMenuScript = settingsMenu.GetComponent<SettingsMenu>();
            settingsMenuScript.Initialize(this, soundManagerScript, gameSettings, true); //Here!
            settingsMenu.SetActive(false);
        }

        if (!entitiesResources["WinMenu"])
            Abort("Failed to find WinMenu resource");
        else {
            winMenu = Instantiate(entitiesResources["WinMenu"]);
            winMenuScript = winMenu.GetComponent<UIWinMenu>();
            winMenuScript.Initialize(this);
            winMenu.SetActive(false);
        }

        if (!entitiesResources["LoseMenu"])
            Abort("Failed to find LoseMenu resource");
        else {
            loseMenu = Instantiate(entitiesResources["LoseMenu"]);
            loseMenuScript = loseMenu.GetComponent<LoseMenu>();
            loseMenuScript.Initialize(this);
            loseMenu.SetActive(false);
        }

        if (!entitiesResources["CreditsMenu"])
            Abort("Failed to find CreditsMenu resource");
        else {
            creditsMenu = Instantiate(entitiesResources["CreditsMenu"]);
            creditsMenuScript = creditsMenu.GetComponent<CreditsMenu>();
            creditsMenuScript.Initialize(this, soundManagerScript);
            creditsMenu.SetActive(false);
        }

        if (!entitiesResources["PauseMenu"])
            Abort("Failed to find PauseMenu resource");
        else {
            pauseMenu = Instantiate(entitiesResources["PauseMenu"]);
            pauseMenuScript = pauseMenu.GetComponent<PauseMenu>();
            pauseMenuScript.Initialize(this);
            pauseMenu.SetActive(false);
        }



        if (!entitiesResources["Player"])
            Abort("Failed to find Player resource");
        else {
            player1 = Instantiate(entitiesResources["Player"]);
            player1.name = "Player_1";
            player1Script = player1.GetComponent<Player>();
            player1Script.Initialize(Player.PlayerType.PLAYER_1, player1ControlScheme, this, soundManagerScript);
            player1Script.SetColor(Color.blue);
            player1.SetActive(false);

            player2 = Instantiate(entitiesResources["Player"]);
            player2.name = "Player_2";
            player2Script = player2.GetComponent<Player>();
            player2Script.Initialize(Player.PlayerType.PLAYER_2, player2ControlScheme, this, soundManagerScript);
            player2Script.SetColor(Color.red);
            player2.SetActive(false);
        }


        if (!entitiesResources["MainCamera"])
            Abort("Failed to find MainCamera resource");
        else {
            mainCamera = Instantiate(entitiesResources["MainCamera"]);
            mainCameraScript = mainCamera.GetComponent<MainCamera>();
            mainCameraScript.Initialize();
            mainCameraScript.AddTarget(player1);
            mainCameraScript.AddTarget(player2);
            soundManagerScript.SetMainCamera(mainCameraScript);
        }

        if (!entitiesResources["EventSystem"])
            Abort("Failed to find EventSystem resource");
        else
            eventSystem = Instantiate(entitiesResources["EventSystem"]);




        if (!levelsResources["Level1"])
            Abort("Failed to find Level1 resource");
        else {
            currentLevel = Instantiate(levelsResources["Level1"]);
            currentLevelScript = currentLevel.GetComponent<Level>();
            currentLevelScript.Initialize(this, soundManagerScript);
        }
    }


    private void UpdatePlayingState() {
        player1Script.Tick();
        player2Script.Tick();
        currentLevelScript.Tick();

        if (tutorialSequencerScript.IsTutorialRunning())
            tutorialSequencerScript.Tick();
    }
    private void UpdateFixedPlayingState() {
        player1Script.FixedTick();
        player2Script.FixedTick();
        mainCameraScript.FixedTick();
        currentLevelScript.FixedTick();
    }


    public void SetGameState(GameState state) {
        switch (state) {
            case GameState.NONE:
                Debug.LogWarning("NONE was sent to SetGameState()");
                break;
            case GameState.MAIN_MENU: 
                SetupMainMenuState(); 
                break;
            case GameState.SETTINGS_MENU:
                SetupSettingsMenuState();
                break;
            case GameState.CUSTOMIZATION_MENU:
                SetupCustomizationMenuState();
                break;
            case GameState.WIN_MENU:
                SetupWinMenuState();
                break;
            case GameState.LOSE_MENU:
                SetupLoseMenuState();
                break;
            case GameState.CREDITS_MENU:
                SetupCreditsMenuState();
                break;
            case GameState.PLAYING:
                SetupPlayingState();
                break;
            case GameState.PAUSE_MENU:
                Debug.LogWarning("You cant use SetGameState to pause the game. Use PauseGame() instead!");
                break;
        }
    }
    private void SetupMainMenuState() {
        SetCursorState(true);
        HideAllMenus();

        soundManagerScript.PlayTrack("MainMenu", true);
        mainMenu.SetActive(true);
    }
    private void SetupSettingsMenuState() {
        SetCursorState(true);
        HideAllMenus();

        settingsMenu.SetActive(true);
    }
    private void SetupCustomizationMenuState() {
        SetCursorState(true);
        HideAllMenus();

    }
    private void SetupWinMenuState() {
        SetCursorState(true);
        HideAllMenus();

        player1.SetActive(false);
        player2.SetActive(false);
        player1Script.DisableInput();
        player2Script.DisableInput();

        winMenu.SetActive(true);
    }
    private void SetupLoseMenuState() {
        SetCursorState(true);
        HideAllMenus();

        //DRY
        player1.SetActive(false);
        player2.SetActive(false);
        player1Script.DisableInput();
        player2Script.DisableInput();

        loseMenu.SetActive(true);
    }
    private void SetupCreditsMenuState() {
        SetCursorState(true);
        HideAllMenus();

        creditsMenu.SetActive(true);
    }
    private void SetupPlayingState() {
        SetCursorState(true);
        HideAllMenus();

        //DRY
        player1.transform.position = currentLevelScript.GetPlayer1SpawnPosition();
        player2.transform.position = currentLevelScript.GetPlayer2SpawnPosition();

        player1.SetActive(true);
        player2.SetActive(true);
        player1Script.EnableInput();
        player2Script.EnableInput();
        currentGameState = GameState.PLAYING;
        StartGame();
    }


    public void PauseGame() {
        HideAllMenus();
        SetCursorState(true);

        pauseMenu.SetActive(true);
        Time.timeScale = 0.0f;
        gamePaused = true;
    }
    public void UnpauseGame() {
        HideAllMenus();
        SetCursorState(false);

        pauseMenu.SetActive(false);
        Time.timeScale = 1.0f;
        gamePaused = false;
    }

    public void LevelFinished(GameResults results) {
        Debug.Log("Game is over with results " + results.ToString());

        if (results == GameResults.WIN) {
            soundManagerScript.PlaySFX("YouWin", Vector3.zero, true);
            SetGameState(GameState.WIN_MENU);
        }
        else if (results == GameResults.LOSE) {
            soundManagerScript.PlaySFX("YouLose", Vector3.zero, true);
            SetGameState(GameState.LOSE_MENU);
        }
        gameStarted = false;
    }
    private void StartGame() {
        gameStarted = true;

        if (playTutorials)
            tutorialSequencerScript.StartTutorial(currentLevelScript);
        else
            currentLevelScript.StartLevel();
    }
    public bool IsGameStarted() {
        return gameStarted;
    }


    public Player GetPlayer1Script() {
        return player1Script;
    }
    public Player GetPlayer2Script() {
        return player2Script;
    }



    private void SetCursorState(bool state) {
        UnityEngine.Cursor.visible = state;
        if (state)
            UnityEngine.Cursor.lockState = CursorLockMode.None;
        else
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
    }
    private void HideAllMenus() {
        //Add all menus here
        mainMenu.SetActive(false);
        settingsMenu.SetActive(false);
        winMenu.SetActive(false);
        loseMenu.SetActive(false);
        creditsMenu.SetActive(false);
        pauseMenu.SetActive(false);
        //Customization
    }


    public MainCamera GetCameraScript() {
        return mainCameraScript;
    }
}
