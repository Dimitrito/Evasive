using UnityEngine;
using UnityEngine.AI;

public class GuardAI : MonoBehaviour
{
    public enum State { Patrol, Chase, Investigate, Return }

    [Header("Patrol")]
    public float patrolRadius = 20f;
    public float waitTime = 2f;
    public float minDistance = 2f;

    [Header("Vision")]
    public Transform eyes;
    public float viewDistance = 12f;
    [Range(0, 360)] public float viewAngle = 90f;
    public LayerMask obstacleMask;
    public string playerTag = "Player";

    [Header("Combat")]
    public float attackRange = 5f;           
    public float fireRate = 1f;              
    public GameObject bulletPrefab;          
    public Transform firePoint;             

    [Header("Investigate")]
    public float investigateTime = 5f;
    public float searchRadius = 5f;

    private NavMeshAgent agent;
    private Transform player;
    private Vector3 startPosition;
    private Vector3 lastKnownPosition;
    private float waitTimer;
    private float investigateTimer;
    private float fireTimer;
    private State state = State.Patrol;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag(playerTag)?.transform;
        startPosition = transform.position;

        GoToRandomPatrolPoint();
    }

    void Update()
    {
        switch (state)
        {
            case State.Patrol:
                Patrol();
                LookForPlayer();
                break;

            case State.Chase:
                Chase();
                break;

            case State.Investigate:
                Investigate();
                break;

            case State.Return:
                ReturnToPatrol();
                break;
        }
    }

    // ----------- Patrol -----------
    void Patrol()
    {
        if (!agent.isOnNavMesh) return;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTime)
            {
                GoToRandomPatrolPoint();
                waitTimer = 0f;
            }
        }
    }

    void GoToRandomPatrolPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += startPosition;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
        {
            if (Vector3.Distance(transform.position, hit.position) >= minDistance)
            {
                agent.SetDestination(hit.position);
            }
            else
            {
                GoToRandomPatrolPoint();
            }
        }
    }

    // ----------- Vision -----------
    void LookForPlayer()
    {
        if (player == null) return;

        Vector3 dirToPlayer = (player.position - eyes.position).normalized;

        if (Vector3.Distance(eyes.position, player.position) <= viewDistance)
        {
            if (Vector3.Angle(eyes.forward, dirToPlayer) < viewAngle / 2f)
            {
                if (Physics.Raycast(eyes.position, dirToPlayer, out RaycastHit hit, viewDistance, ~0))
                {
                    if (hit.collider.CompareTag(playerTag))
                    {
                        lastKnownPosition = player.position;
                        state = State.Chase;
                        agent.SetDestination(player.position);
                    }
                }
            }
        }
    }

    // ----------- Chase -----------
    void Chase()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > viewDistance || !IsPlayerVisible())
        {
            lastKnownPosition = player.position;
            state = State.Investigate;
            agent.SetDestination(lastKnownPosition);
            investigateTimer = 0f;
            return;
        }

        if (distance > attackRange)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
        else
        {
            agent.isStopped = true;
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
            Shoot();
        }
    }

    bool IsPlayerVisible()
    {
        Vector3 dirToPlayer = (player.position - eyes.position).normalized;
        if (Vector3.Angle(eyes.forward, dirToPlayer) < viewAngle / 2f)
        {
            if (Physics.Raycast(eyes.position, dirToPlayer, out RaycastHit hit, viewDistance, ~0))
            {
                return hit.collider.CompareTag(playerTag);
            }
        }
        return false;
    }

    // ----------- Shooting -----------
    void Shoot()
    {
        fireTimer += Time.deltaTime;
        if (fireTimer >= 1f / fireRate)
        {
            fireTimer = 0f;

            if (bulletPrefab != null && firePoint != null)
            {
                Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            }
            else
            {
                Debug.Log("Выстрел по игроку!");
            }
        }
    }

    // ----------- Investigate -----------
    void Investigate()
    {
        investigateTimer += Time.deltaTime;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (investigateTimer < investigateTime)
            {
                Vector3 randomDirection = Random.insideUnitSphere * searchRadius;
                randomDirection += lastKnownPosition;

                if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, searchRadius, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                }
            }
            else
            {
                state = State.Return;
                agent.SetDestination(startPosition);
            }
        }
    }

    // ----------- Return -----------
    void ReturnToPatrol()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            state = State.Patrol;
            GoToRandomPatrolPoint();
        }
    }

    // ----------- Debug -----------
    void OnDrawGizmosSelected()
    {
        if (eyes == null) eyes = transform;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(eyes.position, viewDistance);

        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2, 0) * eyes.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2, 0) * eyes.forward;

        Gizmos.DrawLine(eyes.position, eyes.position + leftBoundary * viewDistance);
        Gizmos.DrawLine(eyes.position, eyes.position + rightBoundary * viewDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(lastKnownPosition, 0.3f);
    }
}