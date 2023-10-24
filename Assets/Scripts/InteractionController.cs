using UnityEngine;

public class InteractionController : MonoBehaviour
{
    public float boxSize = 0.5f;
    public float maxDistance = 1.0f;
    public LayerMask detectionLayer;

    private void Update()
    {
        if (IsInteractTriggered())
        {
            PerformInteraction();
        }
    }

    private bool IsInteractTriggered()
    {
        // Implement logic to check if the interact button is triggered (I selected mouse0 (CLICK)).
        // Return true when the button is pressed 
        return Input.GetKeyDown(KeyCode.Mouse0);

        //Debug.Log("Mouse 0 was clicked");
    }

    private void PerformInteraction()
    {
        Vector3 boxcastOrigin = transform.position + transform.forward * (boxSize / 2.0f);

        if (Physics.BoxCast(boxcastOrigin, new Vector3(boxSize, boxSize, boxSize / 2.0f), transform.forward, out RaycastHit hitInfo, transform.rotation, maxDistance, detectionLayer))
        {
            // Check if the hit object has a Creature component
            Creature creature = hitInfo.collider.GetComponent<Creature>();

            if (creature != null)
            {
                // You found a creature
                Debug.Log("Found a creature!");
            }
            else
            {
                // The hit object does not have a Creature component
                Debug.Log("No creature detected.");
            }
        }
        else
        {
            // BoxCast didn't hit anything
            Debug.Log("No objects in front.");
        }
    }
}
