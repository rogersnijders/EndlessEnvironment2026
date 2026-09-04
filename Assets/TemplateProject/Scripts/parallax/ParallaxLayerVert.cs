using UnityEngine;

[ExecuteInEditMode]
public class ParallaxLayerVert : MonoBehaviour
{
    public float parallaxFactor;

    // CHANGED: gereviewd op sign-consistency met ParallaxCameraVert.cs — geen wijziging nodig.
    // ParallaxCameraVert geeft nog steeds delta = oldPosition - newPosition door, en deze Move()
    // trekt nog steeds delta * parallaxFactor af, dus de twee scripts blijven consistent.
    public void Move(float delta)
    {
        Vector3 newPos = transform.localPosition;
        newPos.y -= delta * parallaxFactor;  // VERTICAAL in plaats van horizontaal

        transform.localPosition = newPos;
    }
}
