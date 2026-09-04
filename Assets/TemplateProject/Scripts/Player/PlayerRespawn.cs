using System.Collections;
using UnityEngine;
using UnityEngine.Serialization; // CHANGED: nodig voor FormerlySerializedAs, gebruikt hieronder om een public field veilig te hernoemen

public class PlayerRespawn : MonoBehaviour
{
    public Transform respawnPoint; // The point where the player will respawn.

    // CHANGED: hernoemd van "thirdGameObjectAnimator" naar "respawnEffectAnimator" voor een zelfverklarende naam.
    // FormerlySerializedAs zorgt dat een reeds ingestelde waarde in de Inspector niet verloren gaat door de rename.
    [FormerlySerializedAs("thirdGameObjectAnimator")]
    public Animator respawnEffectAnimator;

    private void Start()
    {
        if (respawnPoint == null)
        {
            Debug.LogError("Respawn point not set for the player!", this); // CHANGED: 'this' toegevoegd als context zodat de log naar het juiste GameObject verwijst
        }

        if (respawnEffectAnimator == null)
        {
            Debug.LogError("Respawn effect Animator not assigned!", this); // CHANGED: idem + veldnaam in bericht bijgewerkt
        }
    }

    // This method will be called to respawn the player.
    public void Respawn()
    {
        // Ensure the final position is set to the respawn point.
        transform.position = respawnPoint.position;

        // CHANGED: guard tegen een ontbrekende animator toegevoegd, i.p.v. een NullReferenceException
        // te riskeren (Start() logt alleen een error maar voorkomt niet dat Respawn() alsnog wordt aangeroepen).
        if (respawnEffectAnimator != null)
        {
            respawnEffectAnimator.SetTrigger("PlayAnimationTrigger");
        }
    }
}
