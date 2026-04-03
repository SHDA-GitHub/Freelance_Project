using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStatsSO", menuName = "RPG/PlayerStats")]
public class PlayerStatsSO : ScriptableObject
{
    [Header("Stats")]
    public string characterName;
    public int maxHealth;
    public int currentHealth;
    public int maxPP;
    public int currentPP;
    public int level;
    public int currentEXP;
    [Header("Base Stats")]
    public int baseHealth;
    public int basePP;
    public int baseLevel;
    public int baseEXP;
    [SerializeField] private bool startWithBaseStats;

    public void Start()
    {
        if (startWithBaseStats == true)
        {
            currentHealth = baseHealth;
            maxHealth = baseHealth;
            currentPP = basePP;
            maxPP = basePP;
            level = baseLevel;
            currentEXP = baseEXP;
        }
    }

    public void OverworldAddHP(int healAmount)
    {
        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
    }

    public void OverworldAddPP(int restoreAmount)
    {
        currentPP = Mathf.Min(currentPP + restoreAmount, maxPP);
    }
}