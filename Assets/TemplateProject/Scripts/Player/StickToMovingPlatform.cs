using UnityEngine;

public class StickToMovingPlatform : MonoBehaviour
{
    // CHANGED: gereviewd — logica is correct zoals hij was. Alleen een waarschuwing als comment toegevoegd:
    // door direct te parenten aan het platform erft dit object de scale van het platform.
    // Als het platform ooit anders geschaald wordt dan (1,1,1), vervormt dit object visueel.
    // Niet functioneel aangepast omdat dit afhangt van of platforms in dit project geschaald worden;
    // overweeg localScale te cachen vóór het parenten als dit een probleem wordt.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("MovingPlatform"))
        {
            transform.parent = collision.transform;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("MovingPlatform"))
        {
            transform.parent = null;
        }
    }
}
