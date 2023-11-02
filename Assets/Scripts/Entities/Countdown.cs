using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Countdown : MonoBehaviour {

    private bool initialized = false;


    private Action targetCallback = null;
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


    public void StartCountdown(Action callback) {
        if (callback == null) {
            Debug.LogWarning("Unable to start countdown - callback provided was null");
            return;
        }


        targetCallback = callback;
        animationComp.Play("Countdown");
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
        targetCallback = null;
    }
    public void CountdownFinished() {
        if (targetCallback != null)
            targetCallback.Invoke();
        else
            Debug.LogError("Countdown animation player but target callback was invalid!");

        gameObject.SetActive(false);
        targetCallback = null;
    }
}
