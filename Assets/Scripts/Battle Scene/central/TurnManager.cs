using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TurnType { Player, Enemy }

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    public TurnType currentTurn = TurnType.Player;
    public List<CharacterStats> playerParty = new List<CharacterStats>();
    public List<CharacterStats> enemyParty = new List<CharacterStats>();
    public bool isBattleActive = true;

    public FlavorTextUI flavorTextUI;
    private int currentCharacterIndex = 0;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource audioManager;
    [SerializeField] private AudioClip victoryClip;
    [SerializeField] private AudioClip enemyDeath;
    [SerializeField] private float fadeDuration = 1.5f;


    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        StartCoroutine(StartBattle());
    }
    private IEnumerator StartBattle()
    {
        if (enemyParty.Count > 0)
        {
            string[] encounterMessages = {
                "{0} has appeared",
                "{0} blocks your path",
                "{0} approaches you",
                "You encountered {0}"
            };

            string message = string.Format(
                encounterMessages[Random.Range(0, encounterMessages.Length)],
                enemyParty[0].characterName
            );

            yield return flavorTextUI.ShowTextCoroutine(message);
            yield return new WaitForSeconds(0.5f);
        }

        StartTurn();
    }

    public void StartTurn()
    {
        if (!isBattleActive) return;
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
            StartCoroutine(PlayerTurnCoroutine(character));
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
            StartCoroutine(EnemyTurnCoroutine(enemy));
        }
    }

    private IEnumerator PlayerTurnCoroutine(CharacterStats player)
    {
        yield return flavorTextUI.ShowTextCoroutine($"It's {player.characterName}'s turn!");
        UIManager.Instance.ShowPlayerOptions(player);
    }

    private IEnumerator EnemyTurnCoroutine(CharacterStats enemy)
    {
        yield return flavorTextUI.ShowTextCoroutine($"Enemy {enemy.characterName} is taking its turn...");

        if (playerParty.Count > 0)
        {
            var target = playerParty[0];
            var attack = enemy.attacks[0];
            yield return CombatSystem.Instance.ExecuteAttack(enemy, target, attack);
        }
        else
        {
            EndTurn();
        }
    }


    public void EndTurn()
    {
        if (!isBattleActive)
        return;
        currentCharacterIndex++;
        StartTurn();
    }

    public void CheckWinLose()
    {
        bool allEnemiesDead = true;
        foreach (var enemy in enemyParty)
        {
            if (enemy.currentHealth > 0)
            {
                allEnemiesDead = false;
                break;
            }
        }

        if (allEnemiesDead)
        {
            isBattleActive = false;
            StartCoroutine(HandleVictory());
            return;
        }

        bool allPlayersDead = true;
        foreach (var player in playerParty)
        {
            if (player.currentHealth > 0)
            {
                allPlayersDead = false;
                break;
            }
        }

        if (allPlayersDead)
        {
            Debug.Log("Players Lose!");
            EndBattle(false);
        }
    }

    private void EndBattle(bool playerWon)
    {
        if (playerWon)
            Debug.Log("Victory screen here!");
        else
            Debug.Log("Defeat screen here!");

        isBattleActive = false;
    }

    private IEnumerator HandleVictory()
    {
        isBattleActive = false;
        foreach (var enemy in enemyParty)
        {
            if (enemy != null)
                yield return StartCoroutine(FadeOutEnemy(enemy));
        }
        if (musicSource != null)
            musicSource.Stop();
        if (victoryClip != null && audioManager != null)
        {
            audioManager.clip = victoryClip;
            audioManager.Play();
        }
        yield return flavorTextUI.ShowTextCoroutine("You won!");
    }

    private IEnumerator FadeOutEnemy(CharacterStats enemy)
    {
        SpriteRenderer sr = enemy.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;
        audioManager.clip = enemyDeath;
        audioManager.Play();
        float elapsed = 0f;
        Color originalColor = sr.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);

        enemy.gameObject.SetActive(false);
    }
}
