using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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
    public int currentCharacterIndex = 0;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource audioManager;
    [SerializeField] private FadeScript fadeManager;
    [SerializeField] private AudioClip victoryClip;
    [SerializeField] private AudioClip cancelSound;
    [SerializeField] private AudioClip enemyDeath;
    [SerializeField] private AudioClip playerDeath;
    [SerializeField] private GameObject descriptionMenu;
    [SerializeField] private TextMeshProUGUI descriptionTextUI;
    private int currentTargetIndex = 0;
    private int totalBattleEXP = 0;
    private int totalBattleCurrency = 0;
    private bool isSelectingTarget = false;
    public bool isActionInProgress = false;
    private bool rewardsGiven = false;
    private Coroutine targetFlickerCoroutine;
    [SerializeField] private float fadeDuration = 1.5f;
    private CharacterStats lastTarget;
    private CharacterStats currentActingCharacter;
    [SerializeField] private bool playerRevive = true;
    [SerializeField] private List <PlayerStatsSO> playerStats = new List<PlayerStatsSO>();

    [Header("Victory Settings")]
    [SerializeField] private SpriteRenderer victoryFade;
    [SerializeField] private float victoryFadeDuration = 1f;
    [SerializeField] private string overworldSceneName = "Overworld";

    [Header("Game Over Settings")]
    [SerializeField] private AudioClip gameOverClip;
    [SerializeField] private SpriteRenderer fadeOverlay;
    [SerializeField] private float gameOverFadeDuration = 2.0f;

    [Header("Enemy Preset")]
    [SerializeField] private EnemyPreset currentEnemyPreset;

    [Header("Enemy Spawn Points (3 Enemies)")]
    [SerializeField] private Transform middleSlot;
    [SerializeField] private Transform leftSlot;
    [SerializeField] private Transform rightSlot;

    [Header("Enemy Spawn Points (2 Enemies)")]
    [SerializeField] private Transform twoEnemyLeftSlot;
    [SerializeField] private Transform twoEnemyRightSlot;

    [Header("Predetermined Settings")]
    [SerializeField] private AudioSource musicManager;
    [SerializeField] private BackgroundManager backgroundManager;
    [SerializeField] private AudioClip defaultBattleMusic;

    [Header("Run Sounds")]
    [SerializeField] private AudioClip runSoundSuccess;
    [SerializeField] private AudioClip runSoundFail;

    private HashSet<CharacterStats> processedEnemies = new HashSet<CharacterStats>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        controls = new Controls();
        controls.UI.Enable();
    }

    private void Start()
    {
        fadeManager = GetComponent<FadeScript>();
        if (playerRevive == true)
        {
            foreach (PlayerStatsSO stats in playerStats)
            {
                if (stats != null)
                {
                    stats.currentHealth = stats.maxHealth;
                    stats.currentPP = stats.maxPP;
                }
            }
        }
        StartCoroutine(introfade());
        StartCoroutine(ApplyBattleConditions());
    }

    private IEnumerator introfade()
    {
        yield return fadeManager.SpriteFadeOutFlash();
    }

    private IEnumerator ApplyBattleConditions()
    {
        totalBattleEXP = 0;
        totalBattleCurrency = 0;
        rewardsGiven = false;
        battleEnded = false;
        Debug.Log($"Starting battle - Player party count: {playerParty.Count}");
        if (BattleDataBridge.UpcomingEnemyPreset != null)
        {
            currentEnemyPreset = BattleDataBridge.UpcomingEnemyPreset;
            BattleDataBridge.UpcomingEnemyPreset = null;
        }

        if (musicManager != null)
        {
            if (BattleDataBridge.BattleMusic != null)
            {
                musicManager.clip = BattleDataBridge.BattleMusic;
            }
            musicManager.Play();
        }

        if (backgroundManager != null)
        {
            if (BattleDataBridge.UpcomingEnemyPreset != null)
            {
                BattleBackgroundType bg = BattleDataBridge.BackgroundSelection;

                backgroundManager.isNormalEnemy = (bg == BattleBackgroundType.Normal);
                backgroundManager.isMiniBoss = (bg == BattleBackgroundType.Miniboss);
                backgroundManager.isBoss = (bg == BattleBackgroundType.Boss);
                backgroundManager.isMoonSoldier = (bg == BattleBackgroundType.MoonSoldier);
                backgroundManager.isFinalBossPhase = (bg == BattleBackgroundType.FinalBoss) ? 1 : 0;
            }
        }

        SpawnEnemiesFromPreset();

        yield return new WaitForSeconds(0.05f);
        StartCoroutine(StartBattle());
        isBattleActive = true;
    }

    private void Update()
    {
        if (controls.UI.FasterDialogue.IsPressed())
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
                currentTurn = TurnType.Enemy;
                currentCharacterIndex = 0;
                StartTurn();
                return;
            }

            var character = playerParty[currentCharacterIndex];

            if (character.currentHealth <= 0)
            {
                currentCharacterIndex++;
                StartTurn();
                return;
            }

            character.currentPP = Mathf.Min(character.currentPP + 1, character.maxPP);
            battleHUD.UpdateHUD();
            StartCoroutine(PlayerTurnCoroutine(character));
        }
        else
        {
            if (currentCharacterIndex >= enemyParty.Count)
            {
                currentTurn = TurnType.Player;
                currentCharacterIndex = 0;
                StartTurn();
                return;
            }

            var enemy = enemyParty[currentCharacterIndex];

            if (enemy.currentHealth <= 0)
            {
                currentCharacterIndex++;
                StartTurn();
                return;
            }

            enemy.currentPP = Mathf.Min(enemy.currentPP + 1, enemy.maxPP);
            StartCoroutine(EnemyTurnCoroutine(enemy));
        }
    }

    private IEnumerator PlayerTurnCoroutine(CharacterStats player)
    {
        currentActingCharacter = player;
        battleHUD.SetCharacter(player);

        player.ApplyStatusEffects();
        battleHUD.UpdateHUD();

        if (player.IsDOT())
        {
            yield return flavorTextUI.ShowTextCoroutine(
                $"{player.characterName} took {player.overtimeDamage} damage from their affliction"
            );
            yield return new WaitForSeconds(0.3f);
        }

        if (player.IsStunned())
        {
            yield return flavorTextUI.ShowTextCoroutine(
                $"{player.characterName} is locked in place and cannot move"
            );
            yield return new WaitForSeconds(0.3f);

            player.ReduceAllEffectsAfterTurn();
            currentCharacterIndex++;
            StartTurn();
            yield break;
        }

        if (player.IsMissAttack())
        {
            yield return flavorTextUI.ShowTextCoroutine(
                $"{player.characterName}'s accuracy is disrupted"
            );
            yield return new WaitForSeconds(0.3f);
        }

        if (player.IsStatChange())
        {
            yield return flavorTextUI.ShowTextCoroutine(
                $"{player.characterName}'s focus is shifted"
            );
            yield return new WaitForSeconds(0.3f);
        }

        yield return flavorTextUI.ShowTextCoroutine($"It's {player.characterName}'s turn!");

        UIManager.Instance.ShowPlayerOptions(player);
    }

    private IEnumerator EnemyTurnCoroutine(CharacterStats enemy)
    {
        isActionInProgress = true;
        enemy.ApplyStatusEffects();

        if (enemy.currentHealth <= 0)
        {
            yield return HandleEnemyDeath(enemy);
            yield break;
        }

        if (enemy.IsDOT())
        {
            yield return flavorTextUI.ShowTextCoroutine(
                $"{enemy.characterName} took {enemy.overtimeDamage} damage from their affliction"
            );
            yield return new WaitForSeconds(0.3f);
        }

        if (enemy.IsStunned())
        {
            yield return flavorTextUI.ShowTextCoroutine(
                $"{enemy.characterName} is locked in place and cannot move"
            );
            yield return new WaitForSeconds(0.3f);

            enemy.ReduceAllEffectsAfterTurn();
            currentCharacterIndex++;
            StartTurn();
            yield break;
        }

        if (enemy.IsMissAttack())
        {
            yield return flavorTextUI.ShowTextCoroutine(
                $"{enemy.characterName}'s accuracy is disrupted"
            );
            yield return new WaitForSeconds(0.3f);
        }

        if (enemy.IsStatChange())
        {
            yield return flavorTextUI.ShowTextCoroutine(
                $"{enemy.characterName}'s focus is shifted"
            );
            yield return new WaitForSeconds(0.3f);
        }

        BossPhaseController phaseController = enemy.GetComponent<BossPhaseController>();

        if (phaseController != null)
        {
            int enemyCountBefore = enemyParty.Count;

            yield return phaseController.TryHandlePhaseTransition();

            if (enemyParty.Count != enemyCountBefore)
            {
                EndTurn();
                yield break;
            }
        }

        currentActingCharacter = enemy;

        yield return flavorTextUI.ShowTextCoroutine(
            $"{enemy.characterName} is taking its turn..."
        );
        yield return new WaitForSeconds(0.3f);
        List<CharacterStats> alivePlayers = playerParty
            .FindAll(p => p != null && p.currentHealth > 0 && p.gameObject.activeInHierarchy);

        if (alivePlayers.Count == 0)
        {
            CheckWinLose();
            yield break;
        }

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
        isActionInProgress = false;
    }

    public void EndTurn()
    {
        if (!isBattleActive)
            return;

        currentActingCharacter.ReduceAllEffectsAfterTurn();
        currentCharacterIndex++;
        StartTurn();
    }

    public void StartTargetSelection(
        List<CharacterStats> possibleTargets,
        System.Action<CharacterStats> onTargetConfirmed,
        bool targetAll = false,
        bool includeDead = false)
    {
        if (isActionInProgress || currentTurn != TurnType.Player)
            return;

        if (possibleTargets == null || possibleTargets.Count == 0)
            return;

        isSelectingTarget = true;

        if (targetAll)
        {
            StartCoroutine(TargetAllRoutine(possibleTargets, onTargetConfirmed));
            return;
        }

        currentTargetIndex = currentTargetIndex = GetNextValidIndex(possibleTargets, -1, +1);
        StartCoroutine(TargetSelectionRoutine(possibleTargets, onTargetConfirmed, includeDead));
    }

    private int GetNextValidIndex(List<CharacterStats> list, int startIndex, int direction)
    {
        if (list.Count == 0)
            return -1;

        int count = list.Count;

        for (int i = 1; i <= count; i++)
        {
            int index = (startIndex + i * direction) % count;

            if (index < 0)
                index += count;

            if (list[index] == null)
                continue;

            if (!list[index].gameObject.activeInHierarchy)
                continue;

            return index;
        }

        return startIndex;
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
                flavorTextUI.ShowImmediateText(
                    $"Target: {currentTarget.characterName}\n" +
                    $"HP: {currentTarget.currentHealth}");

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
                    currentTargetIndex = GetNextValidIndex(targetList, currentTargetIndex, +1);

                if (input.x < 0)
                    currentTargetIndex = GetNextValidIndex(targetList, currentTargetIndex, -1);
            }

            if (controls.UI.BattleSubmit.triggered)
            {
                isSelectingTarget = false;

                if (targetFlickerCoroutine != null)
                {
                    StopCoroutine(targetFlickerCoroutine);
                    targetFlickerCoroutine = null;
                }
                ResetTargetVisual();
                onTargetConfirmed?.Invoke(currentTarget);
                yield break;
            }

            if (controls.UI.Cancel.triggered && !isActionInProgress)
            {
                isSelectingTarget = false;

                if (targetFlickerCoroutine != null)
                {
                    StopCoroutine(targetFlickerCoroutine);
                    targetFlickerCoroutine = null;
                }
                ResetTargetVisual();
                CancelTargetSelection();
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
            if (controls.UI.BattleSubmit.triggered)
            {
                isSelectingTarget = false;

                foreach (var c in flickers)
                    if (c != null) StopCoroutine(c);

                ResetTargetVisual();
                if (targetFlickerCoroutine != null)
                {
                    StopCoroutine(targetFlickerCoroutine);
                    targetFlickerCoroutine = null;
                }
                ResetAllTargetVisuals(targetList);
                onTargetConfirmed?.Invoke(null);

                yield break;
            }

            if (controls.UI.Cancel.triggered && !isActionInProgress)
            {
                isSelectingTarget = false;

                foreach (var c in flickers)
                    if (c != null) StopCoroutine(c);

                ResetTargetVisual();
                CancelTargetSelection();
                if (targetFlickerCoroutine != null)
                {
                    StopCoroutine(targetFlickerCoroutine);
                    targetFlickerCoroutine = null;
                }
                ResetAllTargetVisuals(targetList);
                yield break;
            }

            yield return null;
        }
    }

    private IEnumerator FlickerSprite(CharacterStats target)
    {
        if (target == null) yield break;

        SpriteRenderer sr = target.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        Color originalColor = sr.color;

        while (target != null && sr != null)
        {
            sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
            yield return new WaitForSeconds(0.6f);

            if (sr == null) yield break;

            sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
            yield return new WaitForSeconds(0.7f);
        }
    }

    public void ResetTargetVisual()
    {
        if (lastTarget != null)
        {
            SpriteRenderer sr = lastTarget.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.color = Color.white;
        }

        lastTarget = null;
    }

    public void ResetAllTargetVisuals(List<CharacterStats> targets)
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
            EndBattle(true);
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

        if (allPlayersDead && isBattleActive)
        {
            isBattleActive = false;
            EndBattle(false);
        }
    }

    private bool battleEnded = false;

    private void EndBattle(bool playerWon)
    {
        if (battleEnded) return;

        battleEnded = true;
        isBattleActive = false;

        BattleResultBridge.HasResult = true;
        BattleResultBridge.BattleWon = playerWon;
        BattleResultBridge.TotalEXP = totalBattleEXP;
        BattleResultBridge.BattleOutcomeText = playerWon ? "You won the battle!" : "You were defeated...";

        if (playerWon)
            StartCoroutine(HandleVictory());
        else
            StartCoroutine(HandleDefeat());
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
        StartCoroutine(VictoryScreenFade());
        yield return flavorTextUI.ShowTextCoroutine("You won!");

        yield return new WaitForSeconds(0.3f);
        StartCoroutine(RewardSystem());

        yield return new WaitUntil(() => RewardsGiven());

        SceneManager.UnloadSceneAsync("Battle Scene");
        Scene overworld = SceneManager.GetSceneByName("Overworld");
        foreach (GameObject root in overworld.GetRootGameObjects())
        {
            RestoreRecursive(root, root.name);
        }
    }

    public bool RewardsGiven()
    {
        return rewardsGiven;
    }

    private IEnumerator RewardSystem()
    {
        if (totalBattleEXP > 0)
        {
            yield return flavorTextUI.ShowTextCoroutine(
                $"You earned {totalBattleEXP} EXP each!"
            );
        }

        List<CharacterStats> activeMembers = playerParty.FindAll(
         p => p != null &&
         p.isPlayer &&
         p.currentHealth > 0 &&
         p.gameObject.activeInHierarchy
        );

        foreach (var member in activeMembers)
        {
            member.GainEXP(totalBattleEXP);
        }

        yield return new WaitForSeconds(0.35f);

        if (totalBattleCurrency == 1)
        {
            yield return flavorTextUI.ShowTextCoroutine(
                $"Your team earned {totalBattleCurrency} dollar!"
            );
        }
        else if (totalBattleCurrency > 0)
        {
            yield return flavorTextUI.ShowTextCoroutine(
                $"Your team earned {totalBattleCurrency} dollars!"
            );
        }

        CurrencyManager.Instance.AddCoins(totalBattleCurrency);

        yield return new WaitForSeconds(0.35f);

        rewardsGiven = true;
    }

    private void RestoreRecursive(GameObject obj, string path)
    {
        if (BattleDataBridge.overworldActiveStates.TryGetValue(path, out bool wasActive))
        {
            obj.SetActive(wasActive);
        }

        foreach (Transform child in obj.transform)
        {
            RestoreRecursive(child.gameObject, path + "/" + child.name);
        }
    }

    private IEnumerator VictoryScreenFade()
    {
        if (victoryFade != null)
        {
            float elapsed = 0f;
            Color fadeColor = victoryFade.color;

            float targetAlpha = 175f / 255f;

            while (elapsed < victoryFadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, targetAlpha, elapsed / victoryFadeDuration);

                victoryFade.color = new Color(
                    fadeColor.r,
                    fadeColor.g,
                    fadeColor.b,
                    alpha
                );

                yield return null;
            }

            victoryFade.color = new Color(
                fadeColor.r,
                fadeColor.g,
                fadeColor.b,
                targetAlpha
            );
        }
    }

    private IEnumerator HandleDefeat()
    {
        if (musicManager != null) musicManager.Stop();
        if (musicSource != null) musicSource.Stop();

        if (gameOverClip != null && audioManager != null)
        {
            audioManager.clip = gameOverClip;
            audioManager.Play();
        }

        yield return flavorTextUI.ShowTextCoroutine("You were defeated...");

        yield return new WaitForSeconds(1.0f);

        if (fadeOverlay != null)
        {
            float elapsed = 0f;
            Color fadeColor = fadeOverlay.color;

            while (elapsed < gameOverFadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, elapsed / gameOverFadeDuration);
                fadeOverlay.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
                yield return null;
            }
            fadeOverlay.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);
        }
        Debug.Log("Game Over Sequence Complete. Add SceneManager.LoadScene here.");
        yield return new WaitForSeconds(2.5f);

        SceneManager.LoadScene(overworldSceneName);
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
        if (processedEnemies.Contains(enemy)) yield break;

        processedEnemies.Add(enemy);

        if (enemyParty.Contains(enemy))
        {
            totalBattleEXP += enemy.expReward;
            totalBattleCurrency += enemy.currencyReward;
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
        isActionInProgress = true;

        if (player.currentPP < attack.powerCost)
        {
            isActionInProgress = false;
            ResetTargetVisual();
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
                enemyParty.FindAll(e => e != null && e.currentHealth > 0 && e.gameObject.activeInHierarchy);

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

        isActionInProgress = false;
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
            ResetTargetVisual();
            UIManager.Instance.ShowPlayerOptions(player);
            yield break;
        }

        player.currentPP -= specAttack.powerCost;
        battleHUD.UpdateHUD();

        if (specAttack.oneUse)
            Inventory.Instance.UseSpecialAttack(invSpecAttack);

        if (target == null)
        {
            List<CharacterStats> aliveEnemies =
                enemyParty.FindAll(e => e != null && e.currentHealth > 0 && e.gameObject.activeInHierarchy);

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

    public IEnumerator TryRun()
    {
        if (!isBattleActive)
            yield break;

        isActionInProgress = true;

        yield return flavorTextUI.ShowTextCoroutine($"{currentActingCharacter.characterName} is trying to run away...");

        yield return new WaitForSeconds(2f);

        bool escaped = Random.value < 0.40f;

        if (!escaped)
        {
            AudioManager.Instance.PlaySFX(runSoundFail);
            yield return flavorTextUI.ShowTextCoroutine("Couldn't run!");
            yield return new WaitForSeconds(0.5f);

            isActionInProgress = false;

            currentCharacterIndex++;
            StartTurn();
        }
        else
        {
            AudioManager.Instance.PlaySFX(runSoundSuccess);
            yield return flavorTextUI.ShowTextCoroutine("Escaped successfully!");
            yield return new WaitForSeconds(0.5f);

            StartCoroutine(HandleEscape());
        }
    }

    private IEnumerator HandleEscape()
    {
        isBattleActive = false;

        if (musicManager != null) musicManager.Stop();
        if (musicSource != null) musicSource.Stop();

        if (fadeManager != null)
            yield return fadeManager.SpriteFadeInFlash();

        rewardsGiven = true;

        BattleResultBridge.HasResult = true;
        BattleResultBridge.BattleWon = false;

        SceneManager.UnloadSceneAsync("Battle Scene");

        Scene overworld = SceneManager.GetSceneByName("Overworld");
        foreach (GameObject root in overworld.GetRootGameObjects())
        {
            RestoreRecursive(root, root.name);
        }
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

    public IEnumerator ReplaceEnemyPreset(EnemyPreset newPreset, System.Action<CharacterStats> onBossSpawned)
    {
        if (newPreset == null)
            yield break;

        currentEnemyPreset = newPreset;

        List<GameObject> prefabs = newPreset.EnemyPrefabs;

        CharacterStats middleBoss = null;

        if (prefabs.Count == 1)
        {
            middleBoss = SpawnEnemy(prefabs[0], middleSlot);
        }
        else if (prefabs.Count == 2)
        {
            middleBoss = SpawnEnemy(prefabs[0], twoEnemyLeftSlot);
            SpawnEnemy(prefabs[1], twoEnemyRightSlot);
        }
        else if (prefabs.Count == 3)
        {
            middleBoss = SpawnEnemy(prefabs[2], middleSlot);
            SpawnEnemy(prefabs[1], leftSlot);
            SpawnEnemy(prefabs[0], rightSlot);
        }

        onBossSpawned?.Invoke(middleBoss);

        yield return null;
    }

    private CharacterStats SpawnEnemy(GameObject prefab, Transform spawnPoint)
    {
        if (prefab == null || spawnPoint == null)
            return null;

        GameObject enemyGO = Instantiate(prefab, spawnPoint);

        enemyGO.transform.localPosition = Vector3.zero;
        enemyGO.transform.localRotation = Quaternion.identity;

        CharacterStats stats = enemyGO.GetComponent<CharacterStats>();

        if (stats != null)
        {
            enemyParty.Add(stats);
            return stats;
        }

        Debug.LogWarning($"Spawned enemy {prefab.name} has no CharacterStats!");
        return null;
    }

    public void ShowDescription(string text)
    {
        if (!UIManager.Instance.attackMenu.activeSelf &&
            !UIManager.Instance.itemMenu.activeSelf &&
            !UIManager.Instance.specialMenu.activeSelf)
        {
            return;
        }

        if (descriptionMenu == null || descriptionTextUI == null)
            return;

        descriptionMenu.SetActive(true);
        descriptionTextUI.text = text;
    }

    public void HideDescription()
    {
        if (descriptionMenu == null)
            return;

        descriptionMenu.SetActive(false);
    }

    public CharacterStats GetCurrentPlayer()
    {
        return currentActingCharacter;
    }
}
