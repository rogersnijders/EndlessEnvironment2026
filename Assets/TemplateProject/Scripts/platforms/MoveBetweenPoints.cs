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

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
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
