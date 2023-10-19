using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "PlayerControlScheme", menuName = "Data/PlayerControlScheme", order = 2)]
public class PlayerControlScheme : ScriptableObject {

    public InputAction movement;
    public InputAction interact;

}
