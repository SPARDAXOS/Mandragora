using UnityEngine;

public class CreatureDeliveryStation : MonoBehaviour {

    private bool initialized = false;
    private SoundManager soundManager;

    private GameObject customersLine = null;
    private Animation customersLineAnimationComp = null;

    public void Initialize() {
        if (initialized)
            return;

        SetupReferences();
        initialized = true;
    }
    private void SetupReferences() {
        customersLine = transform.Find("CustomersLine").gameObject;
        customersLineAnimationComp = customersLine.GetComponent<Animation>();
    }
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
            if (soundManager)
                soundManager.PlaySFX("CreatureDelivered", Vector3.zero, true, false, gameObject);
            else
                Debug.LogWarning("SoundManager is not set on " + gameObject.name);

            if (customersLineAnimationComp)
                customersLineAnimationComp.Play("UpdateQueue");
            else
                Debug.LogWarning("Animation is not set on " + gameObject.name);
            //Some sparks?
        }
    }
}

