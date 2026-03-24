using UnityEngine;

public static class BattleResultBridge
{
    public static bool BattleWon = false;
    public static int TotalEXP = 0;
    public static string BattleOutcomeText = "";

    public static void ResetBridge()
    {
        BattleWon = false;
        TotalEXP = 0;
        BattleOutcomeText = "";
    }
}