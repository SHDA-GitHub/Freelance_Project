using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyCardAnimation
{
    public AttackData attackData;
    public AnimationClip animationClip;
}

public class EnemyAI : MonoBehaviour
{
    [Header("State")]
    public EnemyState currentState = EnemyState.Idle;

    [Header("References")]
    [SerializeField] private Statistics stats;
    [SerializeField] private Statistics playerStats;

    [Header("Loadout")]
    public EnemyLoadout loadout;

    [Header("Enemy Attacks")]
    public List<AttackData> enemyAttacks => loadout.allowedAttacks;
    private Dictionary<AttackData, int> cooldowns = new Dictionary<AttackData, int>();

    [Header("Timings")]
    [SerializeField] private float thinkDelay = 1f;
    [SerializeField] private float playDelay = 1f;
    [SerializeField] private float endTurnDelay = 0.1f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private AnimationClip idleAnimation;
    [SerializeField] private List<EnemyCardAnimation> attackAnimations = new();
    [SerializeField] private AnimationClip hurtAnimation;

    [SerializeField] private float returnToIdleDelay = 0.5f;

    private bool isAnimationLocked;

    private AttackData selectedCard;
    [SerializeField] private float EXPToGive = 0f;
    private float lastHealth;

    private void Awake()
    {
        lastHealth = stats.currentHealth;
        if (loadout == null)
        {
            Debug.LogError("EnemyAI has no loadout assigned!");
            enabled = false;
            return;
        }

        foreach (var attack in loadout.allowedAttacks)
        {
            if (attack == null) continue;
            cooldowns[attack] = 0;
        }

        if (stats == null)
            stats = GetComponent<Statistics>();

        Statistics[] statsfind = FindObjectsByType<Statistics>(FindObjectsSortMode.None);

        foreach (var s in statsfind)
        {
            if (s.CompareTag("Player"))
            {
                playerStats = s;
            }
        }
    }
    private void PlayAnimationForCard(AttackData attack)
    {
        if (animator == null || attack == null || isAnimationLocked) return;

        foreach (var entry in attackAnimations)
        {
            if (entry.attackData == attack && entry.animationClip != null)
            {
                StartCoroutine(PlayLockedAnimation(entry.animationClip));
                return;
            }
        }
    }

    private IEnumerator PlayLockedAnimation(AnimationClip clip)
    {
        isAnimationLocked = true;
        animator.CrossFade(clip.name, 0.1f, 0, 0f);
        yield return new WaitForSeconds(clip.length / animator.speed);
        yield return new WaitForSeconds(returnToIdleDelay);
        animator.CrossFade(idleAnimation.name, 0.15f);
        isAnimationLocked = false;
    }

    private void Update()
    {
        CheckForDamage();
    }

    public void StartEnemyTurn()
    {
        StopAllCoroutines();
        StartCoroutine(EnemyTurnRoutine());
    }

    private IEnumerator EnemyTurnRoutine()
    {
        if (stats.currentHealth <= 0)
        {
            GameManager.Instance.flavorTextUI.ShowPlayText($"{stats.characterType} has been defeated!");
            yield return new WaitForSeconds(0.6f);
            StartCoroutine(Die());
            yield break;
        }

        currentState = EnemyState.Idle;
        yield return new WaitForSeconds(thinkDelay);

        currentState = EnemyState.DecideAction;
        DecideCard();

        if (stats.currentHealth <= 0)
        {
            GameManager.Instance.flavorTextUI.ShowPlayText($"{stats.characterType} has been defeated!");
            yield return new WaitForSeconds(0.6f);
            currentState = EnemyState.Idle;
            Destroy(gameObject);
            yield break;
        }

        yield return new WaitForSeconds(thinkDelay);

        currentState = EnemyState.PlayAttack;
        PlaySelectedCard();

        yield return new WaitForSeconds(playDelay);

        currentState = EnemyState.EndTurn;
        yield return new WaitForSeconds(endTurnDelay);

        currentState = EnemyState.Idle;
        GameManager.Instance.StartPlayerTurn();
    }


