using UnityEngine;
using Mandragora;

public class Level : MonoBehaviour {

    private bool initialize = false;

    private Vector3 player1SpawnPosition = Vector3.zero;
    private Vector3 player2SpawnPosition = Vector3.zero;

    public void Initialize() {
        if (initialize)
            return;

        SetupReferences();
        initialize = true;
    }
    public void Tick() {
        if (!initialize)
            return;


    }
    private void SetupReferences() {
        var player1SpawnPositionTransform = transform.Find("Player1SpawnPoint");
        var player2SpawnPositionTransform = transform.Find("Player2SpawnPoint");

        if (Utility.Validate(player1SpawnPositionTransform, "Level " + gameObject.name + " does not contain a player 1 spawn point!", Utility.ValidationType.WARNING))
            player1SpawnPosition = player1SpawnPositionTransform.position;
        if (Utility.Validate(player2SpawnPositionTransform, "Level " + gameObject.name + " does not contain a player 2 spawn point!", Utility.ValidationType.WARNING))
            player2SpawnPosition = player2SpawnPositionTransform.position;
    }

    public Vector3 GetPlayer1SpawnPosition(){
        return player1SpawnPosition;
    }
    public Vector3 GetPlayer2SpawnPosition() {
        return player2SpawnPosition;
    }
}
