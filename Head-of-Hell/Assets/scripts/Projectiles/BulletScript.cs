using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    private bool hasHit = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasHit)
        {
            hasHit = true;
            Destroy(gameObject);
            Debug.Log("BDestroyed");
            Character character = other.GetComponent<Character>();
            LazyBigus enemy;
            if (character != null)
            {
                character.TakeDamage(3, true);
                enemy = (LazyBigus)character.GetEnemy();
                enemy.StackPoison();
            }
        }
    }
}
