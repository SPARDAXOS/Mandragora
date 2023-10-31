using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Countdown : MonoBehaviour {

    private bool initialized = false;


    private Action targetCallback = null;
    private Animation countdownAnimation = null;


    public void Initialize() {
        if (initialized)
            return;

        SetupReferences();
        initialized = true;
    }
    public void Tick() {
        if (!initialized)
            return;



    }
    private void SetupReferences() {

    }


    public void StartCountdown(Action callback) {
        if (callback == null) {
            Debug.LogWarning("Unable to start countdown - callback provided was null");
            return;
        }



        targetCallback = callback;
        countdownAnimation.Play("Countdown");
    }



    public void CountdownFinished() {
        if (targetCallback != null)
            targetCallback();
        else
            Debug.LogError("Countdown animation player but target callback was invalid!");
    }
}
