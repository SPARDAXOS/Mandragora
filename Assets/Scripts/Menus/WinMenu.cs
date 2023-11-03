using UnityEngine;

public class WinMenu : MonoBehaviour {
    private bool initialize = false;
    private GameInstance gameInstance = null;
    private SoundManager soundManager = null;

    public void Initialize(GameInstance reference, SoundManager soundManager) {
        if (initialize)
            return;

        this.soundManager = soundManager;
        gameInstance = reference;
        initialize = true;
    }


    public void RetryButton() {
        soundManager.PlaySFX("Retry", Vector3.zero, true);
        gameInstance.StartLevelStartFade();
    }
    public void QuitButton() {
        soundManager.PlaySFX("Quit", Vector3.zero, true);
        gameInstance.SetGameState(GameInstance.GameState.MAIN_MENU);
    }
}
