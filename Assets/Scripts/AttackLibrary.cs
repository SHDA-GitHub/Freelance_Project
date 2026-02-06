using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Attacks/Attack Library")]
public class AttackLibrary : ScriptableObject
{
    public List<AttackPrefabEntry> allPlayerattacks;
}
