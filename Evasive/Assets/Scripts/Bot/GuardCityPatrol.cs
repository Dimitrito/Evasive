using UnityEngine;
using UnityEngine.AI;

public class GuardCityPatrol : MonoBehaviour
{
    public float patrolRadius = 20f;   
    public float waitTime = 2f;        
    public float minDistance = 2f;    

    private NavMeshAgent agent;
    private float waitTimer;
    private Vector3 startPosition;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        startPosition = transform.position;
        GoToRandomPoint();
    }

    void Update()
    {
        if (!agent.isOnNavMesh) return;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            waitTimer += Time.deltaTime;

            if (waitTimer >= waitTime)
            {
                GoToRandomPoint();
                waitTimer = 0f;
            }
        }
    }

    void GoToRandomPoint()
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
                GoToRandomPoint(); 
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(Application.isPlaying ? startPosition : transform.position, patrolRadius);
    }
}