    private Dictionary<AttackData, float> BuildBaseWeights()
    {
        Dictionary<AttackData, float> weights = new Dictionary<AttackData, float>();

        var availableAttacks = enemyAttacks.FindAll(c =>
            GameManager.Instance.CanEnemyPlayCard(c) &&
            (c.enemyCardType != EnemyAttackType.Steal || loadout.canUseSteal)
        );

        if (availableAttacks.Count == 0)
        {
            var attack = enemyAttacks.Find(c => c.enemyCardType == EnemyAttackType.Attack);
            if (attack != null)
                availableAttacks.Add(attack);
        }

        float baseWeight = 100f / availableAttacks.Count;
        foreach (var card in availableAttacks)
            weights.Add(card, baseWeight);

        return weights;
    }

    private void ApplyHealthModifiers(Dictionary<AttackData, float> weights)
    {
        if (stats.HealthPercent > 0.3f)
            return;

        foreach (var card in new List<AttackData>(weights.Keys))
        {
            if (card.enemyCardType == EnemyAttackType.Heal ||
                card.enemyCardType == EnemyAttackType.Defend)
            {
                weights[card] += 5f;
            }
        }
    }

    private void ApplyWeightDetractors(Dictionary<AttackData, float> weights)
    {
        foreach (var card in new List<AttackData>(weights.Keys))
        {
            if (card.enemyCardType == EnemyAttackType.Defend)
            {
                if (stats.armor >= stats.maxArmor)
                {
                    weights[card] *= 0.95f;
                }
            }

            if (card.enemyCardType == EnemyAttackType.Heal)
            {
                if (stats.HealthPercent >= 0.85f)
                {
                    weights[card] *= 0.935f;
                }
            }
        }
    }

    private AttackData PickWeightedCard(Dictionary<AttackData, float> weights)
    {
        float totalWeight = 0f;

        foreach (var weight in weights.Values)
            totalWeight += weight;

        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (var pair in weights)
        {
            cumulative += pair.Value;
            if (roll <= cumulative)
                return pair.Key;
        }

        return null;
    }

    private void DecideCard()
    {
        var weights = BuildBaseWeights();
        ApplyHealthModifiers(weights);
        ApplyWeightDetractors(weights);
        selectedCard = PickWeightedCard(weights);
    }

    private void PlaySelectedCard()
    {
        if (selectedCard == null) return;

        if (!GameManager.Instance.CanEnemyPlayCard(selectedCard))
        {
            selectedCard = enemyAttacks.Find(c =>
                c.enemyCardType == EnemyAttackType.Attack &&
                GameManager.Instance.CanEnemyPlayCard(c));

            if (selectedCard == null) return;
        }

        PlayAnimationForCard(selectedCard);

        if (selectedCard.isSpell)
            stats.UsePP(selectedCard.manaCost);

        GameManager.Instance.OnEnemyPlayedCard(selectedCard);
        Debug.Log("Enemy played: " + selectedCard.CardName);
    }

    public IEnumerator Die()
    {
        GameManager.Instance.ClearAllEnemyCombatState();
        if (playerStats != null)
        {
            playerStats.RegisterEnemyKill();
            playerStats.AddEXP(EXPToGive);
        }
        if (BattleRewardManager.Instance != null)
        {
            BattleRewardManager.Instance.TryGiveEnemyReward(this);
        }
        else
        {
            Debug.LogWarning("BattleRewardManager not found in scene!");
        }
        yield return null;
        Destroy(gameObject);
    }

    private void CheckForDamage()
    {
        if (stats == null || animator == null) return;

        if (stats.currentHealth < lastHealth && !isAnimationLocked)
        {
            if (hurtAnimation != null)
            {
                StartCoroutine(PlayLockedAnimation(hurtAnimation));
            }
        }

        lastHealth = stats.currentHealth;
    }
}
