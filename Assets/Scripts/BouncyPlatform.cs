using UnityEngine;

[RequireComponent(typeof(Collider2D))]
/// <summary>
/// Makes a platform launch the player upward when landed on from above.
/// </summary>
public class BouncyPlatform : MonoBehaviour
{
    [Tooltip("Upward velocity applied when the player lands on top (Mario-style mushroom pad).")]
    [Min(0f)]
    public float bounceVelocity = 22f;

    [Tooltip("Minimum downward impact speed (relative) required to bounce.")]
    public float minDownwardSpeed = 2.5f;

    void Awake()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = new Color(0.92f, 0.38f, 0.52f, 1f);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Only bounce the player character.
        PlayerController player = collision.collider.GetComponent<PlayerController>();
        if (player == null)
            return;

        Rigidbody2D rb = collision.rigidbody;
        if (rb == null)
            return;

        if (collision.relativeVelocity.y > -minDownwardSpeed)
            return;

        // Require a top-side contact normal so side hits do not trigger a bounce.
        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint2D cp = collision.GetContact(i);
            if (cp.normal.y > 0.45f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, bounceVelocity);
                return;
            }
        }
    }
}
