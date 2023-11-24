using UnityEngine;

public class SoundUnit : MonoBehaviour {

    public enum SpatialBlendMode { TWO_DIMENSIONAL, THREE_DIMENSIONAL }

    private bool initialize = false;
    private AudioSource audioSource = null;
    
    public void Initialize() {
        if (initialize)
            return;


        audioSource = GetComponent<AudioSource>();
        initialize = true;
    }
    public bool IsPlaying() {
        return audioSource.isPlaying;
    }
    public bool Play(SoundEntry entry, Vector3 position, float volume) {
        if (IsPlaying())
            return false;

        RandomizePitch(entry.minPitch, entry.maxPitch);

        audioSource.volume = volume;
        transform.position = position;
        audioSource.clip = entry.clip;
        audioSource.Play();
        return true;
    }
    public void SetSpatialBlend(SpatialBlendMode mode) {
        if (mode == SpatialBlendMode.TWO_DIMENSIONAL)
            audioSource.spatialBlend = 0.0f;
        else if (mode == SpatialBlendMode.THREE_DIMENSIONAL)
            audioSource.spatialBlend = 1.0f;
    }

    private void RandomizePitch(float min, float max) {
        if (min == max) {
            audioSource.pitch = max;
            return;
        }

        audioSource.pitch = Random.Range(min, max);
    }
}
