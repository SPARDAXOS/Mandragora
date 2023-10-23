using UnityEngine;
using UnityEngine.UI;

public class UISettingsMenu : MonoBehaviour {

    private bool initialize = false;

    private GameInstance gameInstance = null;
    private SoundManager soundManager = null;

    private Slider MasterSlider;
    private Slider SFXSlider;
    private Slider MusicSlider;

    public void Initialize(GameInstance gameInstance, SoundManager soundManager) {
        if (initialize)
            return;

        this.gameInstance = gameInstance;
        this.soundManager = soundManager;
        SetupReference();
        initialize = true;
    }
    private void SetupReference() {
        MasterSlider = transform.Find("MasterSlider").GetComponent<Slider>();
        SFXSlider = transform.Find("SFXSlider").GetComponent<Slider>();
        MusicSlider = transform.Find("MusicSlider").GetComponent<Slider>();

        MasterSlider.value = soundManager.GetMasterVolume();
        SFXSlider.value = soundManager.GetSFXVolume();
        MusicSlider.value = soundManager.GetMusicVolume();
    }

    public void ReturnButton() {   
        if (gameInstance.IsGameStarted())
            gameInstance.SetGameState(GameInstance.GameState.PLAYING);
        else
            gameInstance.SetGameState(GameInstance.GameState.MAIN_MENU);
    }

    public void SetMasterSlider() {
        if (initialize)
            soundManager.SetMasterVolume(MasterSlider.value);
    }
    public void SetSFXSlider() {
        if (initialize)
            soundManager.SetSFXVolume(SFXSlider.value);
    }
    public void SetMusicSlider() {
        if (initialize)
            soundManager.SetMusicVolume(MusicSlider.value);

    }
}
