using System.Collections;
using DG.Tweening;
using UnityEngine.AI;
using UnityEngine;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyAnim))]
[RequireComponent(typeof(LootSpawner))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(EnemyMover))]
public class Enemy : MonoBehaviour, IDamageable
{
    [SerializeField] private int startingHealth = 10;
    [SerializeField] private float hitStunDuration = 0.5f;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private LootSpawner lootSpawner;
    [SerializeField] private EnemyAnim enemyAnim;
    [SerializeField] private float deathAnimLeadTime = 0.35f;
    [SerializeField] private float deathShrinkDuration = 0.9f;
    [SerializeField] private Ease deathShrinkEase = Ease.InOutSine;
    private EnemyHealth health;

    private bool isAlive = true;
    private bool isInHitStun = false;
    private Coroutine hitStunRoutine;
    private Tween deathTween;

    public bool IsAlive => isAlive;
    public bool IsInHitStun => isInHitStun;

    private void Reset()
    {
        rb = GetComponent<Rigidbody>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        lootSpawner = GetComponent<LootSpawner>();
        enemyAnim = GetComponent<EnemyAnim>();
    }

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (navMeshAgent == null) navMeshAgent = GetComponent<NavMeshAgent>();
        if (enemyAnim == null) enemyAnim = GetComponent<EnemyAnim>();
        if (lootSpawner == null) lootSpawner = GetComponent<LootSpawner>();
        if (health == null) health = new EnemyHealth(startingHealth);

        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }
    }

    private void OnEnable()
    {
        if (health == null) health = new EnemyHealth(startingHealth);

        health.OnDied += HandleDied;
        health.OnHit += HandleHit;
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnDied -= HandleDied;
            health.OnHit -= HandleHit;
        }

        if (deathTween != null && deathTween.IsActive())
        {
            deathTween.Kill();
            deathTween = null;
        }
    }

    public void TakeDamage(int damage)
    {
        if (health == null || !isAlive) return;
        health.TakeDamage(damage);
    }

    private void HandleHit()
    {
        if (!isAlive) return;

        if (enemyAnim != null)
            enemyAnim.Hurt();

        if (hitStunRoutine != null)
            StopCoroutine(hitStunRoutine);

        isInHitStun = true;
        hitStunRoutine = StartCoroutine(EndStun());
    }

    private void HandleDied()
    {
        if (!isAlive) return;

        if (enemyAnim != null)
            enemyAnim.Die();

        if (lootSpawner != null)
            lootSpawner.SpawnDrops(transform.position);

        isAlive = false;

        if (navMeshAgent != null && navMeshAgent.enabled)
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.ResetPath();
            navMeshAgent.enabled = false;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        if (deathTween != null && deathTween.IsActive())
            deathTween.Kill();

        Vector3 baseScale = transform.localScale;
        deathTween = DOTween.Sequence()
            .AppendInterval(deathAnimLeadTime)
            .Append(transform.DOScaleY(0f, deathShrinkDuration).SetEase(deathShrinkEase))
            .OnUpdate(() =>
            {
                Vector3 s = transform.localScale;
                s.x = baseScale.x;
                s.z = baseScale.z;
                transform.localScale = s;
            })
            .OnComplete(() =>
            {
                Destroy(gameObject);
            });
    }

    private IEnumerator EndStun()
    {
        yield return new WaitForSeconds(hitStunDuration);
        isInHitStun = false;
        hitStunRoutine = null;
    }

}
