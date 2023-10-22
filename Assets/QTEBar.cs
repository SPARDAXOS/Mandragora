using UnityEngine;
using Mandragora;

public class QTEBar : MonoBehaviour {



    public void ToggleQTEBarTrigger() {
        var script = GetComponentInParent<TaskStation>();

        if (!Utility.Validate(script, "QTEBarCursor couldnt find TaskStation component in parent!", Utility.ValidationType.ERROR))
            return;

        script.ToggleQTEBarTrigger();
    }
}
