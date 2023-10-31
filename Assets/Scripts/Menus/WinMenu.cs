using UnityEngine;

public class WinMenu : MonoBehaviour {
    private bool initialize = false;
    private GameInstance gameInstance = null;


    public void Initialize(GameInstance reference) {
        if (initialize)
            return;

        gameInstance = reference;
        initialize = true;
    }


    public void RetryButton() {
        gameInstance.SetGameState(GameInstance.GameState.PLAYING);
    }
    public void QuitButton() {
        gameInstance.SetGameState(GameInstance.GameState.MAIN_MENU);
    }
}
