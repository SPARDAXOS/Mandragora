using UnityEngine;


public class UIMainMenu : MonoBehaviour {
    private bool initialize = false;
    private GameInstance gameInstance = null;


    public void Initialize(GameInstance reference) {
        if (initialize)
            return;

        gameInstance = reference;
        initialize = true;
    }

    public void StartButton() {
        gameInstance.SetGameState(GameInstance.GameState.PLAYING);
    }
    public void SettingsButton() {
        gameInstance.SetGameState(GameInstance.GameState.SETTINGS_MENU);
    }
    public void CreditsButton() {
        gameInstance.SetGameState(GameInstance.GameState.CREDITS_MENU);
    }
    public void QuitButton() {
        GameInstance.Abort("Game Quit!");
    }
}
