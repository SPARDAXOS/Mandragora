using UnityEngine;

public class UISettingsMenu : MonoBehaviour
{
    private bool initialize = false;
    private GameInstance gameInstance = null;


    public void Initialize(GameInstance reference)
    {
        if (initialize)
            return;

        gameInstance = reference;
        initialize = true;
    }

    public void ReturnFromSettings()
    {

    }
}
