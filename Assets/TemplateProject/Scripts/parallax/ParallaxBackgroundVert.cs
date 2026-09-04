using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ParallaxBackgroundVert : MonoBehaviour
{
    public ParallaxCameraVert parallaxCamera;
    private List<ParallaxLayerVert> parallaxLayers = new List<ParallaxLayerVert>();

    // CHANGED: subscribe-logica verplaatst van Start() naar OnEnable()/OnDisable() hieronder,
    // zelfde reden als ParallaxBackground.cs: voorkomt dubbele subscriptions en een event leak.
    void OnEnable()
    {
        if (parallaxCamera == null)
            parallaxCamera = Camera.main != null ? Camera.main.GetComponent<ParallaxCameraVert>() : null; // CHANGED: guard tegen ontbrekende main camera

        if (parallaxCamera != null)
            parallaxCamera.onCameraTranslate += Move;

        SetLayers();
    }

    // CHANGED: toegevoegd — unsubscribet van het camera-event zodra dit object disabled/destroyed wordt.
    void OnDisable()
    {
        if (parallaxCamera != null)
            parallaxCamera.onCameraTranslate -= Move;
    }

    void SetLayers()
    {
        parallaxLayers.Clear();

        for (int i = 0; i < transform.childCount; i++)
        {
            ParallaxLayerVert layer = transform.GetChild(i).GetComponent<ParallaxLayerVert>();

            if (layer != null)
            {
                layer.name = "Layer-" + i;
                parallaxLayers.Add(layer);
            }
        }
    }

    void Move(float delta)
    {
        foreach (ParallaxLayerVert layer in parallaxLayers)
        {
            layer.Move(delta);
        }
    }
}
