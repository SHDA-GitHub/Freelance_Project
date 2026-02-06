using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public TurnState currentTurn;

    public enum StatusEffectPlayer
    {
        DefenseUp,
        AttackUp,
        Protected,
        Poisoned,
        Bleeding,
        Burning,
        Frozen,
        Electrocuted,
        Linked
    }

    public enum StatusEffectEnemy
    {
        DefenseUp,
        AttackUp,
        Protected,
        Poisoned,
        Bleeding,
        Burning,
        Frozen,
        Electrocuted,
        Linked
    }

    private bool ShouldSpawnBoss()
    {
        int currentRound = consecutiveEnemiesDefeated + 1;
        if (currentRound != 5)
        { return false; }
        return playerStats.playerStats.currentLevel >= 5;
    }

    private enum StealTargetType
    {
        Heal,
        Mana,
        AttackMultiplier,
        PoisonPotion,
        ParalysisPotion,
        FirePotion,
        LightningPotion,
    }
    private class StealTarget
    {
        public StealTargetType type;
        public float weight;

        public StealTarget(StealTargetType type, float weight)
        {
            this.type = type;
            this.weight = weight;
        }
    }

    private Dictionary<Statistics, PoisonEffect> activePoisons = new();

    private class PoisonEffect
    {
        public float damagePerTurn;
        public int turnsRemaining;

        public PoisonEffect(float damage, int duration)
        {
            damagePerTurn = damage;
            turnsRemaining = duration;
        }
    }

    private Dictionary<Statistics, FireEffect> activeFire = new();

    private class FireEffect
    {
        public float damagePerTurn;
        public int turnsRemaining;

        public FireEffect(float damage, int duration)
        {
            damagePerTurn = damage;
            turnsRemaining = duration;
        }
    }

    private Dictionary<Statistics, ThunderEffect> activeThunder = new();

    private class ThunderEffect
    {
        public float damagePerTurn;
        public int turnsRemaining;

        public ThunderEffect(float damage, int duration)
        {
            damagePerTurn = damage;
            turnsRemaining = duration;
        }
    }

    private Dictionary<Statistics, BloodEffect> activeBlood = new();

    private class BloodEffect
    {
        public float damagePerTurn;
        public int turnsRemaining;

        public BloodEffect(float damage, int duration)
        {
            damagePerTurn = damage;
            turnsRemaining = duration;
        }
    }

    private Dictionary<GameObject, float> enemySpawnWeights = new();
    private const float enemyPickWeightMin = 0.7f;
    private const float enemyPickWeightMax = 1.0f;
    private const float weightRemoved = 0.3f;
    private const float weightRecover = 0.15f;

    private Dictionary<Statistics, int> activeParalysis = new();

    private StealTarget PickStealTarget(List<StealTarget> targets)
    {
        float total = 0f;
        foreach (var t in targets)
            total += t.weight;

        float roll = Random.Range(0f, total);
        float increaseEffect = 0f;

        foreach (var t in targets)
        {
            increaseEffect += t.weight;
            if (roll <= increaseEffect)
                return t;
        }

        return null;
    }

    [Header("Enemy Pools By Level")]
    [SerializeField] private List<GameObject> level1Enemies = new();
    [SerializeField] private List<GameObject> level2Enemies = new();
    [SerializeField] private List<GameObject> level3Enemies = new();
    [SerializeField] private List<GameObject> level4Enemies = new();
    [SerializeField] private List<GameObject> bossEnemies = new();

    [SerializeField] public FlavorTextUI flavorTextUI;
    [SerializeField] private EnemyAI enemyAI;
    [SerializeField] private StatusEffectUI playerStatusUI;
    [SerializeField] private StatusEffectUI enemyStatusUI;
    [SerializeField] private FadeScript fade;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform enemySpawnPoint;
    [SerializeField] private EnemyMusicTrigger enemyMusicTrigger;
    [SerializeField] private EnemyBackgroundTrigger enemyBackgroundTrigger;
    [SerializeField] private PlayerAnimationController playerAnimationController;
    private int consecutiveEnemiesDefeated = 0;

    public List<Attack> playerCards;

    public Statistics playerStats;
    public Statistics enemyStats;

    private CooldownTracker playerCooldowns = new();
    private CooldownTracker enemyCooldowns = new();

    private int turnCounter = 0;
    private const int turnsPerManaRegen = 2;
    private const float manaRegenAmount = 1.5f;
    private int enemyTurnOffset = 1;
    public float endPlayerTurnDelay = 1.75f;
    public float endEnemyTurnDelay = 1.75f;
    private int divineTurnsRemaining = 0;
    private int holyBarrierTurnsRemaining = 0;
    private int blacksmithAnvilTurnsRemaining = 0;
    private int attackMultiplierPotionTurnsRemaining = 0;
    private int enemyHolyBarrierTurnsRemaining = 0;
    private int enemyBlacksmithAnvilTurnsRemaining = 0;
    private int enemyAttackMultiplierPotionTurnsRemaining = 0;
    private int enemyDivineTurnsRemaining = 0;
    private const int divineDurationTurns = 2;
    private const int holyBarrierDurationTurns = 1;
    private const int blacksmithAnvilDurationTurns = 2;
    private const int attackMultiplierDurationTurns = 2;
    private const float divineHealAmount = 5f;
    private bool isProcessingDeath = false;
    private bool gameIsOver = false;
    private float playerAttackMultiplier = 1f;
    private bool playerAttackMultiplierActive = false;
    private float enemyAttackMultiplier = 1f;
    private bool enemyAttackMultiplierActive = false;
    private float playerDefenseMultiplier = 1f;
    private bool playerDefenseMultiplierActive = false;
    private float enemyDefenseMultiplier = 1f;
    private bool enemyDefenseMultiplierActive = false;
    private bool playerRiposteActive = false;
    private bool enemyRiposteActive = false;
    private bool playerHolyBarrierActive = false;
    private bool enemyHolyBarrierActive = false;
    private bool playerCursedLinkActive = false;
    private bool enemyCursedLinkActive = false;
    private int cursedLinkTurnsRemaining = 0;
    private int enemyCursedLinkTurnsRemaining = 0;
    private float riposteblock = 0.75f;
    private float ripostereturn = 0.5f;

    private void Awake()
    {
        Instance = this;
        InitializeEnemyWeights();
        AutoAssignStatistics();

        playerCards = new List<Attack>(
            FindObjectsByType<Attack>(FindObjectsSortMode.None)
        );

        enemyAI = FindFirstObjectByType<EnemyAI>();

        foreach (var card in playerCards)
        {
            if (card == null) continue;

            AttackData data = card.GetCardData();
            if (data == null)
            {
                Debug.LogError($"Card '{card.name}' is missing CardData!");
                continue;
            }

            playerCooldowns.Register(data);
        }

        foreach (var card in enemyAI.enemyCards)
        enemyCooldowns.Register(card);
    }

    private void AutoAssignStatistics()
    {
        Statistics[] stats = FindObjectsByType<Statistics>(FindObjectsSortMode.None);

        if (stats.Length < 2)
        {
            Debug.LogError("GameManager: Not enough Statistics components found in the scene!");
            return;
        }

        foreach (var s in stats)
        {
            if (s.CompareTag("Player"))
            {
                playerStats = s;
            }
            else
            {
                enemyStats = s;
            }
        }

        if (playerStats == null)
        {
            Debug.LogError("GameManager: No Statistics found with tag 'Player'!");
        }

        if (enemyStats == null)
        {
            Debug.LogError("GameManager: Enemy Statistics not found (non-Player Statistics)!");
        }
    }

    public bool CanPlayCard(AttackData card)
    {
        if (playerStats.currentHealth <= 0) return false;

        bool hasEnoughMana = true;
        bool hasEnoughHP = true;

        if (card.isSpell)
        {
            hasEnoughMana = playerStats.currentPP >= card.manaCost;
        }
        if (card.isHPDrainSpell)
        {
            hasEnoughHP = playerStats.currentHealth >= 25;
        }

        return currentTurn == TurnState.PlayerTurn &&
               playerCooldowns.IsReady(card) &&
               hasEnoughMana && hasEnoughHP;
    }

    public void StartPlayerTurn()
    {
        if (gameIsOver) return;
        if (playerStats.currentHealth <= 0) return;

        currentTurn = TurnState.PlayerTurn;

        turnCounter++;
        playerCooldowns.Tick();

        if (turnCounter >= turnsPerManaRegen)
        {
            playerStats.RestoreMana(manaRegenAmount);
            turnCounter = 0;
        }

        if (IsParalyzed(playerStats))
        {
            ConsumeParalysisTurn(playerStats);
            flavorTextUI.ShowPlayText("You are paralyzed! Your turn is skipped!");
            StartCoroutine(EndPlayerTurn());
            return;
        }

        ApplyPoisonDamage(playerStats);
        if (CheckForDeathFromStatus(playerStats)) return;
        ApplyFireDamage(playerStats);
        if (CheckForDeathFromStatus(playerStats)) return;
        ApplyThunderDamage(playerStats);
        if (CheckForDeathFromStatus(playerStats)) return;
        ApplyBloodDamage(playerStats);
        if (CheckForDeathFromStatus(playerStats)) return;
        if (playerRiposteActive)
        {
            playerRiposteActive = false;
            playerStatusUI.SetStatus(StatusEffectPlayer.DefenseUp, false);
        }

            if (divineTurnsRemaining > 0)
        {
            playerStats.currentHealth = Mathf.Min(playerStats.currentHealth + divineHealAmount, playerStats.maxHealth);
            divineTurnsRemaining--;
            if (divineTurnsRemaining == 0) Debug.Log("Divine Intervention ended");
            playerStatusUI.SetStatus(StatusEffectPlayer.Protected, false);
        }

        if (holyBarrierTurnsRemaining > 0)
        {
            holyBarrierTurnsRemaining--;
            if (holyBarrierTurnsRemaining == 0)
            {
                Debug.Log("Holy Barrier ended");
                playerStatusUI.SetStatus(StatusEffectPlayer.Protected, false);
                playerHolyBarrierActive = false;
            }
        }

        if (attackMultiplierPotionTurnsRemaining > 0)
        {
            attackMultiplierPotionTurnsRemaining--;
            if (attackMultiplierPotionTurnsRemaining == 0)
            { 
                Debug.Log("Attack Multiplier ended");
                playerStatusUI.SetStatus(StatusEffectPlayer.AttackUp, false);
                playerAttackMultiplierActive = false;
            }
        }

        if (blacksmithAnvilTurnsRemaining > 0)
        {
            blacksmithAnvilTurnsRemaining--;
            if (blacksmithAnvilTurnsRemaining == 0)
            {
                Debug.Log("Defense Multiplier ended");
                playerStatusUI.SetStatus(StatusEffectPlayer.DefenseUp, false);
                playerDefenseMultiplierActive = false;
            }
        }

        if (cursedLinkTurnsRemaining > 0)
        {
            cursedLinkTurnsRemaining--;
            if (cursedLinkTurnsRemaining == 0)
            {
                Debug.Log("The cursed link fades.");
                enemyStatusUI.SetStatus(StatusEffectEnemy.Linked, false);
                playerCursedLinkActive = false;
            }
        }

        foreach (var card in playerCards)
        {
            int cd = playerCooldowns.GetCooldown(card.GetCardData());
            card.SetCooldownVisual(cd);
        }

        flavorTextUI.ShowPlayText("Your turn.");
    }

    public IEnumerator EndPlayerTurn()
    {
        currentTurn = TurnState.Busy;

        foreach (var card in playerCards)
        {
            card.DisableCard();
        }
        yield return new WaitForSeconds(endPlayerTurnDelay);
        StartCoroutine(HandleTurnTransition());
    }

    public IEnumerator EndEnemyTurn()
    {
        currentTurn = TurnState.Busy;
        yield return new WaitForSeconds(endEnemyTurnDelay);
        StartPlayerTurn();
    }

    public void StartEnemyTurn()
    {
        if (gameIsOver) return;
        if (enemyStats.currentHealth <= 0) return;

        enemyCooldowns.Tick();

        if ((turnCounter + enemyTurnOffset) % turnsPerManaRegen == 0)
            enemyStats.RestoreMana(manaRegenAmount);

        if (IsParalyzed(enemyStats))
        {
            ConsumeParalysisTurn(enemyStats);
            flavorTextUI.ShowPlayText("Enemy is paralyzed! Enemy's turn is skipped!");
            StartCoroutine(EndEnemyTurn());
            return;
        }

        ApplyPoisonDamage(enemyStats);
        if (CheckForDeathFromStatus(enemyStats)) return;
        ApplyFireDamage(enemyStats);
        if (CheckForDeathFromStatus(enemyStats)) return;
        ApplyThunderDamage(enemyStats);
        if (CheckForDeathFromStatus(enemyStats)) return;
        ApplyBloodDamage(enemyStats);
        if (CheckForDeathFromStatus(enemyStats)) return;
        if (enemyRiposteActive == true)
        {
        enemyRiposteActive = false;
        enemyStatusUI.SetStatus(StatusEffectEnemy.DefenseUp, false);
        }

        if (enemyDivineTurnsRemaining > 0)
        {
            enemyStats.currentHealth = Mathf.Min(enemyStats.currentHealth + divineHealAmount, enemyStats.maxHealth);
            enemyStatusUI.SetStatus(StatusEffectEnemy.Protected, false);
            enemyDivineTurnsRemaining--;
        }

        if (enemyHolyBarrierTurnsRemaining > 0)
        {
            enemyHolyBarrierTurnsRemaining--;
            if (enemyHolyBarrierTurnsRemaining == 0)
            {
                enemyStatusUI.SetStatus(StatusEffectEnemy.Protected, false);
                enemyHolyBarrierActive = false;
            }
        }

        if (enemyAttackMultiplierPotionTurnsRemaining > 0)
        {
            enemyAttackMultiplierPotionTurnsRemaining--;
            if (enemyAttackMultiplierPotionTurnsRemaining == 0)
            {
                enemyStatusUI.SetStatus(StatusEffectEnemy.AttackUp, false);
                enemyAttackMultiplierActive = false;
            }
        }

        if (enemyBlacksmithAnvilTurnsRemaining > 0)
        {
            enemyBlacksmithAnvilTurnsRemaining--;
            if (enemyBlacksmithAnvilTurnsRemaining == 0)
            {
                enemyStatusUI.SetStatus(StatusEffectEnemy.DefenseUp, false);
                enemyDefenseMultiplierActive = false;
            }
        }

        if (enemyCursedLinkTurnsRemaining > 0)
        {
            enemyCursedLinkTurnsRemaining--;
            if (enemyCursedLinkTurnsRemaining == 0)
            {
                playerStatusUI.SetStatus(StatusEffectPlayer.Linked, false);
                enemyCursedLinkActive = false;
            }
        }

        enemyAI.StartEnemyTurn();
        flavorTextUI.ShowPlayText("Enemy's turn.");
    }

    private IEnumerator HandleTurnTransition()
    {
        while (isProcessingDeath)
            yield return null;

        if (playerStats.currentHealth <= 0)
        {
            Debug.Log("Player is dead. Game Over!");
            yield break;
        }

        if (enemyStats.currentHealth <= 0)
        {
            Debug.Log("Enemy is dead. Player wins!");
            yield break;
        }
        StartEnemyTurn();
    }

    public void OnPlayerPlayedCard(AttackData cardData)
    {
        if (!CanPlayCard(cardData))
        {
            if (cardData.isSpell && playerStats.currentPP < cardData.manaCost)
            {
                flavorTextUI.ShowPlayText($"Not enough Mana to play {cardData.CardName}!");
            }
            else
            {
                flavorTextUI.ShowPlayText("Card is on cooldown!");
            }

            return;
        }

        if (playerAnimationController != null)
        {
            playerAnimationController.PlayCardAnimation(cardData);
        }

        if (cardData.isSpell)
        {
            playerStats.UsePP(cardData.manaCost);
        }

        if (cardData.isHPDrainSpell)
        {
            playerStats.UseHP(cardData.healthCost);
        }

        flavorTextUI.ShowPlayText(cardData.PlayerPlayText);

        ApplyCardEffect(cardData, playerStats, enemyStats);
        playerCooldowns.SetCooldown(cardData, cardData.playerCooldownTurns);

        StartCoroutine(EndPlayerTurn());
    }

    public void OnEnemyPlayedCard(AttackData cardData)
    {
        flavorTextUI.ShowPlayText(cardData.EnemyPlayText);

        ApplyCardEffect(cardData, enemyStats, playerStats);

        enemyCooldowns.SetCooldown(cardData, cardData.enemyCooldownTurns);
    }

    public bool CanEnemyPlayCard(AttackData card)
    {
        bool hasEnoughMana = true;

        if (card.isSpell)
            hasEnoughMana = enemyStats.currentPP >= card.manaCost;

        return enemyCooldowns.IsReady(card) && hasEnoughMana;
    }
    public void GameOver(bool playerWon)
    {
        if (gameIsOver) return;
        gameIsOver = true;

        currentTurn = TurnState.Busy;

        foreach (var card in playerCards)
        {
            card.DisableCard();
            card.ForceShowBack();
        }
        if (playerWon)
            flavorTextUI.ShowPlayText("You have defeated the enemy!");
        else
            flavorTextUI.ShowPlayText("You were defeated...");

        Debug.Log(playerWon ? "Victory!" : "Defeat!");
    }

    public void ShowCardHoverInfo(AttackData cardData)
    {
        flavorTextUI.ShowCardInfoText(cardData.HoverInfoText);
    }

    public void ApplyCardEffect(AttackData cardData, Statistics dealer, Statistics target)
    {
        if (dealer.characterType == Statistics.CharacterType.Player)
        {
            switch (cardData.playerCardType)
            {
                case PlayerCardType.Attack:
                    {
                        float rawDamage = cardData.effectAmountAttack;
                        if (playerAttackMultiplierActive)
                        {
                            rawDamage *= playerAttackMultiplier;
                            playerAttackMultiplierActive = false;
                            playerAttackMultiplier = 1f;
                            playerStatusUI.SetStatus(StatusEffectPlayer.AttackUp, false);
                        }

                        float effectiveArmor = Mathf.Max(target.armor, 0);
                        float damage = Mathf.Max(rawDamage - effectiveArmor, 0);
                        target.armor = Mathf.Max(target.armor - 10, 0);
                        bool riposteTriggered = false;

                        if (enemyRiposteActive)
                        {
                            float blocked = damage * riposteblock;
                            float finalDamage = damage - blocked;
                            float riposteDamage = damage * ripostereturn;

                            target.currentHealth -= finalDamage;
                            dealer.currentHealth -= riposteDamage;
                            enemyStatusUI.SetStatus(StatusEffectEnemy.DefenseUp, false);
                            if (playerCursedLinkActive)
                            {
                                enemyStats.currentHealth -= riposteDamage;
                                Debug.Log("Cursed Link echoes the pain to the enemy!");
                            }
                            enemyRiposteActive = false;
                            riposteTriggered = true;

                            flavorTextUI.ShowPlayText("The enemy deflects your strike and counters viciously!");
                        }
                        else
                        { target.currentHealth -= damage; }
                        if (enemyCursedLinkActive)
                        {
                            playerStats.currentHealth -= damage;
                            Debug.Log("Cursed Link echoes the pain to the enemy!");
                        }
                        if (!riposteTriggered && target.armor > 0)
                        { target.armor -= 10; }
                        Debug.Log($"Player attacks Enemy for {damage}!");
                        break;
                    }

                case PlayerCardType.Defend:
                    if (playerDefenseMultiplierActive)
                    {
                        dealer.armor = Mathf.Max(dealer.armor, cardData.effectAmountDefend * playerDefenseMultiplier);
                        playerDefenseMultiplierActive = false;
                        playerStatusUI.SetStatus(StatusEffectPlayer.DefenseUp, false);
                    }
                    else
                    {
                        dealer.armor = Mathf.Max(dealer.armor, cardData.effectAmountDefend);
                    }
                    Debug.Log($"Player gains {cardData.effectAmountDefend} armor!");
                    break;

                case PlayerCardType.Greatersword:
                    {
                        float rawDamage = cardData.effectAmountAttack;
                        if (playerAttackMultiplierActive)
                        {
                            rawDamage *= playerAttackMultiplier;
                            playerAttackMultiplierActive = false;
                            playerAttackMultiplier = 1f;
                            playerStatusUI.SetStatus(StatusEffectPlayer.AttackUp, false);
                        }

                        float effectiveArmor = Mathf.Max(target.armor, 0);
                        float damage = Mathf.Max(rawDamage - effectiveArmor, 0);
                        target.armor = Mathf.Max(target.armor - 10, 0);
                        bool riposteTriggered = false;

                        if (enemyRiposteActive)
                        {
                            float blocked = damage * riposteblock;
                            float finalDamage = damage - blocked;
                            float riposteDamage = damage * ripostereturn;

                            target.currentHealth -= finalDamage;
                            dealer.currentHealth -= riposteDamage;
                            enemyStatusUI.SetStatus(StatusEffectEnemy.DefenseUp, false);
                            if (playerCursedLinkActive)
                            {
                                enemyStats.currentHealth -= riposteDamage;
                                Debug.Log("Cursed Link echoes the pain to the enemy!");
                            }
                            enemyRiposteActive = false;
                            riposteTriggered = true;

                            flavorTextUI.ShowPlayText("The enemy deflects your strike and counters viciously!");
                        }
                        else
                        { target.currentHealth -= damage; }
                        if (enemyCursedLinkActive)
                        {
                            playerStats.currentHealth -= damage;
                            Debug.Log("Cursed Link echoes the pain to the enemy!");
                        }
                        if (!riposteTriggered && target.armor > 0)
                        { target.armor -= 12; }
                        Debug.Log($"Player strikes Enemy for {damage}!");
                        break;
                    }

                case PlayerCardType.Greatershield:
                    if (playerDefenseMultiplierActive)
                    {
                        dealer.armor = Mathf.Max(dealer.armor, cardData.effectAmountDefend * playerDefenseMultiplier);
                        playerDefenseMultiplierActive = false;
                        playerStatusUI.SetStatus(StatusEffectPlayer.DefenseUp, false);
                    }
                    else
                    {
                        dealer.armor = Mathf.Max(dealer.armor, cardData.effectAmountDefend);
                    }
                    Debug.Log($"Player's defense greatly increased by {cardData.effectAmountDefend}!");
                    break;

                case PlayerCardType.Heal:
                    dealer.currentHealth = Mathf.Min(dealer.currentHealth + cardData.effectAmountHeal, dealer.maxHealth);
                    Debug.Log($"Player heals for {cardData.effectAmountHeal}!");
                    break;

                case PlayerCardType.ManaRefill:
                    dealer.currentPP = Mathf.Min(dealer.currentPP + cardData.effectAmountMana, dealer.maxPP);
                    flavorTextUI.ShowPlayText($"{dealer.characterType} replenished Mana for {cardData.effectAmountMana}!");
                    Debug.Log($"Player replenished Mana for {cardData.effectAmountMana}!");
                    break;

                case PlayerCardType.DivineIntervention:
                    divineTurnsRemaining = divineDurationTurns;
                    playerStatusUI.SetStatus(StatusEffectPlayer.Protected, true);
                    Debug.Log("Player invokes Divine Intervention!");
                    break;

                case PlayerCardType.Steal:
                    {
                        List<StealTarget> targets = new();

                        AttackData enemyHeal = enemyAI.enemyCards
                            .Find(c => c.enemyCardType == EnemyCardType.Heal && enemyCooldowns.IsReady(c));

                        AttackData enemyMana = enemyAI.enemyCards
                            .Find(c => c.enemyCardType == EnemyCardType.ManaRefill && enemyCooldowns.IsReady(c));

                        AttackData enemyAttackMultiplierCard = enemyAI.enemyCards
                            .Find(c => c.enemyCardType == EnemyCardType.AttackMultiplier && enemyCooldowns.IsReady(c));

                        AttackData enemyPoisonPotion = enemyAI.enemyCards
                            .Find(c => c.enemyCardType == EnemyCardType.PoisonPotion && enemyCooldowns.IsReady(c));

                        AttackData enemyParalysisPotion = enemyAI.enemyCards
                            .Find(c => c.enemyCardType == EnemyCardType.ParalysisPotion && enemyCooldowns.IsReady(c));

                        AttackData enemyFirePotion = enemyAI.enemyCards
                            .Find(c => c.enemyCardType == EnemyCardType.FirePotion && enemyCooldowns.IsReady(c));

                        AttackData enemyLightningPotion = enemyAI.enemyCards
                            .Find(c => c.enemyCardType == EnemyCardType.LightningPotion && enemyCooldowns.IsReady(c));

                        if (enemyAttackMultiplierCard != null)
                            targets.Add(new StealTarget(StealTargetType.AttackMultiplier, 1f));

                        if (enemyHeal != null)
                            targets.Add(new StealTarget(StealTargetType.Heal, 1f));

                        if (enemyMana != null)
                            targets.Add(new StealTarget(StealTargetType.Mana, 1f));

                        if (enemyPoisonPotion != null)
                            targets.Add(new StealTarget(StealTargetType.PoisonPotion, 1f));

                        if (enemyParalysisPotion != null)
                            targets.Add(new StealTarget(StealTargetType.ParalysisPotion, 1f));

                        if (enemyFirePotion != null)
                            targets.Add(new StealTarget(StealTargetType.FirePotion, 1f));

                        if (enemyLightningPotion != null)
                            targets.Add(new StealTarget(StealTargetType.LightningPotion, 1f));

                        if (targets.Count == 0)
                        {
                            flavorTextUI.ShowPlayText("There was nothing to steal!");
                            break;
                        }

                        float baseWeight = 100f / targets.Count;
                        foreach (var t in targets)
                            t.weight = baseWeight;

                        bool lowHP = enemyStats.HealthPercent <= 0.3f;
                        bool lowMana = enemyStats.ppPercent <= 0.3f;

                        if (lowHP)
                            AdjustWeights(targets, StealTargetType.Heal, 7.5f);

                        if (lowMana)
                            AdjustWeights(targets, StealTargetType.Mana, 7.5f);

                        StealTarget chosen = PickStealTarget(targets);

                        switch (chosen.type)
                        {
                            case StealTargetType.Heal:
                                enemyCooldowns.SetCooldown(enemyHeal, enemyHeal.enemyCooldownTurns);
                                dealer.currentHealth = Mathf.Min(dealer.currentHealth + 12f, dealer.maxHealth);
                                flavorTextUI.ShowPlayText("You stole a Heal potion!");
                                break;

                            case StealTargetType.Mana:
                                enemyCooldowns.SetCooldown(enemyMana, enemyMana.enemyCooldownTurns);
                                dealer.currentPP = Mathf.Min(dealer.currentPP + 7.5f, dealer.maxPP);
                                flavorTextUI.ShowPlayText("You stole a Mana potion!");
                                break;

                            case StealTargetType.AttackMultiplier:
                                enemyCooldowns.SetCooldown(enemyAttackMultiplierCard, enemyAttackMultiplierCard.enemyCooldownTurns);
                                playerAttackMultiplier = 2f;
                                playerAttackMultiplierActive = true;
                                flavorTextUI.ShowPlayText("You stole the enemy's attack potion!");
                                playerStatusUI.SetStatus(StatusEffectPlayer.AttackUp, true);
                                break;

                            case StealTargetType.PoisonPotion:
                                float stealPImpactDamage = cardData.effectAmountAttack;
                                dealer.currentHealth -= stealPImpactDamage;
                                enemyCooldowns.SetCooldown(enemyPoisonPotion, enemyPoisonPotion.enemyCooldownTurns);
                                float poisonDamage = 4.5f;
                                int poisonDuration = 3;
                                if (activePoisons.ContainsKey(dealer))
                                {
                                    activePoisons[dealer].damagePerTurn += poisonDamage;
                                    activePoisons[dealer].turnsRemaining = Mathf.Max(activePoisons[dealer].turnsRemaining, poisonDuration);
                                }
                                else
                                {
                                    activePoisons[dealer] = new PoisonEffect(poisonDamage, poisonDuration);
                                }
                                flavorTextUI.ShowPlayText($"The player stole a poison potion and it backfires on them!");
                                playerStatusUI.SetStatus(StatusEffectPlayer.Poisoned, true);
                                break;

                            case StealTargetType.FirePotion:
                                float stealFImpactDamage = cardData.effectAmountAttack;
                                dealer.currentHealth -= stealFImpactDamage;
                                enemyCooldowns.SetCooldown(enemyFirePotion, enemyFirePotion.enemyCooldownTurns);
                                float fireDamage = 3f;
                                int fireDuration = 3;
                                if (activeFire.ContainsKey(dealer))
                                {
                                    activeFire[dealer].damagePerTurn += fireDamage;
                                    activeFire[dealer].turnsRemaining = Mathf.Max(activeFire[dealer].turnsRemaining, fireDuration);
                                }
                                else
                                {
                                    activeFire[dealer] = new FireEffect(fireDamage, fireDuration);
                                }
                                flavorTextUI.ShowPlayText($"The player stole a fire potion and it backfires on them!");
                                playerStatusUI.SetStatus(StatusEffectPlayer.Burning, true);
                                break;

                            case StealTargetType.LightningPotion:
                                float stealLImpactDamage = cardData.effectAmountAttack;
                                dealer.currentHealth -= stealLImpactDamage;
                                enemyCooldowns.SetCooldown(enemyLightningPotion, enemyLightningPotion.enemyCooldownTurns);
                                float lightningDamage = 5f;
                                int lightningDuration = 3;
                                if (activeThunder.ContainsKey(dealer))
                                {
                                    activeThunder[dealer].damagePerTurn += lightningDamage;
                                    activeThunder[dealer].turnsRemaining = Mathf.Max(activeThunder[dealer].turnsRemaining, lightningDuration);
                                }
                                else
                                {
                                    activeThunder[dealer] = new ThunderEffect(lightningDamage, lightningDuration);
                                }
                                flavorTextUI.ShowPlayText($"The player stole a lightning potion and it backfires on them!");
                                playerStatusUI.SetStatus(StatusEffectPlayer.Electrocuted, true);
                                break;
                        }
                        break;
                    }

                case PlayerCardType.CallOfNature:
                    float natureDamage = Mathf.Max(cardData.effectAmountAttack - target.armor, 0);
                    target.currentHealth -= natureDamage;
                    if (enemyCursedLinkActive)
                    {
                        playerStats.currentHealth -= natureDamage;
                        Debug.Log("Cursed Link echoes the pain to the enemy!");
                    }
                    if (target.armor > 0) { target.armor -= 11.5f; }
                    float bloodDamage = cardData.effectAmountAttackOvertime;
                    int bloodDuration = 1;
                    if (activeBlood.ContainsKey(enemyStats) && enemyHolyBarrierActive == false)
                    {
                        activeBlood[enemyStats].damagePerTurn += bloodDamage;
                        activeBlood[enemyStats].turnsRemaining = Mathf.Max(activeBlood[enemyStats].turnsRemaining, bloodDuration);
                    }
                    else if (enemyHolyBarrierActive == false)
                    {
                        activeBlood[enemyStats] = new BloodEffect(bloodDamage, bloodDuration);
                    }
                    flavorTextUI.ShowPlayText($"{dealer.characterType} unleashes nature's wrath on Enemy for {natureDamage}!");
                    enemyStatusUI.SetStatus(StatusEffectEnemy.Bleeding, true);
                    Debug.Log($"Player unleashes nature's wrath on Enemy for {natureDamage}!");
                    break;

                case PlayerCardType.AttackMultiplier:
                    playerAttackMultiplier = 2f;
                    playerAttackMultiplierActive = true;
                    attackMultiplierPotionTurnsRemaining = attackMultiplierDurationTurns;
                    flavorTextUI.ShowPlayText("Player drank an invigorating potion and doubled their melee damage for the next turn!");
                    playerStatusUI.SetStatus(StatusEffectPlayer.AttackUp, true);
                    Debug.Log("Player's next attack damage is doubled!");
                    break;

                case PlayerCardType.BlacksmithAnvil:
                    playerDefenseMultiplier = 2f;
                    playerDefenseMultiplierActive = true;
                    blacksmithAnvilTurnsRemaining = blacksmithAnvilDurationTurns;
                    flavorTextUI.ShowPlayText("Player hardened their shield with the anvil and doubled their defense for the next turn!");
                    playerStatusUI.SetStatus(StatusEffectPlayer.DefenseUp, true);
                    Debug.Log("Player's next defense is doubled!");
                    break;

                case PlayerCardType.PoisonPotion:
                    {
                        float impactDamage = cardData.effectAmountAttack;
                        target.currentHealth -= impactDamage;
                        if (enemyCursedLinkActive)
                        {
                            playerStats.currentHealth -= impactDamage;
                            Debug.Log("Cursed Link echoes the pain to the enemy!");
                        }
                        float poisonDamage = cardData.effectAmountAttackOvertime;
                        int poisonDuration = 3;
                        if (activePoisons.ContainsKey(enemyStats) && enemyHolyBarrierActive == false)
                        {
                            activePoisons[enemyStats].damagePerTurn += poisonDamage;
                            activePoisons[enemyStats].turnsRemaining = Mathf.Max(activePoisons[enemyStats].turnsRemaining, poisonDuration);
                        }
                        else if (enemyHolyBarrierActive == false)
                        {
                            activePoisons[enemyStats] = new PoisonEffect(poisonDamage, poisonDuration);
                        }
                        if (cardData.hasFlavortext == true)
                        { flavorTextUI.ShowPlayText("You threw a poison potion at the enemy!"); }
                        enemyStatusUI.SetStatus(StatusEffectEnemy.Poisoned, true);
                        Debug.Log($"Enemy poisoned for {poisonDamage} damage per turn for {poisonDuration} turns.");
                    }
                    break;

                case PlayerCardType.RestorationSpell:
                    dealer.currentHealth = Mathf.Min(dealer.currentHealth + cardData.effectAmountHeal, dealer.maxHealth);
                    flavorTextUI.ShowPlayText($"Player used a spell to gain {cardData.effectAmountHeal} HP!");
                    break;

                case PlayerCardType.KnowledgeTome:
                    dealer.currentPP = Mathf.Min(dealer.currentPP + cardData.effectAmountMana, dealer.maxPP);
                    flavorTextUI.ShowPlayText($"{dealer.characterType} restored a massive amount of Mana for {cardData.effectAmountMana}!");
                    Debug.Log($"Player restored a massive amount of Mana for {cardData.effectAmountMana}!");
                    break;

                case PlayerCardType.BrimstoneRain:
                    {
                        float impactDamage = Mathf.Max(cardData.effectAmountAttack - target.armor, 0);
                        target.currentHealth -= impactDamage;
                        if (enemyCursedLinkActive)
                        {
                            playerStats.currentHealth -= impactDamage;
                            Debug.Log("Cursed Link echoes the pain to the enemy!");
                        }
                        if (target.armor > 0) { target.armor -= 10; }
                        float fireDamage = cardData.effectAmountAttackOvertime;
                        int fireDuration = 4;
                        if (activeFire.ContainsKey(enemyStats) && enemyHolyBarrierActive == false)
                        {
                            activeFire[enemyStats].damagePerTurn += fireDamage;
                            activeFire[enemyStats].turnsRemaining = Mathf.Max(activeFire[enemyStats].turnsRemaining, fireDuration);
                        }
                        else if (enemyHolyBarrierActive == false)
                        {
                            activeFire[enemyStats] = new FireEffect(fireDamage, fireDuration);
                        }
                        if (cardData.hasFlavortext == true)
                        { flavorTextUI.ShowPlayText("You rained down fire on the enemy!"); }
                        enemyStatusUI.SetStatus(StatusEffectEnemy.Burning, true);
                        Debug.Log($"Enemy on fire for {fireDamage} damage per turn for {fireDuration} turns.");
                    }
                    break;

                case PlayerCardType.ZeusWrath:
                    {
                        float impactDamage = Mathf.Max(cardData.effectAmountAttack - target.armor, 0);
                        target.currentHealth -= impactDamage;
                        if (enemyCursedLinkActive)
                        {
                            playerStats.currentHealth -= impactDamage;
                            Debug.Log("Cursed Link echoes the pain to the enemy!");
                        }
                        if (target.armor > 0) { target.armor -= 10; }
                        float thunderDamage = cardData.effectAmountAttackOvertime;
                        int thunderDuration = 3;
                        if (activeThunder.ContainsKey(enemyStats) && enemyHolyBarrierActive == false)
                        {
                            activeThunder[enemyStats].damagePerTurn += thunderDamage;
                            activeThunder[enemyStats].turnsRemaining = Mathf.Max(activeThunder[enemyStats].turnsRemaining, thunderDuration);
                        }
                        else if (enemyHolyBarrierActive == false)
                        {
                            activeThunder[enemyStats] = new ThunderEffect(thunderDamage, thunderDuration);
                        }
                        flavorTextUI.ShowPlayText("You smited the enemy with the power of Zeus!");
                        enemyStatusUI.SetStatus(StatusEffectEnemy.Electrocuted, true);
                        Debug.Log($"Enemy was electrocuted for {thunderDamage} damage per turn for {thunderDuration} turns.");
                    }
                    break;

                case PlayerCardType.BleedStrike:
                    float bleedDamage = Mathf.Max(cardData.effectAmountAttack - target.armor, 0);
                    target.currentHealth -= bleedDamage;
                    if (enemyCursedLinkActive)
                    {
                        playerStats.currentHealth -= bleedDamage;
                        Debug.Log("Cursed Link echoes the pain to the enemy!");
                    }
                    if (target.armor > 0) { target.armor -= 6.5f; }
                    float bleedingDamage = cardData.effectAmountAttackOvertime;
                    int bleedingDuration = 2;
                    if (activeBlood.ContainsKey(enemyStats) && enemyHolyBarrierActive == false)
                    {
                        activeBlood[enemyStats].damagePerTurn += bleedingDamage;
                        activeBlood[enemyStats].turnsRemaining = Mathf.Max(activeBlood[enemyStats].turnsRemaining, bleedingDuration);
                    }
                    else if (enemyHolyBarrierActive == false)
                    {
                        activeBlood[enemyStats] = new BloodEffect(bleedingDamage, bleedingDuration);
                    }
                    enemyStatusUI.SetStatus(StatusEffectEnemy.Bleeding, true);
                    Debug.Log($"The Player's sharp edge causes {bleedDamage} damage!");
                    break;

                case PlayerCardType.ParalysisPotion:
                    {
                        float impactDamage = Mathf.Max(cardData.effectAmountAttack - target.armor, 0);
                        target.currentHealth -= impactDamage;
                        if (enemyCursedLinkActive)
                        {
                            playerStats.currentHealth -= impactDamage;
                            Debug.Log("Cursed Link echoes the pain to the enemy!");
                        }
                        int paralysisTurns = Mathf.RoundToInt(cardData.effectAmountTurnSkip);
                        if (enemyHolyBarrierActive == false)
                        { ApplyParalysis(enemyStats, paralysisTurns); }
                        if (cardData.hasFlavortext == true)
                        {
                            if (enemyHolyBarrierActive == true)
                            { flavorTextUI.ShowPlayText($"The player threw a Paralysis Potion at the enemy, but it failed!"); }
                            else
                            { flavorTextUI.ShowPlayText($"The player threw a Paralysis Potion at the enemy! Enemy is paralyzed for {paralysisTurns} turn{(paralysisTurns > 1 ? "s" : "")}!"); }
                        }
                        enemyStatusUI.SetStatus(StatusEffectEnemy.Frozen, true);
                        Debug.Log($"Enemy paralyzed for {paralysisTurns} turns.");
                    }
                    break;

                case PlayerCardType.FirePotion:
                    {
                        float impactDamage = cardData.effectAmountAttack;
                        target.currentHealth -= impactDamage;
                        if (enemyCursedLinkActive)
                        {
                            playerStats.currentHealth -= impactDamage;
                            Debug.Log("Cursed Link echoes the pain to the enemy!");
                        }
                        float fireDamage = cardData.effectAmountAttackOvertime;
                        int fireDuration = 3;
                        if (activeFire.ContainsKey(enemyStats) && enemyHolyBarrierActive == false)
                        {
                            activeFire[enemyStats].damagePerTurn += fireDamage;
                            activeFire[enemyStats].turnsRemaining = Mathf.Max(activeFire[enemyStats].turnsRemaining, fireDuration);
                        }
                        else if (enemyHolyBarrierActive == false)
                        {
                            activeFire[enemyStats] = new FireEffect(fireDamage, fireDuration);
                        }
                        flavorTextUI.ShowPlayText($"You threw a fire potion at the enemy!");
                        enemyStatusUI.SetStatus(StatusEffectEnemy.Burning, true);
                        Debug.Log($"Enemy enflamed for {fireDamage} damage per turn for {fireDuration} turns.");
                        break;
                    }

                case PlayerCardType.LightningPotion:
                    {
                        float impactDamage = cardData.effectAmountAttack;
                        target.currentHealth -= impactDamage;
                        if (enemyCursedLinkActive)
                        {
                            playerStats.currentHealth -= impactDamage;
                            Debug.Log("Cursed Link echoes the pain to the enemy!");
                        }
                        float thunderDamage = cardData.effectAmountAttackOvertime;
                        int thunderDuration = 2;
                        if (activeThunder.ContainsKey(enemyStats) && enemyHolyBarrierActive == false)
                        {
                            activeThunder[enemyStats].damagePerTurn += thunderDamage;
                            activeThunder[enemyStats].turnsRemaining = Mathf.Max(activeThunder[enemyStats].turnsRemaining, thunderDuration);
                        }
                        else if (enemyHolyBarrierActive == false)
                        {
                            activeThunder[enemyStats] = new ThunderEffect(thunderDamage, thunderDuration);
                        }
                        flavorTextUI.ShowPlayText($"You threw a lightning potion at the enemy!");
                        enemyStatusUI.SetStatus(StatusEffectEnemy.Electrocuted, true);
                        Debug.Log($"Enemy electrocuted for {thunderDamage} damage per turn for {thunderDuration} turns.");
                        break;
                    }

                case PlayerCardType.ThiefSmokeBomb:
                    {
                        int paralysisTurns = Mathf.RoundToInt(cardData.effectAmountTurnSkip);
                        playerAttackMultiplier = 1.5f;
                        playerAttackMultiplierActive = true;
                        enemyStatusUI.SetStatus(StatusEffectEnemy.Frozen, true);
                        ApplyParalysis(enemyStats, paralysisTurns);
                        flavorTextUI.ShowPlayText($"The player threw a smoke bomb at the enemy! The player gets lucky strike next turn and enemy is stunned for {paralysisTurns} turn{(paralysisTurns > 1 ? "s" : "")}!");
                        Debug.Log($"Enemy stunned for {paralysisTurns} turns.");
                        break;
                    }

                case PlayerCardType.RefreshBreeze:
                    {
                        dealer.armor = Mathf.Max(dealer.armor, cardData.effectAmountDefend);
                        flavorTextUI.ShowPlayText($"The player summoned a gust of wind and cleansed their afflictions!");
                        ClearAllStatusEffects(dealer);
                        Debug.Log($"Player gains {cardData.effectAmountDefend} armor!");
                        break;
                    }

                case PlayerCardType.Riposte:
                    {
                        playerRiposteActive = true;
                        playerStatusUI.SetStatus(StatusEffectPlayer.DefenseUp, true);
                        flavorTextUI.ShowPlayText("The player takes a steady and defensive stance, waiting to counter the enemy's strike.");
                        break;
                    }

                case PlayerCardType.EssenceLife:
                    {
                        if (dealer.currentPP < cardData.manaCost)
                        {
                            flavorTextUI.ShowPlayText("Not enough Mana to convert into life!");
                            break;
                        }
                        dealer.currentHealth = Mathf.Min(dealer.currentHealth + cardData.effectAmountHeal, dealer.maxHealth);

                        flavorTextUI.ShowPlayText(
                            $"You transmuted mind into life and restored {cardData.effectAmountHeal} HP!"
                        );
                        Debug.Log(
                            $"Player converted {cardData.manaCost} mana into {cardData.effectAmountHeal} HP."
                        );
                        break;
                    }

                case PlayerCardType.EssenceMind:
                    {
                        if (dealer.currentHealth < 25)
                        {
                            flavorTextUI.ShowPlayText("Not safe to convert HP into Mana!");
                            break;
                        }
                        dealer.currentPP = Mathf.Min(dealer.currentPP + cardData.effectAmountMana, dealer.maxPP);
                        flavorTextUI.ShowPlayText(
                            $"You transmuted life into mind and restored {cardData.effectAmountMana} Mana!"
                        );
                        Debug.Log(
                            $"Player converted {cardData.healthCost} HP into {cardData.effectAmountMana} Mana."
                        );
                        break;
                    }

                case PlayerCardType.PoisonDart:
                    {
                        float impactDamage = cardData.effectAmountAttack;
                        target.currentHealth -= impactDamage;
                        if (enemyCursedLinkActive)
                        {
                            playerStats.currentHealth -= impactDamage;
                            Debug.Log("Cursed Link echoes the pain to the enemy!");
                        }
                        float poisonDamage = cardData.effectAmountAttackOvertime;
                        int poisonDuration = 2;
                        if (activePoisons.ContainsKey(enemyStats) && enemyHolyBarrierActive == false)
                        {
                            activePoisons[enemyStats].damagePerTurn += poisonDamage;
                            activePoisons[enemyStats].turnsRemaining = Mathf.Max(activePoisons[enemyStats].turnsRemaining, poisonDuration);
                        }
                        else if (enemyHolyBarrierActive == false)
                        {
                            activePoisons[enemyStats] = new PoisonEffect(poisonDamage, poisonDuration);
                        }
                        flavorTextUI.ShowPlayText("You shot a poison dart at the enemy!");
                        enemyStatusUI.SetStatus(StatusEffectEnemy.Poisoned, true);
                        Debug.Log($"Enemy poisoned for {poisonDamage} damage per turn for {poisonDuration} turns.");
                    }
                    break;

                case PlayerCardType.ShieldBash:
                    {
                        float rawDamage = cardData.effectAmountAttack;
                        float armorGain = cardData.effectAmountDefend;
                        if (playerAttackMultiplierActive)
                        {
                            rawDamage *= playerAttackMultiplier;
                            playerAttackMultiplierActive = false;
                            playerAttackMultiplier = 1f;
                            playerStatusUI.SetStatus(StatusEffectPlayer.AttackUp, false);
                        }
                        if (playerDefenseMultiplierActive)
                        {
                            armorGain *= playerDefenseMultiplier;
                            playerDefenseMultiplierActive = false;
                            playerDefenseMultiplier = 1f;
                            playerStatusUI.SetStatus(StatusEffectPlayer.DefenseUp, false);
                        }
                        float effectiveArmor = Mathf.Max(target.armor, 0);
                        float damage = Mathf.Max(rawDamage - effectiveArmor, 0);
                        target.currentHealth -= damage;
                        if (enemyCursedLinkActive)
                        {
                            playerStats.currentHealth -= damage;
                            Debug.Log("Cursed Link echoes the pain to the enemy!");
                        }
                        target.armor = Mathf.Max(target.armor - 10, 0);
                        dealer.armor += armorGain;
                        flavorTextUI.ShowPlayText($"The player charged at the enemy with their shield! Enemy takes {damage} damage and defense raised by {armorGain}!");
                        Debug.Log($"Player gains {armorGain} armor and Enemy takes {damage} damage!");
                    }
                    break;

                case PlayerCardType.OverheadSlash:
                    {
                        float rawDamage = cardData.effectAmountAttack;
                        float selfDamage = cardData.effectAmountAttackOvertime;
                        if (playerAttackMultiplierActive)
                        {
                            rawDamage *= playerAttackMultiplier;
                            selfDamage *= playerAttackMultiplier;
                            playerAttackMultiplierActive = false;
                            playerAttackMultiplier = 1f;
                            playerStatusUI.SetStatus(StatusEffectPlayer.AttackUp, false);
                        }

                        float effectiveArmor = Mathf.Max(target.armor, 0);
                        float damage = Mathf.Max(rawDamage - effectiveArmor, 0);
                        target.armor = Mathf.Max(target.armor - 10, 0);
                        bool riposteTriggered = false;

                        if (enemyRiposteActive)
                        {
                            float blocked = damage * riposteblock;
                            float finalDamage = damage - blocked;
                            float riposteDamage = damage * ripostereturn;

                            target.currentHealth -= finalDamage;
                            dealer.currentHealth -= riposteDamage;
                            enemyStatusUI.SetStatus(StatusEffectEnemy.DefenseUp, false);
                            if (playerCursedLinkActive)
                            {
                                enemyStats.currentHealth -= riposteDamage;
                                Debug.Log("Cursed Link echoes the pain to the enemy!");
                            }
                            enemyRiposteActive = false;
                            riposteTriggered = true;

                            flavorTextUI.ShowPlayText("The enemy deflects your strike and counters viciously!");
                        }
                        else
                        { target.currentHealth -= damage; }
                        if (enemyCursedLinkActive)
                        {
                            playerStats.currentHealth -= damage;
                            Debug.Log("Cursed Link echoes the pain to the enemy!");
                        }
                        if (!riposteTriggered && target.armor > 0)
                        { target.armor -= 10; }
                        { dealer.currentHealth -= selfDamage; }
                        flavorTextUI.ShowPlayText($"The player swung their sword at the enemy and dealt {damage} damage, but received {selfDamage} damage!");
                        Debug.Log($"Player attacks Enemy for {damage}!");
                        break;
                    }

                case PlayerCardType.VampiricBlade:
                    {
                        float rawDamage = cardData.effectAmountAttack;
                        float selfHeal = cardData.effectAmountHeal;
                        if (playerAttackMultiplierActive)
                        {
                            rawDamage *= playerAttackMultiplier;
                            selfHeal *= playerAttackMultiplier;
                            playerAttackMultiplierActive = false;
                            playerAttackMultiplier = 1f;
                            playerStatusUI.SetStatus(StatusEffectPlayer.AttackUp, false);
                        }
                        float armorBeforeHit = target.armor;
                        float effectiveArmor = Mathf.Max(armorBeforeHit, 0);
                        float damage = Mathf.Max(rawDamage - effectiveArmor, 0);
                        bool riposteTriggered = false;
                        float actualHeal = 0f;
                        if (enemyRiposteActive)
                        {
                            float blocked = damage * riposteblock;
                            float finalDamage = damage - blocked;
                            float riposteDamage = damage * ripostereturn;

                            target.currentHealth -= finalDamage;
                            dealer.currentHealth -= riposteDamage;
                            enemyStatusUI.SetStatus(StatusEffectEnemy.DefenseUp, false);
                            if (playerCursedLinkActive)
                            {
                                enemyStats.currentHealth -= riposteDamage;
                                Debug.Log("Cursed Link echoes the pain to the enemy!");
                            }

                            enemyRiposteActive = false;
                            riposteTriggered = true;

                            flavorTextUI.ShowPlayText("The enemy deflects your strike and counters viciously!");
                        }
                        else
                        {
                            target.currentHealth -= damage;
                            if (enemyCursedLinkActive)
                            {
                                playerStats.currentHealth -= damage;
                                Debug.Log("Cursed Link echoes the pain to the enemy!");
                            }
                        }
                        if (!riposteTriggered && armorBeforeHit > 0)
                        {
                            target.armor = Mathf.Max(target.armor - 8, 0);
                        }
                        if (armorBeforeHit <= 0 && damage > 0 && !riposteTriggered)
                        {
                            float oldHealth = dealer.currentHealth;
                            dealer.currentHealth = Mathf.Min(dealer.currentHealth + selfHeal, dealer.maxHealth);
                            actualHeal = dealer.currentHealth - oldHealth;
                        }
                        if (actualHeal > 0)
                        {
                            flavorTextUI.ShowPlayText($"The player swung their sword at the enemy and dealt {damage} damage, draining {actualHeal} Health!");
                        }
                        else
                        {
                            flavorTextUI.ShowPlayText($"The player swung their sword at the enemy and dealt {damage} damage!");
                        }
                        Debug.Log($"Player attacks Enemy for {damage} and heals {actualHeal}!");
                    }
                    break;

                case PlayerCardType.BlessedBroadsword:
                    {
                        float rawDamage = cardData.effectAmountAttack;
                        if (playerAttackMultiplierActive)
                        {
                            rawDamage *= playerAttackMultiplier;
                            playerAttackMultiplierActive = false;
                            playerAttackMultiplier = 1f;
                            playerStatusUI.SetStatus(StatusEffectPlayer.AttackUp, false);
                        }

                        float effectiveArmor = Mathf.Max(target.armor, 0);
                        float damage = Mathf.Max(rawDamage - effectiveArmor, 0);
                        target.armor = Mathf.Max(target.armor - 10, 0);
                        bool riposteTriggered = false;

                        if (enemyRiposteActive)
                        {
                            float blocked = damage * riposteblock;
                            float finalDamage = damage - blocked;
                            float riposteDamage = damage * ripostereturn;

                            target.currentHealth -= finalDamage;
                            dealer.currentHealth -= riposteDamage;
                            enemyStatusUI.SetStatus(StatusEffectEnemy.DefenseUp, false);
                            if (playerCursedLinkActive)
                            {
                                enemyStats.currentHealth -= riposteDamage;
                                Debug.Log("Cursed Link echoes the pain to the enemy!");
                            }
                            enemyRiposteActive = false;
                            riposteTriggered = true;

                            flavorTextUI.ShowPlayText("The enemy deflects your strike and counters viciously!");
                        }
                        else
                        { target.currentHealth -= damage; }
                        if (enemyCursedLinkActive)
                        {
                            playerStats.currentHealth -= damage;
                            Debug.Log("Cursed Link echoes the pain to the enemy!");
                        }
                        if (!riposteTriggered && target.armor > 0)
                        { target.armor -= 10; }

                        if (enemyAttackMultiplierActive)
                        {
                            enemyAttackMultiplierActive = false;
                            enemyStatusUI.SetStatus(StatusEffectEnemy.AttackUp, false);
                            flavorTextUI.ShowPlayText($"The player swung their broadsword at the enemy and dealt {damage} damage, removing their attack buff!");
                        }
                        else if (enemyDefenseMultiplierActive)
                        {
                            enemyDefenseMultiplierActive = false;
                            enemyStatusUI.SetStatus(StatusEffectEnemy.DefenseUp, false);
                            flavorTextUI.ShowPlayText($"The player swung their broadsword at the enemy and dealt {damage} damage, removing their defense buff!");
                        }
                        else
                            flavorTextUI.ShowPlayText($"The player swung their sword at the enemy and dealt {damage} damage!");
                        Debug.Log($"Player attacks Enemy for {damage}!");
                    }
                    break;

                case PlayerCardType.HolyBarrier:
                    if (playerDefenseMultiplierActive)
                    {
                        dealer.armor = Mathf.Max(dealer.armor, cardData.effectAmountDefend * playerDefenseMultiplier);
                        playerHolyBarrierActive = true;
                        playerDefenseMultiplierActive = false;
                        playerStatusUI.SetStatus(StatusEffectPlayer.DefenseUp, false);
                    }
                    else
                    {
                        dealer.armor = Mathf.Max(dealer.armor, cardData.effectAmountDefend);
                        playerHolyBarrierActive = true;
                    }
                    holyBarrierTurnsRemaining = holyBarrierDurationTurns;
                    flavorTextUI.ShowPlayText("The player raises a holy barrier and is immune to status effects next turn!");
                    playerStatusUI.SetStatus(StatusEffectPlayer.Protected, true);
                    Debug.Log($"Player gains {cardData.effectAmountDefend} armor!");
                    break;

                case PlayerCardType.CursedLink:
                    {
                        playerCursedLinkActive = true;
                        cursedLinkTurnsRemaining = 2;
                        flavorTextUI.ShowPlayText("Your soul binds with the enemy — pain shall be shared!");
                        enemyStatusUI.SetStatus(StatusEffectEnemy.Linked, true);
                        break;
                    }

                case PlayerCardType.FateCoin:
                    {
                        bool goodOutcome = Random.value < 0.5f;
                        float fateBleedDamage = cardData.effectAmountAttackOvertime;
                        int bleedDuration = 2;
                        if (goodOutcome)
                        {
                            playerAttackMultiplier = 1.5f;
                            playerDefenseMultiplier = 1.5f;
                            playerAttackMultiplierActive = true;
                            playerDefenseMultiplierActive = true;
                            attackMultiplierPotionTurnsRemaining = 2;
                            blacksmithAnvilTurnsRemaining = 2;
                            ClearAllStatusEffects(dealer);
                            playerStatusUI.SetStatus(StatusEffectPlayer.AttackUp, true);
                            playerStatusUI.SetStatus(StatusEffectPlayer.DefenseUp, true);
                            flavorTextUI.ShowPlayText("Fate smiles upon you! Power and protection surge through your body!");
                            Debug.Log("Fate's Coin: Good outcome");
                        }
                        else
                        {
                            playerAttackMultiplier = 0.5f;
                            playerDefenseMultiplier = 0.5f;
                            playerAttackMultiplierActive = true;
                            playerDefenseMultiplierActive = true;
                            attackMultiplierPotionTurnsRemaining = 2;
                            blacksmithAnvilTurnsRemaining = 2;
                            if (activeBlood.ContainsKey(playerStats) && playerHolyBarrierActive == false)
                            {
                                activeBlood[playerStats].damagePerTurn += fateBleedDamage;
                                activeBlood[playerStats].turnsRemaining = Mathf.Max(activeBlood[playerStats].turnsRemaining, bleedDuration);
                            }
                            else if (playerHolyBarrierActive == false)
                            {
                                activeBlood[playerStats] = new BloodEffect(fateBleedDamage, bleedDuration);
                            }
                            playerStatusUI.SetStatus(StatusEffectPlayer.AttackUp, true);
                            playerStatusUI.SetStatus(StatusEffectPlayer.DefenseUp, true);
                            flavorTextUI.ShowPlayText("Fate turns against you! Weakness seeps in as blood spills...");
                            Debug.Log("Fate's Coin: Bad outcome");
                        }
                        break;
                    }

            }
        }

        else if (dealer.characterType == Statistics.CharacterType.Enemy)
        {
            switch (cardData.enemyCardType)
            {
                case EnemyCardType.Attack:
                    {
                        float rawDamage = cardData.effectAmountAttack;
                        if (enemyAttackMultiplierActive)
                        {
                            rawDamage *= enemyAttackMultiplier;
                            enemyAttackMultiplierActive = false;
                            enemyAttackMultiplier = 1f;
                            enemyStatusUI.SetStatus(StatusEffectEnemy.AttackUp, false);
                        }

                        float effectiveArmor = Mathf.Max(target.armor, 0);
                        float damage = Mathf.Max(rawDamage - effectiveArmor, 0);
                        target.armor = Mathf.Max(target.armor - 10, 0);
                        bool riposteTriggered = false;

                        if (playerRiposteActive)
                        {
                            float blocked = damage * riposteblock;
                            float finalDamage = damage - blocked;
                            float riposteDamage = damage * ripostereturn;
                            playerStatusUI.SetStatus(StatusEffectPlayer.DefenseUp, false);

                            target.currentHealth -= finalDamage;
                            dealer.currentHealth -= riposteDamage;
                            if (enemyCursedLinkActive)
                            {
                                playerStats.currentHealth -= riposteDamage;
                                Debug.Log("Cursed Link echoes the pain to the player!");
                            }
                            playerRiposteActive = false;
                            riposteTriggered = true;

                            flavorTextUI.ShowPlayText("The player deflects the enemy's strike and counters viciously!");
                        }
                        else
                        { target.currentHealth -= damage; }
                        if (playerCursedLinkActive)
                        {
                            enemyStats.currentHealth -= damage;
                            Debug.Log("Cursed Link echoes the pain to the enemy!");
                        }
                        if (!riposteTriggered && target.armor > 0)
                        { target.armor -= 12; }
                        Debug.Log($"Enemy attacks player for {damage}!");
                        break;
                    }

                case EnemyCardType.Defend:
                    if (enemyDefenseMultiplierActive)
                    {
                        dealer.armor = Mathf.Max(dealer.armor, cardData.effectAmountDefend * enemyDefenseMultiplier);
                        enemyDefenseMultiplierActive = false;
                        enemyStatusUI.SetStatus(StatusEffectEnemy.DefenseUp, false);
                    }
                    else
                    {
                        dealer.armor = Mathf.Max(dealer.armor, cardData.effectAmountDefend);
                    }
                    Debug.Log($"Enemy gains {cardData.effectAmountDefend} armor!");
                    break;

                case EnemyCardType.Heal:
                    dealer.currentHealth = Mathf.Min(dealer.currentHealth + cardData.effectAmountHeal, dealer.maxHealth);
                    Debug.Log($"Enemy heals for {cardData.effectAmountHeal}!");
                    break;

                case EnemyCardType.ManaRefill:
                    dealer.currentPP = Mathf.Min(dealer.currentPP + cardData.effectAmountMana, dealer.maxPP);
                    flavorTextUI.ShowPlayText($"{dealer.characterType} replenished Mana for {cardData.effectAmountMana}!");
                    Debug.Log($"Enemy replenished Mana for {cardData.effectAmountMana}!");
                    break;

                case EnemyCardType.Greatersword:
                    {
                        float rawDamage = cardData.effectAmountAttack;
                        if (enemyAttackMultiplierActive)
                        {
                            rawDamage *= enemyAttackMultiplier;
                            enemyAttackMultiplierActive = false;
                            enemyAttackMultiplier = 1f;
                            enemyStatusUI.SetStatus(StatusEffectEnemy.AttackUp, false);
                        }

                        float effectiveArmor = Mathf.Max(target.armor, 0);
                        float damage = Mathf.Max(rawDamage - effectiveArmor, 0);
                        target.armor = Mathf.Max(target.armor - 10, 0);
                        bool riposteTriggered = false;

                        if (playerRiposteActive)
                        {
                            float blocked = damage * riposteblock;
                            float finalDamage = damage - blocked;
                            float riposteDamage = damage * ripostereturn;
                            playerStatusUI.SetStatus(StatusEffectPlayer.DefenseUp, false);

                            target.currentHealth -= finalDamage;
                            dealer.currentHealth -= riposteDamage;
                            if (enemyCursedLinkActive)
                            {
                                playerStats.currentHealth -= riposteDamage;
                                Debug.Log("Cursed Link echoes the pain to the player!");
                            }
                            playerRiposteActive = false;
                            riposteTriggered = true;

                            flavorTextUI.ShowPlayText("The player deflects the enemy's strike and counters viciously!");
                        }
                        else
                        { target.currentHealth -= damage; }
                        if (playerCursedLinkActive)
                        {
                            enemyStats.currentHealth -= damage;
                            Debug.Log("Cursed Link echoes the pain to the enemy!");
                        }
                        if (!riposteTriggered && target.armor > 0)
                        { target.armor -= 12; }
                        Debug.Log($"Enemy strikes player for {damage}!");
                        break;
                    }

                case EnemyCardType.Greatershield:
                    if (enemyDefenseMultiplierActive)
                    {
                        dealer.armor = Mathf.Max(dealer.armor, cardData.effectAmountDefend * enemyDefenseMultiplier);
                        enemyDefenseMultiplierActive = false;
                        enemyStatusUI.SetStatus(StatusEffectEnemy.DefenseUp, false);
                    }
                    else
                    {
                        dealer.armor = Mathf.Max(dealer.armor, cardData.effectAmountDefend);
                    }
                    Debug.Log($"Enemy's defense greatly increased by {cardData.effectAmountDefend}!");
                    break;

                case EnemyCardType.DivineIntervention:
                    enemyDivineTurnsRemaining = divineDurationTurns;
                    enemyStatusUI.SetStatus(StatusEffectEnemy.Protected, true);
                    Debug.Log("Enemy invokes Divine Intervention!");
                    break;

                case EnemyCardType.Steal:
                    {
                        List<StealTarget> targets = new();

                        AttackData playerHeal = playerCards
                            .Find(c => c.GetCardData()?.playerCardType == PlayerCardType.Heal && playerCooldowns.IsReady(c.GetCardData()))
                            ?.GetCardData();
                        AttackData playerMana = playerCards
                            .Find(c => c.GetCardData()?.playerCardType == PlayerCardType.ManaRefill && playerCooldowns.IsReady(c.GetCardData()))
                            ?.GetCardData();
                        AttackData playerAttackMultiplier = playerCards
                            .Find(c => c.GetCardData()?.playerCardType == PlayerCardType.AttackMultiplier && playerCooldowns.IsReady(c.GetCardData()))
                            ?.GetCardData();
                        AttackData playerPoisonPotion = playerCards
                            .Find(c => c.GetCardData()?.playerCardType == PlayerCardType.PoisonPotion && playerCooldowns.IsReady(c.GetCardData()))
                            ?.GetCardData();
                        AttackData playerParalysisPotion = playerCards
                            .Find(c => c.GetCardData()?.playerCardType == PlayerCardType.ParalysisPotion && playerCooldowns.IsReady(c.GetCardData()))
                            ?.GetCardData();
                        AttackData playerFirePotion = playerCards
                            .Find(c => c.GetCardData()?.playerCardType == PlayerCardType.FirePotion && playerCooldowns.IsReady(c.GetCardData()))
                            ?.GetCardData();
                        AttackData playerLightningPotion = playerCards
                            .Find(c => c.GetCardData()?.playerCardType == PlayerCardType.LightningPotion && playerCooldowns.IsReady(c.GetCardData()))
                            ?.GetCardData();

                        if (playerHeal != null)
                            targets.Add(new StealTarget(StealTargetType.Heal, 1f));

                        if (playerMana != null)
                            targets.Add(new StealTarget(StealTargetType.Mana, 1f));

                        if (playerAttackMultiplier != null)
                            targets.Add(new StealTarget(StealTargetType.AttackMultiplier, 1f));

                        if (playerPoisonPotion != null)
                            targets.Add(new StealTarget(StealTargetType.PoisonPotion, 1f));

                        if (playerParalysisPotion != null)
                            targets.Add(new StealTarget(StealTargetType.ParalysisPotion, 1f));

                        if (playerFirePotion != null)
                            targets.Add(new StealTarget(StealTargetType.FirePotion, 1f));

                        if (playerLightningPotion != null)
                            targets.Add(new StealTarget(StealTargetType.LightningPotion, 1f));

                        if (targets.Count == 0)
                        {
                            flavorTextUI.ShowPlayText($"{dealer.characterType} tried to steal, but found nothing!");
                            break;
                        }

                        float baseWeight = 100f / targets.Count;
                        foreach (var t in targets)
                            t.weight = baseWeight;

                        bool lowHP = enemyStats.HealthPercent <= 0.3f;
                        bool lowMana = enemyStats.ppPercent <= 0.3f;

                        if (lowHP)
                            AdjustWeights(targets, StealTargetType.Heal, 7.5f);

                        if (lowMana)
                            AdjustWeights(targets, StealTargetType.Mana, 7.5f);

                        StealTarget chosen = PickStealTarget(targets);

                        switch (chosen.type)
                        {
                            case StealTargetType.Heal:
                                playerCooldowns.SetCooldown(playerHeal, playerHeal.playerCooldownTurns);
                                dealer.currentHealth = Mathf.Min(dealer.currentHealth + 12f, dealer.maxHealth);
                                flavorTextUI.ShowPlayText($"{dealer.characterType} stole your Heal potion!");
                                break;

                            case StealTargetType.Mana:
                                playerCooldowns.SetCooldown(playerMana, playerMana.playerCooldownTurns);
                                dealer.currentPP = Mathf.Min(dealer.currentPP + 7.5f, dealer.maxPP);
                                flavorTextUI.ShowPlayText($"{dealer.characterType} stole your Mana potion!");
                                break;

                            case StealTargetType.AttackMultiplier:
                                playerAttackMultiplierActive = false;
                                enemyAttackMultiplier = 2f;
                                enemyAttackMultiplierActive = true;
                                playerCooldowns.SetCooldown(playerAttackMultiplier, playerAttackMultiplier.playerCooldownTurns);
                                enemyStatusUI.SetStatus(StatusEffectEnemy.AttackUp, true);
                                flavorTextUI.ShowPlayText($"{dealer.characterType} stole your attack buff!");
                                break;

                            case StealTargetType.PoisonPotion:
                                float stealPImpactDamage = cardData.effectAmountAttack;
                                dealer.currentHealth -= stealPImpactDamage;
                                float impactDamage = cardData.effectAmountAttack;
                                dealer.currentHealth -= impactDamage;
                                float poisonDamage = 4.5f;
                                int poisonDuration = 3;
                                playerCooldowns.SetCooldown(playerPoisonPotion, playerPoisonPotion.playerCooldownTurns);
                                if (activePoisons.ContainsKey(dealer))
                                {
                                    activePoisons[dealer].damagePerTurn += poisonDamage;
                                    activePoisons[dealer].turnsRemaining = Mathf.Max(activePoisons[dealer].turnsRemaining, poisonDuration);
                                }
                                else
                                {
                                    activePoisons[dealer] = new PoisonEffect(poisonDamage, poisonDuration);
                                }
                                enemyStatusUI.SetStatus(StatusEffectEnemy.Poisoned, true);
                                flavorTextUI.ShowPlayText($"{dealer.characterType} stole a poison potion and it backfires on them!");
                                break;

                            case StealTargetType.FirePotion:
                                float stealFImpactDamage = cardData.effectAmountAttack;
                                dealer.currentHealth -= stealFImpactDamage;
                                float fireDamage = 3f;
                                int fireDuration = 3;
                                playerCooldowns.SetCooldown(playerFirePotion, playerFirePotion.playerCooldownTurns);
                                if (activeFire.ContainsKey(dealer))
                                {
                                    activeFire[dealer].damagePerTurn += fireDamage;
                                    activeFire[dealer].turnsRemaining = Mathf.Max(activeFire[dealer].turnsRemaining, fireDuration);
                                }
                                else
                                {
                                    activeFire[dealer] = new FireEffect(fireDamage, fireDuration);
                                }
                                enemyStatusUI.SetStatus(StatusEffectEnemy.Burning, true);
                                flavorTextUI.ShowPlayText($"{dealer.characterType} stole a fire potion and it backfires on them!");
                                break;

                            case StealTargetType.LightningPotion:
                                float stealLImpactDamage = cardData.effectAmountAttack;
                                dealer.currentHealth -= stealLImpactDamage;
                                float lightningDamage = 5f;
                                int lightningDuration = 3;
                                playerCooldowns.SetCooldown(playerLightningPotion, playerLightningPotion.playerCooldownTurns);
                                if (activeThunder.ContainsKey(dealer))
                                {
                                    activeThunder[dealer].damagePerTurn += lightningDamage;
                                    activeThunder[dealer].turnsRemaining = Mathf.Max(activeThunder[dealer].turnsRemaining, lightningDuration);
                                }
                                else
                                {
                                    activeThunder[dealer] = new ThunderEffect(lightningDamage, lightningDuration);
                                }
                                enemyStatusUI.SetStatus(StatusEffectEnemy.Electrocuted, true);
                                flavorTextUI.ShowPlayText($"{dealer.characterType} stole a lightning potion and it backfires on them!");
                                break;
                        }
                        break;
                    }


                case EnemyCardType.CallOfNature:
                    float natureDamage = Mathf.Max(cardData.effectAmountAttack - target.armor, 0);
                    target.currentHealth -= natureDamage;
                    if (playerCursedLinkActive)
                    {
                        enemyStats.currentHealth -= natureDamage;
                        Debug.Log("Cursed Link echoes the pain to the enemy!");
                    }
                    if (target.armor > 0) { target.armor -= 11.5f; }
                    float bloodDamage = cardData.effectAmountAttackOvertime;
                    int bloodDuration = 1;
                    if (activeBlood.ContainsKey(playerStats) && playerHolyBarrierActive == false)
                    {
                        activeBlood[playerStats].damagePerTurn += bloodDamage;
                        activeBlood[playerStats].turnsRemaining = Mathf.Max(activeBlood[playerStats].turnsRemaining, bloodDuration);
                    }
                    else if (playerHolyBarrierActive == false)
                    {
                        activeBlood[playerStats] = new BloodEffect(bloodDamage, bloodDuration);
                    }
                    flavorTextUI.ShowPlayText($"{dealer.characterType} unleashes nature's wrath on Player for {natureDamage}!");
                    playerStatusUI.SetStatus(StatusEffectPlayer.Bleeding, true);
                    Debug.Log($"Enemy unleashes nature's wrath on Player for {natureDamage}!");
                    break;

                case EnemyCardType.AttackMultiplier:
                    enemyAttackMultiplier = 2f;
                    enemyAttackMultiplierActive = true;
                    enemyAttackMultiplierPotionTurnsRemaining = attackMultiplierDurationTurns;
                    flavorTextUI.ShowPlayText($"{dealer.characterType} drank an invigorating potion and doubled their melee damage for the next turn!");
                    enemyStatusUI.SetStatus(StatusEffectEnemy.AttackUp, true);
                    Debug.Log("Enemy's next attack damage is doubled!");
                    break;

                case EnemyCardType.BlacksmithAnvil:
                    enemyDefenseMultiplier = 2f;
                    enemyDefenseMultiplierActive = true;
                    enemyBlacksmithAnvilTurnsRemaining = blacksmithAnvilDurationTurns;
                    flavorTextUI.ShowPlayText($"{dealer.characterType} hardened their shield with the anvil and doubled their defense for the next turn!");
                    enemyStatusUI.SetStatus(StatusEffectEnemy.DefenseUp, true);
                    Debug.Log("Enemy's next defense is doubled!");
                    break;

                case EnemyCardType.PoisonPotion:
                    {
                        float impactDamage = cardData.effectAmountAttack;
                        target.currentHealth -= impactDamage;
                        if (playerCursedLinkActive)
                        {
                            enemyStats.currentHealth -= impactDamage;
                            Debug.Log("Cursed Link echoes the pain to the enemy!");
                        }
                        float poisonDamage = cardData.effectAmountAttackOvertime;
                        int poisonDuration = 3;
                        if (activePoisons.ContainsKey(playerStats) && playerHolyBarrierActive == false)
                        {
                            activePoisons[playerStats].damagePerTurn += poisonDamage;
                            activePoisons[playerStats].turnsRemaining = Mathf.Max(activePoisons[playerStats].turnsRemaining, poisonDuration);
                        }
                        else if (playerHolyBarrierActive == false)
                        {
                            activePoisons[playerStats] = new PoisonEffect(poisonDamage, poisonDuration);
                        }
                        if (cardData.hasFlavortext == true)
                        { flavorTextUI.ShowPlayText($"{dealer.characterType} threw a poison potion at you!"); }
                        playerStatusUI.SetStatus(StatusEffectPlayer.Poisoned, true);
                        Debug.Log($"Player poisoned for {poisonDamage} damage per turn for {poisonDuration} turns.");
                    }
                    break;

                case EnemyCardType.RestorationSpell:
                    dealer.currentHealth = Mathf.Min(dealer.currentHealth + cardData.effectAmountHeal, dealer.maxHealth);
                    flavorTextUI.ShowPlayText($"{dealer.characterType} used a spell to gain {cardData.effectAmountHeal} HP!");
                    break;

                case EnemyCardType.KnowledgeTome:
                    dealer.currentPP = Mathf.Min(dealer.currentPP + cardData.effectAmountMana, dealer.maxPP);
                    flavorTextUI.ShowPlayText($"{dealer.characterType} restored a massive amount of Mana for {cardData.effectAmountMana}!");
                    Debug.Log($"{dealer.characterType} restored a massive amount of Mana for {cardData.effectAmountMana}!");
                    break;

                case EnemyCardType.BrimstoneRain:
                    {
                        float impactDamage = Mathf.Max(cardData.effectAmountAttack - target.armor, 0);
                        target.currentHealth -= impactDamage;
                        if (playerCursedLinkActive)
                        {
                            enemyStats.currentHealth -= impactDamage;
                            Debug.Log("Cursed Link echoes the pain to the enemy!");
                        }
                        if (target.armor > 0) { target.armor -= 10; }
                        float fireDamage = cardData.effectAmountAttackOvertime;
                        int fireDuration = 4;
                        if (activeFire.ContainsKey(playerStats) && playerHolyBarrierActive == false)
                        {
                            activeFire[playerStats].damagePerTurn += fireDamage;
                            activeFire[playerStats].turnsRemaining = Mathf.Max(activeFire[playerStats].turnsRemaining, fireDuration);
                        }
                        else if (playerHolyBarrierActive == false)
                        {
                            activeFire[playerStats] = new FireEffect(fireDamage, fireDuration);
                        }
                        if (cardData.hasFlavortext == true)
                        { flavorTextUI.ShowPlayText($"{dealer.characterType} rained down fire on you!"); }
                        playerStatusUI.SetStatus(StatusEffectPlayer.Burning, true);
                        Debug.Log($"Player on fire for {fireDamage} damage per turn for {fireDuration} turns.");
                    }
                    break;

                case EnemyCardType.ZeusWrath:
                    {
                        float impactDamage = Mathf.Max(cardData.effectAmountAttack - target.armor, 0);
                        target.currentHealth -= impactDamage;
                        if (playerCursedLinkActive)
                        {
                            enemyStats.currentHealth -= impactDamage;
                            Debug.Log("Cursed Link echoes the pain to the enemy!");
                        }
                        if (target.armor > 0) { target.armor -= 10; }
                        float thunderDamage = cardData.effectAmountAttackOvertime;
                        int thunderDuration = 3;
                        if (activeThunder.ContainsKey(playerStats) && playerHolyBarrierActive == false)
                        {
                            activeThunder[playerStats].damagePerTurn += thunderDamage;
                            activeThunder[playerStats].turnsRemaining = Mathf.Max(activeThunder[playerStats].turnsRemaining, thunderDuration);
                        }
                        else if (playerHolyBarrierActive == false)
                        {
                            activeThunder[playerStats] = new ThunderEffect(thunderDamage, thunderDuration);
                        }
                        flavorTextUI.ShowPlayText($"{dealer.characterType} smited you with the power of Zeus!");
                        playerStatusUI.SetStatus(StatusEffectPlayer.Electrocuted, true);
                        Debug.Log($"Player was electrocuted for {thunderDamage} damage per turn for {thunderDuration} turns.");
                    }
                    break;

                case EnemyCardType.BleedStrike:
                    float bleedDamage = Mathf.Max(cardData.effectAmountAttack - target.armor, 0);
                    target.currentHealth -= bleedDamage;
                    if (playerCursedLinkActive)
                    {
                        enemyStats.currentHealth -= bleedDamage;
                        Debug.Log("Cursed Link echoes the pain to the enemy!");
                    }
                    if (target.armor > 0) { target.armor -= 6.5f; }
                    float bleedingDamage = cardData.effectAmountAttackOvertime;
                    int bleedingDuration = 2;
                    if (activeBlood.ContainsKey(playerStats) && playerHolyBarrierActive == false)
                    {
                        activeBlood[playerStats].damagePerTurn += bleedingDamage;
                        activeBlood[playerStats].turnsRemaining = Mathf.Max(activeBlood[playerStats].turnsRemaining, bleedingDuration);
                    }
                    else if (playerHolyBarrierActive == false)
                    {
                        activeBlood[playerStats] = new BloodEffect(bleedingDamage, bleedingDuration);
                    }
                    playerStatusUI.SetStatus(StatusEffectPlayer.Bleeding, true);
                    Debug.Log($"Enemy's sharp edge causes {bleedDamage} damage!");
                    break;

                case EnemyCardType.ParalysisPotion:
                    {
                        float impactDamage = Mathf.Max(cardData.effectAmountAttack - target.armor, 0);
                        target.currentHealth -= impactDamage;
                        if (playerCursedLinkActive)
                        {
                            enemyStats.currentHealth -= impactDamage;
                            Debug.Log("Cursed Link echoes the pain to the enemy!");
                        }
                        int paralysisTurns = Mathf.RoundToInt(cardData.effectAmountTurnSkip);
                        if (playerHolyBarrierActive == false)
                        { ApplyParalysis(playerStats, paralysisTurns); }
                        if (cardData.hasFlavortext == true)
                        {
                            if (playerHolyBarrierActive == true)
                            { flavorTextUI.ShowPlayText($"The Enemy threw a Paralysis Potion at the player, but it failed!"); }
                            else
                            { flavorTextUI.ShowPlayText($"{dealer.characterType} threw a Paralysis Potion at the player! Player is paralyzed for {paralysisTurns} turn{(paralysisTurns > 1 ? "s" : "")}!"); }
                            playerStatusUI.SetStatus(StatusEffectPlayer.Frozen, true);
                        }
                        Debug.Log($"Player paralyzed for {paralysisTurns} turns.");
                    }
                    break;

                case EnemyCardType.FirePotion:
                    {
                        float impactDamage = cardData.effectAmountAttack;
                        target.currentHealth -= impactDamage;
                        if (playerCursedLinkActive)
                        {
                            enemyStats.currentHealth -= impactDamage;
                            Debug.Log("Cursed Link echoes the pain to the enemy!");
                        }
                        float fireDamage = cardData.effectAmountAttackOvertime;
                        int fireDuration = 3;
                        if (activeFire.ContainsKey(playerStats) && playerHolyBarrierActive == false)
                        {
                            activeFire[playerStats].damagePerTurn += fireDamage;
                            activeFire[playerStats].turnsRemaining = Mathf.Max(activeFire[playerStats].turnsRemaining, fireDuration);
                        }
                        else if (playerHolyBarrierActive == false)
                        {
                            activeFire[playerStats] = new FireEffect(fireDamage, fireDuration);
                        }
                        flavorTextUI.ShowPlayText($"{dealer.characterType} threw a fire potion at you!");
                        playerStatusUI.SetStatus(StatusEffectPlayer.Burning, true);
                        Debug.Log($"Player enflamed for {fireDamage} damage per turn for {fireDuration} turns.");
                    }
                    break;

                case EnemyCardType.LightningPotion:
                    {
                        float impactDamage = cardData.effectAmountAttack;
                        target.currentHealth -= impactDamage;
                        if (playerCursedLinkActive)
                        {
                            enemyStats.currentHealth -= impactDamage;
                            Debug.Log("Cursed Link echoes the pain to the enemy!");
                        }
                        float thunderDamage = cardData.effectAmountAttackOvertime;
                        int thunderDuration = 2;
                        if (activeThunder.ContainsKey(playerStats) && playerHolyBarrierActive == false)
                        {
                            activeThunder[playerStats].damagePerTurn += thunderDamage;
                            activeThunder[playerStats].turnsRemaining = Mathf.Max(activeThunder[playerStats].turnsRemaining, thunderDuration);
                        }
                        else if (playerHolyBarrierActive == false)
                        {
                            activeThunder[playerStats] = new ThunderEffect(thunderDamage, thunderDuration);
                        }
                        flavorTextUI.ShowPlayText($"{dealer.characterType} threw a lightning potion at you!");
                        playerStatusUI.SetStatus(StatusEffectPlayer.Electrocuted, true);
                        Debug.Log($"Player electrocuted for {thunderDamage} damage per turn for {thunderDuration} turns.");
                        break;
                    }

                case EnemyCardType.ThiefSmokeBomb:
                    {
                        int paralysisTurns = Mathf.RoundToInt(cardData.effectAmountTurnSkip);
                        enemyAttackMultiplier = 1.5f;
                        enemyAttackMultiplierActive = true;
                        ApplyParalysis(playerStats, paralysisTurns);
                        flavorTextUI.ShowPlayText($"{dealer.characterType} threw a smoke bomb at the player! {dealer.characterType} gets lucky strike next turn and player is stunned for {paralysisTurns} turn{(paralysisTurns > 1 ? "s" : "")}!");
                        playerStatusUI.SetStatus(StatusEffectPlayer.Frozen, true);
                        Debug.Log($"Player stunned for {paralysisTurns} turns.");
                        break;
                    }

                case EnemyCardType.RefreshBreeze:
                    {
                        dealer.armor = Mathf.Max(dealer.armor, cardData.effectAmountDefend);
                        flavorTextUI.ShowPlayText($"{dealer.characterType} summoned a gust of wind and cleansed their afflictions!");
                        ClearAllStatusEffects(dealer);
                        Debug.Log($"{dealer.characterType} gains {cardData.effectAmountDefend} armor!");
                        break;
                    }

                case EnemyCardType.Riposte:
                    {
                        enemyRiposteActive = true;
                        flavorTextUI.ShowPlayText("The enemy takes a steady and defensive stance, waiting to counter the player's strike.");
                        enemyStatusUI.SetStatus(StatusEffectEnemy.DefenseUp, true);
                        break;
                    }

                case EnemyCardType.EssenceLife:
                    {
                        if (dealer.currentPP < cardData.manaCost)
                        {
                            break;
                        }
                        dealer.currentHealth = Mathf.Min(dealer.currentHealth + cardData.effectAmountHeal, dealer.maxHealth);
                        flavorTextUI.ShowPlayText(
                            $"{dealer.characterType} transmuted mind into life and restored {cardData.effectAmountHeal} HP!"
                        );
                        Debug.Log(
                            $"{dealer.characterType} converted {cardData.manaCost} mana into {cardData.effectAmountHeal} HP."
                        );
                        break;
                    }

                case EnemyCardType.EssenceMind:
                    {
                        if (dealer.currentHealth < 25)
                        {
                            break;
                        }
                        dealer.currentPP = Mathf.Min(dealer.currentPP + cardData.effectAmountMana, dealer.maxPP);
                        flavorTextUI.ShowPlayText(
                            $"{dealer.characterType} transmuted life into mind and restored {cardData.effectAmountMana} Mana!"
                        );
                        Debug.Log(
                            $"{dealer.characterType} converted {cardData.healthCost} HP into {cardData.effectAmountMana} Mana."
                        );
                        break;
                    }

                case EnemyCardType.PoisonDart:
                    {
                        float impactDamage = cardData.effectAmountAttack;
                        target.currentHealth -= impactDamage;
                        if (playerCursedLinkActive)
                        {
                            enemyStats.currentHealth -= impactDamage;
                            Debug.Log("Cursed Link echoes the pain to the enemy!");
                        }
                        float poisonDamage = cardData.effectAmountAttackOvertime;
                        int poisonDuration = 2;
                        if (activePoisons.ContainsKey(playerStats) && playerHolyBarrierActive == false)
                        {
                            activePoisons[playerStats].damagePerTurn += poisonDamage;
                            activePoisons[playerStats].turnsRemaining = Mathf.Max(activePoisons[playerStats].turnsRemaining, poisonDuration);
                        }
                        else if (playerHolyBarrierActive == false)
                        {
                            activePoisons[playerStats] = new PoisonEffect(poisonDamage, poisonDuration);
                        }
                        flavorTextUI.ShowPlayText($"{dealer.characterType} shot a poison dart at you!");
                        playerStatusUI.SetStatus(StatusEffectPlayer.Poisoned, true);
                        Debug.Log($"Player poisoned for {poisonDamage} damage per turn for {poisonDuration} turns.");
                    }
                    break;

                case EnemyCardType.ShieldBash:
                    {
                        float rawDamage = cardData.effectAmountAttack;
                        float armorGain = cardData.effectAmountDefend;
                        if (enemyAttackMultiplierActive)
                        {
                            rawDamage *= enemyAttackMultiplier;
                            enemyAttackMultiplierActive = false;
                            enemyAttackMultiplier = 1f;
                            enemyStatusUI.SetStatus(StatusEffectEnemy.AttackUp, false);
                        }
                        if (enemyDefenseMultiplierActive)
                        {
                            armorGain *= enemyDefenseMultiplier;
                            enemyDefenseMultiplierActive = false;
                            enemyDefenseMultiplier = 1f;
                            enemyStatusUI.SetStatus(StatusEffectEnemy.DefenseUp, false);
                        }
                        float effectiveArmor = Mathf.Max(target.armor, 0);
                        float damage = Mathf.Max(rawDamage - effectiveArmor, 0);
                        target.currentHealth -= damage;
                        if (playerCursedLinkActive)
                        {
                            enemyStats.currentHealth -= damage;
                            Debug.Log("Cursed Link echoes the pain to the enemy!");
                        }
                        target.armor = Mathf.Max(target.armor - 10, 0);
                        dealer.armor += armorGain;
                        flavorTextUI.ShowPlayText($"{dealer.characterType} charged at the player with their shield! Player takes {damage} damage and defense raised by {armorGain}!");
                        Debug.Log($"Enemy gains {armorGain} armor and player takes {damage} damage!");
                    }
                    break;

                case EnemyCardType.OverheadSlash:
                    {
                        float rawDamage = cardData.effectAmountAttack;
                        float selfDamage = cardData.effectAmountAttackOvertime;
                        if (enemyAttackMultiplierActive)
                        {
                            rawDamage *= enemyAttackMultiplier;
                            selfDamage *= enemyAttackMultiplier;
                            enemyAttackMultiplierActive = false;
                            enemyAttackMultiplier = 1f;
                            enemyStatusUI.SetStatus(StatusEffectEnemy.AttackUp, false);
                        }

                        float effectiveArmor = Mathf.Max(target.armor, 0);
                        float damage = Mathf.Max(rawDamage - effectiveArmor, 0);
                        target.armor = Mathf.Max(target.armor - 10, 0);
                        bool riposteTriggered = false;

                        if (playerRiposteActive)
                        {
                            float blocked = damage * riposteblock;
                            float finalDamage = damage - blocked;
                            float riposteDamage = damage * ripostereturn;
                            playerStatusUI.SetStatus(StatusEffectPlayer.DefenseUp, false);

                            target.currentHealth -= finalDamage;
                            dealer.currentHealth -= riposteDamage;
                            if (enemyCursedLinkActive)
                            {
                                playerStats.currentHealth -= riposteDamage;
                                Debug.Log("Cursed Link echoes the pain to the player!");
                            }
                            playerRiposteActive = false;
                            riposteTriggered = true;

                            flavorTextUI.ShowPlayText("The player deflects the enemy's strike and counters viciously!");
                        }
                        else
                        { target.currentHealth -= damage; }
                        if (playerCursedLinkActive)
                        {
                            enemyStats.currentHealth -= damage;
                            Debug.Log("Cursed Link echoes the pain to the enemy!");
                        }
                        if (!riposteTriggered && target.armor > 0)
                        { target.armor -= 10; }
                        { dealer.currentHealth -= selfDamage; }
                        flavorTextUI.ShowPlayText($"The enemy swung their sword at the player and dealt {damage} damage, but received {selfDamage} damage!");
                        Debug.Log($"Enemy attacks Player for {damage}!");
                        break;
                    }

                case EnemyCardType.VampiricBlade:
                    {
                        float rawDamage = cardData.effectAmountAttack;
                        float selfHeal = cardData.effectAmountHeal;
                        if (enemyAttackMultiplierActive)
                        {
                            rawDamage *= enemyAttackMultiplier;
                            selfHeal *= enemyAttackMultiplier;
                            enemyAttackMultiplierActive = false;
                            enemyAttackMultiplier = 1f;
                            enemyStatusUI.SetStatus(StatusEffectEnemy.AttackUp, false);
                        }
                        float armorBeforeHit = target.armor;
                        float effectiveArmor = Mathf.Max(armorBeforeHit, 0);
                        float damage = Mathf.Max(rawDamage - effectiveArmor, 0);
                        bool riposteTriggered = false;
                        float actualHeal = 0f;
                        if (playerRiposteActive)
                        {
                            float blocked = damage * riposteblock;
                            float finalDamage = damage - blocked;
                            float riposteDamage = damage * ripostereturn;
                            playerStatusUI.SetStatus(StatusEffectPlayer.DefenseUp, false);

                            target.currentHealth -= finalDamage;
                            dealer.currentHealth -= riposteDamage;
                            if (enemyCursedLinkActive)
                            {
                                playerStats.currentHealth -= riposteDamage;
                                Debug.Log("Cursed Link echoes the pain to the player!");
                            }
                            playerRiposteActive = false;
                            riposteTriggered = true;

                            flavorTextUI.ShowPlayText("The player deflects the enemy's strike and counters viciously!");
                        }
                        else
                        {
                            target.currentHealth -= damage;
                            if (playerCursedLinkActive)
                            {
                                enemyStats.currentHealth -= damage;
                                Debug.Log("Cursed Link echoes the pain to the enemy!");
                            }
                        }
                        if (!riposteTriggered && armorBeforeHit > 0)
                        {
                            target.armor = Mathf.Max(target.armor - 8, 0);
                        }
                        if (armorBeforeHit <= 0 && damage > 0 && !riposteTriggered)
                        {
                            float oldHealth = dealer.currentHealth;
                            dealer.currentHealth = Mathf.Min(dealer.currentHealth + selfHeal, dealer.maxHealth);
                            actualHeal = dealer.currentHealth - oldHealth;
                        }
                        if (actualHeal > 0)
                        {
                            flavorTextUI.ShowPlayText($"The enemy swung their sword at the player and dealt {damage} damage, draining {actualHeal} Health!");
                        }
                        else
                        {
                            flavorTextUI.ShowPlayText($"The enemy swung their sword at the player and dealt {damage} damage!");
                        }
                        Debug.Log($"Enemy attacks player for {damage} and heals {actualHeal}!");
                    }
                    break;

                case EnemyCardType.BlessedBroadsword:
                    {
                        float rawDamage = cardData.effectAmountAttack;
                        if (enemyAttackMultiplierActive)
                        {
                            rawDamage *= enemyAttackMultiplier;
                            enemyAttackMultiplierActive = false;
                            enemyAttackMultiplier = 1f;
                            enemyStatusUI.SetStatus(StatusEffectEnemy.AttackUp, false);
                        }

                        float effectiveArmor = Mathf.Max(target.armor, 0);
                        float damage = Mathf.Max(rawDamage - effectiveArmor, 0);
                        target.armor = Mathf.Max(target.armor - 10, 0);
                        bool riposteTriggered = false;

                        if (playerRiposteActive)
                        {
                            float blocked = damage * riposteblock;
                            float finalDamage = damage - blocked;
                            float riposteDamage = damage * ripostereturn;
                            playerStatusUI.SetStatus(StatusEffectPlayer.DefenseUp, false);

                            target.currentHealth -= finalDamage;
                            dealer.currentHealth -= riposteDamage;
                            if (enemyCursedLinkActive)
                            {
                                playerStats.currentHealth -= riposteDamage;
                                Debug.Log("Cursed Link echoes the pain to the player!");
                            }
                            playerRiposteActive = false;
                            riposteTriggered = true;

                            flavorTextUI.ShowPlayText("The player deflects the enemy's strike and counters viciously!");
                        }
                        else
                        { target.currentHealth -= damage; }
                        if (playerCursedLinkActive)
                        {
                            enemyStats.currentHealth -= damage;
                            Debug.Log("Cursed Link echoes the pain to the enemy!");
                        }
                        if (!riposteTriggered && target.armor > 0)
                        { target.armor -= 10; }

                        if (playerAttackMultiplierActive)
                        {
                            playerAttackMultiplierActive = false;
                            playerStatusUI.SetStatus(StatusEffectPlayer.AttackUp, false);
                            flavorTextUI.ShowPlayText($"{dealer.characterType} swung their broadsword at the player and dealt {damage} damage, removing their attack buff!");
                        }
                        else if (playerDefenseMultiplierActive)
                        {
                            playerDefenseMultiplierActive = false;
                            playerStatusUI.SetStatus(StatusEffectPlayer.DefenseUp, false);
                            flavorTextUI.ShowPlayText($"{dealer.characterType} swung their broadsword at the player and dealt {damage} damage, removing their defense buff!");
                        }
                        else
                            flavorTextUI.ShowPlayText($"The enemy swung their sword at the player and dealt {damage} damage!");
                        Debug.Log($"Enemy attacks player for {damage}!");
                    }
                    break;

                case EnemyCardType.HolyBarrier:
                    if (enemyDefenseMultiplierActive)
                    {
                        dealer.armor = Mathf.Max(dealer.armor, cardData.effectAmountDefend * enemyDefenseMultiplier);
                        enemyHolyBarrierActive = true;
                        enemyDefenseMultiplierActive = false;
                        enemyStatusUI.SetStatus(StatusEffectEnemy.DefenseUp, false);
                    }
                    else
                    {
                        dealer.armor = Mathf.Max(dealer.armor, cardData.effectAmountDefend);
                        enemyHolyBarrierActive = true;
                    }
                    enemyHolyBarrierTurnsRemaining = holyBarrierDurationTurns;
                    flavorTextUI.ShowPlayText($"{dealer.characterType} raises a holy barrier and is immune to status effects next turn!");
                    enemyStatusUI.SetStatus(StatusEffectEnemy.Protected, true);
                    Debug.Log($"Enemy gains {cardData.effectAmountDefend} armor!");
                    break;

                case EnemyCardType.CursedLink:
                    {
                        enemyCursedLinkActive = true;
                        playerStatusUI.SetStatus(StatusEffectPlayer.Linked, true);
                        enemyCursedLinkTurnsRemaining = 2;
                        flavorTextUI.ShowPlayText("The enemy's soul binds with you — pain shall be shared!");
                        break;
                    }

                case EnemyCardType.FateCoin:
                    {
                        bool goodOutcome = Random.value < 0.5f;
                        float fateBleedDamage = cardData.effectAmountAttackOvertime;
                        int bleedDuration = 2;
                        if (goodOutcome)
                        {
                            enemyAttackMultiplier = 1.5f;
                            enemyDefenseMultiplier = 1.5f;
                            enemyAttackMultiplierActive = true;
                            enemyDefenseMultiplierActive = true;
                            enemyAttackMultiplierPotionTurnsRemaining = 2;
                            enemyBlacksmithAnvilTurnsRemaining = 2;
                            ClearAllStatusEffects(dealer);
                            enemyStatusUI.SetStatus(StatusEffectEnemy.AttackUp, true);
                            enemyStatusUI.SetStatus(StatusEffectEnemy.DefenseUp, true);
                            flavorTextUI.ShowPlayText($"Fate smiles upon {dealer.characterType}! Power and protection surge through your body!");
                            Debug.Log("Fate's Coin: Good outcome");
                        }
                        else
                        {
                            enemyAttackMultiplier = 0.5f;
                            enemyDefenseMultiplier = 0.5f;
                            enemyAttackMultiplierActive = true;
                            enemyDefenseMultiplierActive = true;
                            enemyAttackMultiplierPotionTurnsRemaining = 2;
                            enemyBlacksmithAnvilTurnsRemaining = 2;
                            if (activeBlood.ContainsKey(enemyStats) && playerHolyBarrierActive == false)
                            {
                                activeBlood[enemyStats].damagePerTurn += fateBleedDamage;
                                activeBlood[enemyStats].turnsRemaining = Mathf.Max(activeBlood[enemyStats].turnsRemaining, bleedDuration);
                            }
                            else if (enemyHolyBarrierActive == false)
                            {
                                activeBlood[enemyStats] = new BloodEffect(fateBleedDamage, bleedDuration);
                            }
                            enemyStatusUI.SetStatus(StatusEffectEnemy.AttackUp, true);
                            enemyStatusUI.SetStatus(StatusEffectEnemy.DefenseUp, true);
                            flavorTextUI.ShowPlayText($"Fate turns against {dealer.characterType}! Weakness seeps in as blood spills...");
                            Debug.Log("Fate's Coin: Bad outcome");
                        }
                        break;
                    }
                }
            }

            dealer.currentHealth = Mathf.Clamp(dealer.currentHealth, 0, dealer.maxHealth);
            target.currentHealth = Mathf.Clamp(target.currentHealth, 0, target.maxHealth);

            if (dealer.currentHealth <= 0)
            {
                StartCoroutine(ProcessDeath(dealer));
            }
            else if (target.currentHealth <= 0)
            {
                StartCoroutine(ProcessDeath(target));
            }
        }

    private void AdjustWeights(List<StealTarget> targets, StealTargetType boosted, float bonus)
    {
        StealTarget boostTarget = targets.Find(t => t.type == boosted);
        if (boostTarget == null) return;

        boostTarget.weight += bonus;

        float detract = bonus / (targets.Count - 1);

        foreach (var t in targets)
        {
            if (t.type != boosted)
                t.weight = Mathf.Max(0f, t.weight - detract);
        }
    }
    public void StartNextBattle()
    {
        StartCoroutine(NextBattleRoutine());
    }

    private void ApplyPoisonDamage(Statistics target)
    {
        if (!activePoisons.ContainsKey(target)) return;

        PoisonEffect effect = activePoisons[target];

        target.currentHealth -= effect.damagePerTurn;
        if (playerCursedLinkActive && target == playerStats)
        {
            enemyStats.currentHealth -= effect.damagePerTurn;
            Debug.Log("Cursed Link echoes the pain to the enemy!");
        }
        if (enemyCursedLinkActive)
        {
            playerStats.currentHealth -= effect.damagePerTurn;
            Debug.Log("Cursed Link echoes the pain to the player!");
        }
        Debug.Log($"{target.characterType} takes {effect.damagePerTurn} poison damage! Turns left: {effect.turnsRemaining - 1}");
        effect.turnsRemaining--;
        if (effect.turnsRemaining <= 0)
        {
            if (target.characterType == Statistics.CharacterType.Player)
            {
                playerStatusUI.SetStatus(StatusEffectPlayer.Poisoned, false);
            }
            else
            {
                enemyStatusUI.SetStatus(StatusEffectEnemy.Poisoned, false);
            }
            activePoisons.Remove(target);
            Debug.Log($"{target.characterType} is no longer poisoned.");
        }
    }

    private void ApplyFireDamage(Statistics target)
    {
        if (!activeFire.ContainsKey(target)) return;

        FireEffect effect = activeFire[target];

        target.currentHealth -= effect.damagePerTurn;
        if (playerCursedLinkActive && target == playerStats)
        {
            enemyStats.currentHealth -= effect.damagePerTurn;
            Debug.Log("Cursed Link echoes the pain to the enemy!");
        }
        if (enemyCursedLinkActive)
        {
            playerStats.currentHealth -= effect.damagePerTurn;
            Debug.Log("Cursed Link echoes the pain to the player!");
        }
        Debug.Log($"{target.characterType} takes {effect.damagePerTurn} fire damage! Turns left: {effect.turnsRemaining - 1}");

        effect.turnsRemaining--;

        if (effect.turnsRemaining <= 0)
        {
            if (target.characterType == Statistics.CharacterType.Player)
            {
                playerStatusUI.SetStatus(StatusEffectPlayer.Burning, false);
            }
            else
            {
                enemyStatusUI.SetStatus(StatusEffectEnemy.Burning, false);
            }
            activeFire.Remove(target);
            Debug.Log($"{target.characterType} is no longer on fire.");
        }
    }

    private void ApplyThunderDamage(Statistics target)
    {
        if (!activeThunder.ContainsKey(target)) return;

        ThunderEffect effect = activeThunder[target];

        target.currentHealth -= effect.damagePerTurn;
        if (playerCursedLinkActive && target == playerStats)
        {
            enemyStats.currentHealth -= effect.damagePerTurn;
            Debug.Log("Cursed Link echoes the pain to the enemy!");
        }
        if (enemyCursedLinkActive)
        {
            playerStats.currentHealth -= effect.damagePerTurn;
            Debug.Log("Cursed Link echoes the pain to the player!");
        }
        Debug.Log($"{target.characterType} takes {effect.damagePerTurn} electric damage! Turns left: {effect.turnsRemaining - 1}");

        effect.turnsRemaining--;

        if (effect.turnsRemaining <= 0)
        {
            if (target.characterType == Statistics.CharacterType.Player)
            {
                playerStatusUI.SetStatus(StatusEffectPlayer.Electrocuted, false);
            }
            else
            {
                enemyStatusUI.SetStatus(StatusEffectEnemy.Electrocuted, false);
            }
            activeThunder.Remove(target);
            Debug.Log($"{target.characterType} is no longer electrocuted.");
        }
    }

    private void ApplyBloodDamage(Statistics target)
    {
        if (!activeBlood.ContainsKey(target)) return;

        BloodEffect effect = activeBlood[target];

        target.currentHealth -= effect.damagePerTurn;
        if (playerCursedLinkActive && target == playerStats)
        {
            enemyStats.currentHealth -= effect.damagePerTurn;
            Debug.Log("Cursed Link echoes the pain to the enemy!");
        }
        if (enemyCursedLinkActive)
        {
            playerStats.currentHealth -= effect.damagePerTurn;
            Debug.Log("Cursed Link echoes the pain to the player!");
        }
        Debug.Log($"{target.characterType} lost blood and takes {effect.damagePerTurn} damage! Turns left: {effect.turnsRemaining - 1}");

        effect.turnsRemaining--;

        if (effect.turnsRemaining <= 0)
        {
            if (target.characterType == Statistics.CharacterType.Player)
            {
                playerStatusUI.SetStatus(StatusEffectPlayer.Bleeding, false);
            }
            else
            {
                enemyStatusUI.SetStatus(StatusEffectEnemy.Bleeding, false);
            }
            activeBlood.Remove(target);
            Debug.Log($"{target.characterType} is no longer bleeding.");
        }
    }

    private void ApplyParalysis(Statistics target, int turns)
    {
        if (activeParalysis.ContainsKey(target))
            activeParalysis[target] += turns;
        else
            activeParalysis[target] = turns;
    }

    private bool IsParalyzed(Statistics target)
    {
        return activeParalysis.ContainsKey(target) && activeParalysis[target] > 0;
    }

    private void ConsumeParalysisTurn(Statistics target)
    {
        if (!activeParalysis.ContainsKey(target)) return;

        activeParalysis[target]--;

        if (activeParalysis[target] <= 0)
        {
            if (target.characterType == Statistics.CharacterType.Player)
            {
                playerStatusUI.SetStatus(StatusEffectPlayer.Frozen, false);
            }
            else
            {
                enemyStatusUI.SetStatus(StatusEffectEnemy.Frozen, false);
            }
            activeParalysis.Remove(target);
            Debug.Log($"{target.characterType} is no longer stuck in place.");
        }
    }

    private void ClearAllStatusEffects(Statistics target)
    {
        activePoisons.Remove(target);
        activeFire.Remove(target);
        activeThunder.Remove(target);
        activeBlood.Remove(target);
        activeParalysis.Remove(target);
        var ui = GetUI(target);
        if (target.characterType == Statistics.CharacterType.Player)
        {
            foreach (StatusEffectPlayer effect in System.Enum.GetValues(typeof(StatusEffectPlayer)))
            { ui.SetStatus(effect, false); }
        }
        else if (target.characterType == Statistics.CharacterType.Enemy)
        {
            foreach (StatusEffectEnemy effect in System.Enum.GetValues(typeof(StatusEffectEnemy)))
            { ui.SetStatus(effect, false); }
        }
        Debug.Log($"{target.characterType} had all status effects cleansed!");
    }
    public void ClearAllEnemyCombatState()
    {
        activePoisons.Clear();
        activeFire.Clear();
        activeThunder.Clear();
        activeBlood.Clear();
        activeParalysis.Clear();

        enemyAttackMultiplier = 1f;
        enemyDefenseMultiplier = 1f;
        enemyRiposteActive = false;
        enemyHolyBarrierActive = false;
        enemyCursedLinkActive = false;

        enemyDivineTurnsRemaining = 0;
        enemyHolyBarrierTurnsRemaining = 0;
        enemyBlacksmithAnvilTurnsRemaining = 0;
        enemyAttackMultiplierPotionTurnsRemaining = 0;
        enemyCursedLinkTurnsRemaining = 0;
    }

    private StatusEffectUI GetUI(Statistics stats)
    {
        return stats.characterType == Statistics.CharacterType.Player
            ? playerStatusUI
            : enemyStatusUI;
    }

    private IEnumerator ProcessDeath(Statistics character)
    {
        isProcessingDeath = true;
        flavorTextUI.ShowPlayText($"{character.characterType} has been defeated!");
        yield return new WaitForSeconds(1f);
        HandleDeath(character);
        isProcessingDeath = false;
    }
    private GameObject PickEnemyPrefab()
    {
        List<GameObject> pool = new();
        int level = playerStats.playerStats.currentLevel;

        if (ShouldSpawnBoss())
        {
            pool.AddRange(bossEnemies);
        }
        else
        {
            if (level >= 1) pool.AddRange(level1Enemies);
            if (level >= 2) pool.AddRange(level2Enemies);
            if (level >= 3) pool.AddRange(level3Enemies);
            if (level >= 4) pool.AddRange(level4Enemies);
        }

        if (pool.Count == 0)
        {
            Debug.LogError("Enemy pool is empty!");
            return null;
        }

        float totalWeight = 0f;
        foreach (var enemy in pool)
            totalWeight += enemySpawnWeights[enemy];

        float roll = Random.Range(0f, totalWeight);
        float running = 0f;

        foreach (var enemy in pool)
        {
            running += enemySpawnWeights[enemy];
            if (roll <= running)
            {
                AdjustEnemySpawnWeights(enemy);
                return enemy;
            }
        }

        return pool[0];
    }

    private void HandleDeath(Statistics character)
    {
        if (character.characterType == Statistics.CharacterType.Player)
        {
            if (gameIsOver) return;

            gameIsOver = true;
            flavorTextUI.ShowPlayText("You were defeated...");

            StartCoroutine(PlayerDeathSequence());
        }
        else if (character.characterType == Statistics.CharacterType.Enemy)
        {
            flavorTextUI.ShowPlayText("You have defeated the enemy!");
            enemyAI.StartCoroutine(enemyAI.Die());

            consecutiveEnemiesDefeated++;

            if (consecutiveEnemiesDefeated >= 5)
            {
                consecutiveEnemiesDefeated = 0;
                StartCoroutine(GoToDeckBuilder());
            }
            else
            {
                StartNextBattle();
            }
        }
    }

    private IEnumerator GoToDeckBuilder()
    {
        currentTurn = TurnState.Busy;
        yield return fade.ImageFadeInFlash();
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene("Deck Builder");
    }

    private IEnumerator NextBattleRoutine()
    {
        currentTurn = TurnState.Busy;
        yield return fade.ImageFadeInFlash();
        ResetPlayer();
        StartCoroutine(SpawnNewEnemy());
        flavorTextUI.ShowPlayText("Your turn.");
        yield return new WaitForSeconds(0.25f);
        yield return fade.ImageFadeOutFlash();
        StartPlayerTurn();
    }
    private void ResetPlayer()
    {
        playerStats.currentHealth = playerStats.maxHealth;
        playerStats.currentPP = playerStats.maxPP;
        playerCooldowns = new CooldownTracker();
        ClearAllStatusEffects(playerStats);
        playerAttackMultiplier = 1f;
        playerDefenseMultiplier = 1f;
        divineTurnsRemaining = 0;
        holyBarrierTurnsRemaining = 0;
        blacksmithAnvilTurnsRemaining = 0;
        attackMultiplierPotionTurnsRemaining = 0;
        turnCounter = 0;
    }

    IEnumerator SpawnNewEnemy()
    {
        ClearAllEnemyCombatState();
        ClearAllStatusEffects(enemyStats);

        if (enemyAI != null)
            Destroy(enemyAI.gameObject);

        GameObject enemyPrefab = PickEnemyPrefab();
        if (enemyPrefab == null)
            yield break;

        GameObject enemyGO = Instantiate(
            enemyPrefab,
            enemySpawnPoint.position,
            Quaternion.identity
        );
        enemyAI = enemyGO.GetComponent<EnemyAI>();
        enemyStats = enemyGO.GetComponent<Statistics>();
        yield return null;
        enemyCooldowns = new CooldownTracker();
        enemyStats.currentHealth = enemyStats.maxHealth;
        enemyStats.currentPP = enemyStats.maxPP;
        foreach (var card in enemyAI.enemyCards)
        enemyCooldowns.Register(card);
        AutoAssignStatistics();
        enemyMusicTrigger.StopMusic();
        yield return null;
        enemyMusicTrigger.EnemyFindSequence();
        enemyBackgroundTrigger.EnemyFindSequence();
        enemyStats.BindEnemyUI();
    }

    public void RegisterCards()
    {
        playerCards = new List<Attack>(FindObjectsByType<Attack>(FindObjectsSortMode.None));

        foreach (var card in playerCards)
        {
            if (card == null) continue;

            AttackData data = card.GetCardData();
            if (data == null)
            {
                Debug.LogError($"Card '{card.name}' is missing CardData!");
                continue;
            }

            playerCooldowns.Register(data);
        }
        foreach (var card in enemyAI.enemyCards)
            enemyCooldowns.Register(card);
    }

    private bool CheckForDeathFromStatus(Statistics target)
    {
        if (target.currentHealth > 0)
            return false;

        if (!isProcessingDeath)
        {
            StartCoroutine(ProcessDeath(target));
        }

        return true;
    }
    private IEnumerator PlayerDeathSequence()
    {
        currentTurn = TurnState.Busy;
        foreach (var card in playerCards)
        {
            card.DisableCard();
            card.ForceShowBack();
        }
        yield return new WaitForSeconds(1.25f);
        yield return fade.ImageFadeInFlash();
        yield return new WaitForSeconds(0.25f);
        SceneManager.LoadScene("Title Screen");
    }

    public void ForfeitPlayerTurn()
    {
        if (currentTurn != TurnState.PlayerTurn) return;
        if (gameIsOver) return;
        flavorTextUI.ShowPlayText("You forfeit your turn.");
        AdvancePlayerCooldownsOnly();
        StartCoroutine(EndPlayerTurn());
    }

    private void AdvancePlayerCooldownsOnly()
    {
        turnCounter++;
        playerCooldowns.Tick();

        foreach (var card in playerCards)
        {
            if (card == null) continue;

            int cd = playerCooldowns.GetCooldown(card.GetCardData());
            card.SetCooldownVisual(cd);
        }
    }
    private void InitializeEnemyWeights()
    {
        enemySpawnWeights.Clear();

        void AddEnemies(List<GameObject> list)
        {
            foreach (var enemy in list)
            {
                if (!enemySpawnWeights.ContainsKey(enemy))
                    enemySpawnWeights.Add(enemy, enemyPickWeightMax);
            }
        }

        AddEnemies(level1Enemies);
        AddEnemies(level2Enemies);
        AddEnemies(level3Enemies);
        AddEnemies(level4Enemies);
        AddEnemies(bossEnemies);
    }

    private void AdjustEnemySpawnWeights(GameObject spawnedEnemy)
    {
        var keys = new List<GameObject>(enemySpawnWeights.Keys);

        foreach (var enemy in keys)
        {
            if (enemy == spawnedEnemy)
            {
                enemySpawnWeights[enemy] = Mathf.Max(
                    enemyPickWeightMin,
                    enemySpawnWeights[enemy] - weightRemoved
                );
            }
            else
            {
                enemySpawnWeights[enemy] = Mathf.Min(
                    enemyPickWeightMax,
                    enemySpawnWeights[enemy] + weightRecover
                );
            }
        }
    }
}