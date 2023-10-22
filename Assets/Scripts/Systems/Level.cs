using UnityEngine;
using Mandragora;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine.AI;

public class Level : MonoBehaviour {

    [SerializeField] private GameObject CreaturePrefab;
    [SerializeField] private uint creaturesCount = 7;


    private bool initialize = false;

    private GameInstance gameInstance = null;


    private GameObject floorPlane = null;

    private NavMeshSurface navMesh = null;

    private Vector3 player1SpawnPosition = Vector3.zero;
    private Vector3 player2SpawnPosition = Vector3.zero;

    private List<Creature> creatures = new List<Creature>();
    private TaskStation[] taskStations;

    public Bounds test = new Bounds();
    public void Initialize(GameInstance instance) {
        if (initialize)
            return;

        gameInstance = instance;
        SetupReferences();
        CreateCreaturesPool();
        initialize = true;
    }
    public void Tick() {
        if (!initialize)
            return;

        foreach (var entry in taskStations)
            entry.Tick();

        test = navMesh.navMeshData.sourceBounds;
        Debug.Log(test.size);
        
    }
    private void SetupReferences() {

        var navMeshTransform = transform.Find("NavMesh");
        Utility.Validate(navMeshTransform, "Failed to get reference to NavMesh - " + gameObject.name, Utility.ValidationType.ERROR);
        navMesh = navMeshTransform.GetComponent<NavMeshSurface>();
        Utility.Validate(navMesh, "Failed to get component NavMeshSurface in NavMesh - " + gameObject.name, Utility.ValidationType.ERROR);
        

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
    private void CreateCreaturesPool() {
        if (creaturesCount == 0)
            return;
        for (uint i = 0; i < creaturesCount; i++) {
            GameObject go = Instantiate(CreaturePrefab);
            Creature script = go.GetComponent<Creature>();
            script.Initialize();
            creatures.Add(script);
        }
        RandomizeCreatureSpawns();
    }



    private bool RandomPoint(Vector3 center, float range, out Vector3 result) {

        //Get random point on the floor plane of this level


        for (int i = 0; i < 30; i++) {
            Vector3 randomPoint = center + Random.insideUnitSphere * range;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas)) {
                result = hit.position;
                return true;
            }
        }
        result = Vector3.zero;
        return false;
    }


    private void RandomizeCreatureSpawns() {
        NavMeshTriangulation data = NavMesh.CalculateTriangulation();

        foreach(var creature in creatures) {
            int index = Random.Range(0, data.vertices.Length);
            NavMeshHit hitResults;
            if (NavMesh.SamplePosition(data.vertices[index], out hitResults, 2.0f, 0)) {
                creature.transform.position = hitResults.position;
                Debug.Log(hitResults.position);
            }
        }
    }

    public Vector3 GetPlayer1SpawnPosition(){
        return player1SpawnPosition;
    }
    public Vector3 GetPlayer2SpawnPosition() {
        return player2SpawnPosition;
    }


    public void GameOver() {

    }
}
