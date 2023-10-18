using UnityEngine;
using Mandragora;

public class Level : MonoBehaviour {

    private bool initialize = false;

    private Vector3 player1SpawnPosition = Vector3.zero;
    private Vector3 player2SpawnPosition = Vector3.zero;
    private Vector3 player3SpawnPosition = Vector3.zero;
    private Vector3 player4SpawnPosition = Vector3.zero;

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
        var player3SpawnPositionTransform = transform.Find("Player3SpawnPoint");
        var player4SpawnPositionTransform = transform.Find("Player4SpawnPoint");

        if (Utility.Validate(player1SpawnPositionTransform, "Level " + gameObject.name + " does not contain a player 1 spawn point!", Utility.ValidationType.WARNING))
            player1SpawnPosition = player1SpawnPositionTransform.position;
        if (Utility.Validate(player2SpawnPositionTransform, "Level " + gameObject.name + " does not contain a player 2 spawn point!", Utility.ValidationType.WARNING))
            player2SpawnPosition = player2SpawnPositionTransform.position;
        if (Utility.Validate(player3SpawnPositionTransform, "Level " + gameObject.name + " does not contain a player 3 spawn point!", Utility.ValidationType.WARNING))
            player3SpawnPosition = player3SpawnPositionTransform.position;
        if (Utility.Validate(player4SpawnPositionTransform, "Level " + gameObject.name + " does not contain a player 4 spawn point!", Utility.ValidationType.WARNING))
            player4SpawnPosition = player4SpawnPositionTransform.position;
    }

    public Vector3 GetPlayer1SpawnPosition(){
        return player1SpawnPosition;
    }
    public Vector3 GetPlayer2SpawnPosition() {
        return player2SpawnPosition;
    }
    public Vector3 GetPlayer3SpawnPosition() {
        return player3SpawnPosition;
    }
    public Vector3 GetPlayer4SpawnPosition() {
        return player4SpawnPosition;
    }

}
