using System;
using UnityEngine;

public class Ball : MonoBehaviour
{
    private Transform _owner;
    private Vector2 _velocity = Vector2.zero;
    private Vector2 _bounds = new Vector2(-5, 5);
    private Vector2 _goalLimits = new Vector2(-11, 11);
    private float _speed = .22f;
    private bool _outOfBounds;

    public bool OutOfBounds { get => _outOfBounds; }
    public Transform Owner { get => _owner; set => _owner = value; }

    public void ResetToStart()
    {
        _velocity = Vector2.zero;
        _outOfBounds = false;
    }

    public void SetRandomVelocity(int direction = 0)
    {
        float angle = UnityEngine.Random.Range(20f, 160f);
        float radians = angle * Mathf.Deg2Rad;

        if (direction == 0)
        {
            direction = getRandomSign();
        }

        _velocity = _speed * new Vector2(Mathf.Cos(radians) * getRandomSign(), Mathf.Sin(radians) * direction);
    }

    public void Move()
    {
        if (_owner != null)
        {
            Vector3 offset = _owner.transform.forward * .8f;
            if (_owner.transform.position.z > 0)
            {
                offset *= -1;
            }

            transform.position = _owner.transform.position + offset;
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
                _outOfBounds = true;
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

    public void HandleCollision(UnityEngine.Collision collision)
    {
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

    private int getRandomSign()
    {
        return UnityEngine.Random.Range(0, 2) * 2 - 1;
    }
}
