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
    private FullScreenMode[] windowModeOptions = null;
    private int[] vsyncOptions = new int[5];
    private int[] fpsLimitOptions = new int[4];
    private int[] antiAliasingOptions = new int[4];
    private int[] textureQualityOptions = new int[3];
    private UnityEngine.AnisotropicFiltering[] anisotropicFilteringOptions = new UnityEngine.AnisotropicFiltering[3];
    private int[] pixelLightCountOptions = new int[5];
    private UnityEngine.ShadowQuality[] shadowQualityOptions = new UnityEngine.ShadowQuality[3];
    private UnityEngine.ShadowResolution[] shadowResolutionOptions = new UnityEngine.ShadowResolution[4]; 

    private FullScreenMode currentFullScreenMode = FullScreenMode.FullScreenWindow; //Update this in initial setup!

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



    public void Initialize(GameInstance gameInstance, SoundManager soundManager, GameSettings gameSettings, bool applySettings) {
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

        UpdateData();
        UpdateGUI();
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

        MasterSlider.value = soundManager.GetMasterVolume();
        SFXSlider.value = soundManager.GetSFXVolume();
        MusicSlider.value = soundManager.GetMusicVolume();
    }
    private void UpdateGUI() {

        //Resolution
        var resolution = Screen.currentResolution;
        for (uint i = 0; i < supportedResolutions.Length; i++) {
            var entry = supportedResolutions[i];
            if (entry.width == resolution.width && entry.height == resolution.height && entry.refreshRateRatio.value == resolution.refreshRateRatio.value)
                resolutionDropdown.value = (int)i;
        }

        //WindowMode
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


        //This would need some work
#if UNITY_STANDALONE_WIN
        windowModeOptions = new FullScreenMode[3];
        modes.Add("Exclusive Fullscreen");
        windowModeOptions[2] = FullScreenMode.ExclusiveFullScreen;
#elif UNITY_STANDALONE_OSX
        windowModeOptions = new FullScreenMode[3];
        modes.Add("Maximized Window");
        windowModeOptions[2] = FullScreenMode.MaximizedWindow;
#else
        windowModeOptions = new FullScreenMode[2];
#endif

        windowModeOptions[0] = FullScreenMode.FullScreenWindow;
        windowModeOptions[1] = FullScreenMode.Windowed;

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
    private void SetupAntiAliasingOptions() {
        List<string> options = new List<string>();

        options.Add("Off");
        options.Add("2X");
        options.Add("4X");
        options.Add("8X");

        antiAliasingOptions[0] = 0;
        antiAliasingOptions[1] = 2;
        antiAliasingOptions[2] = 4;
        antiAliasingOptions[3] = 8;

        antiAliasingDropdown.ClearOptions();
        antiAliasingDropdown.AddOptions(options);
    }
    private void SetupAnisotropicFilteringOptions() {
        List<string> options = new List<string>();

        options.Add("Disable");
        options.Add("Enable");
        options.Add("ForceEnable");

        anisotropicFilteringOptions[0] = UnityEngine.AnisotropicFiltering.Disable;
        anisotropicFilteringOptions[1] = UnityEngine.AnisotropicFiltering.Enable;
        anisotropicFilteringOptions[2] = UnityEngine.AnisotropicFiltering.ForceEnable;

        anisotropicFilteringDropdown.ClearOptions();
        anisotropicFilteringDropdown.AddOptions(options);
    }
    private void SetupTextureQualityOptions() {
        var options = new List<string>();

        options.Add("High");
        options.Add("Medium");
        options.Add("Low");

        textureQualityOptions[0] = 0;
        textureQualityOptions[1] = 1;
        textureQualityOptions[2] = 2;

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

        pixelLightCountOptions[0] = 0;
        pixelLightCountOptions[1] = 1;
        pixelLightCountOptions[2] = 2;
        pixelLightCountOptions[3] = 3;
        pixelLightCountOptions[4] = 4;

        pixelLightCountDropdown.ClearOptions();
        pixelLightCountDropdown.AddOptions(options);
    }
    private void SetupShadowQualityOptions() {
        var options = new List<string>();

        options.Add("Disable");
        options.Add("HardOnly");
        options.Add("All");

        shadowQualityOptions[0] = UnityEngine.ShadowQuality.Disable;
        shadowQualityOptions[1] = UnityEngine.ShadowQuality.HardOnly;
        shadowQualityOptions[2] = UnityEngine.ShadowQuality.All;

        shadowQualityDropdown.ClearOptions();
        shadowQualityDropdown.AddOptions(options);
    }
    private void SetupShadowResolutionOptions() {
        var options = new List<string>();

        options.Add("Ultra");
        options.Add("High");
        options.Add("Medium");
        options.Add("Low");

        shadowResolutionOptions[0] = UnityEngine.ShadowResolution.VeryHigh;
        shadowResolutionOptions[1] = UnityEngine.ShadowResolution.High;
        shadowResolutionOptions[2] = UnityEngine.ShadowResolution.Medium;
        shadowResolutionOptions[3] = UnityEngine.ShadowResolution.Low;

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
        Screen.fullScreenMode = windowModeOptions[value];
    }
    public void UpdateResolution() {
        int value = resolutionDropdown.value;
        Assert.IsFalse(value > supportedResolutions.Length - 1 || value < 0);
        var results = supportedResolutions[value];
        Screen.SetResolution(results.width, results.height, currentFullScreenMode, results.refreshRateRatio);
        Debug.Log(results.ToString());
    }
    public void UpdateVsync() {
        int value = vsyncDropdown.value;
        if (!ValidateUserInput(0, vsyncOptions.Length - 1, value, "UpdateVsync"))
            return;
        Debug.Log(vsyncOptions[value].ToString());
        QualitySettings.vSyncCount = vsyncOptions[value]; //Redundant but is for consistency's sake.
    }
    public void UpdateFPSLimit() {
        int value = fpsLimitDropdown.value;
        if (!ValidateUserInput(0, fpsLimitOptions.Length - 1, value, "UpdateFPSLimit"))
            return;

        Debug.Log(fpsLimitOptions[value].ToString());
        Application.targetFrameRate = fpsLimitOptions[value];
    }

    //Quality
    public void UpdateAntiAliasing() {
        int value = antiAliasingDropdown.value;
        if (!ValidateUserInput(0, antiAliasingOptions.Length - 1, value, "UpdateAntiAliasing"))
            return;
        Debug.Log(antiAliasingOptions[value].ToString());
        QualitySettings.antiAliasing = antiAliasingOptions[value];
    }
    public void UpdateTextureQuality() {
        int value = textureQualityDropdown.value;
        if (!ValidateUserInput(0, textureQualityOptions.Length - 1, value, "UpdateTextureQuality"))
            return;

        //Not sure about index 0!
        Debug.Log(textureQualityOptions[value].ToString());
        QualitySettings.globalTextureMipmapLimit = textureQualityOptions[value];
    }
    public void UpdateAnisotropicFiltering() {
        int value = anisotropicFilteringDropdown.value;
        if (!ValidateUserInput(0, anisotropicFilteringOptions.Length - 1, value, "UpdateAnisotropicFiltering"))
            return;

        Debug.Log(anisotropicFilteringOptions[value].ToString());
        QualitySettings.anisotropicFiltering = anisotropicFilteringOptions[value];
    }
    public void UpdatePixelLightCount() {
        int value = pixelLightCountDropdown.value;
        if (!ValidateUserInput(0, pixelLightCountOptions.Length - 1, value, "UpdatePixelLightCount"))
            return;

        Debug.Log(pixelLightCountOptions[value].ToString());
        QualitySettings.pixelLightCount = pixelLightCountOptions[value];
    }
    public void UpdateShadowQuality() {
        int value = shadowQualityDropdown.value;
        if (!ValidateUserInput(0, shadowQualityOptions.Length - 1, value, "UpdateShadowQuality"))
            return;

        Debug.Log(shadowQualityOptions[value].ToString());
        QualitySettings.shadows = shadowQualityOptions[value];
    }
    public void UpdateShadowResolution() {
        int value = shadowResolutionDropdown.value;
        if (!ValidateUserInput(0, shadowResolutionOptions.Length - 1, value, "UpdateShadowResolution"))
            return;

        Debug.Log(shadowResolutionOptions[value].ToString());
        QualitySettings.shadowResolution = shadowResolutionOptions[value];
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
