using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEngine;

public class bombScript : MonoBehaviour
{
    public Animator animator;
    int player;
    int enemy;
    bool exploded = false;
    bool dmgEnd=false;
    bool damageDealt = false;
    Character playa;
    Steelager steelager;
    AudioManager audioManager;
    bool jumpDone = false;

    // Start is called before the first frame update
    void Start()
    {       
        player = 0;
        audioManager = FindObjectOfType<AudioManager>(); // Find and assign the AudioManager

        if (audioManager == null)
        {
            Debug.LogError("AudioManager not found in the scene!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        int player1Layer = LayerMask.NameToLayer("Player1layer");
        int player2Layer = LayerMask.NameToLayer("Player2Layer");

        if ((player == 0))
        {
            if(other.gameObject.layer == player1Layer)
            {
                player = 1;
                enemy = player2Layer;
                playa = other.GetComponent<Character>();
            }
            if (other.gameObject.layer == player2Layer)
            {
                player = 2;
                enemy = player1Layer;
                playa = other.GetComponent<Character>();
            }
            
            return;
        }

        if (other.gameObject.layer == enemy && exploded && !dmgEnd && !damageDealt)
        {

            Character character = other.GetComponent<Character>();
            if (character != null)
            {
                character.TakeDamage(6, true);
                damageDealt = true;
            }
        }

        if (other.gameObject.layer == enemy && !exploded && !dmgEnd && !damageDealt)
        {
            
            Character character = other.GetComponent<Character>();
            if (character != null)
            {
                Explode();
                character.TakeDamage(6, true);
                damageDealt = true;
            }
        }

        if (other.gameObject.layer != enemy && exploded && !dmgEnd && !jumpDone)
        {

            Character[] characters = other.GetComponents<Character>();
            Character activeCharacter = null;

            foreach (Character charComponent in characters)
            {
                if (charComponent.isActiveAndEnabled)
                {
                    activeCharacter = charComponent;
                    break; // Stop after finding the first active component
                }
            }

            if (activeCharacter != null)
            {
                activeCharacter.Knockback(13f, 0.3333f, true);
                jumpDone = true;
            }
        }
    }

    public void Explode()
    {
        if(!exploded)
        {
            exploded = true;
            Collider2D[] colliders = GetComponents<Collider2D>();
            colliders[2].enabled = true;
            if (player != 0)
            {
                audioManager.BoomSound();
            }
            animator.SetTrigger("Explode");
 
        }
    }

    public void DestroyBomb()
    {
        Destroy(gameObject);
    }

    public void EndExplosion()
    {
        dmgEnd = true;
    }
}
