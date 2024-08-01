using UnityEngine;

public abstract class Character : MonoBehaviour
{
    protected PlayerScript player, enemy;
    protected Animator animator;
    protected AudioManager audioManager;
    protected Rigidbody2D rb;
    protected CharacterResources resources;

    public void InitializeCharacter(PlayerScript playa, AudioManager audio, CharacterResources res)
    {
        player = playa;
        animator = player.animator;
        audioManager = audio;
        enemy = player.enemy;
        rb=player.rb;
        resources = res;
    }

    public virtual void HeavyAttack() { }

    public virtual void DealHeavyDamage() { }

    public virtual void Spell() { }

    public virtual void LightAttack() { }

    public void Block()
    {
        animator.SetTrigger("critsi");
        animator.SetBool("Crouch", true);
        player.PlayerBlock(true);

        ResetQuickPunch();
    }

    public void Unblock()
    {
        animator.SetBool("cWalk", false);
        animator.SetBool("Crouch", false);
        player.PlayerBlock(false);

        ResetQuickPunch();
    }

    public void Jump()
    {
        player.rb.velocity = new Vector2(player.rb.velocity.x, player.jumpForce);
        animator.SetTrigger("Jump");
        audioManager.PlaySFX(audioManager.jump, audioManager.jumpVolume);

        ResetQuickPunch();
    }

    public Collider2D HitEnemy()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(player.attackPoint.position, player.attackRange, player.enemyLayer);
        return hitEnemy;
    }

    public bool IsEnemyClose()
    {
        return Vector3.Distance(this.transform.position, enemy.transform.position) <= 3f;
    }

    #region Special Functions
    //Rager
    public void ResetQuickPunch()
    {
        animator.ResetTrigger("punch2");
        player.animator.SetBool("QuickPunch", false);
    }

    public void Grabbed()
    {
        audioManager.PlaySFX(audioManager.grab, audioManager.heavyAttackVolume);
        animator.SetTrigger("grabbed");
    }

    #endregion
}
