using System.Collections.Generic;
using UnityEngine;

public class EnemyBackgroundTrigger : MonoBehaviour
{
    [SerializeField] private EnemyAI enemyAI;
    [SerializeField] private List<EnemyLoadoutBackground> loadoutBackgrounds;
    [SerializeField] private SpriteRenderer backgroundSpriteRenderer;

    private void Start()
    {
        EnemyFindSequence();
    }

    public void EnemyFindSequence()
    {
        enemyAI = FindFirstObjectByType<EnemyAI>();
        StartBackgroundChange(enemyAI);
    }

    public void StartBackgroundChange(EnemyAI enemyAI)
    {
        if (!enemyAI || !enemyAI.loadout)
            return;

        foreach (var entry in loadoutBackgrounds)
        {
            if (entry.loadout == enemyAI.loadout)
            {
                backgroundSpriteRenderer.sprite = entry.backgroundImage;
                return;
            }
        }
    }

    public void ResetBackground()
    {
        backgroundSpriteRenderer.sprite = null;
    }
}