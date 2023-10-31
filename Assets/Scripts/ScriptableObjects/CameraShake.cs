using UnityEngine;


[CreateAssetMenu(fileName = "CameraShake", menuName = "Data/CameraShake")]
public class CameraShake : ScriptableObject 
{
    public float amplitude = 1;
    public float duration = 0.5f;
}
