using UnityEngine;

public static class BattleResultBridge
{
    public static bool HasResult = false;
    public static bool BattleWon = false;
    public static int TotalEXP = 0;
    public static string BattleOutcomeText = "";

    public static void ResetBridge()
    {
        HasResult = false;
        BattleWon = false;
        TotalEXP = 0;
        BattleOutcomeText = "";
    }
}