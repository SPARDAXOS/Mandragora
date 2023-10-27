using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour {

    private bool initialize = false;



    private Resolution[] supportedResolutions = null;
    private int[] vsyncOptions = new int[5];
    private int[] fpsLimitOptions = new int[4];

    private FullScreenMode currentFullScreenMode = FullScreenMode.FullScreenWindow; //Update this in initial setup!

    private GameInstance gameInstance = null;
    private SoundManager soundManager = null;

    private Slider MasterSlider;
    private Slider SFXSlider;
    private Slider MusicSlider;

    private TMP_Dropdown resolutionDropdown = null;
    private TMP_Dropdown windowModeDropdown = null;
    private TMP_Dropdown vsyncDropdown = null;
    private TMP_Dropdown fpsLimitDropdown = null;
    private TMP_Dropdown antiAliasingDropdown = null;
    private TMP_Dropdown textureQualityDropdown = null;
    private TMP_Dropdown anisotropicFilteringDropdown = null;

    private TMP_Dropdown pixelLightCountDropdown = null;
    private Toggle softParticlesToggle = null;
    private TMP_Dropdown shadowsDropdown = null;
    private TMP_Dropdown shadowQualityDropdown = null;



    public void Initialize(GameInstance gameInstance, SoundManager soundManager) {
        if (initialize)
            return;

        //TODO: Take SO of all settings!

        this.gameInstance = gameInstance;
        this.soundManager = soundManager;
        SetupReference();
        SetupResolutionOptions();
        SetupWindowModeOptions();
        SetupVsyncOptions();
        SetupFPSLimitOptions();

        UpdateAllData(); //??
        initialize = true;
    }
    private void SetupReference() {
        MasterSlider = transform.Find("MasterSlider").GetComponent<Slider>();
        SFXSlider = transform.Find("SFXSlider").GetComponent<Slider>();
        MusicSlider = transform.Find("MusicSlider").GetComponent<Slider>();

        resolutionDropdown = transform.Find("ResolutionDropdown").GetComponent<TMP_Dropdown>();
        windowModeDropdown = transform.Find("WindowModeDropdown").GetComponent<TMP_Dropdown>();

        vsyncDropdown = transform.Find("VsyncDropdown").GetComponent<TMP_Dropdown>();
        fpsLimitDropdown = transform.Find("FPSLimitDropdown").GetComponent<TMP_Dropdown>();

        antiAliasingDropdown = transform.Find("AntiAliasingDropdown").GetComponent<TMP_Dropdown>();
        textureQualityDropdown = transform.Find("TextureQualityDropdown").GetComponent<TMP_Dropdown>();

        anisotropicFilteringDropdown = transform.Find("AnisotropicFilteringDropdown").GetComponent<TMP_Dropdown>();
        pixelLightCountDropdown = transform.Find("PixelLightCountDropdown").GetComponent<TMP_Dropdown>();

        shadowsDropdown = transform.Find("ShadowsDropdown").GetComponent<TMP_Dropdown>();
        shadowQualityDropdown = transform.Find("ShadowQualityDropdown").GetComponent<TMP_Dropdown>();

        softParticlesToggle = transform.Find("SoftParticlesToggle").GetComponent<Toggle>();
    }

    private void UpdateAllData() {
        //Updates All GUI - Updates only visuals
        //Update All Settings - actually sets them!

        MasterSlider.value = soundManager.GetMasterVolume();
        SFXSlider.value = soundManager.GetSFXVolume();
        MusicSlider.value = soundManager.GetMusicVolume();
    }

    private void SetupResolutionOptions() {
        supportedResolutions = Screen.resolutions;
        List<string> options = new List<string>(supportedResolutions.Length);
        foreach (var resolution in supportedResolutions)
            options.Add(resolution.width + "X" + resolution.height + "\t" + resolution.refreshRateRatio + "Hz");

        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(options);
    }
    private void SetupWindowModeOptions() {
        List<string> modes = new List<string>();
        modes.Add("Borderless Fullscreen"); //0
        modes.Add("Windowed"); //1

        //2
#if UNITY_STANDALONE_WIN
        modes.Add("Exclusive Fullscreen");
#elif UNITY_STANDALONE_OSX
        modes.Add("Maximized Window");
#endif

        windowModeDropdown.ClearOptions();
        windowModeDropdown.AddOptions(modes);
    }
    private void SetupVsyncOptions() {
        List<string> options = new List<string>();

        options.Add("Off");
        options.Add("On");
        options.Add("2 Frames");
        options.Add("3 Frames");
        options.Add("4 Frames");

        vsyncOptions[0] = 0;
        vsyncOptions[1] = 1;
        vsyncOptions[2] = 2;
        vsyncOptions[3] = 3;
        vsyncOptions[4] = 4;

        vsyncDropdown.ClearOptions();
        vsyncDropdown.AddOptions(options);
    }
    private void SetupFPSLimitOptions() {
        List<string> options = new List<string>();

        options.Add("Off");
        options.Add("30");
        options.Add("60");
        options.Add("144");

        fpsLimitOptions[0] = -1;
        fpsLimitOptions[1] = 30;
        fpsLimitOptions[2] = 60;
        fpsLimitOptions[3] = 144;

        fpsLimitDropdown.ClearOptions();
        fpsLimitDropdown.AddOptions(options);
    }

    private bool ValidateUserInput(int min, int max, int value, string operationName) {
        if (value < min) {
            Debug.LogError("Invalid user input! \n Value less than " + min + " was sent to " + operationName);
            return false;
        }
        else if (value > max) {
            Debug.LogError("Invalid user input! \n Value more than " + max + " was sent to " + operationName);
            return false;
        }

        return true;
    }


    public void ReturnButton() {
        if (gameInstance.IsGameStarted())
            gameInstance.PauseGame(); //???
        else
            gameInstance.SetGameState(GameInstance.GameState.MAIN_MENU);
    }

    //Borderless Fullscreen
    //Exclusive Fullscreen - Win only
    //Maximized Fullscreen - OSX only
    //Windowed

    //NOTE: Ideally i add all options from code here so i can guaruantee the index consistency!

    //Display
    public void UpdateWindowMode() {
        int value = windowModeDropdown.value;
        if (!ValidateUserInput(0, 2, value, "UpdateWindowMode"))
            return;

        if (value == 0)
            currentFullScreenMode = FullScreenMode.FullScreenWindow;
        else if (value == 1)
            currentFullScreenMode = FullScreenMode.Windowed;
        else if (value == 2) {
#if UNITY_STANDALONE_WIN
            currentFullScreenMode = FullScreenMode.ExclusiveFullScreen;
#elif UNITY_STANDALONE_OSX
            currentFullScreenMode = FullScreenMode.MaximizedWindow;
#endif
        }

        Screen.fullScreenMode = currentFullScreenMode;
    }
    public void UpdateResolution() {
        int value = resolutionDropdown.value;
        Assert.IsFalse(value > supportedResolutions.Length - 1 || value < 0);
        var results = supportedResolutions[value];
        Screen.SetResolution(results.width, results.height, currentFullScreenMode, results.refreshRateRatio);
    }
    public void UpdateVsync() {
        int value = vsyncDropdown.value;
        if (!ValidateUserInput(0, vsyncOptions.Length - 1, value, "UpdateVsync"))
            return;

        QualitySettings.vSyncCount = vsyncOptions[value]; //Redundant but is for consistency's sake.
    }
    public void UpdateFPSLimit() {
        int value = fpsLimitDropdown.value;
        if (!ValidateUserInput(0, fpsLimitOptions.Length - 1, value, "UpdateFPSLimit"))
            return;


        Application.targetFrameRate = fpsLimitOptions[value];
    }

    //Quality
    public void UpdateAntiAliasing() {
        int value = antiAliasingDropdown.value;
        if (!ValidateUserInput(0, 3, value, "UpdateAntiAliasing"))
            return;

        if (value == 0)
            value = 0; //No MSAA
        else if (value == 1)
            value = 2;
        else if (value == 2)
            value = 4;
        else if (value == 3)
            value = 8;

        QualitySettings.antiAliasing = value;
    }
    public void UpdateTextureQuality() {
        //0 - fullres, 1 - Half res, 2 - Third res
        int value = textureQualityDropdown.value;
        if (!ValidateUserInput(0, 2, value, "UpdateTextureQuality"))
            return;

        QualitySettings.globalTextureMipmapLimit = value;
    }
    public void UpdateAnisotropicFiltering() {
        int value = anisotropicFilteringDropdown.value;
        if (!ValidateUserInput(0, 2, value, "UpdateAnisotropicFiltering"))
            return;

        var result = AnisotropicFiltering.Disable;
        if (value == 1)
            result = AnisotropicFiltering.Enable;
        else if (value == 2)
            result = AnisotropicFiltering.ForceEnable;

        QualitySettings.anisotropicFiltering = result;
    }


    //Sound
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
