using UnityEngine;

public class UILoseMenu : MonoBehaviour
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

    public void MainMenu()
    {
        gameInstance.SetGameState(GameInstance.GameState.MAIN_MENU);
    }
}
