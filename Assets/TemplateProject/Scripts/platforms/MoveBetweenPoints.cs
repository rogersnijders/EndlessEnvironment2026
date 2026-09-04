using UnityEngine;

public class MoveBetweenPoints : MonoBehaviour
{
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float speed = 2.0f;
    [SerializeField] private float delayAtPoints = 1.0f;

    private Vector3 targetPosition;
    private bool movingToPointB = true;
    private float delayTimer = 0.0f;

    // CHANGED: drempelwaarde één keer als kwadraat vastgelegd, zodat Update() geen sqrt hoeft
    // te berekenen (was Vector3.Distance, die intern een sqrt doet).
    private const float ArrivalThreshold = 0.1f;
    private static readonly float ArrivalThresholdSqr = ArrivalThreshold * ArrivalThreshold;

    void Start()
    {
        targetPosition = pointB.position;
    }

    void Update()
    {
        MoveObject();
    }

    private void MoveObject()
    {
        if (delayTimer > 0)
        {
            delayTimer -= Time.deltaTime;
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        // CHANGED: sqrMagnitude-vergelijking i.p.v. Vector3.Distance(), voorkomt een sqrt() elke frame.
        if ((transform.position - targetPosition).sqrMagnitude < ArrivalThresholdSqr)
        {
            if (movingToPointB)
            {
                targetPosition = pointA.position;
            }
            else
            {
                targetPosition = pointB.position;
            }
            movingToPointB = !movingToPointB;
            delayTimer = delayAtPoints;
        }
    }
}
