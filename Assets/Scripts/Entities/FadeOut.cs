using System;
using UnityEngine;

public class FadeOut : MonoBehaviour {
    private bool initialized = false;


    private Action onFadeCallback = null;
    private Action onFinishedCallback = null;
    private Animation animationComp = null;


    public void Initialize() {
        if (initialized)
            return;

        SetupReferences();
        gameObject.SetActive(false);
        initialized = true;
    }
    private void SetupReferences() {
        animationComp = GetComponent<Animation>();
    }


    public void StartFadeOut(Action onFade, Action onFinished = null) {
        if (onFade == null && onFinished == null) {
            Debug.LogWarning("Unable to start FadeOut - callback provided was null");
            return;
        }

        onFadeCallback = onFade;
        onFinishedCallback = onFinished;
        animationComp.Play("FadeOut");
        gameObject.SetActive(true);
    }
    public bool IsPlaying() {
        return animationComp.isPlaying;
    }
    public void Stop() {
        if (!animationComp.isPlaying)
            return;

        animationComp.Stop();
        gameObject.SetActive(false);
        onFadeCallback = null;
        onFinishedCallback = null;
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
