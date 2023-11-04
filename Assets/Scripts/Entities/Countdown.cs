using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Countdown : MonoBehaviour {

    private bool initialized = false;

    private SoundManager soundManager = null;

    private Action targetCallback = null;
    private Animation animationComp = null;

    private GameObject GUI = null;


    public void Initialize(SoundManager soundManager) {
        if (initialized)
            return;

        this.soundManager = soundManager;
        SetupReferences();
        gameObject.SetActive(false);
        initialized = true;
    }
    private void SetupReferences() {
        animationComp = GetComponent<Animation>();
        GUI = transform.Find("GUI").gameObject;
        GUI.gameObject.SetActive(false);
    }


    public void StartCountdown(Action callback) {
        if (callback == null) {
            Debug.LogWarning("Unable to start countdown - callback provided was null");
            return;
        }


        targetCallback = callback;
        animationComp.Play("Countdown");
        gameObject.SetActive(true);
        GUI.gameObject.SetActive(true);
    }
    public bool IsPlaying() {
        return animationComp.isPlaying;
    }
    public void Stop() {
        if (!animationComp.isPlaying)
            return;

        animationComp.Stop();
        gameObject.SetActive(false);
        GUI.gameObject.SetActive(false);
        targetCallback = null;
    }
    public void CountdownFinished() {
        if (targetCallback != null)
            targetCallback.Invoke();
        else
            Debug.LogError("Countdown animation player but target callback was invalid!");

        gameObject.SetActive(false);
        GUI.gameObject.SetActive(false);
        targetCallback = null;
    }
    public void PlayCountdownSFX() {
        soundManager.PlaySFX("Countdown", Vector3.zero, true);
    }
    public void PlayCountdownFinishedSFX() {
        soundManager.PlaySFX("CountdownFinished", Vector3.zero, true);
    }
}
