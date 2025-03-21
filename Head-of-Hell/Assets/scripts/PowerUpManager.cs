using System.Collections;
using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
    public enum PowerUpType { SpeedBoost, DamageBoost, RefreshCD, Heal , Toxic}
    public PowerUpType type;
    AudioManager audioManager;

    void Start()
    {
        audioManager = FindObjectOfType<AudioManager>();
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Character player = other.GetComponent<Character>();

            if (player != null)
            {
                if(type== PowerUpType.Toxic)
                {
                    audioManager.PlaySFX(audioManager.poison, audioManager.normalVol);
                }
                else
                {
                    audioManager.PlaySFX(audioManager.powerup, audioManager.normalVol);
                }
                ApplyPowerUp(player);
                Destroy(gameObject); // Destroy the power-up after it's collected
            }
        }
    }

    void ApplyPowerUp(Character player)
    {
        switch (type)
        {
            case PowerUpType.SpeedBoost:
                player.SpeedBoost();
                break;
            case PowerUpType.DamageBoost:
                player.DamageShield();
                break;
            case PowerUpType.RefreshCD:
                player.RefreshCD();
                break;
            case PowerUpType.Heal:
                player.HealUp();
                break;
            case PowerUpType.Toxic:
                player.TakeDamage(5,false);
                break;
        }
    }

    private IEnumerator LifeTime()
    {
        yield return new WaitForSeconds(20f);

        Destroy(gameObject);
    }
}
