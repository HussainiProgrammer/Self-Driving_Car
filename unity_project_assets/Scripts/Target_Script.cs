using UnityEngine;

public class Target_Script : MonoBehaviour
{
    [SerializeField] private Transform Car;
    void FixedUpdate()
    {
        Debug.DrawRay(transform.position, Car.position - transform.position, Color.green);
    }
}
