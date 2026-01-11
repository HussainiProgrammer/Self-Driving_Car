using UnityEngine;

public class Camera_Script : MonoBehaviour
{
    [SerializeField] private Vector3 offset;
    [SerializeField] private Transform car;
    [SerializeField] private float smoothSpeed = 0.125f;

    void FixedUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, car.position + offset, smoothSpeed);
    }
}