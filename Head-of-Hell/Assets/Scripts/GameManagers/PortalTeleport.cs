using System.Collections;
using UnityEngine;

public class PortalTeleport : MonoBehaviour
{
    public Transform destinationPortal; // Assign in inspector
    AudioManager audioManager;

    void Start()
    {
        audioManager = FindObjectOfType<AudioManager>();

        if (audioManager == null)
        {
            Debug.LogError("AudioManager not found in the scene!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Try to get the Character script on the collided object
            Character character = other.GetComponent<Character>();
            if (character == null)
            {
                Debug.LogWarning("No Character script found on the player!");
                return;
            }

            // If recently teleported, don't teleport again
            if (character.justTeleported)
            {
                return;
            }

            // Teleport to destination portal but keep original Z position
            Vector3 newPos = new Vector3(
                destinationPortal.position.x,
                destinationPortal.position.y,
                other.transform.position.z
            );

            other.transform.position = newPos;

            // Play sound
            if (audioManager != null)
            {
                audioManager.PlaySFX(audioManager.portal, audioManager.normalVol);
            }

            // Start cooldown so they can't teleport again right away
            character.StartCoroutine(character.TeleportCooldown());
        }
    }
}
