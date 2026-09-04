using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ParallaxBackground : MonoBehaviour
{
    public ParallaxCamera parallaxCamera;
    List<ParallaxLayer> parallaxLayers = new List<ParallaxLayer>();

    // CHANGED: subscribe-logica verplaatst van Start() naar OnEnable()/OnDisable() hieronder.
    // Start() alleen unsubscribede nooit, waardoor het opnieuw enablen van dit object (of
    // [ExecuteInEditMode]-recompiles) dezelfde Move()-callback meerdere keren kon registreren,
    // wat Move() meerdere keren per camerabeweging liet afvuren.
    void OnEnable()
    {
        if (parallaxCamera == null)
            parallaxCamera = Camera.main != null ? Camera.main.GetComponent<ParallaxCamera>() : null; // CHANGED: guard tegen ontbrekende main camera

        if (parallaxCamera != null)
            parallaxCamera.onCameraTranslate += Move;

        SetLayers();
    }

    // CHANGED: toegevoegd — unsubscribet van het camera-event zodra dit object disabled/destroyed wordt,
    // voorkomt de memory/event leak die eerder in de review naar voren kwam.
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
            ParallaxLayer layer = transform.GetChild(i).GetComponent<ParallaxLayer>();

            if (layer != null)
            {
                layer.name = "Layer-" + i;
                parallaxLayers.Add(layer);
            }
        }
    }

    void Move(float delta)
    {
        foreach (ParallaxLayer layer in parallaxLayers)
        {
            layer.Move(delta);
        }
    }
}
