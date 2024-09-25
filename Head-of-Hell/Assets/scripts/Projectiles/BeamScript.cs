using System.Collections;
using UnityEngine;

public class BeamScript : MonoBehaviour
{
    // Reference to the PlayerScript
    public PlayerScript playa;

    // Reference to the Collider2D on this object (assuming it's 2D)
    private Collider2D beamCollider;

    void Start()
    {
        // Get the Collider2D component attached to this GameObject
        beamCollider = GetComponent<Collider2D>();

        // Initially deactivate the collider
        DeactivateCollider();

        
    }

    void Update()
    {
        // Start a coroutine to disable the beam after 1 second
        StartCoroutine(DisableBeamAfterTime(1f));
    }

    // Method to activate the collider
    public void ActivateCollider()
    {
        if (beamCollider != null)
        {
            beamCollider.enabled = true;
        }
    }

    // Method to deactivate the collider
    public void DeactivateCollider()
    {
        if (beamCollider != null)
        {
            beamCollider.enabled = false;
        }
    }

    // Detect when the collider hits something
    void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the object we hit has the tag "Player"
        if (collision.CompareTag("Player"))
        {
                playa.BeamHit();
        }
    }

    // Coroutine to disable the beam GameObject after a delay
    private IEnumerator DisableBeamAfterTime(float delay)
    {
        // Wait for the specified amount of time
        yield return new WaitForSeconds(delay);

        // Deactivate this GameObject (disabling the beam)
        gameObject.SetActive(false);
    }
}
