using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public struct CameraShake {
    public float amplitude;
    public float speed;
    public bool isShaking;
}
public class CameraMovement : MonoBehaviour {
    private bool initialized = false;

    private List<GameObject> targets;
    private float cameraViewAngle;
    [SerializeField] private float restingCameraDistance;
    private float targetDistance;
    private Vector3 targetPos;
    [SerializeField] private float maxTargetAngle;
    [Range(0f, 1f)]
    [SerializeField] private float zoomToFitFactor;
    [SerializeField] private float lerpSpeed;
    public CameraShake cameraShake;
    private float zDist;

    public void Initialize() {
        if (initialized) 
            return;

        maxTargetAngle *= Mathf.Deg2Rad;
        cameraViewAngle = transform.rotation.eulerAngles.x * Mathf.Deg2Rad;
        float restZdist = Mathf.Cos(cameraViewAngle) * restingCameraDistance;

        targets = new List<GameObject>();

        initialized = true;
    }
    public void Tick() {
        if (!initialized) 
            return;

        if (targets.Count != 0)
            UpdatePosition();
    }

    void UpdatePosition() {
        UpdateTargetDistance();
        UpdateTargetPosition();
        Vector3 currentPos = transform.position;
        transform.position = Vector3.Lerp(currentPos, targetPos, lerpSpeed * Time.deltaTime);
        if (cameraShake.isShaking)
            Shake();
    }

    public void Shake() {
        Vector3 position = transform.position;
        transform.position = Vector3.Lerp(position, position + UnityEngine.Random.insideUnitSphere * cameraShake.amplitude, cameraShake.speed * Time.deltaTime);
    }

    public IEnumerator ShakeFor(float duration) {
        cameraShake.isShaking = true;
        yield return new WaitForSeconds(duration);
        cameraShake.isShaking = false;
    }

    public void ShakeOnce() {
        Shake();
        cameraShake.isShaking = false;
    }

    public void AddTarget(GameObject target) {
        if (target)
            targets.Add(target);
    }

    void UpdateTargetDistance() {
        float dist = GetDistBetweenTargets();

        targetDistance = dist / (2 * Mathf.Cos(cameraViewAngle) * Mathf.Tan(maxTargetAngle));

        targetDistance = zoomToFitFactor * targetDistance + (1 - zoomToFitFactor) * restingCameraDistance;

        if (targetDistance < restingCameraDistance) 
            targetDistance = restingCameraDistance;
    }


    void UpdateTargetPosition() {
        zDist = -Mathf.Cos(cameraViewAngle) * targetDistance;
        Vector3 offset = new Vector3(0, Mathf.Sin(cameraViewAngle) * targetDistance, zDist);
        Vector3 position = GetPlayerCenter() + offset;

        targetPos = position;
    }

    Vector3 GetPlayerCenter() {
        Vector3 output = Vector3.zero;
        for (int i = 0; i < targets.Count; i++) {
            if (!targets[i]) {
                targets.RemoveAt(i);
                continue;
            }

            output += targets[i].transform.position;
        }

        output /= targets.Count;
        return output;
    }

    float GetDistBetweenTargets() {
        if (targets.Count < 2)
            return 0;

        float aspect = (float)Screen.width / Screen.height;
        Vector3 toTarget = targets[0].transform.position - targets[1].transform.position;
        toTarget.z *= aspect;
        return toTarget.magnitude;
    }
}
