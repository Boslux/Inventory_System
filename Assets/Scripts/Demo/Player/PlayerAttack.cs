using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private float range = 2f;
    [SerializeField] private int damage = 5;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private Transform attackOrigin; // boşsa transform kullanır

    [SerializeField] private bool canAttack = true;
    [SerializeField] private float cooldownTimer;

    Inputs input;
    void Awake()
    {
        input = new Inputs();
        input.Player.Enable();

        input.Player.Attack.performed += Attack;
    }

    private void Attack(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (!canAttack) return;

        PlayerAnim.instance.Attack();
        StartCoroutine(CanAttackControl());

        Vector3 origin = attackOrigin != null ? attackOrigin.position : transform.position;

        Collider[] hits = Physics.OverlapSphere(origin, range, enemyLayer);
        if (hits == null || hits.Length == 0) return;


        Collider best = hits[0];
        float bestDist = Vector3.SqrMagnitude(best.transform.position - origin);

        for (int i = 1; i < hits.Length; i++)
        {
            float d = Vector3.SqrMagnitude(hits[i].transform.position - origin);
            if (d < bestDist)
            {
                bestDist = d;
                best = hits[i];
            }
        }

        IDamageable damageable = best.GetComponentInParent<IDamageable>();
        if (damageable != null)
            damageable.TakeDamage(damage);
    }
    private IEnumerator CanAttackControl()
    {
        canAttack = false;
        yield return new WaitForSeconds(cooldownTimer);
        canAttack = true;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 origin = attackOrigin != null ? attackOrigin.position : transform.position;
        Gizmos.DrawWireSphere(origin, range);
    }
}
