using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class Ball : NetworkBehaviour
{
    public Action<int> OutOfBounds;
    private Vector2 _velocity = Vector2.zero;
    private Vector2 _bounds = new Vector2(-5, 5);
    private Vector2 _goalLimits = new Vector2(-11, 11);
    private float _speed = .22f;
    private bool _outOfBounds;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void FixedUpdate()
    {
        if (IsServer)
        {
            Move();
        }
    }

    public void ResetToStart()
    {
        _velocity = Vector2.zero;
        NetworkTransform networkTransform = GetComponent<NetworkTransform>();
        networkTransform.Teleport(Vector3.zero, networkTransform.transform.rotation, networkTransform.transform.lossyScale);
        _outOfBounds = false;
    }

    public void SetRandomVelocity()
    {
        float angle = UnityEngine.Random.Range(20f, 160f);
        float radians = angle * Mathf.Deg2Rad;

        _velocity = _speed * new Vector2(Mathf.Cos(radians) * getRandomSign(), Mathf.Sin(radians) * getRandomSign());
    }

    private int getRandomSign()
    {
        return UnityEngine.Random.Range(0, 2) * 2 - 1;
    }

    void Move()
    {
        if (_velocity == Vector2.zero)
        {
            return;
        }

        transform.position += new Vector3(_velocity.x, 0, _velocity.y);

        if (!_outOfBounds)
        {
            // send goal event
            if (transform.position.z < _goalLimits.x)
            {
                handleOutOfBounds(-1);
                return;
            }
            else if (transform.position.z > _goalLimits.y)
            {
                handleOutOfBounds(1);
                return;
            }

            if (transform.position.x < _bounds.x || transform.position.x > _bounds.y)
            {
                float result = 0;

                if (transform.position.x < _bounds.x)
                {
                    result = Math.Abs(_velocity.x);
                }
                else
                {
                    result = -Math.Abs(_velocity.x);
                }

                _velocity.x = result;

                transform.position += new Vector3(_velocity.x, 0, 0);
            }
        }
    }

    private void OnCollisionEnter(UnityEngine.Collision collision)
    {
        if (!IsServer)
        {
            return;
        }

        float result = 0;
        Transform playerTransform = collision.collider.gameObject.transform;
        float width = playerTransform.localScale.x * .5f;
        float deltaX = transform.position.x - playerTransform.position.x;
        float offsetX = Mathf.Min(Math.Abs(deltaX) / width, .6f);

        Debug.Log($"Collision Offset : {offsetX}");

        _velocity.x = offsetX * _speed;
        _velocity.y = (1 - offsetX) * _speed;

        if (deltaX < 0)
        {
            _velocity.x = -_velocity.x;
        }

        if (playerTransform.position.z > 0f)
        {
            result = -Math.Abs(_velocity.y);
        }
        else
        {
            result = Math.Abs(_velocity.y);
        }

        _velocity.y = result;
    }

    private void handleOutOfBounds(int direction)
    {
        _outOfBounds = true;
        OutOfBounds?.Invoke(direction);
    }
}