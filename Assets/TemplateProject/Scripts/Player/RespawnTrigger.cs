using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnTrigger : MonoBehaviour
{
    // CHANGED: gereviewd — logica was al correct en overzichtelijk, geen functionele wijzigingen nodig.
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Check if the colliding object has the "Player" tag.
            PlayerRespawn playerRespawn = other.GetComponent<PlayerRespawn>();

            if (playerRespawn != null)
            {
                // If the colliding object has a PlayerRespawn component, trigger the respawn.
                playerRespawn.Respawn();
            }
        }
    }
}
