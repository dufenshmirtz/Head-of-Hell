using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class platformbehaviour : MonoBehaviour
{

    int playersonplatform = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /*

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Collider2D[] colliders = GetComponents<Collider2D>();

            colliders[1].enabled = true;

            playersonplatform++;
        }

    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {

            playersonplatform--;

            if(playersonplatform == 0)
            {
                Collider2D[] colliders = GetComponents<Collider2D>();

                colliders[1].enabled = false;
            }
            
        }
    }
    */
}
