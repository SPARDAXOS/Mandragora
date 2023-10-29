using UnityEngine;


public class MainMenu : MonoBehaviour {
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

    public void StartButton() {
        soundManager.PlaySFX("NextMenu", Vector3.zero, true);
        gameInstance.SetGameState(GameInstance.GameState.PLAYING);
    }
    public void SettingsButton() {
        soundManager.PlaySFX("NextMenu", Vector3.zero, true);
        gameInstance.SetGameState(GameInstance.GameState.SETTINGS_MENU);
    }
    public void CreditsButton() {
        soundManager.PlaySFX("NextMenu", Vector3.zero, true);
        gameInstance.SetGameState(GameInstance.GameState.CREDITS_MENU);
    }
    public void QuitButton() {
        GameInstance.Abort("Game Quit!");
    }
}
