using UnityEngine;

public class PauseMenu : MonoBehaviour {
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

    public void ResumeButton() {
        soundManager.PlaySFX("NextMenu", Vector3.zero, true);
        gameInstance.UnpauseGame();
    }
    public void SettingsButton() {
        soundManager.PlaySFX("NextMenu", Vector3.zero, true);
        gameInstance.SetGameState(GameInstance.GameState.SETTINGS_MENU);
    }
    public void QuitButton() {
        soundManager.PlaySFX("Quit", Vector3.zero, true);
        gameInstance.QuitGame();
    }
}
