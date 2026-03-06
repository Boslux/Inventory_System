using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnim : MonoBehaviour
{
    public static PlayerAnim instance;

    [SerializeField] private bool useAttackRootMotion = false;

    private Animator anim;
    private Rigidbody rb;

    private void Awake()
    {
        instance = this;
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        // Keep motion ownership in Mover by default.
        anim.applyRootMotion = false;
    }

    public void Walking(bool isWalking)
    {
        anim.SetBool("isWalking", isWalking);
    }

    public void Attack()
    {
        anim.applyRootMotion = useAttackRootMotion;
        anim.SetTrigger("attack");
    }

    public void EndAttack()
    {
        anim.applyRootMotion = false;
    }

    public bool IsRootMotionDrivingMotion()
    {
        return useAttackRootMotion && anim.applyRootMotion;
    }

    private void OnAnimatorMove()
    {
        if (!useAttackRootMotion || !anim.applyRootMotion || rb == null)
        {
            return;
        }

        // Ignore vertical root motion to avoid "flying" with kinematic movement.
        Vector3 delta = anim.deltaPosition;
        delta.y = 0f;
        rb.MovePosition(rb.position + delta);
        rb.MoveRotation(rb.rotation * anim.deltaRotation);
    }

    internal void Dodge()
    {
        anim.SetTrigger("dodge");
    }

}
