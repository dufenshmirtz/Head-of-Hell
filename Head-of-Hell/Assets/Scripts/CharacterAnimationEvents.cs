using UnityEngine;

public class CharacterAnimationEvents : MonoBehaviour
{
    Character character;
    Skipler skipler;
    Rager rager;
    Fin fin;
    Steelager steelager;
    LazyBigus bigus;
    Vander vander;
    Lithra lithra;
    Chiback chiback;
    Lupen lupen;

    //General
    void Start()
    {
        character = GetComponent<Character>();
    }

    public void DealDamageEvent()
    {
        character.DealHeavyDamage();
    }

    public void DealChargeDamageEvent()
    {
        character.DealChargeDmg();
    }

    public void StartHeavyAttackEvent(){
        character.HeavyAttackStart();
    }

    public void EndHeavyAttackEvent(){
        character.HeavyAttackEnd();
    }

    public void PermanentDeathEvent()
    {
        character.PermaDeath();
    }

    public void StayDynamicEvent()
    {
        character.stayDynamic();
        character.IgnoreMovement(false);
    }
    public void StayStaticEvent()
    {
        character.stayStatic();
    }
    //Vander
    public void StabDamageEvent()
    {
        vander = (Vander)character;
        vander.DealStabDmg();
    }
    public void KatanaDamage1Event()
    {
        vander = (Vander)character;
        vander.DealKatanaDmg1();
    }
    public void KatanaDamage2Event()
    {
        vander = (Vander)character;
        vander.DealKatanaDmg2();
    }
    //Rager

    public void ComboGrabEvent()
    {
        rager = (Rager)character;
        rager.DealComboDmg();
    }

    public void StartComboEvent()
    {
        rager = (Rager)character;
        rager.Startcombo();
    }
    public void FirstHitEvent()
    {
        rager = (Rager)character;
        rager.FirstHit();
    }
    public void SecondHitEvent()
    {
        rager = (Rager)character;
        rager.SecondHit();
    }
    public void ThirdHitEvent()
    {
        rager = (Rager)character;
        rager.ThirdHit();
    }
    public void QuickPunchDamageEvent()
    {
        rager = (Rager)character;
        rager.QuickPunchDamage();
    }
    public void StartQuickPunchEvent()
    {
        rager = (Rager)character;
        rager.QuickPunchStart();
    }
    //Bigus
    public void BeamEndEvent()
    {
        bigus = (LazyBigus)character;
        bigus.BeamEnd();
    }
    public void ShootEvent()
    {
        bigus = (LazyBigus)character;
        bigus.Shoot();
    }

    public void FirstFrameShootEvent()
    {
        bigus = (LazyBigus)character;
        bigus.firstShootFrame();
    }

    //Steelager
    public void ResetExplosionEvent()
    {
        steelager = (Steelager)character;
        steelager.ExplosionReset();
    }

    public void ExplosionDamageEvent()
    {
        steelager = (Steelager)character;
        steelager.DealExplosionDamage();
    }

    //Fin
    public void CounterOffEvent()
    {
        fin = (Fin)character;
        fin.CounterOff();
    }

    public void CounterSuccessEvent()
    {
        fin = (Fin)character;
        fin.CounterSuccessOff();
    }

    public void CounterDamageEvent()
    {
        fin = (Fin)character;
        fin.DealCounterDmg();
    }

    //Skipler
    public void SwordDashDamageEvent()
    {
        skipler = (Skipler)character;
        skipler.DealSwordDashDmg();
    }

    public void IdleGlitchEvent()
    {
        skipler = (Skipler)character;
        skipler.IdleGlitch();
    }

    //Lithra
    public void BellDamageEvent()
    {
        lithra = (Lithra)character;
        lithra.DealBellDmg();
    }

    public void SipEvent()
    {
        lithra = (Lithra)character;
        lithra.Sip();
    }

    //Chiback
    public void FireDamageEvent()
    {
        chiback = (Chiback)character;
        chiback.DealFireDamage();
    }

    //Lupen
    public void TransformationEvent()
    {
        lupen = (Lupen)character;
        lupen.Transformation();
    }

    public void DealWipDamageEvent()
    {
        lupen = (Lupen)character;
        lupen.DealWipDamage();
    }

    public void ChangeCharacterEvents(int mode)
    {
        // Get all Character components attached to this GameObject
        Character[] characters = GetComponents<Character>();

        if (characters.Length > 0)
        {
            if (mode == 1)
            {
                // Always pick the last Character component
                character = characters[characters.Length - 1];
            }
            else
            {
                character = characters[0];
            }
            print(character+" kolos");
        }
        else
        {
            Debug.LogWarning("No Character components found on this GameObject.");
        }
    }
}
