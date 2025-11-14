using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PowerUpSpawner : MonoBehaviour
{
    public GameObject[] powerUpPrefabs; // List of power-up prefabs
    public float spawnInterval = 10f; // Time between spawns
    public LayerMask platformMask; // LayerMask for platforms to avoid overlap
    public float checkRadius = 1f; // Radius for overlap check
    bool chanChan;

    private float timer;

    AudioManager audioManager;

    public GameManager gameMngr;

    void Start()
    {

        audioManager = FindObjectOfType<AudioManager>();

        string json = PlayerPrefs.GetString("SelectedRuleset", null);

        if (!string.IsNullOrEmpty(json))
        {
            // Convert the JSON string back to a CustomRuleset object
            CustomRuleset loadedRuleset = JsonUtility.FromJson<CustomRuleset>(json);

            chanChan = loadedRuleset.chanChan;

            if (chanChan)
            {
                this.gameObject.SetActive(Random.value > 0.5f);  //50% chance
            }
            else 
            {
                if (!loadedRuleset.powerupsEnabled && !chanChan)
                {
                    this.gameObject.SetActive(false);
                }
            }

            
        }
        else
        {
            Debug.LogWarning("No ruleset found in PlayerPrefs.");
        }
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

        if (spawnPosition != Vector3.zero && CanSpawnMorePowerUps())
        {
            int randomIndex = Random.Range(0, powerUpPrefabs.Length);
            GameObject parentObject = GameObject.Find("PowerUps");

            // Instantiate the power-up as a child of "Powerups"
            Instantiate(powerUpPrefabs[randomIndex], spawnPosition, Quaternion.identity, parentObject.transform);

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

    bool CanSpawnMorePowerUps()
    {
        // Find the parent object "Powerups"
        GameObject parentObject = GameObject.Find("PowerUps");

        if (parentObject != null)
        {
            // Count the number of child objects under "Powerups"
            int childCount = parentObject.transform.childCount;

            // Return true if there are less than 4 children
            return childCount < 4;
        }

        // If the "Powerups" object doesn't exist, return false or handle accordingly
        return false;
    }
}
