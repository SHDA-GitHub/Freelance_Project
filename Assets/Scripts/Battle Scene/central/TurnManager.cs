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
    [SerializeField] private AudioClip cancelSound;
    [SerializeField] private AudioClip enemyDeath;
    [SerializeField] private AudioClip playerDeath;
    private int currentTargetIndex = 0;
    private bool isSelectingTarget = false;
    private Coroutine targetFlickerCoroutine;
    [SerializeField] private float fadeDuration = 1.5f;
    private CharacterStats lastTarget;
    private CharacterStats currentActingCharacter;

    [Header("Enemy Preset")]
    [SerializeField] private EnemyPreset currentEnemyPreset;

    [Header("Enemy Spawn Points (3 Enemies)")]
    [SerializeField] private Transform middleSlot;
    [SerializeField] private Transform leftSlot;
    [SerializeField] private Transform rightSlot;

    [Header("Enemy Spawn Points (2 Enemies)")]
    [SerializeField] private Transform twoEnemyLeftSlot;
    [SerializeField] private Transform twoEnemyRightSlot;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        controls = new Controls();
        controls.UI.Enable();
    }

    private void Start()
    {
        SpawnEnemiesFromPreset();
        StartCoroutine(StartBattle());
    }

    private void Update()
    {
        if (controls.UI.Submit.IsPressed())
        {
            flavorTextUI.SetFastMode(true);
        }
        else
        {
            flavorTextUI.SetFastMode(false);
        }
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
            if (character.currentHealth > 0)
            {
                character.currentPP = Mathf.Min(character.currentPP + 1, character.maxPP);
                battleHUD.UpdateHUD();
            }

            if (character.currentHealth <= 0)
            {
                currentCharacterIndex++;
                StartTurn();
                return;
            }

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
            if (enemy.currentHealth > 0)
            {
                enemy.currentPP = Mathf.Min(enemy.currentPP + 1, enemy.maxPP);
            }

            StartCoroutine(EnemyTurnCoroutine(enemy));
        }
    }

    private IEnumerator PlayerTurnCoroutine(CharacterStats player)
    {
        currentActingCharacter = player;
        battleHUD.SetCharacter(player);

        player.ApplyStatusEffects();
        battleHUD.UpdateHUD();

        if (player.IsStunned())
        {
            yield return flavorTextUI.ShowTextCoroutine(
                $"{player.characterName} is stunned and cannot move!"
            );
            yield return new WaitForSeconds(0.3f);

            EndTurn();
            yield break;
        }

        yield return flavorTextUI.ShowTextCoroutine($"It's {player.characterName}'s turn!");

        UIManager.Instance.ShowPlayerOptions(player);
    }

    private IEnumerator EnemyTurnCoroutine(CharacterStats enemy)
    {
        enemy.ApplyStatusEffects();

        if (enemy.currentHealth <= 0)
        {
            yield return HandleEnemyDeath(enemy);
            yield break;
        }
        if (enemy.IsStunned())
        {
            yield return flavorTextUI.ShowTextCoroutine(
                $"{enemy.characterName} is stunned and cannot move!"
            );
            yield return new WaitForSeconds(0.3f);

            EndTurn();
            yield break;
        }

        BossPhaseController phaseController = enemy.GetComponent<BossPhaseController>();

        if (phaseController != null)
        {
            yield return phaseController.TryHandlePhaseTransition();
            SpriteRenderer sr = enemy.GetComponent<SpriteRenderer>();
            if (sr != null && sr.color.a == 0f)
            {
                enemyParty.Remove(enemy);
                Destroy(enemy.gameObject);

                currentTurn = TurnType.Player;
                currentCharacterIndex = 0;
                StartTurn();
                yield break;
            }
        }

        currentActingCharacter = enemy;

        yield return flavorTextUI.ShowTextCoroutine(
            $"{enemy.characterName} is taking its turn..."
        );
        yield return new WaitForSeconds(0.3f);
        List<CharacterStats> alivePlayers = playerParty
            .FindAll(p => p != null && p.currentHealth > 0);

        if (alivePlayers.Count == 0)
            yield break;

        CharacterStats target =
            alivePlayers[Random.Range(0, alivePlayers.Count)];
        if (enemy.enemyLoadout == null)
        {
            Debug.LogWarning(
                $"{enemy.characterName} has no EnemyLoadout assigned!"
            );
            yield break;
        }
        var attack = enemy.enemyLoadout.GetRandomAttack();

        if (attack == null)
            yield break;

        if (attack.targetAllEnemies)
        {
            yield return CombatSystem.Instance.ExecuteAttackOnAll(
                enemy,
                alivePlayers,
                attack
            );
        }
        else
        {
            yield return CombatSystem.Instance.ExecuteAttack(
                enemy,
                target,
                attack
            );

            if (target.currentHealth <= 0)
            {
                yield return HandleEnemyDeath(target);
                yield return HandlePlayerDeath(target);
            }

            CheckWinLose();
        }
            yield return new WaitForSeconds(0.3f);
            EndTurn();
    }

    public void EndTurn()
    {
        if (!isBattleActive)
            return;
        if (currentActingCharacter != null)
        {
            currentActingCharacter.ReduceAllEffectsAfterTurn();
        }

        currentCharacterIndex++;
        StartTurn();
    }

    public void StartTargetSelection(
        List<CharacterStats> possibleTargets,
        System.Action<CharacterStats> onTargetConfirmed,
        bool targetAll = false,
        bool includeDead = false)
    {
        if (possibleTargets == null || possibleTargets.Count == 0)
            return;

        isSelectingTarget = true;

        if (targetAll)
        {
            StartCoroutine(TargetAllRoutine(possibleTargets, onTargetConfirmed));
            return;
        }

        currentTargetIndex = GetNextValidIndex(possibleTargets, 0, includeDead);
        StartCoroutine(TargetSelectionRoutine(possibleTargets, onTargetConfirmed, includeDead));
    }

    private int GetNextValidIndex(List<CharacterStats> list, int startIndex, bool includeDead)
    {
        if (list.Count == 0)
            return -1;

        int count = list.Count;
        startIndex = (startIndex % count + count) % count;

        for (int i = 0; i < count; i++)
        {
            int index = (startIndex + i) % count;

            if (list[index] == null)
                continue;

            if (includeDead)
                return index;

            if (list[index].currentHealth > 0)
                return index;
        }

        return -1;
    }

    private IEnumerator TargetSelectionRoutine(
        List<CharacterStats> targetList,
        System.Action<CharacterStats> onTargetConfirmed,
        bool includeDead)
    {
        while (isSelectingTarget)
        {
            if (currentTargetIndex < 0 || currentTargetIndex >= targetList.Count)
                yield break;

            CharacterStats currentTarget = targetList[currentTargetIndex];

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
                    currentTargetIndex = GetNextValidIndex(targetList, currentTargetIndex + 1, includeDead);

                if (input.x < 0)
                    currentTargetIndex = GetNextValidIndex(targetList, currentTargetIndex + 1, includeDead);
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
                CancelTargetSelection();
                StopCoroutine(targetFlickerCoroutine);
                AudioManager.Instance.PlaySFX(cancelSound);
                yield break;
            }

            yield return null;
        }
    }

    private IEnumerator TargetAllRoutine(
    List<CharacterStats> targetList,
    System.Action<CharacterStats> onTargetConfirmed)
    {
        flavorTextUI.ShowImmediateText("Target: All");

        List<Coroutine> flickers = new List<Coroutine>();

        foreach (var target in targetList)
        {
            if (target != null && target.currentHealth > 0)
            {
                flickers.Add(StartCoroutine(FlickerSprite(target)));
            }
        }

        while (isSelectingTarget)
        {
            if (controls.UI.Submit.triggered)
            {
                isSelectingTarget = false;

                foreach (var c in flickers)
                    if (c != null) StopCoroutine(c);

                ResetAllTargetVisuals(targetList);

                onTargetConfirmed?.Invoke(null);

                yield break;
            }

            if (controls.UI.Cancel.triggered)
            {
                CancelTargetSelection();
                StopCoroutine(targetFlickerCoroutine);
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

    private void ResetAllTargetVisuals(List<CharacterStats> targets)
    {
        foreach (var target in targets)
        {
            if (target != null)
            {
                SpriteRenderer sr = target.GetComponent<SpriteRenderer>();
                if (sr != null)
                    sr.color = Color.white;
            }
        }
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
        yield return flavorTextUI.ShowTextCoroutine($"{enemy.characterName} has been defeated!");
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

    public IEnumerator HandlePlayerDeath(CharacterStats player)
    {
        if (playerParty.Contains(player))
        {
            yield return flavorTextUI.ShowTextCoroutine($"{player.characterName} has been knocked out!");

            if (currentTurn == TurnType.Player)
            {
                currentCharacterIndex = Mathf.Max(currentCharacterIndex - 1, 0);
            }

            player.RemoveAllStatusEffects();
            audioManager.clip = playerDeath;
            audioManager.Play();

            CheckWinLose();
        }
    }

    public void CancelTargetSelection()
    {
        isSelectingTarget = false;

        if (targetFlickerCoroutine != null)
            StopCoroutine(targetFlickerCoroutine);

        ResetTargetVisual();

        currentTargetIndex = 0;
        if (currentActingCharacter != null)
            flavorTextUI.ShowImmediateText($"It's {currentActingCharacter.characterName}'s turn!");
        if (currentActingCharacter != null)
            UIManager.Instance.ShowPlayerOptions(currentActingCharacter);
    }

    public IEnumerator HandleEnemyDeath(CharacterStats enemy)
    {
        if (enemyParty.Contains(enemy))
        {
            yield return StartCoroutine(FadeOutEnemy(enemy));
            enemyParty.Remove(enemy);

            if (currentTurn == TurnType.Enemy)
            {
                currentCharacterIndex = Mathf.Max(currentCharacterIndex - 1, 0);
            }

            CheckWinLose();
        }
    }

    private IEnumerator PlayerAttackRoutine(
        CharacterStats player,
        CharacterStats target,
        Attack attack)
    {
        if (player.currentPP < attack.powerCost)
        {
            StopCoroutine(targetFlickerCoroutine);
            yield return flavorTextUI.ShowTextCoroutine(
                $"{player.characterName} does not have enough PP to use {attack.attackName}!"
            );

            AudioManager.Instance.PlaySFX(cancelSound);

            UIManager.Instance.ShowPlayerOptions(player);
            yield break;
        }

        player.currentPP -= attack.powerCost;
        battleHUD.UpdateHUD();

        if (attack.targetAllEnemies)
        {
            List<CharacterStats> aliveEnemies =
                enemyParty.FindAll(e => e != null && e.currentHealth > 0);

            yield return CombatSystem.Instance.ExecuteAttackOnAll(
                player,
                aliveEnemies,
                attack
            );
        }
        else
        {
            yield return CombatSystem.Instance.ExecuteAttack(
                player,
                target,
                attack
            );

            if (target.currentHealth <= 0)
            {
                yield return HandleEnemyDeath(target);
                yield return HandlePlayerDeath(target);
            }

            CheckWinLose();
        }

        EndTurn();
    }

    private IEnumerator PlayerSpecialAttackRoutine(
        CharacterStats player,
        CharacterStats target,
        InventorySpecialAttack invSpecAttack)
    {
        SpecialAttack specAttack = invSpecAttack.attackData;

        if (player.currentPP < specAttack.powerCost)
        {
            yield return flavorTextUI.ShowTextCoroutine(
                $"{player.characterName} does not have enough PP to use {specAttack.specAttackName}!"
            );

            AudioManager.Instance.PlaySFX(cancelSound);
            StopCoroutine(targetFlickerCoroutine);

            UIManager.Instance.ShowPlayerOptions(player);
            yield break;
        }

        player.currentPP -= specAttack.powerCost;
        battleHUD.UpdateHUD();

        if (specAttack.oneUse)
            Inventory.Instance.specAttacks.Remove(invSpecAttack);

        if (target == null)
        {
            List<CharacterStats> aliveEnemies =
                enemyParty.FindAll(e => e != null && e.currentHealth > 0);

            yield return CombatSystem.Instance.ExecuteSpecialAttackOnAll(
                player,
                aliveEnemies,
                invSpecAttack
            );
        }
        else
        {
            yield return CombatSystem.Instance.ExecuteSpecialAttack(
                player,
                target,
                invSpecAttack
            );

            if (target.currentHealth <= 0)
            {
                yield return HandleEnemyDeath(target);
                yield return HandlePlayerDeath(target);
            }

            CheckWinLose();
        }

        EndTurn();
    }

    public void PlayerUseAttack(CharacterStats player, CharacterStats target, Attack attack)
    {
        StartCoroutine(PlayerAttackRoutine(player, target, attack));
    }

    public void PlayerUseSpecialAttack(
        CharacterStats player,
        CharacterStats target,
        InventorySpecialAttack invSpecAttack)
    {
        StartCoroutine(PlayerSpecialAttackRoutine(player, target, invSpecAttack));
    }

    private void SpawnEnemiesFromPreset()
    {
        if (currentEnemyPreset == null)
        {
            Debug.LogWarning("No EnemyPreset assigned!");
            return;
        }

        enemyParty.Clear();

        List<GameObject> prefabs = currentEnemyPreset.EnemyPrefabs;

        if (prefabs.Count == 0)
            return;

        if (prefabs.Count == 1)
        {
            SpawnEnemy(prefabs[0], middleSlot);
        }
        else if (prefabs.Count == 2)
        {
            SpawnEnemy(prefabs[0], twoEnemyLeftSlot);
            SpawnEnemy(prefabs[1], twoEnemyRightSlot);
        }
        else if (prefabs.Count == 3)
        {

            SpawnEnemy(prefabs[2], middleSlot);
            SpawnEnemy(prefabs[1], leftSlot);
            SpawnEnemy(prefabs[0], rightSlot);
        }
        Debug.Log("Spawning from preset: " + currentEnemyPreset.name);
        Debug.Log("Prefab count: " + currentEnemyPreset.EnemyPrefabs.Count);
    }

    public IEnumerator ReplaceEnemyPreset(EnemyPreset newPreset)
    {
        if (newPreset == null)
            yield break;

        currentEnemyPreset = newPreset;

        List<GameObject> prefabs = newPreset.EnemyPrefabs;

        if (prefabs.Count == 1)
        {
            SpawnEnemy(prefabs[0], middleSlot);
        }
        else if (prefabs.Count == 2)
        {
            SpawnEnemy(prefabs[0], twoEnemyLeftSlot);
            SpawnEnemy(prefabs[1], twoEnemyRightSlot);
        }
        else if (prefabs.Count == 3)
        {
            SpawnEnemy(prefabs[2], middleSlot);
            SpawnEnemy(prefabs[1], leftSlot);
            SpawnEnemy(prefabs[0], rightSlot);
        }

        yield return null;
    }

    private void SpawnEnemy(GameObject prefab, Transform spawnPoint)
    {
        if (prefab == null || spawnPoint == null)
            return;

        GameObject enemyGO = Instantiate(prefab, spawnPoint);

        enemyGO.transform.localPosition = Vector3.zero;
        enemyGO.transform.localRotation = Quaternion.identity;

        CharacterStats stats = enemyGO.GetComponent<CharacterStats>();

        if (stats != null)
        {
            enemyParty.Add(stats);
        }
        else
        {
            Debug.LogWarning($"Spawned enemy {prefab.name} has no CharacterStats!");
        }
    }

    public CharacterStats GetCurrentPlayer()
    {
        return currentActingCharacter;
    }
}
