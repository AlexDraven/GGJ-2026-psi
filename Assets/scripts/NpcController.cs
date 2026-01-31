using UnityEngine;

public class NpcController : MonoBehaviour
{
    bool playerInRange;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsPlayer(other))
            return;
        playerInRange = true;
        Debug.Log("[NpcController] Trigger activado: jugador cerca del NPC");
        if (PsychedelicCameraEffect.Instance != null)
            PsychedelicCameraEffect.Instance.AddIntensity(0.2f);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!IsPlayer(other))
            return;
        playerInRange = false;
    }

    static bool IsPlayer(Collider2D other)
    {
        return other.CompareTag("Player") || other.GetComponent<PlayerController>() != null;
    }

    public bool PlayerInRange => playerInRange;
}
