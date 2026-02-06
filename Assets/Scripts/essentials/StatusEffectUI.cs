using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static GameManager;

public class StatusEffectUI : MonoBehaviour
{
    [System.Serializable]
    public class playerStatusIcon
    {
        public StatusEffectPlayer playerEffect;
        public Image playerIcon;
    }
    [System.Serializable]
    public class enemyStatusIcon
    {
        public StatusEffectEnemy enemyEffect;
        public Image enemyIcon;
    }

    [SerializeField] private List<playerStatusIcon> playerIcon;
    private Dictionary<StatusEffectPlayer, Image> playerIconMap;

    [SerializeField] private List<enemyStatusIcon> enemyIcon;
    private Dictionary<StatusEffectEnemy, Image> enemyIconMap;

    private void Awake()
    {
        playerIconMap = new Dictionary<StatusEffectPlayer, Image>();  
        foreach (var i in playerIcon)
        {
            i.playerIcon.gameObject.SetActive(false);
            playerIconMap[i.playerEffect] = i.playerIcon;
        }

        enemyIconMap = new Dictionary<StatusEffectEnemy, Image>();
        foreach (var i in enemyIcon)
        {
            i.enemyIcon.gameObject.SetActive(false);
            enemyIconMap[i.enemyEffect] = i.enemyIcon;
        }
    }

    public void SetStatus(StatusEffectPlayer effect, bool active)
    {
        if (playerIconMap.TryGetValue(effect, out Image img))
            img.gameObject.SetActive(active);
    }

    public void SetStatus(StatusEffectEnemy effect, bool active)
    {
        if (enemyIconMap.TryGetValue(effect, out Image img))
            img.gameObject.SetActive(active);
    }
}
