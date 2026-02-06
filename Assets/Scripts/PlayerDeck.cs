using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Player Deck")]
public class PlayerDeck : ScriptableObject
{
    public List<AttackData> selectedCards;
    public int maxDeckSize = 3;
}
