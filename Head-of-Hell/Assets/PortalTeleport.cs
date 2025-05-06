using System.Collections;
using UnityEngine;

public class PortalTeleport : MonoBehaviour
{
    public Transform destinationPortal; // Assign the other portal's Transform in the inspector
    public bool canTeleport = true; // Prevents immediate re-teleporting loop
    AudioManager audioManager;

    void Start()
    {
        audioManager = FindObjectOfType<AudioManager>(); // Find and assign the AudioManager

        if (audioManager == null)
        {
            Debug.LogError("AudioManager not found in the scene!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (canTeleport && other.CompareTag("Player"))
        {
            // Teleport player to destination
            other.transform.position = destinationPortal.position;

            audioManager.PlaySFX(audioManager.portal, audioManager.normalVol);

            // Disable teleporting temporarily on the destination portal
            PortalTeleport destPortal = destinationPortal.GetComponent<PortalTeleport>();
            if (destPortal != null)
            {
                destPortal.StartCoroutine(destPortal.DisableTeleportForMoment());
            }
        }
    }

    private IEnumerator DisableTeleportForMoment()
    {
        canTeleport = false;
        yield return new WaitForSeconds(2f); // Prevent immediate re-entry loop
        canTeleport = true;
    }
}
