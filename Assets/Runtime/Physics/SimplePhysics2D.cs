using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplePhysics2D : MonoBehaviour
{
	public float baseMinVelocity = 0f;
	public float baseMaxVelocity = 100f;
    public float maxVelocity { get; set; }
    public float minVelocity { get; set; }
    public float friction { get; set; }
    public Vector2 velocity { get; set; }
    public Vector2 acceleration { get; set; }

    void Awake()
    {
        init();
    }

    public void init()
    {
        friction = friction;
        velocity = Vector2.zero;
        acceleration = Vector2.zero;
        minVelocity = baseMinVelocity;
        maxVelocity = baseMaxVelocity;
    }

    public void reset()
    {
        init();
    }

    public void update(float deltaTime)
    {
        Vector2 prevVelocity = velocity;

        if (velocity.magnitude > 0 && !Mathf.Approximately(friction, 0f))
        {
            velocity -= velocity * friction * deltaTime;
        }

        if (acceleration.magnitude > 0f)
        {
            velocity += acceleration * deltaTime;
        }

        if (velocity.magnitude > maxVelocity)
        {
            velocity = velocity.normalized * maxVelocity;
        }
        else if (velocity.magnitude < minVelocity)
        {
            velocity = Vector2.zero;
        }

        Vector2 averageVelocity = (prevVelocity + velocity) * .5f;

        transform.position = (Vector2)transform.position + averageVelocity * deltaTime;
    }
}