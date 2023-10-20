using UnityEngine;
using Mandragora;

public class SoundManager : MonoBehaviour {

    [SerializeField] private SoundsBundle sfxBundle;
    [SerializeField] private SoundsBundle tracksBundle;

    private bool initialized = false;

    private GameObject soundUnitResource = null;
    private AudioSource trackAS = null;

    //List of audio units

    public void Initialize() {
        if (initialized)
            return;

        LoadResourcese();
        SetupReferences();
        initialized = true;
    }
    public void Tick() {
        if (!initialized)
            return;




    }
    private void SetupReferences() {
        trackAS = GetComponent<AudioSource>();
        Utility.Validate(trackAS, "Failed to get AudioSource component on SoundsManager", Utility.ValidationType.WARNING);
    }
    private void LoadResourcese() {
        soundUnitResource = Resources.Load<GameObject>("SoundUnit");
        Utility.Validate(soundUnitResource, "Failed to load SoundUnit resource.", Utility.ValidationType.WARNING);
    }
    public void PlaySFX(string key, Vector3 position) {

    }
    public void PlayTrack(string key) {

    }
    public void StopTrack() {

    }


    //Some fadeins and outs
}
