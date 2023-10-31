using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Countdown : MonoBehaviour {

    private bool initialized = false;


    private Action targetCallback = null;
    private Animation countdownAnimationComp = null;


    public void Initialize() {
        if (initialized)
            return;

        SetupReferences();
        gameObject.SetActive(false);
        initialized = true;
    }
    private void SetupReferences() {
        countdownAnimationComp = GetComponent<Animation>();
    }


    public void StartCountdown(Action callback) {
        if (callback == null) {
            Debug.LogWarning("Unable to start countdown - callback provided was null");
            return;
        }


        targetCallback = callback;
        countdownAnimationComp.Play("Countdown");
        gameObject.SetActive(true);
    }
    public bool IsPlaying() {
        return countdownAnimationComp.isPlaying;
    }
    public void CountdownFinished() {
        if (targetCallback != null)
            targetCallback.Invoke();
        else
            Debug.LogError("Countdown animation player but target callback was invalid!");

        gameObject.SetActive(false);
    }
}
