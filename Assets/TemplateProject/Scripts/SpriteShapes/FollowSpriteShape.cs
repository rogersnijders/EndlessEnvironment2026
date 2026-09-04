using UnityEngine;
using UnityEngine.U2D;

public class FollowSpriteShape : MonoBehaviour
{
    public SpriteShapeController primaryShape;
    private SpriteShapeController secondaryShape;

    // CHANGED: syncInterval toegevoegd zodat de relatief dure spline-copy + BakeCollider() niet per se
    // elke frame hoeft te draaien. Standaard 0 = zelfde gedrag als voorheen (elke frame);
    // hoger zetten in de Inspector verlaagt de kosten wanneer perfect realtime sync niet nodig is.
    [SerializeField] private float syncInterval = 0f;
    private float syncTimer;

    void Start()
    {
        secondaryShape = GetComponent<SpriteShapeController>();
    }

    void Update()
    {
        if (primaryShape == null || secondaryShape == null) return; // CHANGED: early-out, iets overzichtelijker dan de gecombineerde if van voorheen

        // CHANGED: updates afgeremd met syncTimer/syncInterval i.p.v. altijd elke frame te kopiëren.
        syncTimer += Time.deltaTime;
        if (syncTimer < syncInterval) return;
        syncTimer = 0f;

        CopyShapePoints(primaryShape, secondaryShape);
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
