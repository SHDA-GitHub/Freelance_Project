using System.Collections.Generic;
using UnityEngine;

public class EnemyMusicTrigger : MonoBehaviour
{
    [SerializeField] private EnemyAI enemyAI;
    [SerializeField] private List<EnemyLoadoutMusic> loadoutMusic;
    [SerializeField] private MusicManager musicManager;

    private void Start()
    {
        EnemyFindSequence();
    }

    public void EnemyFindSequence()
    {
        enemyAI = FindFirstObjectByType<EnemyAI>();
        StartMusic(enemyAI);
    }

    public void StartMusic(EnemyAI enemyAI)
    {
        if (!enemyAI || !enemyAI.loadout)
            return;

        foreach (var entry in loadoutMusic)
        {
            if (entry.loadout == enemyAI.loadout)
            {
                musicManager.PlayEnemyMusic(entry.musicClip);
                return;
            }
        }
    }

    public void StopMusic()
    {
        musicManager.StopMusic();
    }
}