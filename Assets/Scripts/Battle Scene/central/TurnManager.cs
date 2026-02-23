using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public enum TurnType { Player, Enemy }

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    public TurnType currentTurn = TurnType.Player;
    public List<CharacterStats> playerParty = new List<CharacterStats>();
    public List<CharacterStats> enemyParty = new List<CharacterStats>();
    public bool isBattleActive = true;
    private Controls controls;

    public BattleHUD battleHUD;
    public FlavorTextUI flavorTextUI;
    private int currentCharacterIndex = 0;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource audioManager;
    [SerializeField] private AudioClip victoryClip;
    [SerializeField] private AudioClip enemyDeath;
    private int currentTargetIndex = 0;
    private bool isSelectingTarget = false;
    private Coroutine targetFlickerCoroutine;
    [SerializeField] private float fadeDuration = 1.5f;
    private CharacterStats lastTarget;


    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        controls = new Controls();
        controls.UI.Enable();
    }

    private void Start()
    {
        StartCoroutine(StartBattle());
    }
    private IEnumerator StartBattle()
    {
        if (enemyParty.Count > 0)
        {
            string message = "";

            if (enemyParty.Count == 1)
            {
                string[] encounterMessages =
                {
                "{0} has appeared",
                "{0} blocks your path",
                "{0} approaches you",
                "You encountered {0}"
            };

                message = string.Format(
                    encounterMessages[Random.Range(0, encounterMessages.Length)],
                    enemyParty[0].characterName
                );
            }

            else if (enemyParty.Count == 2)
            {
                CharacterStats randomEnemy = enemyParty[Random.Range(0, enemyParty.Count)];

                string[] twoEnemyMessages =
                {
                "You confront {0} and its cohort",
                "You encounter {0} and its cohort",
                "{0} and its cohort block your path",
                "{0} stands before you with its cohort"
            };

                message = string.Format(
                    twoEnemyMessages[Random.Range(0, twoEnemyMessages.Length)],
                    randomEnemy.characterName
                );
            }

            else if (enemyParty.Count >= 3)
            {
                CharacterStats randomEnemy = enemyParty[Random.Range(0, enemyParty.Count)];

                string[] multiEnemyMessages =
                {
                "You confront {0} and co.",
                "You encounter {0} and co.",
                "You confront {0} and its cohorts",
                "You encounter {0} and its cohorts",
                "{0} leads its cohorts into battle",
                "{0} and its cohorts surround you"
            };

                message = string.Format(
                    multiEnemyMessages[Random.Range(0, multiEnemyMessages.Length)],
                    randomEnemy.characterName
                );
            }

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
        battleHUD.SetCharacter(player);
        yield return flavorTextUI.ShowTextCoroutine($"It's {player.characterName}'s turn!");
        UIManager.Instance.ShowPlayerOptions(player);
    }

    private IEnumerator EnemyTurnCoroutine(CharacterStats enemy)
    {
        yield return flavorTextUI.ShowTextCoroutine($"{enemy.characterName} is taking its turn...");

        if (playerParty.Count > 0)
        {
            var target = playerParty[0];

            if (enemy.enemyLoadout == null)
            {
                Debug.LogWarning($"{enemy.characterName} has no EnemyLoadout assigned!");
                yield break;
            }

            var attack = enemy.enemyLoadout.GetRandomAttack();

            if (attack == null)
                yield break;

            yield return CombatSystem.Instance.ExecuteAttack(enemy, target, attack);
        }
    }

    public void EndTurn()
    {
        if (!isBattleActive)
        return;
        currentCharacterIndex++;
        StartTurn();
    }
    public void StartTargetSelection(System.Action<CharacterStats> onTargetConfirmed)
    {
        if (enemyParty.Count == 0) return;

        isSelectingTarget = true;
        currentTargetIndex = GetNextAliveEnemyIndex(0);

        StartCoroutine(TargetSelectionRoutine(onTargetConfirmed));
    }

    private int GetNextAliveEnemyIndex(int startIndex)
    {
        if (enemyParty.Count == 0)
            return -1;

        int count = enemyParty.Count;

        startIndex = (startIndex % count + count) % count;

        for (int i = 0; i < count; i++)
        {
            int index = (startIndex + i) % count;

            if (enemyParty[index] != null && enemyParty[index].currentHealth > 0)
                return index;
        }

        return -1;
    }

    private IEnumerator TargetSelectionRoutine(System.Action<CharacterStats> onTargetConfirmed)
    {
        while (isSelectingTarget)
        {
            if (currentTargetIndex < 0 || currentTargetIndex >= enemyParty.Count)
                yield break;

            CharacterStats currentTarget = enemyParty[currentTargetIndex];

            if (currentTarget != lastTarget)
            {
                flavorTextUI.ShowImmediateText($"Target: {currentTarget.characterName}");

                if (lastTarget != null)
                {
                    SpriteRenderer oldSR = lastTarget.GetComponent<SpriteRenderer>();
                    if (oldSR != null)
                        oldSR.color = Color.white;
                }

                if (targetFlickerCoroutine != null)
                    StopCoroutine(targetFlickerCoroutine);

                targetFlickerCoroutine = StartCoroutine(FlickerSprite(currentTarget));

                lastTarget = currentTarget;

            }

            if (controls.UI.Navigate.triggered)
            {
                Vector2 input = controls.UI.Navigate.ReadValue<Vector2>();

                if (input.x > 0)
                    currentTargetIndex = GetNextAliveEnemyIndex(currentTargetIndex + 1);

                if (input.x < 0)
                    currentTargetIndex = GetNextAliveEnemyIndex(currentTargetIndex - 1);
            }

            if (controls.UI.Submit.triggered)
            {
                isSelectingTarget = false;

                if (targetFlickerCoroutine != null)
                    StopCoroutine(targetFlickerCoroutine);
                    ResetTargetVisual();

                onTargetConfirmed?.Invoke(currentTarget);
                yield break;
            }

            if (controls.UI.Cancel.triggered)
            {
                isSelectingTarget = false;

                if (targetFlickerCoroutine != null)
                    StopCoroutine(targetFlickerCoroutine);
                    ResetTargetVisual();

                yield break;
            }

            yield return null;
        }
    }
    private IEnumerator FlickerSprite(CharacterStats target)
    {
        SpriteRenderer sr = target.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        Color originalColor = sr.color;

        while (true)
        {
            sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
            yield return new WaitForSeconds(0.6f);

            sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
            yield return new WaitForSeconds(0.7f);
        }
    }
    private void ResetTargetVisual()
    {
        if (lastTarget != null)
        {
            SpriteRenderer sr = lastTarget.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.color = Color.white;
        }

        lastTarget = null;
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

    public IEnumerator HandleEnemyDeath(CharacterStats enemy)
    {
        if (enemyParty.Contains(enemy))
        {
            yield return StartCoroutine(FadeOutEnemy(enemy));
            enemyParty.Remove(enemy);

            if (currentTurn == TurnType.Enemy)
            {
                currentCharacterIndex--;
            }

            CheckWinLose();
        }
    }
}
