using UnityEngine;

[ExecuteInEditMode]
public class ParallaxCamera : MonoBehaviour
{
    public delegate void ParallaxCameraDelegate(float deltaMovement);

    // CHANGED: "event" keyword toegevoegd, zodat alleen deze class het event kan invoken/overschrijven
    // (voorkomt dat een ander script per ongeluk alle subscribers wist met '=' i.p.v. '+=').
    public event ParallaxCameraDelegate onCameraTranslate;

    private float oldPosition;

    // CHANGED: initialisatie verplaatst van Start() naar OnEnable(). Met [ExecuteInEditMode] is
    // Start() niet gegarandeerd betrouwbaar in Edit Mode / bij opnieuw enablen, wat oldPosition
    // stale kon laten. OnEnable() draait consistent in zowel Edit als Play mode.
    void OnEnable()
    {
        oldPosition = transform.position.x;
    }

    void Update()
    {
        // CHANGED: Mathf.Approximately i.p.v. een directe "!=" float-vergelijking, robuuster
        // tegen floating point ruis vanuit het transform-systeem.
        if (!Mathf.Approximately(transform.position.x, oldPosition))
        {
            float delta = oldPosition - transform.position.x;
            onCameraTranslate?.Invoke(delta); // CHANGED: null-conditional invoke i.p.v. handmatige null-check

            oldPosition = transform.position.x;
        }
    }
}
