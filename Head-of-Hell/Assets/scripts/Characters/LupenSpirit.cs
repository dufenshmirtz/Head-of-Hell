using System.Collections;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEditor.Playables;
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

    public void Start()
    {
        
    }


    void Update()
    {
        animator = GetComponent<Animator>();

        if (lupenInFormSpell && !animator.GetBool("Casting"))
        {
            lupen.enabled=true;
            lupenInFormSpell = false;
            lupen.ReturnToLupen();
        }

        if ((Input.GetKeyDown(ability) || (controller && controller && Input.GetButtonDown("Spell" + playerString)))&& lupen.isActiveAndEnabled==false)
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
    }
}