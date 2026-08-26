using System.Collections;
using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    public Transform respawnPoint; // The point where the player will respawn.

    // Reference to the Animator component on the third GameObject
    public Animator thirdGameObjectAnimator;

    private void Start()
    {
        if (respawnPoint == null)
        {
            UnityEngine.Debug.LogError("Respawn point not set for the player!");
        }

        // Ensure that the thirdGameObjectAnimator is assigned
        if (thirdGameObjectAnimator == null)
        {
            UnityEngine.Debug.LogError("Third GameObject Animator not assigned!");
        }
    }

    // This method will be called to respawn the player.
    public void Respawn()
    {
        // Ensure the final position is set to the respawn point.
        transform.position = respawnPoint.position;

        // Trigger the animation on the third GameObject
        thirdGameObjectAnimator.SetTrigger("PlayAnimationTrigger");

    }
}