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

    public void Startgame() {
        gameInstance.SetGameState(GameInstance.GameState.PLAYING);
    }
    public void Settings() {
        gameInstance.SetGameState(GameInstance.GameState.SETTINGS_MENU);
    }
    public void QuitGame() {
        GameInstance.Abort("Game Quit!");
    }
}
