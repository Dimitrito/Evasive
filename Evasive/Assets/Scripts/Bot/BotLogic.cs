using UnityEngine;
using UnityEngine.AI;

public class BotLogic : MonoBehaviour
{
    [Header("Настройки поиска")]
    public float sightRange = 10f;          // Дистанция обнаружения
    public float attackRange = 5f;          // Дистанция атаки
    public float loseRange = 15f;           // Дистанция потери цели
    public float fieldOfView = 90f;         // Угол обзора (градусы)
    public LayerMask playerLayer;           // Слой игрока
    public LayerMask obstacleLayer;         // Слой препятствий

    [Header("Настройки атаки")]
    public float attackRate = 1f;           // Скорость атаки (выстрелов в секунду)
    public Transform firePoint;             // Точка выстрела
    public GameObject bulletPrefab;         // Префаб пули

    [Header("Настройки поиска")]
    public float searchDuration = 5f;       // Длительность поиска
    public float searchRadius = 8f;         // Радиус поиска

    private NavMeshAgent agent;
    private Transform player;
    private Vector3 startPosition;
    private bool playerInSight;
    private bool playerInAttackRange;
    private float nextAttackTime;
    private float searchTimer;
    private bool isSearching;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        startPosition = transform.position;

        if (player == null)
        {
            Debug.LogError("Игрок не найден! Убедитесь, что у игрока тег 'Player'");
        }
    }

    void Update()
    {
        // Проверяем дистанцию до игрока
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Проверяем видимость игрока (с учетом угла обзора)
        bool canSeePlayer = CheckPlayerVisibility();

        playerInSight = canSeePlayer && distanceToPlayer <= sightRange;
        playerInAttackRange = distanceToPlayer <= attackRange;

        if (!playerInSight && distanceToPlayer > loseRange)
        {
            // Игрок потерян
            if (isSearching)
            {
                SearchForPlayer();
            }
            else
            {
                ReturnToStart();
            }
        }
        else if (playerInSight && !playerInAttackRange)
        {
            // Преследование игрока
            ChasePlayer();
            isSearching = false;
        }
        else if (playerInSight && playerInAttackRange)
        {
            // Атака игрока
            AttackPlayer();
            isSearching = false;
        }
        else if (!playerInSight && distanceToPlayer <= loseRange)
        {
            // Начать поиск
            StartSearch();
        }
    }

    bool CheckPlayerVisibility()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        // Проверяем, находится ли игрок в поле зрения
        if (angleToPlayer > fieldOfView / 2f)
        {
            return false; // Игрок вне угла обзора
        }

        // Проверяем, есть ли прямая видимость (без препятствий)
        if (Physics.Raycast(transform.position, directionToPlayer, out RaycastHit hit, sightRange, ~obstacleLayer))
        {
            return hit.collider.CompareTag("Player");
        }

        return false;
    }

    void ChasePlayer()
    {
        agent.isStopped = false;
        agent.SetDestination(player.position);
    }

    void AttackPlayer()
    {
        // Останавливаемся для атаки
        agent.isStopped = true;

        // Поворачиваемся к игроку
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0; // Игнорируем разницу по высоте
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        // Стреляем с заданной скоростью
        if (Time.time >= nextAttackTime)
        {
            Shoot();
            nextAttackTime = Time.time + 1f / attackRate;
        }
    }

    void Shoot()
    {
        if (bulletPrefab != null && firePoint != null)
        {
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        }
        else
        {
            Debug.LogWarning("Не назначен bulletPrefab или firePoint!");
        }
    }

    void StartSearch()
    {
        if (!isSearching)
        {
            isSearching = true;
            searchTimer = 0f;
            SearchForPlayer();
        }
    }

    void SearchForPlayer()
    {
        searchTimer += Time.deltaTime;

        if (searchTimer >= searchDuration)
        {
            // Завершаем поиск и возвращаемся на старт
            isSearching = false;
            ReturnToStart();
            return;
        }

        // Ищем случайную точку для поиска
        if (!agent.hasPath || agent.remainingDistance < 0.5f)
        {
            Vector3 randomPoint = transform.position + Random.insideUnitSphere * searchRadius;
            randomPoint.y = transform.position.y;

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, searchRadius, NavMesh.AllAreas))
            {
                agent.isStopped = false;
                agent.SetDestination(hit.position);
            }
        }
    }

    void ReturnToStart()
    {
        agent.isStopped = false;
        agent.SetDestination(startPosition);

        // Если вернулись на стартовую позицию, останавливаемся
        if (Vector3.Distance(transform.position, startPosition) < 1f)
        {
            agent.isStopped = true;
        }
    }

    // Визуализация в редакторе
    void OnDrawGizmosSelected()
    {
        // Дистанция обнаружения
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        // Дистанция атаки
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Дистанция потери
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, loseRange);

        // Поле зрения
        Gizmos.color = Color.green;
        DrawFieldOfView();

        // Стартовая позиция
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(startPosition, 0.5f);
        }
    }

    void DrawFieldOfView()
    {
        float halfFOV = fieldOfView / 2f;
        Quaternion leftRayRotation = Quaternion.AngleAxis(-halfFOV, Vector3.up);
        Quaternion rightRayRotation = Quaternion.AngleAxis(halfFOV, Vector3.up);

        Vector3 leftRayDirection = leftRayRotation * transform.forward;
        Vector3 rightRayDirection = rightRayRotation * transform.forward;

        Gizmos.DrawRay(transform.position, leftRayDirection * sightRange);
        Gizmos.DrawRay(transform.position, rightRayDirection * sightRange);

        // Дуга для визуализации угла обзора
        Vector3 previousPoint = transform.position + leftRayDirection * sightRange;
        for (int i = 0; i <= fieldOfView; i += 5)
        {
            Quaternion rotation = Quaternion.AngleAxis(-halfFOV + i, Vector3.up);
            Vector3 direction = rotation * transform.forward;
            Vector3 point = transform.position + direction * sightRange;
            Gizmos.DrawLine(transform.position, point);
            Gizmos.DrawLine(previousPoint, point);
            previousPoint = point;
        }
        Gizmos.DrawLine(previousPoint, transform.position + rightRayDirection * sightRange);
    }
}