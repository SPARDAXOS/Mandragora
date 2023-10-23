using UnityEngine;
using Mandragora;
using System.Collections.Generic;
using System;

public class SoundManager : MonoBehaviour {
    private enum ClipType {
        SFX,
        TRACK
    }

    [Header("Bundles")]
    [SerializeField] private SoundsBundle sfxBundle;
    [SerializeField] private SoundsBundle tracksBundle;

    [Space(5)]
    [Header("Fade")]
    [Range(0.01f, 1.0f)][SerializeField] private float fadeInSpeed = 0.1f;
    [Range(0.01f, 1.0f)][SerializeField] private float fadeOutSpeed = 0.1f;
    [Tooltip("Should interrupt a fade in/out and play a new track.")]
    [SerializeField] private bool canInterruptFade = true;

    [Space(5)]
    [Header("Volume")]
    [Range(0.0f, 1.0f)][SerializeField] private float masterVolume = 1.0f;
    [Range(0.0f, 1.0f)][SerializeField] private float trackVolume = 1.0f;
    [Range(0.0f, 1.0f)][SerializeField] private float sfxVolume = 1.0f;


    private bool initialized = false;
    private bool canPlaySFX = false;
    private bool canPlayTracks = false;

    private float currentTrackVolume = 0.0f;
    private float currentTrackEntryVolume = 0.0f;
    private bool fadingIn = false;
    private bool fadingOut = false;
    private SoundEntry? fadeTargetTrack;

    private GameObject soundUnitResource = null;
    private AudioSource trackAudioSource = null;

    private List<SoundUnit> soundUnits = new List<SoundUnit>();


    public void Initialize() {
        if (initialized)
            return;


        ValidateBundles();
        LoadResourcese();
        SetupReferences();
        initialized = true;
    }
    public void Tick() {
        if (!initialized)
            return;

        if (fadingIn)
            UpdateTrackFadeIn();
        else if (fadingOut)
            UpdateTrackFadeOut();
        else
            UpdateTrackVolume();
    }


    private void ValidateBundles() {
        if (Utility.Validate(sfxBundle, "SoundManager is missing an SFX bundle \n Playing SFX wont be possible!", Utility.ValidationType.WARNING))
            canPlaySFX = true;
        if (Utility.Validate(tracksBundle, "SoundManager is missing a tracks bundle \n Playing tracks wont be possible!", Utility.ValidationType.WARNING))
            canPlayTracks = true;
    }
    private void SetupReferences() {
        trackAudioSource = GetComponent<AudioSource>();
        Utility.Validate(trackAudioSource, "Failed to get AudioSource component on SoundsManager", Utility.ValidationType.WARNING);
    }
    private void LoadResourcese() {
        soundUnitResource = Resources.Load<GameObject>("SoundUnit");
        Utility.Validate(soundUnitResource, "Failed to load SoundUnit resource.", Utility.ValidationType.WARNING);
    }


    public bool PlaySFX(string key, Vector3 position) {
        if (!canPlaySFX) {
            Debug.LogError("Unable to play sfx \n SoundManager does not contain an SFX bundle!");
            return false;
        }

        SoundEntry? TargetSoundEntry = FindClip(key, ClipType.SFX);
        if (TargetSoundEntry == null) {
            Debug.Log("Unable to find sfx clip \"" + key + "\" to play!");
            return false;
        }

        SoundUnit soundUnit = GetAvailableSoundUnit();
        if (!soundUnit) {
            Debug.LogError("Unable to play sound file \"" + key + "\" \n Couldnt get a sound unit!");
            return false;
        }

        float volume = masterVolume * sfxVolume * TargetSoundEntry.Value.volume;
        return soundUnit.Play((SoundEntry)TargetSoundEntry, position, volume);
    }
    public bool PlayTrack(string key, bool fade = false) {
        if (!canPlayTracks) {
            Debug.LogError("Unable to play track \n SoundManager does not contain a track bundle!");
            return false;
        }

        SoundEntry? TargetSoundEntry = FindClip(key, ClipType.TRACK);
        if (TargetSoundEntry == null) {
            Debug.Log("Unable to find track clip \"" + key + "\" to play!");
            return false;
        }

        if (fade) {
            if (canInterruptFade) {
                if (fadingIn || fadingOut) {
                    fadingIn = false;
                    fadingOut = false;
                    fadeTargetTrack = null;
                    ApplyTrack((SoundEntry)TargetSoundEntry);
                    Debug.Log("Interrupted!");
                    return true;
                }
            }

            if (fadingIn) //For book-keeping consistency.
                fadingIn = false;

            fadeTargetTrack = TargetSoundEntry; //If already fading out then this will make sure it fades into the last requested track.
            if (!trackAudioSource.isPlaying) {
                currentTrackVolume = 0.0f;
                StarFadeIn((SoundEntry)TargetSoundEntry);
            }
            else
                StartFadeOut();
        }
        else
            ApplyTrack((SoundEntry)TargetSoundEntry);

        return true;
    }
    public void StopTrack(bool fade = false) {
        if (!trackAudioSource.isPlaying)
            return;

        if (fadingIn)
            fadingIn = false;

        if (fadingOut) {
            fadingOut = false;
            fadeTargetTrack = null;
        }

        if (fade)
            StartFadeOut();
        else
            trackAudioSource.Stop();
    }



