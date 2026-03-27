using System.Collections.Generic;
using UnityEngine;

public class StatusEffectDatabase : MonoBehaviour
{
    public static StatusEffectDatabase Instance;

    [System.Serializable]
    public class EffectEntry
    {
        public string effectName;
        public Sprite icon;
    }

    public List<EffectEntry> effects = new List<EffectEntry>();

    private Dictionary<string, Sprite> effectDict;

    private void Awake()
    {
        Instance = this;

        effectDict = new Dictionary<string, Sprite>();

        foreach (var entry in effects)
        {
            if (!effectDict.ContainsKey(entry.effectName))
                effectDict.Add(entry.effectName, entry.icon);
        }
    }

    public Sprite GetSpriteForType(object type)
    {
        string key = type.ToString();

        if (effectDict.TryGetValue(key, out Sprite sprite))
            return sprite;

        Debug.LogWarning($"No sprite found for status effect: {key}");
        return null;
    }
}