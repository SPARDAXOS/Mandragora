using UnityEngine;
using Mandragora;
using UnityEditor;
using System.Collections.Generic;

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
        PLAYING
    }

    [SerializeField] private ResourcesBundle entitiesBundle;
    [SerializeField] private ResourcesBundle levelsBundle;
    [SerializeField] private PlayerControlScheme player1ControlScheme;
    [SerializeField] private PlayerControlScheme player2ControlScheme;


    public GameState currentGameState = GameState.NONE;
    private Dictionary<string, GameObject> entitiesResources;
    private Dictionary<string, GameObject> levelsResources;






    private GameObject level1;
    private Level level1Script;

    private GameObject player1 = null;
    private GameObject player2 = null;

    private GameObject mainMenu = null;
    private GameObject eventSystem = null;
    private GameObject mainCamera = null;

    private Player player1Script = null;
    private Player player2Script = null;
    private CameraMovement cameraScript = null;
    private MainMenu mainMenuScript = null;
 


    void Update() {
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
    public static void Abort(string message) {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
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
        if (!entitiesResources["MainMenu"])
            Abort("Failed to find MainMenu resource");
        else {
            mainMenu = Instantiate(entitiesResources["MainMenu"]);
            mainMenuScript = mainMenu.GetComponent<MainMenu>();
            mainMenuScript.Initialize(this);
            mainMenu.SetActive(false);
        }

        if (!entitiesResources["Player"])
            Abort("Failed to find Player resource");
        else {
            player1 = Instantiate(entitiesResources["Player"]);
            player1.name = "Player_1";
            player1Script = player1.GetComponent<Player>();
            player1Script.Initialize();
            player1Script.SetControlScheme(player1ControlScheme);
            player1Script.SetColor(Color.blue);
            player1.SetActive(false);

            player2 = Instantiate(entitiesResources["Player"]);
            player2.name = "Player_2";
            player2Script = player2.GetComponent<Player>();
            player2Script.Initialize();
            player2Script.SetControlScheme(player2ControlScheme);
            player2Script.SetColor(Color.red);
            player2.SetActive(false);
        }


        if (!entitiesResources["MainCamera"])
            Abort("Failed to find MainCamera resource");
        else {
            mainCamera = Instantiate(entitiesResources["MainCamera"]);
            cameraScript = mainCamera.GetComponent<CameraMovement>();
            cameraScript.Initialize();
            cameraScript.AddTarget(player1);
            cameraScript.AddTarget(player2);
        }

        if (!entitiesResources["EventSystem"])
            Abort("Failed to find EventSystem resource");
        else
            eventSystem = Instantiate(entitiesResources["EventSystem"]);




        if (!levelsResources["Level1"])
            Abort("Failed to find Level1 resource");
        else {
            level1 = Instantiate(levelsResources["Level1"]);
            level1Script = level1.GetComponent<Level>();
            level1Script.Initialize();
        }
    }


    private void UpdatePlayingState() {
        player1Script.Tick();
        player2Script.Tick();
        cameraScript.FixedTick();
    }
    private void UpdateFixedPlayingState() {
        player1Script.FixedTick();
        player2Script.FixedTick();
    }


    public void SetGameState(GameState state) {
        switch (state) {
            case GameState.NONE:
                return;
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
            case GameState.PLAYING:
                SetupPlayingState();
                break;
        }
    }
    private void SetupMainMenuState() {
        SetCursorState(true);
        HideAllMenus();

        mainMenu.SetActive(true);
    }
    private void SetupSettingsMenuState() {
        SetCursorState(true);
        HideAllMenus();

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
    }
    private void SetupLoseMenuState() {
        SetCursorState(true);
        HideAllMenus();

        //DRY
        player1.SetActive(false);
        player2.SetActive(false);
        player1Script.DisableInput();
        player2Script.DisableInput();
    }
    private void SetupPlayingState() {
        SetCursorState(true);
        HideAllMenus();

        //DRY
        player1.transform.position = level1Script.GetPlayer1SpawnPosition();
        player2.transform.position = level1Script.GetPlayer2SpawnPosition();

        player1.SetActive(true);
        player2.SetActive(true);
        player1Script.EnableInput();
        player2Script.EnableInput();
        currentGameState = GameState.PLAYING;
    }

    private void SetCursorState(bool state) {
        Cursor.visible = state;
        if (state)
            Cursor.lockState = CursorLockMode.None;
        else
            Cursor.lockState = CursorLockMode.Locked;
    }
    private void HideAllMenus() {
        //Add all menus here
        mainMenu.SetActive(false);
    }
}
