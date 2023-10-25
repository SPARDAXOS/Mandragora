using UnityEngine;

public class CreatureDeliveryStation : MonoBehaviour {


    private void OnTriggerEnter(Collider other) {

        if (other.CompareTag("Player")) {
            var player = other.GetComponent<Player>();
            Creature creature = player.GetHeldCreature();
            if (!creature)
                return;

            if (!creature.IsSatisfied())
                return;

            creature.RegisterSatisfied();
            player.DropHeldCreature();
            //Some sparks?
        }
    }
}

