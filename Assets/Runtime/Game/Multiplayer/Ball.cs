using System;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class Ball : NetworkBehaviour
{
    public Action OutOfBounds;
    public NetworkTransform owner;
    private Vector2 _velocity = Vector2.zero;
    private Vector2 _bounds = new Vector2(-5, 5);
    private Vector2 _goalLimits = new Vector2(-11, 11);
    private float _speed = .22f;
    private bool _outOfBounds;

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

    [ClientRpc]
    public void OutOfBoundsClientRpc()
    {
        handleOutOfBounds();
    }

    void Move()
    {
        if (owner != null)
        {
            Vector3 offset = owner.transform.forward * .8f;
            if (owner.transform.position.z > 0)
            {
                offset *= -1;
            }

            transform.position = owner.transform.position + offset;
            return;
        }
        
        if (_velocity == Vector2.zero)
        {
            return;
        }

        transform.position += new Vector3(_velocity.x, 0, _velocity.y);

        if (!_outOfBounds)
        {
            // send goal event
            if (transform.position.z < _goalLimits.x || transform.position.z > _goalLimits.y)
            {
                OutOfBoundsClientRpc();

                if (!NetworkManager.Singleton.IsHost)
                {
                    handleOutOfBounds();
                }
                return;
            }

            // handle hitting walls
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

    private void handleOutOfBounds()
    {
        _outOfBounds = true;
        OutOfBounds?.Invoke();
    }
}