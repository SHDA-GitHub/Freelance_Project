using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OverworldBattleHandler : MonoBehaviour
{
    private void OnEnable()
    {
        StartCoroutine(HandleBattleResult());
    }

    private IEnumerator HandleBattleResult()
    {
        yield return null;

        yield return new WaitUntil(() => BattleTransitionManager.Instance != null);

        CheckBattleResult();
    }

    public void ProcessBattleResultDirectly()
    {
        CheckBattleResult();
    }

    private void CheckBattleResult()
    {
        if (!BattleResultBridge.HasResult)
            return;

        if (BattleResultBridge.BattleWon)
        {
            if (BattleTransitionManager.Instance != null)
                BattleTransitionManager.Instance.ResetBattleTransitionForOverworld();

            Debug.Log($"Battle Won! Total EXP: {BattleResultBridge.TotalEXP}");
        }
        else
        {
            Debug.Log("Battle Lost! Reloading Overworld...");
            UnityEngine.SceneManagement.SceneManager.LoadScene("Overworld");
        }

        BattleResultBridge.ResetBridge();
    }
}