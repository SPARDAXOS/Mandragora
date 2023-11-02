using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using static SettingsMenu;

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

    private GameSettings gameSettings = null;
    private GameInstance gameInstance = null;
    private SoundManager soundManager = null;
    private Camera cameraScript = null;

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

    private Canvas canvasComp = null;


    public void Initialize(GameInstance gameInstance, SoundManager soundManager, GameSettings gameSettings, Camera camera) {
        if (initialize)
            return;

        //TODO: Take SO of all settings!

        this.gameSettings = gameSettings;
        this.gameInstance = gameInstance;
        this.soundManager = soundManager;
        cameraScript = camera;

        SetupReference();
        SetupCameraOverlay();

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


        int firstInitializationResults = PlayerPrefs.GetInt("FirstInitialization", -1);
        if (firstInitializationResults == -1) {
            PlayerPrefs.SetInt("FirstInitialization", 1);
            ApplyGameSettings();
            Debug.Log("Player prefs created new entry - Loaded settings from SO");
        }
        else if (firstInitializationResults == 1) {
            //LoadFromIt
            Debug.Log("Player prefs found an old entry - Loaded from player prefs!");
            ApplyGameSettingsFromPlayerPrefs();
        }
        else
            GameInstance.Abort("Undefined Behavior by player prefs retrieving FirstInitialization");


        UpdateGUI();
        initialize = true;
    }
    private void SetupCameraOverlay() {
        canvasComp.renderMode = RenderMode.ScreenSpaceCamera;
        canvasComp.worldCamera = cameraScript;
        canvasComp.planeDistance = 1.0f;
    }
    private void SetupReference() {

        canvasComp = GetComponent<Canvas>();

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
    private void UpdateGUI() {

        //Display
        //Resolution
        var resolution = Screen.currentResolution;
        for (uint i = 0; i < supportedResolutions.Length; i++) {
            var entry = supportedResolutions[i];
            if (entry.width == resolution.width && entry.height == resolution.height && entry.refreshRateRatio.value == resolution.refreshRateRatio.value)
                resolutionDropdown.value = (int)i;
        }

        //WindowMode
        var windowMode = Screen.fullScreenMode;
        for (uint i = 0; i < windowModeOptions.Length; i++) {
            if (windowMode == (UnityEngine.FullScreenMode)windowModeOptions[i]) {
                Debug.Log("It found WindowMode " + windowModeOptions[i] + " to be equal to unitys " + windowMode);
                windowModeDropdown.value = (int)i;
            }
        }

        //Vysnc
        var vsync = QualitySettings.vSyncCount;
        for (uint i = 0; i < vsyncOptions.Length; i++) {
            if (vsync == (int)vsyncOptions[i]) {
                Debug.Log("It found Vsync " + vsyncOptions[i] + " to be equal to unitys " + vsync);
                vsyncDropdown.value = (int)i;
            }
        }

        //FPS Limit
        var fpsLimit = Application.targetFrameRate;
        for (uint i = 0; i < fpsLimitOptions.Length; i++) {
            if (fpsLimit == (int)fpsLimitOptions[i]) {
                Debug.Log("It found FPSLimit " + fpsLimitOptions[i] + " to be equal to unitys " + fpsLimit);
                fpsLimitDropdown.value = (int)i;
            }
        }


        //Quality
        //Anti Aliasing
        var antiAliasing = QualitySettings.antiAliasing;
        for (uint i = 0; i < antiAliasingOptions.Length; i++) {
            if (antiAliasing == (int)antiAliasingOptions[i]) {
                Debug.Log("It found AntiAliasing" + antiAliasingOptions[i] + " to be equal to unitys " + antiAliasing);
                antiAliasingDropdown.value = (int)i;
            }
        }

        //Anisotropic Filtering
        var anisotropicFiltering = QualitySettings.anisotropicFiltering;
        for (uint i = 0; i < anisotropicFilteringOptions.Length; i++) {
            if (anisotropicFiltering == (UnityEngine.AnisotropicFiltering)anisotropicFilteringOptions[i]) {
                Debug.Log("It found AnisotropicFiltering" + anisotropicFilteringOptions[i] + " to be equal to unitys " + anisotropicFiltering);
                anisotropicFilteringDropdown.value = (int)i;
            }
        }

        //Texture Quality
        var textureQuality = QualitySettings.globalTextureMipmapLimit;
        for (uint i = 0; i < textureQualityOptions.Length; i++) {
            if (textureQuality == (int)textureQualityOptions[i]) {
                Debug.Log("It found TextureQuality " + textureQualityOptions[i] + " to be equal to unitys " + textureQuality);
                textureQualityDropdown.value = (int)i;
            }
        }

        //Pixel Light Count
        var pixelLightCount = QualitySettings.pixelLightCount;
        for (uint i = 0; i < pixelLightCountOptions.Length; i++) {
            if (pixelLightCount == (int)pixelLightCountOptions[i]) {
                Debug.Log("It found PixelLightCount " + pixelLightCountOptions[i] + " to be equal to unitys " + pixelLightCount);
                pixelLightCountDropdown.value = (int)i;
            }
        }

        //ShadowQuality
        var shadowQuality = QualitySettings.shadows;
        for (uint i = 0; i < shadowQualityOptions.Length; i++) {
            if (shadowQuality == (UnityEngine.ShadowQuality)shadowQualityOptions[i]) {
                Debug.Log("It found ShadowQuality " + shadowQualityOptions[i] + " to be equal to unitys " + shadowQuality);
                shadowQualityDropdown.value = (int)i;
            }
        }

        //ShadowResolution
        var shadowResolution = QualitySettings.shadowResolution;
        for (uint i = 0; i < shadowResolutionOptions.Length; i++) {
            if (shadowResolution == (UnityEngine.ShadowResolution)shadowResolutionOptions[i]) {
                Debug.Log("It found ShadowResolution " + shadowResolutionOptions[i] + " to be equal to unitys " + shadowResolution);
                shadowResolutionDropdown.value = (int)i;
            }
        }

        //Soft Particles
        var softParticles = QualitySettings.softParticles;
        softParticlesToggle.isOn = softParticles;
        Debug.Log("Soft particle is " + softParticles);

        //Audio - These are giving wierd values in the middle!
        MasterSlider.value = soundManager.GetMasterVolume();
        SFXSlider.value = soundManager.GetSFXVolume();
        MusicSlider.value = soundManager.GetMusicVolume();
    }
    private void ApplyGameSettings() {

        //Any negative indexed enum will crash this.
        //Display

        //Save index in options not the enums index

        //WindowMode
        Screen.fullScreenMode = (UnityEngine.FullScreenMode)gameSettings.windowMode;
        for (uint i = 0; i < windowModeOptions.Length; i++) {
            if (windowModeOptions[(int)i] == gameSettings.windowMode) {
                Debug.Log("WINDOW MODE OK!");
                PlayerPrefs.SetInt("WindowMode", (int)i);
            }
        }

        //Resolution
        var highestResolution = supportedResolutions[supportedResolutions.Length - 1];
        Screen.SetResolution(highestResolution.width, highestResolution.height, Screen.fullScreenMode, highestResolution.refreshRateRatio);
        //HAVENT DONE THIS

        //Vysnc
        QualitySettings.vSyncCount = (int)gameSettings.vsync;
        for (uint i = 0; i < vsyncOptions.Length; i++) {
            if (vsyncOptions[(int)i] == gameSettings.vsync) {
                Debug.Log("VSYNC MODE OK!");
                PlayerPrefs.SetInt("Vsync", (int)i);
            }
        }


        //FPS Limit
        Application.targetFrameRate = (int)gameSettings.fpsLimit;
        for (uint i = 0; i < fpsLimitOptions.Length; i++) {
            if (fpsLimitOptions[(int)i] == gameSettings.fpsLimit) {
                Debug.Log("FPSLIMIT MODE OK!");
                PlayerPrefs.SetInt("FPSLimit", (int)i);
            }
        }


        //Quality
        //Anti Aliasing
        QualitySettings.antiAliasing = (int)gameSettings.antiAliasing;
        for (uint i = 0; i < antiAliasingOptions.Length; i++) {
            if (antiAliasingOptions[(int)i] == gameSettings.antiAliasing) {
                Debug.Log("AntiAliasing MODE OK!");
                PlayerPrefs.SetInt("AntiAliasing", (int)i);
            }
        }

        //Anisotropic Filtering
        QualitySettings.anisotropicFiltering = (UnityEngine.AnisotropicFiltering)gameSettings.anisotropicFiltering;
        for (uint i = 0; i < anisotropicFilteringOptions.Length; i++) {
            if (anisotropicFilteringOptions[(int)i] == gameSettings.anisotropicFiltering) {
                Debug.Log("AnisotropicFiltering MODE OK!");
                PlayerPrefs.SetInt("AnisotropicFiltering", (int)i);
            }
        }


        //Texture Quality
        QualitySettings.globalTextureMipmapLimit = (int)gameSettings.textureQuality;
        for (uint i = 0; i < textureQualityOptions.Length; i++) {
            if (textureQualityOptions[(int)i] == gameSettings.textureQuality) {
                Debug.Log("TextureQuality MODE OK!");
                PlayerPrefs.SetInt("TextureQuality", (int)i);
            }
        }


        //Pixel Light Count
        QualitySettings.pixelLightCount = (int)gameSettings.pixelLightCount;
        for (uint i = 0; i < pixelLightCountOptions.Length; i++) {
            if (pixelLightCountOptions[(int)i] == gameSettings.pixelLightCount) {
                Debug.Log("PixelLightCount MODE OK!");
                PlayerPrefs.SetInt("PixelLightCount", (int)i);
            }
        }


        //ShadowQuality
        QualitySettings.shadows = (UnityEngine.ShadowQuality)gameSettings.shadowQuality;
        for (uint i = 0; i < shadowQualityOptions.Length; i++) {
            if (shadowQualityOptions[(int)i] == gameSettings.shadowQuality) {
                Debug.Log("ShadowQuality MODE OK!");
                PlayerPrefs.SetInt("ShadowQuality", (int)i);
            }
        }


        //ShadowResolution
        QualitySettings.shadowResolution = (UnityEngine.ShadowResolution)gameSettings.shadowResolution;
        for (uint i = 0; i < shadowResolutionOptions.Length; i++) {
            if (shadowResolutionOptions[(int)i] == gameSettings.shadowResolution) {
                Debug.Log("ShadowResolution MODE OK!");
                PlayerPrefs.SetInt("ShadowResolution", (int)i);
            }
        }


        //Soft Particles
        QualitySettings.softParticles = gameSettings.softParticles;
        if (QualitySettings.softParticles)
            PlayerPrefs.SetInt("SoftParticles", 1);
        else if (!QualitySettings.softParticles)
            PlayerPrefs.SetInt("SoftParticles", 0);


        //Audio
        soundManager.SetMasterVolume(gameSettings.masterVolume);
        soundManager.SetMusicVolume(gameSettings.musicVolume);
        soundManager.SetSFXVolume(gameSettings.sfxVolume);

        PlayerPrefs.SetFloat("MasterVolume", gameSettings.masterVolume);
        PlayerPrefs.SetFloat("MusicVolume", gameSettings.musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", gameSettings.sfxVolume);
    }
    private void ApplyGameSettingsFromPlayerPrefs() {
        //Display

        //Window
        int windowMode = PlayerPrefs.GetInt("WindowMode", -1);
        Debug.Log("WindowMode : " + windowMode);
        Screen.fullScreenMode = (UnityEngine.FullScreenMode)windowModeOptions[windowMode];
        
        //Vysnc
        int vsync = PlayerPrefs.GetInt("Vsync", -1);
        Debug.Log("Vsync : " + vsync);
        QualitySettings.vSyncCount = (int)vsyncOptions[vsync];

        //FPS Limit
        int fpsLimit = PlayerPrefs.GetInt("FPSLimit", -1);
        Debug.Log("FPSLimit : " + fpsLimit);
        Application.targetFrameRate = (int)fpsLimitOptions[fpsLimit];

        //Quality
        //Anti Aliasing
        int antiAliasing = PlayerPrefs.GetInt("AntiAliasing", -1);
        Debug.Log("AntiAliasing : " + antiAliasing);
        QualitySettings.antiAliasing = (int)antiAliasingOptions[antiAliasing];

        //Anisotropic Filtering
        var anisotropicFiltering = PlayerPrefs.GetInt("AnisotropicFiltering", -1);
        Debug.Log("anisotropicFiltering : " + anisotropicFiltering);
        QualitySettings.anisotropicFiltering = (UnityEngine.AnisotropicFiltering)anisotropicFilteringOptions[anisotropicFiltering];

        //Texture Quality
        int textureQuality = PlayerPrefs.GetInt("TextureQuality", -1);
        Debug.Log("TextureQuality : " + textureQuality);
        QualitySettings.globalTextureMipmapLimit = (int)textureQualityOptions[textureQuality];


        //Pixel Light Count
        int pixelLightCount = PlayerPrefs.GetInt("PixelLightCount", -1);
        Debug.Log("pIXELlIGHTcoUNT : " + pixelLightCount);
        QualitySettings.pixelLightCount = (int)pixelLightCountOptions[pixelLightCount];


        //ShadowQuality
        var shadowQuality = PlayerPrefs.GetInt("ShadowQuality", -1);
        Debug.Log("ShadowQuality : " + shadowQuality);
        QualitySettings.shadows = (UnityEngine.ShadowQuality)shadowQualityOptions[shadowQuality];


        //ShadowResolution
        var shadowResolution = PlayerPrefs.GetInt("ShadowResolution", -1);
        Debug.Log("ShadowResolution : " + shadowResolution);
        QualitySettings.shadowResolution = (UnityEngine.ShadowResolution)shadowResolutionOptions[shadowResolution];


        //Soft Particles
        int softParticles;
        softParticles = PlayerPrefs.GetInt("SoftParticles", -1);
        if (softParticles == 0)
            QualitySettings.softParticles = false;
        else if (softParticles == 1)
            QualitySettings.softParticles = true;
        else
            Debug.LogError("Got -1 at soft particles!");



        //Audio

        float masterVolume = PlayerPrefs.GetFloat("MasterVolume", 0.0f);
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.0f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.0f);

        Debug.Log("Master Volume - " + masterVolume);
        Debug.Log("Music Volume - " + musicVolume);
        Debug.Log("SFX Volume - " + sfxVolume);

        soundManager.SetMasterVolume(masterVolume);
        soundManager.SetMusicVolume(musicVolume);
        soundManager.SetSFXVolume(sfxVolume);
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

        PlayerPrefs.SetInt("WindowMode", value);
    }
    public void UpdateResolution() {
        int value = resolutionDropdown.value;
        Assert.IsFalse(value > supportedResolutions.Length - 1 || value < 0);
        var results = supportedResolutions[value];
        Screen.SetResolution(results.width, results.height, Screen.fullScreenMode, results.refreshRateRatio);
        Debug.Log(results.ToString());
    }
    public void UpdateVsync() {
        int value = vsyncDropdown.value;
        if (!ValidateUserInput(0, vsyncOptions.Length - 1, value, "UpdateVsync"))
            return;

        Debug.Log((int)vsyncOptions[value]);
        QualitySettings.vSyncCount = (int)vsyncOptions[value];

        PlayerPrefs.SetInt("Vsync", value);
    }
    public void UpdateFPSLimit() {
        int value = fpsLimitDropdown.value;
        if (!ValidateUserInput(0, fpsLimitOptions.Length - 1, value, "UpdateFPSLimit"))
            return;

        Debug.Log((int)fpsLimitOptions[value]);
        Application.targetFrameRate = (int)fpsLimitOptions[value];

        PlayerPrefs.SetInt("FPSLimit", value);
    }

    //Quality
    public void UpdateAntiAliasing() {
        int value = antiAliasingDropdown.value;
        if (!ValidateUserInput(0, antiAliasingOptions.Length - 1, value, "UpdateAntiAliasing"))
            return;
        Debug.Log((int)antiAliasingOptions[value]);
        QualitySettings.antiAliasing = (int)antiAliasingOptions[value];

        PlayerPrefs.SetInt("AntiAliasing", value);
    }
    public void UpdateTextureQuality() {
        int value = textureQualityDropdown.value;
        if (!ValidateUserInput(0, textureQualityOptions.Length - 1, value, "UpdateTextureQuality"))
            return;


        Debug.Log((int)textureQualityOptions[value]);
        QualitySettings.globalTextureMipmapLimit = (int)textureQualityOptions[value];

        PlayerPrefs.SetInt("TextureQuality", value);
    }
    public void UpdateAnisotropicFiltering() {
        int value = anisotropicFilteringDropdown.value;
        if (!ValidateUserInput(0, anisotropicFilteringOptions.Length - 1, value, "UpdateAnisotropicFiltering"))
            return;

        Debug.Log((UnityEngine.AnisotropicFiltering)anisotropicFilteringOptions[value]);
        QualitySettings.anisotropicFiltering = (UnityEngine.AnisotropicFiltering)anisotropicFilteringOptions[value];

        PlayerPrefs.SetInt("AnisotropicFiltering", value);
    }
    public void UpdatePixelLightCount() {
        int value = pixelLightCountDropdown.value;
        if (!ValidateUserInput(0, pixelLightCountOptions.Length - 1, value, "UpdatePixelLightCount"))
            return;

        Debug.Log((int)pixelLightCountOptions[value]);
        QualitySettings.pixelLightCount = (int)pixelLightCountOptions[value];

        PlayerPrefs.SetInt("PixelLightCount", value);
    }
    public void UpdateShadowQuality() {
        int value = shadowQualityDropdown.value;
        if (!ValidateUserInput(0, shadowQualityOptions.Length - 1, value, "UpdateShadowQuality"))
            return;

        Debug.Log((UnityEngine.ShadowQuality)shadowQualityOptions[value]);
        QualitySettings.shadows = (UnityEngine.ShadowQuality)shadowQualityOptions[value];

        PlayerPrefs.SetInt("ShadowQuality", value);
    }
    public void UpdateShadowResolution() {
        int value = shadowResolutionDropdown.value;
        if (!ValidateUserInput(0, shadowResolutionOptions.Length - 1, value, "UpdateShadowResolution"))
            return;

        Debug.Log((UnityEngine.ShadowResolution)shadowResolutionOptions[value]);
        QualitySettings.shadowResolution = (UnityEngine.ShadowResolution)shadowResolutionOptions[value];


        PlayerPrefs.SetInt("ShadowResolution", value);
    }
    public void ToggleSoftParticles() {
        bool value = softParticlesToggle.isOn;
        Debug.Log("Soft Particles: " + value);
        QualitySettings.softParticles = value;

        if (QualitySettings.softParticles)
            PlayerPrefs.SetInt("SoftParticles", 1);
        else if (!QualitySettings.softParticles)
            PlayerPrefs.SetInt("SoftParticles", 0);
    }
    

    //Sound
    public void SetMasterSlider() {
        if (initialize)
            soundManager.SetMasterVolume(MasterSlider.value);

        PlayerPrefs.SetFloat("MasterVolume", MasterSlider.value);
    }
    public void SetSFXSlider() {
        if (initialize)
            soundManager.SetSFXVolume(SFXSlider.value);

        PlayerPrefs.SetFloat("SFXVolume", SFXSlider.value);
    }
    public void SetMusicSlider() {
        if (initialize)
            soundManager.SetMusicVolume(MusicSlider.value);

        PlayerPrefs.SetFloat("MusicVolume", MusicSlider.value);
    }
}
