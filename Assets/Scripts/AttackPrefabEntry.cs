using UnityEngine;

[System.Serializable]
public class AttackPrefabEntry
{
    public AttackData attackData;

    [Header("Prefabs")]
    public GameObject cardPrefab;
    public GameObject deckBuilderCardPrefab;
}
