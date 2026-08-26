using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ParallaxBackgroundVert : MonoBehaviour
{
    public ParallaxCameraVert parallaxCamera;
    private List<ParallaxLayerVert> parallaxLayers = new List<ParallaxLayerVert>();

    void Start()
    {
        if (parallaxCamera == null)
            parallaxCamera = Camera.main.GetComponent<ParallaxCameraVert>();

        if (parallaxCamera != null)
            parallaxCamera.onCameraTranslate += Move;

        SetLayers();
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
