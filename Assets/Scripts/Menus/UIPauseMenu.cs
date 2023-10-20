using UnityEngine;

public class UIPauseMenu : MonoBehaviour
{
    private bool initialize = false;
    private GameInstance gameInstance = null;


    public void Initialize(GameInstance reference)
    {
        if (initialize)
            return;

        gameInstance = reference;
        initialize = true;
    }

    public void UnPause()
    {
        gameInstance.SetGameState(GameInstance.GameState.PLAYING);
    }

    public void Settings()
    {
        gameInstance.SetGameState(GameInstance.GameState.SETTINGS_MENU);
    }

    public void QuitGame()
    {
        GameInstance.Abort("Game Quit!");
    }

    public void MainMenu()
    {
        gameInstance.SetGameState(GameInstance.GameState.MAIN_MENU);
    }
}
