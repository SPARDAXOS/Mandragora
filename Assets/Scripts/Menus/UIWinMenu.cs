using UnityEngine;

public class UIWinMenu : MonoBehaviour
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
    public void Credits()
    {
        gameInstance.SetGameState(GameInstance.GameState.CREDITS_MENU);
    }

    public void MainMenu()
    {
        gameInstance.SetGameState(GameInstance.GameState.MAIN_MENU);
    }
}
