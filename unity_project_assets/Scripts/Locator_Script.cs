using UnityEngine;

public class Locator_Script : MonoBehaviour
{
    [SerializeField] private Transform A;
    [SerializeField] private Transform B;
    [SerializeField] private Transform C;
    [SerializeField] private Transform Car;

    void Start()
    {
        B.position = new Vector3(A.position.x + 1, A.position.y, 1);
        C.position = new Vector3(A.position.x, A.position.y - 1, 1);
    }

    void FixedUpdate()
    {
        float a = Vector3.Distance(A.position, Car.position);
        float b = Vector3.Distance(B.position, Car.position);
        float c = Vector3.Distance(C.position, Car.position);

        transform.position = new Vector3(A.position.x + (a*a - b*b + 1)/2, A.position.y + (-a*a + c*c - 1)/2, 1);
    }

}
