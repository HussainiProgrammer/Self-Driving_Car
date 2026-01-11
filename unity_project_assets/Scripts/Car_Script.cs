using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

// NOTE: One Unity distane unit is equivalent to one decimeter = 1dm = 0.1m

public class CarScript : Agent
{
    [SerializeField] private float linearSpeed = 10;
    [SerializeField] private float rotationalSpeed = 180;
    [SerializeField] private Transform lidar;
    [SerializeField] private LayerMask layerMask;
    public float maximumDistance = 120;
    public float minimumDistance = 1f;
    public int numberOfRays = 36;
    private Dictionary<float, float> distances = new Dictionary<float, float>();

    [SerializeField] private bool showRedRays;

    [SerializeField] private bool showGreenRays;

    [SerializeField] private Transform target;
    private float previousDistance;
    private Vector3 previousDifferenceVector;
    [SerializeField] private float linearReward = 2f;
    [SerializeField] private float rotationReward = 1f;
    [SerializeField] private float finalReward = 100f;

    public List<GameObject> envs;
    private GameObject currentEnv;

    public bool b1 = false;
    public bool b2 = false;
    public bool b3 = false;
    public bool b4 = false;
    public bool b5 = false;
    public bool b6 = false;

    public override void OnEpisodeBegin()
    {
        if (currentEnv != null) { Destroy(currentEnv); }

        int randomIndex = Random.Range(0, envs.Count);
        currentEnv = Instantiate(envs[randomIndex], Vector3.zero, Quaternion.identity);
        target.position = new Vector3(1000, 1000, 0);

        transform.position = currentEnv.GetComponent<Env_Script>().carPosition;
        transform.eulerAngles = currentEnv.GetComponent<Env_Script>().carRotation;
        target.position = currentEnv.GetComponent<Env_Script>().targetPosition;

        previousDistance = Vector3.Distance(target.position, transform.position);

        Vector3 target_direction = Vector3.Normalize(target.position - transform.position);
        Vector3 heading_direction = GetDirectionFromAngle(transform.eulerAngles.z);

        previousDifferenceVector = target_direction - heading_direction;

        distances = new Dictionary<float, float>();
        base.OnEpisodeBegin();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        CastRays();

        Vector3 delta_r = target.position - transform.position;

        sensor.AddObservation(delta_r.magnitude / maximumDistance);

        Vector3 target_direction = Vector3.Normalize(delta_r);
        Vector3 heading_direction = GetDirectionFromAngle(transform.eulerAngles.z);

        sensor.AddObservation(target_direction.y * heading_direction.x - target_direction.x * heading_direction.y);
        sensor.AddObservation(target_direction.x * heading_direction.x + target_direction.y * heading_direction.y);

        foreach (float distance in distances.Values)
        {
            sensor.AddObservation(distance / maximumDistance);
        }

        if (b1) { Debug.Log("Distance to Target: " + (delta_r.magnitude / maximumDistance)); }
        if (b2) { Debug.Log("Sine" + (target_direction.y * heading_direction.x - target_direction.x * heading_direction.y)); }
        if (b3) { Debug.Log("Cosine" + (target_direction.x * heading_direction.x + target_direction.y * heading_direction.y)); }

        base.CollectObservations(sensor);

    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        ExecuteAction(actions.DiscreteActions[0]);

        float currentDistance = Vector3.Distance(target.position, transform.position);
        AddReward((previousDistance - currentDistance) * linearReward / (linearSpeed * Time.fixedDeltaTime));

        Vector3 target_direction = Vector3.Normalize(target.position - transform.position);
        Vector3 heading_direction = GetDirectionFromAngle(transform.eulerAngles.z);
        Vector3 currentDifferenceVector = target_direction - heading_direction;

        AddReward((previousDifferenceVector.magnitude - currentDifferenceVector.magnitude) * rotationReward);
        if (b5) { Debug.Log("Linear Motion Reward: " + ((previousDistance - currentDistance) * linearReward / (linearSpeed * Time.fixedDeltaTime))); }
        if (b6) { Debug.Log("Rotational Motion Reward: " + ((previousDifferenceVector.magnitude - currentDifferenceVector.magnitude) * rotationReward)); }

        previousDistance = currentDistance;
        previousDifferenceVector = currentDifferenceVector;

        if (currentDistance <= 2.5)
        {
            AddReward(finalReward);
            EndEpisode();
            Debug.Log("Goal Reached!");
        }


        base.OnActionReceived(actions);
    }

    void ExecuteAction(int action)
    {
        if (action == 0) { transform.position += linearSpeed * Time.fixedDeltaTime * transform.right; }
        else if (action == 1) { transform.position -= linearSpeed * Time.fixedDeltaTime * transform.right; }
        else if (action == 2) { transform.Rotate(0, 0, rotationalSpeed * Time.fixedDeltaTime); }
        else if (action == 3) { transform.Rotate(0, 0, -rotationalSpeed * Time.fixedDeltaTime); }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Obstacle"))
        {
            AddReward(-finalReward);
            EndEpisode();
            Debug.Log("Wall Hit!");
        }
    }

    void CastRays()
    {
        for (float i = 0; i < 360; i += 360 / numberOfRays)
        {
            CastRay(i, transform.eulerAngles.z);
        }
    }

    void CastRay(float angle, float car_angle)
    {
        Vector3 direction = GetDirectionFromAngle(angle + car_angle);
        RaycastHit2D hit = Physics2D.Raycast(lidar.position, direction, maximumDistance, layerMask);

        if (hit.collider != null && hit.distance > minimumDistance)
        {
            distances[angle] = hit.distance;
            if (showRedRays) { Debug.DrawRay(lidar.position, hit.distance * direction, Color.red); }
        }
        else
        {
            float distance;
            if (distances.ContainsKey(angle)) { distance = distances[angle]; } else { distance = maximumDistance; distances[angle] = distance; }
            if (showGreenRays) { Debug.DrawRay(lidar.position, distance * direction, Color.green); }

        }

        if (b4) { Debug.Log("Distance at " + angle + " degrees: " + distances[angle]); }
    }

    Vector3 GetDirectionFromAngle(float angle)
    {
        return new Vector3(Mathf.Cos(Mathf.Deg2Rad * angle), Mathf.Sin(Mathf.Deg2Rad * angle), 0);
    }
}
