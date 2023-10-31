using UnityEngine;


public class MainMenu : MonoBehaviour {
    private bool initialize = false;
    private GameInstance gameInstance = null;
    private SoundManager soundManager = null;
    private Camera mainCamera = null;

    private Canvas canvasComp = null;
    private Animation animationComp = null;

    private bool fadeInAtStartup = false;

    public void Initialize(GameInstance reference, SoundManager soundManager, Camera mainCamera) {
        if (initialize)
            return;

        this.soundManager = soundManager;
        this.mainCamera = mainCamera;
        gameInstance = reference;
        fadeInAtStartup = true;
        SetupReferences();
        SetupCameraOverlay();
        initialize = true;
    }
    private void SetupReferences() {
        canvasComp = GetComponent<Canvas>();
        animationComp = GetComponent<Animation>();
    }
    private void SetupCameraOverlay() {
        canvasComp.renderMode = RenderMode.ScreenSpaceCamera;
        canvasComp.worldCamera = mainCamera;
        canvasComp.planeDistance = 1.0f; //Not sure!
        Debug.Log(mainCamera);
    }
    public void PlayFadeInAnimation() {
        if (!animationComp.isPlaying && fadeInAtStartup) {
            SetFadeInAtStartUpState(false);
            animationComp.Play("MainMenuFadeIn");
        }
    }
    public void SetFadeInAtStartUpState(bool state) {
        fadeInAtStartup = state;
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
