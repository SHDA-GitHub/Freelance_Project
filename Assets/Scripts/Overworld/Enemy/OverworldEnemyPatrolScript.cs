using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using static BattleDataBridge;

public class OverworldEnemyPatrolScript : MonoBehaviour
{

    private NavMeshAgent agent;
    private Transform player;
    private float timer;
    private bool playerInRange = false;

    [Header("Battle Settings")]
    public List<WeightedEnemy> enemies = new List<WeightedEnemy>();
    public AudioClip battleMusic;
    public BattleBackgroundType backgroundType;
    public BattleTransitionType transitionType = BattleTransitionType.Normal;

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

    public EnemyPreset GetRandomEnemy()
    {
        float totalWeight = 0f;

        foreach (var e in enemies)
            totalWeight += e.spawnChance;

        float randomValue = Random.Range(0, totalWeight);

        float current = 0f;

        foreach (var e in enemies)
        {
            current += e.spawnChance;

            if (randomValue <= current)
                return e.enemy;
        }

        return enemies[0].enemy;
    }

    public void StartBattle()
    {
        BattleDataBridge.UpcomingEnemyPreset = GetRandomEnemy();
        BattleDataBridge.BattleMusic = battleMusic;
        BattleDataBridge.BackgroundSelection = backgroundType;

        var manager = FindFirstObjectByType<BattleTransitionManager>();

        if (manager != null)
        {
            manager.RegisterEncounterEnemy(gameObject);
            manager.StartBattleTransition(transitionType);
        }
    }
}