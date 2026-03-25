using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum BattleBackgroundType { Normal, Miniboss, Boss, MoonSoldier, FinalBoss }

public static class BattleDataBridge
{
        public static EnemyPreset UpcomingEnemyPreset;
        public static AudioClip BattleMusic;
        public static BattleBackgroundType BackgroundSelection;
        public static Dictionary<string, bool> overworldActiveStates = new Dictionary<string, bool>();

    public static void SaveOverworldState(Scene overworld)
    {
        overworldActiveStates.Clear();

        foreach (GameObject root in overworld.GetRootGameObjects())
        {
            SaveRecursive(root, root.name);
        }
    }
        
    private static void SaveRecursive(GameObject obj, string path)
    {
        overworldActiveStates[path] = obj.activeSelf;

        foreach (Transform child in obj.transform)
        {
            SaveRecursive(child.gameObject, path + "/" + child.name);
        }
    }
}