using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tempscriptenemy : MonoBehaviour
{
    public int maxHealth=100;
    public int currHealth;
    // Start is called before the first frame update
    void Start()
    {
        currHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage(int dmg)
    {
        currHealth -= dmg;

        if(currHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("nigga ded");
    }
}
