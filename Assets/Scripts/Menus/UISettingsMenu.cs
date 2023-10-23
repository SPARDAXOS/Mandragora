using UnityEngine;
using UnityEngine.UI;

public class UISettingsMenu : MonoBehaviour
{
    private bool initialize = false;
    private GameInstance gameInstance = null;
    private SoundManager soundManager = null;
    public Slider VolumeSlider;


    public void Initialize(GameInstance gameInstance, SoundManager soundManager)
    {
        if (initialize)
            return;

        this.gameInstance = gameInstance;
        this.soundManager = soundManager;
        initialize = true;
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

    public void SetVolumeSlider(float value)
    {
        //soundManager.setsfxvolume = value;
        //soundManager.settrackvolume = value;
        //soundManager.setmastervolume = value;
    }
}
