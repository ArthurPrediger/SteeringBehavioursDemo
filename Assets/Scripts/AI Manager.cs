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
            Vector3 targetPos = target.transform.position;

            Vector3 steering = Vector3.zero;
            if (curBehaviour == 0)
                steering = SeekSteering(birdPos, birdsVelocities[i], targetPos);
            else if(curBehaviour == 1)
                steering = FleeSteering(birdPos, birdsVelocities[i], targetPos);
            else if (curBehaviour == 2)
                steering = ArrivalSteering(birdPos, birdsVelocities[i], targetPos);
            else if (curBehaviour == 3)
                steering = DepartureSteering(birdPos, birdsVelocities[i], targetPos);
            else if(curBehaviour == 4)
            {
                if(i == 0)
                    steering = ArrivalSteering(birdPos, birdsVelocities[i], targetPos);
                else
                {
                    GameObject leader = birds[0];
                    steering = LeaderSteering(leader.transform.position, leader.transform.right, birdsVelocities[0], birdPos, birdsVelocities[i]); 
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

    private Vector3 LeaderSteering(Vector3 leaderPos, Vector3 leaderVel, Vector3 leaderRight, Vector3 position, Vector3 velocity)
    {
        Vector3 leaderDir = leaderVel.normalized;
        const float followDistance = 10.0f;
        const float safeDistance = 2.0f;
        const float frontAvoidDistance = 20.0f;
        const float frontAvoidAngle = 120.0f; // degrees;

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

        if (dot > Mathf.Cos(frontAvoidAngle * Mathf.Deg2Rad) &&
            dist < frontAvoidDistance)
        {
            // steer to the side of the leader's direction
            Vector3 sideDir = leaderRight;
            sideDir *= (Random.value > 0.5f ? 1f : -1f);
            steering += sideDir * (maxForce * 100.0f);
        }

        // Combine all forces
        steering = Vector3.ClampMagnitude(steering, maxForce);
        return steering;
    }
}
