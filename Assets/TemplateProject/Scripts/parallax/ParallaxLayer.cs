using UnityEngine;

[ExecuteInEditMode]
public class ParallaxLayer : MonoBehaviour
{
    public float parallaxFactor;

    // CHANGED: gereviewd op sign-consistency met ParallaxCamera.cs — geen wijziging nodig.
    // ParallaxCamera geeft nog steeds delta = oldPosition - newPosition door, en deze Move()
    // trekt nog steeds delta * parallaxFactor af, dus de twee scripts blijven consistent.
    public void Move(float delta)
    {
        Vector3 newPos = transform.localPosition;
        newPos.x -= delta * parallaxFactor;

        transform.localPosition = newPos;
    }
}
