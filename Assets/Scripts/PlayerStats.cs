using UnityEngine;

[CreateAssetMenu(menuName = "Player/Player Stats")]
public class PlayerStats : ScriptableObject
{
    [Header("Health")]
    public float currentHealth;
    public float maxHealth;

    [Header("Mana")]
    public float currentMana;
    public float maxMana;

    [Header("Progression")]
    public int currentLevel;
    public float currentEXP;
    public float EXPThreshold;
    public int enemiesSlain;

    public void ResetStats()
    {
        currentLevel = 1;
        currentEXP = 0;
        EXPThreshold = 500f;
        enemiesSlain = 0;

        maxHealth = 100;
        currentHealth = maxHealth;

        maxMana = 15;
        currentMana = maxMana;
    }
}