using UnityEngine;
using UnityEngine.EventSystems;

public enum AttackStatus
{
    show_back,
    show_front,
    rotating_to_back,
    rotating_to_front
}

public class Attack : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    [Header("Attack Data")]
    public AttackData attackData;

    [Header("Rotation")]
    [SerializeField] private float turnTargetTime = 0.3f;
    [SerializeField] private Transform visualRoot;

    [Header("Sprites")]
    [SerializeField] private SpriteRenderer frontSprite;
    [SerializeField] private SpriteRenderer backSprite;

    [Header("Cooldown")]
    [SerializeField] private int cooldownTurns = 0;

    private AttackStatus status = AttackStatus.show_back;
    private float turnTimer;
    private Quaternion startRotation;
    private Quaternion targetRotation;

    private bool isInteractable = true;
    private BoxCollider2D boxCollider;
    public AttackData GetCardData() => attackData;


    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        ShowBackInstant();
        visualRoot.localRotation = Quaternion.identity;
    }

    private void Update()
    {
        if (status == AttackStatus.rotating_to_front || status == AttackStatus.rotating_to_back)
        {
            turnTimer += Time.deltaTime;
            float t = turnTimer / turnTargetTime;

            visualRoot.localRotation =
                Quaternion.Slerp(startRotation, targetRotation, t);

            if (t >= 0.5f)
            {
                if (status == AttackStatus.rotating_to_front)
                    ShowFrontInstant();
                else
                    ShowBackInstant();
            }

            if (t >= 1f)
            {
                status = (status == AttackStatus.rotating_to_front)
                    ? AttackStatus.show_front
                    : AttackStatus.show_back;
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isInteractable) return;
        if (GameManager.Instance.currentTurn != TurnState.PlayerTurn) return;

        TurnToFront();
        GameManager.Instance.ShowCardHoverInfo(attackData);
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isInteractable) return;
        if (GameManager.Instance.currentTurn != TurnState.PlayerTurn) return;

        TurnToBack();
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        if (GameManager.Instance.currentTurn != TurnState.PlayerTurn) return;

        GameManager.Instance.OnPlayerPlayedCard(attackData);
    }


    public void EnableCard()
    {
        isInteractable = true;
        boxCollider.enabled = true;
    }

    public void DisableCard()
    {
        isInteractable = false;
        boxCollider.enabled = false;
        ForceShowBack();
    }

    public void ForceShowFront()
    {
        status = AttackStatus.show_front;
        visualRoot.localRotation = Quaternion.Euler(0, 180, 0);
        ShowFrontInstant();
    }

    public void ForceShowBack()
    {
        status = AttackStatus.show_back;
        visualRoot.localRotation = Quaternion.identity;
        ShowBackInstant();
    }

    public void TurnToFront()
    {
        if (status == AttackStatus.show_front) return;

        startRotation = visualRoot.localRotation;
        targetRotation = Quaternion.Euler(0, 180, 0);
        turnTimer = 0f;
        status = AttackStatus.rotating_to_front;
    }

    public void TurnToBack()
    {
        if (status == AttackStatus.show_back) return;

        startRotation = visualRoot.localRotation;
        targetRotation = Quaternion.identity;
        turnTimer = 0f;
        status = AttackStatus.rotating_to_back;
    }

    private void ShowFrontInstant()
    {
        frontSprite.enabled = true;
        backSprite.enabled = false;
    }

    private void ShowBackInstant()
    {
        frontSprite.enabled = false;
        backSprite.enabled = true;
    }

    public void SetCooldownVisual(int turns)
    {
        if (turns > 0)
            DisableCard();
        else
            EnableCard();
    }



    public bool IsOnCooldown => cooldownTurns > 0;
}
