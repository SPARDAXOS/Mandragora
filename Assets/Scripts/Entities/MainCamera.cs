using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class MainCamera : MonoBehaviour
{
    [SerializeField] private float restingCameraDistance;
    [SerializeField] private float zOffset;
    [SerializeField] private float maxAngleBetweenTargets;
    [SerializeField] private float zoomToFitFactor;
    [SerializeField] private float lerpSpeed;

    private bool initialized = false;

    private CameraShake cameraShake;
    private float shakeMultiplier = 0f;
    private List<GameObject> targets;
    private float cameraViewAngle;
    private float targetDistance;
    private Vector3 targetPos;
    private float zDist;

    public void Initialize() 
    {
        if (initialized) 
            return;

        maxAngleBetweenTargets *= Mathf.Deg2Rad;
        cameraViewAngle = transform.rotation.eulerAngles.x * Mathf.Deg2Rad;
        float restZdist = Mathf.Cos(cameraViewAngle) * restingCameraDistance;

        targets = new List<GameObject>();

        initialized = true;
    }
    public void FixedTick() 
    {
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
    }
    private Vector3 ShakeOffset() 
    {
        return Vector3.zero;
        if (shakeMultiplier <= 0) return Vector3.zero;
        return UnityEngine.Random.insideUnitSphere * cameraShake.amplitude * shakeMultiplier * shakeMultiplier;
    }
    

    // REAL CODE!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    public void ShakeFor(CameraShake cameraShake) 
    {
        this.cameraShake = cameraShake;
        shakeMultiplier = 1f;
    }

public void AddTarget(GameObject target) 
    {
        if (target)
            targets.Add(target);
    }
    void UpdateTargetDistance() 
    {
        float dist = GetDistBetweenTargets();

        targetDistance = dist / (2 * Mathf.Cos(cameraViewAngle) * Mathf.Tan(maxAngleBetweenTargets));
        // These two lines below lerp between the calculated distance and the rest distance, in case you'd want that
        targetDistance *= zoomToFitFactor;
        targetDistance += (1 - zoomToFitFactor) * restingCameraDistance;

        if (targetDistance < restingCameraDistance) 
            targetDistance = restingCameraDistance;
    }
    void UpdateTargetPosition() 
    {
        zDist = -Mathf.Cos(cameraViewAngle) * targetDistance;
        Vector3 offset = new Vector3(0, Mathf.Sin(cameraViewAngle) * targetDistance, zDist + zOffset);
        Vector3 position = GetPlayerCenter() + offset;

        position += ShakeOffset();
        targetPos = position;

        if(shakeMultiplier > 0f)
            shakeMultiplier -= Time.fixedDeltaTime / cameraShake.duration;
    }
    Vector3 GetPlayerCenter() 
    {
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
    float GetDistBetweenTargets() 
    {
        if (targets.Count < 2)
            return 0;

        float aspect = (float)Screen.width / Screen.height;
        float correctAngle = Mathf.Sin(cameraViewAngle);
        Vector3 toTarget = targets[0].transform.position - targets[1].transform.position;
        float distance = Mathf.Max(Mathf.Abs(toTarget.x), Mathf.Abs(toTarget.z));
        return distance;
    }
}
