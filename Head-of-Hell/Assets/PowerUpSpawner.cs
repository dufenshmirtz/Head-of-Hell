using System.Collections;
using UnityEngine;

public class PowerUpSpawner : MonoBehaviour
{
    public GameObject[] powerUpPrefabs; // List of power-up prefabs
    public float spawnInterval = 10f; // Time between spawns
    public LayerMask platformMask; // LayerMask for platforms to avoid overlap
    public float checkRadius = 1f; // Radius for overlap check

    private float timer;

    AudioManager audioManager;

    void Start()
    {
        audioManager = FindObjectOfType<AudioManager>();
    }
    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnPowerUp();
            timer = 0f;
        }
    }

    void SpawnPowerUp()
    {
        Vector3 spawnPosition = GetValidSpawnPosition();

        if (spawnPosition != Vector3.zero)
        {
            int randomIndex = Random.Range(0, powerUpPrefabs.Length);
            Instantiate(powerUpPrefabs[randomIndex], spawnPosition, Quaternion.identity);
            audioManager.PlaySFX(audioManager.powerupSpawned, audioManager.lessVol);
        }
    }

    Vector3 GetValidSpawnPosition()
    {
        Vector3 spawnPosition = Vector3.zero;

        // Define the bounds of the stage to find random positions within
        float xPos = Random.Range(-9f, 9f); // Example bounds
        float yPos = Random.Range(-3f, 5f);    // Ensure yPos is above the ground

        spawnPosition = new Vector3(xPos, yPos, 0);

        // Check if this position overlaps with any platforms
        Collider2D[] colliders = Physics2D.OverlapCircleAll(spawnPosition, checkRadius, platformMask);

        if (colliders.Length == 0) // If there is no overlap
        {
            return spawnPosition;
        }

        return Vector3.zero; // If the position is invalid, return zero
    }
}
