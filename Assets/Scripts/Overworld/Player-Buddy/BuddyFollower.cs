using UnityEngine;

public class BuddyFollower : MonoBehaviour
{
    public PlayerControl player;
    public float followDelay = 0.5f;
    public float stopDistance = 0.2f;
    public float moveSmooth = 10f;

    [SerializeField] private Animator animatorFront;
    [SerializeField] private Animator animatorBack;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (!player.controlsEnabled)
        {
            animatorFront.SetFloat("BlendX", 0f);
            animatorFront.SetFloat("BlendY", 0f);
            animatorBack.SetFloat("BlendX", 0f);
            animatorBack.SetFloat("BlendY", 0f);
            return;
        }

        int delayFrames = Mathf.RoundToInt(followDelay / Time.fixedDeltaTime);

        if (player.history.Count > delayFrames)
        {
            PlayerSnapshot snapshot = player.history[player.history.Count - delayFrames];

            if (Vector3.Distance(rb.position, snapshot.position) > stopDistance)
            {
                Vector3 newPos = Vector3.Lerp(rb.position, snapshot.position, moveSmooth * Time.fixedDeltaTime);
                rb.MovePosition(newPos);
            }

            rb.MoveRotation(snapshot.rotation);

            animatorFront.SetFloat("BlendX", snapshot.blendX);
            animatorFront.SetFloat("BlendY", snapshot.blendY);
            animatorBack.SetFloat("BlendX", snapshot.blendX);
            animatorBack.SetFloat("BlendY", snapshot.blendY);
        }
    }
}