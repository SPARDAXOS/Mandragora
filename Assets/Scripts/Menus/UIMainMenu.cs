using UnityEngine;


public class UIMainMenu : MonoBehaviour
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


    public void QuitGame()
    {
        GameInstance.Abort("Game Quit!");
    }
    public void Startgame()
    {
        gameInstance.SetGameState(GameInstance.GameState.CUSTOMIZATION_MENU);
    }
    public void Settings()
    {
        gameInstance.SetGameState(GameInstance.GameState.SETTINGS_MENU);
    }
}
