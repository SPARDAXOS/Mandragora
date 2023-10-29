using UnityEngine;

public class CreditsMenu : MonoBehaviour {
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

    public void ReturnButton() {
        soundManager.PlaySFX("NextMenu", Vector3.zero, true);
        gameInstance.SetGameState(GameInstance.GameState.MAIN_MENU);
    }
}
