using UnityEngine;
using Mandragora;
using System.Collections.Generic;

public class Level : MonoBehaviour {

    private bool initialize = false;

    private GameInstance gameInstance = null;

    private Vector3 player1SpawnPosition = Vector3.zero;
    private Vector3 player2SpawnPosition = Vector3.zero;

    private TaskStation[] taskStations;

    public void Initialize(GameInstance instance) {
        if (initialize)
            return;

        gameInstance = instance;
        SetupReferences();
        initialize = true;
    }
    public void Tick() {
        if (!initialize)
            return;

        foreach (var entry in taskStations)
            entry.Tick();
    }
    private void SetupReferences() {
        var player1SpawnPositionTransform = transform.Find("Player1SpawnPoint");
        var player2SpawnPositionTransform = transform.Find("Player2SpawnPoint");

        if (Utility.Validate(player1SpawnPositionTransform, gameObject.name + " does not contain a player 1 spawn point!", Utility.ValidationType.WARNING))
            player1SpawnPosition = player1SpawnPositionTransform.position;
        if (Utility.Validate(player2SpawnPositionTransform, gameObject.name + " does not contain a player 2 spawn point!", Utility.ValidationType.WARNING))
            player2SpawnPosition = player2SpawnPositionTransform.position;

        var taskStationsTransform = transform.Find("TaskStations");
        if (Utility.Validate(taskStationsTransform, gameObject.name + " does not contain a TaskStations!", Utility.ValidationType.WARNING)) {
            taskStations = taskStationsTransform.GetComponentsInChildren<TaskStation>();
            if (taskStations.Length == 0)
                Debug.LogWarning("TaskStations does not contain any children!");
            else {
                foreach (var entry in taskStations)
                    entry.Initialize(gameInstance.GetCameraScript());
            }
        }
    }

    public Vector3 GetPlayer1SpawnPosition(){
        return player1SpawnPosition;
    }
    public Vector3 GetPlayer2SpawnPosition() {
        return player2SpawnPosition;
    }
}
