using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStatsSO", menuName = "RPG/PlayerStats")]
public class PlayerStatsSO : ScriptableObject
{
    public string characterName;
    public int maxHealth;
    public int currentHealth;
    public int maxPP;
    public int currentPP;
    public int level;
    public int currentEXP;

    public void GainEXP(int amount)
    {
        currentEXP += amount;
        int expToNext = level * 50;
        while (currentEXP >= expToNext)
        {
            currentEXP -= expToNext;
            LevelUp();
            expToNext = level * 50;
        }
    }

    void LevelUp()
    {
        level++;
        currentEXP = 0;
        maxHealth += 2;
        maxPP += 1;
        currentHealth = maxHealth;
        currentPP = maxPP;
    }
}