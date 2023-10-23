using UnityEngine;

public class UIPauseMenu : MonoBehaviour
{
    private bool initialize = false;
    private GameInstance gameInstance = null;


    public void Initialize(GameInstance reference) {
        if (initialize)
            return;

        gameInstance = reference;
        initialize = true;
    }

    public void ResumeButton() {
        gameInstance.UnpauseGame();
    }
    public void SettingsButton() {
        gameInstance.SetGameState(GameInstance.GameState.SETTINGS_MENU);
    }
    public void QuitButton() {
        //END GAME
        gameInstance.SetGameState(GameInstance.GameState.MAIN_MENU);
    }
}
