using UnityEngine;

public class SoundUnit : MonoBehaviour {

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

    private void RandomizePitch(float min, float max) {
        if (min == max) {
            audioSource.pitch = max;
            return;
        }

        audioSource.pitch = Random.Range(min, max);
    }
}
