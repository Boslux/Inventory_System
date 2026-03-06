using UnityEngine;
using UnityEngine.AI;



public class EnemyMover : MonoBehaviour
{
    private enum AiState
    {
        Patrol,
        Follow,
        Attack
    }

    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private string targetTag = "Player";

    [Header("Ranges")]
    [SerializeField] private float followRange = 8f;
    [SerializeField] private float attackRange = 1.8f;

    [Header("Attack")]
    [SerializeField] private float attackCooldown = 1.1f;
    [SerializeField] private int attackDamage = 2;

    [Header("Patrol")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolWaitTime = 0.8f;
    [SerializeField] private float randomPatrolRadius = 6f;
    [SerializeField] private float randomPatrolSampleDistance = 2f;

    [Header("Performance")]
    [SerializeField] private float repathInterval = 0.2f;

    private NavMeshAgent agent;
    private Enemy enemy;
    private EnemyAnim enemyAnim;

    private AiState state;
    private Vector3 spawnPoint;
    private int patrolIndex = -1;
    private bool hasPatrolDestination;
    private float waitTimer;
    private float nextRepathTime;
    private float nextAttackTime;
    private float nextTargetSearchTime;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        enemy = GetComponent<Enemy>();
        enemyAnim = GetComponent<EnemyAnim>();
        spawnPoint = transform.position;

        if (target == null)
            TryFindTarget();
    }

    private void OnEnable()
    {
        state = AiState.Patrol;
        hasPatrolDestination = false;
        waitTimer = 0f;
        nextRepathTime = 0f;
        nextAttackTime = 0f;
    }

    private void Update()
    {
        if (agent == null || enemy == null || enemyAnim == null) return;
        if (!agent.isOnNavMesh) return;

        if (!enemy.IsAlive)
        {
            StopAgent();
            enemyAnim.Walking(0f);
            return;
        }

        if (enemy.IsInHitStun)
        {
            StopAgent();
            enemyAnim.Walking(0f);
            return;
        }

        if (target == null && Time.time >= nextTargetSearchTime)
        {
            nextTargetSearchTime = Time.time + 1f;
            TryFindTarget();
        }

        float sqrDistanceToTarget = float.PositiveInfinity;
        if (target != null)
            sqrDistanceToTarget = (target.position - transform.position).sqrMagnitude;

        float followRangeSqr = followRange * followRange;
        float attackRangeSqr = attackRange * attackRange;

        if (target != null && sqrDistanceToTarget <= followRangeSqr)
        {
            if (sqrDistanceToTarget <= attackRangeSqr)
                UpdateAttack();
            else
                UpdateFollow();
        }
        else
        {
            UpdatePatrol();
        }

        enemyAnim.Walking(agent.isStopped ? 0f : agent.velocity.magnitude);
    }

    private void UpdateFollow()
    {
        state = AiState.Follow;
        waitTimer = 0f;
        hasPatrolDestination = false;
        agent.isStopped = false;
        agent.stoppingDistance = attackRange * 0.9f;

        if (target == null) return;
        if (Time.time < nextRepathTime) return;

        nextRepathTime = Time.time + repathInterval;
        agent.SetDestination(target.position);
    }

    private void UpdateAttack()
    {
        state = AiState.Attack;
        StopAgent();
        FaceTarget();

        if (Time.time < nextAttackTime) return;

        nextAttackTime = Time.time + attackCooldown;
        enemyAnim.Attack();

        if (target != null && target.TryGetComponent(out IDamageable damageable))
            damageable.TakeDamage(attackDamage);
    }

    private void UpdatePatrol()
    {
        state = AiState.Patrol;
        agent.stoppingDistance = 0f;

        if (!hasPatrolDestination)
            SetNextPatrolDestination();

        if (!hasPatrolDestination)
        {
            StopAgent();
            return;
        }

        agent.isStopped = false;

        if (agent.pathPending) return;

        bool reached = agent.remainingDistance <= 0.2f;
        if (!reached) return;

        waitTimer += Time.deltaTime;
        if (waitTimer < patrolWaitTime) return;

        waitTimer = 0f;
        SetNextPatrolDestination();
    }

    private void SetNextPatrolDestination()
    {
        if (!agent.isOnNavMesh) return;

        Vector3 destination;

        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
            destination = patrolPoints[patrolIndex].position;
        }
        else
        {
            if (!TryGetRandomPatrolPoint(out destination))
            {
                hasPatrolDestination = false;
                return;
            }
        }

        hasPatrolDestination = true;
        agent.SetDestination(destination);
    }

    private bool TryGetRandomPatrolPoint(out Vector3 destination)
    {
        for (int i = 0; i < 5; i++)
        {
            Vector3 randomOffset = Random.insideUnitSphere * randomPatrolRadius;
            randomOffset.y = 0f;
            Vector3 candidate = spawnPoint + randomOffset;

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, randomPatrolSampleDistance, NavMesh.AllAreas))
            {
                destination = hit.position;
                return true;
            }
        }

        destination = spawnPoint;
        return false;
    }

    private void FaceTarget()
    {
        if (target == null) return;

        Vector3 lookDirection = target.position - transform.position;
        lookDirection.y = 0f;
        if (lookDirection.sqrMagnitude < 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 14f * Time.deltaTime);
    }

    private void StopAgent()
    {
        if (agent.isStopped) return;

        agent.isStopped = true;
        if (agent.hasPath)
            agent.ResetPath();
    }

    private void TryFindTarget()
    {
        GameObject go = GameObject.FindGameObjectWithTag(targetTag);
        if (go != null) target = go.transform;
    }
}
