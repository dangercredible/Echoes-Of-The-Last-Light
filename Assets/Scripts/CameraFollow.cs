using UnityEngine;

/// <summary>
/// Smoothly follows the player/camera target with a configurable offset.
/// </summary>
public class CameraFollow : MonoBehaviour
{

    public Transform target;
    public float smoothSpeed = 5f;
    public Vector2 offset = new Vector2(0f, 1f);
    public float maxFollowSpeed = 40f;

    Vector3 velocity;

    void LateUpdate()
    {
        if (target == null) return;

        // Keep Z from current camera transform so only XY follows.
        Vector3 desired = new Vector3(
            target.position.x + offset.x,
            target.position.y + offset.y,
            transform.position.z
        );

        float smoothTime = smoothSpeed <= 0f ? 0f : 1f / smoothSpeed;
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime, maxFollowSpeed, Time.deltaTime);
    }
}
