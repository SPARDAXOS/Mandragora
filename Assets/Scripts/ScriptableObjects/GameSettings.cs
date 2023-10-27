using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "GameSettings", menuName = "Data/GameSettings", order = 5)]
public class GameSettings : ScriptableObject {

    public enum WindowMode {
        BORDERLESS_FULLSCREEN,
        WINDOWED,
        EXCLUSIVE_FULLSCREEN,
        MAXIMIZED_WINDOWED
    }
    public enum Vsync {
        OFF,
        ON,
        FRAME_2,
        FRAME_3,
        FRAME_4,
    }
    public enum FPSLimit {
        OFF,
        LIMIT_30,
        LIMIT_60,
        LIMIT_144
    }
    public enum AntiAliasing {
        NO_MSAA,
        MULTISAMPLING_X2,
        MULTISAMPLING_X4,
        MULTISAMPLING_X8
    }
    public enum TextureQuality {
        LOW,
        MEDIUM,
        HIGH
    }
    public enum AnisotropicFiltering {
        DISABLE,
        ENABLE,
        FORCE_ENABLE
    }
    public enum PixelLightCount {
        OFF,
        LOW,
        MEDIUM,
        HIGH,
        ULTRA
    }
    public enum ShadowQuality {
        DISABLE,
        HARD_ONLY,
        HARD_AND_SOFT
    }
    public enum ShadowResolution {
        LOW,
        MEDIUM,
        HIGH,
        ULTRA
    }


    [Header("Display")]
    [SerializeField] public WindowMode windowMode = WindowMode.BORDERLESS_FULLSCREEN;
    [SerializeField] public Vsync vsync = Vsync.ON;
    [SerializeField] public FPSLimit fpsLimit = FPSLimit.OFF;


    [Header("Quality")]
    [SerializeField] public AntiAliasing antiAliasing = AntiAliasing.NO_MSAA;
    [SerializeField] public TextureQuality textureQuality = TextureQuality.HIGH;
    [SerializeField] public AnisotropicFiltering anisotropicFiltering = AnisotropicFiltering.ENABLE;
    [SerializeField] public PixelLightCount pixelLightCount = PixelLightCount.ULTRA;
    [SerializeField] public bool softParticles = true;
    [SerializeField] public ShadowQuality shadowQuality = ShadowQuality.HARD_AND_SOFT;
    [SerializeField] public ShadowResolution shadowResolution = ShadowResolution.ULTRA;


    [Header("Sound")]
    [Range(0.0f, 1.0f)][SerializeField] public float masterVolume = 1.0f;
    [Range(0.0f, 1.0f)][SerializeField] public float musicVolume = 1.0f;
    [Range(0.0f, 1.0f)][SerializeField] public float sfxVolume = 1.0f;
}
