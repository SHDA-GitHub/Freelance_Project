using UnityEngine;
using UnityEngine.AI;

public class OverworldEnemyPatrolScript : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform player;
    private float timer;
    private bool playerInRange = false;
    public EnemyPreset enemyType;
    [SerializeField] private float wanderRadius = 25f;
    [SerializeField] private float wanderTimer = 5f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.isStopped = false;
        timer = wanderTimer;
    }

    void FixedUpdate()
    {
        if (playerInRange && player != null)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
        else
        {
            Wander();
        }
        HandleRotation();
    }

    private void Wander()
    {
        timer += Time.deltaTime;

        if (timer >= wanderTimer)
        {
            Vector3 newPos = RandomNavSphere(transform.position, wanderRadius);
            agent.SetDestination(newPos);
            agent.isStopped = false;
            timer = 0;
        }
    }

    private void HandleRotation()
    {
        if (agent.velocity.sqrMagnitude < 0.05f)
            return;

        Vector3 moveDirection = agent.velocity;
        moveDirection.y = 0f;

        float angle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;

        float targetYRotation = (angle > 45 && angle < 135) ? 0f : 180f;

        Quaternion targetRotation = Quaternion.Euler(0f, targetYRotation, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 8f);
    }

    public void SetPlayerInRange(Transform playerTransform, bool inRange)
    {
        player = playerTransform;
        playerInRange = inRange;

        agent.isStopped = false;
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float distance)
    {
        Vector3 randomDirection = Random.insideUnitSphere * distance;
        randomDirection += origin;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit navHit, distance, NavMesh.AllAreas))
        {
            return navHit.position;
        }

        return origin;
    }
}