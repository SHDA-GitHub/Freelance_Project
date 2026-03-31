using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using static BattleDataBridge;

public class PatrolNPC : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform player;
    private float timer;
    private bool playerInRange = false;

    [SerializeField] private NPCDialogue dialogue;
    [SerializeField] private float wanderRadius = 25f;
    [SerializeField] private float wanderTimer = 5f;

    private bool waitingForDialogue = false;
    private PlayerControl playerControl;

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

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerControl player = other.GetComponent<PlayerControl>();

            if (player.isInteracting && !DialogueManager.Instance.IsDialogueActive())
            {
                DialogueManager.Instance.StartDialogue(dialogue, dialogue.GetDialogueLines(), dialogue.GetDialogueMusic());
            }
        }
    }
}