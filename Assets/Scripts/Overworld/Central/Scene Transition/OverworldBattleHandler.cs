using UnityEngine;

public class OverworldBattleHandler : MonoBehaviour
{
    private static bool hasCheckedBattle = false;

    private void OnEnable()
    {
        if (!hasCheckedBattle)
        {
            hasCheckedBattle = true;
            CheckBattleResult();
        }
    }

    private void CheckBattleResult()
    {
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

        BattleResultBridge.BattleWon = false;
        BattleResultBridge.TotalEXP = 0;
        BattleResultBridge.BattleOutcomeText = "";
    }
}