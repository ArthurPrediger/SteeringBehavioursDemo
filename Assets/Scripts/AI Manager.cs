using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class AIManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> birds;
    private List<Vector3> birdsVelocities = new();
    [SerializeField] private float maxSpeed = 1.0f;
    [SerializeField] private float maxForce = 0.5f;
    [SerializeField] private float slowRadius = 1.0f;
    [SerializeField] private GameObject target;
    private int curBehaviour = 0;

    // Start is called before the first frame update
    void Start()
    {
        for(uint i = 0; i < birds.Count; ++i)
        {
            birdsVelocities.Add(Vector3.zero);
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < birds.Count; ++i)
        {
            Vector3 birdPos = birds[i].transform.position;
            Vector3 targetPos = target.transform.position + (Vector3.up * target.GetComponent<Renderer>().bounds.extents.y * 2f);

            Vector3 steering = Vector3.zero;
            if (curBehaviour == 0)
                steering = SeekSteering(birdPos, birdsVelocities[i], targetPos);
            else if (curBehaviour == 1)
                steering = FleeSteering(birdPos, birdsVelocities[i], targetPos);
            else if (curBehaviour == 2)
                steering = ArrivalSteering(birdPos, birdsVelocities[i], targetPos);
            else if (curBehaviour == 3)
                steering = DepartureSteering(birdPos, birdsVelocities[i], targetPos);
            else if (curBehaviour == 4)
            {
                if (i == 0)
                    steering = ArrivalSteering(birdPos, birdsVelocities[i], targetPos);
                else
                {
                    GameObject leader = birds[0];
                    steering = LeaderSteering(leader.transform, birdsVelocities[0], birdPos, birdsVelocities[i]);
                }
            }
            else if (curBehaviour == 5)
            {
                steering = SeparationSteering(birds[i], birds);

                if (steering.sqrMagnitude < 0.0001f)
                    birdsVelocities[i] = Vector3.zero;
            }
            else if (curBehaviour == 6)
            {
                steering = CohesionSteering(birds[i], birdsVelocities[i], birds);
            }
            else if (curBehaviour == 7)
            {
                if (i == 0)
                    steering = ArrivalSteering(birdPos, birdsVelocities[i], targetPos);
                else
                {
                    GameObject leader = birds[0];
                    steering = LeaderSteering(leader.transform, birdsVelocities[0], birdPos, birdsVelocities[i]) +
                        SeparationSteering(birds[i], birds) * 4f +
                        CohesionSteering(birds[i], birdsVelocities[i], birds) + 
                        AlignmentSteering(birds[i], birdsVelocities[i], birds, birdsVelocities);
                }
            }

            birdsVelocities[i] += steering * Time.deltaTime;
            birdsVelocities[i] = Vector3.ClampMagnitude(birdsVelocities[i], maxSpeed);
            birds[i].transform.position += birdsVelocities[i] * Time.deltaTime;
            birds[i].transform.forward = birdsVelocities[i].normalized;
        }
    }

    public void SetSteeringBehaviour(int behaviour)
    {
        curBehaviour = behaviour;
    }

    private Vector3 SeekSteering(Vector3 position, Vector3 velocity,  Vector3 targetPos)
    {
        Vector3 desired = (targetPos - position).normalized * maxSpeed;
        Vector3 steering = desired - velocity;

        steering = Vector3.ClampMagnitude(steering, maxForce);

        return steering;
    }

    private Vector3 FleeSteering(Vector3 position, Vector3 velocity, Vector3 targetPos)
    {
        Vector3 desired = (position - targetPos).normalized * maxSpeed;
        Vector3 steering = desired - velocity;
        
        steering = Vector3.ClampMagnitude(steering, maxForce);

        return steering;
    }

    private Vector3 ArrivalSteering(Vector3 position, Vector3 velocity, Vector3 targetPos)
    {
        Vector3 desired = targetPos - position;
        float distance = desired.magnitude;
        desired.Normalize();

        if (distance < slowRadius)
            desired *= maxSpeed * (distance / slowRadius);
        else
            desired *= maxSpeed;

        Vector3 steer = desired - velocity;
        return Vector3.ClampMagnitude(steer, maxForce);
    }

    private Vector3 DepartureSteering(Vector3 position, Vector3 velocity, Vector3 targetPos)
    {
        Vector3 desired = position - targetPos;
        float distance = desired.magnitude;
        desired.Normalize();

        if (distance < slowRadius)
            desired *= maxSpeed * (Mathf.Clamp01(distance / slowRadius) + 0.02f);
        else
            desired *= maxSpeed;

        Vector3 steer = desired - velocity;
        return Vector3.ClampMagnitude(steer, maxForce);
    }

    private Vector3 LeaderSteering(Transform leaderTransform, Vector3 leaderVel, Vector3 position, Vector3 velocity)
    {
        Vector3 leaderPos = leaderTransform.position;
        Vector3 leaderDir = leaderTransform.forward;
        Vector3 leaderRight = leaderTransform.right;
        const float followDistance = 8.0f;
        const float safeDistance = 2.0f;
        const float frontAvoidPathRadius = 10.0f;
        const float frontAvoidPathAngle = 60.0f; // degrees
        float avoidLeaderPathForce = maxForce * 10.0f;

        Vector3 steering = Vector3.zero;

        // Seek a point behind the leader
        Vector3 followTargetPos = leaderPos - leaderDir * followDistance;

        steering = ArrivalSteering(position, velocity, followTargetPos);

        float dist = Vector3.Distance(position, followTargetPos);
        if (dist < safeDistance)
            steering += DepartureSteering(position, velocity, followTargetPos);

        // Avoid the area in front of the leader (front cone)
        Vector3 toFollower = (position - leaderPos).normalized;
        float dot = Vector3.Dot(leaderDir, toFollower);

        if(dot > Mathf.Cos(frontAvoidPathAngle * Mathf.Deg2Rad) && 
            DistancePointToRay(position, leaderPos, leaderDir) < frontAvoidPathRadius)
        {
            // steer to the side of the leader's direction
            Vector3 sideDir = leaderRight;
            if(Vector3.Dot(-leaderDir, toFollower) > Vector3.Dot(leaderRight, toFollower))
                sideDir = -leaderRight;
            steering += sideDir * avoidLeaderPathForce;
        }

        // Combine all forces
        steering = Vector3.ClampMagnitude(steering, maxForce);
        return steering;
    }

    private float DistancePointToRay(Vector3 point, Vector3 origin, Vector3 dir)
    {
        Vector3 toPoint = point - origin;
        float t = Vector3.Dot(toPoint, dir);

        // If t < 0, the closest point is behind the ray origin
        if (t < 0f)
            return Vector3.Distance(point, origin);

        Vector3 closest = origin + dir * t;
        return Vector3.Distance(point, closest);
    }

    private Vector3 SeparationSteering(GameObject self, List<GameObject> flock)
    {
        const float separationRadius = 5.0f;
        const float repelForce = 6.0f;
        Vector3 steering = Vector3.zero;
        int count = 0;

        foreach (GameObject bird in flock)
        {
            if (bird == self) continue;

            float distance = Vector3.Distance(self.transform.position, bird.transform.position);
            if (distance > 0 && distance < separationRadius)
            {
                // Repel stronger when closer
                Vector3 diff = (self.transform.position - bird.transform.position).normalized;
                diff /= distance; // weight by proximity
                steering += diff;
                count++;
            }
        }

        if (count > 0)
            steering /= count; // average all forces

        steering = Vector3.ClampMagnitude(steering * repelForce, maxForce);

        return steering;
    }

    private Vector3 AlignmentSteering(GameObject self, Vector3 velocity, List<GameObject> flock, List<Vector3> flockVelocities)
    {
        const float alignmentRadius = 10.0f;
        Vector3 sum = Vector3.zero;
        int count = 0;

        int i = 0;
        foreach (GameObject bird in flock)
        {
            if (bird == self) continue;

            float distance = Vector3.Distance(self.transform.position, bird.transform.position);
            if (distance < alignmentRadius)
            {
                sum += birdsVelocities[i++];  // use neighbor’s current velocity
                count++;
            }
        }

        if (count > 0)
        {
            Vector3 average = sum / count;
            Vector3 desired = average.normalized * maxSpeed;
            Vector3 steer = desired - velocity;
            return Vector3.ClampMagnitude(steer, maxForce);
        }

        return Vector3.zero;
    }

    private Vector3 CohesionSteering(GameObject self, Vector3 velocity, List<GameObject> flock)
    {
        const float cohesionRadius = 20.0f;
        Vector3 center = Vector3.zero;
        int count = 0;

        foreach (GameObject bird in flock)
        {
            if (bird == self) continue;

            float distance = Vector3.Distance(self.transform.position, bird.transform.position);
            if (distance < cohesionRadius)
            {
                center += bird.transform.position;
                count++;
            }
        }

        if (count > 0)
        {
            center /= count; // average neighbor positions
            return ArrivalSteering(self.transform.position, velocity, center); // use your existing Seek() or Arrival()
        }

        return Vector3.zero;
    }
}
