using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour {

    //NOTE: Map the unity values to these and create arrays of these. Then map the drop down options to these.

    public enum WindowMode {
        BORDERLESS_FULLSCREEN = FullScreenMode.FullScreenWindow,
        WINDOWED = FullScreenMode.Windowed,
        EXCLUSIVE_FULLSCREEN = FullScreenMode.ExclusiveFullScreen,
        MAXIMIZED_WINDOWED = FullScreenMode.MaximizedWindow
    }
    public enum Vsync {
        OFF = 0,
        ON = 1,
        FRAME_2 = 2,
        FRAME_3 = 3,
        FRAME_4 = 4
    }
    public enum FPSLimit {
        OFF = -1,
        LIMIT_30 = 30,
        LIMIT_60 = 60,
        LIMIT_144 = 144
    }
    public enum AntiAliasing {
        NO_MSAA = 0,
        MULTISAMPLING_X2 = 2,
        MULTISAMPLING_X4 = 4,
        MULTISAMPLING_X8 = 8
    }
    public enum TextureQuality {
        HIGH = 0,
        MEDIUM = 1,
        LOW = 2
    }
    public enum AnisotropicFiltering {
        DISABLE = UnityEngine.AnisotropicFiltering.Disable,
        ENABLE = UnityEngine.AnisotropicFiltering.Enable,
        FORCE_ENABLE = UnityEngine.AnisotropicFiltering.ForceEnable
    }
    public enum PixelLightCount {
        OFF = 0,
        LOW = 1,
        MEDIUM = 2,
        HIGH = 3,
        ULTRA = 4
    }
    public enum ShadowQuality {
        DISABLE = UnityEngine.ShadowQuality.Disable,
        HARD_ONLY = UnityEngine.ShadowQuality.HardOnly,
        HARD_AND_SOFT = UnityEngine.ShadowQuality.All
    }
    public enum ShadowResolution {
        ULTRA = UnityEngine.ShadowResolution.VeryHigh,
        HIGH = UnityEngine.ShadowResolution.High,
        MEDIUM = UnityEngine.ShadowResolution.Medium,
        LOW = UnityEngine.ShadowResolution.Low
    }


    private bool initialize = false;

    private Resolution[] supportedResolutions = null;
    private WindowMode[] windowModeOptions = null;
    private Vsync[] vsyncOptions = new Vsync[5];
    private FPSLimit[] fpsLimitOptions = new FPSLimit[4];
    private AntiAliasing[] antiAliasingOptions = new AntiAliasing[4];
    private TextureQuality[] textureQualityOptions = new TextureQuality[3];
    private AnisotropicFiltering[] anisotropicFilteringOptions = new AnisotropicFiltering[3];
    private PixelLightCount[] pixelLightCountOptions = new PixelLightCount[5];
    private ShadowQuality[] shadowQualityOptions = new ShadowQuality[3];
    private ShadowResolution[] shadowResolutionOptions = new ShadowResolution[4]; 

    private WindowMode currentFullScreenMode = WindowMode.BORDERLESS_FULLSCREEN; //Update this in initial setup!

    private GameInstance gameInstance = null;
    private SoundManager soundManager = null;

    private GameSettings gameSettings = null;

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
    private TMP_Dropdown shadowQualityDropdown = null;
    private TMP_Dropdown shadowResolutionDropdown = null;



    public void Initialize(GameInstance gameInstance, SoundManager soundManager, GameSettings gameSettings, bool firstInitialization) {
        if (initialize)
            return;

        //TODO: Take SO of all settings!

        this.gameInstance = gameInstance;
        this.soundManager = soundManager;
        this.gameSettings = gameSettings;

        SetupReference();
        SetupResolutionOptions();
        SetupWindowModeOptions();
        SetupVsyncOptions();
        SetupFPSLimitOptions();
        SetupAntiAliasingOptions();
        SetupTextureQualityOptions();
        SetupAnisotropicFilteringOptions();
        SetupPixelLightCountOptions();
        SetupShadowQualityOptions();
        SetupShadowResolutionOptions();

        if (firstInitialization)
            ApplyGameSettings(gameSettings);
        else
            UpdateGUI();

        //Flow
        //If first time, Take GameSettings apply them from it then update gui from current settings.
        //If not first, just update GUI using Unity current settings
        initialize = true;
    }
    private void SetupReference() {

        var Display = transform.Find("Display");
        var Audio = transform.Find("Audio");
        var Quality = transform.Find("Quality");

        MasterSlider = Audio.Find("MasterSlider").GetComponent<Slider>();
        SFXSlider = Audio.Find("SFXSlider").GetComponent<Slider>();
        MusicSlider = Audio.Find("MusicSlider").GetComponent<Slider>();

        resolutionDropdown = Display.Find("ResolutionDropdown").GetComponent<TMP_Dropdown>();
        windowModeDropdown = Display.Find("WindowModeDropdown").GetComponent<TMP_Dropdown>();

        vsyncDropdown = Display.Find("VsyncDropdown").GetComponent<TMP_Dropdown>();
        fpsLimitDropdown = Display.Find("FPSLimitDropdown").GetComponent<TMP_Dropdown>();

        antiAliasingDropdown = Quality.Find("AntiAliasingDropdown").GetComponent<TMP_Dropdown>();
        textureQualityDropdown = Quality.Find("TextureQualityDropdown").GetComponent<TMP_Dropdown>();

        anisotropicFilteringDropdown = Quality.Find("AnisotropicFilteringDropdown").GetComponent<TMP_Dropdown>();
        pixelLightCountDropdown = Quality.Find("PixelLightCountDropdown").GetComponent<TMP_Dropdown>();

        shadowQualityDropdown = Quality.Find("ShadowQualityDropdown").GetComponent<TMP_Dropdown>();
        shadowResolutionDropdown = Quality.Find("ShadowResolutionDropdown").GetComponent<TMP_Dropdown>();

        softParticlesToggle = Quality.Find("SoftParticlesToggle").GetComponent<Toggle>();
    }
    private void UpdateData() {
        //Updates All GUI - Updates only visuals
        //Update All Settings - actually sets them!
    }
    private void UpdateGUI() {

        //Always take from Unity

        //Resolution
        var resolution = Screen.currentResolution;
        for (uint i = 0; i < supportedResolutions.Length; i++) {
            var entry = supportedResolutions[i];
            if (entry.width == resolution.width && entry.height == resolution.height && entry.refreshRateRatio.value == resolution.refreshRateRatio.value)
                resolutionDropdown.value = (int)i;
        }

        //WindowMode
        


        //Audio
        MasterSlider.value = soundManager.GetMasterVolume();
        SFXSlider.value = soundManager.GetSFXVolume();
        MusicSlider.value = soundManager.GetMusicVolume();

    }
    private void UpdateGUI(GameSettings gameSettings) {
        //Remember what i had in mind. 
        //I think one updates from the so and other from actual game settings.
        //Doesnt seem needed...

    }
    private void ApplyGameSettings(GameSettings gameSettngs) {

    }

    private void SetupResolutionOptions() {
        supportedResolutions = Screen.resolutions;
        List<string> options = new List<string>(supportedResolutions.Length);
        foreach (var resolution in supportedResolutions)
            options.Add(resolution.width + "X" + resolution.height + "  " + ((int)resolution.refreshRateRatio.value).ToString() + "Hz");

        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(options);
    }
    private void SetupWindowModeOptions() {
        List<string> modes = new List<string>();
        modes.Add("Borderless Fullscreen");
        modes.Add("Windowed");


#if UNITY_STANDALONE_WIN
        windowModeOptions = new WindowMode[3];
        modes.Add("Exclusive Fullscreen");
        windowModeOptions[2] = WindowMode.EXCLUSIVE_FULLSCREEN;

#elif UNITY_STANDALONE_OSX
        windowModeOptions = new WindowMode[3];
        modes.Add("Maximized Window");
        windowModeOptions[2] = WindowMode.MAXIMIZED_WINDOWED;

#else
        windowModeOptions = new WindowMode[2];
#endif

        windowModeOptions[0] = WindowMode.BORDERLESS_FULLSCREEN;
        windowModeOptions[1] = WindowMode.WINDOWED;

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

        vsyncOptions[0] = Vsync.OFF;
        vsyncOptions[1] = Vsync.ON;
        vsyncOptions[2] = Vsync.FRAME_2;
        vsyncOptions[3] = Vsync.FRAME_3;
        vsyncOptions[4] = Vsync.FRAME_4;

        vsyncDropdown.ClearOptions();
        vsyncDropdown.AddOptions(options);
    }
    private void SetupFPSLimitOptions() {
        List<string> options = new List<string>();

        options.Add("Off");
        options.Add("30");
        options.Add("60");
        options.Add("144");

        fpsLimitOptions[0] = FPSLimit.OFF;
        fpsLimitOptions[1] = FPSLimit.LIMIT_30;
        fpsLimitOptions[2] = FPSLimit.LIMIT_60;
        fpsLimitOptions[3] = FPSLimit.LIMIT_144;

        fpsLimitDropdown.ClearOptions();
        fpsLimitDropdown.AddOptions(options);
    }
    private void SetupAntiAliasingOptions() {
        List<string> options = new List<string>();

        options.Add("Off");
        options.Add("2X");
        options.Add("4X");
        options.Add("8X");

        antiAliasingOptions[0] = AntiAliasing.NO_MSAA;
        antiAliasingOptions[1] = AntiAliasing.MULTISAMPLING_X2;
        antiAliasingOptions[2] = AntiAliasing.MULTISAMPLING_X4;
        antiAliasingOptions[3] = AntiAliasing.MULTISAMPLING_X8;

        antiAliasingDropdown.ClearOptions();
        antiAliasingDropdown.AddOptions(options);
    }
    private void SetupAnisotropicFilteringOptions() {
        List<string> options = new List<string>();

        options.Add("Disable");
        options.Add("Enable");
        options.Add("ForceEnable");

        anisotropicFilteringOptions[0] = AnisotropicFiltering.DISABLE;
        anisotropicFilteringOptions[1] = AnisotropicFiltering.ENABLE;
        anisotropicFilteringOptions[2] = AnisotropicFiltering.FORCE_ENABLE;

        anisotropicFilteringDropdown.ClearOptions();
        anisotropicFilteringDropdown.AddOptions(options);
    }
    private void SetupTextureQualityOptions() {
        var options = new List<string>();

        options.Add("High");
        options.Add("Medium");
        options.Add("Low");

        textureQualityOptions[0] = TextureQuality.HIGH;
        textureQualityOptions[1] = TextureQuality.MEDIUM;
        textureQualityOptions[2] = TextureQuality.LOW;

        textureQualityDropdown.ClearOptions();
        textureQualityDropdown.AddOptions(options);
    }
    private void SetupPixelLightCountOptions() {
        var options = new List<string>();

        options.Add("Off");
        options.Add("Low");
        options.Add("Medium");
        options.Add("High");
        options.Add("Ultra");

        pixelLightCountOptions[0] = PixelLightCount.OFF;
        pixelLightCountOptions[1] = PixelLightCount.LOW;
        pixelLightCountOptions[2] = PixelLightCount.MEDIUM;
        pixelLightCountOptions[3] = PixelLightCount.HIGH;
        pixelLightCountOptions[4] = PixelLightCount.ULTRA;

        pixelLightCountDropdown.ClearOptions();
        pixelLightCountDropdown.AddOptions(options);
    }
    private void SetupShadowQualityOptions() {
        var options = new List<string>();

        options.Add("Disable");
        options.Add("HardOnly");
        options.Add("All");

        shadowQualityOptions[0] = ShadowQuality.DISABLE;
        shadowQualityOptions[1] = ShadowQuality.HARD_ONLY;
        shadowQualityOptions[2] = ShadowQuality.HARD_AND_SOFT;

        shadowQualityDropdown.ClearOptions();
        shadowQualityDropdown.AddOptions(options);
    }
    private void SetupShadowResolutionOptions() {
        var options = new List<string>();

        options.Add("Ultra");
        options.Add("High");
        options.Add("Medium");
        options.Add("Low");

        shadowResolutionOptions[0] = ShadowResolution.ULTRA;
        shadowResolutionOptions[1] = ShadowResolution.HIGH;
        shadowResolutionOptions[2] = ShadowResolution.MEDIUM;
        shadowResolutionOptions[3] = ShadowResolution.LOW;

        shadowResolutionDropdown.ClearOptions();
        shadowResolutionDropdown.AddOptions(options);
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
        soundManager.PlaySFX("NextMenu", Vector3.zero, true);
    }


    //Display
    public void UpdateWindowMode() {
        int value = windowModeDropdown.value;
        if (!ValidateUserInput(0, windowModeOptions.Length - 1, value, "UpdateWindowMode"))
            return;

        Debug.Log(windowModeOptions[value].ToString());
        Screen.fullScreenMode = (UnityEngine.FullScreenMode)windowModeOptions[value];
    }
    public void UpdateResolution() {
        int value = resolutionDropdown.value;
        Assert.IsFalse(value > supportedResolutions.Length - 1 || value < 0);
        var results = supportedResolutions[value];
        Screen.SetResolution(results.width, results.height, (UnityEngine.FullScreenMode)currentFullScreenMode, results.refreshRateRatio);
        Debug.Log(results.ToString());
    }
    public void UpdateVsync() {
        int value = vsyncDropdown.value;
        if (!ValidateUserInput(0, vsyncOptions.Length - 1, value, "UpdateVsync"))
            return;

        Debug.Log((int)vsyncOptions[value]);
        QualitySettings.vSyncCount = (int)vsyncOptions[value];
    }
    public void UpdateFPSLimit() {
        int value = fpsLimitDropdown.value;
        if (!ValidateUserInput(0, fpsLimitOptions.Length - 1, value, "UpdateFPSLimit"))
            return;

        Debug.Log((int)fpsLimitOptions[value]);
        Application.targetFrameRate = (int)fpsLimitOptions[value];
    }

    //Quality
    public void UpdateAntiAliasing() {
        int value = antiAliasingDropdown.value;
        if (!ValidateUserInput(0, antiAliasingOptions.Length - 1, value, "UpdateAntiAliasing"))
            return;
        Debug.Log((int)antiAliasingOptions[value]);
        QualitySettings.antiAliasing = (int)antiAliasingOptions[value];
    }
    public void UpdateTextureQuality() {
        int value = textureQualityDropdown.value;
        if (!ValidateUserInput(0, textureQualityOptions.Length - 1, value, "UpdateTextureQuality"))
            return;


        Debug.Log((int)textureQualityOptions[value]);
        QualitySettings.globalTextureMipmapLimit = (int)textureQualityOptions[value];
    }
    public void UpdateAnisotropicFiltering() {
        int value = anisotropicFilteringDropdown.value;
        if (!ValidateUserInput(0, anisotropicFilteringOptions.Length - 1, value, "UpdateAnisotropicFiltering"))
            return;

        Debug.Log((UnityEngine.AnisotropicFiltering)anisotropicFilteringOptions[value]);
        QualitySettings.anisotropicFiltering = (UnityEngine.AnisotropicFiltering)anisotropicFilteringOptions[value];
    }
    public void UpdatePixelLightCount() {
        int value = pixelLightCountDropdown.value;
        if (!ValidateUserInput(0, pixelLightCountOptions.Length - 1, value, "UpdatePixelLightCount"))
            return;

        Debug.Log((int)pixelLightCountOptions[value]);
        QualitySettings.pixelLightCount = (int)pixelLightCountOptions[value];
    }
    public void UpdateShadowQuality() {
        int value = shadowQualityDropdown.value;
        if (!ValidateUserInput(0, shadowQualityOptions.Length - 1, value, "UpdateShadowQuality"))
            return;

        Debug.Log((UnityEngine.ShadowQuality)shadowQualityOptions[value]);
        QualitySettings.shadows = (UnityEngine.ShadowQuality)shadowQualityOptions[value];
    }
    public void UpdateShadowResolution() {
        int value = shadowResolutionDropdown.value;
        if (!ValidateUserInput(0, shadowResolutionOptions.Length - 1, value, "UpdateShadowResolution"))
            return;

        Debug.Log((UnityEngine.ShadowResolution)shadowResolutionOptions[value]);
        QualitySettings.shadowResolution = (UnityEngine.ShadowResolution)shadowResolutionOptions[value];
    }
    public void ToggleSoftParticles() {
        bool value = softParticlesToggle.isOn;
        Debug.Log("Soft Particles: " + value);
        QualitySettings.softParticles = value;
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
