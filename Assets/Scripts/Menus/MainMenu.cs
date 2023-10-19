using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour {

    private GameInstance gameInstance;

    public void Initialize(GameInstance reference) {
        gameInstance = reference;
    }


    public void StartButton() {
        if (!gameInstance)
            return;

        gameInstance.SetGameState(GameInstance.GameState.PLAYING);
    }
}
