using UnityEngine;
using Mandragora;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine.AI;

public class Level : MonoBehaviour {

    [Header("CreatureState")]
    [SerializeField] private GameObject OnionPrefab;
    [SerializeField] private GameObject MushroomPrefab;
    [Range(1, 24)][SerializeField] private uint creaturesCount = 12;

    [Header("Spawner")]
    [Range(1, 15)] [SerializeField] private uint spawnPointCalculationRetries = 10;
    [Range(0.0f, 20.0f)][SerializeField] private float spawnHeightOffset = 7.0f;

    private bool initialize = false;

    public uint currentSatisfiedCreatures = 0;

    float leftNavMeshEdge  = 0.0f;
    float rightNavMeshEdge = 0.0f;
    float upperNavMeshEdge = 0.0f;
    float lowerNavMeshEdge = 0.0f;

    private GameInstance gameInstance = null;
    private SoundManager soundManager = null;

    private NavMeshSurface navMesh = null;

    private Vector3 player1SpawnPosition = Vector3.zero;
    private Vector3 player2SpawnPosition = Vector3.zero;

    private GameObject effects = null;

    private List<Creature> creatures = new List<Creature>();
    private TaskStation[] taskStations;
    private CreatureDeliveryStation creatureDeliveryStationScript = null;

    private Bounds navMeshBounds = new Bounds();

    private Creature tutorialCreature = null;


    public void Initialize(GameInstance instance, SoundManager soundManager) {
        if (initialize)
            return;

        this.soundManager = soundManager;
        gameInstance = instance;
        SetupReferences();
        DisableEffects();
        CreateCreaturesPool();
        initialize = true;
    }
    public void Tick() {
        if (!initialize)
            return;

        if (gameInstance.IsGameStarted()) {
            foreach (var entry in taskStations)
                entry.Tick();
        }

        if (gameInstance.IsGameStarted()) {
            foreach (var entry in creatures) {
                if (entry.GetActive())
                    entry.Tick();
            }
        }
    }
    public void FixedTick() {
        if (gameInstance.IsGameStarted()) {
            foreach (var entry in creatures) {
                if (entry.GetActive())
                    entry.FixedTick();
            }
        }
    }
    private void SetupReferences() {

        var navMeshTransform = transform.Find("NavMesh");
        Utility.Validate(navMeshTransform, "Failed to get reference to NavMesh - " + gameObject.name, Utility.ValidationType.ERROR);
        navMesh = navMeshTransform.GetComponent<NavMeshSurface>();
        Utility.Validate(navMesh, "Failed to get component NavMeshSurface in NavMesh - " + gameObject.name, Utility.ValidationType.ERROR);
        navMeshBounds = navMesh.navMeshData.sourceBounds;
        leftNavMeshEdge = transform.position.x - (navMeshBounds.size.x / 2);
        rightNavMeshEdge = transform.position.x + (navMeshBounds.size.x / 2);
        upperNavMeshEdge = transform.position.z + (navMeshBounds.size.z / 2);
        lowerNavMeshEdge = transform.position.z - (navMeshBounds.size.z / 2);

        var player1SpawnPositionTransform = transform.Find("Player1SpawnPoint");
        var player2SpawnPositionTransform = transform.Find("Player2SpawnPoint");

        effects = transform.Find("Effects").gameObject;
        Utility.Validate(effects, gameObject.name + " does not contain a Effects!", Utility.ValidationType.WARNING);


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
                    entry.Initialize(gameInstance.GetCameraScript(), soundManager);
            }
        }

        var creatureDeliveryStation = transform.Find("CreatureDeliveryStation").gameObject;
        creatureDeliveryStationScript = creatureDeliveryStation.GetComponent<CreatureDeliveryStation>();
        creatureDeliveryStationScript.Initialize();
        creatureDeliveryStationScript.SetSoundManagerReference(soundManager);
    }
    private void CreateCreaturesPool() {
        if (creaturesCount == 0)
            return;

        int rand = 0;
        for (uint i = 0; i < creaturesCount; i++) {
            GameObject go = null;
            rand = Random.Range(0, 2);
            if (rand == 0)
                go = Instantiate(OnionPrefab);
            else if (rand == 1)
                go = Instantiate(MushroomPrefab);
            else {
                Debug.LogError("Error randomizing creature types - got value " + rand);
                return;
            }

            Creature script = go.GetComponent<Creature>();
            script.Initialize(this);
            script.SetActive(false);
            creatures.Add(script);
        }
    }

    public void EnableEffects() {
        if (effects)
            effects.SetActive(true);
    }
    public void DisableEffects() {
        if (effects)
            effects.SetActive(false);
    }




    private Vector3 GetRandomPointOnNavMesh() {
        NavMeshHit hit;
        for (uint i = 0; i < spawnPointCalculationRetries; i++) {
            float randomX = Random.Range(leftNavMeshEdge, rightNavMeshEdge);
            float randomZ = Random.Range(lowerNavMeshEdge, upperNavMeshEdge);
            Vector3 randomPoint = new Vector3(randomX, gameObject.transform.position.y, randomZ);
            if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
                return hit.position;
        }
        return Vector3.zero;
    }
    private void RandomizeCreatureSpawns() {
        foreach(var creature in creatures) {
            if (!creature.GetActive())
                continue;

            Vector3 spawnPosition = GetRandomPointOnNavMesh();
            spawnPosition.y += spawnHeightOffset;
            creature.transform.position = spawnPosition;
        }
    }

    public Vector3 GetPlayer1SpawnPosition(){
        return player1SpawnPosition;
    }
    public Vector3 GetPlayer2SpawnPosition() {
        return player2SpawnPosition;
    }


    public void RegisterCreatureDesatisfied() {
        gameInstance.EndGame(GameInstance.GameResults.LOSE);
    }
    public void RegisterSatisfiedCreature() {
        currentSatisfiedCreatures++;
        if (currentSatisfiedCreatures == creaturesCount)
            gameInstance.EndGame(GameInstance.GameResults.WIN);
    }


    public Creature StartTutorial() {
        if (tutorialCreature)
            return tutorialCreature;

        currentSatisfiedCreatures = 0;

        tutorialCreature = creatures[0];
        tutorialCreature.ClearAllTasks();
        tutorialCreature.SetActive(true);
        tutorialCreature.SetupStartState();
        tutorialCreature.StopDissatisfaction();
        tutorialCreature.SetTutorialCreature(true); //THIS!!!!! Need to control it for tutorial of delivering creatures!
        Vector3 spawnPosition = GetRandomPointOnNavMesh();
        spawnPosition.y += spawnHeightOffset;
        tutorialCreature.transform.position = spawnPosition;
        soundManager.PlayTrack("Tutorial", true);
        return tutorialCreature;
    }


    public void StartLevel() {
        EnableEffects();

        foreach (var entry in creatures) {
            entry.SetActive(true);
            entry.SetupStartState();
        }
        foreach (var taskStation in taskStations)
            taskStation.ResetState();

        RandomizeCreatureSpawns();
        currentSatisfiedCreatures = 0;
        //soundManager.PlayTrack("Gameplay", true);
    }
    public void GameOver() {

        DisableEffects();

        foreach (var creature in creatures)
            creature.SetActive(false);

        //will happen at start regardless
        foreach (var taskStation in taskStations)
            taskStation.ResetState(); //Some other reset function!
    }
}
