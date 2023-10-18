using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mandragora;
using UnityEditor;
using static UnityEditor.Experimental.GraphView.GraphView;

namespace Initialization {
    public class Initialization {

        [RuntimeInitializeOnLoadMethod]
        public static void InitilaizeGame() {
            var resource = Resources.Load<GameObject>("GameInstance");
            var gameInstance = GameObject.Instantiate(resource);
            var Comp = gameInstance.GetComponent<GameInstance>();
            Comp.Initialize();
        }
    }
}

public class GameInstance : MonoBehaviour {

    private GameObject playerResource;

    private GameObject player1 = null;
    private GameObject player2 = null;

    void Start()
    {
        
    }
    void Update()
    {
        
    }
    public void Initialize() {
        LoadResources();
        CreateEntities();


    }
    public static void Abort(string message) {
        Debug.LogError(message);

#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    private void LoadResources() {
        playerResource = Resources.Load<GameObject>("Player");
        if (!Utility.Validate(playerResource, "", Utility.ValidationType.ERROR))
            Abort("Failed to load player");

    }
    private void CreateEntities() {
        player1 = Instantiate(playerResource);
        player2 = Instantiate(playerResource);
    }


    
}
