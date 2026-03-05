using UnityEngine;

public enum BattleBackgroundType { Normal, Miniboss, Boss, MoonSoldier, FinalBoss }

public static class BattleDataBridge
{
        public static EnemyPreset UpcomingEnemyPreset;
        public static AudioClip BattleMusic;
        public static BattleBackgroundType BackgroundSelection;
}