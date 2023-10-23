using UnityEngine;
using UnityEngine.UI;

public class UISettingsMenu : MonoBehaviour
{
    private bool initialize = false;
    private GameInstance gameInstance = null;
    private SoundManager soundManager = null;
    private Slider MasterSlider;
    private Slider SFXSlider;
    private Slider TrackSlider;

    public void Initialize(GameInstance gameInstance, SoundManager soundManager)
    {
        if (initialize)
            return;

        this.gameInstance = gameInstance;
        this.soundManager = soundManager;
        SetupReference();
        initialize = true;
    }
    private void SetupReference()
    {
        MasterSlider = transform.Find("MasterSlider").GetComponent<Slider>();
        SFXSlider = transform.Find("SFXSlider").GetComponent<Slider>();
        TrackSlider = transform.Find("TrackSlider").GetComponent<Slider>();

        MasterSlider.value = soundManager.GetMasterVolume();
        SFXSlider.value = soundManager.GetSFXVolume();
        TrackSlider.value = soundManager.GetTrackVolume();
    }

    public void ReturnFromSettings()
    {   
        if (gameInstance.IsGameStarted())
        {
            gameInstance.SetGameState(GameInstance.GameState.PLAYING);
        }
        else
        {
            gameInstance.SetGameState(GameInstance.GameState.MAIN_MENU);
        }
    }

    public void SetSfxSlider(float value)
    {
        soundManager.SetSFXVolume(value);

    }
    public void SetMasterSlider(float value)
    {
        soundManager.SetMasterVolume(value);

    }
    public void SetTrackSlider(float value)
    {
        soundManager.SetTrackVolume(value);

    }
}
