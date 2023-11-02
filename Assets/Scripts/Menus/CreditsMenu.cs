using UnityEngine;

public class CreditsMenu : MonoBehaviour {
    private bool initialize = false;
    private GameInstance gameInstance = null;
    private SoundManager soundManager = null;
    private Camera cameraScript = null;

    private Canvas canvasComp = null;

    public void Initialize(GameInstance gameInstance, SoundManager soundManager, Camera camera) {
        if (initialize)
            return;

        this.gameInstance = gameInstance;
        this.soundManager = soundManager;
        cameraScript = camera;

        SetupReferences();
        SetupCameraOverlay();
        initialize = true;
    }
    private void SetupReferences() {
        canvasComp = GetComponent<Canvas>();
    }
    private void SetupCameraOverlay() {
        canvasComp.renderMode = RenderMode.ScreenSpaceCamera;
        canvasComp.worldCamera = cameraScript;
        canvasComp.planeDistance = 1.0f;
    }

    public void ReturnButton() {
        soundManager.PlaySFX("NextMenu", Vector3.zero, true);
        gameInstance.SetGameState(GameInstance.GameState.MAIN_MENU);
    }
}
