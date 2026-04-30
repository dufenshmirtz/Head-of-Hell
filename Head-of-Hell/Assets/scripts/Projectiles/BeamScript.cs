using System.Collections;
using UnityEngine;

public class BeamScript : MonoBehaviour
{
    public LazyBigus playa;

    private Collider2D beamCollider;
    private bool hasHit;

    private void Awake()
    {
        beamCollider = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        hasHit = false;

        if (beamCollider != null)
        {
            beamCollider.enabled = false;
        }

        StartCoroutine(BeamLifetime(1f));
    }

    public void ActivateCollider()
    {
        if (beamCollider != null)
        {
            beamCollider.enabled = true;

            // Προαιρετικό: άμεσο check για κάποιον που είναι ήδη μέσα
            CheckOverlappingPlayers();
        }
    }

    public void DeactivateCollider()
    {
        if (beamCollider != null)
        {
            beamCollider.enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        TryHit(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        TryHit(collision);
    }

    private void TryHit(Collider2D collision)
    {
        if (hasHit) return;

        if (collision.CompareTag("Player"))
        {
            Character target = collision.GetComponent<Character>();
            if (target != null && target != playa)
            {
                playa.ChangeEnemy(target);
            }
            hasHit = true;
            playa.BeamHit();
        }
    }

    private void CheckOverlappingPlayers()
    {
        if (beamCollider == null) return;

        Collider2D[] hits = Physics2D.OverlapBoxAll(
            beamCollider.bounds.center,
            beamCollider.bounds.size,
            0f
        );

        foreach (Collider2D hit in hits)
        {
            if (hit != beamCollider && hit.CompareTag("Player"))
            {
                Character target = hit.GetComponent<Character>();
                if (target != null && target != playa)
                {
                    playa.ChangeEnemy(target);
                }
                hasHit = true;
                playa.BeamHit();
                break;
            }
        }
    }

    private IEnumerator BeamLifetime(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }
}
