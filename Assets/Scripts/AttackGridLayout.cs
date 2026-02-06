using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class CenteredAttackGridLayout : MonoBehaviour
{
    public int maxColumns = 4;
    public Vector2 baseSpacing = new Vector2(2f, 2.5f);
    public float minCardScale = 0.5f;

    private BoxCollider2D bounds;

    private void Awake()
    {
        bounds = GetComponent<BoxCollider2D>();
        bounds.enabled = false;
        if (GetComponent<Rigidbody2D>() == null)
        {
            Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    private void Start()
    {
        Arrange();
    }

    private void OnTransformChildrenChanged()
    {
        Arrange();
    }

    public void Arrange()
    {
        int count = transform.childCount;
        if (count == 0) return;

        BoxCollider2D attackCollider = transform.GetChild(0).GetComponent<BoxCollider2D>();
        if (!attackCollider)
        {
            Debug.LogError("Each card root needs a BoxCollider2D.");
            return;
        }

        Vector2 attackSize = attackCollider.size;
        Vector2 boxSize = bounds.size;

        int columns = Mathf.Min(maxColumns, count);
        int rows = Mathf.CeilToInt((float)count / columns);

        Vector2 unitSize = attackSize + baseSpacing;

        float rawWidth = columns * unitSize.x - baseSpacing.x;
        float rawHeight = rows * unitSize.y - baseSpacing.y;

        float scaleX = boxSize.x / rawWidth;
        float scaleY = boxSize.y / rawHeight;

        float finalScale = Mathf.Clamp(Mathf.Min(scaleX, scaleY), minCardScale, 1f);

        Vector2 scaledAttackSize = attackSize * finalScale;
        Vector2 scaledSpacing = baseSpacing * finalScale;

        float totalWidth = columns * scaledAttackSize.x + (columns - 1) * scaledSpacing.x;
        float totalHeight = rows * scaledAttackSize.y + (rows - 1) * scaledSpacing.y;

        Vector2 startOffset = new Vector2(
            -totalWidth / 2f + scaledAttackSize.x / 2f,
            totalHeight / 2f - scaledAttackSize.y / 2f
        );

        for (int i = 0; i < count; i++)
        {
            Transform card = transform.GetChild(i);

            int row = i / columns;
            int col = i % columns;

            card.localScale = Vector3.one * finalScale;

            card.localPosition = new Vector3(
                startOffset.x + col * (scaledAttackSize.x + scaledSpacing.x),
                startOffset.y - row * (scaledAttackSize.y + scaledSpacing.y),
                0f
            );
        }
    }
}
