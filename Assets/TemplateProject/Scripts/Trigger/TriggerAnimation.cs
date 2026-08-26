using UnityEngine;

public class TriggerAnimation : MonoBehaviour
{
    // References to the objects that will be animated
    public GameObject TriggerToAnimate;
    public GameObject TargetToAnimate;

    // References to the Animator components of both objects
    private Animator TriggerAnimator;
    private Animator TargetAnimator;

    void Start()
    {
        // Get the Animator component from the first object
        if (TriggerToAnimate != null)
        {
           TriggerAnimator = TriggerToAnimate.GetComponent<Animator>();
        }

        // Get the Animator component from the second object
        if (TargetToAnimate != null)
        {
            TargetAnimator =TargetToAnimate.GetComponent<Animator>();
        }
    }

    // This method should be called when the object is hit
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the colliding object has a tag or condition you want
        if (other.CompareTag("Player"))
        {
            // Trigger the animation on the first object
            if (TriggerAnimator != null)
            {
               TriggerAnimator.SetTrigger("TriggerAnimation");
            }

            // Trigger the animation on the second object
            if (TargetAnimator != null)
            {
                TargetAnimator.SetTrigger("TriggerAnimation");
            }
        }
    }
}
