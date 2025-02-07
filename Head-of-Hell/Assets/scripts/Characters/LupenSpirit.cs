using System.Collections;
using Unity.VisualScripting;
//using UnityEditor.PackageManager;
//using UnityEditor.Playables;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.UIElements.Experimental;

public class LupenSpirit : MonoBehaviour
{
    public Lupen lupen;
    public KeyCode ability;
    public string playerString;
    public bool controller;
    bool lupenInFormSpell = false;
    public CharacterManager characterChoiceHandler;
    public Animator animator;
    public CharacterAnimationEvents cEvents;
    public int whipDamage,robberyCounter;
    public Character enemy,stolenCharacter;
    public int currentHealth;
    public helthbarscript healthbar;
    public int maxHealth;
    bool swapped;

    public void Start()
    {
        animator = GetComponent<Animator>();
    }


    void Update()
    {
        if (swapped)
        {
            stolenCharacter.SetCurrentHealth(currentHealth);
            swapped = false;
        }
        
        if (lupenInFormSpell && !animator.GetBool("Casting"))
        {
            lupen.enabled=true;
            lupenInFormSpell = false;
            currentHealth=stolenCharacter.GetCurrentHealth();
            lupen.ReturnToLupen(whipDamage,robberyCounter,currentHealth);
        }

        if ((Input.GetKeyDown(ability) || (controller && Input.GetButtonDown("Spell" + playerString)))&& lupen.isActiveAndEnabled==false)
        {
            StartCoroutine(SetLupenInFormSpellAfterDelay(1f));
        }
        return;
    }

    private IEnumerator SetLupenInFormSpellAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay); // Wait for the specified delay
        lupenInFormSpell = true; // Set the variable to true
        print("LupenInFormSpell is now true");
    }

    public void Action()
    {
        lupen.enabled=false;

        print(stolenCharacter);

        swapped = true;

        //stolenCharacter.TakeDamageNoAnimation(maxHealth - currentHealth, false);
    }
}