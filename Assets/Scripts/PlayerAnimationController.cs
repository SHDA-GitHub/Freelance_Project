using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerCardAnimation
{
    public AttackData cardData;
    public AnimationClip animationClip;
}

public class PlayerAnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Statistics stats;
    [SerializeField] private PlayerDeck playerDeck;

    [Header("Animations")]
    [SerializeField] private AnimationClip idleAnimation;
    [SerializeField] private AnimationClip hurtAnimation;
    [SerializeField] private List<PlayerCardAnimation> cardAnimations = new();

    [SerializeField] private float returnToIdleDelay = 0.5f;

    private bool isAnimationLocked;
    private float lastHealth;

    private Dictionary<AttackData, AnimationClip> animationLookup;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (stats == null)
            stats = GetComponent<Statistics>();

        lastHealth = stats.currentHealth;
    }
    private void Start()
    {
        Debug.Log("PlayerAnimationController STARTED on " + gameObject.name);
        BuildLookup();
    }

    private void Update()
    {
        CheckForDamage();
    }

    private void BuildLookup()
    {
        animationLookup = new Dictionary<AttackData, AnimationClip>();

        foreach (var entry in cardAnimations)
        {
            if (entry.cardData == null || entry.animationClip == null)
                continue;

            animationLookup[entry.cardData] = entry.animationClip;
        }
    }

    public void PlayCardAnimation(AttackData card)
    {
        Debug.Log("PlayCardAnimation called with: " + (card ? card.CardName : "NULL"));

        if (card == null || animator == null || isAnimationLocked)
            return;

        if (!playerDeck.selectedCards.Contains(card))
        {
            Debug.LogWarning("Card NOT in deck: " + card.CardName);
            return;
        }

        if (animationLookup.TryGetValue(card, out var clip))
        {
            StartCoroutine(PlayLockedAnimation(clip));
        }
    }

    private IEnumerator PlayLockedAnimation(AnimationClip clip)
    {
        isAnimationLocked = true;
        animator.CrossFade(clip.name, 0.1f, 0, 0f);
        yield return new WaitForSeconds(clip.length / animator.speed);
        yield return new WaitForSeconds(returnToIdleDelay);
        animator.CrossFade(idleAnimation.name, 0.15f);
        isAnimationLocked = false;
    }


    private void CheckForDamage()
    {
        if (stats.currentHealth < lastHealth && !isAnimationLocked)
        {
            if (hurtAnimation != null)
                StartCoroutine(PlayLockedAnimation(hurtAnimation));
        }

        lastHealth = stats.currentHealth;
    }
}