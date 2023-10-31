using System;
using UnityEngine;

public class FadeOut : MonoBehaviour {
    private bool initialized = false;


    private Action onFadeCallback = null;
    private Action onFinishedCallback = null;
    private Animation fadeOutAnimationComp = null;


    public void Initialize() {
        if (initialized)
            return;

        SetupReferences();
        gameObject.SetActive(false);
        initialized = true;
    }
    private void SetupReferences() {
        fadeOutAnimationComp = GetComponent<Animation>();
    }


    public void StartFadeOut(Action onFade, Action onFinished = null) {
        if (onFade == null && onFinished == null) {
            Debug.LogWarning("Unable to start FadeOut - callback provided was null");
            return;
        }

        onFadeCallback = onFade;
        onFinishedCallback = onFinished;
        fadeOutAnimationComp.Play("FadeOut");
        gameObject.SetActive(true);
    }
    public bool IsPlaying() {
        return fadeOutAnimationComp.isPlaying;
    }
    public void FadeOutCallback() {
        if (onFadeCallback != null)
            onFadeCallback.Invoke();
    }
    public void FadeOutFinished() {
        if (onFinishedCallback != null)
            onFinishedCallback.Invoke();


        onFadeCallback = null;
        onFinishedCallback = null;
        gameObject.SetActive(false);
    }
}
