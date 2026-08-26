using UnityEngine;

[ExecuteInEditMode]
public class ParallaxLayerVert : MonoBehaviour
{
    public float parallaxFactor;

    public void Move(float delta)
    {
        Vector3 newPos = transform.localPosition;
        newPos.y -= delta * parallaxFactor;  // VERTICAAL in plaats van horizontaal

        transform.localPosition = newPos;
    }
}
