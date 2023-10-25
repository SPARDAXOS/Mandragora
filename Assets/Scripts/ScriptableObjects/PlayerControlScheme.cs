using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "PlayerControllerScheme", menuName = "Data/PlayerControlScheme", order = 2)]
public class PlayerControlScheme : ScriptableObject
{
    public InputAction movement;
    public InputAction interact;
    public InputAction dash;
    public InputAction pause;
}
