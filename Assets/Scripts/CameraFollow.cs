using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    public Transform target;
    public float smoothSpeed = 5f;
    public Vector2 offset = new Vector2(0f, 1f);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = new Vector3(
            target.position.x + offset.x,
            target.position.y + offset.y,
            transform.position.z
        );

        transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
    }
}
