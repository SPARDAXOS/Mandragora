using UnityEngine;

public class CreatureDeliveryStation : MonoBehaviour {

    private SoundManager soundManager;

    public void SetSoundManagerReference(SoundManager soundManager) {
        this.soundManager = soundManager;
    }


    private void OnTriggerStay(Collider other) {
        if (other.CompareTag("Player")) {
            var player = other.GetComponent<Player>();
            Creature creature = player.GetHeldCreature();
            if (!creature)
                return;

            if (!creature.IsSatisfied() || creature.GetTutorialCreature())
                return;

            creature.RegisterSatisfied();
            player.DropHeldCreature();
            if (!soundManager)
                soundManager.PlaySFX("CreatureDelivered", Vector3.zero, true, false, gameObject);
            else
                Debug.LogWarning("SoundManager is not set on " + gameObject.name);

            //Some sparks?
        }
    }
    private void OnTriggerEnter(Collider other) {


    }
}

