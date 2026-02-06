using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class Statistics : MonoBehaviour
{
    public Dictionary<string, float> ShieldMaxValues = new Dictionary<string, float>()
    {
        { "Defend", 7.5f },
        { "Greatershield", 12f },
        { "Woodenshield", 3f },
        { "Rogueshield", 5f },
        { "Trickyrogueshield", 6f },
    };

    [Header("Persistent Player Data")]
    public PlayerStats playerStats;

    public enum CharacterType
    {
        Player,
        Enemy
    }

    //public enum CharacterName
    //{
    //    AngeredBlacksmith,
    //    ArmoredOrc,
    //    Assassin,
    //    BattleCleric,
    //    Cavalry,
    //    CorruptedDruid,
    //    CostlyMimic,
    //    Cryomancer,
    //    DrunkenPeasant,
    //    FateTouchedHeretic,
    //    FireGolem,
    //    FlamebornDragon,
    //    FrostbornDragon,
    //    Goblin,
    //    GrandPriestess,
    //    HighMageOfFire,
    //    HighMageOfIce,
    //    HighMageOfNature,
    //    HighMageOfThunder,
    //    HighPriest,
    //    HostileMerchant,
    //    IronwallSentinel,
    //    Knight,
    //    LegendaryMimic,
    //    MightyArchmage,
    //    Mimic,
    //    Necromancer,
    //    OrcBoss,
    //    Paladin,
    //    Pyromancer,
    //    Rogue,
    //    RoyalGuardsman,
    //    SkeletonKnight,
    //    SneakyGoblin,
    //    TricksterRogue,
    //    TrickyBowman,
    //    VeteranDuellist,
    //    WarriorOrc,
    //}

    public enum Nametag
    {
        Goblin,
        Knight,
        Paladin,
        HostileMerchant,
        BattleCleric,
    }

    [Header("Identity")]
    public bool isEnemy = false;
    public CharacterType characterType;

    [Header("Health")]
    [Range(0, 500)] public float currentHealth = 100;
    public float maxHealth = 100;
    [Header("Mana")]
    [Range(0, 100)] public float currentPP = 15;
    public float maxPP = 15f;

    [Header("References")]
    [SerializeField] private EnemyAI enemyAI;

    [Header("Combat")]
    public float damage = 10;
    public float armor = 7.5f;
    public float maxArmor = 0f;
    public float healPower = 15;

    public float HealthPercent => currentHealth / maxHealth;
    public float ppPercent => currentPP / maxPP;

    [SerializeField] Image healthBar;
    [SerializeField] Image ghostBar;
    [SerializeField] private Image ppBar;
    [SerializeField] private Image ppGhostBar;

    float ghostTimer;
    private float ppGhostTimer;
    private void Start()
    {
        if (!isEnemy)
        {
            currentHealth = playerStats.currentHealth;
            maxHealth = playerStats.maxHealth;
            currentPP = playerStats.currentMana;
            maxPP = playerStats.maxMana;
        }
        else
        {
            currentHealth = maxHealth;
        }

        if (isEnemy)
            ApplyEnemyMaxArmor();
        else
            ApplyPlayerMaxArmor();
    }


    private void Awake()
    {
        if (characterType != CharacterType.Enemy)
            return;

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas not found!");
            return;
        }

        Transform enemyHealth = canvas.transform.Find("EnemyHealth");
        if (enemyHealth != null)
        {
            healthBar = enemyHealth.Find("Healthbar")?.GetComponent<Image>();
            ghostBar = enemyHealth.Find("Ghostbar")?.GetComponent<Image>();
        }

        Transform enemyPP = canvas.transform.Find("EnemyPP");
        if (enemyPP != null)
        {
            ppBar = enemyPP.Find("PPbar")?.GetComponent<Image>();
            ppGhostBar = enemyPP.Find("Ghostbar")?.GetComponent<Image>();
        }

        if (!healthBar || !ghostBar || !ppBar || !ppGhostBar)
        {
            Debug.LogWarning("One or more enemy UI Image references are missing!");
        }
    }

    private void Update()
    {
        if (ghostTimer > 0)
        {
            ghostTimer -= Time.deltaTime;
        }
        else
        {
            ghostBar.fillAmount = currentHealth / maxHealth;
        }

        healthBar.fillAmount = currentHealth / maxHealth;

        if (ppGhostTimer > 0)
        {
            ppGhostTimer -= Time.deltaTime;
        }
        else
        {
            ppGhostBar.fillAmount = currentPP / maxPP;
        }
        ppBar.fillAmount = currentPP / maxPP;
    }

    public void HealPlayer(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }
    private void ApplyPlayerMaxArmor()
    {
        List<Attack> allCards = GameManager.Instance != null ? GameManager.Instance.playerCards : null;

        if (allCards == null || allCards.Count == 0)
        {
            Debug.LogWarning("No player cards found in GameManager!");
            maxArmor = 0f;
            armor = maxArmor;
            return;
        }

        maxArmor = 0f;

        for (int i = 0; i < allCards.Count; i++)
        {
            Attack cardComponent = allCards[i];

            if (cardComponent != null && cardComponent.attackData != null)
            {
                string cardName = cardComponent.attackData.CardName;

                if (ShieldMaxValues.ContainsKey(cardName))
                {
                    maxArmor = Mathf.Max(maxArmor, ShieldMaxValues[cardName]);
                }
            }
        }
    }

    private void ApplyEnemyMaxArmor()
    {
        if (enemyAI == null || enemyAI.enemyAttacks == null)
        {
            Debug.LogWarning("EnemyAI or enemyCards not assigned!");
            return;
        }

        maxArmor = 0f;

        for (int i = 0; i < enemyAI.enemyAttacks.Count; i++)
        {
            AttackData card = enemyAI.enemyAttacks[i];
            if (card != null && ShieldMaxValues.ContainsKey(card.CardName))
            {
                maxArmor = Mathf.Max(maxArmor, ShieldMaxValues[card.CardName]);
            }
        }
    }

    public void DamagePlayer(float Amount)
    {
        ghostTimer = .7f;
        currentHealth -= Amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        if (currentHealth <= 0)
        {
            PlayerDeath();
        }
    }

    void PlayerDeath()
    {
        Time.timeScale = 0;
    }

    public void UsePP(float amount)
    {
        currentPP = Mathf.Max(currentPP - amount, 0);
        ppGhostTimer = 0.7f;
    }

    public void UseHP(float amount)
    {
        currentHealth = Mathf.Max(currentHealth - amount, 0);
        ghostTimer = 0.7f;
    }

    public void RestoreMana(float amount)
    {
        currentPP = Mathf.Min(currentPP + amount, maxPP);
    }

    public void AddEXP(float amount)
    {
        playerStats.currentEXP += amount;

        while (playerStats.currentEXP >= playerStats.EXPThreshold)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        playerStats.currentEXP -= playerStats.EXPThreshold;
        playerStats.currentLevel++;
        playerStats.EXPThreshold += 200f;

        Debug.Log($"Level Up! New Level: {playerStats.currentLevel}");

        playerStats.maxHealth += 8.5f;
        playerStats.currentHealth = playerStats.maxHealth;

        playerStats.maxMana += 1.5f;
        playerStats.currentMana = playerStats.maxMana;

        maxHealth = playerStats.maxHealth;
        currentHealth = playerStats.currentHealth;
        maxPP = playerStats.maxMana;
        currentPP = playerStats.currentMana;
    }
    public void RegisterEnemyKill()
    {
        playerStats.enemiesSlain++;
    }

    public void BindEnemyUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();

        Transform enemyHealth = canvas.transform.Find("EnemyHealth");
        healthBar = enemyHealth.Find("Healthbar").GetComponent<Image>();
        ghostBar = enemyHealth.Find("Ghostbar").GetComponent<Image>();

        Transform enemyMana = canvas.transform.Find("EnemyMana");
        ppBar = enemyMana.Find("Manabar").GetComponent<Image>();
        ppGhostBar = enemyMana.Find("Ghostbar").GetComponent<Image>();
    }
}
