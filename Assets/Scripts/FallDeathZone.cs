using UnityEngine;

public class FallDeathZone : MonoBehaviour
{
    public Transform respawnPoint;
    Vector3 fallbackSpawn;
    bool capturedFallback;

    void Awake()
    {
        GameObject playerObject = GameObject.Find("Player");
        if (playerObject != null)
        {
            fallbackSpawn = playerObject.transform.position;
            capturedFallback = true;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null)
            return;

        player.Die();

        Transform t = other.transform;
        if (respawnPoint != null)
            t.position = respawnPoint.position;
        else if (capturedFallback)
            t.position = fallbackSpawn;

        Rigidbody2D body = other.attachedRigidbody;
        if (body != null)
            body.linearVelocity = Vector2.zero;
    }
}
