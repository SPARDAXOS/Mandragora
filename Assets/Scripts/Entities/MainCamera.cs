using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;


[Serializable]
public struct CameraShake 
{
    public float amplitude;
    public float frequency;
    public bool isShaking;
}
public class MainCamera : MonoBehaviour 
{
    [SerializeField] private float restingCameraDistance;
    [SerializeField] private float zOffset;
    [SerializeField] private float maxAngleBetweenTargets;
    [SerializeField] private float zoomToFitFactor;
    [SerializeField] private float lerpSpeed;

    private bool initialized = false;

    private List<GameObject> targets;
    private float cameraViewAngle;
    private float targetDistance;
    private Vector3 targetPos;
    [Range(0f, 1f)]
    public CameraShake cameraShake;
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
        return UnityEngine.Random.insideUnitSphere * cameraShake.amplitude;
    }
    public IEnumerator ShakeFor(float duration) 
    {
        cameraShake.isShaking = true;
        yield return new WaitForSeconds(duration);
        cameraShake.isShaking = false;
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
        // These two lines lerp between the calculated distance and the rest distance, in case you'd want that
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

        if (cameraShake.isShaking)
        {
            position += ShakeOffset();
        }
        
        targetPos = position;
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