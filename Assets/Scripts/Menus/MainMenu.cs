using UnityEngine;


public class MainMenu : MonoBehaviour {
    private bool initialize = false;
    private GameInstance gameInstance = null;
    private SoundManager soundManager = null;
    private Camera mainCamera = null;

    private Canvas canvasComp = null;

    public void Initialize(GameInstance reference, SoundManager soundManager, Camera mainCamera) {
        if (initialize)
            return;

        this.soundManager = soundManager;
        this.mainCamera = mainCamera;
        gameInstance = reference;
        SetupReferences();
        SetupCameraOverlay();
        initialize = true;
    }
    private void SetupReferences() {
        canvasComp = GetComponent<Canvas>();
    }
    private void SetupCameraOverlay() {
        canvasComp.renderMode = RenderMode.ScreenSpaceCamera;
        canvasComp.worldCamera = mainCamera;
        Debug.Log(mainCamera);
    }

    public void StartButton() {
        soundManager.PlaySFX("NextMenu", Vector3.zero, true);
        gameInstance.StartLevelStartFade();
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
