using System.Collections;
using UnityEngine;

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
    public int whipDamage, robberyCounter;
    public Character enemy, stolenCharacter;
    public int currentHealth;
    public helthbarscript healthbar;
    public int maxHealth;
    bool swapped;
    bool healthswap = true;

    // --- Injected input provider (same as the Character uses) ---
    private IInputProvider input;
    public void SetInput(IInputProvider provider) => input = provider;

    public void Start()
    {
        animator = GetComponent<Animator>();
        // Fallback to keyboard if someone forgot to set us:
        if (input == null) input = new KeyboardInputProvider();
    }

    void Update()
    {
        if (!healthswap) // set stolen char health once we’ve swapped
        {
            stolenCharacter.SetCurrentHealth(currentHealth);
            healthswap = true;
        }

        // If we swapped forms and the form dies, return to Lupen and die.
        if (swapped)
        {
            if (stolenCharacter.GetCurrentHealth() <= 0)
            {
                lupen.enabled = true;
                lupenInFormSpell = false;
                swapped = false;
                currentHealth = stolenCharacter.GetCurrentHealth();
                lupen.ReturnToLupen(whipDamage, robberyCounter, currentHealth);
                lupen.Die();
            }
        }

        // If Lupen is currently in-form spell and the animation finished, return.
        if (lupenInFormSpell && !animator.GetBool("Casting"))
        {
            lupen.enabled = true;
            lupenInFormSpell = false;
            swapped = false;
            currentHealth = stolenCharacter.GetCurrentHealth();
            lupen.ReturnToLupen(whipDamage, robberyCounter, currentHealth);
        }

        // *** INPUT: use provider instead of Input. ***
        // Ability to trigger return while in stolen form:
        bool abilityPressed =
            input.GetKeyDown(ability) ||
            (controller && input.GetButtonDown("Spell" + playerString));

        if (abilityPressed && lupen.isActiveAndEnabled == false && !enemy.AmICasting())
        {
            stolenCharacter.chargeDisable = true;
            StartCoroutine(SetLupenInFormSpellAfterDelay(1f));
        }
    }

    private IEnumerator SetLupenInFormSpellAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        lupenInFormSpell = true;
        Debug.Log("LupenInFormSpell is now true");
    }

    public void Action()
    {
        lupen.enabled = false;
        swapped = true;
        healthswap = false;

        // Ensure the *stolen form* receives the SAME input provider,
        // so the Agent/keyboard keeps controlling seamlessly.
        if (stolenCharacter != null)
        {
            stolenCharacter.SetInput(input);
        }
    }
}
