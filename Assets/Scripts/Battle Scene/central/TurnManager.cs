using System.Collections.Generic;
using UnityEngine;

public enum TurnType { Player, Enemy }

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    public TurnType currentTurn = TurnType.Player;
    public List<CharacterStats> playerParty = new List<CharacterStats>();
    public List<CharacterStats> enemyParty = new List<CharacterStats>();

    private int currentCharacterIndex = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        StartTurn();
    }

    public void StartTurn()
    {
        if (currentTurn == TurnType.Player)
        {
            if (currentCharacterIndex >= playerParty.Count)
            {
                currentCharacterIndex = 0;
                currentTurn = TurnType.Enemy;
                StartTurn();
                return;
            }

            var character = playerParty[currentCharacterIndex];
            character.StartTurn();
        }
        else
        {
            if (currentCharacterIndex >= enemyParty.Count)
            {
                currentCharacterIndex = 0;
                currentTurn = TurnType.Player;
                StartTurn();
                return;
            }

            var enemy = enemyParty[currentCharacterIndex];
            enemy.StartTurn();
        }
    }

    public void EndTurn()
    {
        currentCharacterIndex++;
        StartTurn();
    }
}
