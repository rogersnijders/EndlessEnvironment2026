using UnityEngine;
using UnityEngine.U2D;

public class FollowSpriteShape : MonoBehaviour
{
    public SpriteShapeController primaryShape;
    private SpriteShapeController secondaryShape;

    void Start()
    {
        secondaryShape = GetComponent<SpriteShapeController>();
    }

    void Update()
    {
        if (primaryShape != null && secondaryShape != null)
        {
            CopyShapePoints(primaryShape, secondaryShape);
        }
    }

    void CopyShapePoints(SpriteShapeController source, SpriteShapeController target)
    {
        // Clear existing points
        target.spline.Clear();

        // Copy points from source to target
        for (int i = 0; i < source.spline.GetPointCount(); i++)
        {
            Vector3 position = source.spline.GetPosition(i);
            ShapeTangentMode tangentMode = source.spline.GetTangentMode(i);  // Corrected tangent mode type
            Vector3 leftTangent = source.spline.GetLeftTangent(i);
            Vector3 rightTangent = source.spline.GetRightTangent(i);
            
            target.spline.InsertPointAt(i, position);
            target.spline.SetTangentMode(i, tangentMode);
            target.spline.SetLeftTangent(i, leftTangent);
            target.spline.SetRightTangent(i, rightTangent);
        }

        // Apply changes to the SpriteShape
        target.BakeCollider();
    }
}
