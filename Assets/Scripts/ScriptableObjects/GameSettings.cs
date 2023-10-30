using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SettingsMenu;


[CreateAssetMenu(fileName = "GameSettings", menuName = "Data/GameSettings", order = 5)]
public class GameSettings : ScriptableObject {

    [Header("Display")]
    [SerializeField] public WindowMode windowMode = WindowMode.BORDERLESS_FULLSCREEN;
    [SerializeField] public Vsync vsync = Vsync.ON;
    [SerializeField] public FPSLimit fpsLimit = FPSLimit.OFF;


    [Header("Quality")]
    [SerializeField] public AntiAliasing antiAliasing = AntiAliasing.NO_MSAA;
    [SerializeField] public TextureQuality textureQuality = TextureQuality.HIGH;
    [SerializeField] public SettingsMenu.AnisotropicFiltering anisotropicFiltering = SettingsMenu.AnisotropicFiltering.ENABLE;
    [SerializeField] public PixelLightCount pixelLightCount = PixelLightCount.ULTRA;
    [SerializeField] public bool softParticles = true;
    [SerializeField] public SettingsMenu.ShadowQuality shadowQuality = SettingsMenu.ShadowQuality.HARD_AND_SOFT;
    [SerializeField] public SettingsMenu.ShadowResolution shadowResolution = SettingsMenu.ShadowResolution.ULTRA;


[Header("Sound")]
    [Range(0.0f, 1.0f)][SerializeField] public float masterVolume = 1.0f;
    [Range(0.0f, 1.0f)][SerializeField] public float musicVolume = 1.0f;
    [Range(0.0f, 1.0f)][SerializeField] public float sfxVolume = 1.0f;
}
