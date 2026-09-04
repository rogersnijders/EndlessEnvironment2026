using UnityEngine;

[ExecuteInEditMode]
public class ParallaxCameraVert : MonoBehaviour
{
    public delegate void ParallaxCameraDelegate(float deltaMovement);

    // CHANGED: "event" keyword toegevoegd, zodat alleen deze class het event kan invoken/overschrijven
    // (voorkomt dat een ander script per ongeluk alle subscribers wist met '=' i.p.v. '+=').
    public event ParallaxCameraDelegate onCameraTranslate;

    private float oldPosition;

    // CHANGED: initialisatie verplaatst van Start() naar OnEnable(), zelfde reden als ParallaxCamera.cs:
    // Start() is niet gegarandeerd betrouwbaar met [ExecuteInEditMode].
    void OnEnable()
    {
        oldPosition = transform.position.y;
    }

    void Update()
    {
        // CHANGED: Mathf.Approximately i.p.v. een directe "!=" float-vergelijking.
        if (!Mathf.Approximately(transform.position.y, oldPosition))
        {
            float delta = oldPosition - transform.position.y;
            onCameraTranslate?.Invoke(delta); // CHANGED: null-conditional invoke i.p.v. handmatige null-check

            oldPosition = transform.position.y;
        }
    }
}
