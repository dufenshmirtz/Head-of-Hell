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

    //General
    public void SetCharacter(Character characterio)
    {
        character = characterio;
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
    public void BigusKnockEvent()
    {
        bigus = (LazyBigus)character;
        bigus.BigusKnock();
    }

    public void RejuvenationEvent()
    {
        bigus = (LazyBigus)character;
        bigus.Rejuvenation();
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
    public void ReActivateEvent()
    {
        steelager = (Steelager)character;
        steelager.ReActivate();
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

    //Lithra
    public void BellDamageEvent()
    {
        lithra = (Lithra)character;
        lithra.DealBellDmg();
    }

    //Lithra
    public void FireDamageEvent()
    {
        chiback = (Chiback)character;
        chiback.DealFireDamage();
    }
    //sth
}