    public float GetMasterVolume() {
        return masterVolume;
    }
    public float GetTrackVolume() { 
        return trackVolume; 
    }
    public float GetSFXVolume() {
        return sfxVolume;
    }


    public void SetMasterVolume(float volume) {
        masterVolume = volume;
    }
    public void SetTrackVolume(float volume) {
        trackVolume = volume;
    }
    public void SetSFXVolume(float volume) {
        sfxVolume = volume;
    }


    private Nullable<SoundEntry> FindClip(string key, ClipType type) {
        if (key == null)
            return null;

        if (type == ClipType.SFX) {
            if (sfxBundle.entries.Length == 0)
                return null;

            foreach (var entry in sfxBundle.entries) {
                if (entry.key == key)
                    return entry;
            }
        }
        else if (type == ClipType.TRACK) {
            if (tracksBundle.entries.Length == 0)
                return null;

            foreach (var entry in tracksBundle.entries) {
                if (entry.key == key)
                    return entry;
            }
        }

        return null;
    }
    private SoundUnit GetAvailableSoundUnit() {
        if (soundUnits.Count == 0)
            return AddSoundUnit();

        foreach(var entry in soundUnits) {
            if (!entry.IsPlaying())
                return entry;
        }

        return AddSoundUnit();
    }
    private SoundUnit AddSoundUnit() {
        if (!soundUnitResource) {
            Debug.LogError("Unable to add new sound unit - Resource is null!");
            return null;
        }

        var gameObject = Instantiate(soundUnitResource);
        gameObject.transform.parent = transform;
        var script = gameObject.GetComponent<SoundUnit>();
        script.Initialize();
        soundUnits.Add(script);
        return script;
    }


    private void ApplyTrack(SoundEntry track) {
        currentTrackEntryVolume = track.volume;
        trackAudioSource.volume = masterVolume * trackVolume * currentTrackEntryVolume;
        trackAudioSource.pitch = UnityEngine.Random.Range(track.minPitch, track.maxPitch);
        trackAudioSource.clip = track.clip;
        trackAudioSource.Play();
    }
    private void StartFadeOut() {
        fadingOut = true;
    }
    private void StarFadeIn(SoundEntry track) {
        ApplyTrack(track);
        fadingIn = true;
    }
    private void UpdateTrackFadeIn() {
        //The defaultTrackVolume being the target is a concern.
        if (currentTrackVolume >= currentTrackEntryVolume)
            return;

        currentTrackVolume += fadeInSpeed * Time.deltaTime;
        trackAudioSource.volume = currentTrackVolume * masterVolume * trackVolume; //So sliders affect also fade in and out.
        if (currentTrackVolume >= currentTrackEntryVolume) {
            currentTrackVolume = currentTrackEntryVolume;
            fadingIn = false;
        }
    }
    private void UpdateTrackFadeOut() {
        if (currentTrackVolume <= 0.0f)
            return;

        currentTrackVolume -= fadeOutSpeed * Time.deltaTime;
        trackAudioSource.volume = currentTrackVolume * masterVolume * trackVolume; //So sliders affect also fade in and out.
        if (currentTrackVolume <= 0.0f) {
            currentTrackVolume = 0.0f;
            trackAudioSource.Stop();
            fadingOut = false;
            if (fadeTargetTrack != null)
                StarFadeIn((SoundEntry)fadeTargetTrack);
        }
    }
    private void UpdateTrackVolume() {
        if (!trackAudioSource.isPlaying)
            return;

        trackAudioSource.volume = masterVolume * trackVolume * currentTrackEntryVolume;
    }
}
