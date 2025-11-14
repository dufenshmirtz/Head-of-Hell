// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class FartManager : MonoBehaviour
// {
//     public float duration; // Time before the fart disappears
//     public int poisonDamage = 1; // Damage per second
//     public Visvia owner; // The character who spawned the fart

//     private List<Character> enemiesInGas = new List<Character>();

//     void Start()
//     {
//         StartCoroutine(DestroyAfterTime());
//     }


//     void OnTriggerEnter2D(Collider2D other)
//     {
//         Character enemy = other.GetComponent<Character>();

//         // Ignore the owner (the character who spawned the fart)
//         if (enemy != null && enemy != owner)
//         {
//             enemiesInGas.Add(enemy);

//             if (owner.farted == 0)
//             {
//                 owner.FartDamage();
//             }
//             owner.farted++;
//         }
        
//     }

//     void OnTriggerExit2D(Collider2D other)
//     {
//         Character enemy = other.GetComponent<Character>();
//         if (enemy != null && enemy != owner)
//         {
//             owner.farted--;

//             enemiesInGas.Remove(enemy);
//             if (owner.farted == 0)
//             {
//                 owner.FartEscape();
//             }
//         }
//     }

//     IEnumerator DestroyAfterTime()
//     {
//         yield return new WaitForSeconds(duration);
//         Destroy(gameObject);
//     }

//     public void Explode()
//     {
//         StartCoroutine(ExplosionEffect());
//     }

//     IEnumerator ExplosionEffect()
//     {
//         SpriteRenderer sr = GetComponent<SpriteRenderer>();
//         if (sr != null)
//         {
//             sr.color = Color.yellow; // Change to red before explosion
//         }             

//         yield return new WaitForSeconds(0.1f); // Wait a bit before dealing damage

//         if (sr != null)
//         {
//             sr.color = Color.red; // Change to red before explosion
//         }

//         yield return new WaitForSeconds(0.1f); // Wait a bit before dealing damage

//         foreach (Character enemy in enemiesInGas)
//         {
//             owner.FartExploded(); // Explosion deals 10 damage
//         }

//         Destroy(gameObject); // Destroy after explosion
//     }
// }
