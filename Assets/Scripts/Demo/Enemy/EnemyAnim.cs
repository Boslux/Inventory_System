using UnityEngine;
using DG.Tweening;

public class EnemyAnim : MonoBehaviour
{
    [SerializeField] private Animator anim;

    void Awake()
    {
        if (anim == null) anim = GetComponent<Animator>();
    }

    public void Hurt()
    {
        if (anim == null) return;
        anim.SetTrigger("hurt");
    }

    public void Walking(float speed)
    {
        if (anim == null) return;
        anim.SetFloat("speed", speed);
    }

    public void Die()
    {
        if (anim == null) return;
        anim.SetBool("isAlive", false);
        
    }

    public void Attack()
    {
        if (anim == null) return;
        anim.SetTrigger("attack");
    }
}
